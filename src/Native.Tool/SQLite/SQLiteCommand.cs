/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 *
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
  using System;
  using System.Data;
  using System.Data.Common;
  using System.Diagnostics;
  using System.Collections.Generic;
  using System.ComponentModel;

  /// <summary>
  /// SQLite implementation of DbCommand.
  /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
  [Designer("SQLite.Designer.SQLiteCommandDesigner, SQLite.Designer, Version=" + SQLite3.DesignerVersion + ", Culture=neutral, PublicKeyToken=db937bc2d44ff139"), ToolboxItem(true)]
#endif
  public sealed class SQLiteCommand : DbCommand, ICloneable
  {
    /// <summary>
    /// The default connection string to be used when creating a temporary
    /// connection to execute a command via the static
    /// <see cref="Execute(string,SQLiteExecuteType,string,object[])" /> or
    /// <see cref="Execute(string,SQLiteExecuteType,CommandBehavior,string,object[])" />
    /// methods.
    /// </summary>
    private static readonly string DefaultConnectionString = "Data Source=:memory:;";

    /// <summary>
    /// The command text this command is based on
    /// </summary>
    private string _commandText;
    /// <summary>
    /// The connection the command is associated with
    /// </summary>
    private SQLiteConnection _cnn;
    /// <summary>
    /// The version of the connection the command is associated with
    /// </summary>
    private int _version;
    /// <summary>
    /// Indicates whether or not a DataReader is active on the command.
    /// </summary>
    private WeakReference _activeReader;
    /// <summary>
    /// The timeout for the command, kludged because SQLite doesn't support per-command timeout values
    /// </summary>
    internal int _commandTimeout;
    /// <summary>
    /// Designer support
    /// </summary>
    private bool _designTimeVisible;
    /// <summary>
    /// Used by DbDataAdapter to determine updating behavior
    /// </summary>
    private UpdateRowSource _updateRowSource;
    /// <summary>
    /// The collection of parameters for the command
    /// </summary>
    private SQLiteParameterCollection _parameterCollection;
    /// <summary>
    /// The SQL command text, broken into individual SQL statements as they are executed
    /// </summary>
    internal List<SQLiteStatement> _statementList;
    /// <summary>
    /// Unprocessed SQL text that has not been executed
    /// </summary>
    internal string _remainingText;
    /// <summary>
    /// Transaction associated with this command
    /// </summary>
    private SQLiteTransaction _transaction;

    ///<overloads>
    /// Constructs a new SQLiteCommand
    /// </overloads>
    /// <summary>
    /// Default constructor
    /// </summary>
    public SQLiteCommand() :this(null, null)
    {
    }

    /// <summary>
    /// Initializes the command with the given command text
    /// </summary>
    /// <param name="commandText">The SQL command text</param>
    public SQLiteCommand(string commandText)
      : this(commandText, null, null)
    {
    }

    /// <summary>
    /// Initializes the command with the given SQL command text and attach the command to the specified
    /// connection.
    /// </summary>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="connection">The connection to associate with the command</param>
    public SQLiteCommand(string commandText, SQLiteConnection connection)
      : this(commandText, connection, null)
    {
    }

    /// <summary>
    /// Initializes the command and associates it with the specified connection.
    /// </summary>
    /// <param name="connection">The connection to associate with the command</param>
    public SQLiteCommand(SQLiteConnection connection)
      : this(null, connection, null)
    {
    }

    private SQLiteCommand(SQLiteCommand source) : this(source.CommandText, source.Connection, source.Transaction)
    {
      CommandTimeout = source.CommandTimeout;
      DesignTimeVisible = source.DesignTimeVisible;
      UpdatedRowSource = source.UpdatedRowSource;

      foreach (SQLiteParameter param in source._parameterCollection)
      {
        Parameters.Add(param.Clone());
      }
    }

    /// <summary>
    /// Initializes a command with the given SQL, connection and transaction
    /// </summary>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="connection">The connection to associate with the command</param>
    /// <param name="transaction">The transaction the command should be associated with</param>
    public SQLiteCommand(string commandText, SQLiteConnection connection, SQLiteTransaction transaction)
    {
      _commandTimeout = 30;
      _parameterCollection = new SQLiteParameterCollection(this);
      _designTimeVisible = true;
      _updateRowSource = UpdateRowSource.None;

      if (commandText != null)
        CommandText = commandText;

      if (connection != null)
      {
        DbConnection = connection;
        _commandTimeout = connection.DefaultTimeout;
      }

      if (transaction != null)
        Transaction = transaction;

      SQLiteConnection.OnChanged(connection, new ConnectionEventArgs(
          SQLiteConnectionEventType.NewCommand, null, transaction, this,
          null, null, null, null));
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    [Conditional("CHECK_STATE")]
    internal static void Check(SQLiteCommand command)
    {
        if (command == null)
            throw new ArgumentNullException("command");

        command.CheckDisposed();
        SQLiteConnection.Check(command._cnn);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    #region IDisposable "Pattern" Members
    private bool disposed;
    private void CheckDisposed() /* throw */
    {
#if THROW_ON_DISPOSED
        if (disposed)
            throw new ObjectDisposedException(typeof(SQLiteCommand).Name);
#endif
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Disposes of the command and clears all member variables
    /// </summary>
    /// <param name="disposing">Whether or not the class is being explicitly or implicitly disposed</param>
    protected override void Dispose(bool disposing)
    {
        SQLiteConnection.OnChanged(_cnn, new ConnectionEventArgs(
            SQLiteConnectionEventType.DisposingCommand, null, _transaction, this,
            null, null, null, new object[] { disposing, disposed }));

        bool skippedDispose = false;

        try
        {
            if (!disposed)
            {
                if (disposing)
                {
                    ////////////////////////////////////
                    // dispose managed resources here...
                    ////////////////////////////////////

                    // If a reader is active on this command, don't destroy the command, instead let the reader do it
                    SQLiteDataReader reader = null;
                    if (_activeReader != null)
                    {
                        try
                        {
                            reader = _activeReader.Target as SQLiteDataReader;
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }

                    if (reader != null)
                    {
                        reader._disposeCommand = true;
                        _activeReader = null;
                        skippedDispose = true;
                        return;
                    }

                    Connection = null;
                    _parameterCollection.Clear();
                    _commandText = null;
                }

                //////////////////////////////////////
                // release unmanaged resources here...
                //////////////////////////////////////
            }
        }
        finally
        {
            if (!skippedDispose)
            {
                base.Dispose(disposing);

                //
                // NOTE: Everything should be fully disposed at this point.
                //
                disposed = true;
            }
        }
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This method attempts to query the flags associated with the database
    /// connection in use.  If the database connection is disposed, the default
    /// flags will be returned.
    /// </summary>
    /// <param name="command">
    /// The command containing the databse connection to query the flags from.
    /// </param>
    /// <returns>
    /// The connection flags value.
    /// </returns>
    internal static SQLiteConnectionFlags GetFlags(
        SQLiteCommand command
        )
    {
        try
        {
            if (command != null)
            {
                SQLiteConnection cnn = command._cnn;

                if (cnn != null)
                    return cnn.Flags;
            }
        }
        catch (ObjectDisposedException)
        {
            // do nothing.
        }

        return SQLiteConnectionFlags.Default;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    private void DisposeStatements()
    {
        if (_statementList == null) return;

        int x = _statementList.Count;

        for (int n = 0; n < x; n++)
        {
            SQLiteStatement stmt = _statementList[n];
            if (stmt == null) continue;
            stmt.Dispose();
        }

        _statementList = null;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    private void ClearDataReader()
    {
      if (_activeReader != null)
      {
        SQLiteDataReader reader = null;

        try
        {
          reader = _activeReader.Target as SQLiteDataReader;
        }
        catch(InvalidOperationException)
        {
          // do nothing.
        }

        if (reader != null)
          reader.Close(); /* Dispose */

        _activeReader = null;
      }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Clears and destroys all statements currently prepared
    /// </summary>
    internal void ClearCommands()
    {
      ClearDataReader();
      DisposeStatements();

      _parameterCollection.Unbind();
    }

    /// <summary>
    /// Builds an array of prepared statements for each complete SQL statement in the command text
    /// </summary>
    internal SQLiteStatement BuildNextCommand()
    {
      SQLiteStatement stmt = null;

      try
      {
        if ((_cnn != null) && (_cnn._sql != null))
        {
          if (_statementList == null)
            _remainingText = _commandText;

          stmt = _cnn._sql.Prepare(_cnn, _remainingText, (_statementList == null) ? null : _statementList[_statementList.Count - 1], (uint)(_commandTimeout * 1000), ref _remainingText);

          if (stmt != null)
          {
            stmt._command = this;

            if (_statementList == null)
              _statementList = new List<SQLiteStatement>();

            _statementList.Add(stmt);

            _parameterCollection.MapParameters(stmt);
            stmt.BindParameters();
          }
        }
        return stmt;
      }
      catch (Exception)
      {
        if (stmt != null)
        {
          if ((_statementList != null) && _statementList.Contains(stmt))
            _statementList.Remove(stmt);

          stmt.Dispose();
        }

        // If we threw an error compiling the statement, we cannot continue on so set the remaining text to null.
        _remainingText = null;

        throw;
      }
    }

    internal SQLiteStatement GetStatement(int index)
    {
      // Haven't built any statements yet
      if (_statementList == null) return BuildNextCommand();

      // If we're at the last built statement and want the next unbuilt statement, then build it
      if (index == _statementList.Count)
      {
        if (String.IsNullOrEmpty(_remainingText) == false) return BuildNextCommand();
        else return null; // No more commands
      }

      SQLiteStatement stmt = _statementList[index];
      stmt.BindParameters();

      return stmt;
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public override void Cancel()
    {
      CheckDisposed();

      if (_activeReader != null)
      {
        SQLiteDataReader reader = _activeReader.Target as SQLiteDataReader;
        if (reader != null)
          reader.Cancel();
      }
    }

    /// <summary>
    /// The SQL command text associated with the command
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [DefaultValue(""), RefreshProperties(RefreshProperties.All), Editor("Microsoft.VSDesigner.Data.SQL.Design.SqlCommandTextEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
#endif
    public override string CommandText
    {
      get
      {
        CheckDisposed();

        return _commandText;
      }
      set
      {
        CheckDisposed();

        if (_commandText == value) return;

        if (_activeReader != null && _activeReader.IsAlive)
        {
          throw new InvalidOperationException("Cannot set CommandText while a DataReader is active");
        }

        ClearCommands();
        _commandText = value;

        if (_cnn == null) return;
      }
    }

    /// <summary>
    /// The amount of time to wait for the connection to become available before erroring out
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [DefaultValue((int)30)]
#endif
    public override int CommandTimeout
    {
      get
      {
        CheckDisposed();
        return _commandTimeout;
      }
      set
      {
        CheckDisposed();
        _commandTimeout = value;
      }
    }

    /// <summary>
    /// The type of the command.  SQLite only supports CommandType.Text
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [RefreshProperties(RefreshProperties.All), DefaultValue(CommandType.Text)]
#endif
    public override CommandType CommandType
    {
      get
      {
        CheckDisposed();
        return CommandType.Text;
      }
      set
      {
        CheckDisposed();

        if (value != CommandType.Text)
        {
          throw new NotSupportedException();
        }
      }
    }

    /// <summary>
    /// Forwards to the local CreateParameter() function
    /// </summary>
    /// <returns></returns>
    protected override DbParameter CreateDbParameter()
    {
      return CreateParameter();
    }

    /// <summary>
    /// Create a new parameter
    /// </summary>
    /// <returns></returns>
    public new SQLiteParameter CreateParameter()
    {
      CheckDisposed();
      return new SQLiteParameter(this);
    }

    /// <summary>
    /// The connection associated with this command
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [DefaultValue((string)null), Editor("Microsoft.VSDesigner.Data.Design.DbConnectionEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
#endif
    public new SQLiteConnection Connection
    {
      get { CheckDisposed(); return _cnn; }
      set
      {
        CheckDisposed();

        if (_activeReader != null && _activeReader.IsAlive)
          throw new InvalidOperationException("Cannot set Connection while a DataReader is active");

        if (_cnn != null)
        {
          ClearCommands();
          //_cnn.RemoveCommand(this);
        }

        _cnn = value;
        if (_cnn != null)
          _version = _cnn._version;

        //if (_cnn != null)
        //  _cnn.AddCommand(this);
      }
    }

    /// <summary>
    /// Forwards to the local Connection property
    /// </summary>
    protected override DbConnection DbConnection
    {
      get
      {
        return Connection;
      }
      set
      {
        Connection = (SQLiteConnection)value;
      }
    }

    /// <summary>
    /// Returns the SQLiteParameterCollection for the given command
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
#endif
    public new SQLiteParameterCollection Parameters
    {
      get { CheckDisposed(); return _parameterCollection; }
    }

    /// <summary>
    /// Forwards to the local Parameters property
    /// </summary>
    protected override DbParameterCollection DbParameterCollection
    {
      get
      {
        return Parameters;
      }
    }

    /// <summary>
    /// The transaction associated with this command.  SQLite only supports one transaction per connection, so this property forwards to the
    /// command's underlying connection.
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
    public new SQLiteTransaction Transaction
    {
      get { CheckDisposed(); return _transaction; }
      set
      {
        CheckDisposed();

        if (_cnn != null)
        {
          if (_activeReader != null && _activeReader.IsAlive)
            throw new InvalidOperationException("Cannot set Transaction while a DataReader is active");

          if (value != null)
          {
            if (value._cnn != _cnn)
              throw new ArgumentException("Transaction is not associated with the command's connection");
          }
          _transaction = value;
        }
        else
        {
          if (value != null) Connection = value.Connection;
          _transaction = value;
        }
      }
    }

    /// <summary>
    /// Forwards to the local Transaction property
    /// </summary>
    protected override DbTransaction DbTransaction
    {
      get
      {
        return Transaction;
      }
      set
      {
        Transaction = (SQLiteTransaction)value;
      }
    }

    /// <summary>
    /// Verifies that all SQL queries associated with the current command text
    /// can be successfully compiled.  A <see cref="SQLiteException" /> will be
    /// raised if any errors occur.
    /// </summary>
    public void VerifyOnly()
    {
        CheckDisposed();

        SQLiteConnection connection = _cnn;
        SQLiteConnection.Check(connection); /* throw */
        SQLiteBase sqlBase = connection._sql;

        if ((connection == null) || (sqlBase == null))
            throw new SQLiteException("invalid or unusable connection");

        List<SQLiteStatement> statements = null;
        SQLiteStatement currentStatement = null;

        try
        {
            string text = _commandText;
            uint timeout = (uint)(_commandTimeout * 1000);
            SQLiteStatement previousStatement = null;

            while ((text != null) && (text.Length > 0))
            {
                currentStatement = sqlBase.Prepare(
                    connection, text, previousStatement, timeout,
                    ref text); /* throw */

                previousStatement = currentStatement;

                if (currentStatement != null)
                {
                    if (statements == null)
                        statements = new List<SQLiteStatement>();

                    statements.Add(currentStatement);
                    currentStatement = null;
                }

                if (text == null) continue;
                text = text.Trim();
            }
        }
        finally
        {
            if (currentStatement != null)
            {
                currentStatement.Dispose();
                currentStatement = null;
            }

            if (statements != null)
            {
                foreach (SQLiteStatement statement in statements)
                {
                    if (statement == null)
                        continue;

                    statement.Dispose();
                }

                statements.Clear();
                statements = null;
            }
        }
    }

    /// <summary>
    /// This function ensures there are no active readers, that we have a valid connection,
    /// that the connection is open, that all statements are prepared and all parameters are assigned
    /// in preparation for allocating a data reader.
    /// </summary>
    private void InitializeForReader()
    {
      if (_activeReader != null && _activeReader.IsAlive)
        throw new InvalidOperationException("DataReader already active on this command");

      if (_cnn == null)
        throw new InvalidOperationException("No connection associated with this command");

      if (_cnn.State != ConnectionState.Open)
        throw new InvalidOperationException("Database is not open");

      // If the version of the connection has changed, clear out any previous commands before starting
      if (_cnn._version != _version)
      {
        _version = _cnn._version;
        ClearCommands();
      }

      // Map all parameters for statements already built
      _parameterCollection.MapParameters(null);

      //// Set the default command timeout
      //_cnn._sql.SetTimeout(_commandTimeout * 1000);
    }

    /// <summary>
    /// Creates a new SQLiteDataReader to execute/iterate the array of SQLite prepared statements
    /// </summary>
    /// <param name="behavior">The behavior the data reader should adopt</param>
    /// <returns>Returns a SQLiteDataReader object</returns>
    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
      return ExecuteReader(behavior);
    }

    /// <summary>
    /// This method creates a new connection, executes the query using the given
    /// execution type, closes the connection, and returns the results.  If the
    /// connection string is null, a temporary in-memory database connection will
    /// be used.
    /// </summary>
    /// <param name="commandText">
    /// The text of the command to be executed.
    /// </param>
    /// <param name="executeType">
    /// The execution type for the command.  This is used to determine which method
    /// of the command object to call, which then determines the type of results
    /// returned, if any.
    /// </param>
    /// <param name="connectionString">
    /// The connection string to the database to be opened, used, and closed.  If
    /// this parameter is null, a temporary in-memory databse will be used.
    /// </param>
    /// <param name="args">
    /// The SQL parameter values to be used when building the command object to be
    /// executed, if any.
    /// </param>
    /// <returns>
    /// The results of the query -OR- null if no results were produced from the
    /// given execution type.
    /// </returns>
    public static object Execute(
        string commandText,
        SQLiteExecuteType executeType,
        string connectionString,
        params object[] args
        )
    {
        return Execute(
            commandText, executeType, CommandBehavior.Default,
            connectionString, args);
    }

    /// <summary>
    /// This method creates a new connection, executes the query using the given
    /// execution type and command behavior, closes the connection unless a data
    /// reader is created, and returns the results.  If the connection string is
    /// null, a temporary in-memory database connection will be used.
    /// </summary>
    /// <param name="commandText">
    /// The text of the command to be executed.
    /// </param>
    /// <param name="executeType">
    /// The execution type for the command.  This is used to determine which method
    /// of the command object to call, which then determines the type of results
    /// returned, if any.
    /// </param>
    /// <param name="commandBehavior">
    /// The command behavior flags for the command.
    /// </param>
    /// <param name="connectionString">
    /// The connection string to the database to be opened, used, and closed.  If
    /// this parameter is null, a temporary in-memory databse will be used.
    /// </param>
    /// <param name="args">
    /// The SQL parameter values to be used when building the command object to be
    /// executed, if any.
    /// </param>
    /// <returns>
    /// The results of the query -OR- null if no results were produced from the
    /// given execution type.
    /// </returns>
    public static object Execute(
        string commandText,
        SQLiteExecuteType executeType,
        CommandBehavior commandBehavior,
        string connectionString,
        params object[] args
        )
    {
        SQLiteConnection connection = null;

        try
        {
            if (connectionString == null)
                connectionString = DefaultConnectionString;

            using (connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = commandText;

                    if (args != null)
                    {
                        foreach (object arg in args)
                        {
                            SQLiteParameter parameter = arg as SQLiteParameter;

                            if (parameter == null)
                            {
                                parameter = command.CreateParameter();
                                parameter.DbType = DbType.Object;
                                parameter.Value = arg;
                            }

                            command.Parameters.Add(parameter);
                        }
                    }

                    switch (executeType)
                    {
                        case SQLiteExecuteType.None:
                            {
                                //
                                // NOTE: Do nothing.
                                //
                                break;
                            }
                        case SQLiteExecuteType.NonQuery:
                            {
                                return command.ExecuteNonQuery(commandBehavior);
                            }
                        case SQLiteExecuteType.Scalar:
                            {
                                return command.ExecuteScalar(commandBehavior);
                            }
                        case SQLiteExecuteType.Reader:
                            {
                                bool success = true;

                                try
                                {
                                    //
                                    // NOTE: The CloseConnection flag is being added here.
                                    //       This should force the returned data reader to
                                    //       close the connection when it is disposed.  In
                                    //       order to prevent the containing using block
                                    //       from disposing the connection prematurely,
                                    //       the innermost finally block sets the internal
                                    //       no-disposal flag to true.  The outer finally
                                    //       block will reset the internal no-disposal flag
                                    //       to false so that the data reader will be able
                                    //       to (eventually) dispose of the connection.
                                    //
                                    return command.ExecuteReader(
                                        commandBehavior | CommandBehavior.CloseConnection);
                                }
                                catch
                                {
                                    success = false;
                                    throw;
                                }
                                finally
                                {
                                    //
                                    // NOTE: If an exception was not thrown, that can only
                                    //       mean the data reader was successfully created
                                    //       and now owns the connection.  Therefore, set
                                    //       the internal no-disposal flag (temporarily)
                                    //       in order to exit the containing using block
                                    //       without disposing it.
                                    //
                                    if (success)
                                        connection._noDispose = true;
                                }
                            }
                    }
                }
            }
        }
        finally
        {
            //
            // NOTE: Now that the using block has been exited, reset the
            //       internal disposal flag for the connection.  This is
            //       always done if the connection was created because
            //       it will be harmless whether or not the data reader
            //       now owns it.
            //
            if (connection != null)
                connection._noDispose = false;
        }

        return null;
    }

    /// <summary>
    /// Overrides the default behavior to return a SQLiteDataReader specialization class
    /// </summary>
    /// <param name="behavior">The flags to be associated with the reader.</param>
    /// <returns>A SQLiteDataReader</returns>
    public new SQLiteDataReader ExecuteReader(CommandBehavior behavior)
    {
      CheckDisposed();
      SQLiteConnection.Check(_cnn);
      InitializeForReader();

      SQLiteDataReader rd = new SQLiteDataReader(this, behavior);
      _activeReader = new WeakReference(rd, false);

      return rd;
    }

    /// <summary>
    /// Overrides the default behavior of DbDataReader to return a specialized SQLiteDataReader class
    /// </summary>
    /// <returns>A SQLiteDataReader</returns>
    public new SQLiteDataReader ExecuteReader()
    {
      CheckDisposed();
      SQLiteConnection.Check(_cnn);
      return ExecuteReader(CommandBehavior.Default);
    }

    /// <summary>
    /// Called by the SQLiteDataReader when the data reader is closed.
    /// </summary>
    internal void ResetDataReader()
    {
      _activeReader = null;
    }

    /// <summary>
    /// Execute the command and return the number of rows inserted/updated affected by it.
    /// </summary>
    /// <returns>The number of rows inserted/updated affected by it.</returns>
    public override int ExecuteNonQuery()
    {
        CheckDisposed();
        SQLiteConnection.Check(_cnn);
        return ExecuteNonQuery(CommandBehavior.Default);
    }

    /// <summary>
    /// Execute the command and return the number of rows inserted/updated affected by it.
    /// </summary>
    /// <param name="behavior">The flags to be associated with the reader.</param>
    /// <returns>The number of rows inserted/updated affected by it.</returns>
    public int ExecuteNonQuery(
        CommandBehavior behavior
        )
    {
      CheckDisposed();
      SQLiteConnection.Check(_cnn);

      using (SQLiteDataReader reader = ExecuteReader(behavior |
          CommandBehavior.SingleRow | CommandBehavior.SingleResult))
      {
        while (reader.NextResult()) ;
        return reader.RecordsAffected;
      }
    }

    /// <summary>
    /// Execute the command and return the first column of the first row of the resultset
    /// (if present), or null if no resultset was returned.
    /// </summary>
    /// <returns>The first column of the first row of the first resultset from the query.</returns>
    public override object ExecuteScalar()
    {
      CheckDisposed();
      SQLiteConnection.Check(_cnn);
      return ExecuteScalar(CommandBehavior.Default);
    }

    /// <summary>
    /// Execute the command and return the first column of the first row of the resultset
    /// (if present), or null if no resultset was returned.
    /// </summary>
    /// <param name="behavior">The flags to be associated with the reader.</param>
    /// <returns>The first column of the first row of the first resultset from the query.</returns>
    public object ExecuteScalar(
        CommandBehavior behavior
        )
    {
      CheckDisposed();
      SQLiteConnection.Check(_cnn);

      using (SQLiteDataReader reader = ExecuteReader(behavior |
          CommandBehavior.SingleRow | CommandBehavior.SingleResult))
      {
        if (reader.Read() && (reader.FieldCount > 0))
          return reader[0];
      }
      return null;
    }

    /// <summary>
    /// This method resets all the prepared statements held by this instance
    /// back to their initial states, ready to be re-executed.
    /// </summary>
    public void Reset()
    {
        CheckDisposed();
        SQLiteConnection.Check(_cnn);

        Reset(true, false);
    }

    /// <summary>
    /// This method resets all the prepared statements held by this instance
    /// back to their initial states, ready to be re-executed.
    /// </summary>
    /// <param name="clearBindings">
    /// Non-zero if the parameter bindings should be cleared as well.
    /// </param>
    /// <param name="ignoreErrors">
    /// If this is zero, a <see cref="SQLiteException" /> may be thrown for
    /// any unsuccessful return codes from the native library; otherwise, a
    /// <see cref="SQLiteException" /> will only be thrown if the connection
    /// or its state is invalid.
    /// </param>
    public void Reset(
        bool clearBindings,
        bool ignoreErrors
        )
    {
        CheckDisposed();
        SQLiteConnection.Check(_cnn);

        if (clearBindings && (_parameterCollection != null))
            _parameterCollection.Unbind();

        ClearDataReader();

        if (_statementList == null)
            return;

        SQLiteBase sqlBase = _cnn._sql;
        SQLiteErrorCode rc;

        foreach (SQLiteStatement item in _statementList)
        {
            if (item == null)
                continue;

            SQLiteStatementHandle stmt = item._sqlite_stmt;

            if (stmt == null)
                continue;

            rc = sqlBase.Reset(item);

            if ((rc == SQLiteErrorCode.Ok) && clearBindings &&
                (SQLite3.SQLiteVersionNumber >= 3003007))
            {
                rc = UnsafeNativeMethods.sqlite3_clear_bindings(stmt);
            }

            if (!ignoreErrors && (rc != SQLiteErrorCode.Ok))
                throw new SQLiteException(rc, sqlBase.GetLastError());
        }
    }

    /// <summary>
    /// Does nothing.  Commands are prepared as they are executed the first time, and kept in prepared state afterwards.
    /// </summary>
    public override void Prepare()
    {
      CheckDisposed();
      SQLiteConnection.Check(_cnn);
    }

    /// <summary>
    /// Sets the method the SQLiteCommandBuilder uses to determine how to update inserted or updated rows in a DataTable.
    /// </summary>
    [DefaultValue(UpdateRowSource.None)]
    public override UpdateRowSource UpdatedRowSource
    {
      get
      {
        CheckDisposed();
        return _updateRowSource;
      }
      set
      {
        CheckDisposed();
        _updateRowSource = value;
      }
    }

    /// <summary>
    /// Determines if the command is visible at design time.  Defaults to True.
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [DesignOnly(true), Browsable(false), DefaultValue(true), EditorBrowsable(EditorBrowsableState.Never)]
#endif
    public override bool DesignTimeVisible
    {
      get
      {
        CheckDisposed();
        return _designTimeVisible;
      }
      set
      {
        CheckDisposed();

        _designTimeVisible = value;
#if !PLATFORM_COMPACTFRAMEWORK
        TypeDescriptor.Refresh(this);
#endif
      }
    }

    /// <summary>
    /// Clones a command, including all its parameters
    /// </summary>
    /// <returns>A new SQLiteCommand with the same commandtext, connection and parameters</returns>
    public object Clone()
    {
      CheckDisposed();
      return new SQLiteCommand(this);
    }
  }
}