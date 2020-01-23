/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Joe Mistachkin (joe@mistachkin.com)
 *
 * Released to the public domain, use at your own risk!
 ********************************************************/

using System.Collections;
using System.Collections.Generic;

#if DEBUG
using System.Diagnostics;
#endif

using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Data.SQLite
{
    #region Session Extension Enumerations
    /// <summary>
    /// This enumerated type represents a type of conflict seen when apply
    /// changes from a change set or patch set.
    /// </summary>
    public enum SQLiteChangeSetConflictType
    {
        /// <summary>
        /// This value is seen when processing a DELETE or UPDATE change if a
        /// row with the required PRIMARY KEY fields is present in the
        /// database, but one or more other (non primary-key) fields modified
        /// by the update do not contain the expected "before" values.
        /// </summary>
        Data = 1,

        /// <summary>
        /// This value is seen when processing a DELETE or UPDATE change if a
        /// row with the required PRIMARY KEY fields is not present in the
        /// database.  There is no conflicting row in this case.
        ///
        /// The results of invoking the
        /// <see cref="ISQLiteChangeSetMetadataItem.GetConflictValue" />
        /// method are undefined.
        /// </summary>
        NotFound = 2,

        /// <summary>
        /// This value is seen when processing an INSERT change if the
        /// operation would result in duplicate primary key values.
        /// The conflicting row in this case is the database row with the
        /// matching primary key.
        /// </summary>
        Conflict = 3,

        /// <summary>
        /// If a non-foreign key constraint violation occurs while applying a
        /// change (i.e. a UNIQUE, CHECK or NOT NULL constraint), the conflict
        /// callback will see this value.
        ///
        /// There is no conflicting row in this case. The results of invoking
        /// the <see cref="ISQLiteChangeSetMetadataItem.GetConflictValue" />
        /// method are undefined.
        /// </summary>
        Constraint = 4,

        /// <summary>
        /// If foreign key handling is enabled, and applying a changes leaves
        /// the database in a state containing foreign key violations, this
        /// value will be seen exactly once before the changes are committed.
        /// If the conflict handler
        /// <see cref="SQLiteChangeSetConflictResult.Omit" />, the changes,
        /// including those that caused the foreign key constraint violation,
        /// are committed. Or, if it returns
        /// <see cref="SQLiteChangeSetConflictResult.Abort" />, the changes are
        /// rolled back.
        ///
        /// No current or conflicting row information is provided. The only
        /// method it is possible to call on the supplied
        /// <see cref="ISQLiteChangeSetMetadataItem" /> object is
        /// <see cref="ISQLiteChangeSetMetadataItem.NumberOfForeignKeyConflicts" />.
        /// </summary>
        ForeignKey = 5
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This enumerated type represents the result of a user-defined conflict
    /// resolution callback.
    /// </summary>
    public enum SQLiteChangeSetConflictResult
    {
        /// <summary>
        /// If a conflict callback returns this value no special action is
        /// taken. The change that caused the conflict is not applied. The
        /// application of changes continues with the next change.
        /// </summary>
        Omit = 0,

        /// <summary>
        /// This value may only be returned from a conflict callback if the
        /// type of conflict was <see cref="SQLiteChangeSetConflictType.Data" />
        /// or <see cref="SQLiteChangeSetConflictType.Conflict" />. If this is
        /// not the case, any changes applied so far are rolled back and the
        /// call to
        /// <see cref="ISQLiteChangeSet.Apply(SessionConflictCallback,SessionTableFilterCallback,object)" />
        /// will raise a <see cref="SQLiteException" /> with an error code of
        /// <see cref="SQLiteErrorCode.Misuse" />.
        ///
        /// If this value is returned for a
        /// <see cref="SQLiteChangeSetConflictType.Data" /> conflict, then the
        /// conflicting row is either updated or deleted, depending on the type
        /// of change.
        ///
        /// If this value is returned for a
        /// <see cref="SQLiteChangeSetConflictType.Conflict" /> conflict, then
        /// the conflicting row is removed from the database and a second
        /// attempt to apply the change is made. If this second attempt fails,
        /// the original row is restored to the database before continuing.
        /// </summary>
        Replace = 1,

        /// <summary>
        /// If this value is returned, any changes applied so far are rolled
        /// back and the call to
        /// <see cref="ISQLiteChangeSet.Apply(SessionConflictCallback,SessionTableFilterCallback,object)" />
        /// will raise a <see cref="SQLiteException" /> with an error code of
        /// <see cref="SQLiteErrorCode.Abort" />.
        /// </summary>
        Abort = 2
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This enumerated type represents possible flags that may be passed
    /// to the appropriate overloads of various change set creation methods.
    /// </summary>
    public enum SQLiteChangeSetStartFlags
    {
        /// <summary>
        /// No special handling.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Invert the change set while iterating through it.
        /// This is equivalent to inverting a change set using
        /// <see cref="ISQLiteChangeSet.Invert" /> before
        /// applying it. It is an error to specify this flag
        /// with a patch set.
        /// </summary>
        Invert = 0x2
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region Session Extension Delegates
    /// <summary>
    /// This callback is invoked when a determination must be made about
    /// whether changes to a specific table should be tracked -OR- applied.
    /// It will not be called for tables that are already attached to a
    /// <see cref="ISQLiteSession" />.
    /// </summary>
    /// <param name="clientData">
    /// The optional application-defined context data that was originally
    /// passed to the <see cref="ISQLiteSession.SetTableFilter" /> or
    /// <see cref="ISQLiteChangeSet.Apply(SessionConflictCallback,SessionTableFilterCallback,object)" />
    /// methods.  This value may be null.
    /// </param>
    /// <param name="name">
    /// The name of the table.
    /// </param>
    /// <returns>
    /// Non-zero if changes to the table should be considered; otherwise,
    /// zero.  Throwing an exception from this callback will result in
    /// undefined behavior.
    /// </returns>
    public delegate bool SessionTableFilterCallback(
        object clientData,
        string name
    );

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This callback is invoked when there is a conflict while apply changes
    /// to a database.
    /// </summary>
    /// <param name="clientData">
    /// The optional application-defined context data that was originally
    /// passed to the
    /// <see cref="ISQLiteChangeSet.Apply(SessionConflictCallback,SessionTableFilterCallback,object)" />
    /// method.  This value may be null.
    /// </param>
    /// <param name="type">
    /// The type of this conflict.
    /// </param>
    /// <param name="item">
    /// The <see cref="ISQLiteChangeSetMetadataItem" /> object associated with
    /// this conflict.  This value may not be null; however, only properties
    /// that are applicable to the conflict type will be available.  Further
    /// information on this is available within the descriptions of the
    /// available <see cref="SQLiteChangeSetConflictType" /> values.
    /// </param>
    /// <returns>
    /// A <see cref="SQLiteChangeSetConflictResult" /> value that indicates the
    /// action to be taken in order to resolve the conflict.  Throwing an
    /// exception from this callback will result in undefined behavior.
    /// </returns>
    public delegate SQLiteChangeSetConflictResult SessionConflictCallback(
        object clientData,
        SQLiteChangeSetConflictType type,
        ISQLiteChangeSetMetadataItem item
    );
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region ISQLiteChangeSet Interface
    /// <summary>
    /// This interface contains methods used to manipulate a set of changes for
    /// a database.
    /// </summary>
    public interface ISQLiteChangeSet :
        IEnumerable<ISQLiteChangeSetMetadataItem>, IDisposable
    {
        /// <summary>
        /// This method "inverts" the set of changes within this instance.
        /// Applying an inverted set of changes to a database reverses the
        /// effects of applying the uninverted changes.  Specifically:
        /// <![CDATA[<ul>]]><![CDATA[<li>]]>
        /// Each DELETE change is changed to an INSERT, and
        /// <![CDATA[</li>]]><![CDATA[<li>]]>
        /// Each INSERT change is changed to a DELETE, and
        /// <![CDATA[</li>]]><![CDATA[<li>]]>
        /// For each UPDATE change, the old.* and new.* values are exchanged.
        /// <![CDATA[</li>]]><![CDATA[</ul>]]>
        /// This method does not change the order in which changes appear
        /// within the set of changes. It merely reverses the sense of each
        /// individual change.
        /// </summary>
        /// <returns>
        /// The new <see cref="ISQLiteChangeSet" /> instance that represents
        /// the resulting set of changes -OR- null if it is not available.
        /// </returns>
        ISQLiteChangeSet Invert();

        /// <summary>
        /// This method combines the specified set of changes with the ones
        /// contained in this instance.
        /// </summary>
        /// <param name="changeSet">
        /// The changes to be combined with those in this instance.
        /// </param>
        /// <returns>
        /// The new <see cref="ISQLiteChangeSet" /> instance that represents
        /// the resulting set of changes -OR- null if it is not available.
        /// </returns>
        ISQLiteChangeSet CombineWith(ISQLiteChangeSet changeSet);

        /// <summary>
        /// Attempts to apply the set of changes in this instance to the
        /// associated database.
        /// </summary>
        /// <param name="conflictCallback">
        /// The <see cref="SessionConflictCallback" /> delegate that will need
        /// to handle any conflicting changes that may arise.
        /// </param>
        /// <param name="clientData">
        /// The optional application-defined context data.  This value may be
        /// null.
        /// </param>
        void Apply(
            SessionConflictCallback conflictCallback,
            object clientData
        );

        /// <summary>
        /// Attempts to apply the set of changes in this instance to the
        /// associated database.
        /// </summary>
        /// <param name="conflictCallback">
        /// The <see cref="SessionConflictCallback" /> delegate that will need
        /// to handle any conflicting changes that may arise.
        /// </param>
        /// <param name="tableFilterCallback">
        /// The optional <see cref="SessionTableFilterCallback" /> delegate
        /// that can be used to filter the list of tables impacted by the set
        /// of changes.
        /// </param>
        /// <param name="clientData">
        /// The optional application-defined context data.  This value may be
        /// null.
        /// </param>
        void Apply(
            SessionConflictCallback conflictCallback,
            SessionTableFilterCallback tableFilterCallback,
            object clientData
        );
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region ISQLiteChangeGroup Interface
    /// <summary>
    /// This interface contains methods used to manipulate multiple sets of
    /// changes for a database.
    /// </summary>
    public interface ISQLiteChangeGroup : IDisposable
    {
        /// <summary>
        /// Attempts to add a change set (or patch set) to this change group
        /// instance.  The underlying data must be contained entirely within
        /// the <paramref name="rawData" /> byte array.
        /// </summary>
        /// <param name="rawData">
        /// The raw byte data for the specified change set (or patch set).
        /// </param>
        void AddChangeSet(byte[] rawData);

        /// <summary>
        /// Attempts to add a change set (or patch set) to this change group
        /// instance.  The underlying data will be read from the specified
        /// <see cref="Stream" />.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream" /> instance containing the raw change set
        /// (or patch set) data to read.
        /// </param>
        void AddChangeSet(Stream stream);

        /// <summary>
        /// Attempts to create and return, via <paramref name="rawData" />, the
        /// combined set of changes represented by this change group instance.
        /// </summary>
        /// <param name="rawData">
        /// Upon success, this will contain the raw byte data for all the
        /// changes in this change group instance.
        /// </param>
        void CreateChangeSet(ref byte[] rawData);

        /// <summary>
        /// Attempts to create and write, via <paramref name="stream" />, the
        /// combined set of changes represented by this change group instance.
        /// </summary>
        /// <param name="stream">
        /// Upon success, the raw byte data for all the changes in this change
        /// group instance will be written to this <see cref="Stream" />.
        /// </param>
        void CreateChangeSet(Stream stream);
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region ISQLiteChangeSetMetadataItem Interface
    /// <summary>
    /// This interface contains properties and methods used to fetch metadata
    /// about one change within a set of changes for a database.
    /// </summary>
    public interface ISQLiteChangeSetMetadataItem : IDisposable
    {
        /// <summary>
        /// The name of the table the change was made to.
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// The number of columns impacted by this change.  This value can be
        /// used to determine the highest valid column index that may be used
        /// with the <see cref="GetOldValue" />, <see cref="GetNewValue" />,
        /// and <see cref="GetConflictValue" /> methods of this interface.  It
        /// will be this value minus one.
        /// </summary>
        int NumberOfColumns { get; }

        /// <summary>
        /// This will contain the value
        /// <see cref="SQLiteAuthorizerActionCode.Insert" />,
        /// <see cref="SQLiteAuthorizerActionCode.Update" />, or
        /// <see cref="SQLiteAuthorizerActionCode.Delete" />, corresponding to
        /// the overall type of change this item represents.
        /// </summary>
        SQLiteAuthorizerActionCode OperationCode { get; }

        /// <summary>
        /// Non-zero if this change is considered to be indirect (i.e. as
        /// though they were made via a trigger or foreign key action).
        /// </summary>
        bool Indirect { get; }

        /// <summary>
        /// This array contains a <see cref="Boolean" /> for each column in
        /// the table associated with this change.  The element will be zero
        /// if the column is not part of the primary key; otherwise, it will
        /// be non-zero.
        /// </summary>
        bool[] PrimaryKeyColumns { get; }

        /// <summary>
        /// This method may only be called from within a
        /// <see cref="SessionConflictCallback" /> delegate when the conflict
        /// type is <see cref="SQLiteChangeSetConflictType.ForeignKey" />.  It
        /// returns the total number of known foreign key violations in the
        /// destination database.
        /// </summary>
        int NumberOfForeignKeyConflicts { get; }

        /// <summary>
        /// Queries and returns the original value of a given column for this
        /// change.  This method may only be called when the
        /// <see cref="OperationCode" /> has a value of
        /// <see cref="SQLiteAuthorizerActionCode.Update" /> or
        /// <see cref="SQLiteAuthorizerActionCode.Delete" />.
        /// </summary>
        /// <param name="columnIndex">
        /// The index for the column.  This value must be between zero and one
        /// less than the total number of columns for this table.
        /// </param>
        /// <returns>
        /// The original value of a given column for this change.
        /// </returns>
        SQLiteValue GetOldValue(int columnIndex);

        /// <summary>
        /// Queries and returns the updated value of a given column for this
        /// change.  This method may only be called when the
        /// <see cref="OperationCode" /> has a value of
        /// <see cref="SQLiteAuthorizerActionCode.Insert" /> or
        /// <see cref="SQLiteAuthorizerActionCode.Update" />.
        /// </summary>
        /// <param name="columnIndex">
        /// The index for the column.  This value must be between zero and one
        /// less than the total number of columns for this table.
        /// </param>
        /// <returns>
        /// The updated value of a given column for this change.
        /// </returns>
        SQLiteValue GetNewValue(int columnIndex);

        /// <summary>
        /// Queries and returns the conflicting value of a given column for
        /// this change.  This method may only be called from within a
        /// <see cref="SessionConflictCallback" /> delegate when the conflict
        /// type is <see cref="SQLiteChangeSetConflictType.Data" /> or
        /// <see cref="SQLiteChangeSetConflictType.Conflict" />.
        /// </summary>
        /// <param name="columnIndex">
        /// The index for the column.  This value must be between zero and one
        /// less than the total number of columns for this table.
        /// </param>
        /// <returns>
        /// The conflicting value of a given column for this change.
        /// </returns>
        SQLiteValue GetConflictValue(int columnIndex);
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region ISQLiteSession Interface
    /// <summary>
    /// This interface contains methods to query and manipulate the state of a
    /// change tracking session for a database.
    /// </summary>
    public interface ISQLiteSession : IDisposable
    {
        /// <summary>
        /// Determines if this session is currently tracking changes to its
        /// associated database.
        /// </summary>
        /// <returns>
        /// Non-zero if changes to the associated database are being trakced;
        /// otherwise, zero.
        /// </returns>
        bool IsEnabled();

        /// <summary>
        /// Enables tracking of changes to the associated database.
        /// </summary>
        void SetToEnabled();

        /// <summary>
        /// Disables tracking of changes to the associated database.
        /// </summary>
        void SetToDisabled();

        /// <summary>
        /// Determines if this session is currently set to mark changes as
        /// indirect (i.e. as though they were made via a trigger or foreign
        /// key action).
        /// </summary>
        /// <returns>
        /// Non-zero if changes to the associated database are being marked as
        /// indirect; otherwise, zero.
        /// </returns>
        bool IsIndirect();

        /// <summary>
        /// Sets the indirect flag for this session.  Subsequent changes will
        /// be marked as indirect until this flag is changed again.
        /// </summary>
        void SetToIndirect();

        /// <summary>
        /// Clears the indirect flag for this session.  Subsequent changes will
        /// be marked as direct until this flag is changed again.
        /// </summary>
        void SetToDirect();

        /// <summary>
        /// Determines if there are any tracked changes currently within the
        /// data for this session.
        /// </summary>
        /// <returns>
        /// Non-zero if there are no changes within the data for this session;
        /// otherwise, zero.
        /// </returns>
        bool IsEmpty();

        /// <summary>
        /// Upon success, causes changes to the specified table(s) to start
        /// being tracked.  Any tables impacted by calls to this method will
        /// not cause the <see cref="SessionTableFilterCallback" /> callback
        /// to be invoked.
        /// </summary>
        /// <param name="name">
        /// The name of the table to be tracked -OR- null to track all
        /// applicable tables within this database.
        /// </param>
        void AttachTable(string name);

        /// <summary>
        /// This method is used to set the table filter for this instance.
        /// </summary>
        /// <param name="callback">
        /// The table filter callback -OR- null to clear any existing table
        /// filter callback.
        /// </param>
        /// <param name="clientData">
        /// The optional application-defined context data.  This value may be
        /// null.
        /// </param>
        void SetTableFilter(
            SessionTableFilterCallback callback,
            object clientData
        );

        /// <summary>
        /// Attempts to create and return, via <paramref name="rawData" />, the
        /// combined set of changes represented by this session instance.
        /// </summary>
        /// <param name="rawData">
        /// Upon success, this will contain the raw byte data for all the
        /// changes in this session instance.
        /// </param>
        void CreateChangeSet(ref byte[] rawData);

        /// <summary>
        /// Attempts to create and write, via <paramref name="stream" />, the
        /// combined set of changes represented by this session instance.
        /// </summary>
        /// <param name="stream">
        /// Upon success, the raw byte data for all the changes in this session
        /// instance will be written to this <see cref="Stream" />.
        /// </param>
        void CreateChangeSet(Stream stream);

        /// <summary>
        /// Attempts to create and return, via <paramref name="rawData" />, the
        /// combined set of changes represented by this session instance as a
        /// patch set.
        /// </summary>
        /// <param name="rawData">
        /// Upon success, this will contain the raw byte data for all the
        /// changes in this session instance.
        /// </param>
        void CreatePatchSet(ref byte[] rawData);

        /// <summary>
        /// Attempts to create and write, via <paramref name="stream" />, the
        /// combined set of changes represented by this session instance as a
        /// patch set.
        /// </summary>
        /// <param name="stream">
        /// Upon success, the raw byte data for all the changes in this session
        /// instance will be written to this <see cref="Stream" />.
        /// </param>
        void CreatePatchSet(Stream stream);

        /// <summary>
        /// This method loads the differences between two tables [with the same
        /// name, set of columns, and primary key definition] into this session
        /// instance.
        /// </summary>
        /// <param name="fromDatabaseName">
        /// The name of the database containing the table with the original
        /// data (i.e. it will need updating in order to be identical to the
        /// one within the database associated with this session instance).
        /// </param>
        /// <param name="tableName">
        /// The name of the table.
        /// </param>
        void LoadDifferencesFromTable(
            string fromDatabaseName,
            string tableName
        );
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteSessionHelpers Class
    /// <summary>
    /// This class contains some static helper methods for use within this
    /// subsystem.
    /// </summary>
    internal static class SQLiteSessionHelpers
    {
        #region Public Methods
        /// <summary>
        /// This method checks the byte array specified by the caller to make
        /// sure it will be usable.
        /// </summary>
        /// <param name="rawData">
        /// A byte array provided by the caller into one of the public methods
        /// for the classes that belong to this subsystem.  This value cannot
        /// be null or represent an empty array; otherwise, an appropriate
        /// exception will be thrown.
        /// </param>
        public static void CheckRawData(
            byte[] rawData
            )
        {
            if (rawData == null)
                throw new ArgumentNullException("rawData");

            if (rawData.Length == 0)
            {
                throw new ArgumentException(
                    "empty change set data", "rawData");
            }
        }
        #endregion
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteConnectionLock Class
    /// <summary>
    /// This class is used to hold the native connection handle associated with
    /// a <see cref="SQLiteConnection" /> open until this subsystem is totally
    /// done with it.  This class is for internal use by this subsystem only.
    /// </summary>
    internal abstract class SQLiteConnectionLock : IDisposable
    {
        #region Private Constants
        /// <summary>
        /// The SQL statement used when creating the native statement handle.
        /// There are no special requirements for this other than counting as
        /// an "open statement handle".
        /// </summary>
        private const string LockNopSql = "SELECT 1;";

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The format of the error message used when reporting, during object
        /// disposal, that the statement handle is still open (i.e. because
        /// this situation is considered a fairly serious programming error).
        /// </summary>
        private const string StatementMessageFormat =
            "Connection lock object was {0} with statement {1}";
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Data
        /// <summary>
        /// The wrapped native connection handle associated with this lock.
        /// </summary>
        private SQLiteConnectionHandle handle;

        /// <summary>
        /// The flags associated with the connection represented by the
        /// <see cref="handle" /> value.
        /// </summary>
        private SQLiteConnectionFlags flags;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The native statement handle for this lock.  The garbage collector
        /// cannot cause this statement to be finalized; therefore, it will
        /// serve to hold the associated native connection open until it is
        /// freed manually using the <see cref="Unlock" /> method.
        /// </summary>
        private IntPtr statement;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs a new instance of this class using the specified wrapped
        /// native connection handle and associated flags.
        /// </summary>
        /// <param name="handle">
        /// The wrapped native connection handle to be associated with this
        /// lock.
        /// </param>
        /// <param name="flags">
        /// The flags associated with the connection represented by the
        /// <paramref name="handle" /> value.
        /// </param>
        /// <param name="autoLock">
        /// Non-zero if the <see cref="Lock" /> method should be called prior
        /// to returning from this constructor.
        /// </param>
        public SQLiteConnectionLock(
            SQLiteConnectionHandle handle,
            SQLiteConnectionFlags flags,
            bool autoLock
            )
        {
            this.handle = handle;
            this.flags = flags;

            if (autoLock)
                Lock();
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Protected Methods
        /// <summary>
        /// Queries and returns the wrapped native connection handle for this
        /// instance.
        /// </summary>
        /// <returns>
        /// The wrapped native connection handle for this instance -OR- null
        /// if it is unavailable.
        /// </returns>
        protected SQLiteConnectionHandle GetHandle()
        {
            return handle;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Queries and returns the flags associated with the connection for
        /// this instance.
        /// </summary>
        /// <returns>
        /// The <see cref="SQLiteConnectionFlags" /> value.  There is no return
        /// value reserved to indicate an error.
        /// </returns>
        protected SQLiteConnectionFlags GetFlags()
        {
            return flags;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Queries and returns the native connection handle for this instance.
        /// </summary>
        /// <returns>
        /// The native connection handle for this instance.  If this value is
        /// unavailable or invalid an exception will be thrown.
        /// </returns>
        protected IntPtr GetIntPtr()
        {
            if (handle == null)
            {
                throw new InvalidOperationException(
                    "Connection lock object has an invalid handle.");
            }

            IntPtr handlePtr = handle;

            if (handlePtr == IntPtr.Zero)
            {
                throw new InvalidOperationException(
                    "Connection lock object has an invalid handle pointer.");
            }

            return handlePtr;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Methods
        /// <summary>
        /// This method attempts to "lock" the associated native connection
        /// handle by preparing a SQL statement that will not be finalized
        /// until the <see cref="Unlock" /> method is called (i.e. and which
        /// cannot be done by the garbage collector).  If the statement is
        /// already prepared, nothing is done.  If the statement cannot be
        /// prepared for any reason, an exception will be thrown.
        /// </summary>
        public void Lock()
        {
            CheckDisposed();

            if (statement != IntPtr.Zero)
                return;

            IntPtr pSql = IntPtr.Zero;

            try
            {
                int nSql = 0;

                pSql = SQLiteString.Utf8IntPtrFromString(LockNopSql, ref nSql);

                IntPtr pRemain = IntPtr.Zero;

#if !SQLITE_STANDARD
                int nRemain = 0;
                string functionName = "sqlite3_prepare_interop";

                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3_prepare_interop(
                    GetIntPtr(), pSql, nSql, ref statement, ref pRemain,
                    ref nRemain);
#else
#if USE_PREPARE_V2
                string functionName = "sqlite3_prepare_v2";

                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3_prepare_v2(
                    GetIntPtr(), pSql, nSql, ref statement, ref pRemain);
#else
                string functionName = "sqlite3_prepare";

                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3_prepare(
                    GetIntPtr(), pSql, nSql, ref statement, ref pRemain);
#endif
#endif

                if (rc != SQLiteErrorCode.Ok)
                    throw new SQLiteException(rc, functionName);
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

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This method attempts to "unlock" the associated native connection
        /// handle by finalizing the previously prepared statement.  If the
        /// statement is already finalized, nothing is done.  If the statement
        /// cannot be finalized for any reason, an exception will be thrown.
        /// </summary>
        public void Unlock()
        {
            CheckDisposed();

            if (statement == IntPtr.Zero)
                return;

#if !SQLITE_STANDARD
            string functionName = "sqlite3_finalize_interop";

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3_finalize_interop(
                statement);
#else
            string functionName = "sqlite3_finalize";

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3_finalize(
                statement);
#endif

            if (rc != SQLiteErrorCode.Ok)
                throw new SQLiteException(rc, functionName);

            statement = IntPtr.Zero;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable Members
        /// <summary>
        /// Disposes of this object instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        /// <summary>
        /// Non-zero if this object instance has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Throws an exception if this object instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(SQLiteConnectionLock).Name);
            }
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes or finalizes this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this object is being disposed; otherwise, this object
        /// is being finalized.
        /// </param>
        protected virtual void Dispose(bool disposing)
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

                    if (statement != IntPtr.Zero)
                    {
                        //
                        // NOTE: This should never happen.  This object was
                        //       disposed (or finalized) without the Unlock
                        //       method being called first.
                        //
                        try
                        {
                            if (HelperMethods.LogPrepare(GetFlags()))
                            {
                                /* throw */
                                SQLiteLog.LogMessage(
                                    SQLiteErrorCode.Misuse,
                                    HelperMethods.StringFormat(
                                    CultureInfo.CurrentCulture,
                                    StatementMessageFormat, disposing ?
                                        "disposed" : "finalized",
                                    statement));
                            }
                        }
                        catch
                        {
                            // do nothing.
                        }

#if DEBUG
                        Debugger.Break();
#endif
                    }
                }
            }
            finally
            {
                //
                // NOTE: Everything should be fully disposed at this point.
                //
                disposed = true;
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Destructor
        /// <summary>
        /// Finalizes this object instance.
        /// </summary>
        ~SQLiteConnectionLock()
        {
            Dispose(false);
        }
        #endregion
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteChangeSetIterator Class
    /// <summary>
    /// This class manages the native change set iterator.  It is used as the
    /// base class for the <see cref="SQLiteMemoryChangeSetIterator" /> and
    /// <see cref="SQLiteStreamChangeSetIterator" /> classes.  It knows how to
    /// advance the native iterator handle as well as finalize it.
    /// </summary>
    internal class SQLiteChangeSetIterator : IDisposable
    {
        #region Private Data
        /// <summary>
        /// The native change set (a.k.a. iterator) handle.
        /// </summary>
        private IntPtr iterator;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Non-zero if this instance owns the native iterator handle in the
        /// <see cref="iterator" /> field.  In that case, this instance will
        /// finalize the native iterator handle upon being disposed or
        /// finalized.
        /// </summary>
        private bool ownHandle;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Protected Constructors
        /// <summary>
        /// Constructs a new instance of this class using the specified native
        /// iterator handle.
        /// </summary>
        /// <param name="iterator">
        /// The native iterator handle to use.
        /// </param>
        /// <param name="ownHandle">
        /// Non-zero if this instance is to take ownership of the native
        /// iterator handle specified by <paramref name="iterator" />.
        /// </param>
        protected SQLiteChangeSetIterator(
            IntPtr iterator,
            bool ownHandle
            )
        {
            this.iterator = iterator;
            this.ownHandle = ownHandle;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Methods
        /// <summary>
        /// Throws an exception if the native iterator handle is invalid.
        /// </summary>
        internal void CheckHandle()
        {
            if (iterator == IntPtr.Zero)
                throw new InvalidOperationException("iterator is not open");
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Used to query the native iterator handle.  This method is only used
        /// by the <see cref="SQLiteChangeSetMetadataItem" /> class.
        /// </summary>
        /// <returns>
        /// The native iterator handle -OR- <see cref="IntPtr.Zero" /> if it
        /// is not available.
        /// </returns>
        internal IntPtr GetIntPtr()
        {
            return iterator;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Methods
        /// <summary>
        /// Attempts to advance the native iterator handle to its next item.
        /// </summary>
        /// <returns>
        /// Non-zero if the native iterator handle was advanced and contains
        /// more data; otherwise, zero.  If the underlying native API returns
        /// an unexpected value then an exception will be thrown.
        /// </returns>
        public bool Next()
        {
            CheckDisposed();
            CheckHandle();

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changeset_next(
                iterator);

            switch (rc)
            {
                case SQLiteErrorCode.Ok:
                    {
                        throw new SQLiteException(SQLiteErrorCode.Ok,
                            "sqlite3changeset_next: unexpected result Ok");
                    }
                case SQLiteErrorCode.Row:
                    {
                        return true;
                    }
                case SQLiteErrorCode.Done:
                    {
                        return false;
                    }
                default:
                    {
                        throw new SQLiteException(rc, "sqlite3changeset_next");
                    }
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Static "Factory" Methods
        /// <summary>
        /// Attempts to create an instance of this class that is associated
        /// with the specified native iterator handle.  Ownership of the
        /// native iterator handle is NOT transferred to the new instance of
        /// this class.
        /// </summary>
        /// <param name="iterator">
        /// The native iterator handle to use.
        /// </param>
        /// <returns>
        /// The new instance of this class.  No return value is reserved to
        /// indicate an error; however, if the native iterator handle is not
        /// valid, any subsequent attempt to make use of it via the returned
        /// instance of this class may throw exceptions.
        /// </returns>
        public static SQLiteChangeSetIterator Attach(
            IntPtr iterator
            )
        {
            return new SQLiteChangeSetIterator(iterator, false);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable Members
        /// <summary>
        /// Disposes of this object instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        /// <summary>
        /// Non-zero if this object instance has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Throws an exception if this object instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(SQLiteChangeSetIterator).Name);
            }
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes or finalizes this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this object is being disposed; otherwise, this object
        /// is being finalized.
        /// </param>
        protected virtual void Dispose(bool disposing)
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

                    if (iterator != IntPtr.Zero)
                    {
                        if (ownHandle)
                        {
                            UnsafeNativeMethods.sqlite3changeset_finalize(
                                iterator);
                        }

                        iterator = IntPtr.Zero;
                    }
                }
            }
            finally
            {
                //
                // NOTE: Everything should be fully disposed at this point.
                //
                disposed = true;
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Destructor
        /// <summary>
        /// Finalizes this object instance.
        /// </summary>
        ~SQLiteChangeSetIterator()
        {
            Dispose(false);
        }
        #endregion
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteMemoryChangeSetIterator Class
    /// <summary>
    /// This class manages the native change set iterator for a set of changes
    /// contained entirely in memory.
    /// </summary>
    internal sealed class SQLiteMemoryChangeSetIterator :
        SQLiteChangeSetIterator
    {
        #region Private Data
        /// <summary>
        /// The native memory buffer allocated to contain the set of changes
        /// associated with this instance.  This will always be freed when this
        /// instance is disposed or finalized.
        /// </summary>
        private IntPtr pData;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified native
        /// memory buffer and native iterator handle.
        /// </summary>
        /// <param name="pData">
        /// The native memory buffer to use.
        /// </param>
        /// <param name="iterator">
        /// The native iterator handle to use.
        /// </param>
        /// <param name="ownHandle">
        /// Non-zero if this instance is to take ownership of the native
        /// iterator handle specified by <paramref name="iterator" />.
        /// </param>
        private SQLiteMemoryChangeSetIterator(
            IntPtr pData,
            IntPtr iterator,
            bool ownHandle
            )
            : base(iterator, ownHandle)
        {
            this.pData = pData;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Static "Factory" Methods
        /// <summary>
        /// Attempts to create an instance of this class using the specified
        /// raw byte data.
        /// </summary>
        /// <param name="rawData">
        /// The raw byte data containing the set of changes for this native
        /// iterator.
        /// </param>
        /// <returns>
        /// The new instance of this class -OR- null if it cannot be created.
        /// </returns>
        public static SQLiteMemoryChangeSetIterator Create(
            byte[] rawData
            )
        {
            SQLiteSessionHelpers.CheckRawData(rawData);

            SQLiteMemoryChangeSetIterator result = null;
            IntPtr pData = IntPtr.Zero;
            IntPtr iterator = IntPtr.Zero;

            try
            {
                int nData = 0;

                pData = SQLiteBytes.ToIntPtr(rawData, ref nData);

                if (pData == IntPtr.Zero)
                    throw new SQLiteException(SQLiteErrorCode.NoMem, null);

                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changeset_start(
                    ref iterator, nData, pData);

                if (rc != SQLiteErrorCode.Ok)
                    throw new SQLiteException(rc, "sqlite3changeset_start");

                result = new SQLiteMemoryChangeSetIterator(
                    pData, iterator, true);
            }
            finally
            {
                if (result == null)
                {
                    if (iterator != IntPtr.Zero)
                    {
                        UnsafeNativeMethods.sqlite3changeset_finalize(
                            iterator);

                        iterator = IntPtr.Zero;
                    }

                    if (pData != IntPtr.Zero)
                    {
                        SQLiteMemory.Free(pData);
                        pData = IntPtr.Zero;
                    }
                }
            }

            return result;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to create an instance of this class using the specified
        /// raw byte data.
        /// </summary>
        /// <param name="rawData">
        /// The raw byte data containing the set of changes for this native
        /// iterator.
        /// </param>
        /// <param name="flags">
        /// The flags used to create the change set iterator.
        /// </param>
        /// <returns>
        /// The new instance of this class -OR- null if it cannot be created.
        /// </returns>
        public static SQLiteMemoryChangeSetIterator Create(
            byte[] rawData,
            SQLiteChangeSetStartFlags flags
            )
        {
            SQLiteSessionHelpers.CheckRawData(rawData);

            SQLiteMemoryChangeSetIterator result = null;
            IntPtr pData = IntPtr.Zero;
            IntPtr iterator = IntPtr.Zero;

            try
            {
                int nData = 0;

                pData = SQLiteBytes.ToIntPtr(rawData, ref nData);

                if (pData == IntPtr.Zero)
                    throw new SQLiteException(SQLiteErrorCode.NoMem, null);

                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changeset_start_v2(
                    ref iterator, nData, pData, flags);

                if (rc != SQLiteErrorCode.Ok)
                    throw new SQLiteException(rc, "sqlite3changeset_start_v2");

                result = new SQLiteMemoryChangeSetIterator(
                    pData, iterator, true);
            }
            finally
            {
                if (result == null)
                {
                    if (iterator != IntPtr.Zero)
                    {
                        UnsafeNativeMethods.sqlite3changeset_finalize(
                            iterator);

                        iterator = IntPtr.Zero;
                    }

                    if (pData != IntPtr.Zero)
                    {
                        SQLiteMemory.Free(pData);
                        pData = IntPtr.Zero;
                    }
                }
            }

            return result;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        /// <summary>
        /// Non-zero if this object instance has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Throws an exception if this object instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(SQLiteMemoryChangeSetIterator).Name);
            }
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes or finalizes this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this object is being disposed; otherwise, this object
        /// is being finalized.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            //
            // NOTE: Must dispose of the base class first (leaky abstraction)
            //       because it contains the iterator handle, which must be
            //       closed *prior* to freeing the underlying memory.
            //
            base.Dispose(disposing);

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

                    if (pData != IntPtr.Zero)
                    {
                        SQLiteMemory.Free(pData);
                        pData = IntPtr.Zero;
                    }
                }
            }
            finally
            {
                //
                // NOTE: Everything should be fully disposed at this point.
                //
                disposed = true;
            }
        }
        #endregion
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteStreamChangeSetIterator Class
    /// <summary>
    /// This class manages the native change set iterator for a set of changes
    /// backed by a <see cref="Stream" /> instance.
    /// </summary>
    internal sealed class SQLiteStreamChangeSetIterator :
        SQLiteChangeSetIterator
    {
        #region Private Data
        /// <summary>
        /// The <see cref="SQLiteStreamAdapter" /> instance that is managing
        /// the underlying <see cref="Stream" /> used as the backing store for
        /// the set of changes associated with this native change set iterator.
        /// </summary>
        private SQLiteStreamAdapter streamAdapter;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified native
        /// iterator handle and <see cref="SQLiteStreamAdapter" />.
        /// </summary>
        /// <param name="streamAdapter">
        /// The <see cref="SQLiteStreamAdapter" /> instance to use.
        /// </param>
        /// <param name="iterator">
        /// The native iterator handle to use.
        /// </param>
        /// <param name="ownHandle">
        /// Non-zero if this instance is to take ownership of the native
        /// iterator handle specified by <paramref name="iterator" />.
        /// </param>
        private SQLiteStreamChangeSetIterator(
            SQLiteStreamAdapter streamAdapter,
            IntPtr iterator,
            bool ownHandle
            )
            : base(iterator, ownHandle)
        {
            this.streamAdapter = streamAdapter;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Static "Factory" Methods
        /// <summary>
        /// Attempts to create an instance of this class using the specified
        /// <see cref="Stream" />.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream" /> where the raw byte data for the set of
        /// changes may be read.
        /// </param>
        /// <param name="connectionFlags">
        /// The flags associated with the parent connection.
        /// </param>
        /// <returns>
        /// The new instance of this class -OR- null if it cannot be created.
        /// </returns>
        public static SQLiteStreamChangeSetIterator Create(
            Stream stream,
            SQLiteConnectionFlags connectionFlags
            )
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            SQLiteStreamAdapter streamAdapter = null;
            SQLiteStreamChangeSetIterator result = null;
            IntPtr iterator = IntPtr.Zero;

            try
            {
                streamAdapter = new SQLiteStreamAdapter(stream, connectionFlags);

                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changeset_start_strm(
                    ref iterator, streamAdapter.GetInputDelegate(), IntPtr.Zero);

                if (rc != SQLiteErrorCode.Ok)
                {
                    throw new SQLiteException(
                        rc, "sqlite3changeset_start_strm");
                }

                result = new SQLiteStreamChangeSetIterator(
                    streamAdapter, iterator, true);
            }
            finally
            {
                if (result == null)
                {
                    if (iterator != IntPtr.Zero)
                    {
                        UnsafeNativeMethods.sqlite3changeset_finalize(
                            iterator);

                        iterator = IntPtr.Zero;
                    }

                    if (streamAdapter != null)
                    {
                        streamAdapter.Dispose();
                        streamAdapter = null;
                    }
                }
            }

            return result;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to create an instance of this class using the specified
        /// <see cref="Stream" />.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream" /> where the raw byte data for the set of
        /// changes may be read.
        /// </param>
        /// <param name="connectionFlags">
        /// The flags associated with the parent connection.
        /// </param>
        /// <param name="startFlags">
        /// The flags used to create the change set iterator.
        /// </param>
        /// <returns>
        /// The new instance of this class -OR- null if it cannot be created.
        /// </returns>
        public static SQLiteStreamChangeSetIterator Create(
            Stream stream,
            SQLiteConnectionFlags connectionFlags,
            SQLiteChangeSetStartFlags startFlags
            )
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            SQLiteStreamAdapter streamAdapter = null;
            SQLiteStreamChangeSetIterator result = null;
            IntPtr iterator = IntPtr.Zero;

            try
            {
                streamAdapter = new SQLiteStreamAdapter(stream, connectionFlags);

                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changeset_start_v2_strm(
                    ref iterator, streamAdapter.GetInputDelegate(), IntPtr.Zero,
                    startFlags);

                if (rc != SQLiteErrorCode.Ok)
                {
                    throw new SQLiteException(
                        rc, "sqlite3changeset_start_v2_strm");
                }

                result = new SQLiteStreamChangeSetIterator(
                    streamAdapter, iterator, true);
            }
            finally
            {
                if (result == null)
                {
                    if (iterator != IntPtr.Zero)
                    {
                        UnsafeNativeMethods.sqlite3changeset_finalize(
                            iterator);

                        iterator = IntPtr.Zero;
                    }

                    if (streamAdapter != null)
                    {
                        streamAdapter.Dispose();
                        streamAdapter = null;
                    }
                }
            }

            return result;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        /// <summary>
        /// Non-zero if this object instance has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Throws an exception if this object instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(SQLiteStreamChangeSetIterator).Name);
            }
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes or finalizes this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this object is being disposed; otherwise, this object
        /// is being finalized.
        /// </param>
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
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteStreamAdapter Class
    /// <summary>
    /// This class is used to act as a bridge between a <see cref="Stream" />
    /// instance and the delegates used with the native streaming API.
    /// </summary>
    internal sealed class SQLiteStreamAdapter : IDisposable
    {
        #region Private Data
        /// <summary>
        /// The managed stream instance used to in order to service the native
        /// delegates for both input and output.
        /// </summary>
        private Stream stream;

        /// <summary>
        /// The flags associated with the connection.
        /// </summary>
        private SQLiteConnectionFlags flags;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The delegate used to provide input to the native streaming API.
        /// It will be null -OR- point to the <see cref="Input" /> method.
        /// </summary>
        private UnsafeNativeMethods.xSessionInput xInput;

        /// <summary>
        /// The delegate used to provide output to the native streaming API.
        /// It will be null -OR- point to the <see cref="Output" /> method.
        /// </summary>
        private UnsafeNativeMethods.xSessionOutput xOutput;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs a new instance of this class using the specified managed
        /// stream and connection flags.
        /// </summary>
        /// <param name="stream">
        /// The managed stream instance to be used in order to service the
        /// native delegates for both input and output.
        /// </param>
        /// <param name="flags">
        /// The flags associated with the parent connection.
        /// </param>
        public SQLiteStreamAdapter(
            Stream stream,
            SQLiteConnectionFlags flags
            )
        {
            this.stream = stream;
            this.flags = flags;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Methods
        /// <summary>
        /// Queries and returns the flags associated with the connection for
        /// this instance.
        /// </summary>
        /// <returns>
        /// The <see cref="SQLiteConnectionFlags" /> value.  There is no return
        /// value reserved to indicate an error.
        /// </returns>
        private SQLiteConnectionFlags GetFlags()
        {
            return flags;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Methods
        /// <summary>
        /// Returns a delegate that wraps the <see cref="Input" /> method,
        /// creating it first if necessary.
        /// </summary>
        /// <returns>
        /// A delegate that refers to the <see cref="Input" /> method.
        /// </returns>
        public UnsafeNativeMethods.xSessionInput GetInputDelegate()
        {
            CheckDisposed();

            if (xInput == null)
                xInput = new UnsafeNativeMethods.xSessionInput(Input);

            return xInput;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns a delegate that wraps the <see cref="Output" /> method,
        /// creating it first if necessary.
        /// </summary>
        /// <returns>
        /// A delegate that refers to the <see cref="Output" /> method.
        /// </returns>
        public UnsafeNativeMethods.xSessionOutput GetOutputDelegate()
        {
            CheckDisposed();

            if (xOutput == null)
                xOutput = new UnsafeNativeMethods.xSessionOutput(Output);

            return xOutput;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Native Callback Methods
        /// <summary>
        /// This method attempts to read <paramref name="nData" /> bytes from
        /// the managed stream, writing them to the <paramref name="pData"/>
        /// buffer.
        /// </summary>
        /// <param name="context">
        /// Optional extra context information.  Currently, this will always
        /// have a value of <see cref="IntPtr.Zero" />.
        /// </param>
        /// <param name="pData">
        /// A preallocated native buffer to receive the requested input bytes.
        /// It must be at least <paramref name="nData" /> bytes in size.
        /// </param>
        /// <param name="nData">
        /// Upon entry, the number of bytes to read.  Upon exit, the number of
        /// bytes actually read.  This value may be zero upon exit.
        /// </param>
        /// <returns>
        /// The value <see cref="SQLiteErrorCode.Ok" /> upon success -OR- an
        /// appropriate error code upon failure.
        /// </returns>
        private SQLiteErrorCode Input(
            IntPtr context,
            IntPtr pData,
            ref int nData
            )
        {
            try
            {
                Stream localStream = stream;

                if (localStream == null)
                    return SQLiteErrorCode.Misuse;

                if (nData > 0)
                {
                    byte[] bytes = new byte[nData];
                    int nRead = localStream.Read(bytes, 0, nData);

                    if ((nRead > 0) && (pData != IntPtr.Zero))
                        Marshal.Copy(bytes, 0, pData, nRead);

                    nData = nRead;
                }

                return SQLiteErrorCode.Ok;
            }
            catch (Exception e)
            {
                try
                {
                    if (HelperMethods.LogCallbackExceptions(GetFlags()))
                    {
                        SQLiteLog.LogMessage(
                            SQLiteBase.COR_E_EXCEPTION,
                            HelperMethods.StringFormat(
                            CultureInfo.CurrentCulture,
                            UnsafeNativeMethods.ExceptionMessageFormat,
                            "xSessionInput", e)); /* throw */
                    }
                }
                catch
                {
                    // do nothing.
                }
            }

            return SQLiteErrorCode.IoErr_Read;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This method attempts to write <paramref name="nData" /> bytes to
        /// the managed stream, reading them from the <paramref name="pData"/>
        /// buffer.
        /// </summary>
        /// <param name="context">
        /// Optional extra context information.  Currently, this will always
        /// have a value of <see cref="IntPtr.Zero" />.
        /// </param>
        /// <param name="pData">
        /// A preallocated native buffer containing the requested output
        /// bytes.  It must be at least <paramref name="nData" /> bytes in
        /// size.
        /// </param>
        /// <param name="nData">
        /// The number of bytes to write.
        /// </param>
        /// <returns>
        /// The value <see cref="SQLiteErrorCode.Ok" /> upon success -OR- an
        /// appropriate error code upon failure.
        /// </returns>
        private SQLiteErrorCode Output(
            IntPtr context,
            IntPtr pData,
            int nData
            )
        {
            try
            {
                Stream localStream = stream;

                if (localStream == null)
                    return SQLiteErrorCode.Misuse;

                if (nData > 0)
                {
                    byte[] bytes = new byte[nData];

                    if (pData != IntPtr.Zero)
                        Marshal.Copy(pData, bytes, 0, nData);

                    localStream.Write(bytes, 0, nData);
                }

                localStream.Flush();

                return SQLiteErrorCode.Ok;
            }
            catch (Exception e)
            {
                try
                {
                    if (HelperMethods.LogCallbackExceptions(GetFlags()))
                    {
                        SQLiteLog.LogMessage(
                            SQLiteBase.COR_E_EXCEPTION,
                            HelperMethods.StringFormat(
                            CultureInfo.CurrentCulture,
                            UnsafeNativeMethods.ExceptionMessageFormat,
                            "xSessionOutput", e)); /* throw */
                    }
                }
                catch
                {
                    // do nothing.
                }
            }

            return SQLiteErrorCode.IoErr_Write;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable Members
        /// <summary>
        /// Disposes of this object instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        /// <summary>
        /// Non-zero if this object instance has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Throws an exception if this object instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(SQLiteStreamAdapter).Name);
            }
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes or finalizes this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this object is being disposed; otherwise, this object
        /// is being finalized.
        /// </param>
        private /* protected virtual */ void Dispose(bool disposing)
        {
            try
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        ////////////////////////////////////
                        // dispose managed resources here...
                        ////////////////////////////////////

                        if (xInput != null)
                            xInput = null;

                        if (xOutput != null)
                            xOutput = null;

                        if (stream != null)
                            stream = null; /* NOT OWNED */
                    }

                    //////////////////////////////////////
                    // release unmanaged resources here...
                    //////////////////////////////////////
                }
            }
            finally
            {
                //
                // NOTE: Everything should be fully disposed at this point.
                //
                disposed = true;
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Destructor
        /// <summary>
        /// Finalizes this object instance.
        /// </summary>
        ~SQLiteStreamAdapter()
        {
            Dispose(false);
        }
        #endregion
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteSessionStreamManager Class
    /// <summary>
    /// This class manages a collection of <see cref="SQLiteStreamAdapter"/>
    /// instances. When used, it takes responsibility for creating, returning,
    /// and disposing of its <see cref="SQLiteStreamAdapter"/> instances.
    /// </summary>
    internal sealed class SQLiteSessionStreamManager : IDisposable
    {
        #region Private Data
        /// <summary>
        /// The managed collection of <see cref="SQLiteStreamAdapter" />
        /// instances, keyed by their associated <see cref="Stream" />
        /// instance.
        /// </summary>
        private Dictionary<Stream, SQLiteStreamAdapter> streamAdapters;

        /// <summary>
        /// The flags associated with the connection.
        /// </summary>
        private SQLiteConnectionFlags flags;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs a new instance of this class using the specified
        /// connection flags.
        /// </summary>
        /// <param name="flags">
        /// The flags associated with the parent connection.
        /// </param>
        public SQLiteSessionStreamManager(
            SQLiteConnectionFlags flags
            )
        {
            this.flags = flags;

            InitializeStreamAdapters();
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Methods
        /// <summary>
        /// Makes sure the collection of <see cref="SQLiteStreamAdapter" />
        /// is created.
        /// </summary>
        private void InitializeStreamAdapters()
        {
            if (streamAdapters != null)
                return;

            streamAdapters = new Dictionary<Stream, SQLiteStreamAdapter>();
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Makes sure the collection of <see cref="SQLiteStreamAdapter" />
        /// is disposed.
        /// </summary>
        private void DisposeStreamAdapters()
        {
            if (streamAdapters == null)
                return;

            foreach (KeyValuePair<Stream, SQLiteStreamAdapter> pair
                    in streamAdapters)
            {
                SQLiteStreamAdapter streamAdapter = pair.Value;

                if (streamAdapter == null)
                    continue;

                streamAdapter.Dispose();
            }

            streamAdapters.Clear();
            streamAdapters = null;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Methods
        /// <summary>
        /// Attempts to return a <see cref="SQLiteStreamAdapter" /> instance
        /// suitable for the specified <see cref="Stream" />.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream" /> instance.  If this value is null, a null
        /// value will be returned.
        /// </param>
        /// <returns>
        /// A <see cref="SQLiteStreamAdapter" /> instance.  Typically, these
        /// are always freshly created; however, this method is designed to
        /// return the existing <see cref="SQLiteStreamAdapter" /> instance
        /// associated with the specified stream, should one exist.
        /// </returns>
        public SQLiteStreamAdapter GetAdapter(
            Stream stream
            )
        {
            CheckDisposed();

            if (stream == null)
                return null;

            SQLiteStreamAdapter streamAdapter;

            if (streamAdapters.TryGetValue(stream, out streamAdapter))
                return streamAdapter;

            streamAdapter = new SQLiteStreamAdapter(stream, flags);
            streamAdapters.Add(stream, streamAdapter);

            return streamAdapter;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable Members
        /// <summary>
        /// Disposes of this object instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        /// <summary>
        /// Non-zero if this object instance has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Throws an exception if this object instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(SQLiteSessionStreamManager).Name);
            }
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes or finalizes this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this object is being disposed; otherwise, this object
        /// is being finalized.
        /// </param>
        private /* protected virtual */ void Dispose(bool disposing)
        {
            try
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        ////////////////////////////////////
                        // dispose managed resources here...
                        ////////////////////////////////////

                        DisposeStreamAdapters();
                    }

                    //////////////////////////////////////
                    // release unmanaged resources here...
                    //////////////////////////////////////
                }
            }
            finally
            {
                //
                // NOTE: Everything should be fully disposed at this point.
                //
                disposed = true;
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Destructor
        /// <summary>
        /// Finalizes this object instance.
        /// </summary>
        ~SQLiteSessionStreamManager()
        {
            Dispose(false);
        }
        #endregion
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteChangeGroup Class
    /// <summary>
    /// This class represents a group of change sets (or patch sets).
    /// </summary>
    internal sealed class SQLiteChangeGroup : ISQLiteChangeGroup
    {
        #region Private Data
        /// <summary>
        /// The <see cref="SQLiteSessionStreamManager" /> instance associated
        /// with this change group.
        /// </summary>
        private SQLiteSessionStreamManager streamManager;

        /// <summary>
        /// The flags associated with the connection.
        /// </summary>
        private SQLiteConnectionFlags flags;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The native handle for this change group.  This will be deleted when
        /// this instance is disposed or finalized.
        /// </summary>
        private IntPtr changeGroup;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs a new instance of this class using the specified
        /// connection flags.
        /// </summary>
        /// <param name="flags">
        /// The flags associated with the parent connection.
        /// </param>
        public SQLiteChangeGroup(
            SQLiteConnectionFlags flags
            )
        {
            this.flags = flags;

            InitializeHandle();
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Methods
        /// <summary>
        /// Throws an exception if the native change group handle is invalid.
        /// </summary>
        private void CheckHandle()
        {
            if (changeGroup == IntPtr.Zero)
                throw new InvalidOperationException("change group not open");
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Makes sure the native change group handle is valid, creating it if
        /// necessary.
        /// </summary>
        private void InitializeHandle()
        {
            if (changeGroup != IntPtr.Zero)
                return;

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changegroup_new(
                ref changeGroup);

            if (rc != SQLiteErrorCode.Ok)
                throw new SQLiteException(rc, "sqlite3changegroup_new");
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Makes sure the <see cref="SQLiteSessionStreamManager" /> instance
        /// is available, creating it if necessary.
        /// </summary>
        private void InitializeStreamManager()
        {
            if (streamManager != null)
                return;

            streamManager = new SQLiteSessionStreamManager(flags);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to return a <see cref="SQLiteStreamAdapter" /> instance
        /// suitable for the specified <see cref="Stream" />.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream" /> instance.  If this value is null, a null
        /// value will be returned.
        /// </param>
        /// <returns>
        /// A <see cref="SQLiteStreamAdapter" /> instance.  Typically, these
        /// are always freshly created; however, this method is designed to
        /// return the existing <see cref="SQLiteStreamAdapter" /> instance
        /// associated with the specified stream, should one exist.
        /// </returns>
        private SQLiteStreamAdapter GetStreamAdapter(
            Stream stream
            )
        {
            InitializeStreamManager();

            return streamManager.GetAdapter(stream);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region ISQLiteChangeGroup Members
        /// <summary>
        /// Attempts to add a change set (or patch set) to this change group
        /// instance.  The underlying data must be contained entirely within
        /// the <paramref name="rawData" /> byte array.
        /// </summary>
        /// <param name="rawData">
        /// The raw byte data for the specified change set (or patch set).
        /// </param>
        public void AddChangeSet(
            byte[] rawData
            )
        {
            CheckDisposed();
            CheckHandle();

            SQLiteSessionHelpers.CheckRawData(rawData);

            IntPtr pData = IntPtr.Zero;

            try
            {
                int nData = 0;

                pData = SQLiteBytes.ToIntPtr(rawData, ref nData);

                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changegroup_add(
                    changeGroup, nData, pData);

                if (rc != SQLiteErrorCode.Ok)
                    throw new SQLiteException(rc, "sqlite3changegroup_add");
            }
            finally
            {
                if (pData != IntPtr.Zero)
                {
                    SQLiteMemory.Free(pData);
                    pData = IntPtr.Zero;
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to add a change set (or patch set) to this change group
        /// instance.  The underlying data will be read from the specified
        /// <see cref="Stream" />.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream" /> instance containing the raw change set
        /// (or patch set) data to read.
        /// </param>
        public void AddChangeSet(
            Stream stream
            )
        {
            CheckDisposed();
            CheckHandle();

            if (stream == null)
                throw new ArgumentNullException("stream");

            SQLiteStreamAdapter streamAdapter = GetStreamAdapter(stream);

            if (streamAdapter == null)
            {
                throw new SQLiteException(
                    "could not get or create adapter for input stream");
            }

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changegroup_add_strm(
                changeGroup, streamAdapter.GetInputDelegate(), IntPtr.Zero);

            if (rc != SQLiteErrorCode.Ok)
                throw new SQLiteException(rc, "sqlite3changegroup_add_strm");
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to create and return, via <paramref name="rawData" />, the
        /// combined set of changes represented by this change group instance.
        /// </summary>
        /// <param name="rawData">
        /// Upon success, this will contain the raw byte data for all the
        /// changes in this change group instance.
        /// </param>
        public void CreateChangeSet(
            ref byte[] rawData
            )
        {
            CheckDisposed();
            CheckHandle();

            IntPtr pData = IntPtr.Zero;

            try
            {
                int nData = 0;

                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changegroup_output(
                    changeGroup, ref nData, ref pData);

                if (rc != SQLiteErrorCode.Ok)
                    throw new SQLiteException(rc, "sqlite3changegroup_output");

                rawData = SQLiteBytes.FromIntPtr(pData, nData);
            }
            finally
            {
                if (pData != IntPtr.Zero)
                {
                    SQLiteMemory.FreeUntracked(pData);
                    pData = IntPtr.Zero;
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to create and write, via <paramref name="stream" />, the
        /// combined set of changes represented by this change group instance.
        /// </summary>
        /// <param name="stream">
        /// Upon success, the raw byte data for all the changes in this change
        /// group instance will be written to this <see cref="Stream" />.
        /// </param>
        public void CreateChangeSet(
            Stream stream
            )
        {
            CheckDisposed();
            CheckHandle();

            if (stream == null)
                throw new ArgumentNullException("stream");

            SQLiteStreamAdapter streamAdapter = GetStreamAdapter(stream);

            if (streamAdapter == null)
            {
                throw new SQLiteException(
                    "could not get or create adapter for output stream");
            }

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changegroup_output_strm(
                changeGroup, streamAdapter.GetOutputDelegate(), IntPtr.Zero);

            if (rc != SQLiteErrorCode.Ok)
                throw new SQLiteException(rc, "sqlite3changegroup_output_strm");
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable Members
        /// <summary>
        /// Disposes of this object instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        /// <summary>
        /// Non-zero if this object instance has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Throws an exception if this object instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(SQLiteChangeGroup).Name);
            }
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes or finalizes this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this object is being disposed; otherwise, this object
        /// is being finalized.
        /// </param>
        private /* protected virtual */ void Dispose(bool disposing)
        {
            try
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        ////////////////////////////////////
                        // dispose managed resources here...
                        ////////////////////////////////////

                        if (streamManager != null)
                        {
                            streamManager.Dispose();
                            streamManager = null;
                        }
                    }

                    //////////////////////////////////////
                    // release unmanaged resources here...
                    //////////////////////////////////////

                    if (changeGroup != IntPtr.Zero)
                    {
                        UnsafeNativeMethods.sqlite3changegroup_delete(
                            changeGroup);

                        changeGroup = IntPtr.Zero;
                    }
                }
            }
            finally
            {
                //
                // NOTE: Everything should be fully disposed at this point.
                //
                disposed = true;
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Destructor
        /// <summary>
        /// Finalizes this object instance.
        /// </summary>
        ~SQLiteChangeGroup()
        {
            Dispose(false);
        }
        #endregion
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteSession Class
    /// <summary>
    /// This class represents the change tracking session associated with a
    /// database.
    /// </summary>
    internal sealed class SQLiteSession : SQLiteConnectionLock, ISQLiteSession
    {
        #region Private Data
        /// <summary>
        /// The <see cref="SQLiteSessionStreamManager" /> instance associated
        /// with this session.
        /// </summary>
        private SQLiteSessionStreamManager streamManager;

        /// <summary>
        /// The name of the database (e.g. "main") for this session.
        /// </summary>
        private string databaseName;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The native handle for this session.  This will be deleted when
        /// this instance is disposed or finalized.
        /// </summary>
        private IntPtr session;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The delegate used to provide table filtering to the native API.
        /// It will be null -OR- point to the <see cref="Filter" /> method.
        /// </summary>
        private UnsafeNativeMethods.xSessionFilter xFilter;

        /// <summary>
        /// The managed callback used to filter tables for this session.  Set
        /// via the <see cref="SetTableFilter" /> method.
        /// </summary>
        private SessionTableFilterCallback tableFilterCallback;

        /// <summary>
        /// The optional application-defined context data that was passed to
        /// the <see cref="SetTableFilter" /> method.  This value may be null.
        /// </summary>
        private object tableFilterClientData;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs a new instance of this class using the specified wrapped
        /// native connection handle and associated flags.
        /// </summary>
        /// <param name="handle">
        /// The wrapped native connection handle to be associated with this
        /// session.
        /// </param>
        /// <param name="flags">
        /// The flags associated with the connection represented by the
        /// <paramref name="handle" /> value.
        /// </param>
        /// <param name="databaseName">
        /// The name of the database (e.g. "main") for this session.
        /// </param>
        public SQLiteSession(
            SQLiteConnectionHandle handle,
            SQLiteConnectionFlags flags,
            string databaseName
            )
            : base(handle, flags, true)
        {
            this.databaseName = databaseName;

            InitializeHandle();
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Methods
        /// <summary>
        /// Throws an exception if the native session handle is invalid.
        /// </summary>
        private void CheckHandle()
        {
            if (session == IntPtr.Zero)
                throw new InvalidOperationException("session is not open");
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Makes sure the native session handle is valid, creating it if
        /// necessary.
        /// </summary>
        private void InitializeHandle()
        {
            if (session != IntPtr.Zero)
                return;

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3session_create(
                GetIntPtr(), SQLiteString.GetUtf8BytesFromString(databaseName),
                ref session);

            if (rc != SQLiteErrorCode.Ok)
                throw new SQLiteException(rc, "sqlite3session_create");
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This method sets up the internal table filtering associated state
        /// of this instance.
        /// </summary>
        /// <param name="callback">
        /// The table filter callback -OR- null to clear any existing table
        /// filter callback.
        /// </param>
        /// <param name="clientData">
        /// The optional application-defined context data.  This value may be
        /// null.
        /// </param>
        /// <returns>
        /// The <see cref="UnsafeNativeMethods.xSessionFilter" /> native
        /// delegate -OR- null to clear any existing table filter.
        /// </returns>
        private UnsafeNativeMethods.xSessionFilter ApplyTableFilter(
            SessionTableFilterCallback callback, /* in: NULL OK */
            object clientData                    /* in: NULL OK */
            )
        {
            tableFilterCallback = callback;
            tableFilterClientData = clientData;

            if (callback == null)
            {
                if (xFilter != null)
                    xFilter = null;

                return null;
            }

            if (xFilter == null)
                xFilter = new UnsafeNativeMethods.xSessionFilter(Filter);

            return xFilter;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Makes sure the <see cref="SQLiteSessionStreamManager" /> instance
        /// is available, creating it if necessary.
        /// </summary>
        private void InitializeStreamManager()
        {
            if (streamManager != null)
                return;

            streamManager = new SQLiteSessionStreamManager(GetFlags());
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to return a <see cref="SQLiteStreamAdapter" /> instance
        /// suitable for the specified <see cref="Stream" />.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream" /> instance.  If this value is null, a null
        /// value will be returned.
        /// </param>
        /// <returns>
        /// A <see cref="SQLiteStreamAdapter" /> instance.  Typically, these
        /// are always freshly created; however, this method is designed to
        /// return the existing <see cref="SQLiteStreamAdapter" /> instance
        /// associated with the specified stream, should one exist.
        /// </returns>
        private SQLiteStreamAdapter GetStreamAdapter(
            Stream stream
            )
        {
            InitializeStreamManager();

            return streamManager.GetAdapter(stream);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Native Callback Methods
        /// <summary>
        /// This method is called when determining if a table needs to be
        /// included in the tracked changes for the associated database.
        /// </summary>
        /// <param name="context">
        /// Optional extra context information.  Currently, this will always
        /// have a value of <see cref="IntPtr.Zero" />.
        /// </param>
        /// <param name="pTblName">
        /// The native pointer to the name of the table.
        /// </param>
        /// <returns>
        /// Non-zero if changes to the specified table should be considered;
        /// otherwise, zero.
        /// </returns>
        private int Filter(
            IntPtr context, /* NOT USED */
            IntPtr pTblName
            )
        {
            try
            {
                return tableFilterCallback(tableFilterClientData,
                    SQLiteString.StringFromUtf8IntPtr(pTblName)) ? 1 : 0;
            }
            catch (Exception e)
            {
                try
                {
                    if (HelperMethods.LogCallbackExceptions(GetFlags()))
                    {
                        SQLiteLog.LogMessage( /* throw */
                            SQLiteBase.COR_E_EXCEPTION,
                            HelperMethods.StringFormat(
                            CultureInfo.CurrentCulture,
                            UnsafeNativeMethods.ExceptionMessageFormat,
                            "xSessionFilter", e));
                    }
                }
                catch
                {
                    // do nothing.
                }
            }

            return 0;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region ISQLiteSession Members
        /// <summary>
        /// Determines if this session is currently tracking changes to its
        /// associated database.
        /// </summary>
        /// <returns>
        /// Non-zero if changes to the associated database are being trakced;
        /// otherwise, zero.
        /// </returns>
        public bool IsEnabled()
        {
            CheckDisposed();
            CheckHandle();

            return UnsafeNativeMethods.sqlite3session_enable(session, -1) != 0;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enables tracking of changes to the associated database.
        /// </summary>
        public void SetToEnabled()
        {
            CheckDisposed();
            CheckHandle();

            UnsafeNativeMethods.sqlite3session_enable(session, 1);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disables tracking of changes to the associated database.
        /// </summary>
        public void SetToDisabled()
        {
            CheckDisposed();
            CheckHandle();

            UnsafeNativeMethods.sqlite3session_enable(session, 0);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Determines if this session is currently set to mark changes as
        /// indirect (i.e. as though they were made via a trigger or foreign
        /// key action).
        /// </summary>
        /// <returns>
        /// Non-zero if changes to the associated database are being marked as
        /// indirect; otherwise, zero.
        /// </returns>
        public bool IsIndirect()
        {
            CheckDisposed();
            CheckHandle();

            return UnsafeNativeMethods.sqlite3session_indirect(session, -1) != 0;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sets the indirect flag for this session.  Subsequent changes will
        /// be marked as indirect until this flag is changed again.
        /// </summary>
        public void SetToIndirect()
        {
            CheckDisposed();
            CheckHandle();

            UnsafeNativeMethods.sqlite3session_indirect(session, 1);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Clears the indirect flag for this session.  Subsequent changes will
        /// be marked as direct until this flag is changed again.
        /// </summary>
        public void SetToDirect()
        {
            CheckDisposed();
            CheckHandle();

            UnsafeNativeMethods.sqlite3session_indirect(session, 0);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Determines if there are any tracked changes currently within the
        /// data for this session.
        /// </summary>
        /// <returns>
        /// Non-zero if there are no changes within the data for this session;
        /// otherwise, zero.
        /// </returns>
        public bool IsEmpty()
        {
            CheckDisposed();
            CheckHandle();

            return UnsafeNativeMethods.sqlite3session_isempty(session) != 0;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Upon success, causes changes to the specified table(s) to start
        /// being tracked.  Any tables impacted by calls to this method will
        /// not cause the <see cref="SessionTableFilterCallback" /> callback
        /// to be invoked.
        /// </summary>
        /// <param name="name">
        /// The name of the table to be tracked -OR- null to track all
        /// applicable tables within this database.
        /// </param>
        public void AttachTable(
            string name /* in: NULL OK */
            )
        {
            CheckDisposed();
            CheckHandle();

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3session_attach(
                session, SQLiteString.GetUtf8BytesFromString(name));

            if (rc != SQLiteErrorCode.Ok)
                throw new SQLiteException(rc, "sqlite3session_attach");
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This method is used to set the table filter for this instance.
        /// </summary>
        /// <param name="callback">
        /// The table filter callback -OR- null to clear any existing table
        /// filter callback.
        /// </param>
        /// <param name="clientData">
        /// The optional application-defined context data.  This value may be
        /// null.
        /// </param>
        public void SetTableFilter(
            SessionTableFilterCallback callback, /* in: NULL OK */
            object clientData                    /* in: NULL OK */
            )
        {
            CheckDisposed();
            CheckHandle();

            UnsafeNativeMethods.sqlite3session_table_filter(
                session, ApplyTableFilter(callback, clientData), IntPtr.Zero);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to create and return, via <paramref name="rawData" />, the
        /// set of changes represented by this session instance.
        /// </summary>
        /// <param name="rawData">
        /// Upon success, this will contain the raw byte data for all the
        /// changes in this session instance.
        /// </param>
        public void CreateChangeSet(
            ref byte[] rawData
            )
        {
            CheckDisposed();
            CheckHandle();

            IntPtr pData = IntPtr.Zero;

            try
            {
                int nData = 0;

                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3session_changeset(
                    session, ref nData, ref pData);

                if (rc != SQLiteErrorCode.Ok)
                    throw new SQLiteException(rc, "sqlite3session_changeset");

                rawData = SQLiteBytes.FromIntPtr(pData, nData);
            }
            finally
            {
                if (pData != IntPtr.Zero)
                {
                    SQLiteMemory.FreeUntracked(pData);
                    pData = IntPtr.Zero;
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to create and write, via <paramref name="stream" />, the
        /// set of changes represented by this session instance.
        /// </summary>
        /// <param name="stream">
        /// Upon success, the raw byte data for all the changes in this session
        /// instance will be written to this <see cref="Stream" />.
        /// </param>
        public void CreateChangeSet(
            Stream stream
            )
        {
            CheckDisposed();
            CheckHandle();

            if (stream == null)
                throw new ArgumentNullException("stream");

            SQLiteStreamAdapter streamAdapter = GetStreamAdapter(stream);

            if (streamAdapter == null)
            {
                throw new SQLiteException(
                    "could not get or create adapter for output stream");
            }

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3session_changeset_strm(
                session, streamAdapter.GetOutputDelegate(), IntPtr.Zero);

            if (rc != SQLiteErrorCode.Ok)
                throw new SQLiteException(rc, "sqlite3session_changeset_strm");
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to create and return, via <paramref name="rawData" />, the
        /// set of changes represented by this session instance as a patch set.
        /// </summary>
        /// <param name="rawData">
        /// Upon success, this will contain the raw byte data for all the
        /// changes in this session instance.
        /// </param>
        public void CreatePatchSet(
            ref byte[] rawData
            )
        {
            CheckDisposed();
            CheckHandle();

            IntPtr pData = IntPtr.Zero;

            try
            {
                int nData = 0;

                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3session_patchset(
                    session, ref nData, ref pData);

                if (rc != SQLiteErrorCode.Ok)
                    throw new SQLiteException(rc, "sqlite3session_patchset");

                rawData = SQLiteBytes.FromIntPtr(pData, nData);
            }
            finally
            {
                if (pData != IntPtr.Zero)
                {
                    SQLiteMemory.FreeUntracked(pData);
                    pData = IntPtr.Zero;
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to create and write, via <paramref name="stream" />, the
        /// set of changes represented by this session instance as a patch set.
        /// </summary>
        /// <param name="stream">
        /// Upon success, the raw byte data for all the changes in this session
        /// instance will be written to this <see cref="Stream" />.
        /// </param>
        public void CreatePatchSet(
            Stream stream
            )
        {
            CheckDisposed();
            CheckHandle();

            if (stream == null)
                throw new ArgumentNullException("stream");

            SQLiteStreamAdapter streamAdapter = GetStreamAdapter(stream);

            if (streamAdapter == null)
            {
                throw new SQLiteException(
                    "could not get or create adapter for output stream");
            }

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3session_patchset_strm(
                session, streamAdapter.GetOutputDelegate(), IntPtr.Zero);

            if (rc != SQLiteErrorCode.Ok)
                throw new SQLiteException(rc, "sqlite3session_patchset_strm");
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This method loads the differences between two tables [with the same
        /// name, set of columns, and primary key definition] into this session
        /// instance.
        /// </summary>
        /// <param name="fromDatabaseName">
        /// The name of the database containing the table with the original
        /// data (i.e. it will need updating in order to be identical to the
        /// one within the database associated with this session instance).
        /// </param>
        /// <param name="tableName">
        /// The name of the table.
        /// </param>
        public void LoadDifferencesFromTable(
            string fromDatabaseName,
            string tableName
            )
        {
            CheckDisposed();
            CheckHandle();

            if (fromDatabaseName == null)
                throw new ArgumentNullException("fromDatabaseName");

            if (tableName == null)
                throw new ArgumentNullException("tableName");

            IntPtr pError = IntPtr.Zero;

            try
            {
                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3session_diff(
                    session, SQLiteString.GetUtf8BytesFromString(fromDatabaseName),
                    SQLiteString.GetUtf8BytesFromString(tableName), ref pError);

                if (rc != SQLiteErrorCode.Ok)
                {
                    string error = null;

                    if (pError != IntPtr.Zero)
                    {
                        error = SQLiteString.StringFromUtf8IntPtr(pError);

                        if (!String.IsNullOrEmpty(error))
                        {
                            error = HelperMethods.StringFormat(
                                CultureInfo.CurrentCulture, ": {0}", error);
                        }
                    }

                    throw new SQLiteException(rc, HelperMethods.StringFormat(
                        CultureInfo.CurrentCulture, "{0}{1}",
                        "sqlite3session_diff", error));
                }
            }
            finally
            {
                if (pError != IntPtr.Zero)
                {
                    SQLiteMemory.FreeUntracked(pError);
                    pError = IntPtr.Zero;
                }
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        /// <summary>
        /// Non-zero if this object instance has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Throws an exception if this object instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
                throw new ObjectDisposedException(typeof(SQLiteSession).Name);
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes or finalizes this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this object is being disposed; otherwise, this object
        /// is being finalized.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        ////////////////////////////////////
                        // dispose managed resources here...
                        ////////////////////////////////////

                        if (xFilter != null)
                            xFilter = null;

                        if (streamManager != null)
                        {
                            streamManager.Dispose();
                            streamManager = null;
                        }
                    }

                    //////////////////////////////////////
                    // release unmanaged resources here...
                    //////////////////////////////////////

                    if (session != IntPtr.Zero)
                    {
                        UnsafeNativeMethods.sqlite3session_delete(session);
                        session = IntPtr.Zero;
                    }

                    Unlock();
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
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteChangeSetBase Class
    /// <summary>
    /// This class represents the abstract concept of a set of changes.  It
    /// acts as the base class for the <see cref="SQLiteMemoryChangeSet" />
    /// and <see cref="SQLiteStreamChangeSet" /> classes.  It derives from
    /// the <see cref="SQLiteConnectionLock" /> class, which is used to hold
    /// the underlying native connection handle open until the instances of
    /// this class are disposed or finalized.  It also provides the ability
    /// to construct wrapped native delegates of the
    /// <see cref="UnsafeNativeMethods.xSessionFilter" /> and
    /// <see cref="UnsafeNativeMethods.xSessionConflict" /> types.
    /// </summary>
    internal class SQLiteChangeSetBase : SQLiteConnectionLock
    {
        #region Private Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified wrapped
        /// native connection handle.
        /// </summary>
        /// <param name="handle">
        /// The wrapped native connection handle to be associated with this
        /// change set.
        /// </param>
        /// <param name="flags">
        /// The flags associated with the connection represented by the
        /// <paramref name="handle" /> value.
        /// </param>
        internal SQLiteChangeSetBase(
            SQLiteConnectionHandle handle,
            SQLiteConnectionFlags flags
            )
            : base(handle, flags, true)
        {
            // do nothing.
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Methods
        /// <summary>
        /// Creates and returns a concrete implementation of the
        /// <see cref="ISQLiteChangeSetMetadataItem" /> interface.
        /// </summary>
        /// <param name="iterator">
        /// The native iterator handle to use.
        /// </param>
        /// <returns>
        /// An instance of the <see cref="ISQLiteChangeSetMetadataItem"/>
        /// interface, which can be used to fetch metadata associated with
        /// the current item in this set of changes.
        /// </returns>
        private ISQLiteChangeSetMetadataItem CreateMetadataItem(
            IntPtr iterator
            )
        {
            return new SQLiteChangeSetMetadataItem(
                SQLiteChangeSetIterator.Attach(iterator));
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Protected Methods
        /// <summary>
        /// Attempts to create a
        /// <see cref="UnsafeNativeMethods.xSessionFilter" /> native delegate
        /// that invokes the specified
        /// <see cref="SessionTableFilterCallback" /> delegate.
        /// </summary>
        /// <param name="tableFilterCallback">
        /// The <see cref="SessionTableFilterCallback" /> to invoke when the
        /// <see cref="UnsafeNativeMethods.xSessionFilter" /> native delegate
        /// is called.  If this value is null then null is returned.
        /// </param>
        /// <param name="clientData">
        /// The optional application-defined context data.  This value may be
        /// null.
        /// </param>
        /// <returns>
        /// The created <see cref="UnsafeNativeMethods.xSessionFilter" />
        /// native delegate -OR- null if it cannot be created.
        /// </returns>
        protected UnsafeNativeMethods.xSessionFilter GetDelegate(
            SessionTableFilterCallback tableFilterCallback,
            object clientData
            )
        {
            if (tableFilterCallback == null)
                return null;

            UnsafeNativeMethods.xSessionFilter xFilter;

            xFilter = new UnsafeNativeMethods.xSessionFilter(
                delegate(IntPtr context, IntPtr pTblName)
            {
                try
                {
                    string name = SQLiteString.StringFromUtf8IntPtr(
                        pTblName);

                    return tableFilterCallback(clientData, name) ? 1 : 0;
                }
                catch (Exception e)
                {
                    try
                    {
                        if (HelperMethods.LogCallbackExceptions(GetFlags()))
                        {
                            SQLiteLog.LogMessage( /* throw */
                                SQLiteBase.COR_E_EXCEPTION,
                                HelperMethods.StringFormat(
                                CultureInfo.CurrentCulture,
                                UnsafeNativeMethods.ExceptionMessageFormat,
                                "xSessionFilter", e));
                        }
                    }
                    catch
                    {
                        // do nothing.
                    }
                }

                return 0;
            });

            return xFilter;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to create a
        /// <see cref="UnsafeNativeMethods.xSessionConflict" /> native delegate
        /// that invokes the specified
        /// <see cref="SessionConflictCallback" /> delegate.
        /// </summary>
        /// <param name="conflictCallback">
        /// The <see cref="SessionConflictCallback" /> to invoke when the
        /// <see cref="UnsafeNativeMethods.xSessionConflict" /> native delegate
        /// is called.  If this value is null then null is returned.
        /// </param>
        /// <param name="clientData">
        /// The optional application-defined context data.  This value may be
        /// null.
        /// </param>
        /// <returns>
        /// The created <see cref="UnsafeNativeMethods.xSessionConflict" />
        /// native delegate -OR- null if it cannot be created.
        /// </returns>
        protected UnsafeNativeMethods.xSessionConflict GetDelegate(
            SessionConflictCallback conflictCallback,
            object clientData
            )
        {
            if (conflictCallback == null)
                return null;

            UnsafeNativeMethods.xSessionConflict xConflict;

            xConflict = new UnsafeNativeMethods.xSessionConflict(
                delegate(IntPtr context,
                         SQLiteChangeSetConflictType type,
                         IntPtr iterator)
            {
                try
                {
                    ISQLiteChangeSetMetadataItem item = CreateMetadataItem(
                        iterator);

                    if (item == null)
                    {
                        throw new SQLiteException(
                            "could not create metadata item");
                    }

                    return conflictCallback(clientData, type, item);
                }
                catch (Exception e)
                {
                    try
                    {
                        if (HelperMethods.LogCallbackExceptions(GetFlags()))
                        {
                            SQLiteLog.LogMessage( /* throw */
                                SQLiteBase.COR_E_EXCEPTION,
                                HelperMethods.StringFormat(
                                CultureInfo.CurrentCulture,
                                UnsafeNativeMethods.ExceptionMessageFormat,
                                "xSessionConflict", e));
                        }
                    }
                    catch
                    {
                        // do nothing.
                    }
                }

                return SQLiteChangeSetConflictResult.Abort;
            });

            return xConflict;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        /// <summary>
        /// Non-zero if this object instance has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Throws an exception if this object instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(SQLiteChangeSetBase).Name);
            }
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes or finalizes this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this object is being disposed; otherwise, this object
        /// is being finalized.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        ////////////////////////////////////
                        // dispose managed resources here...
                        ////////////////////////////////////
                    }

                    //////////////////////////////////////
                    // release unmanaged resources here...
                    //////////////////////////////////////

                    Unlock();
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
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteMemoryChangeSet Class
    /// <summary>
    /// This class represents a set of changes contained entirely in memory.
    /// </summary>
    internal sealed class SQLiteMemoryChangeSet :
        SQLiteChangeSetBase, ISQLiteChangeSet
    {
        #region Private Data
        /// <summary>
        /// The raw byte data for this set of changes.  Since this data must
        /// be marshalled to a native memory buffer before being used, there
        /// must be enough memory available to store at least two times the
        /// amount of data contained within it.
        /// </summary>
        private byte[] rawData;

        /// <summary>
        /// The flags used to create the change set iterator.
        /// </summary>
        private SQLiteChangeSetStartFlags startFlags;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified raw byte
        /// data and wrapped native connection handle.
        /// </summary>
        /// <param name="rawData">
        /// The raw byte data for the specified change set (or patch set).
        /// </param>
        /// <param name="handle">
        /// The wrapped native connection handle to be associated with this
        /// set of changes.
        /// </param>
        /// <param name="connectionFlags">
        /// The flags associated with the connection represented by the
        /// <paramref name="handle" /> value.
        /// </param>
        internal SQLiteMemoryChangeSet(
            byte[] rawData,
            SQLiteConnectionHandle handle,
            SQLiteConnectionFlags connectionFlags
            )
            : base(handle, connectionFlags)
        {
            this.rawData = rawData;
            this.startFlags = SQLiteChangeSetStartFlags.None;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructs an instance of this class using the specified raw byte
        /// data and wrapped native connection handle.
        /// </summary>
        /// <param name="rawData">
        /// The raw byte data for the specified change set (or patch set).
        /// </param>
        /// <param name="handle">
        /// The wrapped native connection handle to be associated with this
        /// set of changes.
        /// </param>
        /// <param name="connectionFlags">
        /// The flags associated with the connection represented by the
        /// <paramref name="handle" /> value.
        /// </param>
        /// <param name="startFlags">
        /// The flags used to create the change set iterator.
        /// </param>
        internal SQLiteMemoryChangeSet(
            byte[] rawData,
            SQLiteConnectionHandle handle,
            SQLiteConnectionFlags connectionFlags,
            SQLiteChangeSetStartFlags startFlags
            )
            : base(handle, connectionFlags)
        {
            this.rawData = rawData;
            this.startFlags = startFlags;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region ISQLiteChangeSet Members
        /// <summary>
        /// This method "inverts" the set of changes within this instance.
        /// Applying an inverted set of changes to a database reverses the
        /// effects of applying the uninverted changes.  Specifically:
        /// <![CDATA[<ul>]]><![CDATA[<li>]]>
        /// Each DELETE change is changed to an INSERT, and
        /// <![CDATA[</li>]]><![CDATA[<li>]]>
        /// Each INSERT change is changed to a DELETE, and
        /// <![CDATA[</li>]]><![CDATA[<li>]]>
        /// For each UPDATE change, the old.* and new.* values are exchanged.
        /// <![CDATA[</li>]]><![CDATA[</ul>]]>
        /// This method does not change the order in which changes appear
        /// within the set of changes. It merely reverses the sense of each
        /// individual change.
        /// </summary>
        /// <returns>
        /// The new <see cref="ISQLiteChangeSet" /> instance that represents
        /// the resulting set of changes.
        /// </returns>
        public ISQLiteChangeSet Invert()
        {
            CheckDisposed();

            SQLiteSessionHelpers.CheckRawData(rawData);

            IntPtr pInData = IntPtr.Zero;
            IntPtr pOutData = IntPtr.Zero;

            try
            {
                int nInData = 0;

                pInData = SQLiteBytes.ToIntPtr(rawData, ref nInData);

                int nOutData = 0;

                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changeset_invert(
                    nInData, pInData, ref nOutData, ref pOutData);

                if (rc != SQLiteErrorCode.Ok)
                    throw new SQLiteException(rc, "sqlite3changeset_invert");

                byte[] newData = SQLiteBytes.FromIntPtr(pOutData, nOutData);

                return new SQLiteMemoryChangeSet(
                    newData, GetHandle(), GetFlags());
            }
            finally
            {
                if (pOutData != IntPtr.Zero)
                {
                    SQLiteMemory.FreeUntracked(pOutData);
                    pOutData = IntPtr.Zero;
                }

                if (pInData != IntPtr.Zero)
                {
                    SQLiteMemory.Free(pInData);
                    pInData = IntPtr.Zero;
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This method combines the specified set of changes with the ones
        /// contained in this instance.
        /// </summary>
        /// <param name="changeSet">
        /// The changes to be combined with those in this instance.
        /// </param>
        /// <returns>
        /// The new <see cref="ISQLiteChangeSet" /> instance that represents
        /// the resulting set of changes.
        /// </returns>
        public ISQLiteChangeSet CombineWith(
            ISQLiteChangeSet changeSet
            )
        {
            CheckDisposed();

            SQLiteSessionHelpers.CheckRawData(rawData);

            SQLiteMemoryChangeSet memoryChangeSet =
                changeSet as SQLiteMemoryChangeSet;

            if (memoryChangeSet == null)
            {
                throw new ArgumentException(
                    "not a memory based change set", "changeSet");
            }

            SQLiteSessionHelpers.CheckRawData(memoryChangeSet.rawData);

            IntPtr pInData1 = IntPtr.Zero;
            IntPtr pInData2 = IntPtr.Zero;
            IntPtr pOutData = IntPtr.Zero;

            try
            {
                int nInData1 = 0;

                pInData1 = SQLiteBytes.ToIntPtr(rawData, ref nInData1);

                int nInData2 = 0;

                pInData2 = SQLiteBytes.ToIntPtr(
                    memoryChangeSet.rawData, ref nInData2);

                int nOutData = 0;

                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changeset_concat(
                    nInData1, pInData1, nInData2, pInData2, ref nOutData,
                    ref pOutData);

                if (rc != SQLiteErrorCode.Ok)
                    throw new SQLiteException(rc, "sqlite3changeset_concat");

                byte[] newData = SQLiteBytes.FromIntPtr(pOutData, nOutData);

                return new SQLiteMemoryChangeSet(
                    newData, GetHandle(), GetFlags());
            }
            finally
            {
                if (pOutData != IntPtr.Zero)
                {
                    SQLiteMemory.FreeUntracked(pOutData);
                    pOutData = IntPtr.Zero;
                }

                if (pInData2 != IntPtr.Zero)
                {
                    SQLiteMemory.Free(pInData2);
                    pInData2 = IntPtr.Zero;
                }

                if (pInData1 != IntPtr.Zero)
                {
                    SQLiteMemory.Free(pInData1);
                    pInData1 = IntPtr.Zero;
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to apply the set of changes in this instance to the
        /// associated database.
        /// </summary>
        /// <param name="conflictCallback">
        /// The <see cref="SessionConflictCallback" /> delegate that will need
        /// to handle any conflicting changes that may arise.
        /// </param>
        /// <param name="clientData">
        /// The optional application-defined context data.  This value may be
        /// null.
        /// </param>
        public void Apply(
            SessionConflictCallback conflictCallback,
            object clientData
            )
        {
            CheckDisposed();

            Apply(conflictCallback, null, clientData);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to apply the set of changes in this instance to the
        /// associated database.
        /// </summary>
        /// <param name="conflictCallback">
        /// The <see cref="SessionConflictCallback" /> delegate that will need
        /// to handle any conflicting changes that may arise.
        /// </param>
        /// <param name="tableFilterCallback">
        /// The optional <see cref="SessionTableFilterCallback" /> delegate
        /// that can be used to filter the list of tables impacted by the set
        /// of changes.
        /// </param>
        /// <param name="clientData">
        /// The optional application-defined context data.  This value may be
        /// null.
        /// </param>
        public void Apply(
            SessionConflictCallback conflictCallback,
            SessionTableFilterCallback tableFilterCallback,
            object clientData
            )
        {
            CheckDisposed();

            SQLiteSessionHelpers.CheckRawData(rawData);

            if (conflictCallback == null)
                throw new ArgumentNullException("conflictCallback");

            UnsafeNativeMethods.xSessionFilter xFilter = GetDelegate(
                tableFilterCallback, clientData);

            UnsafeNativeMethods.xSessionConflict xConflict = GetDelegate(
                conflictCallback, clientData);

            IntPtr pData = IntPtr.Zero;

            try
            {
                int nData = 0;

                pData = SQLiteBytes.ToIntPtr(rawData, ref nData);

                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changeset_apply(
                    GetIntPtr(), nData, pData, xFilter, xConflict, IntPtr.Zero);

                if (rc != SQLiteErrorCode.Ok)
                    throw new SQLiteException(rc, "sqlite3changeset_apply");
            }
            finally
            {
                if (pData != IntPtr.Zero)
                {
                    SQLiteMemory.Free(pData);
                    pData = IntPtr.Zero;
                }
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IEnumerable<ISQLiteChangeSetMetadataItem> Members
        /// <summary>
        /// Creates an <see cref="IEnumerator" /> capable of iterating over the
        /// items within this set of changes.
        /// </summary>
        /// <returns>
        /// The new <see cref="IEnumerator{ISQLiteChangeSetMetadataItem}" />
        /// instance.
        /// </returns>
        public IEnumerator<ISQLiteChangeSetMetadataItem> GetEnumerator()
        {
            if (startFlags != SQLiteChangeSetStartFlags.None)
            {
                return new SQLiteMemoryChangeSetEnumerator(
                    rawData, startFlags);
            }
            else
            {
                return new SQLiteMemoryChangeSetEnumerator(rawData);
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IEnumerable Members
        /// <summary>
        /// Creates an <see cref="IEnumerator" /> capable of iterating over the
        /// items within this set of changes.
        /// </summary>
        /// <returns>
        /// The new <see cref="IEnumerator" /> instance.
        /// </returns>
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        /// <summary>
        /// Non-zero if this object instance has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Throws an exception if this object instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(SQLiteMemoryChangeSet).Name);
            }
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes or finalizes this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this object is being disposed; otherwise, this object
        /// is being finalized.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        ////////////////////////////////////
                        // dispose managed resources here...
                        ////////////////////////////////////

                        if (rawData != null)
                            rawData = null;
                    }

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
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteStreamChangeSet Class
    /// <summary>
    /// This class represents a set of changes that are backed by a
    /// <see cref="Stream" /> instance.
    /// </summary>
    internal sealed class SQLiteStreamChangeSet :
        SQLiteChangeSetBase, ISQLiteChangeSet
    {
        #region Private Data
        /// <summary>
        /// The <see cref="SQLiteStreamAdapter" /> instance that is managing
        /// the underlying input <see cref="Stream" /> used as the backing
        /// store for the set of changes associated with this instance.
        /// </summary>
        private SQLiteStreamAdapter inputStreamAdapter;

        /// <summary>
        /// The <see cref="SQLiteStreamAdapter" /> instance that is managing
        /// the underlying output <see cref="Stream" /> used as the backing
        /// store for the set of changes generated by the <see cref="Invert" />
        /// or <see cref="CombineWith" /> methods.
        /// </summary>
        private SQLiteStreamAdapter outputStreamAdapter;

        /// <summary>
        /// The <see cref="Stream" /> instance used as the backing store for
        /// the set of changes associated with this instance.
        /// </summary>
        private Stream inputStream;

        /// <summary>
        /// The <see cref="Stream" /> instance used as the backing store for
        /// the set of changes generated by the <see cref="Invert" /> or
        /// <see cref="CombineWith" /> methods.
        /// </summary>
        private Stream outputStream;

        /// <summary>
        /// The flags used to create the change set iterator.
        /// </summary>
        private SQLiteChangeSetStartFlags startFlags;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified streams
        /// and wrapped native connection handle.
        /// </summary>
        /// <param name="inputStream">
        /// The <see cref="Stream" /> where the raw byte data for the set of
        /// changes may be read.
        /// </param>
        /// <param name="outputStream">
        /// The <see cref="Stream" /> where the raw byte data for resulting
        /// sets of changes may be written.
        /// </param>
        /// <param name="handle">
        /// The wrapped native connection handle to be associated with this
        /// set of changes.
        /// </param>
        /// <param name="connectionFlags">
        /// The flags associated with the connection represented by the
        /// <paramref name="handle" /> value.
        /// </param>
        internal SQLiteStreamChangeSet(
            Stream inputStream,
            Stream outputStream,
            SQLiteConnectionHandle handle,
            SQLiteConnectionFlags connectionFlags
            )
            : base(handle, connectionFlags)
        {
            this.inputStream = inputStream;
            this.outputStream = outputStream;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructs an instance of this class using the specified streams
        /// and wrapped native connection handle.
        /// </summary>
        /// <param name="inputStream">
        /// The <see cref="Stream" /> where the raw byte data for the set of
        /// changes may be read.
        /// </param>
        /// <param name="outputStream">
        /// The <see cref="Stream" /> where the raw byte data for resulting
        /// sets of changes may be written.
        /// </param>
        /// <param name="handle">
        /// The wrapped native connection handle to be associated with this
        /// set of changes.
        /// </param>
        /// <param name="connectionFlags">
        /// The flags associated with the connection represented by the
        /// <paramref name="handle" /> value.
        /// </param>
        /// <param name="startFlags">
        /// The flags used to create the change set iterator.
        /// </param>
        internal SQLiteStreamChangeSet(
            Stream inputStream,
            Stream outputStream,
            SQLiteConnectionHandle handle,
            SQLiteConnectionFlags connectionFlags,
            SQLiteChangeSetStartFlags startFlags
            )
            : base(handle, connectionFlags)
        {
            this.inputStream = inputStream;
            this.outputStream = outputStream;
            this.startFlags = startFlags;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Methods
        /// <summary>
        /// Throws an exception if the input stream or its associated stream
        /// adapter are invalid.
        /// </summary>
        private void CheckInputStream()
        {
            if (inputStream == null)
            {
                throw new InvalidOperationException(
                    "input stream unavailable");
            }

            if (inputStreamAdapter == null)
            {
                inputStreamAdapter = new SQLiteStreamAdapter(
                    inputStream, GetFlags());
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Throws an exception if the output stream or its associated stream
        /// adapter are invalid.
        /// </summary>
        private void CheckOutputStream()
        {
            if (outputStream == null)
            {
                throw new InvalidOperationException(
                    "output stream unavailable");
            }

            if (outputStreamAdapter == null)
            {
                outputStreamAdapter = new SQLiteStreamAdapter(
                    outputStream, GetFlags());
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region ISQLiteChangeSet Members
        /// <summary>
        /// This method "inverts" the set of changes within this instance.
        /// Applying an inverted set of changes to a database reverses the
        /// effects of applying the uninverted changes.  Specifically:
        /// <![CDATA[<ul>]]><![CDATA[<li>]]>
        /// Each DELETE change is changed to an INSERT, and
        /// <![CDATA[</li>]]><![CDATA[<li>]]>
        /// Each INSERT change is changed to a DELETE, and
        /// <![CDATA[</li>]]><![CDATA[<li>]]>
        /// For each UPDATE change, the old.* and new.* values are exchanged.
        /// <![CDATA[</li>]]><![CDATA[</ul>]]>
        /// This method does not change the order in which changes appear
        /// within the set of changes. It merely reverses the sense of each
        /// individual change.
        /// </summary>
        /// <returns>
        /// Since the resulting set of changes is written to the output stream,
        /// this method always returns null.
        /// </returns>
        public ISQLiteChangeSet Invert()
        {
            CheckDisposed();
            CheckInputStream();
            CheckOutputStream();

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changeset_invert_strm(
                inputStreamAdapter.GetInputDelegate(), IntPtr.Zero,
                outputStreamAdapter.GetOutputDelegate(), IntPtr.Zero);

            if (rc != SQLiteErrorCode.Ok)
                throw new SQLiteException(rc, "sqlite3changeset_invert_strm");

            return null;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This method combines the specified set of changes with the ones
        /// contained in this instance.
        /// </summary>
        /// <param name="changeSet">
        /// The changes to be combined with those in this instance.
        /// </param>
        /// <returns>
        /// Since the resulting set of changes is written to the output stream,
        /// this method always returns null.
        /// </returns>
        public ISQLiteChangeSet CombineWith(
            ISQLiteChangeSet changeSet
            )
        {
            CheckDisposed();
            CheckInputStream();
            CheckOutputStream();

            SQLiteStreamChangeSet streamChangeSet =
                changeSet as SQLiteStreamChangeSet;

            if (streamChangeSet == null)
            {
                throw new ArgumentException(
                    "not a stream based change set", "changeSet");
            }

            streamChangeSet.CheckInputStream();

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changeset_concat_strm(
                inputStreamAdapter.GetInputDelegate(), IntPtr.Zero,
                streamChangeSet.inputStreamAdapter.GetInputDelegate(),
                IntPtr.Zero, outputStreamAdapter.GetOutputDelegate(),
                IntPtr.Zero);

            if (rc != SQLiteErrorCode.Ok)
                throw new SQLiteException(rc, "sqlite3changeset_concat_strm");

            return null;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to apply the set of changes in this instance to the
        /// associated database.
        /// </summary>
        /// <param name="conflictCallback">
        /// The <see cref="SessionConflictCallback" /> delegate that will need
        /// to handle any conflicting changes that may arise.
        /// </param>
        /// <param name="clientData">
        /// The optional application-defined context data.  This value may be
        /// null.
        /// </param>
        public void Apply(
            SessionConflictCallback conflictCallback,
            object clientData
            )
        {
            CheckDisposed();

            Apply(conflictCallback, null, clientData);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to apply the set of changes in this instance to the
        /// associated database.
        /// </summary>
        /// <param name="conflictCallback">
        /// The <see cref="SessionConflictCallback" /> delegate that will need
        /// to handle any conflicting changes that may arise.
        /// </param>
        /// <param name="tableFilterCallback">
        /// The optional <see cref="SessionTableFilterCallback" /> delegate
        /// that can be used to filter the list of tables impacted by the set
        /// of changes.
        /// </param>
        /// <param name="clientData">
        /// The optional application-defined context data.  This value may be
        /// null.
        /// </param>
        public void Apply(
            SessionConflictCallback conflictCallback,
            SessionTableFilterCallback tableFilterCallback,
            object clientData
            )
        {
            CheckDisposed();
            CheckInputStream();

            if (conflictCallback == null)
                throw new ArgumentNullException("conflictCallback");

            UnsafeNativeMethods.xSessionFilter xFilter = GetDelegate(
                tableFilterCallback, clientData);

            UnsafeNativeMethods.xSessionConflict xConflict = GetDelegate(
                conflictCallback, clientData);

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changeset_apply_strm(
                GetIntPtr(), inputStreamAdapter.GetInputDelegate(), IntPtr.Zero,
                xFilter, xConflict, IntPtr.Zero);

            if (rc != SQLiteErrorCode.Ok)
                throw new SQLiteException(rc, "sqlite3changeset_apply_strm");
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IEnumerable<ISQLiteChangeSetMetadataItem> Members
        /// <summary>
        /// Creates an <see cref="IEnumerator" /> capable of iterating over the
        /// items within this set of changes.
        /// </summary>
        /// <returns>
        /// The new <see cref="IEnumerator{ISQLiteChangeSetMetadataItem}" />
        /// instance.
        /// </returns>
        public IEnumerator<ISQLiteChangeSetMetadataItem> GetEnumerator()
        {
            if (startFlags != SQLiteChangeSetStartFlags.None)
            {
                return new SQLiteStreamChangeSetEnumerator(
                    inputStream, GetFlags(), startFlags);
            }
            else
            {
                return new SQLiteStreamChangeSetEnumerator(
                    inputStream, GetFlags());
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IEnumerable Members
        /// <summary>
        /// Creates an <see cref="IEnumerator" /> capable of iterating over the
        /// items within this set of changes.
        /// </summary>
        /// <returns>
        /// The new <see cref="IEnumerator" /> instance.
        /// </returns>
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        /// <summary>
        /// Non-zero if this object instance has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Throws an exception if this object instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(SQLiteStreamChangeSet).Name);
            }
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes or finalizes this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this object is being disposed; otherwise, this object
        /// is being finalized.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        ////////////////////////////////////
                        // dispose managed resources here...
                        ////////////////////////////////////

                        if (outputStreamAdapter != null)
                        {
                            outputStreamAdapter.Dispose();
                            outputStreamAdapter = null;
                        }

                        if (inputStreamAdapter != null)
                        {
                            inputStreamAdapter.Dispose();
                            inputStreamAdapter = null;
                        }

                        if (outputStream != null)
                            outputStream = null; /* NOT OWNED */

                        if (inputStream != null)
                            inputStream = null; /* NOT OWNED */
                    }

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
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteChangeSetEnumerator Class
    /// <summary>
    /// This class represents an <see cref="IEnumerator" /> that is capable of
    /// enumerating over a set of changes.  It serves as the base class for the
    /// <see cref="SQLiteMemoryChangeSetEnumerator" /> and
    /// <see cref="SQLiteStreamChangeSetEnumerator" /> classes.  It manages and
    /// owns an instance of the <see cref="SQLiteChangeSetIterator" /> class.
    /// </summary>
    internal abstract class SQLiteChangeSetEnumerator :
        IEnumerator<ISQLiteChangeSetMetadataItem>
    {
        #region Private Data
        /// <summary>
        /// This managed change set iterator is managed and owned by this
        /// class.  It will be disposed when this class is disposed.
        /// </summary>
        private SQLiteChangeSetIterator iterator;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified managed
        /// change set iterator.
        /// </summary>
        /// <param name="iterator">
        /// The managed iterator instance to use.
        /// </param>
        public SQLiteChangeSetEnumerator(
            SQLiteChangeSetIterator iterator
            )
        {
            SetIterator(iterator);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Methods
        /// <summary>
        /// Throws an exception if the managed iterator instance is invalid.
        /// </summary>
        private void CheckIterator()
        {
            if (iterator == null)
                throw new InvalidOperationException("iterator unavailable");

            iterator.CheckHandle();
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sets the managed iterator instance to a new value.
        /// </summary>
        /// <param name="iterator">
        /// The new managed iterator instance to use.
        /// </param>
        private void SetIterator(
            SQLiteChangeSetIterator iterator
            )
        {
            this.iterator = iterator;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes of the managed iterator instance and sets its value to
        /// null.
        /// </summary>
        private void CloseIterator()
        {
            if (iterator != null)
            {
                iterator.Dispose();
                iterator = null;
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Protected Methods
        /// <summary>
        /// Disposes of the existing managed iterator instance and then sets it
        /// to a new value.
        /// </summary>
        /// <param name="iterator">
        /// The new managed iterator instance to use.
        /// </param>
        protected void ResetIterator(
            SQLiteChangeSetIterator iterator
            )
        {
            CloseIterator();
            SetIterator(iterator);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IEnumerator<ISQLiteChangeSetMetadataItem> Members
        /// <summary>
        /// Returns the current change within the set of changes, represented
        /// by a <see cref="ISQLiteChangeSetMetadataItem" /> instance.
        /// </summary>
        public ISQLiteChangeSetMetadataItem Current
        {
            get
            {
                CheckDisposed();

                return new SQLiteChangeSetMetadataItem(iterator);
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IEnumerator Members
        /// <summary>
        /// Returns the current change within the set of changes, represented
        /// by a <see cref="ISQLiteChangeSetMetadataItem" /> instance.
        /// </summary>
        object Collections.IEnumerator.Current
        {
            get
            {
                CheckDisposed();

                return Current;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to advance to the next item in the set of changes.
        /// </summary>
        /// <returns>
        /// Non-zero if more items are available; otherwise, zero.
        /// </returns>
        public bool MoveNext()
        {
            CheckDisposed();
            CheckIterator();

            return iterator.Next();
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Throws <see cref="NotImplementedException" /> because not all the
        /// derived classes are able to support reset functionality.
        /// </summary>
        public virtual void Reset()
        {
            CheckDisposed();

            throw new NotImplementedException();
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable Members
        /// <summary>
        /// Disposes of this object instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        /// <summary>
        /// Non-zero if this object instance has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Throws an exception if this object instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(SQLiteChangeSetEnumerator).Name);
            }
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes or finalizes this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this object is being disposed; otherwise, this object
        /// is being finalized.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        ////////////////////////////////////
                        // dispose managed resources here...
                        ////////////////////////////////////

                        CloseIterator();
                    }

                    //////////////////////////////////////
                    // release unmanaged resources here...
                    //////////////////////////////////////
                }
            }
            finally
            {
                //
                // NOTE: Everything should be fully disposed at this point.
                //
                disposed = true;
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Destructor
        /// <summary>
        /// Finalizes this object instance.
        /// </summary>
        ~SQLiteChangeSetEnumerator()
        {
            Dispose(false);
        }
        #endregion
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteMemoryChangeSetEnumerator Class
    /// <summary>
    /// This class represents an <see cref="IEnumerator" /> that is capable of
    /// enumerating over a set of changes contained entirely in memory.
    /// </summary>
    internal sealed class SQLiteMemoryChangeSetEnumerator :
        SQLiteChangeSetEnumerator
    {
        #region Private Data
        /// <summary>
        /// The raw byte data for this set of changes.  Since this data must
        /// be marshalled to a native memory buffer before being used, there
        /// must be enough memory available to store at least two times the
        /// amount of data contained within it.
        /// </summary>
        private byte[] rawData;

        /// <summary>
        /// The flags used to create the change set iterator.
        /// </summary>
        private SQLiteChangeSetStartFlags flags;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified raw byte
        /// data.
        /// </summary>
        /// <param name="rawData">
        /// The raw byte data containing the set of changes for this
        /// enumerator.
        /// </param>
        public SQLiteMemoryChangeSetEnumerator(
            byte[] rawData
            )
            : base(SQLiteMemoryChangeSetIterator.Create(rawData))
        {
            this.rawData = rawData;
            this.flags = SQLiteChangeSetStartFlags.None;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructs an instance of this class using the specified raw byte
        /// data.
        /// </summary>
        /// <param name="rawData">
        /// The raw byte data containing the set of changes for this
        /// enumerator.
        /// </param>
        /// <param name="flags">
        /// The flags used to create the change set iterator.
        /// </param>
        public SQLiteMemoryChangeSetEnumerator(
            byte[] rawData,
            SQLiteChangeSetStartFlags flags
            )
            : base(SQLiteMemoryChangeSetIterator.Create(rawData, flags))
        {
            this.rawData = rawData;
            this.flags = flags;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IEnumerator Overrides
        /// <summary>
        /// Resets the enumerator to its initial position.
        /// </summary>
        public override void Reset()
        {
            CheckDisposed();

            SQLiteMemoryChangeSetIterator result;

            if (flags != SQLiteChangeSetStartFlags.None)
                result = SQLiteMemoryChangeSetIterator.Create(rawData, flags);
            else
                result = SQLiteMemoryChangeSetIterator.Create(rawData);

            ResetIterator(result);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        /// <summary>
        /// Non-zero if this object instance has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Throws an exception if this object instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(SQLiteMemoryChangeSetEnumerator).Name);
            }
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes or finalizes this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this object is being disposed; otherwise, this object
        /// is being finalized.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        ////////////////////////////////////
                        // dispose managed resources here...
                        ////////////////////////////////////
                    }

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
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteStreamChangeSetEnumerator Class
    /// <summary>
    /// This class represents an <see cref="IEnumerator" /> that is capable of
    /// enumerating over a set of changes backed by a <see cref="Stream" />
    /// instance.
    /// </summary>
    internal sealed class SQLiteStreamChangeSetEnumerator :
        SQLiteChangeSetEnumerator
    {
        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified stream.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream" /> where the raw byte data for the set of
        /// changes may be read.
        /// </param>
        /// <param name="connectionFlags">
        /// The flags associated with the parent connection.
        /// </param>
        public SQLiteStreamChangeSetEnumerator(
            Stream stream,
            SQLiteConnectionFlags connectionFlags
            )
            : base(SQLiteStreamChangeSetIterator.Create(
                stream, connectionFlags))
        {
            // do nothing.
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructs an instance of this class using the specified stream.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="Stream" /> where the raw byte data for the set of
        /// changes may be read.
        /// </param>
        /// <param name="connectionFlags">
        /// The flags associated with the parent connection.
        /// </param>
        /// <param name="startFlags">
        /// The flags used to create the change set iterator.
        /// </param>
        public SQLiteStreamChangeSetEnumerator(
            Stream stream,
            SQLiteConnectionFlags connectionFlags,
            SQLiteChangeSetStartFlags startFlags
            )
            : base(SQLiteStreamChangeSetIterator.Create(
                stream, connectionFlags, startFlags))
        {
            // do nothing.
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        /// <summary>
        /// Non-zero if this object instance has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Throws an exception if this object instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(SQLiteStreamChangeSetEnumerator).Name);
            }
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes or finalizes this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this object is being disposed; otherwise, this object
        /// is being finalized.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                //if (!disposed)
                //{
                //    if (disposing)
                //    {
                //        ////////////////////////////////////
                //        // dispose managed resources here...
                //        ////////////////////////////////////
                //    }

                //    //////////////////////////////////////
                //    // release unmanaged resources here...
                //    //////////////////////////////////////
                //}
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
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteChangeSetMetadataItem Class
    /// <summary>
    /// This interface implements properties and methods used to fetch metadata
    /// about one change within a set of changes for a database.
    /// </summary>
    internal sealed class SQLiteChangeSetMetadataItem :
        ISQLiteChangeSetMetadataItem
    {
        #region Private Data
        /// <summary>
        /// The <see cref="SQLiteChangeSetIterator" /> instance to use.  This
        /// will NOT be owned by this class and will not be disposed upon this
        /// class being disposed or finalized.
        /// </summary>
        private SQLiteChangeSetIterator iterator;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class using the specified iterator
        /// instance.
        /// </summary>
        /// <param name="iterator">
        /// The managed iterator instance to use.
        /// </param>
        public SQLiteChangeSetMetadataItem(
            SQLiteChangeSetIterator iterator
            )
        {
            this.iterator = iterator;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Methods
        /// <summary>
        /// Throws an exception if the managed iterator instance is invalid.
        /// </summary>
        private void CheckIterator()
        {
            if (iterator == null)
                throw new InvalidOperationException("iterator unavailable");

            iterator.CheckHandle();
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Populates the underlying data for the <see cref="TableName" />,
        /// <see cref="NumberOfColumns" />, <see cref="OperationCode" />, and
        /// <see cref="Indirect" /> properties, using the appropriate native
        /// API.
        /// </summary>
        private void PopulateOperationMetadata()
        {
            if ((tableName == null) || (numberOfColumns == null) ||
                (operationCode == null) || (indirect == null))
            {
                CheckIterator();

                IntPtr pTblName = IntPtr.Zero;
                SQLiteAuthorizerActionCode op = SQLiteAuthorizerActionCode.None;
                int bIndirect = 0;
                int nColumns = 0;

                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changeset_op(
                    iterator.GetIntPtr(), ref pTblName, ref nColumns, ref op,
                    ref bIndirect);

                if (rc != SQLiteErrorCode.Ok)
                    throw new SQLiteException(rc, "sqlite3changeset_op");

                tableName = SQLiteString.StringFromUtf8IntPtr(pTblName);
                numberOfColumns = nColumns;
                operationCode = op;
                indirect = (bIndirect != 0);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Populates the underlying data for the
        /// <see cref="PrimaryKeyColumns" /> property using the appropriate
        /// native API.
        /// </summary>
        private void PopulatePrimaryKeyColumns()
        {
            if (primaryKeyColumns == null)
            {
                CheckIterator();

                IntPtr pPrimaryKeys = IntPtr.Zero;
                int nColumns = 0;

                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changeset_pk(
                    iterator.GetIntPtr(), ref pPrimaryKeys, ref nColumns);

                if (rc != SQLiteErrorCode.Ok)
                    throw new SQLiteException(rc, "sqlite3changeset_pk");

                byte[] bytes = SQLiteBytes.FromIntPtr(pPrimaryKeys, nColumns);

                if (bytes != null)
                {
                    primaryKeyColumns = new bool[nColumns];

                    for (int index = 0; index < bytes.Length; index++)
                        primaryKeyColumns[index] = (bytes[index] != 0);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Populates the underlying data for the
        /// <see cref="NumberOfForeignKeyConflicts" /> property using the
        /// appropriate native API.
        /// </summary>
        private void PopulateNumberOfForeignKeyConflicts()
        {
            if (numberOfForeignKeyConflicts == null)
            {
                CheckIterator();

                int conflicts = 0;

                SQLiteErrorCode rc =
                    UnsafeNativeMethods.sqlite3changeset_fk_conflicts(
                        iterator.GetIntPtr(), ref conflicts);

                if (rc != SQLiteErrorCode.Ok)
                {
                    throw new SQLiteException(rc,
                        "sqlite3changeset_fk_conflicts");
                }

                numberOfForeignKeyConflicts = conflicts;
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region ISQLiteChangeSetMetadataItem Members
        /// <summary>
        /// Backing field for the <see cref="TableName" /> property. This value
        /// will be null if this field has not yet been populated via the
        /// underlying native API.
        /// </summary>
        private string tableName;

        /// <summary>
        /// The name of the table the change was made to.
        /// </summary>
        public string TableName
        {
            get
            {
                CheckDisposed();
                PopulateOperationMetadata();

                return tableName;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Backing field for the <see cref="NumberOfColumns" /> property. This
        /// value will be null if this field has not yet been populated via the
        /// underlying native API.
        /// </summary>
        private int? numberOfColumns;

        /// <summary>
        /// The number of columns impacted by this change.  This value can be
        /// used to determine the highest valid column index that may be used
        /// with the <see cref="GetOldValue" />, <see cref="GetNewValue" />,
        /// and <see cref="GetConflictValue" /> methods of this interface.  It
        /// will be this value minus one.
        /// </summary>
        public int NumberOfColumns
        {
            get
            {
                CheckDisposed();
                PopulateOperationMetadata();

                return (int)numberOfColumns;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Backing field for the <see cref="OperationCode" /> property.  This
        /// value will be null if this field has not yet been populated via the
        /// underlying native API.
        /// </summary>
        private SQLiteAuthorizerActionCode? operationCode;

        /// <summary>
        /// This will contain the value
        /// <see cref="SQLiteAuthorizerActionCode.Insert" />,
        /// <see cref="SQLiteAuthorizerActionCode.Update" />, or
        /// <see cref="SQLiteAuthorizerActionCode.Delete" />, corresponding to
        /// the overall type of change this item represents.
        /// </summary>
        public SQLiteAuthorizerActionCode OperationCode
        {
            get
            {
                CheckDisposed();
                PopulateOperationMetadata();

                return (SQLiteAuthorizerActionCode)operationCode;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Backing field for the <see cref="Indirect" /> property.  This value
        /// will be null if this field has not yet been populated via the
        /// underlying native API.
        /// </summary>
        private bool? indirect;

        /// <summary>
        /// Non-zero if this change is considered to be indirect (i.e. as
        /// though they were made via a trigger or foreign key action).
        /// </summary>
        public bool Indirect
        {
            get
            {
                CheckDisposed();
                PopulateOperationMetadata();

                return (bool)indirect;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Backing field for the <see cref="PrimaryKeyColumns" /> property.
        /// This value will be null if this field has not yet been populated
        /// via the underlying native API.
        /// </summary>
        private bool[] primaryKeyColumns;

        /// <summary>
        /// This array contains a <see cref="Boolean" /> for each column in
        /// the table associated with this change.  The element will be zero
        /// if the column is not part of the primary key; otherwise, it will
        /// be non-zero.
        /// </summary>
        public bool[] PrimaryKeyColumns
        {
            get
            {
                CheckDisposed();
                PopulatePrimaryKeyColumns();

                return primaryKeyColumns;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Backing field for the <see cref="NumberOfForeignKeyConflicts" />
        /// property.  This value will be null if this field has not yet been
        /// populated via the underlying native API.
        /// </summary>
        private int? numberOfForeignKeyConflicts;

        /// <summary>
        /// This method may only be called from within a
        /// <see cref="SessionConflictCallback" /> delegate when the conflict
        /// type is <see cref="SQLiteChangeSetConflictType.ForeignKey" />.  It
        /// returns the total number of known foreign key violations in the
        /// destination database.
        /// </summary>
        public int NumberOfForeignKeyConflicts
        {
            get
            {
                CheckDisposed();
                PopulateNumberOfForeignKeyConflicts();

                return (int)numberOfForeignKeyConflicts;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Queries and returns the original value of a given column for this
        /// change.  This method may only be called when the
        /// <see cref="OperationCode" /> has a value of
        /// <see cref="SQLiteAuthorizerActionCode.Update" /> or
        /// <see cref="SQLiteAuthorizerActionCode.Delete" />.
        /// </summary>
        /// <param name="columnIndex">
        /// The index for the column.  This value must be between zero and one
        /// less than the total number of columns for this table.
        /// </param>
        /// <returns>
        /// The original value of a given column for this change.
        /// </returns>
        public SQLiteValue GetOldValue(
            int columnIndex
            )
        {
            CheckDisposed();
            CheckIterator();

            IntPtr pValue = IntPtr.Zero;

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changeset_old(
                iterator.GetIntPtr(), columnIndex, ref pValue);

            return SQLiteValue.FromIntPtr(pValue);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Queries and returns the updated value of a given column for this
        /// change.  This method may only be called when the
        /// <see cref="OperationCode" /> has a value of
        /// <see cref="SQLiteAuthorizerActionCode.Insert" /> or
        /// <see cref="SQLiteAuthorizerActionCode.Update" />.
        /// </summary>
        /// <param name="columnIndex">
        /// The index for the column.  This value must be between zero and one
        /// less than the total number of columns for this table.
        /// </param>
        /// <returns>
        /// The updated value of a given column for this change.
        /// </returns>
        public SQLiteValue GetNewValue(
            int columnIndex
            )
        {
            CheckDisposed();
            CheckIterator();

            IntPtr pValue = IntPtr.Zero;

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changeset_new(
                iterator.GetIntPtr(), columnIndex, ref pValue);

            return SQLiteValue.FromIntPtr(pValue);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Queries and returns the conflicting value of a given column for
        /// this change.  This method may only be called from within a
        /// <see cref="SessionConflictCallback" /> delegate when the conflict
        /// type is <see cref="SQLiteChangeSetConflictType.Data" /> or
        /// <see cref="SQLiteChangeSetConflictType.Conflict" />.
        /// </summary>
        /// <param name="columnIndex">
        /// The index for the column.  This value must be between zero and one
        /// less than the total number of columns for this table.
        /// </param>
        /// <returns>
        /// The conflicting value of a given column for this change.
        /// </returns>
        public SQLiteValue GetConflictValue(
            int columnIndex
            )
        {
            CheckDisposed();
            CheckIterator();

            IntPtr pValue = IntPtr.Zero;

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3changeset_conflict(
                iterator.GetIntPtr(), columnIndex, ref pValue);

            return SQLiteValue.FromIntPtr(pValue);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable Members
        /// <summary>
        /// Disposes of this object instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        /// <summary>
        /// Non-zero if this object instance has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Throws an exception if this object instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(SQLiteChangeSetMetadataItem).Name);
            }
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes or finalizes this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this object is being disposed; otherwise, this object
        /// is being finalized.
        /// </param>
        private /* protected virtual */ void Dispose(bool disposing)
        {
            try
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        ////////////////////////////////////
                        // dispose managed resources here...
                        ////////////////////////////////////

                        if (iterator != null)
                            iterator = null; /* NOT OWNED */
                    }

                    //////////////////////////////////////
                    // release unmanaged resources here...
                    //////////////////////////////////////
                }
            }
            finally
            {
                //
                // NOTE: Everything should be fully disposed at this point.
                //
                disposed = true;
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Destructor
        /// <summary>
        /// Finalizes this object instance.
        /// </summary>
        ~SQLiteChangeSetMetadataItem()
        {
            Dispose(false);
        }
        #endregion
    }
    #endregion
}
