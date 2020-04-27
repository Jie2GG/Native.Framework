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
    /// SQLite implementation of DbTransaction that does not support nested transactions.
    /// </summary>
    public class SQLiteTransaction : SQLiteTransactionBase
    {
        /// <summary>
        /// Constructs the transaction object, binding it to the supplied connection
        /// </summary>
        /// <param name="connection">The connection to open a transaction on</param>
        /// <param name="deferredLock">TRUE to defer the writelock, or FALSE to lock immediately</param>
        internal SQLiteTransaction(SQLiteConnection connection, bool deferredLock)
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
                throw new ObjectDisposedException(typeof(SQLiteTransaction).Name);
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

            if (_cnn._transactionLevel - 1 == 0)
            {
                using (SQLiteCommand cmd = _cnn.CreateCommand())
                {
                    cmd.CommandText = "COMMIT;";
                    cmd.ExecuteNonQuery();
                }
            }
            _cnn._transactionLevel--;
            _cnn = null;
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
            if (_cnn._transactionLevel++ == 0)
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
        protected override void IssueRollback(
            bool throwError
            )
        {
            SQLiteConnection cnn = Interlocked.Exchange(ref _cnn, null);

            if (cnn != null)
            {
                try
                {
                    using (SQLiteCommand cmd = cnn.CreateCommand())
                    {
                        cmd.CommandText = "ROLLBACK;";
                        cmd.ExecuteNonQuery();
                    }
                }
                catch
                {
                    if (throwError)
                        throw;
                }
                cnn._transactionLevel = 0;
            }
        }
    }
}
