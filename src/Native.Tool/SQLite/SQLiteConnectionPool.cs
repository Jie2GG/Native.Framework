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

#if !PLATFORM_COMPACTFRAMEWORK && DEBUG
    using System.Text;
#endif

    using System.Threading;

    ///////////////////////////////////////////////////////////////////////////

    #region Null Connection Pool Class
#if !PLATFORM_COMPACTFRAMEWORK && DEBUG
    /// <summary>
    /// This class implements a connection pool where all methods of the
    /// <see cref="ISQLiteConnectionPool" /> interface are NOPs.  This class
    /// is used for testing purposes only.
    /// </summary>
    internal sealed class NullConnectionPool : ISQLiteConnectionPool
    {
        #region Private Data
        /// <summary>
        /// This field keeps track of all method calls made into the
        /// <see cref="ISQLiteConnectionPool" /> interface methods of this
        /// class.
        /// </summary>
        private StringBuilder log;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Non-zero to dispose of database connection handles received via the
        /// <see cref="Add" /> method.
        /// </summary>
        private bool dispose;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Constructors
        /// <summary>
        /// Constructs a connection pool object where all methods of the
        /// <see cref="ISQLiteConnectionPool" /> interface are NOPs.  This
        /// class is used for testing purposes only.
        /// </summary>
        private NullConnectionPool()
        {
            log = new StringBuilder();
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs a connection pool object where all methods of the
        /// <see cref="ISQLiteConnectionPool" /> interface are NOPs.  This
        /// class is used for testing purposes only.
        /// </summary>
        /// <param name="dispose">
        /// Non-zero to dispose of database connection handles received via the
        /// <see cref="Add" /> method.
        /// </param>
        public NullConnectionPool(
            bool dispose
            )
            : this()
        {
            this.dispose = dispose;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region ISQLiteConnectionPool Members
        /// <summary>
        /// Counts the number of pool entries matching the specified file name.
        /// </summary>
        /// <param name="fileName">
        /// The file name to match or null to match all files.
        /// </param>
        /// <param name="counts">
        /// The pool entry counts for each matching file.
        /// </param>
        /// <param name="openCount">
        /// The total number of connections successfully opened from any pool.
        /// </param>
        /// <param name="closeCount">
        /// The total number of connections successfully closed from any pool.
        /// </param>
        /// <param name="totalCount">
        /// The total number of pool entries for all matching files.
        /// </param>
        public void GetCounts(
            string fileName,
            ref Dictionary<string, int> counts,
            ref int openCount,
            ref int closeCount,
            ref int totalCount
            )
        {
            if (log != null)
            {
                log.AppendFormat(
                    "GetCounts(\"{0}\", {1}, {2}, {3}, {4}){5}", fileName,
                    counts, openCount, closeCount, totalCount,
                    Environment.NewLine);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes of all pooled connections associated with the specified
        /// database file name.
        /// </summary>
        /// <param name="fileName">
        /// The database file name.
        /// </param>
        public void ClearPool(
            string fileName
            )
        {
            if (log != null)
            {
                log.AppendFormat(
                    "ClearPool(\"{0}\"){1}", fileName, Environment.NewLine);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes of all pooled connections.
        /// </summary>
        public void ClearAllPools()
        {
            if (log != null)
            {
                log.AppendFormat(
                    "ClearAllPools(){0}", Environment.NewLine);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Adds a connection to the pool of those associated with the
        /// specified database file name.
        /// </summary>
        /// <param name="fileName">
        /// The database file name.
        /// </param>
        /// <param name="handle">
        /// The database connection handle.
        /// </param>
        /// <param name="version">
        /// The connection pool version at the point the database connection
        /// handle was received from the connection pool.  This is also the
        /// connection pool version that the database connection handle was
        /// created under.
        /// </param>
        public void Add(
            string fileName,
            object handle,
            int version
            )
        {
            if (log != null)
            {
                log.AppendFormat(
                    "Add(\"{0}\", {1}, {2}){3}", fileName, handle, version,
                    Environment.NewLine);
            }

            //
            // NOTE: If configured to do so, dispose of the received connection
            //       handle now.
            //
            if (dispose)
            {
                IDisposable disposable = handle as IDisposable;

                if (disposable != null)
                    disposable.Dispose();
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Removes a connection from the pool of those associated with the
        /// specified database file name with the intent of using it to
        /// interact with the database.
        /// </summary>
        /// <param name="fileName">
        /// The database file name.
        /// </param>
        /// <param name="maxPoolSize">
        /// The new maximum size of the connection pool for the specified
        /// database file name.
        /// </param>
        /// <param name="version">
        /// The connection pool version associated with the returned database
        /// connection handle, if any.
        /// </param>
        /// <returns>
        /// The database connection handle associated with the specified
        /// database file name or null if it cannot be obtained.
        /// </returns>
        public object Remove(
            string fileName,
            int maxPoolSize,
            out int version
            )
        {
            version = 0;

            if (log != null)
            {
                log.AppendFormat(
                    "Remove(\"{0}\", {1}, {2}){3}", fileName, maxPoolSize,
                    version, Environment.NewLine);
            }

            return null;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region System.Object Overrides
        /// <summary>
        /// Overrides the default <see cref="System.Object.ToString" /> method
        /// to provide a log of all methods called on the
        /// <see cref="ISQLiteConnectionPool" /> interface.
        /// </summary>
        /// <returns>
        /// A string containing a log of all method calls into the
        /// <see cref="ISQLiteConnectionPool" /> interface, along with their
        /// parameters, delimited by <see cref="Environment.NewLine" />.
        /// </returns>
        public override string ToString()
        {
            return (log != null) ? log.ToString() : String.Empty;
        }
        #endregion
    }
#endif
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region Public Connection Pool Interface
    /// <summary>
    /// This interface represents a custom connection pool implementation
    /// usable by System.Data.SQLite.
    /// </summary>
    public interface ISQLiteConnectionPool
    {
        /// <summary>
        /// Counts the number of pool entries matching the specified file name.
        /// </summary>
        /// <param name="fileName">
        /// The file name to match or null to match all files.
        /// </param>
        /// <param name="counts">
        /// The pool entry counts for each matching file.
        /// </param>
        /// <param name="openCount">
        /// The total number of connections successfully opened from any pool.
        /// </param>
        /// <param name="closeCount">
        /// The total number of connections successfully closed from any pool.
        /// </param>
        /// <param name="totalCount">
        /// The total number of pool entries for all matching files.
        /// </param>
        void GetCounts(string fileName, ref Dictionary<string, int> counts,
            ref int openCount, ref int closeCount, ref int totalCount);

        /// <summary>
        /// Disposes of all pooled connections associated with the specified
        /// database file name.
        /// </summary>
        /// <param name="fileName">
        /// The database file name.
        /// </param>
        void ClearPool(string fileName);

        /// <summary>
        /// Disposes of all pooled connections.
        /// </summary>
        void ClearAllPools();

        /// <summary>
        /// Adds a connection to the pool of those associated with the
        /// specified database file name.
        /// </summary>
        /// <param name="fileName">
        /// The database file name.
        /// </param>
        /// <param name="handle">
        /// The database connection handle.
        /// </param>
        /// <param name="version">
        /// The connection pool version at the point the database connection
        /// handle was received from the connection pool.  This is also the
        /// connection pool version that the database connection handle was
        /// created under.
        /// </param>
        void Add(string fileName, object handle, int version);

        /// <summary>
        /// Removes a connection from the pool of those associated with the
        /// specified database file name with the intent of using it to
        /// interact with the database.
        /// </summary>
        /// <param name="fileName">
        /// The database file name.
        /// </param>
        /// <param name="maxPoolSize">
        /// The new maximum size of the connection pool for the specified
        /// database file name.
        /// </param>
        /// <param name="version">
        /// The connection pool version associated with the returned database
        /// connection handle, if any.
        /// </param>
        /// <returns>
        /// The database connection handle associated with the specified
        /// database file name or null if it cannot be obtained.
        /// </returns>
        object Remove(string fileName, int maxPoolSize, out int version);
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region Connection Pool Subsystem & Default Implementation
    /// <summary>
    /// This default method implementations in this class should not be used by
    /// applications that make use of COM (either directly or indirectly) due
    /// to possible deadlocks that can occur during finalization of some COM
    /// objects.
    /// </summary>
    internal static class SQLiteConnectionPool
    {
        #region Private Pool Class
        /// <summary>
        /// Keeps track of connections made on a specified file.  The PoolVersion
        /// dictates whether old objects get returned to the pool or discarded
        /// when no longer in use.
        /// </summary>
        private sealed class PoolQueue
        {
            #region Private Data
            /// <summary>
            /// The queue of weak references to the actual database connection
            /// handles.
            /// </summary>
            internal readonly Queue<WeakReference> Queue =
                new Queue<WeakReference>();

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// This pool version associated with the database connection
            /// handles in this pool queue.
            /// </summary>
            internal int PoolVersion;

            ///////////////////////////////////////////////////////////////////

            /// <summary>
            /// The maximum size of this pool queue.
            /// </summary>
            internal int MaxPoolSize;
            #endregion

            ///////////////////////////////////////////////////////////////////

            #region Private Constructors
            /// <summary>
            /// Constructs a connection pool queue using the specified version
            /// and maximum size.  Normally, all the database connection
            /// handles in this pool are associated with a single database file
            /// name.
            /// </summary>
            /// <param name="version">
            /// The initial pool version for this connection pool queue.
            /// </param>
            /// <param name="maxSize">
            /// The initial maximum size for this connection pool queue.
            /// </param>
            internal PoolQueue(
                int version,
                int maxSize
                )
            {
                PoolVersion = version;
                MaxPoolSize = maxSize;
            }
            #endregion
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Static Data
        /// <summary>
        /// This field is used to synchronize access to the private static data
        /// in this class.
        /// </summary>
        private static readonly object _syncRoot = new object();

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When this field is non-null, it will be used to provide the
        /// implementation of all the connection pool methods; otherwise,
        /// the default method implementations will be used.
        /// </summary>
        private static ISQLiteConnectionPool _connectionPool = null;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The dictionary of connection pools, based on the normalized file
        /// name of the SQLite database.
        /// </summary>
        private static SortedList<string, PoolQueue> _queueList =
            new SortedList<string, PoolQueue>(StringComparer.OrdinalIgnoreCase);

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The default version number new pools will get.
        /// </summary>
        private static int _poolVersion = 1;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The number of connections successfully opened from any pool.
        /// This value is incremented by the Remove method.
        /// </summary>
        private static int _poolOpened = 0;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The number of connections successfully closed from any pool.
        /// This value is incremented by the Add method.
        /// </summary>
        private static int _poolClosed = 0;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region ISQLiteConnectionPool Members (Static, Non-Formal)
        /// <summary>
        /// Counts the number of pool entries matching the specified file name.
        /// </summary>
        /// <param name="fileName">
        /// The file name to match or null to match all files.
        /// </param>
        /// <param name="counts">
        /// The pool entry counts for each matching file.
        /// </param>
        /// <param name="openCount">
        /// The total number of connections successfully opened from any pool.
        /// </param>
        /// <param name="closeCount">
        /// The total number of connections successfully closed from any pool.
        /// </param>
        /// <param name="totalCount">
        /// The total number of pool entries for all matching files.
        /// </param>
        internal static void GetCounts(
            string fileName,
            ref Dictionary<string, int> counts,
            ref int openCount,
            ref int closeCount,
            ref int totalCount
            )
        {
            ISQLiteConnectionPool connectionPool = GetConnectionPool();

            if (connectionPool != null)
            {
                connectionPool.GetCounts(
                    fileName, ref counts, ref openCount, ref closeCount,
                    ref totalCount);
            }
            else
            {
                lock (_syncRoot)
                {
                    openCount = _poolOpened;
                    closeCount = _poolClosed;

                    if (counts == null)
                    {
                        counts = new Dictionary<string, int>(
                            StringComparer.OrdinalIgnoreCase);
                    }

                    if (fileName != null)
                    {
                        PoolQueue queue;

                        if (_queueList.TryGetValue(fileName, out queue))
                        {
                            Queue<WeakReference> poolQueue = queue.Queue;
                            int count = (poolQueue != null) ? poolQueue.Count : 0;

                            counts.Add(fileName, count);
                            totalCount += count;
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<string, PoolQueue> pair in _queueList)
                        {
                            if (pair.Value == null)
                                continue;

                            Queue<WeakReference> poolQueue = pair.Value.Queue;
                            int count = (poolQueue != null) ? poolQueue.Count : 0;

                            counts.Add(pair.Key, count);
                            totalCount += count;
                        }
                    }
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes of all pooled connections associated with the specified
        /// database file name.
        /// </summary>
        /// <param name="fileName">
        /// The database file name.
        /// </param>
        internal static void ClearPool(string fileName)
        {
            ISQLiteConnectionPool connectionPool = GetConnectionPool();

            if (connectionPool != null)
            {
                connectionPool.ClearPool(fileName);
            }
            else
            {
                lock (_syncRoot)
                {
                    PoolQueue queue;

                    if (_queueList.TryGetValue(fileName, out queue))
                    {
                        queue.PoolVersion++;

                        Queue<WeakReference> poolQueue = queue.Queue;
                        if (poolQueue == null) return;

                        while (poolQueue.Count > 0)
                        {
                            WeakReference connection = poolQueue.Dequeue();

                            if (connection == null) continue;

                            SQLiteConnectionHandle handle =
                                connection.Target as SQLiteConnectionHandle;

                            if (handle != null)
                                handle.Dispose();

                            GC.KeepAlive(handle);
                        }
                    }
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes of all pooled connections.
        /// </summary>
        internal static void ClearAllPools()
        {
            ISQLiteConnectionPool connectionPool = GetConnectionPool();

            if (connectionPool != null)
            {
                connectionPool.ClearAllPools();
            }
            else
            {
                lock (_syncRoot)
                {
                    foreach (KeyValuePair<string, PoolQueue> pair in _queueList)
                    {
                        if (pair.Value == null)
                            continue;

                        Queue<WeakReference> poolQueue = pair.Value.Queue;

                        while (poolQueue.Count > 0)
                        {
                            WeakReference connection = poolQueue.Dequeue();

                            if (connection == null) continue;

                            SQLiteConnectionHandle handle =
                                connection.Target as SQLiteConnectionHandle;

                            if (handle != null)
                                handle.Dispose();

                            GC.KeepAlive(handle);
                        }

                        //
                        // NOTE: Keep track of the highest revision so we can
                        //       go one higher when we are finished.
                        //
                        if (_poolVersion <= pair.Value.PoolVersion)
                            _poolVersion = pair.Value.PoolVersion + 1;
                    }

                    //
                    // NOTE: All pools are cleared and we have a new highest
                    //       version number to force all old version active
                    //       items to get discarded instead of going back to
                    //       the queue when they are closed.  We can get away
                    //       with this because we have pumped up the pool
                    //       version out of range of all active connections,
                    //       so they will all get discarded when they try to
                    //       put themselves back into their pools.
                    //
                    _queueList.Clear();
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Adds a connection to the pool of those associated with the
        /// specified database file name.
        /// </summary>
        /// <param name="fileName">
        /// The database file name.
        /// </param>
        /// <param name="handle">
        /// The database connection handle.
        /// </param>
        /// <param name="version">
        /// The connection pool version at the point the database connection
        /// handle was received from the connection pool.  This is also the
        /// connection pool version that the database connection handle was
        /// created under.
        /// </param>
        internal static void Add(
            string fileName,
            SQLiteConnectionHandle handle,
            int version
            )
        {
            ISQLiteConnectionPool connectionPool = GetConnectionPool();

            if (connectionPool != null)
            {
                connectionPool.Add(fileName, handle, version);
            }
            else
            {
                lock (_syncRoot)
                {
                    //
                    // NOTE: If the queue does not exist in the pool, then it
                    //       must have been cleared sometime after the
                    //       connection was created.
                    //
                    PoolQueue queue;

                    if (_queueList.TryGetValue(fileName, out queue) &&
                        (version == queue.PoolVersion))
                    {
                        ResizePool(queue, true);

                        Queue<WeakReference> poolQueue = queue.Queue;
                        if (poolQueue == null) return;

                        poolQueue.Enqueue(new WeakReference(handle, false));
                        Interlocked.Increment(ref _poolClosed);
                    }
                    else
                    {
                        handle.Close();
                    }

                    GC.KeepAlive(handle);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Removes a connection from the pool of those associated with the
        /// specified database file name with the intent of using it to
        /// interact with the database.
        /// </summary>
        /// <param name="fileName">
        /// The database file name.
        /// </param>
        /// <param name="maxPoolSize">
        /// The new maximum size of the connection pool for the specified
        /// database file name.
        /// </param>
        /// <param name="version">
        /// The connection pool version associated with the returned database
        /// connection handle, if any.
        /// </param>
        /// <returns>
        /// The database connection handle associated with the specified
        /// database file name or null if it cannot be obtained.
        /// </returns>
        internal static SQLiteConnectionHandle Remove(
            string fileName,
            int maxPoolSize,
            out int version
            )
        {
            ISQLiteConnectionPool connectionPool = GetConnectionPool();

            if (connectionPool != null)
            {
                return connectionPool.Remove(fileName, maxPoolSize,
                    out version) as SQLiteConnectionHandle;
            }
            else
            {
                int localVersion;
                Queue<WeakReference> poolQueue;

                //
                // NOTE: This lock cannot be held while checking the queue for
                //       available connections because other methods of this
                //       class are called from the GC finalizer thread and we
                //       use the WaitForPendingFinalizers method (below).
                //       Holding this lock while calling that method would
                //       therefore result in a deadlock.  Instead, this lock
                //       is held only while a temporary copy of the queue is
                //       created, and if necessary, when committing changes
                //       back to that original queue prior to returning from
                //       this method.
                //
                lock (_syncRoot)
                {
                    PoolQueue queue;

                    //
                    // NOTE: Default to the highest pool version.
                    //
                    version = _poolVersion;

                    //
                    // NOTE: If we didn't find a pool for this file, create one
                    //       even though it will be empty.  We have to do this
                    //       here because otherwise calling ClearPool() on the
                    //       file will not work for active connections that have
                    //       never seen the pool yet.
                    //
                    if (!_queueList.TryGetValue(fileName, out queue))
                    {
                        queue = new PoolQueue(_poolVersion, maxPoolSize);
                        _queueList.Add(fileName, queue);

                        return null;
                    }

                    //
                    // NOTE: We found a pool for this file, so use its version
                    //       number.
                    //
                    version = localVersion = queue.PoolVersion;
                    queue.MaxPoolSize = maxPoolSize;

                    //
                    // NOTE: Now, resize the pool to the new maximum size, if
                    //       necessary.
                    //
                    ResizePool(queue, false);

                    //
                    // NOTE: Try and get a pooled connection from the queue.
                    //
                    poolQueue = queue.Queue;
                    if (poolQueue == null) return null;

                    //
                    // NOTE: Temporarily tranfer the queue for this file into
                    //       a local variable.  The queue for this file will
                    //       be modified and then committed back to the real
                    //       pool list (below) prior to returning from this
                    //       method.
                    //
                    _queueList.Remove(fileName);
                    poolQueue = new Queue<WeakReference>(poolQueue);
                }

                try
                {
                    while (poolQueue.Count > 0)
                    {
                        WeakReference connection = poolQueue.Dequeue();

                        if (connection == null) continue;

                        SQLiteConnectionHandle handle =
                            connection.Target as SQLiteConnectionHandle;

                        if (handle == null) continue;

                        //
                        // BUGFIX: For ticket [996d13cd87], step #1.  After
                        //         this point, make sure that the finalizer for
                        //         the connection handle just obtained from the
                        //         queue cannot START running (i.e. it may
                        //         still be pending but it will no longer start
                        //         after this point).
                        //
                        GC.SuppressFinalize(handle);

                        try
                        {
                            //
                            // BUGFIX: For ticket [996d13cd87], step #2.  Now,
                            //         we must wait for all pending finalizers
                            //         which have STARTED running and have not
                            //         yet COMPLETED.  This must be done just
                            //         in case the finalizer for the connection
                            //         handle just obtained from the queue has
                            //         STARTED running at some point before
                            //         SuppressFinalize was called on it.
                            //
                            //         After this point, checking properties of
                            //         the connection handle (e.g. IsClosed)
                            //         should work reliably without having to
                            //         worry that they will (due to the
                            //         finalizer) change out from under us.
                            //
                            GC.WaitForPendingFinalizers();

                            //
                            // BUGFIX: For ticket [996d13cd87], step #3.  Next,
                            //         verify that the connection handle is
                            //         actually valid and [still?] not closed
                            //         prior to actually returning it to our
                            //         caller.
                            //
                            if (!handle.IsInvalid && !handle.IsClosed)
                            {
                                Interlocked.Increment(ref _poolOpened);
                                return handle;
                            }
                        }
                        finally
                        {
                            //
                            // BUGFIX: For ticket [996d13cd87], step #4.  Next,
                            //         we must re-register the connection
                            //         handle for finalization now that we have
                            //         a strong reference to it (i.e. the
                            //         finalizer will not run at least until
                            //         the connection is subsequently closed).
                            //
                            GC.ReRegisterForFinalize(handle);
                        }

                        GC.KeepAlive(handle);
                    }
                }
                finally
                {
                    //
                    // BUGFIX: For ticket [996d13cd87], step #5.  Finally,
                    //         commit any changes to the pool/queue for this
                    //         database file.
                    //
                    lock (_syncRoot)
                    {
                        //
                        // NOTE: We must check [again] if a pool exists for
                        //       this file because one may have been added
                        //       while the search for an available connection
                        //       was in progress (above).
                        //
                        PoolQueue queue;
                        Queue<WeakReference> newPoolQueue;
                        bool addPool;

                        if (_queueList.TryGetValue(fileName, out queue))
                        {
                            addPool = false;
                        }
                        else
                        {
                            addPool = true;
                            queue = new PoolQueue(localVersion, maxPoolSize);
                        }

                        newPoolQueue = queue.Queue;

                        while (poolQueue.Count > 0)
                            newPoolQueue.Enqueue(poolQueue.Dequeue());

                        ResizePool(queue, false);

                        if (addPool)
                            _queueList.Add(fileName, queue);
                    }
                }

                return null;
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Helper Methods
        /// <summary>
        /// This method is used to obtain a reference to the custom connection
        /// pool implementation currently in use, if any.
        /// </summary>
        /// <returns>
        /// The custom connection pool implementation or null if the default
        /// connection pool implementation should be used.
        /// </returns>
        internal static ISQLiteConnectionPool GetConnectionPool()
        {
            lock (_syncRoot)
            {
                return _connectionPool;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This method is used to set the reference to the custom connection
        /// pool implementation to use, if any.
        /// </summary>
        /// <param name="connectionPool">
        /// The custom connection pool implementation to use or null if the
        /// default connection pool implementation should be used.
        /// </param>
        internal static void SetConnectionPool(
            ISQLiteConnectionPool connectionPool
            )
        {
            lock (_syncRoot)
            {
                _connectionPool = connectionPool;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// We do not have to thread-lock anything in this function, because it
        /// is only called by other functions above which already take the lock.
        /// </summary>
        /// <param name="queue">
        /// The pool queue to resize.
        /// </param>
        /// <param name="add">
        /// If a function intends to add to the pool, this is true, which
        /// forces the resize to take one more than it needs from the pool.
        /// </param>
        private static void ResizePool(
            PoolQueue queue,
            bool add
            )
        {
            int target = queue.MaxPoolSize;

            if (add && target > 0) target--;

            Queue<WeakReference> poolQueue = queue.Queue;
            if (poolQueue == null) return;

            while (poolQueue.Count > target)
            {
                WeakReference connection = poolQueue.Dequeue();

                if (connection == null) continue;

                SQLiteConnectionHandle handle =
                    connection.Target as SQLiteConnectionHandle;

                if (handle != null)
                    handle.Dispose();

                GC.KeepAlive(handle);
            }
        }
        #endregion
    }
    #endregion
}
