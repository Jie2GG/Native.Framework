/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 *
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
    using System;
    using System.Threading;

    ///////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// SQLite implementation of DbTransaction that does support nested transactions.
    /// </summary>
    public sealed class SQLiteTransaction2 : SQLiteTransaction
    {
        /// <summary>
        /// The original transaction level for the associated connection
        /// when this transaction was created (i.e. begun).
        /// </summary>
        private int _beginLevel;

        /// <summary>
        /// The SAVEPOINT name for this transaction, if any.  This will
        /// only be non-null if this transaction is a nested one.
        /// </summary>
        private string _savePointName;

        ///////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructs the transaction object, binding it to the supplied connection
        /// </summary>
        /// <param name="connection">The connection to open a transaction on</param>
        /// <param name="deferredLock">TRUE to defer the writelock, or FALSE to lock immediately</param>
        internal SQLiteTransaction2(SQLiteConnection connection, bool deferredLock)
            : base(connection, deferredLock)
        {
            // do nothing.
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        private bool disposed;
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
                throw new ObjectDisposedException(typeof(SQLiteTransaction2).Name);
#endif
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes the transaction.  If it is currently active, any changes are rolled back.
        /// </summary>
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

                        if (IsValid(false))
                        {
                            IssueRollback(false);
                        }
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

        ///////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        public override void Commit()
        {
            CheckDisposed();
            SQLiteConnection.Check(_cnn);
            IsValid(true);

            if (_beginLevel == 0)
            {
                using (SQLiteCommand cmd = _cnn.CreateCommand())
                {
                    cmd.CommandText = "COMMIT;";
                    cmd.ExecuteNonQuery();
                }

                _cnn._transactionLevel = 0;
                _cnn = null;
            }
            else
            {
                using (SQLiteCommand cmd = _cnn.CreateCommand())
                {
                    if (String.IsNullOrEmpty(_savePointName))
                        throw new SQLiteException("Cannot commit, unknown SAVEPOINT");

                    cmd.CommandText = String.Format(
                        "RELEASE {0};", _savePointName);

                    cmd.ExecuteNonQuery();
                }

                _cnn._transactionLevel--;
                _cnn = null;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to start a transaction.  An exception will be thrown if the transaction cannot
        /// be started for any reason.
        /// </summary>
        /// <param name="deferredLock">TRUE to defer the writelock, or FALSE to lock immediately</param>
        protected override void Begin(
            bool deferredLock
            )
        {
            int transactionLevel;

            if ((transactionLevel = _cnn._transactionLevel++) == 0)
            {
                try
                {
                    using (SQLiteCommand cmd = _cnn.CreateCommand())
                    {
                        if (!deferredLock)
                            cmd.CommandText = "BEGIN IMMEDIATE;";
                        else
                            cmd.CommandText = "BEGIN;";

                        cmd.ExecuteNonQuery();

                        _beginLevel = transactionLevel;
                    }
                }
                catch (SQLiteException)
                {
                    _cnn._transactionLevel--;
                    _cnn = null;

                    throw;
                }
            }
            else
            {
                try
                {
                    using (SQLiteCommand cmd = _cnn.CreateCommand())
                    {
                        _savePointName = GetSavePointName();

                        cmd.CommandText = String.Format(
                            "SAVEPOINT {0};", _savePointName);

                        cmd.ExecuteNonQuery();

                        _beginLevel = transactionLevel;
                    }
                }
                catch (SQLiteException)
                {
                    _cnn._transactionLevel--;
                    _cnn = null;

                    throw;
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Issue a ROLLBACK command against the database connection,
        /// optionally re-throwing any caught exception.
        /// </summary>
        /// <param name="throwError">
        /// Non-zero to re-throw caught exceptions.
        /// </param>
        protected override void IssueRollback(bool throwError)
        {
            SQLiteConnection cnn = Interlocked.Exchange(ref _cnn, null);

            if (cnn != null)
            {
                if (_beginLevel == 0)
                {
                    try
                    {
                        using (SQLiteCommand cmd = cnn.CreateCommand())
                        {
                            cmd.CommandText = "ROLLBACK;";
                            cmd.ExecuteNonQuery();
                        }

                        cnn._transactionLevel = 0;
                    }
                    catch
                    {
                        if (throwError)
                            throw;
                    }
                }
                else
                {
                    try
                    {
                        using (SQLiteCommand cmd = cnn.CreateCommand())
                        {
                            if (String.IsNullOrEmpty(_savePointName))
                                throw new SQLiteException("Cannot rollback, unknown SAVEPOINT");

                            cmd.CommandText = String.Format(
                                "ROLLBACK TO {0};", _savePointName);

                            cmd.ExecuteNonQuery();
                        }

                        cnn._transactionLevel--;
                    }
                    catch
                    {
                        if (throwError)
                            throw;
                    }
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructs the name of a new savepoint for this transaction.  It
        /// should only be called from the constructor of this class.
        /// </summary>
        /// <returns>
        /// The name of the new savepoint -OR- null if it cannot be constructed.
        /// </returns>
        private string GetSavePointName()
        {
            int sequence = ++_cnn._transactionSequence;

            return String.Format(
                "sqlite_dotnet_savepoint_{0}", sequence);
        }
    }
}
