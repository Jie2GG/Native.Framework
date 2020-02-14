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

#if !NET_COMPACT_20 && TRACE_CONNECTION
  using System.Diagnostics;
#endif

  using System.Globalization;
  using System.IO;
  using System.Runtime.InteropServices;


  /// <summary>
  /// Alternate SQLite3 object, overriding many text behaviors to support UTF-16 (Unicode)
  /// </summary>
  internal sealed class SQLite3_UTF16 : SQLite3
  {
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
    /// The fully qualified file name associated with <paramref name="db" />.
    /// </param>
    /// <param name="ownHandle">
    /// Non-zero if the newly created object instance will need to dispose
    /// of <paramref name="db" /> when it is no longer needed.
    /// </param>
    internal SQLite3_UTF16(
        SQLiteDateFormats fmt,
        DateTimeKind kind,
        string fmtString,
        IntPtr db,
        string fileName,
        bool ownHandle
        )
        : base(fmt, kind, fmtString, db, fileName, ownHandle)
    {
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    #region IDisposable "Pattern" Members
    private bool disposed;
    private void CheckDisposed() /* throw */
    {
#if THROW_ON_DISPOSED
        if (disposed)
            throw new ObjectDisposedException(typeof(SQLite3_UTF16).Name);
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

    /// <summary>
    /// Overrides SQLiteConvert.ToString() to marshal UTF-16 strings instead of UTF-8
    /// </summary>
    /// <param name="b">A pointer to a UTF-16 string</param>
    /// <param name="nbytelen">The length (IN BYTES) of the string</param>
    /// <returns>A .NET string</returns>
    public override string ToString(IntPtr b, int nbytelen)
    {
      CheckDisposed();
      return UTF16ToString(b, nbytelen);
    }

    public static string UTF16ToString(IntPtr b, int nbytelen)
    {
      if (nbytelen == 0 || b == IntPtr.Zero) return String.Empty;

      if (nbytelen == -1)
        return Marshal.PtrToStringUni(b);
      else
        return Marshal.PtrToStringUni(b, nbytelen / 2);
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
      _fileName = strFilename;
      _flags = connectionFlags;

      if (usePool)
      {
        _sql = SQLiteConnectionPool.Remove(strFilename, maxPoolSize, out _poolVersion);

        SQLiteConnection.OnChanged(null, new ConnectionEventArgs(
            SQLiteConnectionEventType.OpenedFromPool, null, null,
            null, null, _sql, strFilename, new object[] {
            typeof(SQLite3_UTF16), strFilename, vfsName,
            connectionFlags, openFlags, maxPoolSize, usePool,
            _poolVersion }));

#if !NET_COMPACT_20 && TRACE_CONNECTION
        Trace.WriteLine(HelperMethods.StringFormat(
            CultureInfo.CurrentCulture,
            "Open16 (Pool): {0}",
            HandleToString()));
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

          int extFuncs = HelperMethods.HasFlags(connectionFlags, SQLiteConnectionFlags.NoExtensionFunctions) ? 0 : 1;

#if !SQLITE_STANDARD
          if ((vfsName != null) || (extFuncs != 0))
          {
            n = UnsafeNativeMethods.sqlite3_open16_interop(ToUTF8(strFilename), ToUTF8(vfsName), openFlags, extFuncs, ref db);
          }
          else
#endif
          {
            //
            // NOTE: This flag check is designed to enforce the constraint that opening
            //       a database file that does not already exist requires specifying the
            //       "Create" flag, even when a native API is used that does not accept
            //       a flags parameter.
            //
            if (((openFlags & SQLiteOpenFlagsEnum.Create) != SQLiteOpenFlagsEnum.Create) && !File.Exists(strFilename))
              throw new SQLiteException(SQLiteErrorCode.CantOpen, strFilename);

            if (vfsName != null)
            {
              throw new SQLiteException(SQLiteErrorCode.CantOpen, HelperMethods.StringFormat(
                CultureInfo.CurrentCulture,
                "cannot open using UTF-16 and VFS \"{0}\": need interop assembly", vfsName));
            }

            n = UnsafeNativeMethods.sqlite3_open16(strFilename, ref db);
          }

#if !NET_COMPACT_20 && TRACE_CONNECTION
          Trace.WriteLine(HelperMethods.StringFormat(
              CultureInfo.CurrentCulture,
              "Open16: {0}", db));
#endif

          if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, null);
          _sql = new SQLiteConnectionHandle(db, true);
        }
        lock (_sql) { /* HACK: Force the SyncBlock to be "created" now. */ }

        SQLiteConnection.OnChanged(null, new ConnectionEventArgs(
            SQLiteConnectionEventType.NewCriticalHandle, null,
            null, null, null, _sql, strFilename, new object[] {
            typeof(SQLite3_UTF16), strFilename, vfsName,
            connectionFlags, openFlags, maxPoolSize, usePool }));
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

    internal override void Bind_DateTime(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, DateTime dt)
    {
        switch (_datetimeFormat)
        {
            case SQLiteDateFormats.Ticks:
            case SQLiteDateFormats.JulianDay:
            case SQLiteDateFormats.UnixEpoch:
                {
                    base.Bind_DateTime(stmt, flags, index, dt);
                    break;
                }
            default:
                {
#if !PLATFORM_COMPACTFRAMEWORK
                    if (HelperMethods.LogBind(flags))
                    {
                        SQLiteStatementHandle handle =
                            (stmt != null) ? stmt._sqlite_stmt : null;

                        LogBind(handle, index, dt);
                    }
#endif

                    Bind_Text(stmt, flags, index, ToString(dt));
                    break;
                }
        }
    }

    internal override void Bind_Text(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, string value)
    {
        SQLiteStatementHandle handle = stmt._sqlite_stmt;

#if !PLATFORM_COMPACTFRAMEWORK
        if (HelperMethods.LogBind(flags))
        {
            LogBind(handle, index, value);
        }
#endif

        SQLiteErrorCode n = UnsafeNativeMethods.sqlite3_bind_text16(handle, index, value, value.Length * 2, (IntPtr)(-1));
        if (n != SQLiteErrorCode.Ok) throw new SQLiteException(n, GetLastError());
    }

    internal override DateTime GetDateTime(SQLiteStatement stmt, int index)
    {
      if (_datetimeFormat == SQLiteDateFormats.Ticks)
        return TicksToDateTime(GetInt64(stmt, index), _datetimeKind);
      else if (_datetimeFormat == SQLiteDateFormats.JulianDay)
        return ToDateTime(GetDouble(stmt, index), _datetimeKind);
      else if (_datetimeFormat == SQLiteDateFormats.UnixEpoch)
        return UnixEpochToDateTime(GetInt64(stmt, index), _datetimeKind);

      return ToDateTime(GetText(stmt, index));
    }

    internal override string ColumnName(SQLiteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len = 0;
      IntPtr p = UnsafeNativeMethods.sqlite3_column_name16_interop(stmt._sqlite_stmt, index, ref len);
#else
      IntPtr p = UnsafeNativeMethods.sqlite3_column_name16(stmt._sqlite_stmt, index);
#endif
      if (p == IntPtr.Zero)
        throw new SQLiteException(SQLiteErrorCode.NoMem, GetLastError());
#if !SQLITE_STANDARD
      return UTF16ToString(p, len);
#else
      return UTF16ToString(p, -1);
#endif
    }

    internal override string GetText(SQLiteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len = 0;
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_text16_interop(stmt._sqlite_stmt, index, ref len), len);
#else
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_text16(stmt._sqlite_stmt, index),
        UnsafeNativeMethods.sqlite3_column_bytes16(stmt._sqlite_stmt, index));
#endif
    }

    internal override string ColumnOriginalName(SQLiteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len = 0;
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_origin_name16_interop(stmt._sqlite_stmt, index, ref len), len);
#else
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_origin_name16(stmt._sqlite_stmt, index), -1);
#endif
    }

    internal override string ColumnDatabaseName(SQLiteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len = 0;
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_database_name16_interop(stmt._sqlite_stmt, index, ref len), len);
#else
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_database_name16(stmt._sqlite_stmt, index), -1);
#endif
    }

    internal override string ColumnTableName(SQLiteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len = 0;
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_table_name16_interop(stmt._sqlite_stmt, index, ref len), len);
#else
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_table_name16(stmt._sqlite_stmt, index), -1);
#endif
    }

    internal override string GetParamValueText(IntPtr ptr)
    {
#if !SQLITE_STANDARD
      int len = 0;
      return UTF16ToString(UnsafeNativeMethods.sqlite3_value_text16_interop(ptr, ref len), len);
#else
      return UTF16ToString(UnsafeNativeMethods.sqlite3_value_text16(ptr),
        UnsafeNativeMethods.sqlite3_value_bytes16(ptr));
#endif
    }

    internal override void ReturnError(IntPtr context, string value)
    {
      UnsafeNativeMethods.sqlite3_result_error16(context, value, value.Length * 2);
    }

    internal override void ReturnText(IntPtr context, string value)
    {
      UnsafeNativeMethods.sqlite3_result_text16(context, value, value.Length * 2, (IntPtr)(-1));
    }
  }
}
