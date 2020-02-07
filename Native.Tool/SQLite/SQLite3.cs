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

#if !NET_COMPACT_20 && (TRACE_CONNECTION || TRACE_STATEMENT)
  using System.Diagnostics;
#endif

  using System.Globalization;
  using System.Runtime.InteropServices;
  using System.Text;
  using System.Threading;

  /// <summary>
  /// This is the method signature for the SQLite core library logging callback
  /// function for use with sqlite3_log() and the SQLITE_CONFIG_LOG.
  ///
  /// WARNING: This delegate is used more-or-less directly by native code, do
  ///          not modify its type signature.
  /// </summary>
  /// <param name="pUserData">
  /// The extra data associated with this message, if any.
  /// </param>
  /// <param name="errorCode">
  /// The error code associated with this message.
  /// </param>
  /// <param name="pMessage">
  /// The message string to be logged.
  /// </param>
#if !PLATFORM_COMPACTFRAMEWORK
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
  internal delegate void SQLiteLogCallback(IntPtr pUserData, int errorCode, IntPtr pMessage);

  /// <summary>
  /// This class implements SQLiteBase completely, and is the guts of the code that interop's SQLite with .NET
  /// </summary>
  internal class SQLite3 : SQLiteBase
  {
    private static object syncRoot = new object();

    /// <summary>
    /// This field is used to refer to memory allocated for the
    /// SQLITE_DBCONFIG_MAINDBNAME value used with the native
    /// "sqlite3_db_config" API.  If allocated, the associated
    /// memeory will be freed when the underlying connection is
    /// closed.
    /// </summary>
    private IntPtr dbName = IntPtr.Zero;

    //
    // NOTE: This is the public key for the System.Data.SQLite assembly.  If you change the
    //       SNK file, you will need to change this as well.
    //
    internal const string PublicKey =
        "002400000480000094000000060200000024000052534131000400000100010005a288de5687c4e1" +
        "b621ddff5d844727418956997f475eb829429e411aff3e93f97b70de698b972640925bdd44280df0" +
        "a25a843266973704137cbb0e7441c1fe7cae4e2440ae91ab8cde3933febcb1ac48dd33b40e13c421" +
        "d8215c18a4349a436dd499e3c385cc683015f886f6c10bd90115eb2bd61b67750839e3a19941dc9c";

#if !PLATFORM_COMPACTFRAMEWORK
    internal const string DesignerVersion = "1.0.112.0";
#endif

    /// <summary>
    /// The opaque pointer returned to us by the sqlite provider
    /// </summary>
    protected internal SQLiteConnectionHandle _sql;
    protected string _fileName;
    protected SQLiteConnectionFlags _flags;
    private bool _setLogCallback;
    protected bool _usePool;
    protected int _poolVersion;
    private int _cancelCount;

#if (NET_35 || NET_40 || NET_45 || NET_451 || NET_452 || NET_46 || NET_461 || NET_462 || NET_47 || NET_471 || NET_472) && !PLATFORM_COMPACTFRAMEWORK
    private bool _buildingSchema;
#endif

    /// <summary>
    /// The user-defined functions registered on this connection
    /// </summary>
    protected Dictionary<SQLiteFunctionAttribute, SQLiteFunction> _functions;

#if INTEROP_VIRTUAL_TABLE
    /// <summary>
    /// This is the name of the native library file that contains the
    /// "vtshim" extension [wrapper].
    /// </summary>
    protected string _shimExtensionFileName = null;

    /// <summary>
    /// This is the flag indicate whether the native library file that
    /// contains the "vtshim" extension must be dynamically loaded by
    /// this class prior to use.
    /// </summary>
    protected bool? _shimIsLoadNeeded = null;

    /// <summary>
    /// This is the name of the native entry point for the "vtshim"
    /// extension [wrapper].
    /// </summary>
    protected string _shimExtensionProcName = "sqlite3_vtshim_init";

    /// <summary>
    /// The modules created using this connection.
    /// </summary>
    protected Dictionary<string, SQLiteModule> _modules;
#endif

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Constructs the object used to interact with the SQLite core library
    /// using the UTF-8 text encoding.
    /// </summary>
    /// <param name="fmt">
    /// The DateTime format to be used when converting string values to a
    /// DateTime and binding DateTime parameters.
    /// </param>
    /// <param name="kind">
    /// The <see cref="DateTimeKind" /> to be used when creating DateTime
    /// values.
    /// </param>
    /// <param name="fmtString">
    /// The format string to be used when parsing and formatting DateTime
    /// values.
    /// </param>
    /// <param name="db">
    /// The native handle to be associated with the database connection.
    /// </param>
    /// <param name="fileName">
    /// The fully qualified file name associated with <paramref name="db "/>.
    /// </param>
    /// <param name="ownHandle">
    /// Non-zero if the newly created object instance will need to dispose
    /// of <paramref name="db" /> when it is no longer needed.
    /// </param>
    internal SQLite3(
        SQLiteDateFormats fmt,
        DateTimeKind kind,
        string fmtString,
        IntPtr db,
        string fileName,
        bool ownHandle
        )
      : base(fmt, kind, fmtString)
    {
        if (db != IntPtr.Zero)
        {
            _sql = new SQLiteConnectionHandle(db, ownHandle);
            _fileName = fileName;

            SQLiteConnection.OnChanged(null, new ConnectionEventArgs(
                SQLiteConnectionEventType.NewCriticalHandle, null,
                null, null, null, _sql, fileName, new object[] {
                typeof(SQLite3), fmt, kind, fmtString, db, fileName,
                ownHandle }));
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    #region IDisposable "Pattern" Members
    private bool disposed;
    private void CheckDisposed() /* throw */
    {
#if THROW_ON_DISPOSED
        if (disposed)
            throw new ObjectDisposedException(typeof(SQLite3).Name);
#endif
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    protected override void Dispose(bool disposing)
    {
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

#if INTEROP_VIRTUAL_TABLE
                DisposeModules();
#endif

                Close(true); /* Disposing, cannot throw. */
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

#if DEBUG
    public override string ToString()
    {
        return HelperMethods.StringFormat(
            CultureInfo.InvariantCulture, "fileName = {0}, flags = {1}",
            _fileName, _flags);
    }
#endif

    ///////////////////////////////////////////////////////////////////////////////////////////////

#if INTEROP_VIRTUAL_TABLE
    /// <summary>
    /// This method attempts to dispose of all the <see cref="SQLiteModule" /> derived
    /// object instances currently associated with the native database connection.
    /// </summary>
    private void DisposeModules()
    {
        //
        // NOTE: If any modules were created, attempt to dispose of
        //       them now.  This code is designed to avoid throwing
        //       exceptions unless the Dispose method of the module
        //       itself throws an exception.
        //
        if (_modules != null)
        {
            foreach (KeyValuePair<string, SQLiteModule> pair in _modules)
            {
                SQLiteModule module = pair.Value;

                if (module == null)
                    continue;

                module.Dispose();
            }

            _modules.Clear();
        }
    }
#endif

    ///////////////////////////////////////////////////////////////////////////////////////////////

    // It isn't necessary to cleanup any functions we've registered.  If the connection
    // goes to the pool and is resurrected later, re-registered functions will overwrite the
    // previous functions.  The SQLiteFunctionCookieHandle will take care of freeing unmanaged
    // resources belonging to the previously-registered functions.
    internal override void Close(bool disposing)
    {
      if (_sql != null)
      {
          if (!_sql.OwnHandle)
          {
              _sql = null;
              return;
          }

          bool unbindFunctions = HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UnbindFunctionsOnClose);

      retry:

          if (_usePool)
          {
              if (SQLiteBase.ResetConnection(_sql, _sql, !disposing) &&
                  UnhookNativeCallbacks(true, !disposing))
              {
                  if (unbindFunctions)
                  {
                      if (SQLiteFunction.UnbindAllFunctions(this, _flags, false))
                      {
#if !NET_COMPACT_20 && TRACE_CONNECTION
                          Trace.WriteLine(HelperMethods.StringFormat(
                              CultureInfo.CurrentCulture,
                              "UnbindFunctions (Pool) Success: {0}",
                              HandleToString()));
#endif
                      }
                      else
                      {
#if !NET_COMPACT_20 && TRACE_CONNECTION
                          Trace.WriteLine(HelperMethods.StringFormat(
                              CultureInfo.CurrentCulture,
                              "UnbindFunctions (Pool) Failure: {0}",
                              HandleToString()));
#endif
                      }
                  }

#if INTEROP_VIRTUAL_TABLE
                  DisposeModules();
#endif

                  SQLiteConnectionPool.Add(_fileName, _sql, _poolVersion);

                  SQLiteConnection.OnChanged(null, new ConnectionEventArgs(
                      SQLiteConnectionEventType.ClosedToPool, null, null,
                      null, null, _sql, _fileName, new object[] {
                      typeof(SQLite3), !disposing, _fileName, _poolVersion }));

#if !NET_COMPACT_20 && TRACE_CONNECTION
                  Trace.WriteLine(HelperMethods.StringFormat(
                      CultureInfo.CurrentCulture,
                      "Close (Pool) Success: {0}",
                      HandleToString()));
#endif
              }
              else
              {
#if !NET_COMPACT_20 && TRACE_CONNECTION
                  Trace.WriteLine(HelperMethods.StringFormat(
                      CultureInfo.CurrentCulture,
                      "Close (Pool) Failure: {0}",
                      HandleToString()));
#endif

                  //
                  // NOTE: This connection cannot be added to the pool;
                  //       therefore, just use the normal disposal
                  //       procedure on it.
                  //
                  _usePool = false;
                  goto retry;
              }
          }
          else
          {
              /* IGNORED */
              UnhookNativeCallbacks(disposing, !disposing);

              if (unbindFunctions)
              {
                  if (SQLiteFunction.UnbindAllFunctions(this, _flags, false))
                  {
#if !NET_COMPACT_20 && TRACE_CONNECTION
                      Trace.WriteLine(HelperMethods.StringFormat(
                          CultureInfo.CurrentCulture,
                          "UnbindFunctions Success: {0}",
                          HandleToString()));
#endif
                  }
                  else
                  {
#if !NET_COMPACT_20 && TRACE_CONNECTION
                      Trace.WriteLine(HelperMethods.StringFormat(
                          CultureInfo.CurrentCulture,
                          "UnbindFunctions Failure: {0}",
                          HandleToString()));
#endif
                  }
              }

              _sql.Dispose();

              FreeDbName(!disposing);
          }
          _sql = null;
      }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

#if !NET_COMPACT_20 && TRACE_CONNECTION
    protected string HandleToString()
    {
        if (_sql == null)
            return "<null>";

        return _sql.ToString();
    }
#endif

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Returns the number of times the <see cref="Cancel" /> method has been
    /// called.
    /// </summary>
    private int GetCancelCount()
    {
        return Interlocked.CompareExchange(ref _cancelCount, 0, 0);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This method determines whether or not a <see cref="SQLiteException" />
    /// with a return code of <see cref="SQLiteErrorCode.Interrupt" /> should
    /// be thrown after making a call into the SQLite core library.
    /// </summary>
    /// <returns>
    /// Non-zero if a <see cref="SQLiteException" /> to be thrown.  This method
    /// will only return non-zero if the <see cref="Cancel" /> method was called
    /// one or more times during a call into the SQLite core library (e.g. when
    /// the sqlite3_prepare*() or sqlite3_step() APIs are used).
    /// </returns>
    private bool ShouldThrowForCancel()
    {
        return GetCancelCount() > 0;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Resets the value of the <see cref="_cancelCount" /> field.
    /// </summary>
    private int ResetCancelCount()
    {
        return Interlocked.CompareExchange(ref _cancelCount, 0, _cancelCount);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Attempts to interrupt the query currently executing on the associated
    /// native database connection.
    /// </summary>
    internal override void Cancel()
    {
      try
      {
        // do nothing.
      }
      finally /* NOTE: Thread.Abort() protection. */
      {
        Interlocked.Increment(ref _cancelCount);
        UnsafeNativeMethods.sqlite3_interrupt(_sql);
      }
    }

    /// <summary>
    /// This function binds a user-defined function to the connection.
    /// </summary>
    /// <param name="functionAttribute">
    /// The <see cref="SQLiteFunctionAttribute"/> object instance containing
    /// the metadata for the function to be bound.
    /// </param>
    /// <param name="function">
    /// The <see cref="SQLiteFunction"/> object instance that implements the
    /// function to be bound.
    /// </param>
    /// <param name="flags">
    /// The flags associated with the parent connection object.
    /// </param>
    internal override void BindFunction(
        SQLiteFunctionAttribute functionAttribute,
        SQLiteFunction function,
        SQLiteConnectionFlags flags
        )
    {
        if (functionAttribute == null)
            throw new ArgumentNullException("functionAttribute");

        if (function == null)
            throw new ArgumentNullException("function");

        SQLiteFunction.BindFunction(this, functionAttribute, function, flags);

        if (_functions == null)
            _functions = new Dictionary<SQLiteFunctionAttribute, SQLiteFunction>();

        _functions[functionAttribute] = function;
    }

    /// <summary>
    /// This function binds a user-defined function to the connection.
    /// </summary>
    /// <param name="functionAttribute">
    /// The <see cref="SQLiteFunctionAttribute"/> object instance containing
    /// the metadata for the function to be unbound.
    /// </param>
    /// <param name="flags">
    /// The flags associated with the parent connection object.
    /// </param>
    /// <returns>Non-zero if the function was unbound and removed.</returns>
    internal override bool UnbindFunction(
        SQLiteFunctionAttribute functionAttribute,
        SQLiteConnectionFlags flags
        )
    {
        if (functionAttribute == null)
            throw new ArgumentNullException("functionAttribute");

        if (_functions == null)
            return false;

        SQLiteFunction function;

        if (_functions.TryGetValue(functionAttribute, out function))
        {
            if (SQLiteFunction.UnbindFunction(
                    this, functionAttribute, function, flags) &&
                _functions.Remove(functionAttribute))
            {
                return true;
            }
        }

        return false;
    }

    internal override string Version
    {
      get
      {
        return SQLiteVersion;
      }
    }

    internal override int VersionNumber
    {
      get
      {
        return SQLiteVersionNumber;
      }
    }

    internal static string DefineConstants
    {
        get
        {
            StringBuilder result = new StringBuilder();
            IList<string> list = SQLiteDefineConstants.OptionList;

            if (list != null)
            {
                foreach (string element in list)
                {
                    if (element == null)
                        continue;

                    if (result.Length > 0)
                        result.Append(' ');

                    result.Append(element);
                }
            }

            return result.ToString();
        }
    }

    internal static string SQLiteVersion
    {
      get
      {
        return UTF8ToString(UnsafeNativeMethods.sqlite3_libversion(), -1);
      }
    }

    internal static int SQLiteVersionNumber
    {
      get
      {
        return UnsafeNativeMethods.sqlite3_libversion_number();
      }
    }

    internal static string SQLiteSourceId
    {
      get
      {
        return UTF8ToString(UnsafeNativeMethods.sqlite3_sourceid(), -1);
      }
    }

    internal static string SQLiteCompileOptions
    {
        get
        {
            StringBuilder result = new StringBuilder();
            int index = 0;
            IntPtr zValue = UnsafeNativeMethods.sqlite3_compileoption_get(index++);

            while (zValue != IntPtr.Zero)
            {
                if (result.Length > 0)
                    result.Append(' ');

                result.Append(UTF8ToString(zValue, -1));
                zValue = UnsafeNativeMethods.sqlite3_compileoption_get(index++);
            }

            return result.ToString();
        }
    }

    internal static string InteropVersion
    {
        get
        {
#if !SQLITE_STANDARD
            return UTF8ToString(UnsafeNativeMethods.interop_libversion(), -1);
#else
            return null;
#endif
        }
    }

    internal static string InteropSourceId
    {
        get
        {
#if !SQLITE_STANDARD
            return UTF8ToString(UnsafeNativeMethods.interop_sourceid(), -1);
#else
            return null;
#endif
        }
    }

    internal static string InteropCompileOptions
    {
        get
        {
#if !SQLITE_STANDARD
            StringBuilder result = new StringBuilder();
            int index = 0;
            IntPtr zValue = UnsafeNativeMethods.interop_compileoption_get(index++);

            while (zValue != IntPtr.Zero)
            {
                if (result.Length > 0)
                    result.Append(' ');

                result.Append(UTF8ToString(zValue, -1));
                zValue = UnsafeNativeMethods.interop_compileoption_get(index++);
            }

            return result.ToString();
#else
            return null;
#endif
        }
    }

    internal override bool AutoCommit
    {
      get
      {
        return IsAutocommit(_sql, _sql);
      }
    }

    internal override bool IsReadOnly(
        string name
        )
    {
        IntPtr pDbName = IntPtr.Zero;

        try
        {
            if (name != null)
                pDbName = SQLiteString.Utf8IntPtrFromString(name);

            int result = UnsafeNativeMethods.sqlite3_db_readonly(
                _sql, pDbName);

            if (result == -1) /* database not found */
            {
                throw new SQLiteException(HelperMethods.StringFormat(
                    CultureInfo.CurrentCulture,
                    "database \"{0}\" not found", name));
            }

            return result == 0 ? false : true;
        }
        finally
        {
            if (pDbName != IntPtr.Zero)
            {
                SQLiteMemory.Free(pDbName);
                pDbName = IntPtr.Zero;
            }
        }
    }

    internal override long LastInsertRowId
    {
      get
      {
#if !PLATFORM_COMPACTFRAMEWORK
        return UnsafeNativeMethods.sqlite3_last_insert_rowid(_sql);
#elif !SQLITE_STANDARD
        long rowId = 0;
        UnsafeNativeMethods.sqlite3_last_insert_rowid_interop(_sql, ref rowId);
        return rowId;
#else
        throw new NotImplementedException();
#endif
      }
    }

    internal override int Changes
    {
      get
      {
#if !SQLITE_STANDARD
        return UnsafeNativeMethods.sqlite3_changes_interop(_sql);
#else
        return UnsafeNativeMethods.sqlite3_changes(_sql);
#endif
      }
    }

    internal override long MemoryUsed
    {
        get
        {
            return StaticMemoryUsed;
        }
    }

    internal static long StaticMemoryUsed
    {
        get
        {
#if !PLATFORM_COMPACTFRAMEWORK
            return UnsafeNativeMethods.sqlite3_memory_used();
#elif !SQLITE_STANDARD
            long bytes = 0;
            UnsafeNativeMethods.sqlite3_memory_used_interop(ref bytes);
            return bytes;
#else
            throw new NotImplementedException();
#endif
        }
    }

    internal override long MemoryHighwater
    {
        get
        {
            return StaticMemoryHighwater;
        }
    }

    internal static long StaticMemoryHighwater
    {
        get
        {
#if !PLATFORM_COMPACTFRAMEWORK
            return UnsafeNativeMethods.sqlite3_memory_highwater(0);
#elif !SQLITE_STANDARD
            long bytes = 0;
            UnsafeNativeMethods.sqlite3_memory_highwater_interop(0, ref bytes);
            return bytes;
#else
            throw new NotImplementedException();
#endif
        }
    }

    /// <summary>
    /// Returns non-zero if the underlying native connection handle is owned
    /// by this instance.
    /// </summary>
    internal override bool OwnHandle
    {
        get
        {
            if (_sql == null)
                throw new SQLiteException("no connection handle available");

            return _sql.OwnHandle;
        }
    }

    /// <summary>
    /// Returns the logical list of functions associated with this connection.
    /// </summary>
    internal override IDictionary<SQLiteFunctionAttribute, SQLiteFunction> Functions
    {
        get { return _functions; }
    }

    internal override SQLiteErrorCode SetMemoryStatus(bool value)
    {
        return StaticSetMemoryStatus(value);
    }

    internal static SQLiteErrorCode StaticSetMemoryStatus(bool value)
    {
        SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3_config_int(
            SQLiteConfigOpsEnum.SQLITE_CONFIG_MEMSTATUS, value ? 1 : 0);

        return rc;
    }

    /// <summary>
    /// Attempts to free as much heap memory as possible for the database connection.
    /// </summary>
    /// <returns>A standard SQLite return code (i.e. zero for success and non-zero for failure).</returns>
    internal override SQLiteErrorCode ReleaseMemory()
    {
        SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3_db_release_memory(_sql);
        return rc;
    }

    /// <summary>
    /// Attempts to free N bytes of heap memory by deallocating non-essential memory
    /// allocations held by the database library. Memory used to cache database pages
    /// to improve performance is an example of non-essential memory.  This is a no-op
    /// returning zero if the SQLite core library was not compiled with the compile-time
    /// option SQLITE_ENABLE_MEMORY_MANAGEMENT.  Optionally, attempts to reset and/or
    /// compact the Win32 native heap, if applicable.
    /// </summary>
    /// <param name="nBytes">
    /// The requested number of bytes to free.
    /// </param>
    /// <param name="reset">
    /// Non-zero to attempt a heap reset.
    /// </param>
    /// <param name="compact">
    /// Non-zero to attempt heap compaction.
    /// </param>
    /// <param name="nFree">
    /// The number of bytes actually freed.  This value may be zero.
    /// </param>
    /// <param name="resetOk">
    /// This value will be non-zero if the heap reset was successful.
    /// </param>
    /// <param name="nLargest">
    /// The size of the largest committed free block in the heap, in bytes.
    /// This value will be zero unless heap compaction is enabled.
    /// </param>
    /// <returns>
    /// A standard SQLite return code (i.e. zero for success and non-zero
    /// for failure).
    /// </returns>
    internal static SQLiteErrorCode StaticReleaseMemory(
        int nBytes,
        bool reset,
        bool compact,
        ref int nFree,
        ref bool resetOk,
        ref uint nLargest
        )
    {
        SQLiteErrorCode rc = SQLiteErrorCode.Ok;

        int nFreeLocal = UnsafeNativeMethods.sqlite3_release_memory(nBytes);
        uint nLargestLocal = 0;
        bool resetOkLocal = false;

#if !DEBUG && WINDOWS // NOTE: Should be "WIN32HEAP && !MEMDEBUG && WINDOWS"
        if (HelperMethods.IsWindows())
        {
            if ((rc == SQLiteErrorCode.Ok) && reset)
            {
                rc = UnsafeNativeMethods.sqlite3_win32_reset_heap();

                if (rc == SQLiteErrorCode.Ok)
                    resetOkLocal = true;
            }

            if ((rc == SQLiteErrorCode.Ok) && compact)
                rc = UnsafeNativeMethods.sqlite3_win32_compact_heap(ref nLargestLocal);
        }
        else
#endif
        if (reset || compact)
        {
            rc = SQLiteErrorCode.NotFound;
        }

        nFree = nFreeLocal;
        nLargest = nLargestLocal;
        resetOk = resetOkLocal;

        return rc;
    }

    /// <summary>
    /// Shutdown the SQLite engine so that it can be restarted with different
    /// configuration options.  We depend on auto initialization to recover.
    /// </summary>
    /// <returns>Returns a standard SQLite result code.</returns>
    internal override SQLiteErrorCode Shutdown()
    {
        return StaticShutdown(false);
    }

    /// <summary>
    /// Shutdown the SQLite engine so that it can be restarted with different
    /// configuration options.  We depend on auto initialization to recover.
    /// </summary>
    /// <param name="directories">
    /// Non-zero to reset the database and temporary directories to their
    /// default values, which should be null for both.  This parameter has no
    /// effect on non-Windows operating systems.
    /// </param>
    /// <returns>Returns a standard SQLite result code.</returns>
    internal static SQLiteErrorCode StaticShutdown(
        bool directories
        )
    {
        SQLiteErrorCode rc = SQLiteErrorCode.Ok;

        if (directories)
        {
#if WINDOWS
            if (HelperMethods.IsWindows())
            {
                if (rc == SQLiteErrorCode.Ok)
                    rc = UnsafeNativeMethods.sqlite3_win32_set_directory(1, null);

                if (rc == SQLiteErrorCode.Ok)
                    rc = UnsafeNativeMethods.sqlite3_win32_set_directory(2, null);
            }
            else
#endif
            {
#if !NET_COMPACT_20 && TRACE_CONNECTION
                Trace.WriteLine(
                    "Shutdown: Cannot reset directories on this platform.");
#endif
            }
        }

        if (rc == SQLiteErrorCode.Ok)
            rc = UnsafeNativeMethods.sqlite3_shutdown();

        return rc;
    }

    /// <summary>
    /// Determines if the associated native connection handle is open.
    /// </summary>
    /// <returns>
    /// Non-zero if the associated native connection handle is open.
    /// </returns>
    internal override bool IsOpen()
    {
        return (_sql != null) && !_sql.IsInvalid && !_sql.IsClosed;
    }

    /// <summary>
    /// Returns the fully qualified path and file name for the currently open
    /// database, if any.
    /// </summary>
    /// <param name="dbName">
    /// The name of the attached database to query.
    /// </param>
    /// <returns>
    /// The fully qualified path and file name for the currently open database,
    /// if any.
    /// </returns>
    internal override string GetFileName(string dbName)
    {
        if (_sql == null)
            return null;

        return UTF8ToString(UnsafeNativeMethods.sqlite3_db_filename_bytes(
            _sql, ToUTF8(dbName)), -1);
    }

    /// <summary>
    /// This method attempts to determine if a database connection opened
    /// with the specified <see cref="SQLiteOpenFlagsEnum" /> should be
    /// allowed into the connection pool.
    /// </summary>
    /// <param name="openFlags">
    /// The <see cref="SQLiteOpenFlagsEnum" /> that were specified when the
    /// connection was opened.
    /// </param>
    /// <returns>
    /// Non-zero if the connection should (eventually) be allowed into the
    /// connection pool; otherwise, zero.
    /// </returns>
    private static bool IsAllowedToUsePool(
        SQLiteOpenFlagsEnum openFlags
        )
    {
        return openFlags == SQLiteOpenFlagsEnum.Default;
    }

    internal override void Open(string strFilename, string vfsName, SQLiteConnectionFlags connectionFlags, SQLiteOpenFlagsEnum openFlags, int maxPoolSize, bool usePool)
    {
      //
      // NOTE: If the database connection is currently open, attempt to
      //       close it now.  This must be done because the file name or
      //       other parameters that may impact the underlying database
      //       connection may have changed.
      //
      if (_sql != null) Close(false);

      //
      // NOTE: If the connection was not closed successfully, throw an
      //       exception now.
      //
      if (_sql != null)
          throw new SQLiteException("connection handle is still active");

      _usePool = usePool;

      //
      // BUGFIX: Do not allow a connection into the pool if it was opened
      //         with flags that are incompatible with the default flags
      //         (e.g. read-only).
      //
      if (_usePool && !IsAllowedToUsePool(openFlags))
          _usePool = false;

      _fileName = strFilename;
      _flags = connectionFlags;

      if (usePool)
      {
        _sql = SQLiteConnectionPool.Remove(strFilename, maxPoolSize, out _poolVersion);

        SQLiteConnection.OnChanged(null, new ConnectionEventArgs(
            SQLiteConnectionEventType.OpenedFromPool, null, null,
            null, null, _sql, strFilename, new object[] {
            typeof(SQLite3), strFilename, vfsName, connectionFlags,
            openFlags, maxPoolSize, usePool, _poolVersion }));

#if !NET_COMPACT_20 && TRACE_CONNECTION
        Trace.WriteLine(HelperMethods.StringFormat(
            CultureInfo.CurrentCulture,
            "Open (Pool): {0}", HandleToString()));
#endif
      }

      if (_sql == null)
      {
        try
        {
            // do nothing.
        }
        finally /* NOTE: Thread.Abort() protection. */
        {
          IntPtr db = IntPtr.Zero;
          SQLiteErrorCode n;

#if !SQLITE_STANDARD
          int extFuncs = HelperMethods.HasFlags(connectionFlags, SQLiteConnectionFlags.NoExtensionFunctions) ? 0 : 1;

          if (extFuncs != 0)
          {
            n = UnsafeNativeMethods.sqlite3_open_interop(ToUTF8(strFilename), ToUTF8(vfsName), openFlags, extFuncs, ref db);
          }
          else
#endif
          {
            n = UnsafeNativeMethods.sqlite3_open_v2(ToUTF8(strFilename), ref db, openFlags, ToUTF8(vfsName));
          }

#if !NET_COMPACT_20 && TRACE_CONNECTION
          Trace.WriteLine(HelperMethods.StringFormat(
              CultureInfo.CurrentCulture,
              "Open: {0}", db));
#endif

          if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, null);
          _sql = new SQLiteConnectionHandle(db, true);
        }
        lock (_sql) { /* HACK: Force the SyncBlock to be "created" now. */ }

        SQLiteConnection.OnChanged(null, new ConnectionEventArgs(
            SQLiteConnectionEventType.NewCriticalHandle, null,
            null, null, null, _sql, strFilename, new object[] {
            typeof(SQLite3), strFilename, vfsName, connectionFlags,
            openFlags, maxPoolSize, usePool }));
      }

      // Bind functions to this connection.  If any previous functions of the same name
      // were already bound, then the new bindings replace the old.
      if (!HelperMethods.HasFlags(connectionFlags, SQLiteConnectionFlags.NoBindFunctions))
      {
          if (_functions == null)
              _functions = new Dictionary<SQLiteFunctionAttribute, SQLiteFunction>();

          foreach (KeyValuePair<SQLiteFunctionAttribute, SQLiteFunction> pair
                  in SQLiteFunction.BindFunctions(this, connectionFlags))
          {
              _functions[pair.Key] = pair.Value;
          }
      }

      SetTimeout(0);
      GC.KeepAlive(_sql);
    }

    internal override void ClearPool()
    {
      SQLiteConnectionPool.ClearPool(_fileName);
    }

    internal override int CountPool()
    {
        Dictionary<string, int> counts = null;
        int openCount = 0;
        int closeCount = 0;
        int totalCount = 0;

        SQLiteConnectionPool.GetCounts(_fileName,
            ref counts, ref openCount, ref closeCount,
            ref totalCount);

        return totalCount;
    }

    internal override void SetTimeout(int nTimeoutMS)
    {
      IntPtr db = _sql;
      if (db == IntPtr.Zero) throw new SQLiteException("no connection handle available");
      SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_busy_timeout(db, nTimeoutMS);
      if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
    }

    internal override bool Step(SQLiteStatement stmt)
    {
      SQLiteErrorCode n;
      Random rnd = null;
      uint starttick = (uint)Environment.TickCount;
      uint timeout = (uint)(stmt._command._commandTimeout * 1000);

      ResetCancelCount();

      while (true)
      {
        try
        {
            // do nothing.
        }
        finally /* NOTE: Thread.Abort() protection. */
        {
          n = UnsafeNativeMethods.sqlite3_step(stmt._sqlite_stmt);
        }

        if (ShouldThrowForCancel())
        {
            if ((n == SQLiteErrorCode.Ok) ||
                (n == SQLiteErrorCode.Row) ||
                (n == SQLiteErrorCode.Done))
            {
                n = SQLiteErrorCode.Interrupt;
            }

            throw new SQLiteException(n, null);
        }

        if (n == SQLiteErrorCode.Interrupt) return false;
        if (n == SQLiteErrorCode.Row) return true;
        if (n == SQLiteErrorCode.Done) return false;

        if (n != SQLiteErrorCode.Ok)
        {
          SQLiteErrorCode r;

          // An error occurred, attempt to reset the statement.  If the reset worked because the
          // schema has changed, re-try the step again.  If it errored our because the database
          // is locked, then keep retrying until the command timeout occurs.
          r = Reset(stmt);

          if (r == SQLiteErrorCode.Ok)
            throw new SQLiteException(n, GetLastError());

          else if ((r == SQLiteErrorCode.Locked || r == SQLiteErrorCode.Busy) && stmt._command != null)
          {
            // Keep trying
            if (rnd == null) // First time we've encountered the lock
              rnd = new Random();

            // If we've exceeded the command's timeout, give up and throw an error
            if ((uint)Environment.TickCount - starttick > timeout)
            {
              throw new SQLiteException(r, GetLastError());
            }
            else
            {
              // Otherwise sleep for a random amount of time up to 150ms
              System.Threading.Thread.Sleep(rnd.Next(1, 150));
            }
          }
        }
      }
    }

    /// <summary>
    /// Has the sqlite3_errstr() core library API been checked for yet?
    /// If so, is it present?
    /// </summary>
    private static bool? have_errstr = null;

    /// <summary>
    /// Returns the error message for the specified SQLite return code using
    /// the sqlite3_errstr() function, falling back to the internal lookup
    /// table if necessary.
    ///
    /// WARNING: Do not remove this method, it is used via reflection.
    /// </summary>
    /// <param name="rc">The SQLite return code.</param>
    /// <returns>The error message or null if it cannot be found.</returns>
    internal static string GetErrorString(SQLiteErrorCode rc)
    {
        try
        {
            if (have_errstr == null)
            {
                int versionNumber = SQLiteVersionNumber;
                have_errstr = (versionNumber >= 3007015);
            }

            if ((bool)have_errstr)
            {
                IntPtr ptr = UnsafeNativeMethods.sqlite3_errstr(rc);

                if (ptr != IntPtr.Zero)
                {
#if !PLATFORM_COMPACTFRAMEWORK
                    return Marshal.PtrToStringAnsi(ptr);
#else
                    return UTF8ToString(ptr, -1);
#endif
                }
            }
        }
        catch (EntryPointNotFoundException)
        {
            // do nothing.
        }

        return FallbackGetErrorString(rc);
    }

    /// <summary>
    /// Has the sqlite3_stmt_readonly() core library API been checked for yet?
    /// If so, is it present?
    /// </summary>
    private static bool? have_stmt_readonly = null;

    /// <summary>
    /// Returns non-zero if the specified statement is read-only in nature.
    /// </summary>
    /// <param name="stmt">The statement to check.</param>
    /// <returns>True if the outer query is read-only.</returns>
    internal override bool IsReadOnly(
        SQLiteStatement stmt
        )
    {
        try
        {
            if (have_stmt_readonly == null)
            {
                int versionNumber = SQLiteVersionNumber;
                have_stmt_readonly = (versionNumber >= 3007004);
            }

            if ((bool)have_stmt_readonly)
            {
                return UnsafeNativeMethods.sqlite3_stmt_readonly(
                    stmt._sqlite_stmt) != 0;
            }
        }
        catch (EntryPointNotFoundException)
        {
            // do nothing.
        }

        return false; /* NOTE: Unknown, assume false. */
    }

    internal override SQLiteErrorCode Reset(SQLiteStatement stmt)
    {
      SQLiteErrorCode n;

#if !SQLITE_STANDARD
      n = UnsafeNativeMethods.sqlite3_reset_interop(stmt._sqlite_stmt);
#else
      n = UnsafeNativeMethods.sqlite3_reset(stmt._sqlite_stmt);
#endif

      // If the schema changed, try and re-prepare it
      if (n == SQLiteErrorCode.Schema)
      {
        // Recreate a dummy statement
        string str = null;
        using (SQLiteStatement tmp = Prepare(null, stmt._sqlStatement, null, (uint)(stmt._command._commandTimeout * 1000), ref str))
        {
          // Finalize the existing statement
          stmt._sqlite_stmt.Dispose();
          // Reassign a new statement pointer to the old statement and clear the temporary one
          if (tmp != null)
          {
            stmt._sqlite_stmt = tmp._sqlite_stmt;
            tmp._sqlite_stmt = null;
          }

          // Reapply parameters
          stmt.BindParameters();
        }
        return SQLiteErrorCode.Unknown; // Reset was OK, with schema change
      }
      else if (n == SQLiteErrorCode.Locked || n == SQLiteErrorCode.Busy)
        return n;

      if (n != SQLiteErrorCode.Ok)
        throw new SQLiteException(n, GetLastError());

      return n; // We reset OK, no schema changes
    }

    internal override string GetLastError()
    {
        return GetLastError(null);
    }

    internal override string GetLastError(string defValue)
    {
        string result = SQLiteBase.GetLastError(_sql, _sql);
        if (String.IsNullOrEmpty(result)) result = defValue;
        return result;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    #region Query Diagnostics Support
    /// <summary>
    /// This field is used to keep track of whether or not the
    /// "SQLite_ForceLogPrepare" environment variable has been queried.  If so,
    /// it will only be non-zero if the environment variable was present.
    /// </summary>
    private static bool? forceLogPrepare = null;

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Determines if all calls to prepare a SQL query will be logged,
    /// regardless of the flags for the associated connection.
    /// </summary>
    /// <returns>
    /// Non-zero to log all calls to prepare a SQL query.
    /// </returns>
    internal static bool ForceLogPrepare()
    {
        lock (syncRoot)
        {
            if (forceLogPrepare == null)
            {
                if (UnsafeNativeMethods.GetSettingValue(
                        "SQLite_ForceLogPrepare", null) != null)
                {
                    forceLogPrepare = true;
                }
                else
                {
                    forceLogPrepare = false;
                }
            }

            return (bool)forceLogPrepare;
        }
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////////////////////////

    internal override SQLiteStatement Prepare(SQLiteConnection cnn, string strSql, SQLiteStatement previous, uint timeoutMS, ref string strRemain)
    {
      if (!String.IsNullOrEmpty(strSql)) strSql = strSql.Trim();
      if (!String.IsNullOrEmpty(strSql))
      {
        //
        // NOTE: SQLite does not support the concept of separate schemas
        //       in one database; therefore, remove the base schema name
        //       used to smooth integration with the base .NET Framework
        //       data classes.
        //
        string baseSchemaName = (cnn != null) ? cnn._baseSchemaName : null;

        if (!String.IsNullOrEmpty(baseSchemaName))
        {
          strSql = strSql.Replace(
              HelperMethods.StringFormat(CultureInfo.InvariantCulture,
              "[{0}].", baseSchemaName), String.Empty);

          strSql = strSql.Replace(
              HelperMethods.StringFormat(CultureInfo.InvariantCulture,
              "{0}.", baseSchemaName), String.Empty);
        }
      }

      SQLiteConnectionFlags flags =
          (cnn != null) ? cnn.Flags : SQLiteConnectionFlags.Default;

      if (ForceLogPrepare() ||
          HelperMethods.LogPrepare(flags))
      {
          if ((strSql == null) || (strSql.Length == 0) || (strSql.Trim().Length == 0))
              SQLiteLog.LogMessage("Preparing {<nothing>}...");
          else
              SQLiteLog.LogMessage(HelperMethods.StringFormat(
                  CultureInfo.CurrentCulture, "Preparing {{{0}}}...", strSql));
      }

      IntPtr stmt = IntPtr.Zero;
      IntPtr ptr = IntPtr.Zero;
      int len = 0;
      SQLiteErrorCode n = SQLiteErrorCode.Schema;
      int retries = 0;
      int maximumRetries = (cnn != null) ? cnn._prepareRetries : SQLiteConnection.DefaultPrepareRetries;
      byte[] b = ToUTF8(strSql);
      string typedefs = null;
      SQLiteStatement cmd = null;
      Random rnd = null;
      uint starttick = (uint)Environment.TickCount;

      ResetCancelCount();

      GCHandle handle = GCHandle.Alloc(b, GCHandleType.Pinned);
      IntPtr psql = handle.AddrOfPinnedObject();
      SQLiteStatementHandle statementHandle = null;
      try
      {
        while ((n == SQLiteErrorCode.Schema || n == SQLiteErrorCode.Locked || n == SQLiteErrorCode.Busy) && retries < maximumRetries)
        {
          try
          {
            // do nothing.
          }
          finally /* NOTE: Thread.Abort() protection. */
          {
            stmt = IntPtr.Zero;
            ptr = IntPtr.Zero;

#if !SQLITE_STANDARD
            len = 0;
            n = UnsafeNativeMethods.sqlite3_prepare_interop(_sql, psql, b.Length - 1, ref stmt, ref ptr, ref len);
#else
#if USE_PREPARE_V2
            n = UnsafeNativeMethods.sqlite3_prepare_v2(_sql, psql, b.Length - 1, ref stmt, ref ptr);
#else
            n = UnsafeNativeMethods.sqlite3_prepare(_sql, psql, b.Length - 1, ref stmt, ref ptr);
#endif
            len = -1;
#endif

#if !NET_COMPACT_20 && TRACE_STATEMENT
            Trace.WriteLine(HelperMethods.StringFormat(
                CultureInfo.CurrentCulture,
                "Prepare ({0}): {1}", n, stmt));
#endif

            if ((n == SQLiteErrorCode.Ok) && (stmt != IntPtr.Zero))
            {
              if (statementHandle != null) statementHandle.Dispose();
              statementHandle = new SQLiteStatementHandle(_sql, stmt);
            }
          }

          if (statementHandle != null)
          {
            SQLiteConnection.OnChanged(null, new ConnectionEventArgs(
              SQLiteConnectionEventType.NewCriticalHandle, null, null,
              null, null, statementHandle, strSql, new object[] {
              typeof(SQLite3), cnn, strSql, previous, timeoutMS }));
          }

          if (ShouldThrowForCancel())
          {
              if ((n == SQLiteErrorCode.Ok) ||
                  (n == SQLiteErrorCode.Row) ||
                  (n == SQLiteErrorCode.Done))
              {
                  n = SQLiteErrorCode.Interrupt;
              }

              throw new SQLiteException(n, null);
          }

          if (n == SQLiteErrorCode.Interrupt)
            break;
          else if (n == SQLiteErrorCode.Schema)
            retries++;
          else if (n == SQLiteErrorCode.Error)
          {
            if (String.Compare(GetLastError(), "near \"TYPES\": syntax error", StringComparison.OrdinalIgnoreCase) == 0)
            {
              int pos = strSql.IndexOf(';');
              if (pos == -1) pos = strSql.Length - 1;

              typedefs = strSql.Substring(0, pos + 1);
              strSql = strSql.Substring(pos + 1);

              strRemain = String.Empty;

              while (cmd == null && strSql.Length > 0)
              {
                cmd = Prepare(cnn, strSql, previous, timeoutMS, ref strRemain);
                strSql = strRemain;
              }

              if (cmd != null)
                cmd.SetTypes(typedefs);

              return cmd;
            }
#if (NET_35 || NET_40 || NET_45 || NET_451 || NET_452 || NET_46 || NET_461 || NET_462 || NET_47 || NET_471 || NET_472) && !PLATFORM_COMPACTFRAMEWORK
            else if (_buildingSchema == false && String.Compare(GetLastError(), 0, "no such table: TEMP.SCHEMA", 0, 26, StringComparison.OrdinalIgnoreCase) == 0)
            {
              strRemain = String.Empty;
              _buildingSchema = true;
              try
              {
                ISQLiteSchemaExtensions ext = ((IServiceProvider)SQLiteFactory.Instance).GetService(typeof(ISQLiteSchemaExtensions)) as ISQLiteSchemaExtensions;

                if (ext != null)
                  ext.BuildTempSchema(cnn);

                while (cmd == null && strSql.Length > 0)
                {
                  cmd = Prepare(cnn, strSql, previous, timeoutMS, ref strRemain);
                  strSql = strRemain;
                }

                return cmd;
              }
              finally
              {
                _buildingSchema = false;
              }
            }
#endif
          }
          else if (n == SQLiteErrorCode.Locked || n == SQLiteErrorCode.Busy) // Locked -- delay a small amount before retrying
          {
            // Keep trying
            if (rnd == null) // First time we've encountered the lock
              rnd = new Random();

            // If we've exceeded the command's timeout, give up and throw an error
            if ((uint)Environment.TickCount - starttick > timeoutMS)
            {
              throw new SQLiteException(n, GetLastError());
            }
            else
            {
              // Otherwise sleep for a random amount of time up to 150ms
              System.Threading.Thread.Sleep(rnd.Next(1, 150));
            }
          }
        }

        if (ShouldThrowForCancel())
        {
            if ((n == SQLiteErrorCode.Ok) ||
                (n == SQLiteErrorCode.Row) ||
                (n == SQLiteErrorCode.Done))
            {
                n = SQLiteErrorCode.Interrupt;
            }

            throw new SQLiteException(n, null);
        }

        if (n == SQLiteErrorCode.Interrupt) return null;
        if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());

        strRemain = UTF8ToString(ptr, len);

        if (statementHandle != null) cmd = new SQLiteStatement(this, flags, statementHandle, strSql.Substring(0, strSql.Length - strRemain.Length), previous);

        return cmd;
      }
      finally
      {
        handle.Free();
      }
    }

    protected static void LogBind(SQLiteStatementHandle handle, int index)
    {
        IntPtr handleIntPtr = handle;

        SQLiteLog.LogMessage(HelperMethods.StringFormat(
            CultureInfo.CurrentCulture,
            "Binding statement {0} paramter #{1} as NULL...",
            handleIntPtr, index));
    }

    protected static void LogBind(SQLiteStatementHandle handle, int index, ValueType value)
    {
        IntPtr handleIntPtr = handle;

        SQLiteLog.LogMessage(HelperMethods.StringFormat(
            CultureInfo.CurrentCulture,
            "Binding statement {0} paramter #{1} as type {2} with value {{{3}}}...",
            handleIntPtr, index, value.GetType(), value));
    }

    private static string FormatDateTime(DateTime value)
    {
        StringBuilder result = new StringBuilder();

        result.Append(value.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK"));
        result.Append(' ');
        result.Append(value.Kind);
        result.Append(' ');
        result.Append(value.Ticks);

        return result.ToString();
    }

    protected static void LogBind(SQLiteStatementHandle handle, int index, DateTime value)
    {
        IntPtr handleIntPtr = handle;

        SQLiteLog.LogMessage(HelperMethods.StringFormat(
            CultureInfo.CurrentCulture,
            "Binding statement {0} paramter #{1} as type {2} with value {{{3}}}...",
            handleIntPtr, index, typeof(DateTime), FormatDateTime(value)));
    }

    protected static void LogBind(SQLiteStatementHandle handle, int index, string value)
    {
        IntPtr handleIntPtr = handle;

        SQLiteLog.LogMessage(HelperMethods.StringFormat(
            CultureInfo.CurrentCulture,
            "Binding statement {0} paramter #{1} as type {2} with value {{{3}}}...",
            handleIntPtr, index, typeof(String), (value != null) ? value : "<null>"));
    }

    private static string ToHexadecimalString(
        byte[] array
        )
    {
        if (array == null)
            return null;

        StringBuilder result = new StringBuilder(array.Length * 2);

        int length = array.Length;

        for (int index = 0; index < length; index++)
            result.Append(array[index].ToString("x2"));

        return result.ToString();
    }

    protected static void LogBind(SQLiteStatementHandle handle, int index, byte[] value)
    {
        IntPtr handleIntPtr = handle;

        SQLiteLog.LogMessage(HelperMethods.StringFormat(
            CultureInfo.CurrentCulture,
            "Binding statement {0} paramter #{1} as type {2} with value {{{3}}}...",
            handleIntPtr, index, typeof(Byte[]), (value != null) ? ToHexadecimalString(value) : "<null>"));
    }

    internal override void Bind_Double(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, double value)
    {
        SQLiteStatementHandle handle = stmt._sqlite_stmt;

        if (ForceLogPrepare() || HelperMethods.LogBind(flags))
        {
            LogBind(handle, index, value);
        }

#if !PLATFORM_COMPACTFRAMEWORK
        SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_double(handle, index, value);
        if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
#elif !SQLITE_STANDARD
        SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_double_interop(handle, index, ref value);
        if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
#else
        throw new NotImplementedException();
#endif
    }

    internal override void Bind_Int32(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, int value)
    {
        SQLiteStatementHandle handle = stmt._sqlite_stmt;

        if (ForceLogPrepare() || HelperMethods.LogBind(flags))
        {
            LogBind(handle, index, value);
        }

        SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_int(handle, index, value);
        if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
    }

    internal override void Bind_UInt32(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, uint value)
    {
        SQLiteStatementHandle handle = stmt._sqlite_stmt;

        if (ForceLogPrepare() || HelperMethods.LogBind(flags))
        {
            LogBind(handle, index, value);
        }

        SQLiteErrorCode n;

        if (HelperMethods.HasFlags(flags, SQLiteConnectionFlags.BindUInt32AsInt64))
        {
            long value2 = value;

#if !PLATFORM_COMPACTFRAMEWORK
            n = UnsafeNativeMethods.sqlite3_bind_int64(handle, index, value2);
#elif !SQLITE_STANDARD
            n = UnsafeNativeMethods.sqlite3_bind_int64_interop(handle, index, ref value2);
#else
            throw new NotImplementedException();
#endif
        }
        else
        {
            n = UnsafeNativeMethods.sqlite3_bind_uint(handle, index, value);
        }
        if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
    }

    internal override void Bind_Int64(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, long value)
    {
        SQLiteStatementHandle handle = stmt._sqlite_stmt;

        if (ForceLogPrepare() || HelperMethods.LogBind(flags))
        {
            LogBind(handle, index, value);
        }

#if !PLATFORM_COMPACTFRAMEWORK
        SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_int64(handle, index, value);
        if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
#elif !SQLITE_STANDARD
        SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_int64_interop(handle, index, ref value);
        if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
#else
        throw new NotImplementedException();
#endif
    }

    internal override void Bind_UInt64(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, ulong value)
    {
        SQLiteStatementHandle handle = stmt._sqlite_stmt;

        if (ForceLogPrepare() || HelperMethods.LogBind(flags))
        {
            LogBind(handle, index, value);
        }

#if !PLATFORM_COMPACTFRAMEWORK
        SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_uint64(handle, index, value);
        if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
#elif !SQLITE_STANDARD
        SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_uint64_interop(handle, index, ref value);
        if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
#else
        throw new NotImplementedException();
#endif
    }

    internal override void Bind_Boolean(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, bool value)
    {
        SQLiteStatementHandle handle = stmt._sqlite_stmt;

        if (ForceLogPrepare() || HelperMethods.LogBind(flags))
        {
            LogBind(handle, index, value);
        }

        int value2 = value ? 1 : 0;

        SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_int(handle, index, value2);
        if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
    }

    internal override void Bind_Text(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, string value)
    {
        SQLiteStatementHandle handle = stmt._sqlite_stmt;

        if (ForceLogPrepare() || HelperMethods.LogBind(flags))
        {
            LogBind(handle, index, value);
        }

        byte[] b = ToUTF8(value);

        if (ForceLogPrepare() || HelperMethods.LogBind(flags))
        {
            LogBind(handle, index, b);
        }

        SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_text(handle, index, b, b.Length - 1, (IntPtr)(-1));
        if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
    }

    internal override void Bind_DateTime(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, DateTime dt)
    {
        SQLiteStatementHandle handle = stmt._sqlite_stmt;

        if (ForceLogPrepare() || HelperMethods.LogBind(flags))
        {
            LogBind(handle, index, dt);
        }

        if (HelperMethods.HasFlags(flags, SQLiteConnectionFlags.BindDateTimeWithKind))
        {
            if ((_datetimeKind != DateTimeKind.Unspecified) &&
                (dt.Kind != DateTimeKind.Unspecified) &&
                (dt.Kind != _datetimeKind))
            {
                if (_datetimeKind == DateTimeKind.Utc)
                    dt = dt.ToUniversalTime();
                else if (_datetimeKind == DateTimeKind.Local)
                    dt = dt.ToLocalTime();
            }
        }

        switch (_datetimeFormat)
        {
            case SQLiteDateFormats.Ticks:
                {
                    long value = dt.Ticks;

                    if (ForceLogPrepare() || HelperMethods.LogBind(flags))
                    {
                        LogBind(handle, index, value);
                    }

#if !PLATFORM_COMPACTFRAMEWORK
                    SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_int64(handle, index, value);
                    if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
                    break;
#elif !SQLITE_STANDARD
                    SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_int64_interop(handle, index, ref value);
                    if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
                    break;
#else
                    throw new NotImplementedException();
#endif
                }
            case SQLiteDateFormats.JulianDay:
                {
                    double value = ToJulianDay(dt);

                    if (ForceLogPrepare() || HelperMethods.LogBind(flags))
                    {
                        LogBind(handle, index, value);
                    }

#if !PLATFORM_COMPACTFRAMEWORK
                    SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_double(handle, index, value);
                    if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
                    break;
#elif !SQLITE_STANDARD
                    SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_double_interop(handle, index, ref value);
                    if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
                    break;
#else
                    throw new NotImplementedException();
#endif
                }
            case SQLiteDateFormats.UnixEpoch:
                {
                    long value = Convert.ToInt64(dt.Subtract(UnixEpoch).TotalSeconds);

                    if (ForceLogPrepare() || HelperMethods.LogBind(flags))
                    {
                        LogBind(handle, index, value);
                    }

#if !PLATFORM_COMPACTFRAMEWORK
                    SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_int64(handle, index, value);
                    if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
                    break;
#elif !SQLITE_STANDARD
                    SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_int64_interop(handle, index, ref value);
                    if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
                    break;
#else
                    throw new NotImplementedException();
#endif
                }
            default:
                {
                    byte[] b = ToUTF8(dt);

                    if (ForceLogPrepare() || HelperMethods.LogBind(flags))
                    {
                        LogBind(handle, index, b);
                    }

                    SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_text(handle, index, b, b.Length - 1, (IntPtr)(-1));
                    if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
                    break;
                }
        }
    }

    internal override void Bind_Blob(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, byte[] blobData)
    {
        SQLiteStatementHandle handle = stmt._sqlite_stmt;

        if (ForceLogPrepare() || HelperMethods.LogBind(flags))
        {
            LogBind(handle, index, blobData);
        }

        SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_blob(handle, index, blobData, blobData.Length, (IntPtr)(-1));
        if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
    }

    internal override void Bind_Null(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index)
    {
        SQLiteStatementHandle handle = stmt._sqlite_stmt;

        if (ForceLogPrepare() || HelperMethods.LogBind(flags))
        {
            LogBind(handle, index);
        }

        SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_null(handle, index);
        if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
    }

    internal override int Bind_ParamCount(SQLiteStatement stmt, SQLiteConnectionFlags flags)
    {
        SQLiteStatementHandle handle = stmt._sqlite_stmt;
        int value = UnsafeNativeMethods.sqlite3_bind_parameter_count(handle);

        if (ForceLogPrepare() || HelperMethods.LogBind(flags))
        {
            IntPtr handleIntPtr = handle;

            SQLiteLog.LogMessage(HelperMethods.StringFormat(
                CultureInfo.CurrentCulture,
                "Statement {0} paramter count is {1}.",
                handleIntPtr, value));
        }

        return value;
    }

    internal override string Bind_ParamName(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index)
    {
        SQLiteStatementHandle handle = stmt._sqlite_stmt;
        string name;

#if !SQLITE_STANDARD
        int len = 0;
        name = UTF8ToString(UnsafeNativeMethods.sqlite3_bind_parameter_name_interop(handle, index, ref len), len);
#else
        name = UTF8ToString(UnsafeNativeMethods.sqlite3_bind_parameter_name(handle, index), -1);
#endif

        if (ForceLogPrepare() || HelperMethods.LogBind(flags))
        {
            IntPtr handleIntPtr = handle;

            SQLiteLog.LogMessage(HelperMethods.StringFormat(
                CultureInfo.CurrentCulture,
                "Statement {0} paramter #{1} name is {{{2}}}.",
                handleIntPtr, index, name));
        }

        return name;
    }

    internal override int Bind_ParamIndex(SQLiteStatement stmt, SQLiteConnectionFlags flags, string paramName)
    {
        SQLiteStatementHandle handle = stmt._sqlite_stmt;
        int index = UnsafeNativeMethods.sqlite3_bind_parameter_index(handle, ToUTF8(paramName));

        if (ForceLogPrepare() || HelperMethods.LogBind(flags))
        {
            IntPtr handleIntPtr = handle;

            SQLiteLog.LogMessage(HelperMethods.StringFormat(
                CultureInfo.CurrentCulture,
                "Statement {0} paramter index of name {{{1}}} is #{2}.",
                handleIntPtr, paramName, index));
        }

        return index;
    }

    internal override int ColumnCount(SQLiteStatement stmt)
    {
      return UnsafeNativeMethods.sqlite3_column_count(stmt._sqlite_stmt);
    }

    internal override string ColumnName(SQLiteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len = 0;
      IntPtr p = UnsafeNativeMethods.sqlite3_column_name_interop(stmt._sqlite_stmt, index, ref len);
#else
      IntPtr p = UnsafeNativeMethods.sqlite3_column_name(stmt._sqlite_stmt, index);
#endif
      if (p == IntPtr.Zero)
        throw new SQLiteException(SQLiteErrorCode.NoMem, GetLastError());
#if !SQLITE_STANDARD
      return UTF8ToString(p, len);
#else
      return UTF8ToString(p, -1);
#endif
    }

    internal override TypeAffinity ColumnAffinity(SQLiteStatement stmt, int index)
    {
      return UnsafeNativeMethods.sqlite3_column_type(stmt._sqlite_stmt, index);
    }

    internal override string ColumnType(SQLiteStatement stmt, int index, ref TypeAffinity nAffinity)
    {
        int len;
#if !SQLITE_STANDARD
        len = 0;
        IntPtr p = UnsafeNativeMethods.sqlite3_column_decltype_interop(stmt._sqlite_stmt, index, ref len);
#else
        len = -1;
        IntPtr p = UnsafeNativeMethods.sqlite3_column_decltype(stmt._sqlite_stmt, index);
#endif
        nAffinity = ColumnAffinity(stmt, index);

        if ((p != IntPtr.Zero) && ((len > 0) || (len == -1)))
        {
            string declType = UTF8ToString(p, len);

            if (!String.IsNullOrEmpty(declType))
                return declType;
        }

        string[] ar = stmt.TypeDefinitions;

        if (ar != null)
        {
            if (index < ar.Length && ar[index] != null)
                return ar[index];
        }

        return String.Empty;
    }

    internal override int ColumnIndex(SQLiteStatement stmt, string columnName)
    {
      int x = ColumnCount(stmt);

      for (int n = 0; n < x; n++)
      {
        if (String.Compare(columnName, ColumnName(stmt, n), StringComparison.OrdinalIgnoreCase) == 0)
          return n;
      }
      return -1;
    }

    internal override string ColumnOriginalName(SQLiteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len = 0;
      return UTF8ToString(UnsafeNativeMethods.sqlite3_column_origin_name_interop(stmt._sqlite_stmt, index, ref len), len);
#else
      return UTF8ToString(UnsafeNativeMethods.sqlite3_column_origin_name(stmt._sqlite_stmt, index), -1);
#endif
    }

    internal override string ColumnDatabaseName(SQLiteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len = 0;
      return UTF8ToString(UnsafeNativeMethods.sqlite3_column_database_name_interop(stmt._sqlite_stmt, index, ref len), len);
#else
      return UTF8ToString(UnsafeNativeMethods.sqlite3_column_database_name(stmt._sqlite_stmt, index), -1);
#endif
    }

    internal override string ColumnTableName(SQLiteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len = 0;
      return UTF8ToString(UnsafeNativeMethods.sqlite3_column_table_name_interop(stmt._sqlite_stmt, index, ref len), len);
#else
      return UTF8ToString(UnsafeNativeMethods.sqlite3_column_table_name(stmt._sqlite_stmt, index), -1);
#endif
    }

    internal override bool DoesTableExist(
        string dataBase,
        string table
        )
    {
        string dataType = null; /* NOT USED */
        string collateSequence = null; /* NOT USED */
        bool notNull = false; /* NOT USED */
        bool primaryKey = false; /* NOT USED */
        bool autoIncrement = false; /* NOT USED */

        return ColumnMetaData(
            dataBase, table, null, false, ref dataType,
            ref collateSequence, ref notNull, ref primaryKey,
            ref autoIncrement);
    }

    internal override bool ColumnMetaData(string dataBase, string table, string column, bool canThrow, ref string dataType, ref string collateSequence, ref bool notNull, ref bool primaryKey, ref bool autoIncrement)
    {
      IntPtr dataTypePtr = IntPtr.Zero;
      IntPtr collSeqPtr = IntPtr.Zero;
      int nnotNull = 0;
      int nprimaryKey = 0;
      int nautoInc = 0;
      SQLiteErrorCode n;
      int dtLen;
      int csLen;

#if !SQLITE_STANDARD
      dtLen = 0;
      csLen = 0;
      n = UnsafeNativeMethods.sqlite3_table_column_metadata_interop(_sql, ToUTF8(dataBase), ToUTF8(table), ToUTF8(column), ref dataTypePtr, ref collSeqPtr, ref nnotNull, ref nprimaryKey, ref nautoInc, ref dtLen, ref csLen);
#else
      dtLen = -1;
      csLen = -1;

      n = UnsafeNativeMethods.sqlite3_table_column_metadata(_sql, ToUTF8(dataBase), ToUTF8(table), ToUTF8(column), ref dataTypePtr, ref collSeqPtr, ref nnotNull, ref nprimaryKey, ref nautoInc);
#endif
      if (canThrow && (n != SQLiteErrorCode.Ok)) throw new SQLiteException(n, GetLastError());

      dataType = UTF8ToString(dataTypePtr, dtLen);
      collateSequence = UTF8ToString(collSeqPtr, csLen);

      notNull = (nnotNull == 1);
      primaryKey = (nprimaryKey == 1);
      autoIncrement = (nautoInc == 1);

      return (n == SQLiteErrorCode.Ok);
    }

    internal override object GetObject(SQLiteStatement stmt, int index)
    {
        switch (ColumnAffinity(stmt, index))
        {
            case TypeAffinity.Int64:
                {
                    return GetInt64(stmt, index);
                }
            case TypeAffinity.Double:
                {
                    return GetDouble(stmt, index);
                }
            case TypeAffinity.Text:
                {
                    return GetText(stmt, index);
                }
            case TypeAffinity.Blob:
                {
                    long size = GetBytes(stmt, index, 0, null, 0, 0);

                    if ((size > 0) && (size <= int.MaxValue))
                    {
                        byte[] bytes = new byte[(int)size];

                        GetBytes(stmt, index, 0, bytes, 0, (int)size);

                        return bytes;
                    }
                    break;
                }
            case TypeAffinity.Null:
                {
                    return DBNull.Value;
                }
        }

        throw new NotImplementedException();
    }

    internal override double GetDouble(SQLiteStatement stmt, int index)
    {
#if !PLATFORM_COMPACTFRAMEWORK
      return UnsafeNativeMethods.sqlite3_column_double(stmt._sqlite_stmt, index);
#elif !SQLITE_STANDARD
      double value = 0.0;
      UnsafeNativeMethods.sqlite3_column_double_interop(stmt._sqlite_stmt, index, ref value);
      return value;
#else
      throw new NotImplementedException();
#endif
    }

    internal override bool GetBoolean(SQLiteStatement stmt, int index)
    {
      return ToBoolean(GetObject(stmt, index), CultureInfo.InvariantCulture, false);
    }

    internal override sbyte GetSByte(SQLiteStatement stmt, int index)
    {
      return unchecked((sbyte)(GetInt32(stmt, index) & byte.MaxValue));
    }

    internal override byte GetByte(SQLiteStatement stmt, int index)
    {
      return unchecked((byte)(GetInt32(stmt, index) & byte.MaxValue));
    }

    internal override short GetInt16(SQLiteStatement stmt, int index)
    {
      return unchecked((short)(GetInt32(stmt, index) & ushort.MaxValue));
    }

    internal override ushort GetUInt16(SQLiteStatement stmt, int index)
    {
      return unchecked((ushort)(GetInt32(stmt, index) & ushort.MaxValue));
    }

    internal override int GetInt32(SQLiteStatement stmt, int index)
    {
      return UnsafeNativeMethods.sqlite3_column_int(stmt._sqlite_stmt, index);
    }

    internal override uint GetUInt32(SQLiteStatement stmt, int index)
    {
      return unchecked((uint)GetInt32(stmt, index));
    }

    internal override long GetInt64(SQLiteStatement stmt, int index)
    {
#if !PLATFORM_COMPACTFRAMEWORK
      return UnsafeNativeMethods.sqlite3_column_int64(stmt._sqlite_stmt, index);
#elif !SQLITE_STANDARD
      long value = 0;
      UnsafeNativeMethods.sqlite3_column_int64_interop(stmt._sqlite_stmt, index, ref value);
      return value;
#else
      throw new NotImplementedException();
#endif
    }

    internal override ulong GetUInt64(SQLiteStatement stmt, int index)
    {
      return unchecked((ulong)GetInt64(stmt, index));
    }

    internal override string GetText(SQLiteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len = 0;
      return UTF8ToString(UnsafeNativeMethods.sqlite3_column_text_interop(stmt._sqlite_stmt, index, ref len), len);
#else
      return UTF8ToString(UnsafeNativeMethods.sqlite3_column_text(stmt._sqlite_stmt, index),
        UnsafeNativeMethods.sqlite3_column_bytes(stmt._sqlite_stmt, index));
#endif
    }

    internal override DateTime GetDateTime(SQLiteStatement stmt, int index)
    {
      if (_datetimeFormat == SQLiteDateFormats.Ticks)
        return TicksToDateTime(GetInt64(stmt, index), _datetimeKind);
      else if (_datetimeFormat == SQLiteDateFormats.JulianDay)
        return ToDateTime(GetDouble(stmt, index), _datetimeKind);
      else if (_datetimeFormat == SQLiteDateFormats.UnixEpoch)
        return UnixEpochToDateTime(GetInt64(stmt, index), _datetimeKind);

#if !SQLITE_STANDARD
      int len = 0;
      return ToDateTime(UnsafeNativeMethods.sqlite3_column_text_interop(stmt._sqlite_stmt, index, ref len), len);
#else
      return ToDateTime(UnsafeNativeMethods.sqlite3_column_text(stmt._sqlite_stmt, index),
        UnsafeNativeMethods.sqlite3_column_bytes(stmt._sqlite_stmt, index));
#endif
    }

    internal override long GetBytes(SQLiteStatement stmt, int index, int nDataOffset, byte[] bDest, int nStart, int nLength)
    {
      int nlen = UnsafeNativeMethods.sqlite3_column_bytes(stmt._sqlite_stmt, index);

      // If no destination buffer, return the size needed.
      if (bDest == null) return nlen;

      int nCopied = nLength;

      if (nCopied + nStart > bDest.Length) nCopied = bDest.Length - nStart;
      if (nCopied + nDataOffset > nlen) nCopied = nlen - nDataOffset;

      if (nCopied > 0)
      {
        IntPtr ptr = UnsafeNativeMethods.sqlite3_column_blob(stmt._sqlite_stmt, index);

        Marshal.Copy((IntPtr)(ptr.ToInt64() + nDataOffset), bDest, nStart, nCopied);
      }
      else
      {
        nCopied = 0;
      }

      return nCopied;
    }

    internal override char GetChar(SQLiteStatement stmt, int index)
    {
      return Convert.ToChar(GetUInt16(stmt, index));
    }

    internal override long GetChars(SQLiteStatement stmt, int index, int nDataOffset, char[] bDest, int nStart, int nLength)
    {
      int nlen;
      int nCopied = nLength;

      string str = GetText(stmt, index);
      nlen = str.Length;

      if (bDest == null) return nlen;

      if (nCopied + nStart > bDest.Length) nCopied = bDest.Length - nStart;
      if (nCopied + nDataOffset > nlen) nCopied = nlen - nDataOffset;

      if (nCopied > 0)
        str.CopyTo(nDataOffset, bDest, nStart, nCopied);
      else nCopied = 0;

      return nCopied;
    }

    internal override bool IsNull(SQLiteStatement stmt, int index)
    {
      return (ColumnAffinity(stmt, index) == TypeAffinity.Null);
    }

    internal override int AggregateCount(IntPtr context)
    {
      return UnsafeNativeMethods.sqlite3_aggregate_count(context);
    }

    internal override SQLiteErrorCode CreateFunction(string strFunction, int nArgs, bool needCollSeq, SQLiteCallback func, SQLiteCallback funcstep, SQLiteFinalCallback funcfinal, bool canThrow)
    {
      SQLiteErrorCode n;

#if !SQLITE_STANDARD
      n = UnsafeNativeMethods.sqlite3_create_function_interop(_sql, ToUTF8(strFunction), nArgs, 4, IntPtr.Zero, func, funcstep, funcfinal, (needCollSeq == true) ? 1 : 0);
      if (n == SQLiteErrorCode.Ok) n = UnsafeNativeMethods.sqlite3_create_function_interop(_sql, ToUTF8(strFunction), nArgs, 1, IntPtr.Zero, func, funcstep, funcfinal, (needCollSeq == true) ? 1 : 0);
#else
      n = UnsafeNativeMethods.sqlite3_create_function(_sql, ToUTF8(strFunction), nArgs, 4, IntPtr.Zero, func, funcstep, funcfinal);
      if (n == SQLiteErrorCode.Ok) n = UnsafeNativeMethods.sqlite3_create_function(_sql, ToUTF8(strFunction), nArgs, 1, IntPtr.Zero, func, funcstep, funcfinal);
#endif
      if (canThrow && (n != SQLiteErrorCode.Ok)) throw new SQLiteException(n, GetLastError());
      return n;
    }

    internal override SQLiteErrorCode CreateCollation(string strCollation, SQLiteCollation func, SQLiteCollation func16, bool canThrow)
    {
      SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_create_collation(_sql, ToUTF8(strCollation), 2, IntPtr.Zero, func16);
      if (n == SQLiteErrorCode.Ok) n = UnsafeNativeMethods.sqlite3_create_collation(_sql, ToUTF8(strCollation), 1, IntPtr.Zero, func);
      if (canThrow && (n != SQLiteErrorCode.Ok)) throw new SQLiteException(n, GetLastError());
      return n;
    }

    internal override int ContextCollateCompare(CollationEncodingEnum enc, IntPtr context, string s1, string s2)
    {
#if !SQLITE_STANDARD
      byte[] b1;
      byte[] b2;
      System.Text.Encoding converter = null;

      switch (enc)
      {
        case CollationEncodingEnum.UTF8:
          converter = System.Text.Encoding.UTF8;
          break;
        case CollationEncodingEnum.UTF16LE:
          converter = System.Text.Encoding.Unicode;
          break;
        case CollationEncodingEnum.UTF16BE:
          converter = System.Text.Encoding.BigEndianUnicode;
          break;
      }

      b1 = converter.GetBytes(s1);
      b2 = converter.GetBytes(s2);

      return UnsafeNativeMethods.sqlite3_context_collcompare_interop(context, b1, b1.Length, b2, b2.Length);
#else
      throw new NotImplementedException();
#endif
    }

    internal override int ContextCollateCompare(CollationEncodingEnum enc, IntPtr context, char[] c1, char[] c2)
    {
#if !SQLITE_STANDARD
      byte[] b1;
      byte[] b2;
      System.Text.Encoding converter = null;

      switch (enc)
      {
        case CollationEncodingEnum.UTF8:
          converter = System.Text.Encoding.UTF8;
          break;
        case CollationEncodingEnum.UTF16LE:
          converter = System.Text.Encoding.Unicode;
          break;
        case CollationEncodingEnum.UTF16BE:
          converter = System.Text.Encoding.BigEndianUnicode;
          break;
      }

      b1 = converter.GetBytes(c1);
      b2 = converter.GetBytes(c2);

      return UnsafeNativeMethods.sqlite3_context_collcompare_interop(context, b1, b1.Length, b2, b2.Length);
#else
      throw new NotImplementedException();
#endif
    }

    internal override CollationSequence GetCollationSequence(SQLiteFunction func, IntPtr context)
    {
#if !SQLITE_STANDARD
      CollationSequence seq = new CollationSequence();
      int len = 0;
      int type = 0;
      int enc = 0;
      IntPtr p = UnsafeNativeMethods.sqlite3_context_collseq_interop(context, ref type, ref enc, ref len);

      if (p != null) seq.Name = UTF8ToString(p, len);
      seq.Type = (CollationTypeEnum)type;
      seq._func = func;
      seq.Encoding = (CollationEncodingEnum)enc;

      return seq;
#else
      throw new NotImplementedException();
#endif
    }

    internal override long GetParamValueBytes(IntPtr p, int nDataOffset, byte[] bDest, int nStart, int nLength)
    {
      int nlen = UnsafeNativeMethods.sqlite3_value_bytes(p);

      // If no destination buffer, return the size needed.
      if (bDest == null) return nlen;

      int nCopied = nLength;

      if (nCopied + nStart > bDest.Length) nCopied = bDest.Length - nStart;
      if (nCopied + nDataOffset > nlen) nCopied = nlen - nDataOffset;

      if (nCopied > 0)
      {
        IntPtr ptr = UnsafeNativeMethods.sqlite3_value_blob(p);

        Marshal.Copy((IntPtr)(ptr.ToInt64() + nDataOffset), bDest, nStart, nCopied);
      }
      else
      {
        nCopied = 0;
      }

      return nCopied;
    }

    internal override double GetParamValueDouble(IntPtr ptr)
    {
#if !PLATFORM_COMPACTFRAMEWORK
      return UnsafeNativeMethods.sqlite3_value_double(ptr);
#elif !SQLITE_STANDARD
      double value = 0.0;
      UnsafeNativeMethods.sqlite3_value_double_interop(ptr, ref value);
      return value;
#else
      throw new NotImplementedException();
#endif
    }

    internal override int GetParamValueInt32(IntPtr ptr)
    {
      return UnsafeNativeMethods.sqlite3_value_int(ptr);
    }

    internal override long GetParamValueInt64(IntPtr ptr)
    {
#if !PLATFORM_COMPACTFRAMEWORK
      return UnsafeNativeMethods.sqlite3_value_int64(ptr);
#elif !SQLITE_STANDARD
      Int64 value = 0;
      UnsafeNativeMethods.sqlite3_value_int64_interop(ptr, ref value);
      return value;
#else
      throw new NotImplementedException();
#endif
    }

    internal override string GetParamValueText(IntPtr ptr)
    {
#if !SQLITE_STANDARD
      int len = 0;
      return UTF8ToString(UnsafeNativeMethods.sqlite3_value_text_interop(ptr, ref len), len);
#else
      return UTF8ToString(UnsafeNativeMethods.sqlite3_value_text(ptr),
        UnsafeNativeMethods.sqlite3_value_bytes(ptr));
#endif
    }

    internal override TypeAffinity GetParamValueType(IntPtr ptr)
    {
      return UnsafeNativeMethods.sqlite3_value_type(ptr);
    }

    internal override void ReturnBlob(IntPtr context, byte[] value)
    {
      UnsafeNativeMethods.sqlite3_result_blob(context, value, value.Length, (IntPtr)(-1));
    }

    internal override void ReturnDouble(IntPtr context, double value)
    {
#if !PLATFORM_COMPACTFRAMEWORK
      UnsafeNativeMethods.sqlite3_result_double(context, value);
#elif !SQLITE_STANDARD
      UnsafeNativeMethods.sqlite3_result_double_interop(context, ref value);
#else
      throw new NotImplementedException();
#endif
    }

    internal override void ReturnError(IntPtr context, string value)
    {
      UnsafeNativeMethods.sqlite3_result_error(context, ToUTF8(value), value.Length);
    }

    internal override void ReturnInt32(IntPtr context, int value)
    {
      UnsafeNativeMethods.sqlite3_result_int(context, value);
    }

    internal override void ReturnInt64(IntPtr context, long value)
    {
#if !PLATFORM_COMPACTFRAMEWORK
      UnsafeNativeMethods.sqlite3_result_int64(context, value);
#elif !SQLITE_STANDARD
      UnsafeNativeMethods.sqlite3_result_int64_interop(context, ref value);
#else
      throw new NotImplementedException();
#endif
    }

    internal override void ReturnNull(IntPtr context)
    {
      UnsafeNativeMethods.sqlite3_result_null(context);
    }

    internal override void ReturnText(IntPtr context, string value)
    {
      byte[] b = ToUTF8(value);
      UnsafeNativeMethods.sqlite3_result_text(context, ToUTF8(value), b.Length - 1, (IntPtr)(-1));
    }

#if INTEROP_VIRTUAL_TABLE
    /// <summary>
    /// Determines the file name of the native library containing the native
    /// "vtshim" extension -AND- whether it should be dynamically loaded by
    /// this class.
    /// </summary>
    /// <param name="isLoadNeeded">
    /// This output parameter will be set to non-zero if the returned native
    /// library file name should be dynamically loaded prior to attempting
    /// the creation of native disposable extension modules.
    /// </param>
    /// <returns>
    /// The file name of the native library containing the native "vtshim"
    /// extension -OR- null if it cannot be determined.
    /// </returns>
    private string GetShimExtensionFileName(
        ref bool isLoadNeeded /* out */
        )
    {
        if (_shimIsLoadNeeded != null)
            isLoadNeeded = (bool)_shimIsLoadNeeded;
        else
#if SQLITE_STANDARD || USE_INTEROP_DLL || PLATFORM_COMPACTFRAMEWORK
            isLoadNeeded = HelperMethods.IsWindows(); /* COMPAT */
#else
            isLoadNeeded = false; /* mixed-mode assembly */
#endif

        string fileName = _shimExtensionFileName;

        if (fileName != null)
            return fileName;

#if (SQLITE_STANDARD || USE_INTEROP_DLL || PLATFORM_COMPACTFRAMEWORK) && PRELOAD_NATIVE_LIBRARY
        return UnsafeNativeMethods.GetNativeLibraryFileNameOnly(); /* COMPAT */
#else
        return null;
#endif
    }

    /// <summary>
    /// Calls the native SQLite core library in order to create a disposable
    /// module containing the implementation of a virtual table.
    /// </summary>
    /// <param name="module">
    /// The module object to be used when creating the native disposable module.
    /// </param>
    /// <param name="flags">
    /// The flags for the associated <see cref="SQLiteConnection" /> object instance.
    /// </param>
    internal override void CreateModule(SQLiteModule module, SQLiteConnectionFlags flags)
    {
        if (module == null)
            throw new ArgumentNullException("module");

        if (HelperMethods.NoLogModule(flags))
        {
            module.LogErrors = HelperMethods.LogModuleError(flags);
            module.LogExceptions = HelperMethods.LogModuleException(flags);
        }

        if (_sql == null)
            throw new SQLiteException("connection has an invalid handle");

        bool isLoadNeeded = false;
        string fileName = GetShimExtensionFileName(ref isLoadNeeded);

        if (isLoadNeeded)
        {
            if (fileName == null)
                throw new SQLiteException("the file name for the \"vtshim\" extension is unknown");

            if (_shimExtensionProcName == null)
                throw new SQLiteException("the entry point for the \"vtshim\" extension is unknown");

            SetLoadExtension(true);
            LoadExtension(fileName, _shimExtensionProcName);
        }

        if (module.CreateDisposableModule(_sql))
        {
            if (_modules == null)
                _modules = new Dictionary<string, SQLiteModule>();

            _modules.Add(module.Name, module);

            if (_usePool)
            {
                _usePool = false;

#if !NET_COMPACT_20 && TRACE_CONNECTION
                Trace.WriteLine(HelperMethods.StringFormat(
                    CultureInfo.CurrentCulture,
                    "CreateModule (Pool) Disabled: {0}",
                    HandleToString()));
#endif
            }
        }
        else
        {
            throw new SQLiteException(GetLastError());
        }
    }

    /// <summary>
    /// Calls the native SQLite core library in order to cleanup the resources
    /// associated with a module containing the implementation of a virtual table.
    /// </summary>
    /// <param name="module">
    /// The module object previously passed to the <see cref="CreateModule" />
    /// method.
    /// </param>
    /// <param name="flags">
    /// The flags for the associated <see cref="SQLiteConnection" /> object instance.
    /// </param>
    internal override void DisposeModule(SQLiteModule module, SQLiteConnectionFlags flags)
    {
        if (module == null)
            throw new ArgumentNullException("module");

        module.Dispose();
    }
#endif

    internal override IntPtr AggregateContext(IntPtr context)
    {
      return UnsafeNativeMethods.sqlite3_aggregate_context(context, 1);
    }

#if INTEROP_VIRTUAL_TABLE
    /// <summary>
    /// Calls the native SQLite core library in order to declare a virtual table
    /// in response to a call into the <see cref="ISQLiteNativeModule.xCreate" />
    /// or <see cref="ISQLiteNativeModule.xConnect" /> virtual table methods.
    /// </summary>
    /// <param name="module">
    /// The virtual table module that is to be responsible for the virtual table
    /// being declared.
    /// </param>
    /// <param name="strSql">
    /// The string containing the SQL statement describing the virtual table to
    /// be declared.
    /// </param>
    /// <param name="error">
    /// Upon success, the contents of this parameter are undefined.  Upon failure,
    /// it should contain an appropriate error message.
    /// </param>
    /// <returns>
    /// A standard SQLite return code.
    /// </returns>
    internal override SQLiteErrorCode DeclareVirtualTable(
        SQLiteModule module,
        string strSql,
        ref string error
        )
    {
        if (_sql == null)
        {
            error = "connection has an invalid handle";
            return SQLiteErrorCode.Error;
        }

        IntPtr pSql = IntPtr.Zero;

        try
        {
            pSql = SQLiteString.Utf8IntPtrFromString(strSql);

            SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_declare_vtab(
                _sql, pSql);

            if ((n == SQLiteErrorCode.Ok) && (module != null))
                module.Declared = true;

            if (n != SQLiteErrorCode.Ok) error = GetLastError();

            return n;
        }
        finally
        {
            if (pSql != IntPtr.Zero)
            {
                SQLiteMemory.Free(pSql);
                pSql = IntPtr.Zero;
            }
        }
    }

    /// <summary>
    /// Calls the native SQLite core library in order to declare a virtual table
    /// function in response to a call into the <see cref="ISQLiteNativeModule.xCreate" />
    /// or <see cref="ISQLiteNativeModule.xConnect" /> virtual table methods.
    /// </summary>
    /// <param name="module">
    /// The virtual table module that is to be responsible for the virtual table
    /// function being declared.
    /// </param>
    /// <param name="argumentCount">
    /// The number of arguments to the function being declared.
    /// </param>
    /// <param name="name">
    /// The name of the function being declared.
    /// </param>
    /// <param name="error">
    /// Upon success, the contents of this parameter are undefined.  Upon failure,
    /// it should contain an appropriate error message.
    /// </param>
    /// <returns>
    /// A standard SQLite return code.
    /// </returns>
    internal override SQLiteErrorCode DeclareVirtualFunction(
        SQLiteModule module,
        int argumentCount,
        string name,
        ref string error
        )
    {
        if (_sql == null)
        {
            error = "connection has an invalid handle";
            return SQLiteErrorCode.Error;
        }

        IntPtr pName = IntPtr.Zero;

        try
        {
            pName = SQLiteString.Utf8IntPtrFromString(name);

            SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_overload_function(
                _sql, pName, argumentCount);

            if (n != SQLiteErrorCode.Ok) error = GetLastError();

            return n;
        }
        finally
        {
            if (pName != IntPtr.Zero)
            {
                SQLiteMemory.Free(pName);
                pName = IntPtr.Zero;
            }
        }
    }
#endif

    /// <summary>
    /// Builds an error message string fragment containing the
    /// defined values of the <see cref="SQLiteStatusOpsEnum" />
    /// enumeration.
    /// </summary>
    /// <returns>
    /// The built string fragment.
    /// </returns>
    private static string GetStatusDbOpsNames()
    {
        StringBuilder builder = new StringBuilder();

#if !PLATFORM_COMPACTFRAMEWORK
        foreach (string name in Enum.GetNames(
                typeof(SQLiteStatusOpsEnum)))
        {
            if (String.IsNullOrEmpty(name))
                continue;

            if (builder.Length > 0)
                builder.Append(", ");

            builder.Append(name);
        }
#else
        //
        // TODO: Update this list if the available values in the
        //       "SQLiteConfigDbOpsEnum" enumeration change.
        //
        builder.AppendFormat(CultureInfo.InvariantCulture,
            "{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}",
            SQLiteStatusOpsEnum.SQLITE_DBSTATUS_LOOKASIDE_USED,
            SQLiteStatusOpsEnum.SQLITE_DBSTATUS_CACHE_USED,
            SQLiteStatusOpsEnum.SQLITE_DBSTATUS_SCHEMA_USED,
            SQLiteStatusOpsEnum.SQLITE_DBSTATUS_STMT_USED,
            SQLiteStatusOpsEnum.SQLITE_DBSTATUS_LOOKASIDE_HIT,
            SQLiteStatusOpsEnum.SQLITE_DBSTATUS_LOOKASIDE_MISS_SIZE,
            SQLiteStatusOpsEnum.SQLITE_DBSTATUS_LOOKASIDE_MISS_FULL,
            SQLiteStatusOpsEnum.SQLITE_DBSTATUS_CACHE_HIT,
            SQLiteStatusOpsEnum.SQLITE_DBSTATUS_CACHE_MISS,
            SQLiteStatusOpsEnum.SQLITE_DBSTATUS_CACHE_WRITE,
            SQLiteStatusOpsEnum.SQLITE_DBSTATUS_DEFERRED_FKS,
            SQLiteStatusOpsEnum.SQLITE_DBSTATUS_CACHE_USED_SHARED);
#endif

        return builder.ToString();
    }

    /// <summary>
    /// Builds an error message string fragment containing the
    /// defined values of the <see cref="SQLiteLimitOpsEnum" />
    /// enumeration.
    /// </summary>
    /// <returns>
    /// The built string fragment.
    /// </returns>
    private static string GetLimitOpsNames()
    {
        StringBuilder builder = new StringBuilder();

#if !PLATFORM_COMPACTFRAMEWORK
        foreach (string name in Enum.GetNames(
                typeof(SQLiteLimitOpsEnum)))
        {
            if (String.IsNullOrEmpty(name))
                continue;

            if (builder.Length > 0)
                builder.Append(", ");

            builder.Append(name);
        }
#else
        //
        // TODO: Update this list if the available values in the
        //       "SQLiteLimitOpsEnum" enumeration change.
        //
        builder.AppendFormat(CultureInfo.InvariantCulture,
            "{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}",
            SQLiteLimitOpsEnum.SQLITE_LIMIT_LENGTH,
            SQLiteLimitOpsEnum.SQLITE_LIMIT_SQL_LENGTH,
            SQLiteLimitOpsEnum.SQLITE_LIMIT_COLUMN,
            SQLiteLimitOpsEnum.SQLITE_LIMIT_EXPR_DEPTH,
            SQLiteLimitOpsEnum.SQLITE_LIMIT_COMPOUND_SELECT,
            SQLiteLimitOpsEnum.SQLITE_LIMIT_VDBE_OP,
            SQLiteLimitOpsEnum.SQLITE_LIMIT_FUNCTION_ARG,
            SQLiteLimitOpsEnum.SQLITE_LIMIT_ATTACHED,
            SQLiteLimitOpsEnum.SQLITE_LIMIT_LIKE_PATTERN_LENGTH,
            SQLiteLimitOpsEnum.SQLITE_LIMIT_VARIABLE_NUMBER,
            SQLiteLimitOpsEnum.SQLITE_LIMIT_TRIGGER_DEPTH,
            SQLiteLimitOpsEnum.SQLITE_LIMIT_WORKER_THREADS);
#endif

        return builder.ToString();
    }

    /// <summary>
    /// Builds an error message string fragment containing the
    /// defined values of the <see cref="SQLiteConfigDbOpsEnum" />
    /// enumeration.
    /// </summary>
    /// <returns>
    /// The built string fragment.
    /// </returns>
    private static string GetConfigDbOpsNames()
    {
        StringBuilder builder = new StringBuilder();

#if !PLATFORM_COMPACTFRAMEWORK
        foreach (string name in Enum.GetNames(
                typeof(SQLiteConfigDbOpsEnum)))
        {
            if (String.IsNullOrEmpty(name))
                continue;

            if (builder.Length > 0)
                builder.Append(", ");

            builder.Append(name);
        }
#else
        //
        // TODO: Update this list if the available values in the
        //       "SQLiteConfigDbOpsEnum" enumeration change.
        //
        builder.AppendFormat(CultureInfo.InvariantCulture,
            "{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, " +
            "{9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}",
            SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_NONE,
            SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_MAINDBNAME,
            SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_LOOKASIDE,
            SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_FKEY,
            SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_TRIGGER,
            SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_FTS3_TOKENIZER,
            SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION,
            SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_NO_CKPT_ON_CLOSE,
            SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_QPSG,
            SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_TRIGGER_EQP,
            SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_RESET_DATABASE,
            SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_DEFENSIVE,
            SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_WRITABLE_SCHEMA,
            SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_LEGACY_ALTER_TABLE,
            SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_DQS_DML,
            SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_DQS_DDL,
            SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_VIEW);
#endif

        return builder.ToString();
    }

    /// <summary>
    /// Returns the current and/or highwater values for the specified
    /// database status parameter.
    /// </summary>
    /// <param name="option">
    /// The database status parameter to query.
    /// </param>
    /// <param name="reset">
    /// Non-zero to reset the highwater value to the current value.
    /// </param>
    /// <param name="current">
    /// If applicable, receives the current value.
    /// </param>
    /// <param name="highwater">
    /// If applicable, receives the highwater value.
    /// </param>
    /// <returns>
    /// A standard SQLite return code.
    /// </returns>
    internal override SQLiteErrorCode GetStatusParameter(
        SQLiteStatusOpsEnum option,
        bool reset,
        ref int current,
        ref int highwater
        )
    {
        if (!Enum.IsDefined(typeof(SQLiteStatusOpsEnum), option))
        {
            throw new SQLiteException(HelperMethods.StringFormat(
                CultureInfo.CurrentCulture,
                "unrecognized status option, must be: {0}",
                GetStatusDbOpsNames()));
        }

        return UnsafeNativeMethods.sqlite3_db_status(
            _sql, option, ref current, ref highwater, reset ? 1 : 0);
    }

    /// <summary>
    /// Change a limit value for the database.
    /// </summary>
    /// <param name="option">
    /// The database limit to change.
    /// </param>
    /// <param name="value">
    /// The new value for the specified limit.
    /// </param>
    /// <returns>
    /// The old value for the specified limit -OR- negative one if an error
    /// occurs.
    /// </returns>
    internal override int SetLimitOption(
        SQLiteLimitOpsEnum option,
        int value
        )
    {
        if (!Enum.IsDefined(typeof(SQLiteLimitOpsEnum), option))
        {
            throw new SQLiteException(HelperMethods.StringFormat(
                CultureInfo.CurrentCulture,
                "unrecognized limit option, must be: {0}",
                GetLimitOpsNames()));
        }

        return UnsafeNativeMethods.sqlite3_limit(_sql, option, value);
    }

    /// <summary>
    /// Change a configuration option value for the database.
    /// </summary>
    /// <param name="option">
    /// The database configuration option to change.
    /// </param>
    /// <param name="value">
    /// The new value for the specified configuration option.
    /// </param>
    /// <returns>
    /// A standard SQLite return code.
    /// </returns>
    internal override SQLiteErrorCode SetConfigurationOption(
        SQLiteConfigDbOpsEnum option,
        object value
        )
    {
        if (!Enum.IsDefined(typeof(SQLiteConfigDbOpsEnum), option))
        {
            throw new SQLiteException(HelperMethods.StringFormat(
                CultureInfo.CurrentCulture,
                "unrecognized configuration option, must be: {0}",
                GetConfigDbOpsNames()));
        }

        switch (option)
        {
            case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_NONE: // nil
                {
                    //
                    // NOTE: Do nothing, return success.
                    //
                    return SQLiteErrorCode.Ok;
                }
            case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_MAINDBNAME: // char*
                {
                    if (value == null)
                        throw new ArgumentNullException("value");

                    if (!(value is string))
                    {
                        throw new SQLiteException(HelperMethods.StringFormat(
                            CultureInfo.CurrentCulture,
                            "configuration value type mismatch, must be of type {0}",
                            typeof(string)));
                    }

                    SQLiteErrorCode rc = SQLiteErrorCode.Error;
                    IntPtr pDbName = IntPtr.Zero;

                    try
                    {
                        pDbName = SQLiteString.Utf8IntPtrFromString(
                            (string)value);

                        if (pDbName == IntPtr.Zero)
                        {
                            throw new SQLiteException(
                                SQLiteErrorCode.NoMem,
                                "cannot allocate database name");
                        }

                        rc = UnsafeNativeMethods.sqlite3_db_config_charptr(
                            _sql, option, pDbName);

                        if (rc == SQLiteErrorCode.Ok)
                        {
                            FreeDbName(true);

                            dbName = pDbName;
                            pDbName = IntPtr.Zero;
                        }
                    }
                    finally
                    {
                        if ((rc != SQLiteErrorCode.Ok) &&
                            (pDbName != IntPtr.Zero))
                        {
                            SQLiteMemory.Free(pDbName);
                            pDbName = IntPtr.Zero;
                        }
                    }

                    return rc;
                }
            case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_LOOKASIDE: // void* int int
                {
                    object[] array = value as object[];

                    if (array == null)
                    {
                        throw new SQLiteException(HelperMethods.StringFormat(
                            CultureInfo.CurrentCulture,
                            "configuration value type mismatch, must be of type {0}",
                            typeof(object[])));
                    }

                    if (!(array[0] is IntPtr))
                    {
                        throw new SQLiteException(HelperMethods.StringFormat(
                            CultureInfo.CurrentCulture,
                            "configuration element zero (0) type mismatch, must be of type {0}",
                            typeof(IntPtr)));
                    }

                    if (!(array[1] is int))
                    {
                        throw new SQLiteException(HelperMethods.StringFormat(
                            CultureInfo.CurrentCulture,
                            "configuration element one (1) type mismatch, must be of type {0}",
                            typeof(int)));
                    }

                    if (!(array[2] is int))
                    {
                        throw new SQLiteException(HelperMethods.StringFormat(
                            CultureInfo.CurrentCulture,
                            "configuration element two (2) type mismatch, must be of type {0}",
                            typeof(int)));
                    }

                    return UnsafeNativeMethods.sqlite3_db_config_intptr_two_ints(
                        _sql, option, (IntPtr)array[0], (int)array[1], (int)array[2]);
                }
            case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_FKEY: // int int*
            case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_TRIGGER: // int int*
            case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_FTS3_TOKENIZER: // int int*
            case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION: // int int*
            case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_NO_CKPT_ON_CLOSE: // int int*
            case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_QPSG: // int int*
            case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_TRIGGER_EQP: // int int*
            case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_RESET_DATABASE: // int int*
            case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_DEFENSIVE: // int int*
            case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_WRITABLE_SCHEMA: // int int*
            case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_LEGACY_ALTER_TABLE: // int int*
            case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_DQS_DML: // int int*
            case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_DQS_DDL: // int int*
            case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_VIEW: // int int*
                {
                    if (!(value is bool))
                    {
                        throw new SQLiteException(HelperMethods.StringFormat(
                            CultureInfo.CurrentCulture,
                            "configuration value type mismatch, must be of type {0}",
                            typeof(bool)));
                    }

                    int result = 0; /* NOT USED */

                    return UnsafeNativeMethods.sqlite3_db_config_int_refint(
                        _sql, option, ((bool)value ? 1 : 0), ref result);
                }
            default:
                {
                    throw new SQLiteException(HelperMethods.StringFormat(
                        CultureInfo.CurrentCulture,
                        "unsupported configuration option {0}", option));
                }
        }
    }

    /// <summary>
    /// Enables or disables extension loading by SQLite.
    /// </summary>
    /// <param name="bOnOff">
    /// True to enable loading of extensions, false to disable.
    /// </param>
    internal override void SetLoadExtension(bool bOnOff)
    {
        SQLiteErrorCode n;

        if (SQLiteVersionNumber >= 3013000)
        {
            n = SetConfigurationOption(
                SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION,
                bOnOff);
        }
        else
        {
            n = UnsafeNativeMethods.sqlite3_enable_load_extension(
                _sql, (bOnOff ? -1 : 0));
        }

        if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
    }

    /// <summary>
    /// Loads a SQLite extension library from the named file.
    /// </summary>
    /// <param name="fileName">
    /// The name of the dynamic link library file containing the extension.
    /// </param>
    /// <param name="procName">
    /// The name of the exported function used to initialize the extension.
    /// If null, the default "sqlite3_extension_init" will be used.
    /// </param>
    internal override void LoadExtension(string fileName, string procName)
    {
        if (fileName == null)
            throw new ArgumentNullException("fileName");

        IntPtr pError = IntPtr.Zero;

        try
        {
            byte[] utf8FileName = UTF8Encoding.UTF8.GetBytes(fileName + '\0');
            byte[] utf8ProcName = null;

            if (procName != null)
                utf8ProcName = UTF8Encoding.UTF8.GetBytes(procName + '\0');

            SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_load_extension(
                _sql, utf8FileName, utf8ProcName, ref pError);

            if (n != SQLiteErrorCode.Ok)
                throw new SQLiteException(n, UTF8ToString(pError, -1));
        }
        finally
        {
            if (pError != IntPtr.Zero)
            {
                UnsafeNativeMethods.sqlite3_free(pError);
                pError = IntPtr.Zero;
            }
        }
    }

    /// Enables or disables extended result codes returned by SQLite
    internal override void SetExtendedResultCodes(bool bOnOff)
    {
      SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_extended_result_codes(
          _sql, (bOnOff ? -1 : 0));

      if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
    }
    /// Gets the last SQLite error code
    internal override SQLiteErrorCode ResultCode()
    {
      return UnsafeNativeMethods.sqlite3_errcode(_sql);
    }
    /// Gets the last SQLite extended error code
    internal override SQLiteErrorCode ExtendedResultCode()
    {
      return UnsafeNativeMethods.sqlite3_extended_errcode(_sql);
    }

    /// Add a log message via the SQLite sqlite3_log interface.
    internal override void LogMessage(SQLiteErrorCode iErrCode, string zMessage)
    {
      StaticLogMessage(iErrCode, zMessage);
    }

    /// Add a log message via the SQLite sqlite3_log interface.
    internal static void StaticLogMessage(SQLiteErrorCode iErrCode, string zMessage)
    {
      UnsafeNativeMethods.sqlite3_log(iErrCode, ToUTF8(zMessage));
    }

#if INTEROP_CODEC || INTEROP_INCLUDE_SEE
    private static void ZeroPassword(byte[] passwordBytes)
    {
        if (passwordBytes == null) return;

        for (int index = 0; index < passwordBytes.Length; index++)
        {
            byte value = (byte)((index + 1) % byte.MaxValue);

            passwordBytes[index] = value;
            passwordBytes[index] ^= value;
        }
    }

    internal override void SetPassword(byte[] passwordBytes)
    {
      SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_key(_sql, passwordBytes, passwordBytes.Length);

      if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.HidePassword))
        ZeroPassword(passwordBytes);

      if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());

      if (_usePool)
      {
        _usePool = false;

#if !NET_COMPACT_20 && TRACE_CONNECTION
        Trace.WriteLine(HelperMethods.StringFormat(
          CultureInfo.CurrentCulture,
          "SetPassword (Pool) Disabled: {0}",
          HandleToString()));
#endif
      }
    }

    internal override void ChangePassword(byte[] newPasswordBytes)
    {
      SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_rekey(_sql, newPasswordBytes, (newPasswordBytes == null) ? 0 : newPasswordBytes.Length);

      if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.HidePassword))
        ZeroPassword(newPasswordBytes);

      if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());

      if (_usePool)
      {
        _usePool = false;

#if !NET_COMPACT_20 && TRACE_CONNECTION
        Trace.WriteLine(HelperMethods.StringFormat(
          CultureInfo.CurrentCulture,
          "ChangePassword (Pool) Disabled: {0}",
          HandleToString()));
#endif
      }
    }
#endif

    internal override void SetProgressHook(int nOps, SQLiteProgressCallback func)
    {
        UnsafeNativeMethods.sqlite3_progress_handler(_sql, nOps, func, IntPtr.Zero);
    }

    internal override void SetAuthorizerHook(SQLiteAuthorizerCallback func)
    {
      UnsafeNativeMethods.sqlite3_set_authorizer(_sql, func, IntPtr.Zero);
    }

    internal override void SetUpdateHook(SQLiteUpdateCallback func)
    {
      UnsafeNativeMethods.sqlite3_update_hook(_sql, func, IntPtr.Zero);
    }

    internal override void SetCommitHook(SQLiteCommitCallback func)
    {
      UnsafeNativeMethods.sqlite3_commit_hook(_sql, func, IntPtr.Zero);
    }

    internal override void SetTraceCallback(SQLiteTraceCallback func)
    {
      UnsafeNativeMethods.sqlite3_trace(_sql, func, IntPtr.Zero);
    }

    internal override void SetTraceCallback2(SQLiteTraceFlags mask, SQLiteTraceCallback2 func)
    {
        UnsafeNativeMethods.sqlite3_trace_v2(_sql, mask, func, IntPtr.Zero);
    }

    internal override void SetRollbackHook(SQLiteRollbackCallback func)
    {
      UnsafeNativeMethods.sqlite3_rollback_hook(_sql, func, IntPtr.Zero);
    }

    /// <summary>
    /// Allows the setting of a logging callback invoked by SQLite when a
    /// log event occurs.  Only one callback may be set.  If NULL is passed,
    /// the logging callback is unregistered.
    /// </summary>
    /// <param name="func">The callback function to invoke.</param>
    /// <returns>Returns a result code</returns>
    internal override SQLiteErrorCode SetLogCallback(SQLiteLogCallback func)
    {
        SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3_config_log(
            SQLiteConfigOpsEnum.SQLITE_CONFIG_LOG, func, IntPtr.Zero);

        if (rc == SQLiteErrorCode.Ok)
            _setLogCallback = (func != null);

        return rc;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Appends an error message and an appropriate line-ending to a <see cref="StringBuilder" />
    /// instance.  This is useful because the .NET Compact Framework has a slightly different set
    /// of supported methods for the <see cref="StringBuilder" /> class.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="StringBuilder" /> instance to append to.
    /// </param>
    /// <param name="message">
    /// The message to append.  It will be followed by an appropriate line-ending.
    /// </param>
    private static void AppendError(
        StringBuilder builder,
        string message
        )
    {
        if (builder == null)
            return;

#if !PLATFORM_COMPACTFRAMEWORK
        builder.AppendLine(message);
#else
        builder.Append(message);
        builder.Append("\r\n");
#endif
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This method attempts to cause the SQLite native library to invalidate
    /// its function pointers that refer to this instance.  This is necessary
    /// to prevent calls from native code into delegates that may have been
    /// garbage collected.  Normally, these types of issues can only arise for
    /// connections that are added to the pool; howver, it is good practice to
    /// unconditionally invalidate function pointers that may refer to objects
    /// being disposed.
    /// </summary>
    /// <param name="includeGlobal">
    /// Non-zero to also invalidate global function pointers (i.e. those that
    /// are not directly associated with this connection on the native side).
    /// </param>
    /// <param name="canThrow">
    /// Non-zero if this method is being executed within a context where it can
    /// throw an exception in the event of failure; otherwise, zero.
    /// </param>
    /// <returns>
    /// Non-zero if this method was successful; otherwise, zero.
    /// </returns>
    private bool UnhookNativeCallbacks(
        bool includeGlobal,
        bool canThrow
        )
    {
        //
        // NOTE: Initially, this method assumes success.  Then, if any attempt
        //       to invalidate a function pointer fails, the overall result is
        //       set to failure.  However, this will not prevent further
        //       attempts, if any, to invalidate subsequent function pointers.
        //
        bool result = true;
        SQLiteErrorCode rc = SQLiteErrorCode.Ok;
        StringBuilder builder = new StringBuilder();

        ///////////////////////////////////////////////////////////////////////////////////////////

        #region Rollback Hook (Per-Connection)
        try
        {
            SetRollbackHook(null); /* throw */
        }
#if !NET_COMPACT_20 && TRACE_CONNECTION
        catch (Exception e)
#else
        catch (Exception)
#endif
        {
#if !NET_COMPACT_20 && TRACE_CONNECTION
            try
            {
                Trace.WriteLine(HelperMethods.StringFormat(
                    CultureInfo.CurrentCulture,
                    "Failed to unset rollback hook: {0}",
                    e)); /* throw */
            }
            catch
            {
                // do nothing.
            }
#endif

            AppendError(builder, "failed to unset rollback hook");
            rc = SQLiteErrorCode.Error;

            result = false;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////////////

        #region Trace Callback (Per-Connection)
        try
        {
            //
            // NOTE: When using version 3.14 (or later) of the SQLite core
            //       library, use the newer sqlite3_trace_v2() API in order
            //       to unhook the trace callback, just in case the older
            //       API is not available (e.g. SQLITE_OMIT_DEPRECATED).
            //
            if (UnsafeNativeMethods.sqlite3_libversion_number() >= 3014000)
                SetTraceCallback2(SQLiteTraceFlags.SQLITE_TRACE_NONE, null); /* throw */
            else
                SetTraceCallback(null); /* throw */
        }
#if !NET_COMPACT_20 && TRACE_CONNECTION
        catch (Exception e)
#else
        catch (Exception)
#endif
        {
#if !NET_COMPACT_20 && TRACE_CONNECTION
            try
            {
                Trace.WriteLine(HelperMethods.StringFormat(
                    CultureInfo.CurrentCulture,
                    "Failed to unset trace callback: {0}",
                    e)); /* throw */
            }
            catch
            {
                // do nothing.
            }
#endif

            AppendError(builder, "failed to unset trace callback");
            rc = SQLiteErrorCode.Error;

            result = false;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////////////

        #region Commit Hook (Per-Connection)
        try
        {
            SetCommitHook(null); /* throw */
        }
#if !NET_COMPACT_20 && TRACE_CONNECTION
        catch (Exception e)
#else
        catch (Exception)
#endif
        {
#if !NET_COMPACT_20 && TRACE_CONNECTION
            try
            {
                Trace.WriteLine(HelperMethods.StringFormat(
                    CultureInfo.CurrentCulture,
                    "Failed to unset commit hook: {0}",
                    e)); /* throw */
            }
            catch
            {
                // do nothing.
            }
#endif

            AppendError(builder, "failed to unset commit hook");
            rc = SQLiteErrorCode.Error;

            result = false;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////////////

        #region Update Hook (Per-Connection)
        try
        {
            SetUpdateHook(null); /* throw */
        }
#if !NET_COMPACT_20 && TRACE_CONNECTION
        catch (Exception e)
#else
        catch (Exception)
#endif
        {
#if !NET_COMPACT_20 && TRACE_CONNECTION
            try
            {
                Trace.WriteLine(HelperMethods.StringFormat(
                    CultureInfo.CurrentCulture,
                    "Failed to unset update hook: {0}",
                    e)); /* throw */
            }
            catch
            {
                // do nothing.
            }
#endif

            AppendError(builder, "failed to unset update hook");
            rc = SQLiteErrorCode.Error;

            result = false;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////////////

        #region Authorizer Hook (Per-Connection)
        try
        {
            SetAuthorizerHook(null); /* throw */
        }
#if !NET_COMPACT_20 && TRACE_CONNECTION
        catch (Exception e)
#else
        catch (Exception)
#endif
        {
#if !NET_COMPACT_20 && TRACE_CONNECTION
            try
            {
                Trace.WriteLine(HelperMethods.StringFormat(
                    CultureInfo.CurrentCulture,
                    "Failed to unset authorizer hook: {0}",
                    e)); /* throw */
            }
            catch
            {
                // do nothing.
            }
#endif

            AppendError(builder, "failed to unset authorizer hook");
            rc = SQLiteErrorCode.Error;

            result = false;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////////////

        #region Progress Hook (Per-Connection)
        try
        {
            SetProgressHook(0, null); /* throw */
        }
#if !NET_COMPACT_20 && TRACE_CONNECTION
        catch (Exception e)
#else
        catch (Exception)
#endif
        {
#if !NET_COMPACT_20 && TRACE_CONNECTION
            try
            {
                Trace.WriteLine(HelperMethods.StringFormat(
                    CultureInfo.CurrentCulture,
                    "Failed to unset progress hook: {0}",
                    e)); /* throw */
            }
            catch
            {
                // do nothing.
            }
#endif

            AppendError(builder, "failed to unset progress hook");
            rc = SQLiteErrorCode.Error;

            result = false;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////////////

        #region Log Callback (Global)
        //
        // NOTE: We have to be careful here because the log callback
        //       is not per-connection on the native side.  It should
        //       only be unset by this method if this instance was
        //       responsible for setting it.
        //
        if (includeGlobal && _setLogCallback)
        {
            try
            {
                SQLiteErrorCode rc2 = SetLogCallback(null); /* throw */

                if (rc2 != SQLiteErrorCode.Ok)
                {
                    AppendError(builder, "could not unset log callback");
                    rc = rc2;

                    result = false;
                }
            }
#if !NET_COMPACT_20 && TRACE_CONNECTION
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if !NET_COMPACT_20 && TRACE_CONNECTION
                try
                {
                    Trace.WriteLine(HelperMethods.StringFormat(
                        CultureInfo.CurrentCulture,
                        "Failed to unset log callback: {0}",
                        e)); /* throw */
                }
                catch
                {
                    // do nothing.
                }
#endif

                AppendError(builder, "failed to unset log callback");
                rc = SQLiteErrorCode.Error;

                result = false;
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////////////

        if (!result && canThrow)
            throw new SQLiteException(rc, builder.ToString());

        return result;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This method attempts to free the cached database name used with the
    /// <see cref="SetConfigurationOption" /> method.
    /// </summary>
    /// <param name="canThrow">
    /// Non-zero if this method is being executed within a context where it can
    /// throw an exception in the event of failure; otherwise, zero.
    /// </param>
    /// <returns>
    /// Non-zero if this method was successful; otherwise, zero.
    /// </returns>
    private bool FreeDbName(
        bool canThrow
        )
    {
        try
        {
            if (dbName != IntPtr.Zero)
            {
                SQLiteMemory.Free(dbName);
                dbName = IntPtr.Zero;
            }

            return true;
        }
#if !NET_COMPACT_20 && TRACE_CONNECTION
        catch (Exception e)
#else
        catch (Exception)
#endif
        {
#if !NET_COMPACT_20 && TRACE_CONNECTION
            try
            {
                Trace.WriteLine(HelperMethods.StringFormat(
                    CultureInfo.CurrentCulture,
                    "Failed to free database name: {0}",
                    e)); /* throw */
            }
            catch
            {
                // do nothing.
            }
#endif

            if (canThrow)
                throw;
        }

        return false;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Creates a new SQLite backup object based on the provided destination
    /// database connection.  The source database connection is the one
    /// associated with this object.  The source and destination database
    /// connections cannot be the same.
    /// </summary>
    /// <param name="destCnn">The destination database connection.</param>
    /// <param name="destName">The destination database name.</param>
    /// <param name="sourceName">The source database name.</param>
    /// <returns>The newly created backup object.</returns>
    internal override SQLiteBackup InitializeBackup(
        SQLiteConnection destCnn,
        string destName,
        string sourceName
        )
    {
        if (destCnn == null)
            throw new ArgumentNullException("destCnn");

        if (destName == null)
            throw new ArgumentNullException("destName");

        if (sourceName == null)
            throw new ArgumentNullException("sourceName");

        SQLite3 destSqlite3 = destCnn._sql as SQLite3;

        if (destSqlite3 == null)
            throw new ArgumentException(
                "Destination connection has no wrapper.",
                "destCnn");

        SQLiteConnectionHandle destHandle = destSqlite3._sql;

        if (destHandle == null)
            throw new ArgumentException(
                "Destination connection has an invalid handle.",
                "destCnn");

        SQLiteConnectionHandle sourceHandle = _sql;

        if (sourceHandle == null)
            throw new InvalidOperationException(
                "Source connection has an invalid handle.");

        byte[] zDestName = ToUTF8(destName);
        byte[] zSourceName = ToUTF8(sourceName);

        SQLiteBackupHandle backupHandle = null;

        try
        {
            // do nothing.
        }
        finally /* NOTE: Thread.Abort() protection. */
        {
            IntPtr backup = UnsafeNativeMethods.sqlite3_backup_init(
                destHandle, zDestName, sourceHandle, zSourceName);

            if (backup == IntPtr.Zero)
            {
                SQLiteErrorCode resultCode = ResultCode();

                if (resultCode != SQLiteErrorCode.Ok)
                    throw new SQLiteException(resultCode, GetLastError());
                else
                    throw new SQLiteException("failed to initialize backup");
            }

            backupHandle = new SQLiteBackupHandle(destHandle, backup);
        }

        SQLiteConnection.OnChanged(null, new ConnectionEventArgs(
            SQLiteConnectionEventType.NewCriticalHandle, null,
            null, null, null, backupHandle, null, new object[] {
            typeof(SQLite3), destCnn, destName, sourceName }));

        return new SQLiteBackup(
            this, backupHandle, destHandle, zDestName, sourceHandle,
            zSourceName);
    }

    /// <summary>
    /// Copies up to N pages from the source database to the destination
    /// database associated with the specified backup object.
    /// </summary>
    /// <param name="backup">The backup object to use.</param>
    /// <param name="nPage">
    /// The number of pages to copy, negative to copy all remaining pages.
    /// </param>
    /// <param name="retry">
    /// Set to true if the operation needs to be retried due to database
    /// locking issues; otherwise, set to false.
    /// </param>
    /// <returns>
    /// True if there are more pages to be copied, false otherwise.
    /// </returns>
    internal override bool StepBackup(
        SQLiteBackup backup,
        int nPage,
        ref bool retry
        )
    {
        retry = false;

        if (backup == null)
            throw new ArgumentNullException("backup");

        SQLiteBackupHandle handle = backup._sqlite_backup;

        if (handle == null)
            throw new InvalidOperationException(
                "Backup object has an invalid handle.");

        IntPtr handlePtr = handle;

        if (handlePtr == IntPtr.Zero)
            throw new InvalidOperationException(
                "Backup object has an invalid handle pointer.");

        SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_backup_step(handlePtr, nPage);
        backup._stepResult = n; /* NOTE: Save for use by FinishBackup. */

        if (n == SQLiteErrorCode.Ok)
        {
            return true;
        }
        else if (n == SQLiteErrorCode.Busy)
        {
            retry = true;
            return true;
        }
        else if (n == SQLiteErrorCode.Locked)
        {
            retry = true;
            return true;
        }
        else if (n == SQLiteErrorCode.Done)
        {
            return false;
        }
        else
        {
            throw new SQLiteException(n, GetLastError());
        }
    }

    /// <summary>
    /// Returns the number of pages remaining to be copied from the source
    /// database to the destination database associated with the specified
    /// backup object.
    /// </summary>
    /// <param name="backup">The backup object to check.</param>
    /// <returns>The number of pages remaining to be copied.</returns>
    internal override int RemainingBackup(
        SQLiteBackup backup
        )
    {
        if (backup == null)
            throw new ArgumentNullException("backup");

        SQLiteBackupHandle handle = backup._sqlite_backup;

        if (handle == null)
            throw new InvalidOperationException(
                "Backup object has an invalid handle.");

        IntPtr handlePtr = handle;

        if (handlePtr == IntPtr.Zero)
            throw new InvalidOperationException(
                "Backup object has an invalid handle pointer.");

        return UnsafeNativeMethods.sqlite3_backup_remaining(handlePtr);
    }

    /// <summary>
    /// Returns the total number of pages in the source database associated
    /// with the specified backup object.
    /// </summary>
    /// <param name="backup">The backup object to check.</param>
    /// <returns>The total number of pages in the source database.</returns>
    internal override int PageCountBackup(
        SQLiteBackup backup
        )
    {
        if (backup == null)
            throw new ArgumentNullException("backup");

        SQLiteBackupHandle handle = backup._sqlite_backup;

        if (handle == null)
            throw new InvalidOperationException(
                "Backup object has an invalid handle.");

        IntPtr handlePtr = handle;

        if (handlePtr == IntPtr.Zero)
            throw new InvalidOperationException(
                "Backup object has an invalid handle pointer.");

        return UnsafeNativeMethods.sqlite3_backup_pagecount(handlePtr);
    }

    /// <summary>
    /// Destroys the backup object, rolling back any backup that may be in
    /// progess.
    /// </summary>
    /// <param name="backup">The backup object to destroy.</param>
    internal override void FinishBackup(
        SQLiteBackup backup
        )
    {
        if (backup == null)
            throw new ArgumentNullException("backup");

        SQLiteBackupHandle handle = backup._sqlite_backup;

        if (handle == null)
            throw new InvalidOperationException(
                "Backup object has an invalid handle.");

        IntPtr handlePtr = handle;

        if (handlePtr == IntPtr.Zero)
            throw new InvalidOperationException(
                "Backup object has an invalid handle pointer.");

#if !SQLITE_STANDARD
        SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_backup_finish_interop(handlePtr);
#else
        SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_backup_finish(handlePtr);
#endif
        handle.SetHandleAsInvalid();

#if COUNT_HANDLE
        if ((n == SQLiteErrorCode.Ok) || (n == backup._stepResult)) handle.WasReleasedOk();
#endif

        if ((n != SQLiteErrorCode.Ok) && (n != backup._stepResult))
            throw new SQLiteException(n, GetLastError());
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Determines if the SQLite core library has been initialized for the
    /// current process.
    /// </summary>
    /// <returns>
    /// A boolean indicating whether or not the SQLite core library has been
    /// initialized for the current process.
    /// </returns>
    internal override bool IsInitialized()
    {
        return StaticIsInitialized();
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Determines if the SQLite core library has been initialized for the
    /// current process.
    /// </summary>
    /// <returns>
    /// A boolean indicating whether or not the SQLite core library has been
    /// initialized for the current process.
    /// </returns>
    internal static bool StaticIsInitialized()
    {
        //
        // BUGFIX: Prevent races with other threads for this entire block, due
        //         to the try/finally semantics.  See ticket [72905c9a77].
        //
        lock (syncRoot)
        {
            //
            // NOTE: Save the state of the logging class and then restore it
            //       after we are done to avoid logging too many false errors.
            //
            bool savedEnabled = SQLiteLog.Enabled;
            SQLiteLog.Enabled = false;

            try
            {
                //
                // NOTE: This method [ab]uses the fact that SQLite will always
                //       return SQLITE_ERROR for any unknown configuration option
                //       *unless* the SQLite library has already been initialized.
                //       In that case it will always return SQLITE_MISUSE.
                //
                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3_config_none(
                    SQLiteConfigOpsEnum.SQLITE_CONFIG_NONE);

                return (rc == SQLiteErrorCode.Misuse);
            }
            finally
            {
                SQLiteLog.Enabled = savedEnabled;
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

#if USE_INTEROP_DLL && INTEROP_LOG
    internal static SQLiteErrorCode ConfigureLogForInterop(
        string className
        )
    {
        SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3_config_log_interop();

        if (rc == SQLiteErrorCode.Ok)
        {
            UnsafeNativeMethods.sqlite3_log(rc, SQLiteConvert.ToUTF8(
                HelperMethods.StringFormat(CultureInfo.InvariantCulture,
                    "logging initialized via \"{0}\".", className)));
        }
        else if (rc == SQLiteErrorCode.Done)
        {
            rc = SQLiteErrorCode.Ok;
        }

        return rc;
    }
#endif

    ///////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Helper function to retrieve a column of data from an active statement.
    /// </summary>
    /// <param name="stmt">The statement being step()'d through</param>
    /// <param name="flags">The flags associated with the connection.</param>
    /// <param name="index">The column index to retrieve</param>
    /// <param name="typ">The type of data contained in the column.  If Uninitialized, this function will retrieve the datatype information.</param>
    /// <returns>Returns the data in the column</returns>
    internal override object GetValue(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, SQLiteType typ)
    {
      TypeAffinity aff = typ.Affinity;
      if (aff == TypeAffinity.Null) return DBNull.Value;
      Type t = null;

      if (typ.Type != DbType.Object)
      {
        t = SQLiteConvert.SQLiteTypeToType(typ);
        aff = TypeToAffinity(t, flags);
      }

      if (HelperMethods.HasFlags(flags, SQLiteConnectionFlags.GetAllAsText))
          return GetText(stmt, index);

      switch (aff)
      {
        case TypeAffinity.Blob:
          if (typ.Type == DbType.Guid && typ.Affinity == TypeAffinity.Text)
            return new Guid(GetText(stmt, index));

          int n = (int)GetBytes(stmt, index, 0, null, 0, 0);
          byte[] b = new byte[n];
          GetBytes(stmt, index, 0, b, 0, n);

          if (typ.Type == DbType.Guid && n == 16)
            return new Guid(b);

          return b;
        case TypeAffinity.DateTime:
          return GetDateTime(stmt, index);
        case TypeAffinity.Double:
          if (t == null) return GetDouble(stmt, index);
          return Convert.ChangeType(GetDouble(stmt, index), t,
              HelperMethods.HasFlags(flags, SQLiteConnectionFlags.GetInvariantDouble) ?
                  CultureInfo.InvariantCulture : CultureInfo.CurrentCulture);
        case TypeAffinity.Int64:
          if (t == null) return GetInt64(stmt, index);
          if (t == typeof(Boolean)) return GetBoolean(stmt, index);
          if (t == typeof(SByte)) return GetSByte(stmt, index);
          if (t == typeof(Byte)) return GetByte(stmt, index);
          if (t == typeof(Int16)) return GetInt16(stmt, index);
          if (t == typeof(UInt16)) return GetUInt16(stmt, index);
          if (t == typeof(Int32)) return GetInt32(stmt, index);
          if (t == typeof(UInt32)) return GetUInt32(stmt, index);
          if (t == typeof(Int64)) return GetInt64(stmt, index);
          if (t == typeof(UInt64)) return GetUInt64(stmt, index);
          return Convert.ChangeType(GetInt64(stmt, index), t,
              HelperMethods.HasFlags(flags, SQLiteConnectionFlags.GetInvariantInt64) ?
                  CultureInfo.InvariantCulture : CultureInfo.CurrentCulture);
        default:
          return GetText(stmt, index);
      }
    }

    internal override int GetCursorForTable(SQLiteStatement stmt, int db, int rootPage)
    {
#if !SQLITE_STANDARD
      return UnsafeNativeMethods.sqlite3_table_cursor_interop(stmt._sqlite_stmt, db, rootPage);
#else
      return -1;
#endif
    }

    internal override long GetRowIdForCursor(SQLiteStatement stmt, int cursor)
    {
#if !SQLITE_STANDARD
      long rowid = 0;
      SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3_cursor_rowid_interop(stmt._sqlite_stmt, cursor, ref rowid);
      if (rc == SQLiteErrorCode.Ok) return rowid;

      return 0;
#else
      return 0;
#endif
    }

    internal override void GetIndexColumnExtendedInfo(string database, string index, string column, ref int sortMode, ref int onError, ref string collationSequence)
    {
#if !SQLITE_STANDARD
      IntPtr coll = IntPtr.Zero;
      int colllen = 0;
      SQLiteErrorCode rc;

      rc = UnsafeNativeMethods.sqlite3_index_column_info_interop(_sql, ToUTF8(database), ToUTF8(index), ToUTF8(column), ref sortMode, ref onError, ref coll, ref colllen);
      if (rc != SQLiteErrorCode.Ok) throw new SQLiteException(rc, null);

      collationSequence = UTF8ToString(coll, colllen);
#else
      sortMode = 0;
      onError = 2;
      collationSequence = "BINARY";
#endif
    }

    internal override SQLiteErrorCode FileControl(string zDbName, int op, IntPtr pArg)
    {
      return UnsafeNativeMethods.sqlite3_file_control(_sql, (zDbName != null) ? ToUTF8(zDbName) : null, op, pArg);
    }
  }
}
