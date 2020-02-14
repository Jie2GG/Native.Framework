/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 *
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
  using System;
  using System.Data.Common;
  using System.Globalization;

#if !PLATFORM_COMPACTFRAMEWORK
  using System.Reflection;
  using System.Runtime.Serialization;
  using System.Security.Permissions;
#endif

  /// <summary>
  /// SQLite exception class.
  /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
  [Serializable()]
  public sealed class SQLiteException : DbException, ISerializable
#else
  public sealed class SQLiteException : Exception
#endif
  {
    #region Private Constants
    /// <summary>
    /// This value was copied from the "WinError.h" file included with the
    /// Platform SDK for Windows 10.
    /// </summary>
    private const int FACILITY_SQLITE = 1967;
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    private SQLiteErrorCode _errorCode;

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    /// <summary>
    /// Private constructor for use with serialization.
    /// </summary>
    /// <param name="info">
    /// Holds the serialized object data about the exception being thrown.
    /// </param>
    /// <param name="context">
    /// Contains contextual information about the source or destination.
    /// </param>
    private SQLiteException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      _errorCode = (SQLiteErrorCode)info.GetInt32("errorCode");

      Initialize();
    }
#endif

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Public constructor for generating a SQLite exception given the error
    /// code and message.
    /// </summary>
    /// <param name="errorCode">
    /// The SQLite return code to report.
    /// </param>
    /// <param name="message">
    /// Message text to go along with the return code message text.
    /// </param>
    public SQLiteException(SQLiteErrorCode errorCode, string message)
      : base(GetStockErrorMessage(errorCode, message))
    {
      _errorCode = errorCode;

      Initialize();
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Public constructor that uses the base class constructor for the error
    /// message.
    /// </summary>
    /// <param name="message">Error message text.</param>
    public SQLiteException(string message)
      : this(SQLiteErrorCode.Unknown, message)
    {
        // do nothing.
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Public constructor that uses the default base class constructor.
    /// </summary>
    public SQLiteException()
        : base()
    {
        Initialize();
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Public constructor that uses the base class constructor for the error
    /// message and inner exception.
    /// </summary>
    /// <param name="message">Error message text.</param>
    /// <param name="innerException">The original (inner) exception.</param>
    public SQLiteException(string message, Exception innerException)
      : base(message, innerException)
    {
        Initialize();
    }

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    /// <summary>
    /// Adds extra information to the serialized object data specific to this
    /// class type.  This is only used for serialization.
    /// </summary>
    /// <param name="info">
    /// Holds the serialized object data about the exception being thrown.
    /// </param>
    /// <param name="context">
    /// Contains contextual information about the source or destination.
    /// </param>
    [SecurityPermission(
      SecurityAction.LinkDemand,
      Flags = SecurityPermissionFlag.SerializationFormatter)]
    public override void GetObjectData(
      SerializationInfo info,
      StreamingContext context)
    {
      if (info != null)
        info.AddValue("errorCode", _errorCode);

      base.GetObjectData(info, context);
    }
#endif

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Gets the associated SQLite result code for this exception as a
    /// <see cref="SQLiteErrorCode" />.  This property returns the same
    /// underlying value as the <see cref="ErrorCode" /> property.
    /// </summary>
    public SQLiteErrorCode ResultCode
    {
      get { return _errorCode; }
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Gets the associated SQLite return code for this exception as an
    /// <see cref="Int32" />.  For desktop versions of the .NET Framework,
    /// this property overrides the property of the same name within the
    /// <see cref="System.Runtime.InteropServices.ExternalException" />
    /// class.  This property returns the same underlying value as the
    /// <see cref="ResultCode" /> property.
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    public override int ErrorCode
#else
    public int ErrorCode
#endif
    {
      get { return (int)_errorCode; }
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This method performs extra initialization tasks.  It may be called by
    /// any of the constructors of this class.  It must not throw exceptions.
    /// </summary>
    private void Initialize()
    {
        if (HResult == unchecked((int)0x80004005)) /* E_FAIL */
        {
            int? localHResult = GetHResultForErrorCode(ResultCode);

            if (localHResult != null)
                HResult = (int)localHResult;
        }
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Maps a Win32 error code to an HRESULT.
    /// </summary>
    /// <param name="errorCode">
    /// The specified Win32 error code.  It must be within the range of zero
    /// (0) to 0xFFFF (65535).
    /// </param>
    /// <param name="success">
    /// Non-zero if the HRESULT should indicate success; otherwise, zero.
    /// </param>
    /// <returns>
    /// The integer value of the HRESULT.
    /// </returns>
    private static int MakeHResult(
        int errorCode,
        bool success
        )
    {
        return (errorCode & 0xFFFF) | FACILITY_SQLITE |
            (success ? 0 : unchecked((int)0x80000000));
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Attempts to map the specified <see cref="SQLiteErrorCode" /> onto an
    /// existing HRESULT -OR- a Win32 error code wrapped in an HRESULT.  The
    /// mappings may not have perfectly matching semantics; however, they do
    /// have the benefit of being unique within the context of this exception
    /// type.
    /// </summary>
    /// <param name="errorCode">
    /// The <see cref="SQLiteErrorCode" /> to map.
    /// </param>
    /// <returns>
    /// The integer HRESULT value -OR- null if there is no known mapping.
    /// </returns>
    private static int? GetHResultForErrorCode(
        SQLiteErrorCode errorCode
        )
    {
        switch (errorCode & SQLiteErrorCode.NonExtendedMask)
        {
            case SQLiteErrorCode.Ok:
                {
                    return 0; /* S_OK */
                }
            case SQLiteErrorCode.Error:
                {
                    return MakeHResult(0x001F, false); /* ERROR_GEN_FAILURE */
                }
            case SQLiteErrorCode.Internal:
                {
                    return unchecked((int)0x8000FFFF); /* E_UNEXPECTED */
                }
            case SQLiteErrorCode.Perm:
                {
                    return MakeHResult(0x0005, false); /* ERROR_ACCESS_DENIED */
                }
            case SQLiteErrorCode.Abort:
                {
                    return unchecked((int)0x80004004); /* E_ABORT */
                }
            case SQLiteErrorCode.Busy:
                {
                    return MakeHResult(0x00AA, false); /* ERROR_BUSY */
                }
            case SQLiteErrorCode.Locked:
                {
                    return MakeHResult(0x00D4, false); /* ERROR_LOCKED */
                }
            case SQLiteErrorCode.NoMem:
                {
                    return MakeHResult(0x000E, false); /* ERROR_OUTOFMEMORY */
                }
            case SQLiteErrorCode.ReadOnly:
                {
                    return MakeHResult(0x1779, false); /* ERROR_FILE_READ_ONLY */
                }
            case SQLiteErrorCode.Interrupt:
                {
                    return MakeHResult(0x04C7, false); /* ERROR_CANCELLED */
                }
            case SQLiteErrorCode.IoErr:
                {
                    return MakeHResult(0x045D, false); /* ERROR_IO_DEVICE */
                }
            case SQLiteErrorCode.Corrupt:
                {
                    return MakeHResult(0x054E, false); /* ERROR_INTERNAL_DB_CORRUPTION */
                }
            case SQLiteErrorCode.NotFound:
                {
                    return MakeHResult(0x0032, false); /* ERROR_NOT_SUPPORTED */
                }
            case SQLiteErrorCode.Full:
                {
                    return MakeHResult(0x0070, false); /* ERROR_DISK_FULL */
                }
            case SQLiteErrorCode.CantOpen:
                {
                    return MakeHResult(0x03F3, false); /* ERROR_CANTOPEN */
                }
            case SQLiteErrorCode.Protocol:
                {
                    return MakeHResult(0x05B4, false); /* ERROR_TIMEOUT */
                }
            case SQLiteErrorCode.Empty:
                {
                    return MakeHResult(0x10D2, false); /* ERROR_EMPTY */
                }
            case SQLiteErrorCode.Schema:
                {
                    return MakeHResult(0x078B, false); /* ERROR_CONTEXT_EXPIRED */
                }
            case SQLiteErrorCode.TooBig:
                {
                    return unchecked((int)0x800288C5); /* TYPE_E_SIZETOOBIG */
                }
            case SQLiteErrorCode.Constraint:
                {
                    return MakeHResult(0x202F, false); /* ERROR_DS_CONSTRAINT_VIOLATION */
                }
            case SQLiteErrorCode.Mismatch:
                {
                    return MakeHResult(0x065D, false); /* ERROR_DATATYPE_MISMATCH */
                }
            case SQLiteErrorCode.Misuse:
                {
                    return MakeHResult(0x0649, false); /* ERROR_INVALID_HANDLE_STATE */
                }
            case SQLiteErrorCode.NoLfs:
                {
                    return MakeHResult(0x0646, false); /* ERROR_UNKNOWN_FEATURE */
                }
            case SQLiteErrorCode.Auth:
                {
                    return MakeHResult(0x078F, false); /* ERROR_AUTHENTICATION_FIREWALL_FAILED */
                }
            case SQLiteErrorCode.Format:
                {
                    return MakeHResult(0x000B, false); /* ERROR_BAD_FORMAT */
                }
            case SQLiteErrorCode.Range:
                {
                    return unchecked((int)0x80028CA1); /* TYPE_E_OUTOFBOUNDS */
                }
            case SQLiteErrorCode.NotADb:
                {
                    return MakeHResult(0x0570, false); /* ERROR_FILE_CORRUPT */
                }
            case SQLiteErrorCode.Notice:
            case SQLiteErrorCode.Warning:
            case SQLiteErrorCode.Row:
            case SQLiteErrorCode.Done:
                {
                    //
                    // NOTE: These result codes are not errors, per se;
                    //       therefore, mask off all HRESULT bits that
                    //       are not part of the "code" portion (e.g.
                    //       the severity, facility, etc).  This will
                    //       have the effect of creating an HRESULT
                    //       that indicates success, while (hopefully)
                    //       preserving the specified result code.  At
                    //       the time this method was written (2018-02),
                    //       no SQLite result codes were outside of the
                    //       supported range for HRESULT codes (e.g.
                    //       0x0000 to 0xFFFF, inclusive), which made
                    //       the following masking operation a harmless
                    //       NOOP.
                    //
                    return MakeHResult((int)errorCode, true);
                }
        }

        return null;
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Returns the error message for the specified SQLite return code.
    /// </summary>
    /// <param name="errorCode">The SQLite return code.</param>
    /// <returns>The error message or null if it cannot be found.</returns>
    private static string GetErrorString(
        SQLiteErrorCode errorCode
        )
    {
#if !PLATFORM_COMPACTFRAMEWORK
        //
        // HACK: This must be done via reflection in order to prevent
        //       the RuntimeHelpers.PrepareDelegate method from over-
        //       eagerly attempting to locate the new (and optional)
        //       sqlite3_errstr() function in the SQLite core library
        //       because it happens to be in the static call graph for
        //       the AppDomain.DomainUnload event handler registered
        //       by the SQLiteLog class.
        //
        BindingFlags flags = BindingFlags.Static |
            BindingFlags.NonPublic | BindingFlags.InvokeMethod;

        return typeof(SQLite3).InvokeMember("GetErrorString",
            flags, null, null, new object[] { errorCode }) as string;
#else
        return SQLite3.GetErrorString(errorCode);
#endif
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Returns the composite error message based on the SQLite return code
    /// and the optional detailed error message.
    /// </summary>
    /// <param name="errorCode">The SQLite return code.</param>
    /// <param name="message">Optional detailed error message.</param>
    /// <returns>Error message text for the return code.</returns>
    private static string GetStockErrorMessage(
        SQLiteErrorCode errorCode,
        string message
        )
    {
        return HelperMethods.StringFormat(
            CultureInfo.CurrentCulture,
            "{0}{1}{2}",
            GetErrorString(errorCode),
#if !NET_COMPACT_20
            Environment.NewLine, message).Trim();
#else
            "\r\n", message).Trim();
#endif
    }

    ///////////////////////////////////////////////////////////////////////////

    #region System.Object Overrides
    public override string ToString()
    {
        return HelperMethods.StringFormat(
            CultureInfo.CurrentCulture, "code = {0} ({1}), message = {2}",
            _errorCode, (int)_errorCode, base.ToString());
    }
    #endregion
  }

  /// <summary>
  /// SQLite error codes.  Actually, this enumeration represents a return code,
  /// which may also indicate success in one of several ways (e.g. SQLITE_OK,
  /// SQLITE_ROW, and SQLITE_DONE).  Therefore, the name of this enumeration is
  /// something of a misnomer.
  /// </summary>
  public enum SQLiteErrorCode
  {
      /// <summary>
      /// The error code is unknown.  This error code
      /// is only used by the managed wrapper itself.
      /// </summary>
      Unknown = -1,
      /// <summary>
      /// Successful result
      /// </summary>
      Ok /* 0 */,
      /// <summary>
      /// SQL error or missing database
      /// </summary>
      Error /* 1 */,
      /// <summary>
      /// Internal logic error in SQLite
      /// </summary>
      Internal /* 2 */,
      /// <summary>
      /// Access permission denied
      /// </summary>
      Perm /* 3 */,
      /// <summary>
      /// Callback routine requested an abort
      /// </summary>
      Abort /* 4 */,
      /// <summary>
      /// The database file is locked
      /// </summary>
      Busy /* 5 */,
      /// <summary>
      /// A table in the database is locked
      /// </summary>
      Locked /* 6 */,
      /// <summary>
      /// A malloc() failed
      /// </summary>
      NoMem /* 7 */,
      /// <summary>
      /// Attempt to write a readonly database
      /// </summary>
      ReadOnly /* 8 */,
      /// <summary>
      /// Operation terminated by sqlite3_interrupt()
      /// </summary>
      Interrupt /* 9 */,
      /// <summary>
      /// Some kind of disk I/O error occurred
      /// </summary>
      IoErr /* 10 */,
      /// <summary>
      /// The database disk image is malformed
      /// </summary>
      Corrupt /* 11 */,
      /// <summary>
      /// Unknown opcode in sqlite3_file_control()
      /// </summary>
      NotFound /* 12 */,
      /// <summary>
      /// Insertion failed because database is full
      /// </summary>
      Full /* 13 */,
      /// <summary>
      /// Unable to open the database file
      /// </summary>
      CantOpen /* 14 */,
      /// <summary>
      /// Database lock protocol error
      /// </summary>
      Protocol /* 15 */,
      /// <summary>
      /// Database is empty
      /// </summary>
      Empty /* 16 */,
      /// <summary>
      /// The database schema changed
      /// </summary>
      Schema /* 17 */,
      /// <summary>
      /// String or BLOB exceeds size limit
      /// </summary>
      TooBig /* 18 */,
      /// <summary>
      /// Abort due to constraint violation
      /// </summary>
      Constraint /* 19 */,
      /// <summary>
      /// Data type mismatch
      /// </summary>
      Mismatch /* 20 */,
      /// <summary>
      /// Library used incorrectly
      /// </summary>
      Misuse /* 21 */,
      /// <summary>
      /// Uses OS features not supported on host
      /// </summary>
      NoLfs /* 22 */,
      /// <summary>
      /// Authorization denied
      /// </summary>
      Auth /* 23 */,
      /// <summary>
      /// Auxiliary database format error
      /// </summary>
      Format /* 24 */,
      /// <summary>
      /// 2nd parameter to sqlite3_bind out of range
      /// </summary>
      Range /* 25 */,
      /// <summary>
      /// File opened that is not a database file
      /// </summary>
      NotADb /* 26 */,
      /// <summary>
      /// Notifications from sqlite3_log()
      /// </summary>
      Notice /* 27 */,
      /// <summary>
      /// Warnings from sqlite3_log()
      /// </summary>
      Warning /* 28 */,
      /// <summary>
      /// sqlite3_step() has another row ready
      /// </summary>
      Row = 100,
      /// <summary>
      /// sqlite3_step() has finished executing
      /// </summary>
      Done, /* 101 */
      /// <summary>
      /// Used to mask off extended result codes
      /// </summary>
      NonExtendedMask = 0xFF,

      /////////////////////////////////////////////////////////////////////////
      // BEGIN EXTENDED RESULT CODES
      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// A collation sequence was referenced by a schema and it cannot be
      /// found.
      /// </summary>
      Error_Missing_CollSeq = (Error | (1 << 8)),
      /// <summary>
      /// An internal operation failed and it may succeed if retried.
      /// </summary>
      Error_Retry = (Error | (2 << 8)),
      /// <summary>
      /// A file read operation failed.
      /// </summary>
      IoErr_Read = (IoErr | (1 << 8)),
      /// <summary>
      /// A file read operation returned less data than requested.
      /// </summary>
      IoErr_Short_Read = (IoErr | (2 << 8)),
      /// <summary>
      /// A file write operation failed.
      /// </summary>
      IoErr_Write = (IoErr | (3 << 8)),
      /// <summary>
      /// A file synchronization operation failed.
      /// </summary>
      IoErr_Fsync = (IoErr | (4 << 8)),
      /// <summary>
      /// A directory synchronization operation failed.
      /// </summary>
      IoErr_Dir_Fsync = (IoErr | (5 << 8)),
      /// <summary>
      /// A file truncate operation failed.
      /// </summary>
      IoErr_Truncate = (IoErr | (6 << 8)),
      /// <summary>
      /// A file metadata operation failed.
      /// </summary>
      IoErr_Fstat = (IoErr | (7 << 8)),
      /// <summary>
      /// A file unlock operation failed.
      /// </summary>
      IoErr_Unlock = (IoErr | (8 << 8)),
      /// <summary>
      /// A file lock operation failed.
      /// </summary>
      IoErr_RdLock = (IoErr | (9 << 8)),
      /// <summary>
      /// A file delete operation failed.
      /// </summary>
      IoErr_Delete = (IoErr | (10 << 8)),
      /// <summary>
      /// Not currently used.
      /// </summary>
      IoErr_Blocked = (IoErr | (11 << 8)),
      /// <summary>
      /// Out-of-memory during a file operation.
      /// </summary>
      IoErr_NoMem = (IoErr | (12 << 8)),
      /// <summary>
      /// A file existence/status operation failed.
      /// </summary>
      IoErr_Access = (IoErr | (13 << 8)),
      /// <summary>
      /// A check for a reserved lock failed.
      /// </summary>
      IoErr_CheckReservedLock = (IoErr | (14 << 8)),
      /// <summary>
      /// A file lock operation failed.
      /// </summary>
      IoErr_Lock = (IoErr | (15 << 8)),
      /// <summary>
      /// A file close operation failed.
      /// </summary>
      IoErr_Close = (IoErr | (16 << 8)),
      /// <summary>
      /// A directory close operation failed.
      /// </summary>
      IoErr_Dir_Close = (IoErr | (17 << 8)),
      /// <summary>
      /// A shared memory open operation failed.
      /// </summary>
      IoErr_ShmOpen = (IoErr | (18 << 8)),
      /// <summary>
      /// A shared memory size operation failed.
      /// </summary>
      IoErr_ShmSize = (IoErr | (19 << 8)),
      /// <summary>
      /// A shared memory lock operation failed.
      /// </summary>
      IoErr_ShmLock = (IoErr | (20 << 8)),
      /// <summary>
      /// A shared memory map operation failed.
      /// </summary>
      IoErr_ShmMap = (IoErr | (21 << 8)),
      /// <summary>
      /// A file seek operation failed.
      /// </summary>
      IoErr_Seek = (IoErr | (22 << 8)),
      /// <summary>
      /// A file delete operation failed because it does not exist.
      /// </summary>
      IoErr_Delete_NoEnt = (IoErr | (23 << 8)),
      /// <summary>
      /// A file memory mapping operation failed.
      /// </summary>
      IoErr_Mmap = (IoErr | (24 << 8)),
      /// <summary>
      /// The temporary directory path could not be obtained.
      /// </summary>
      IoErr_GetTempPath = (IoErr | (25 << 8)),
      /// <summary>
      /// A path string conversion operation failed.
      /// </summary>
      IoErr_ConvPath = (IoErr | (26 << 8)),
      /// <summary>
      /// Reserved.
      /// </summary>
      IoErr_VNode = (IoErr | (27 << 8)),
      /// <summary>
      /// An attempt to authenticate failed.
      /// </summary>
      IoErr_Auth = (IoErr | (28 << 8)),
      /// <summary>
      /// An attempt to begin a file system transaction failed.
      /// </summary>
      IoErr_Begin_Atomic = (IoErr | (29 << 8)),
      /// <summary>
      /// An attempt to commit a file system transaction failed.
      /// </summary>
      IoErr_Commit_Atomic = (IoErr | (30 << 8)),
      /// <summary>
      /// An attempt to rollback a file system transaction failed.
      /// </summary>
      IoErr_Rollback_Atomic = (IoErr | (31 << 8)),
      /// <summary>
      /// A database table is locked in shared-cache mode.
      /// </summary>
      Locked_SharedCache = (Locked | (1 << 8)),
      /// <summary>
      /// A virtual table in the database is locked.
      /// </summary>
      Locked_Vtab = (Locked | (2 << 8)),
      /// <summary>
      /// A database file is locked due to a recovery operation.
      /// </summary>
      Busy_Recovery = (Busy | (1 << 8)),
      /// <summary>
      /// A database file is locked due to snapshot semantics.
      /// </summary>
      Busy_Snapshot = (Busy | (2 << 8)),
      /// <summary>
      /// A database file cannot be opened because no temporary directory is available.
      /// </summary>
      CantOpen_NoTempDir = (CantOpen | (1 << 8)),
      /// <summary>
      /// A database file cannot be opened because its path represents a directory.
      /// </summary>
      CantOpen_IsDir = (CantOpen | (2 << 8)),
      /// <summary>
      /// A database file cannot be opened because its full path could not be obtained.
      /// </summary>
      CantOpen_FullPath = (CantOpen | (3 << 8)),
      /// <summary>
      /// A database file cannot be opened because a path string conversion operation failed.
      /// </summary>
      CantOpen_ConvPath = (CantOpen | (4 << 8)),
      /// <summary>
      /// A virtual table is malformed.
      /// </summary>
      Corrupt_Vtab = (Corrupt | (1 << 8)),
      /// <summary>
      /// A required sequence table is missing or corrupt.
      /// </summary>
      Corrupt_Sequence = (Corrupt | (2 << 8)),
      /// <summary>
      /// A database file is read-only due to a recovery operation.
      /// </summary>
      ReadOnly_Recovery = (ReadOnly | (1 << 8)),
      /// <summary>
      /// A database file is read-only because a lock could not be obtained.
      /// </summary>
      ReadOnly_CantLock = (ReadOnly | (2 << 8)),
      /// <summary>
      /// A database file is read-only because it needs rollback processing.
      /// </summary>
      ReadOnly_Rollback = (ReadOnly | (3 << 8)),
      /// <summary>
      /// A database file is read-only because it was moved while open.
      /// </summary>
      ReadOnly_DbMoved = (ReadOnly | (4 << 8)),
      /// <summary>
      /// The shared-memory file is read-only and it should be read-write.
      /// </summary>
      ReadOnly_CantInit = (ReadOnly | (5 << 8)),
      /// <summary>
      /// Unable to create journal file because the directory is read-only.
      /// </summary>
      ReadOnly_Directory = (ReadOnly | (6 << 8)),
      /// <summary>
      /// An operation is being aborted due to rollback processing.
      /// </summary>
      Abort_Rollback = (Abort | (2 << 8)),
      /// <summary>
      /// A CHECK constraint failed.
      /// </summary>
      Constraint_Check = (Constraint | (1 << 8)),
      /// <summary>
      /// A commit hook produced a unsuccessful return code.
      /// </summary>
      Constraint_CommitHook = (Constraint | (2 << 8)),
      /// <summary>
      /// A FOREIGN KEY constraint failed.
      /// </summary>
      Constraint_ForeignKey = (Constraint | (3 << 8)),
      /// <summary>
      /// Not currently used.
      /// </summary>
      Constraint_Function = (Constraint | (4 << 8)),
      /// <summary>
      /// A NOT NULL constraint failed.
      /// </summary>
      Constraint_NotNull = (Constraint | (5 << 8)),
      /// <summary>
      /// A PRIMARY KEY constraint failed.
      /// </summary>
      Constraint_PrimaryKey = (Constraint | (6 << 8)),
      /// <summary>
      /// The RAISE function was used by a trigger-program.
      /// </summary>
      Constraint_Trigger = (Constraint | (7 << 8)),
      /// <summary>
      /// A UNIQUE constraint failed.
      /// </summary>
      Constraint_Unique = (Constraint | (8 << 8)),
      /// <summary>
      /// Not currently used.
      /// </summary>
      Constraint_Vtab = (Constraint | (9 << 8)),
      /// <summary>
      /// A ROWID constraint failed.
      /// </summary>
      Constraint_RowId = (Constraint | (10 << 8)),
      /// <summary>
      /// Frames were recovered from the WAL log file.
      /// </summary>
      Notice_Recover_Wal = (Notice | (1 << 8)),
      /// <summary>
      /// Pages were recovered from the journal file.
      /// </summary>
      Notice_Recover_Rollback = (Notice | (2 << 8)),
      /// <summary>
      /// An automatic index was created to process a query.
      /// </summary>
      Warning_AutoIndex = (Warning | (1 << 8)),
      /// <summary>
      /// User authentication failed.
      /// </summary>
      Auth_User = (Auth | (1 << 8)),
      /// <summary>
      /// Success.  Prevents the extension from unloading until the process
      /// terminates.
      /// </summary>
      Ok_Load_Permanently = (Ok | (1 << 8))
  }
}
