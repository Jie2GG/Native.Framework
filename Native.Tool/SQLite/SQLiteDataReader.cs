/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 *
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Data;
  using System.Data.Common;
  using System.Globalization;

  /// <summary>
  /// SQLite implementation of DbDataReader.
  /// </summary>
  public sealed class SQLiteDataReader : DbDataReader
  {
    /// <summary>
    /// Underlying command this reader is attached to
    /// </summary>
    private SQLiteCommand _command;
    /// <summary>
    /// The flags pertaining to the associated connection (via the command).
    /// </summary>
    private SQLiteConnectionFlags _flags;
    /// <summary>
    /// Index of the current statement in the command being processed
    /// </summary>
    private int _activeStatementIndex;
    /// <summary>
    /// Current statement being Read()
    /// </summary>
    private SQLiteStatement _activeStatement;
    /// <summary>
    /// State of the current statement being processed.
    /// -1 = First Step() executed, so the first Read() will be ignored
    ///  0 = Actively reading
    ///  1 = Finished reading
    ///  2 = Non-row-returning statement, no records
    /// </summary>
    private int _readingState;
    /// <summary>
    /// Number of records affected by the insert/update statements executed on the command
    /// </summary>
    private int _rowsAffected;
    /// <summary>
    /// Count of fields (columns) in the row-returning statement currently being processed
    /// </summary>
    private int _fieldCount;
    /// <summary>
    /// The number of calls to Step() that have returned true (i.e. the number of rows that
    /// have been read in the current result set).
    /// </summary>
    private int _stepCount;
    /// <summary>
    /// Maps the field (column) names to their corresponding indexes within the results.
    /// </summary>
    private Dictionary<string, int> _fieldIndexes;
    /// <summary>
    /// Datatypes of active fields (columns) in the current statement, used for type-restricting data
    /// </summary>
    private SQLiteType[] _fieldTypeArray;

    /// <summary>
    /// The behavior of the datareader
    /// </summary>
    private CommandBehavior _commandBehavior;

    /// <summary>
    /// If set, then dispose of the command object when the reader is finished
    /// </summary>
    internal bool _disposeCommand;

    /// <summary>
    /// If set, then raise an exception when the object is accessed after being disposed.
    /// </summary>
    internal bool _throwOnDisposed;

    /// <summary>
    /// An array of rowid's for the active statement if CommandBehavior.KeyInfo is specified
    /// </summary>
    private SQLiteKeyReader _keyInfo;

    /// <summary>
    /// Matches the version of the connection.
    /// </summary>
    internal int _version;

    /// <summary>
    /// The "stub" (i.e. placeholder) base schema name to use when returning
    /// column schema information.  Matches the base schema name used by the
    /// associated connection.
    /// </summary>
    private string _baseSchemaName;

    /// <summary>
    /// Internal constructor, initializes the datareader and sets up to begin executing statements
    /// </summary>
    /// <param name="cmd">The SQLiteCommand this data reader is for</param>
    /// <param name="behave">The expected behavior of the data reader</param>
    internal SQLiteDataReader(SQLiteCommand cmd, CommandBehavior behave)
    {
      _throwOnDisposed = true;
      _command = cmd;
      _version = _command.Connection._version;
      _baseSchemaName = _command.Connection._baseSchemaName;

      _commandBehavior = behave;
      _activeStatementIndex = -1;
      _rowsAffected = -1;

      RefreshFlags();

      SQLiteConnection.OnChanged(GetConnection(this),
          new ConnectionEventArgs(SQLiteConnectionEventType.NewDataReader,
          null, null, _command, this, null, null, new object[] { behave }));

      if (_command != null)
          NextResult();
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    #region IDisposable "Pattern" Members
    private bool disposed;
    private void CheckDisposed() /* throw */
    {
#if THROW_ON_DISPOSED
        if (disposed && _throwOnDisposed)
            throw new ObjectDisposedException(typeof(SQLiteDataReader).Name);
#endif
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Dispose of all resources used by this datareader.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        SQLiteConnection.OnChanged(GetConnection(this),
            new ConnectionEventArgs(SQLiteConnectionEventType.DisposingDataReader,
            null, null, _command, this, null, null, new object[] { disposing,
            disposed, _commandBehavior, _readingState, _rowsAffected, _stepCount,
            _fieldCount, _disposeCommand, _throwOnDisposed }));

        try
        {
            if (!disposed)
            {
                //if (disposing)
                //{
                //    ////////////////////////////////////
                //    // dispose managed resources here...
                //    ////////////////////////////////////
                //}

                //////////////////////////////////////
                // release unmanaged resources here...
                //////////////////////////////////////

                //
                // NOTE: Fix for ticket [e1b2e0f769], do NOT throw exceptions
                //       while we are being disposed.
                //
                _throwOnDisposed = false;
            }
        }
        finally
        {
            base.Dispose(disposing);

            //
            // NOTE: Everything should be fully disposed at this point.
            //
            disposed = true;
        }
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////////////////////////

    internal void Cancel()
    {
      _version = 0;
    }

    /// <summary>
    /// Closes the datareader, potentially closing the connection as well if CommandBehavior.CloseConnection was specified.
    /// </summary>
    public override void Close()
    {
      CheckDisposed();

      SQLiteConnection.OnChanged(GetConnection(this),
          new ConnectionEventArgs(SQLiteConnectionEventType.ClosingDataReader,
          null, null, _command, this, null, null, new object[] { _commandBehavior,
          _readingState, _rowsAffected, _stepCount, _fieldCount, _disposeCommand,
          _throwOnDisposed }));

      try
      {
        if (_command != null)
        {
          try
          {
            try
            {
              // Make sure we've not been canceled
              if (_version != 0)
              {
                try
                {
                  while (NextResult())
                  {
                  }
                }
                catch(SQLiteException)
                {
                }
              }
              _command.ResetDataReader();
            }
            finally
            {
              // If the datareader's behavior includes closing the connection, then do so here.
              if ((_commandBehavior & CommandBehavior.CloseConnection) != 0 && _command.Connection != null)
                _command.Connection.Close();
            }
          }
          finally
          {
            if (_disposeCommand)
              _command.Dispose();
          }
        }

        _command = null;
        _activeStatement = null;
        _fieldIndexes = null;
        _fieldTypeArray = null;
      }
      finally
      {
        if (_keyInfo != null)
        {
          _keyInfo.Dispose();
          _keyInfo = null;
        }
      }
    }

    /// <summary>
    /// Throw an error if the datareader is closed
    /// </summary>
    private void CheckClosed()
    {
      if (!_throwOnDisposed)
        return;

      if (_command == null)
        throw new InvalidOperationException("DataReader has been closed");

      if (_version == 0)
        throw new SQLiteException("Execution was aborted by the user");

      SQLiteConnection connection = _command.Connection;

      if (connection._version != _version || connection.State != ConnectionState.Open)
        throw new InvalidOperationException("Connection was closed, statement was terminated");
    }

    /// <summary>
    /// Throw an error if a row is not loaded
    /// </summary>
    private void CheckValidRow()
    {
      if (_readingState != 0)
        throw new InvalidOperationException("No current row");
    }

    /// <summary>
    /// Enumerator support
    /// </summary>
    /// <returns>Returns a DbEnumerator object.</returns>
    public override Collections.IEnumerator GetEnumerator()
    {
      CheckDisposed();
      return new DbEnumerator(this, ((_commandBehavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection));
    }

    /// <summary>
    /// Not implemented.  Returns 0
    /// </summary>
    public override int Depth
    {
      get
      {
        CheckDisposed();
        CheckClosed();
        return 0;
      }
    }

    /// <summary>
    /// Returns the number of columns in the current resultset
    /// </summary>
    public override int FieldCount
    {
      get
      {
        CheckDisposed();
        CheckClosed();

        if (_keyInfo == null)
          return _fieldCount;

        return _fieldCount + _keyInfo.Count;
      }
    }

    /// <summary>
    /// Forces the connection flags cached by this data reader to be refreshed
    /// from the underlying connection.
    /// </summary>
    public void RefreshFlags()
    {
        CheckDisposed();

        _flags = SQLiteCommand.GetFlags(_command);
    }

    /// <summary>
    /// Returns the number of rows seen so far in the current result set.
    /// </summary>
    public int StepCount
    {
        get
        {
            CheckDisposed();
            CheckClosed();

            return _stepCount;
        }
    }

    private int PrivateVisibleFieldCount
    {
        get { return _fieldCount; }
    }

    /// <summary>
    /// Returns the number of visible fields in the current resultset
    /// </summary>
    public override int VisibleFieldCount
    {
      get
      {
        CheckDisposed();
        CheckClosed();
        return PrivateVisibleFieldCount;
      }
    }

    /// <summary>
    /// This method is used to make sure the result set is open and a row is currently available.
    /// </summary>
    private void VerifyForGet()
    {
        CheckClosed();
        CheckValidRow();
    }

    /// <summary>
    /// SQLite is inherently un-typed.  All datatypes in SQLite are natively strings.  The definition of the columns of a table
    /// and the affinity of returned types are all we have to go on to type-restrict data in the reader.
    ///
    /// This function attempts to verify that the type of data being requested of a column matches the datatype of the column.  In
    /// the case of columns that are not backed into a table definition, we attempt to match up the affinity of a column (int, double, string or blob)
    /// to a set of known types that closely match that affinity.  It's not an exact science, but its the best we can do.
    /// </summary>
    /// <returns>
    /// This function throws an InvalidTypeCast() exception if the requested type doesn't match the column's definition or affinity.
    /// </returns>
    /// <param name="i">The index of the column to type-check</param>
    /// <param name="typ">The type we want to get out of the column</param>
    private TypeAffinity VerifyType(int i, DbType typ)
    {
        if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.NoVerifyTypeAffinity))
            return TypeAffinity.None;

        TypeAffinity affinity = GetSQLiteType(_flags, i).Affinity;

        switch (affinity)
        {
            case TypeAffinity.Int64:
                if (typ == DbType.Int64) return affinity;
                if (typ == DbType.Int32) return affinity;
                if (typ == DbType.Int16) return affinity;
                if (typ == DbType.Byte) return affinity;
                if (typ == DbType.SByte) return affinity;
                if (typ == DbType.Boolean) return affinity;
                if (typ == DbType.DateTime) return affinity;
                if (typ == DbType.Double) return affinity;
                if (typ == DbType.Single) return affinity;
                if (typ == DbType.Decimal) return affinity;
                break;
            case TypeAffinity.Double:
                if (typ == DbType.Double) return affinity;
                if (typ == DbType.Single) return affinity;
                if (typ == DbType.Decimal) return affinity;
                if (typ == DbType.DateTime) return affinity;
                break;
            case TypeAffinity.Text:
                if (typ == DbType.String) return affinity;
                if (typ == DbType.Guid) return affinity;
                if (typ == DbType.DateTime) return affinity;
                if (typ == DbType.Decimal) return affinity;
                break;
            case TypeAffinity.Blob:
                if (typ == DbType.Guid) return affinity;
                if (typ == DbType.Binary) return affinity;
                if (typ == DbType.String) return affinity;
                break;
        }

        throw new InvalidCastException();
    }

    /// <summary>
    /// Invokes the data reader value callback configured for the database
    /// type name associated with the specified column.  If no data reader
    /// value callback is available for the database type name, do nothing.
    /// </summary>
    /// <param name="index">
    /// The index of the column being read.
    /// </param>
    /// <param name="eventArgs">
    /// The extra event data to pass into the callback.
    /// </param>
    /// <param name="complete">
    /// Non-zero if the default handling for the data reader call should be
    /// skipped.  If this is set to non-zero and the necessary return value
    /// is unavailable or unsuitable, an exception will be thrown.
    /// </param>
    private void InvokeReadValueCallback(
        int index,
        SQLiteReadEventArgs eventArgs,
        out bool complete
        )
    {
        complete = false;
        SQLiteConnectionFlags oldFlags = _flags;
        _flags &= ~SQLiteConnectionFlags.UseConnectionReadValueCallbacks;

        try
        {
            string typeName = GetDataTypeName(index);

            if (typeName == null)
                return;

            SQLiteConnection connection = GetConnection(this);

            if (connection == null)
                return;

            SQLiteTypeCallbacks callbacks;

            if (!connection.TryGetTypeCallbacks(typeName, out callbacks) ||
                (callbacks == null))
            {
                return;
            }

            SQLiteReadValueCallback callback = callbacks.ReadValueCallback;

            if (callback == null)
                return;

            object userData = callbacks.ReadValueUserData;

            callback(
                _activeStatement._sql, this, oldFlags, eventArgs, typeName,
                index, userData, out complete); /* throw */
        }
        finally
        {
            _flags |= SQLiteConnectionFlags.UseConnectionReadValueCallbacks;
        }
    }

    /// <summary>
    /// Attempts to query the integer identifier for the current row.  This
    /// will not work for tables that were created WITHOUT ROWID -OR- if the
    /// query does not include the "rowid" column or one of its aliases -OR-
    /// if the <see cref="SQLiteDataReader" /> was not created with the
    /// <see cref="CommandBehavior.KeyInfo"/> flag.
    /// </summary>
    /// <param name="i">
    /// The index of the BLOB column.
    /// </param>
    /// <returns>
    /// The integer identifier for the current row -OR- null if it could not
    /// be determined.
    /// </returns>
    internal long? GetRowId(
        int i
        )
    {
        // CheckDisposed();
        VerifyForGet();

        if (_keyInfo == null)
            return null;

        string databaseName = GetDatabaseName(i);
        string tableName = GetTableName(i);
        int iRowId = _keyInfo.GetRowIdIndex(databaseName, tableName);

        if (iRowId != -1)
            return GetInt64(iRowId);

        return _keyInfo.GetRowId(databaseName, tableName);
    }

    /// <summary>
    /// Retrieves the column as a <see cref="SQLiteBlob" /> object.
    /// This will not work for tables that were created WITHOUT ROWID
    /// -OR- if the query does not include the "rowid" column or one
    /// of its aliases -OR- if the <see cref="SQLiteDataReader" /> was
    /// not created with the <see cref="CommandBehavior.KeyInfo" />
    /// flag.
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <param name="readOnly">
    /// Non-zero to open the blob object for read-only access.
    /// </param>
    /// <returns>A new <see cref="SQLiteBlob" /> object.</returns>
    public SQLiteBlob GetBlob(int i, bool readOnly)
    {
        CheckDisposed();
        VerifyForGet();

        if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UseConnectionReadValueCallbacks))
        {
            SQLiteDataReaderValue value = new SQLiteDataReaderValue();
            bool complete;

            InvokeReadValueCallback(i, new SQLiteReadValueEventArgs(
                "GetBlob", new SQLiteReadBlobEventArgs(readOnly), value),
                out complete);

            if (complete)
                return (SQLiteBlob)value.BlobValue;
        }

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetBlob(i - PrivateVisibleFieldCount, readOnly);

        return SQLiteBlob.Create(this, i, readOnly);
    }

    /// <summary>
    /// Retrieves the column as a boolean value
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>bool</returns>
    public override bool GetBoolean(int i)
    {
        CheckDisposed();
        VerifyForGet();

        if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UseConnectionReadValueCallbacks))
        {
            SQLiteDataReaderValue value = new SQLiteDataReaderValue();
            bool complete;

            InvokeReadValueCallback(i, new SQLiteReadValueEventArgs(
                "GetBoolean", null, value), out complete);

            if (complete)
            {
                if (value.BooleanValue == null)
                    throw new SQLiteException("missing boolean return value");

                return (bool)value.BooleanValue;
            }
        }

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetBoolean(i - PrivateVisibleFieldCount);

        VerifyType(i, DbType.Boolean);
        return Convert.ToBoolean(GetValue(i), CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Retrieves the column as a single byte value
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>byte</returns>
    public override byte GetByte(int i)
    {
        CheckDisposed();
        VerifyForGet();

        if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UseConnectionReadValueCallbacks))
        {
            SQLiteDataReaderValue value = new SQLiteDataReaderValue();
            bool complete;

            InvokeReadValueCallback(i, new SQLiteReadValueEventArgs(
                "GetByte", null, value), out complete);

            if (complete)
            {
                if (value.ByteValue == null)
                    throw new SQLiteException("missing byte return value");

                return (byte)value.ByteValue;
            }
        }

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetByte(i - PrivateVisibleFieldCount);

        VerifyType(i, DbType.Byte);
        return _activeStatement._sql.GetByte(_activeStatement, i);
    }

    /// <summary>
    /// Retrieves a column as an array of bytes (blob)
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <param name="fieldOffset">The zero-based index of where to begin reading the data</param>
    /// <param name="buffer">The buffer to write the bytes into</param>
    /// <param name="bufferoffset">The zero-based index of where to begin writing into the array</param>
    /// <param name="length">The number of bytes to retrieve</param>
    /// <returns>The actual number of bytes written into the array</returns>
    /// <remarks>
    /// To determine the number of bytes in the column, pass a null value for the buffer.  The total length will be returned.
    /// </remarks>
    public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
    {
        CheckDisposed();
        VerifyForGet();

        if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UseConnectionReadValueCallbacks))
        {
            SQLiteReadArrayEventArgs eventArgs = new SQLiteReadArrayEventArgs(
                fieldOffset, buffer, bufferoffset, length);

            SQLiteDataReaderValue value = new SQLiteDataReaderValue();
            bool complete;

            InvokeReadValueCallback(i, new SQLiteReadValueEventArgs(
                "GetBytes", eventArgs, value), out complete);

            if (complete)
            {
                byte[] bytes = value.BytesValue;

                if (bytes != null)
                {
#if !PLATFORM_COMPACTFRAMEWORK
                    Array.Copy(bytes, /* throw */
                        eventArgs.DataOffset, eventArgs.ByteBuffer,
                        eventArgs.BufferOffset, eventArgs.Length);
#else
                    Array.Copy(bytes, /* throw */
                        (int)eventArgs.DataOffset, eventArgs.ByteBuffer,
                        eventArgs.BufferOffset, eventArgs.Length);
#endif

                    return eventArgs.Length;
                }
                else
                {
                    return -1;
                }
            }
        }

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetBytes(i - PrivateVisibleFieldCount, fieldOffset, buffer, bufferoffset, length);

        VerifyType(i, DbType.Binary);
        return _activeStatement._sql.GetBytes(_activeStatement, i, (int)fieldOffset, buffer, bufferoffset, length);
    }

    /// <summary>
    /// Returns the column as a single character
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>char</returns>
    public override char GetChar(int i)
    {
        CheckDisposed();
        VerifyForGet();

        if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UseConnectionReadValueCallbacks))
        {
            SQLiteDataReaderValue value = new SQLiteDataReaderValue();
            bool complete;

            InvokeReadValueCallback(i, new SQLiteReadValueEventArgs(
                "GetChar", null, value), out complete);

            if (complete)
            {
                if (value.CharValue == null)
                    throw new SQLiteException("missing character return value");

                return (char)value.CharValue;
            }
        }

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetChar(i - PrivateVisibleFieldCount);

        VerifyType(i, DbType.SByte);
        return _activeStatement._sql.GetChar(_activeStatement, i);
    }

    /// <summary>
    /// Retrieves a column as an array of chars (blob)
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <param name="fieldoffset">The zero-based index of where to begin reading the data</param>
    /// <param name="buffer">The buffer to write the characters into</param>
    /// <param name="bufferoffset">The zero-based index of where to begin writing into the array</param>
    /// <param name="length">The number of bytes to retrieve</param>
    /// <returns>The actual number of characters written into the array</returns>
    /// <remarks>
    /// To determine the number of characters in the column, pass a null value for the buffer.  The total length will be returned.
    /// </remarks>
    public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
    {
        CheckDisposed();
        VerifyForGet();

        if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UseConnectionReadValueCallbacks))
        {
            SQLiteReadArrayEventArgs eventArgs = new SQLiteReadArrayEventArgs(
                fieldoffset, buffer, bufferoffset, length);

            SQLiteDataReaderValue value = new SQLiteDataReaderValue();
            bool complete;

            InvokeReadValueCallback(i, new SQLiteReadValueEventArgs(
                "GetChars", eventArgs, value), out complete);

            if (complete)
            {
                char[] chars = value.CharsValue;

                if (chars != null)
                {
#if !PLATFORM_COMPACTFRAMEWORK
                    Array.Copy(chars, /* throw */
                        eventArgs.DataOffset, eventArgs.CharBuffer,
                        eventArgs.BufferOffset, eventArgs.Length);
#else
                    Array.Copy(chars, /* throw */
                        (int)eventArgs.DataOffset, eventArgs.CharBuffer,
                        eventArgs.BufferOffset, eventArgs.Length);
#endif

                    return eventArgs.Length;
                }
                else
                {
                    return -1;
                }
            }
        }

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetChars(i - PrivateVisibleFieldCount, fieldoffset, buffer, bufferoffset, length);

        if (!HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.NoVerifyTextAffinity))
            VerifyType(i, DbType.String);

        return _activeStatement._sql.GetChars(_activeStatement, i, (int)fieldoffset, buffer, bufferoffset, length);
    }

    /// <summary>
    /// Retrieves the name of the back-end datatype of the column
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>string</returns>
    public override string GetDataTypeName(int i)
    {
        CheckDisposed();

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetDataTypeName(i - PrivateVisibleFieldCount);

        TypeAffinity affin = TypeAffinity.Uninitialized;
        return _activeStatement._sql.ColumnType(_activeStatement, i, ref affin);
    }

    /// <summary>
    /// Retrieve the column as a date/time value
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>DateTime</returns>
    public override DateTime GetDateTime(int i)
    {
        CheckDisposed();
        VerifyForGet();

        if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UseConnectionReadValueCallbacks))
        {
            SQLiteDataReaderValue value = new SQLiteDataReaderValue();
            bool complete;

            InvokeReadValueCallback(i, new SQLiteReadValueEventArgs(
                "GetDateTime", null, value), out complete);

            if (complete)
            {
                if (value.DateTimeValue == null)
                    throw new SQLiteException("missing date/time return value");

                return (DateTime)value.DateTimeValue;
            }
        }

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetDateTime(i - PrivateVisibleFieldCount);

        VerifyType(i, DbType.DateTime);
        return _activeStatement._sql.GetDateTime(_activeStatement, i);
    }

    /// <summary>
    /// Retrieve the column as a decimal value
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>decimal</returns>
    public override decimal GetDecimal(int i)
    {
        CheckDisposed();
        VerifyForGet();

        if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UseConnectionReadValueCallbacks))
        {
            SQLiteDataReaderValue value = new SQLiteDataReaderValue();
            bool complete;

            InvokeReadValueCallback(i, new SQLiteReadValueEventArgs(
                "GetDecimal", null, value), out complete);

            if (complete)
            {
                if (value.DecimalValue == null)
                    throw new SQLiteException("missing decimal return value");

                return (decimal)value.DecimalValue;
            }
        }

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetDecimal(i - PrivateVisibleFieldCount);

        VerifyType(i, DbType.Decimal);

        CultureInfo cultureInfo = CultureInfo.CurrentCulture;

        if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.GetInvariantDecimal))
            cultureInfo = CultureInfo.InvariantCulture;

        return Decimal.Parse(_activeStatement._sql.GetText(_activeStatement, i), NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign, cultureInfo);
    }

    /// <summary>
    /// Returns the column as a double
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>double</returns>
    public override double GetDouble(int i)
    {
        CheckDisposed();
        VerifyForGet();

        if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UseConnectionReadValueCallbacks))
        {
            SQLiteDataReaderValue value = new SQLiteDataReaderValue();
            bool complete;

            InvokeReadValueCallback(i, new SQLiteReadValueEventArgs(
                "GetDouble", null, value), out complete);

            if (complete)
            {
                if (value.DoubleValue == null)
                    throw new SQLiteException("missing double return value");

                return (double)value.DoubleValue;
            }
        }

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetDouble(i - PrivateVisibleFieldCount);

        VerifyType(i, DbType.Double);
        return _activeStatement._sql.GetDouble(_activeStatement, i);
    }

    /// <summary>
    /// Determines and returns the <see cref="TypeAffinity" /> of the
    /// specified column.
    /// </summary>
    /// <param name="i">
    /// The index of the column.
    /// </param>
    /// <returns>
    /// The <see cref="TypeAffinity" /> associated with the specified
    /// column, if any.
    /// </returns>
    public TypeAffinity GetFieldAffinity(int i)
    {
        CheckDisposed();

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetFieldAffinity(i - PrivateVisibleFieldCount);

        return GetSQLiteType(_flags, i).Affinity;
    }

    /// <summary>
    /// Returns the .NET type of a given column
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>Type</returns>
    public override Type GetFieldType(int i)
    {
        CheckDisposed();

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetFieldType(i - PrivateVisibleFieldCount);

        return SQLiteConvert.SQLiteTypeToType(GetSQLiteType(_flags, i));
    }

    /// <summary>
    /// Returns a column as a float value
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>float</returns>
    public override float GetFloat(int i)
    {
        CheckDisposed();
        VerifyForGet();

        if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UseConnectionReadValueCallbacks))
        {
            SQLiteDataReaderValue value = new SQLiteDataReaderValue();
            bool complete;

            InvokeReadValueCallback(i, new SQLiteReadValueEventArgs(
                "GetFloat", null, value), out complete);

            if (complete)
            {
                if (value.FloatValue == null)
                    throw new SQLiteException("missing float return value");

                return (float)value.FloatValue;
            }
        }

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetFloat(i - PrivateVisibleFieldCount);

        VerifyType(i, DbType.Single);
        return Convert.ToSingle(_activeStatement._sql.GetDouble(_activeStatement, i));
    }

    /// <summary>
    /// Returns the column as a Guid
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>Guid</returns>
    public override Guid GetGuid(int i)
    {
        CheckDisposed();
        VerifyForGet();

        if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UseConnectionReadValueCallbacks))
        {
            SQLiteDataReaderValue value = new SQLiteDataReaderValue();
            bool complete;

            InvokeReadValueCallback(i, new SQLiteReadValueEventArgs(
                "GetGuid", null, value), out complete);

            if (complete)
            {
                if (value.GuidValue == null)
                    throw new SQLiteException("missing guid return value");

                return (Guid)value.GuidValue;
            }
        }

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetGuid(i - PrivateVisibleFieldCount);

        TypeAffinity affinity = VerifyType(i, DbType.Guid);
        if (affinity == TypeAffinity.Blob)
        {
            byte[] buffer = new byte[16];
            _activeStatement._sql.GetBytes(_activeStatement, i, 0, buffer, 0, 16);
            return new Guid(buffer);
        }
        else
            return new Guid(_activeStatement._sql.GetText(_activeStatement, i));
    }

    /// <summary>
    /// Returns the column as a short
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>Int16</returns>
    public override Int16 GetInt16(int i)
    {
        CheckDisposed();
        VerifyForGet();

        if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UseConnectionReadValueCallbacks))
        {
            SQLiteDataReaderValue value = new SQLiteDataReaderValue();
            bool complete;

            InvokeReadValueCallback(i, new SQLiteReadValueEventArgs(
                "GetInt16", null, value), out complete);

            if (complete)
            {
                if (value.Int16Value == null)
                    throw new SQLiteException("missing int16 return value");

                return (Int16)value.Int16Value;
            }
        }

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetInt16(i - PrivateVisibleFieldCount);

        VerifyType(i, DbType.Int16);
        return _activeStatement._sql.GetInt16(_activeStatement, i);
    }

    /// <summary>
    /// Retrieves the column as an int
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>Int32</returns>
    public override Int32 GetInt32(int i)
    {
        CheckDisposed();
        VerifyForGet();

        if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UseConnectionReadValueCallbacks))
        {
            SQLiteDataReaderValue value = new SQLiteDataReaderValue();
            bool complete;

            InvokeReadValueCallback(i, new SQLiteReadValueEventArgs(
                "GetInt32", null, value), out complete);

            if (complete)
            {
                if (value.Int32Value == null)
                    throw new SQLiteException("missing int32 return value");

                return (Int32)value.Int32Value;
            }
        }

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetInt32(i - PrivateVisibleFieldCount);

        VerifyType(i, DbType.Int32);
        return _activeStatement._sql.GetInt32(_activeStatement, i);
    }

    /// <summary>
    /// Retrieves the column as a long
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>Int64</returns>
    public override Int64 GetInt64(int i)
    {
        CheckDisposed();
        VerifyForGet();

        if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UseConnectionReadValueCallbacks))
        {
            SQLiteDataReaderValue value = new SQLiteDataReaderValue();
            bool complete;

            InvokeReadValueCallback(i, new SQLiteReadValueEventArgs(
                "GetInt64", null, value), out complete);

            if (complete)
            {
                if (value.Int64Value == null)
                    throw new SQLiteException("missing int64 return value");

                return (Int64)value.Int64Value;
            }
        }

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetInt64(i - PrivateVisibleFieldCount);

        VerifyType(i, DbType.Int64);
        return _activeStatement._sql.GetInt64(_activeStatement, i);
    }

    /// <summary>
    /// Retrieves the name of the column
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>string</returns>
    public override string GetName(int i)
    {
        CheckDisposed();

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetName(i - PrivateVisibleFieldCount);

        return _activeStatement._sql.ColumnName(_activeStatement, i);
    }

    /// <summary>
    /// Returns the name of the database associated with the specified column.
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>string</returns>
    public string GetDatabaseName(int i)
    {
        CheckDisposed();

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetDatabaseName(i - PrivateVisibleFieldCount);

        return _activeStatement._sql.ColumnDatabaseName(_activeStatement, i);
    }

    /// <summary>
    /// Returns the name of the table associated with the specified column.
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>string</returns>
    public string GetTableName(int i)
    {
        CheckDisposed();

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetTableName(i - PrivateVisibleFieldCount);

        return _activeStatement._sql.ColumnTableName(_activeStatement, i);
    }

    /// <summary>
    /// Returns the original name of the specified column.
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>string</returns>
    public string GetOriginalName(int i)
    {
        CheckDisposed();

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetName(i - PrivateVisibleFieldCount);

        return _activeStatement._sql.ColumnOriginalName(_activeStatement, i);
    }

    /// <summary>
    /// Retrieves the i of a column, given its name
    /// </summary>
    /// <param name="name">The name of the column to retrieve</param>
    /// <returns>The int i of the column</returns>
    public override int GetOrdinal(string name)
    {
      CheckDisposed();

      if (_throwOnDisposed) SQLiteCommand.Check(_command);

      //
      // NOTE: First, check if the column name cache has been initialized yet.
      //       If not, do it now.
      //
      if (_fieldIndexes == null)
      {
          _fieldIndexes = new Dictionary<string, int>(
              StringComparer.OrdinalIgnoreCase);
      }

      //
      // NOTE: Next, see if the index for the requested column name has been
      //       cached already.  If so, return the cached value.  Otherwise,
      //       lookup the value and then cache the result for future use.
      //
      int r;

      if (!_fieldIndexes.TryGetValue(name, out r))
      {
          r = _activeStatement._sql.ColumnIndex(_activeStatement, name);

          if (r == -1 && _keyInfo != null)
          {
              r = _keyInfo.GetOrdinal(name);
              if (r > -1) r += PrivateVisibleFieldCount;
          }

          _fieldIndexes.Add(name, r);
      }

      if (r == -1 && HelperMethods.HasFlags(
            _flags, SQLiteConnectionFlags.StrictConformance))
      {
          throw new IndexOutOfRangeException();
      }

      return r;
    }

    /// <summary>
    /// Schema information in SQLite is difficult to map into .NET conventions, so a lot of work must be done
    /// to gather the necessary information so it can be represented in an ADO.NET manner.
    /// </summary>
    /// <returns>Returns a DataTable containing the schema information for the active SELECT statement being processed.</returns>
    public override DataTable GetSchemaTable()
    {
      CheckDisposed();
      return GetSchemaTable(true, false);
    }

    ///////////////////////////////////////////////////////////////////////////

    #region ColumnParent Class
    private sealed class ColumnParent : IEqualityComparer<ColumnParent>
    {
        #region Public Fields
        public string DatabaseName;
        public string TableName;
        public string ColumnName;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        public ColumnParent()
        {
            // do nothing.
        }

        ///////////////////////////////////////////////////////////////////////

        public ColumnParent(
            string databaseName,
            string tableName,
            string columnName
            )
            : this()
        {
            this.DatabaseName = databaseName;
            this.TableName = tableName;
            this.ColumnName = columnName;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IEqualityComparer<ColumnParent> Members
        public bool Equals(ColumnParent x, ColumnParent y)
        {
            if ((x == null) && (y == null))
            {
                return true;
            }
            else if ((x == null) || (y == null))
            {
                return false;
            }
            else
            {
                if (!String.Equals(x.DatabaseName, y.DatabaseName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (!String.Equals(x.TableName, y.TableName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (!String.Equals(x.ColumnName, y.ColumnName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                return true;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        public int GetHashCode(ColumnParent obj)
        {
            int result = 0;

            if ((obj != null) && (obj.DatabaseName != null))
                result ^= obj.DatabaseName.GetHashCode();

            if ((obj != null) && (obj.TableName != null))
                result ^= obj.TableName.GetHashCode();

            if ((obj != null) && (obj.ColumnName != null))
                result ^= obj.ColumnName.GetHashCode();

            return result;
        }
        #endregion
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    private static void GetStatementColumnParents(
        SQLiteBase sql,
        SQLiteStatement stmt,
        int fieldCount,
        ref Dictionary<ColumnParent, List<int>> parentToColumns,
        ref Dictionary<int, ColumnParent> columnToParent
        )
    {
        if (parentToColumns == null)
            parentToColumns = new Dictionary<ColumnParent, List<int>>(
                new ColumnParent());

        if (columnToParent == null)
            columnToParent = new Dictionary<int, ColumnParent>();

        for (int n = 0; n < fieldCount; n++)
        {
            string databaseName = sql.ColumnDatabaseName(stmt, n);
            string tableName = sql.ColumnTableName(stmt, n);
            string columnName = sql.ColumnOriginalName(stmt, n);

            ColumnParent key = new ColumnParent(databaseName, tableName, null);
            ColumnParent value = new ColumnParent(databaseName, tableName, columnName);

            List<int> indexList;

            if (!parentToColumns.TryGetValue(key, out indexList))
                parentToColumns.Add(key, new List<int>(new int[] { n }));
            else if (indexList != null)
                indexList.Add(n);
            else
                parentToColumns[key] = new List<int>(new int[] { n });

            columnToParent.Add(n, value);
        }
    }

    ///////////////////////////////////////////////////////////////////////////

    private static int CountParents(
        Dictionary<ColumnParent, List<int>> parentToColumns
        )
    {
        int result = 0;

        if (parentToColumns != null)
        {
            foreach (ColumnParent key in parentToColumns.Keys)
            {
                if (key == null)
                    continue;

                string tableName = key.TableName;

                if (String.IsNullOrEmpty(tableName))
                    continue;

                result++;
            }
        }

        return result;
    }

    ///////////////////////////////////////////////////////////////////////////

    internal DataTable GetSchemaTable(bool wantUniqueInfo, bool wantDefaultValue)
    {
        CheckClosed();
        if (_throwOnDisposed) SQLiteCommand.Check(_command);

        //
        // BUGFIX: We need to quickly scan all the fields in the current
        //         "result set" to see how many distinct tables are actually
        //         involved.  This information is necessary so that some
        //         intelligent decisions can be made when constructing the
        //         metadata below.  For example, we need to be very careful
        //         about flagging a particular column as "unique" just
        //         because it was in its original underlying database table
        //         if there are now multiple tables involved in the
        //         "result set".  See ticket [7e3fa93744] for more detailed
        //         information.
        //
        Dictionary<ColumnParent, List<int>> parentToColumns = null;
        Dictionary<int, ColumnParent> columnToParent = null;
        SQLiteBase sql = _command.Connection._sql;

        GetStatementColumnParents(
            sql, _activeStatement, _fieldCount,
            ref parentToColumns, ref columnToParent);

        DataTable tbl = new DataTable("SchemaTable");
        DataTable tblIndexes = null;
        DataTable tblIndexColumns;
        DataRow row;
        string temp;
        string strCatalog = String.Empty;
        string strTable = String.Empty;
        string strColumn = String.Empty;

        tbl.Locale = CultureInfo.InvariantCulture;
        tbl.Columns.Add(SchemaTableColumn.ColumnName, typeof(String));
        tbl.Columns.Add(SchemaTableColumn.ColumnOrdinal, typeof(int));
        tbl.Columns.Add(SchemaTableColumn.ColumnSize, typeof(int));
        tbl.Columns.Add(SchemaTableColumn.NumericPrecision, typeof(int));
        tbl.Columns.Add(SchemaTableColumn.NumericScale, typeof(int));
        tbl.Columns.Add(SchemaTableColumn.IsUnique, typeof(Boolean));
        tbl.Columns.Add(SchemaTableColumn.IsKey, typeof(Boolean));
        tbl.Columns.Add(SchemaTableOptionalColumn.BaseServerName, typeof(string));
        tbl.Columns.Add(SchemaTableOptionalColumn.BaseCatalogName, typeof(String));
        tbl.Columns.Add(SchemaTableColumn.BaseColumnName, typeof(String));
        tbl.Columns.Add(SchemaTableColumn.BaseSchemaName, typeof(String));
        tbl.Columns.Add(SchemaTableColumn.BaseTableName, typeof(String));
        tbl.Columns.Add(SchemaTableColumn.DataType, typeof(Type));
        tbl.Columns.Add(SchemaTableColumn.AllowDBNull, typeof(Boolean));
        tbl.Columns.Add(SchemaTableColumn.ProviderType, typeof(int));
        tbl.Columns.Add(SchemaTableColumn.IsAliased, typeof(Boolean));
        tbl.Columns.Add(SchemaTableColumn.IsExpression, typeof(Boolean));
        tbl.Columns.Add(SchemaTableOptionalColumn.IsAutoIncrement, typeof(Boolean));
        tbl.Columns.Add(SchemaTableOptionalColumn.IsRowVersion, typeof(Boolean));
        tbl.Columns.Add(SchemaTableOptionalColumn.IsHidden, typeof(Boolean));
        tbl.Columns.Add(SchemaTableColumn.IsLong, typeof(Boolean));
        tbl.Columns.Add(SchemaTableOptionalColumn.IsReadOnly, typeof(Boolean));
        tbl.Columns.Add(SchemaTableOptionalColumn.ProviderSpecificDataType, typeof(Type));
        tbl.Columns.Add(SchemaTableOptionalColumn.DefaultValue, typeof(object));
        tbl.Columns.Add("DataTypeName", typeof(string));
        tbl.Columns.Add("CollationType", typeof(string));
        tbl.BeginLoadData();

        for (int n = 0; n < _fieldCount; n++)
        {
            SQLiteType sqlType = GetSQLiteType(_flags, n);

            row = tbl.NewRow();

            DbType typ = sqlType.Type;

            // Default settings for the column
            row[SchemaTableColumn.ColumnName] = GetName(n);
            row[SchemaTableColumn.ColumnOrdinal] = n;
            row[SchemaTableColumn.ColumnSize] = SQLiteConvert.DbTypeToColumnSize(typ);
            row[SchemaTableColumn.NumericPrecision] = SQLiteConvert.DbTypeToNumericPrecision(typ);
            row[SchemaTableColumn.NumericScale] = SQLiteConvert.DbTypeToNumericScale(typ);
            row[SchemaTableColumn.ProviderType] = sqlType.Type;
            row[SchemaTableColumn.IsLong] = false;
            row[SchemaTableColumn.AllowDBNull] = true;
            row[SchemaTableOptionalColumn.IsReadOnly] = false;
            row[SchemaTableOptionalColumn.IsRowVersion] = false;
            row[SchemaTableColumn.IsUnique] = false;
            row[SchemaTableColumn.IsKey] = false;
            row[SchemaTableOptionalColumn.IsAutoIncrement] = false;
            row[SchemaTableColumn.DataType] = GetFieldType(n);
            row[SchemaTableOptionalColumn.IsHidden] = false;
            row[SchemaTableColumn.BaseSchemaName] = _baseSchemaName;

            strColumn = columnToParent[n].ColumnName;
            if (String.IsNullOrEmpty(strColumn) == false) row[SchemaTableColumn.BaseColumnName] = strColumn;

            row[SchemaTableColumn.IsExpression] = String.IsNullOrEmpty(strColumn);
            row[SchemaTableColumn.IsAliased] = (String.Compare(GetName(n), strColumn, StringComparison.OrdinalIgnoreCase) != 0);

            temp = columnToParent[n].TableName;
            if (String.IsNullOrEmpty(temp) == false) row[SchemaTableColumn.BaseTableName] = temp;

            temp = columnToParent[n].DatabaseName;
            if (String.IsNullOrEmpty(temp) == false) row[SchemaTableOptionalColumn.BaseCatalogName] = temp;

            string dataType = null;
            // If we have a table-bound column, extract the extra information from it
            if (String.IsNullOrEmpty(strColumn) == false)
            {
                string baseCatalogName = String.Empty;

                if (row[SchemaTableOptionalColumn.BaseCatalogName] != DBNull.Value)
                    baseCatalogName = (string)row[SchemaTableOptionalColumn.BaseCatalogName];

                string baseTableName = String.Empty;

                if (row[SchemaTableColumn.BaseTableName] != DBNull.Value)
                    baseTableName = (string)row[SchemaTableColumn.BaseTableName];

                if (sql.DoesTableExist(baseCatalogName, baseTableName))
                {
                    string baseColumnName = String.Empty;

                    if (row[SchemaTableColumn.BaseColumnName] != DBNull.Value)
                        baseColumnName = (string)row[SchemaTableColumn.BaseColumnName];

                    string collSeq = null;
                    bool bNotNull = false;
                    bool bPrimaryKey = false;
                    bool bAutoIncrement = false;
                    string[] arSize;

                    // Get the column meta data
                    _command.Connection._sql.ColumnMetaData(
                        baseCatalogName,
                        baseTableName,
                        strColumn,
                        true,
                        ref dataType, ref collSeq, ref bNotNull, ref bPrimaryKey, ref bAutoIncrement);

                    if (bNotNull || bPrimaryKey) row[SchemaTableColumn.AllowDBNull] = false;
                    bool allowDbNull = (bool)row[SchemaTableColumn.AllowDBNull];

                    row[SchemaTableColumn.IsKey] = bPrimaryKey && CountParents(parentToColumns) <= 1;
                    row[SchemaTableOptionalColumn.IsAutoIncrement] = bAutoIncrement;
                    row["CollationType"] = collSeq;

                    // For types like varchar(50) and such, extract the size
                    arSize = dataType.Split('(');
                    if (arSize.Length > 1)
                    {
                        dataType = arSize[0];
                        arSize = arSize[1].Split(')');
                        if (arSize.Length > 1)
                        {
                            arSize = arSize[0].Split(',', '.');
                            if (sqlType.Type == DbType.Binary || SQLiteConvert.IsStringDbType(sqlType.Type))
                            {
                                row[SchemaTableColumn.ColumnSize] = Convert.ToInt32(arSize[0], CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                row[SchemaTableColumn.NumericPrecision] = Convert.ToInt32(arSize[0], CultureInfo.InvariantCulture);
                                if (arSize.Length > 1)
                                    row[SchemaTableColumn.NumericScale] = Convert.ToInt32(arSize[1], CultureInfo.InvariantCulture);
                            }
                        }
                    }

                    if (wantDefaultValue)
                    {
                        // Determine the default value for the column, which sucks because we have to query the schema for each column
                        using (SQLiteCommand cmdTable = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "PRAGMA [{0}].TABLE_INFO([{1}])",
                            baseCatalogName,
                            baseTableName
                        ), _command.Connection))
                        using (DbDataReader rdTable = cmdTable.ExecuteReader())
                        {
                            // Find the matching column
                            while (rdTable.Read())
                            {
                                if (String.Compare(baseColumnName, rdTable.GetString(1), StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    if (rdTable.IsDBNull(4) == false)
                                        row[SchemaTableOptionalColumn.DefaultValue] = rdTable[4];

                                    break;
                                }
                            }
                        }
                    }

                    // Determine IsUnique properly, which is a pain in the butt!
                    if (wantUniqueInfo)
                    {
                        if (baseCatalogName != strCatalog || baseTableName != strTable)
                        {
                            strCatalog = baseCatalogName;
                            strTable = baseTableName;

                            tblIndexes = _command.Connection.GetSchema("Indexes", new string[] {
                                baseCatalogName,
                                null,
                                baseTableName,
                                null
                            });
                        }

                        foreach (DataRow rowIndexes in tblIndexes.Rows)
                        {
                            tblIndexColumns = _command.Connection.GetSchema("IndexColumns", new string[] {
                                baseCatalogName,
                                null,
                                baseTableName,
                                (string)rowIndexes["INDEX_NAME"],
                                null
                            });
                            foreach (DataRow rowColumnIndex in tblIndexColumns.Rows)
                            {
                                if (String.Compare(SQLiteConvert.GetStringOrNull(rowColumnIndex["COLUMN_NAME"]), strColumn, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    //
                                    // BUGFIX: Make sure that we only flag this column as "unique"
                                    //         if we are not processing of some kind of multi-table
                                    //         construct (i.e. a join) because in that case we must
                                    //         allow duplicate values (refer to ticket [7e3fa93744]).
                                    //
                                    if (parentToColumns.Count == 1 && tblIndexColumns.Rows.Count == 1 && allowDbNull == false)
                                        row[SchemaTableColumn.IsUnique] = rowIndexes["UNIQUE"];

                                    // If its an integer primary key and the only primary key in the table, then its a rowid alias and is autoincrement
                                    // NOTE:  Currently commented out because this is not always the desired behavior.  For example, a 1:1 relationship with
                                    //        another table, where the other table is autoincrement, but this one is not, and uses the rowid from the other.
                                    //        It is safer to only set Autoincrement on tables where we're SURE the user specified AUTOINCREMENT, even if its a rowid column.

                                    //if (tblIndexColumns.Rows.Count == 1 && (bool)rowIndexes["PRIMARY_KEY"] == true && String.IsNullOrEmpty(dataType) == false &&
                                    //  String.Compare(dataType, "integer", StringComparison.OrdinalIgnoreCase) == 0)
                                    //{
                                    //    //  row[SchemaTableOptionalColumn.IsAutoIncrement] = true;
                                    //}

                                    break;
                                }
                            }
                        }
                    }
                }

                if (String.IsNullOrEmpty(dataType))
                {
                    TypeAffinity affin = TypeAffinity.Uninitialized;
                    dataType = _activeStatement._sql.ColumnType(_activeStatement, n, ref affin);
                }

                if (String.IsNullOrEmpty(dataType) == false)
                    row["DataTypeName"] = dataType;
            }

            tbl.Rows.Add(row);
        }

        if (_keyInfo != null)
            _keyInfo.AppendSchemaTable(tbl);

        tbl.AcceptChanges();
        tbl.EndLoadData();

        return tbl;
    }

    /// <summary>
    /// Retrieves the column as a string
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>string</returns>
    public override string GetString(int i)
    {
        CheckDisposed();
        VerifyForGet();

        if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UseConnectionReadValueCallbacks))
        {
            SQLiteDataReaderValue value = new SQLiteDataReaderValue();
            bool complete;

            InvokeReadValueCallback(i, new SQLiteReadValueEventArgs(
                "GetString", null, value), out complete);

            if (complete)
                return value.StringValue;
        }

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetString(i - PrivateVisibleFieldCount);

        if (!HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.NoVerifyTextAffinity))
            VerifyType(i, DbType.String);

        return _activeStatement._sql.GetText(_activeStatement, i);
    }

    /// <summary>
    /// Retrieves the column as an object corresponding to the underlying datatype of the column
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>object</returns>
    public override object GetValue(int i)
    {
        CheckDisposed();
        VerifyForGet();

        if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UseConnectionReadValueCallbacks))
        {
            SQLiteDataReaderValue value = new SQLiteDataReaderValue();
            bool complete;

            InvokeReadValueCallback(i, new SQLiteReadValueEventArgs(
                "GetValue", null, value), out complete);

            if (complete)
                return value.Value;
        }

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.GetValue(i - PrivateVisibleFieldCount);

        SQLiteType typ = GetSQLiteType(_flags, i);

        if (HelperMethods.HasFlags(
                _flags, SQLiteConnectionFlags.DetectTextAffinity) &&
            ((typ == null) || (typ.Affinity == TypeAffinity.Text)))
        {
            typ = GetSQLiteType(
                typ, _activeStatement._sql.GetText(_activeStatement, i));
        }
        else if (HelperMethods.HasFlags(
                _flags, SQLiteConnectionFlags.DetectStringType) &&
            ((typ == null) || SQLiteConvert.IsStringDbType(typ.Type)))
        {
            typ = GetSQLiteType(
                typ, _activeStatement._sql.GetText(_activeStatement, i));
        }

        return _activeStatement._sql.GetValue(_activeStatement, _flags, i, typ);
    }

    /// <summary>
    /// Retreives the values of multiple columns, up to the size of the supplied array
    /// </summary>
    /// <param name="values">The array to fill with values from the columns in the current resultset</param>
    /// <returns>The number of columns retrieved</returns>
    public override int GetValues(object[] values)
    {
      CheckDisposed();

      int nMax = FieldCount;
      if (values.Length < nMax) nMax = values.Length;

      for (int n = 0; n < nMax; n++)
      {
        values[n] = GetValue(n);
      }

      return nMax;
    }

    /// <summary>
    /// Returns a collection containing all the column names and values for the
    /// current row of data in the current resultset, if any.  If there is no
    /// current row or no current resultset, an exception may be thrown.
    /// </summary>
    /// <returns>
    /// The collection containing the column name and value information for the
    /// current row of data in the current resultset or null if this information
    /// cannot be obtained.
    /// </returns>
    public NameValueCollection GetValues()
    {
        CheckDisposed();

        if ((_activeStatement == null) || (_activeStatement._sql == null))
            throw new InvalidOperationException();

        int nMax = PrivateVisibleFieldCount;
        NameValueCollection result = new NameValueCollection(nMax);

        for (int n = 0; n < nMax; n++)
        {
            string name = _activeStatement._sql.ColumnName(_activeStatement, n);
            string value = _activeStatement._sql.GetText(_activeStatement, n);

            result.Add(name, value);
        }

        return result;
    }

    /// <summary>
    /// Returns True if the resultset has rows that can be fetched
    /// </summary>
    public override bool HasRows
    {
      get
      {
        CheckDisposed();
        CheckClosed();

        //
        // NOTE: If the "sticky" flag has been set, use the new behavior,
        //       which returns non-zero if there were ever any rows in
        //       the associated result sets.  Generally, this flag is only
        //       useful when it is necessary to retain compatibility with
        //       other ADO.NET providers that use these same semantics for
        //       the HasRows property.
        //
        if (HelperMethods.HasFlags(
                _flags, SQLiteConnectionFlags.StickyHasRows))
        {
          return ((_readingState != 1) || (_stepCount > 0));
        }

        //
        // NOTE: This is the default behavior.  It returns non-zero only if
        //       more rows are available (i.e. a call to the Read method is
        //       expected to succeed).  Prior to the introduction of the
        //       "sticky" flag, this is how this property has always worked.
        //
        return (_readingState != 1);
      }
    }

    /// <summary>
    /// Returns True if the data reader is closed
    /// </summary>
    public override bool IsClosed
    {
      get { CheckDisposed(); return (_command == null); }
    }

    /// <summary>
    /// Returns True if the specified column is null
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>True or False</returns>
    public override bool IsDBNull(int i)
    {
        CheckDisposed();
        VerifyForGet();

        if (i >= PrivateVisibleFieldCount && _keyInfo != null)
            return _keyInfo.IsDBNull(i - PrivateVisibleFieldCount);

        return _activeStatement._sql.IsNull(_activeStatement, i);
    }

    /// <summary>
    /// Moves to the next resultset in multiple row-returning SQL command.
    /// </summary>
    /// <returns>True if the command was successful and a new resultset is available, False otherwise.</returns>
    public override bool NextResult()
    {
      CheckDisposed();
      CheckClosed();
      if (_throwOnDisposed) SQLiteCommand.Check(_command);

      SQLiteStatement stmt = null;
      int fieldCount;
      bool schemaOnly = ((_commandBehavior & CommandBehavior.SchemaOnly) != 0);

      while (true)
      {
        if (stmt == null && _activeStatement != null && _activeStatement._sql != null && _activeStatement._sql.IsOpen())
        {
          // Reset the previously-executed statement
          if (!schemaOnly) _activeStatement._sql.Reset(_activeStatement);

          // If we're only supposed to return a single rowset, step through all remaining statements once until
          // they are all done and return false to indicate no more resultsets exist.
          if ((_commandBehavior & CommandBehavior.SingleResult) != 0)
          {
            for (; ; )
            {
              stmt = _command.GetStatement(_activeStatementIndex + 1);
              if (stmt == null) break;
              _activeStatementIndex++;

              if (!schemaOnly && stmt._sql.Step(stmt)) _stepCount++;
              if (stmt._sql.ColumnCount(stmt) == 0)
              {
                int changes = 0;
                bool readOnly = false;
                if (stmt.TryGetChanges(ref changes, ref readOnly))
                {
                  if (!readOnly)
                  {
                    if (_rowsAffected == -1) _rowsAffected = 0;
                    _rowsAffected += changes;
                  }
                }
                else
                {
                  return false;
                }
              }
              if (!schemaOnly) stmt._sql.Reset(stmt); // Gotta reset after every step to release any locks and such!
            }
            return false;
          }
        }

        // Get the next statement to execute
        stmt = _command.GetStatement(_activeStatementIndex + 1);

        // If we've reached the end of the statements, return false, no more resultsets
        if (stmt == null)
          return false;

        // If we were on a current resultset, set the state to "done reading" for it
        if (_readingState < 1)
          _readingState = 1;

        _activeStatementIndex++;

        fieldCount = stmt._sql.ColumnCount(stmt);

        // If the statement is not a select statement or we're not retrieving schema only, then perform the initial step
        if (!schemaOnly || (fieldCount == 0))
        {
          if (!schemaOnly && stmt._sql.Step(stmt))
          {
            _stepCount++;
            _readingState = -1;
          }
          else if (fieldCount == 0) // No rows returned, if fieldCount is zero, skip to the next statement
          {
            int changes = 0;
            bool readOnly = false;
            if (stmt.TryGetChanges(ref changes, ref readOnly))
            {
              if (!readOnly)
              {
                if (_rowsAffected == -1) _rowsAffected = 0;
                _rowsAffected += changes;
              }
            }
            else
            {
              return false;
            }
            if (!schemaOnly) stmt._sql.Reset(stmt);
            continue; // Skip this command and move to the next, it was not a row-returning resultset
          }
          else // No rows, fieldCount is non-zero so stop here
          {
            _readingState = 1; // This command returned columns but no rows, so return true, but HasRows = false and Read() returns false
          }
        }

        // Ahh, we found a row-returning resultset eligible to be returned!
        _activeStatement = stmt;
        _fieldCount = fieldCount;
        _fieldIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        _fieldTypeArray = new SQLiteType[PrivateVisibleFieldCount];

        if ((_commandBehavior & CommandBehavior.KeyInfo) != 0)
          LoadKeyInfo();

        return true;
      }
    }

    /// <summary>
    /// This method attempts to query the database connection associated with
    /// the data reader in use.  If the underlying command or connection is
    /// unavailable, a null value will be returned.
    /// </summary>
    /// <returns>
    /// The connection object -OR- null if it is unavailable.
    /// </returns>
    internal static SQLiteConnection GetConnection(
        SQLiteDataReader dataReader
        )
    {
        try
        {
            if (dataReader != null)
            {
                SQLiteCommand command = dataReader._command;

                if (command != null)
                {
                    SQLiteConnection connection = command.Connection;

                    if (connection != null)
                        return connection;
                }
            }
        }
        catch (ObjectDisposedException)
        {
            // do nothing.
        }

        return null;
    }

    /// <summary>
    /// Retrieves the SQLiteType for a given column and row value.
    /// </summary>
    /// <param name="oldType">
    /// The original SQLiteType structure, based only on the column.
    /// </param>
    /// <param name="text">
    /// The textual value of the column for a given row.
    /// </param>
    /// <returns>
    /// The SQLiteType structure.
    /// </returns>
    private SQLiteType GetSQLiteType(
        SQLiteType oldType, /* PASS-THROUGH */
        string text
        )
    {
        if (SQLiteConvert.LooksLikeNull(text))
            return new SQLiteType(TypeAffinity.Null, DbType.Object);

        if (SQLiteConvert.LooksLikeInt64(text))
            return new SQLiteType(TypeAffinity.Int64, DbType.Int64);

        if (SQLiteConvert.LooksLikeDouble(text))
            return new SQLiteType(TypeAffinity.Double, DbType.Double);

        if ((_activeStatement != null) &&
            SQLiteConvert.LooksLikeDateTime(_activeStatement._sql, text))
        {
            return new SQLiteType(TypeAffinity.DateTime, DbType.DateTime);
        }

        return oldType;
    }

    /// <summary>
    /// Retrieves the SQLiteType for a given column, and caches it to avoid repetetive interop calls.
    /// </summary>
    /// <param name="flags">The flags associated with the parent connection object.</param>
    /// <param name="i">The index of the column.</param>
    /// <returns>A SQLiteType structure</returns>
    private SQLiteType GetSQLiteType(SQLiteConnectionFlags flags, int i)
    {
        SQLiteType typ = _fieldTypeArray[i];

        if (typ == null)
        {
            // Initialize this column's field type instance
            typ = _fieldTypeArray[i] = new SQLiteType();
        }

        // If not initialized, then fetch the declared column datatype and attempt to convert it
        // to a known DbType.
        if (typ.Affinity == TypeAffinity.Uninitialized)
        {
            typ.Type = SQLiteConvert.TypeNameToDbType(
                GetConnection(this), _activeStatement._sql.ColumnType(
                _activeStatement, i, ref typ.Affinity), flags);
        }
        else
        {
            typ.Affinity = _activeStatement._sql.ColumnAffinity(
                _activeStatement, i);
        }

        return typ;
    }

    /// <summary>
    /// Reads the next row from the resultset
    /// </summary>
    /// <returns>True if a new row was successfully loaded and is ready for processing</returns>
    public override bool Read()
    {
      CheckDisposed();
      CheckClosed();
      if (_throwOnDisposed) SQLiteCommand.Check(_command);

      if ((_commandBehavior & CommandBehavior.SchemaOnly) != 0)
        return false;

      if (_readingState == -1) // First step was already done at the NextResult() level, so don't step again, just return true.
      {
        _readingState = 0;
        return true;
      }
      else if (_readingState == 0) // Actively reading rows
      {
        // Don't read a new row if the command behavior dictates SingleRow.  We've already read the first row.
        if ((_commandBehavior & CommandBehavior.SingleRow) == 0)
        {
          if (_activeStatement._sql.Step(_activeStatement) == true)
          {
            _stepCount++;

            if (_keyInfo != null)
              _keyInfo.Reset();

            return true;
          }
        }

        _readingState = 1; // Finished reading rows
      }

      return false;
    }

    /// <summary>
    /// Returns the number of rows affected by the statement being executed.
    /// The value returned may not be accurate for DDL statements.  Also, it
    /// will be -1 for any statement that does not modify the database (e.g.
    /// SELECT).  If an otherwise read-only statement modifies the database
    /// indirectly (e.g. via a virtual table or user-defined function), the
    /// value returned is undefined.
    /// </summary>
    public override int RecordsAffected
    {
      get { CheckDisposed(); return _rowsAffected; }
    }

    /// <summary>
    /// Indexer to retrieve data from a column given its name
    /// </summary>
    /// <param name="name">The name of the column to retrieve data for</param>
    /// <returns>The value contained in the column</returns>
    public override object this[string name]
    {
      get { CheckDisposed(); return GetValue(GetOrdinal(name)); }
    }

    /// <summary>
    /// Indexer to retrieve data from a column given its i
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>The value contained in the column</returns>
    public override object this[int i]
    {
      get { CheckDisposed(); return GetValue(i); }
    }

    private void LoadKeyInfo()
    {
        if (_keyInfo != null)
        {
            _keyInfo.Dispose();
            _keyInfo = null;
        }

        _keyInfo = new SQLiteKeyReader(_command.Connection, this, _activeStatement);
    }
  }
}
