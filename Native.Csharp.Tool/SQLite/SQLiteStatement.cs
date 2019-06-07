/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 *
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
  using System;
  using System.Globalization;

  /// <summary>
  /// Represents a single SQL statement in SQLite.
  /// </summary>
  internal sealed class SQLiteStatement : IDisposable
  {
    /// <summary>
    /// The underlying SQLite object this statement is bound to
    /// </summary>
    internal SQLiteBase        _sql;
    /// <summary>
    /// The command text of this SQL statement
    /// </summary>
    internal string            _sqlStatement;
    /// <summary>
    /// The actual statement pointer
    /// </summary>
    internal SQLiteStatementHandle  _sqlite_stmt;
    /// <summary>
    /// An index from which unnamed parameters begin
    /// </summary>
    internal int               _unnamedParameters;
    /// <summary>
    /// Names of the parameters as SQLite understands them to be
    /// </summary>
    internal string[]          _paramNames;
    /// <summary>
    /// Parameters for this statement
    /// </summary>
    internal SQLiteParameter[] _paramValues;
    /// <summary>
    /// Command this statement belongs to (if any)
    /// </summary>
    internal SQLiteCommand     _command;

    /// <summary>
    /// The flags associated with the parent connection object.
    /// </summary>
    private SQLiteConnectionFlags _flags;

    private string[] _types;

    /// <summary>
    /// Initializes the statement and attempts to get all information about parameters in the statement
    /// </summary>
    /// <param name="sqlbase">The base SQLite object</param>
    /// <param name="flags">The flags associated with the parent connection object</param>
    /// <param name="stmt">The statement</param>
    /// <param name="strCommand">The command text for this statement</param>
    /// <param name="previous">The previous command in a multi-statement command</param>
    internal SQLiteStatement(SQLiteBase sqlbase, SQLiteConnectionFlags flags, SQLiteStatementHandle stmt, string strCommand, SQLiteStatement previous)
    {
      _sql     = sqlbase;
      _sqlite_stmt = stmt;
      _sqlStatement  = strCommand;
      _flags = flags;

      // Determine parameters for this statement (if any) and prepare space for them.
      int nCmdStart = 0;
      int n = _sql.Bind_ParamCount(this, _flags);
      int x;
      string s;

      if (n > 0)
      {
        if (previous != null)
          nCmdStart = previous._unnamedParameters;

        _paramNames = new string[n];
        _paramValues = new SQLiteParameter[n];

        for (x = 0; x < n; x++)
        {
          s = _sql.Bind_ParamName(this, _flags, x + 1);
          if (String.IsNullOrEmpty(s))
          {
            s = HelperMethods.StringFormat(CultureInfo.InvariantCulture, ";{0}", nCmdStart);
            nCmdStart++;
            _unnamedParameters++;
          }
          _paramNames[x] = s;
          _paramValues[x] = null;
        }
      }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    #region IDisposable Members
    /// <summary>
    /// Disposes and finalizes the statement
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////////////////////////

    #region IDisposable "Pattern" Members
    private bool disposed;
    private void CheckDisposed() /* throw */
    {
#if THROW_ON_DISPOSED
        if (disposed)
            throw new ObjectDisposedException(typeof(SQLiteStatement).Name);
#endif
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    private void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                ////////////////////////////////////
                // dispose managed resources here...
                ////////////////////////////////////

                if (_sqlite_stmt != null)
                {
                    _sqlite_stmt.Dispose();
                    _sqlite_stmt = null;
                }

                _paramNames = null;
                _paramValues = null;
                _sql = null;
                _sqlStatement = null;
            }

            //////////////////////////////////////
            // release unmanaged resources here...
            //////////////////////////////////////

            disposed = true;
        }
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////////////////////////

    #region Destructor
    ~SQLiteStatement()
    {
        Dispose(false);
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// If the underlying database connection is open, fetches the number of changed rows
    /// resulting from the most recent query; otherwise, does nothing.
    /// </summary>
    /// <param name="changes">
    /// The number of changes when true is returned.
    /// Undefined if false is returned.
    /// </param>
    /// <param name="readOnly">
    /// The read-only flag when true is returned.
    /// Undefined if false is returned.
    /// </param>
    /// <returns>Non-zero if the number of changed rows was fetched.</returns>
    internal bool TryGetChanges(
        ref int changes,
        ref bool readOnly
        )
    {
        if ((_sql != null) && _sql.IsOpen())
        {
            changes = _sql.Changes;
            readOnly = _sql.IsReadOnly(this);

            return true;
        }

        return false;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Called by SQLiteParameterCollection, this function determines if the specified parameter name belongs to
    /// this statement, and if so, keeps a reference to the parameter so it can be bound later.
    /// </summary>
    /// <param name="s">The parameter name to map</param>
    /// <param name="p">The parameter to assign it</param>
    internal bool MapParameter(string s, SQLiteParameter p)
    {
      if (_paramNames == null) return false;

      int startAt = 0;
      if (s.Length > 0)
      {
        if (":$@;".IndexOf(s[0]) == -1)
          startAt = 1;
      }

      int x = _paramNames.Length;
      for (int n = 0; n < x; n++)
      {
        if (String.Compare(_paramNames[n], startAt, s, 0, Math.Max(_paramNames[n].Length - startAt, s.Length), StringComparison.OrdinalIgnoreCase) == 0)
        {
          _paramValues[n] = p;
          return true;
        }
      }
      return false;
    }

    /// <summary>
    ///  Bind all parameters, making sure the caller didn't miss any
    /// </summary>
    internal void BindParameters()
    {
      if (_paramNames == null) return;

      int x = _paramNames.Length;
      for (int n = 0; n < x; n++)
      {
        BindParameter(n + 1, _paramValues[n]);
      }
    }

    /// <summary>
    /// This method attempts to query the database connection associated with
    /// the statement in use.  If the underlying command or connection is
    /// unavailable, a null value will be returned.
    /// </summary>
    /// <returns>
    /// The connection object -OR- null if it is unavailable.
    /// </returns>
    private static SQLiteConnection GetConnection(
        SQLiteStatement statement
        )
    {
        try
        {
            if (statement != null)
            {
                SQLiteCommand command = statement._command;

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
    /// Invokes the parameter binding callback configured for the database
    /// type name associated with the specified column.  If no parameter
    /// binding callback is available for the database type name, do
    /// nothing.
    /// </summary>
    /// <param name="index">
    /// The index of the column being read.
    /// </param>
    /// <param name="parameter">
    /// The <see cref="SQLiteParameter" /> instance being bound to the
    /// command.
    /// </param>
    /// <param name="complete">
    /// Non-zero if the default handling for the parameter binding call
    /// should be skipped (i.e. the parameter should not be bound at all).
    /// Great care should be used when setting this to non-zero.
    /// </param>
    private void InvokeBindValueCallback(
        int index,
        SQLiteParameter parameter,
        out bool complete
        )
    {
        complete = false;
        SQLiteConnectionFlags oldFlags = _flags;
        _flags &= ~SQLiteConnectionFlags.UseConnectionBindValueCallbacks;

        try
        {
            if (parameter == null)
                return;

            SQLiteConnection connection = GetConnection(this);

            if (connection == null)
                return;

            //
            // NOTE: First, always look for an explicitly set database type
            //       name.
            //
            string typeName = parameter.TypeName;

            if (typeName == null)
            {
                //
                // NOTE: Are we allowed to fallback to using the parameter name
                //       as the basis for looking up the binding callback?
                //
                if (HelperMethods.HasFlags(
                        _flags, SQLiteConnectionFlags.UseParameterNameForTypeName))
                {
                    typeName = parameter.ParameterName;
                }
            }

            if (typeName == null)
            {
                //
                // NOTE: Are we allowed to fallback to using the database type
                //       name translated from the DbType as the basis for looking
                //       up the binding callback?
                //
                if (HelperMethods.HasFlags(
                        _flags, SQLiteConnectionFlags.UseParameterDbTypeForTypeName))
                {
                    typeName = SQLiteConvert.DbTypeToTypeName(
                        connection, parameter.DbType, _flags);
                }
            }

            if (typeName == null)
                return;

            SQLiteTypeCallbacks callbacks;

            if (!connection.TryGetTypeCallbacks(typeName, out callbacks) ||
                (callbacks == null))
            {
                return;
            }

            SQLiteBindValueCallback callback = callbacks.BindValueCallback;

            if (callback == null)
                return;

            object userData = callbacks.BindValueUserData;

            callback(
                _sql, _command, oldFlags, parameter, typeName, index,
                userData, out complete); /* throw */
        }
        finally
        {
            _flags |= SQLiteConnectionFlags.UseConnectionBindValueCallbacks;
        }
    }

    /// <summary>
    /// Perform the bind operation for an individual parameter
    /// </summary>
    /// <param name="index">The index of the parameter to bind</param>
    /// <param name="param">The parameter we're binding</param>
    private void BindParameter(int index, SQLiteParameter param)
    {
      if (param == null)
        throw new SQLiteException("Insufficient parameters supplied to the command");

      if (HelperMethods.HasFlags(
            _flags, SQLiteConnectionFlags.UseConnectionBindValueCallbacks))
      {
          bool complete;

          InvokeBindValueCallback(index, param, out complete);

          if (complete)
              return;
      }

      object obj = param.Value;
      DbType objType = param.DbType;

      if ((obj != null) && (objType == DbType.Object))
          objType = SQLiteConvert.TypeToDbType(obj.GetType());

      if (SQLite3.ForceLogPrepare() || HelperMethods.LogPreBind(_flags))
      {
          IntPtr handle = _sqlite_stmt;

          SQLiteLog.LogMessage(HelperMethods.StringFormat(
              CultureInfo.CurrentCulture,
              "Binding statement {0} paramter #{1} with database type {2} and raw value {{{3}}}...",
              handle, index, objType, obj));
      }

      if ((obj == null) || Convert.IsDBNull(obj))
      {
          _sql.Bind_Null(this, _flags, index);
        return;
      }

      CultureInfo invariantCultureInfo = CultureInfo.InvariantCulture;

      bool invariantText = HelperMethods.HasFlags(
          _flags, SQLiteConnectionFlags.BindInvariantText);

      CultureInfo cultureInfo = CultureInfo.CurrentCulture;

      if (HelperMethods.HasFlags(
            _flags, SQLiteConnectionFlags.ConvertInvariantText))
      {
          cultureInfo = invariantCultureInfo;
      }

      if (HelperMethods.HasFlags(
            _flags, SQLiteConnectionFlags.BindAllAsText))
      {
          if (obj is DateTime)
          {
              _sql.Bind_DateTime(this, _flags, index, (DateTime)obj);
          }
          else
          {
              _sql.Bind_Text(this, _flags, index, invariantText ?
                  SQLiteConvert.ToStringWithProvider(obj, invariantCultureInfo) :
                  SQLiteConvert.ToStringWithProvider(obj, cultureInfo));
          }

          return;
      }

      bool invariantDecimal = HelperMethods.HasFlags(
          _flags, SQLiteConnectionFlags.BindInvariantDecimal);

      if (HelperMethods.HasFlags(
            _flags, SQLiteConnectionFlags.BindDecimalAsText))
      {
          if (obj is Decimal)
          {
              _sql.Bind_Text(this, _flags, index, invariantText || invariantDecimal ?
                  SQLiteConvert.ToStringWithProvider(obj, invariantCultureInfo) :
                  SQLiteConvert.ToStringWithProvider(obj, cultureInfo));

              return;
          }
      }

      switch (objType)
      {
        case DbType.Date:
        case DbType.Time:
        case DbType.DateTime:
          //
          // NOTE: The old method (commented below) does not honor the selected date format
          //       for the connection.
          // _sql.Bind_DateTime(this, index, Convert.ToDateTime(obj, cultureInfo));
            _sql.Bind_DateTime(this, _flags, index, (obj is string) ?
              _sql.ToDateTime((string)obj) : Convert.ToDateTime(obj, cultureInfo));
          break;
        case DbType.Boolean:
          _sql.Bind_Boolean(this, _flags, index, SQLiteConvert.ToBoolean(obj, cultureInfo, true));
          break;
        case DbType.SByte:
          _sql.Bind_Int32(this, _flags, index, Convert.ToSByte(obj, cultureInfo));
          break;
        case DbType.Int16:
          _sql.Bind_Int32(this, _flags, index, Convert.ToInt16(obj, cultureInfo));
          break;
        case DbType.Int32:
          _sql.Bind_Int32(this, _flags, index, Convert.ToInt32(obj, cultureInfo));
          break;
        case DbType.Int64:
          _sql.Bind_Int64(this, _flags, index, Convert.ToInt64(obj, cultureInfo));
          break;
        case DbType.Byte:
          _sql.Bind_UInt32(this, _flags, index, Convert.ToByte(obj, cultureInfo));
          break;
        case DbType.UInt16:
          _sql.Bind_UInt32(this, _flags, index, Convert.ToUInt16(obj, cultureInfo));
          break;
        case DbType.UInt32:
          _sql.Bind_UInt32(this, _flags, index, Convert.ToUInt32(obj, cultureInfo));
          break;
        case DbType.UInt64:
          _sql.Bind_UInt64(this, _flags, index, Convert.ToUInt64(obj, cultureInfo));
          break;
        case DbType.Single:
        case DbType.Double:
        case DbType.Currency:
          _sql.Bind_Double(this, _flags, index, Convert.ToDouble(obj, cultureInfo));
          break;
        case DbType.Binary:
          _sql.Bind_Blob(this, _flags, index, (byte[])obj);
          break;
        case DbType.Guid:
          if (_command.Connection._binaryGuid == true)
          {
            _sql.Bind_Blob(this, _flags, index, ((Guid)obj).ToByteArray());
          }
          else
          {
            _sql.Bind_Text(this, _flags, index, invariantText ?
              SQLiteConvert.ToStringWithProvider(obj, invariantCultureInfo) :
              SQLiteConvert.ToStringWithProvider(obj, cultureInfo));
          }
          break;
        case DbType.Decimal: // Dont store decimal as double ... loses precision
          _sql.Bind_Text(this, _flags, index, invariantText || invariantDecimal ?
            SQLiteConvert.ToStringWithProvider(Convert.ToDecimal(obj, cultureInfo), invariantCultureInfo) :
            SQLiteConvert.ToStringWithProvider(Convert.ToDecimal(obj, cultureInfo), cultureInfo));
          break;
        default:
          _sql.Bind_Text(this, _flags, index, invariantText ?
              SQLiteConvert.ToStringWithProvider(obj, invariantCultureInfo) :
              SQLiteConvert.ToStringWithProvider(obj, cultureInfo));
          break;
      }
    }

    internal string[] TypeDefinitions
    {
      get { return _types; }
    }

    internal void SetTypes(string typedefs)
    {
      int pos = typedefs.IndexOf("TYPES", 0, StringComparison.OrdinalIgnoreCase);
      if (pos == -1) throw new ArgumentOutOfRangeException();

      string[] types = typedefs.Substring(pos + 6).Replace(" ", String.Empty).Replace(";", String.Empty).Replace("\"", String.Empty).Replace("[", String.Empty).Replace("]", String.Empty).Replace("`", String.Empty).Split(',', '\r', '\n', '\t');

      int n;
      for (n = 0; n < types.Length; n++)
      {
        if (String.IsNullOrEmpty(types[n]) == true)
          types[n] = null;
      }
      _types = types;
    }
  }
}
