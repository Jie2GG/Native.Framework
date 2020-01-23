/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Joe Mistachkin (joe@mistachkin.com)
 *
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
    using System;

    /// <summary>
    /// Represents a single SQL blob in SQLite.
    /// </summary>
    public sealed class SQLiteBlob : IDisposable
    {
        #region Private Data
        /// <summary>
        /// The underlying SQLite object this blob is bound to.
        /// </summary>
        internal SQLiteBase _sql;

        /// <summary>
        /// The actual blob handle.
        /// </summary>
        internal SQLiteBlobHandle _sqlite_blob;
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////////////////

        #region Private Constructors
        /// <summary>
        /// Initializes the blob.
        /// </summary>
        /// <param name="sqlbase">The base SQLite object.</param>
        /// <param name="blob">The blob handle.</param>
        private SQLiteBlob(
            SQLiteBase sqlbase,
            SQLiteBlobHandle blob
            )
        {
            _sql = sqlbase;
            _sqlite_blob = blob;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////////////////

        #region Static "Factory" Methods
        /// <summary>
        /// Creates a <see cref="SQLiteBlob" /> object.  This will not work
        /// for tables that were created WITHOUT ROWID -OR- if the query
        /// does not include the "rowid" column or one of its aliases -OR-
        /// if the <see cref="SQLiteDataReader" /> was not created with the
        /// <see cref="CommandBehavior.KeyInfo" /> flag.
        /// </summary>
        /// <param name="dataReader">
        /// The <see cref="SQLiteDataReader" /> instance with a result set
        /// containing the desired blob column.
        /// </param>
        /// <param name="i">
        /// The index of the blob column.
        /// </param>
        /// <param name="readOnly">
        /// Non-zero to open the blob object for read-only access.
        /// </param>
        /// <returns>
        /// The newly created <see cref="SQLiteBlob" /> instance -OR- null
        /// if an error occurs.
        /// </returns>
        public static SQLiteBlob Create(
            SQLiteDataReader dataReader,
            int i,
            bool readOnly
            )
        {
            if (dataReader == null)
                throw new ArgumentNullException("dataReader");

            long? rowId = dataReader.GetRowId(i);

            if (rowId == null)
                throw new InvalidOperationException("No RowId is available");

            return Create(
                SQLiteDataReader.GetConnection(dataReader),
                dataReader.GetDatabaseName(i), dataReader.GetTableName(i),
                dataReader.GetName(i), (long)rowId, readOnly);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a <see cref="SQLiteBlob" /> object.  This will not work
        /// for tables that were created WITHOUT ROWID.
        /// </summary>
        /// <param name="connection">
        /// The connection to use when opening the blob object.
        /// </param>
        /// <param name="databaseName">
        /// The name of the database containing the blob object.
        /// </param>
        /// <param name="tableName">
        /// The name of the table containing the blob object.
        /// </param>
        /// <param name="columnName">
        /// The name of the column containing the blob object.
        /// </param>
        /// <param name="rowId">
        /// The integer identifier for the row associated with the desired
        /// blob object.
        /// </param>
        /// <param name="readOnly">
        /// Non-zero to open the blob object for read-only access.
        /// </param>
        /// <returns>
        /// The newly created <see cref="SQLiteBlob" /> instance -OR- null
        /// if an error occurs.
        /// </returns>
        public static SQLiteBlob Create(
            SQLiteConnection connection,
            string databaseName,
            string tableName,
            string columnName,
            long rowId,
            bool readOnly
            )
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            SQLite3 sqlite3 = connection._sql as SQLite3;

            if (sqlite3 == null)
                throw new InvalidOperationException("Connection has no wrapper");

            SQLiteConnectionHandle handle = sqlite3._sql;

            if (handle == null)
                throw new InvalidOperationException("Connection has an invalid handle.");

            SQLiteBlobHandle blob = null;

            try
            {
                // do nothing.
            }
            finally /* NOTE: Thread.Abort() protection. */
            {
                IntPtr ptrBlob = IntPtr.Zero;

                SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3_blob_open(
                    handle, SQLiteConvert.ToUTF8(databaseName),
                    SQLiteConvert.ToUTF8(tableName), SQLiteConvert.ToUTF8(
                    columnName), rowId, readOnly ? 0 : 1, ref ptrBlob);

                if (rc != SQLiteErrorCode.Ok)
                    throw new SQLiteException(rc, null);

                blob = new SQLiteBlobHandle(handle, ptrBlob);
            }

            SQLiteConnection.OnChanged(connection, new ConnectionEventArgs(
                SQLiteConnectionEventType.NewCriticalHandle, null, null,
                null, null, blob, null, new object[] { typeof(SQLiteBlob),
                databaseName, tableName, columnName, rowId, readOnly }));

            return new SQLiteBlob(sqlite3, blob);
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////////////////

        #region Private Methods
        /// <summary>
        /// Throws an exception if the blob object does not appear to be open.
        /// </summary>
        private void CheckOpen()
        {
            if (_sqlite_blob == IntPtr.Zero)
                throw new InvalidOperationException("Blob is not open");
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Throws an exception if an invalid read/write parameter is detected.
        /// </summary>
        /// <param name="buffer">
        /// When reading, this array will be populated with the bytes read from
        /// the underlying database blob.  When writing, this array contains new
        /// values for the specified portion of the underlying database blob.
        /// </param>
        /// <param name="count">
        /// The number of bytes to read or write.
        /// </param>
        /// <param name="offset">
        /// The byte offset, relative to the start of the underlying database
        /// blob, where the read or write operation will begin.
        /// </param>
        private void VerifyParameters(
            byte[] buffer,
            int count,
            int offset
            )
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (offset < 0)
                throw new ArgumentException("Negative offset not allowed.");

            if (count < 0)
                throw new ArgumentException("Negative count not allowed.");

            if (count > buffer.Length)
                throw new ArgumentException("Buffer is too small.");
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////////////////

        #region Public Methods
        /// <summary>
        /// Retargets this object to an underlying database blob for a
        /// different row; the database, table, and column remain exactly
        /// the same.  If this operation fails for any reason, this blob
        /// object is automatically disposed.
        /// </summary>
        /// <param name="rowId">
        /// The integer identifier for the new row.
        /// </param>
        public void Reopen(
            long rowId
            )
        {
            CheckDisposed();
            CheckOpen();

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3_blob_reopen(
                _sqlite_blob, rowId);

            if (rc != SQLiteErrorCode.Ok)
            {
                Dispose();
                throw new SQLiteException(rc, null);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Queries the total number of bytes for the underlying database blob.
        /// </summary>
        /// <returns>
        /// The total number of bytes for the underlying database blob.
        /// </returns>
        public int GetCount()
        {
            CheckDisposed();
            CheckOpen();

            return UnsafeNativeMethods.sqlite3_blob_bytes(_sqlite_blob);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reads data from the underlying database blob.
        /// </summary>
        /// <param name="buffer">
        /// This array will be populated with the bytes read from the
        /// underlying database blob.
        /// </param>
        /// <param name="count">
        /// The number of bytes to read.
        /// </param>
        /// <param name="offset">
        /// The byte offset, relative to the start of the underlying
        /// database blob, where the read operation will begin.
        /// </param>
        public void Read(
            byte[] buffer,
            int count,
            int offset
            )
        {
            CheckDisposed();
            CheckOpen();
            VerifyParameters(buffer, count, offset);

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3_blob_read(
                _sqlite_blob, buffer, count, offset);

            if (rc != SQLiteErrorCode.Ok)
                throw new SQLiteException(rc, null);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Writes data into the underlying database blob.
        /// </summary>
        /// <param name="buffer">
        /// This array contains the new values for the specified portion of
        /// the underlying database blob.
        /// </param>
        /// <param name="count">
        /// The number of bytes to write.
        /// </param>
        /// <param name="offset">
        /// The byte offset, relative to the start of the underlying
        /// database blob, where the write operation will begin.
        /// </param>
        public void Write(
            byte[] buffer,
            int count,
            int offset
            )
        {
            CheckDisposed();
            CheckOpen();
            VerifyParameters(buffer, count, offset);

            SQLiteErrorCode rc = UnsafeNativeMethods.sqlite3_blob_write(
                _sqlite_blob, buffer, count, offset);

            if (rc != SQLiteErrorCode.Ok)
                throw new SQLiteException(rc, null);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Closes the blob, freeing the associated resources.
        /// </summary>
        public void Close()
        {
            Dispose();
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////////////////

        #region IDisposable Members
        /// <summary>
        /// Disposes and finalizes the blob.
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
            throw new ObjectDisposedException(typeof(SQLiteBlob).Name);
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

                    if (_sqlite_blob != null)
                    {
                        _sqlite_blob.Dispose();
                        _sqlite_blob = null;
                    }

                    _sql = null;
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
        /// <summary>
        /// The destructor.
        /// </summary>
        ~SQLiteBlob()
        {
            Dispose(false);
        }
        #endregion
    }
}
