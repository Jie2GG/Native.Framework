/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Joe Mistachkin (joe@mistachkin.com)
 *
 * Released to the public domain, use at your own risk!
 ********************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Globalization;

#region Non-Generic Classes
namespace System.Data.SQLite
{
    #region SQLiteVirtualTableCursorEnumerator Class
    /// <summary>
    /// This class represents a virtual table cursor to be used with the
    /// <see cref="SQLiteModuleEnumerable" /> class.  It is not sealed and may
    /// be used as the base class for any user-defined virtual table cursor
    /// class that wraps an <see cref="IEnumerator" /> object instance.
    /// </summary>
    public class SQLiteVirtualTableCursorEnumerator :
            SQLiteVirtualTableCursor, IEnumerator /* NOT SEALED */
    {
        #region Private Data
        /// <summary>
        /// The <see cref="IEnumerator" /> instance provided when this cursor
        /// was created.
        /// </summary>
        private IEnumerator enumerator;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This value will be non-zero if false has been returned from the
        /// <see cref="IEnumerator.MoveNext" /> method.
        /// </summary>
        private bool endOfEnumerator;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class.
        /// </summary>
        /// <param name="table">
        /// The <see cref="SQLiteVirtualTable" /> object instance associated
        /// with this object instance.
        /// </param>
        /// <param name="enumerator">
        /// The <see cref="IEnumerator" /> instance to expose as a virtual
        /// table cursor.
        /// </param>
        public SQLiteVirtualTableCursorEnumerator(
            SQLiteVirtualTable table,
            IEnumerator enumerator
            )
            : base(table)
        {
            this.enumerator = enumerator;
            this.endOfEnumerator = true;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Members
        /// <summary>
        /// Advances to the next row of the virtual table cursor using the
        /// <see cref="IEnumerator.MoveNext" /> method of the
        /// <see cref="IEnumerator" /> object instance.
        /// </summary>
        /// <returns>
        /// Non-zero if the current row is valid; zero otherwise.  If zero is
        /// returned, no further rows are available.
        /// </returns>
        public virtual bool MoveNext()
        {
            CheckDisposed();
            CheckClosed();

            if (enumerator == null)
                return false;

            endOfEnumerator = !enumerator.MoveNext();

            if (!endOfEnumerator)
                NextRowIndex();

            return !endOfEnumerator;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns the value for the current row of the virtual table cursor
        /// using the <see cref="IEnumerator.Current" /> property of the
        /// <see cref="IEnumerator" /> object instance.
        /// </summary>
        public virtual object Current
        {
            get
            {
                CheckDisposed();
                CheckClosed();

                if (enumerator == null)
                    return null;

                return enumerator.Current;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Resets the virtual table cursor position, also invalidating the
        /// current row, using the <see cref="IEnumerator.Reset" /> method of
        /// the <see cref="IEnumerator" /> object instance.
        /// </summary>
        public virtual void Reset()
        {
            CheckDisposed();
            CheckClosed();

            if (enumerator == null)
                return;

            enumerator.Reset();
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns non-zero if the end of the virtual table cursor has been
        /// seen (i.e. no more rows are available, including the current one).
        /// </summary>
        public virtual bool EndOfEnumerator
        {
            get { CheckDisposed(); CheckClosed(); return endOfEnumerator; }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns non-zero if the virtual table cursor is open.
        /// </summary>
        public virtual bool IsOpen
        {
            get { CheckDisposed(); return (enumerator != null); }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Closes the virtual table cursor.  This method must not throw any
        /// exceptions.
        /// </summary>
        public virtual void Close()
        {
            // CheckDisposed();
            // CheckClosed();

            if (enumerator != null)
                enumerator = null;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Throws an <see cref="InvalidOperationException" /> if the virtual
        /// table cursor has been closed.
        /// </summary>
        public virtual void CheckClosed()
        {
            CheckDisposed();

            if (!IsOpen)
            {
                throw new InvalidOperationException(
                    "virtual table cursor is closed");
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        private bool disposed;
        /// <summary>
        /// Throws an <see cref="ObjectDisposedException" /> if this object
        /// instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(SQLiteVirtualTableCursorEnumerator).Name);
            }
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes of this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this method is being called from the
        /// <see cref="IDisposable.Dispose" /> method.  Zero if this method is
        /// being called from the finalizer.
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

                    Close();
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

    #region SQLiteModuleEnumerable Class
    /// <summary>
    /// This class implements a virtual table module that exposes an
    /// <see cref="IEnumerable" /> object instance as a read-only virtual
    /// table.  It is not sealed and may be used as the base class for any
    /// user-defined virtual table class that wraps an
    /// <see cref="IEnumerable" /> object instance.  The following short
    /// example shows it being used to treat an array of strings as a table
    /// data source:
    /// <code>
    ///   public static class Sample
    ///   {
    ///     public static void Main()
    ///     {
    ///       using (SQLiteConnection connection = new SQLiteConnection(
    ///           "Data Source=:memory:;"))
    ///       {
    ///         connection.Open();
    ///
    ///         connection.CreateModule(new SQLiteModuleEnumerable(
    ///           "sampleModule", new string[] { "one", "two", "three" }));
    ///
    ///         using (SQLiteCommand command = connection.CreateCommand())
    ///         {
    ///           command.CommandText =
    ///               "CREATE VIRTUAL TABLE t1 USING sampleModule;";
    ///
    ///           command.ExecuteNonQuery();
    ///         }
    ///
    ///         using (SQLiteCommand command = connection.CreateCommand())
    ///         {
    ///           command.CommandText = "SELECT * FROM t1;";
    ///
    ///           using (SQLiteDataReader dataReader = command.ExecuteReader())
    ///           {
    ///             while (dataReader.Read())
    ///               Console.WriteLine(dataReader[0].ToString());
    ///           }
    ///         }
    ///
    ///         connection.Close();
    ///       }
    ///     }
    ///   }
    /// </code>
    /// </summary>
    public class SQLiteModuleEnumerable : SQLiteModuleCommon /* NOT SEALED */
    {
        #region Private Data
        /// <summary>
        /// The <see cref="IEnumerable" /> instance containing the backing data
        /// for the virtual table.
        /// </summary>
        private IEnumerable enumerable;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Non-zero if different object instances with the same value should
        /// generate different row identifiers, where applicable.  This has no
        /// effect on the .NET Compact Framework.
        /// </summary>
        private bool objectIdentity;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class.
        /// </summary>
        /// <param name="name">
        /// The name of the module.  This parameter cannot be null.
        /// </param>
        /// <param name="enumerable">
        /// The <see cref="IEnumerable" /> instance to expose as a virtual
        /// table.  This parameter cannot be null.
        /// </param>
        public SQLiteModuleEnumerable(
            string name,
            IEnumerable enumerable
            )
            : this(name, enumerable, false)
        {
            // do nothing.
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructs an instance of this class.
        /// </summary>
        /// <param name="name">
        /// The name of the module.  This parameter cannot be null.
        /// </param>
        /// <param name="enumerable">
        /// The <see cref="IEnumerable" /> instance to expose as a virtual
        /// table.  This parameter cannot be null.
        /// </param>
        /// <param name="objectIdentity">
        /// Non-zero if different object instances with the same value should
        /// generate different row identifiers, where applicable.  This
        /// parameter has no effect on the .NET Compact Framework.
        /// </param>
        public SQLiteModuleEnumerable(
            string name,
            IEnumerable enumerable,
            bool objectIdentity
            )
            : base(name)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");

            this.enumerable = enumerable;
            this.objectIdentity = objectIdentity;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Protected Methods
        /// <summary>
        /// Sets the table error message to one that indicates the virtual
        /// table cursor has no current row.
        /// </summary>
        /// <param name="cursor">
        /// The <see cref="SQLiteVirtualTableCursor" /> object instance.
        /// </param>
        /// <returns>
        /// The value of <see cref="SQLiteErrorCode.Error" />.
        /// </returns>
        protected virtual SQLiteErrorCode CursorEndOfEnumeratorError(
            SQLiteVirtualTableCursor cursor
            )
        {
            SetCursorError(cursor, "already hit end of enumerator");
            return SQLiteErrorCode.Error;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region ISQLiteManagedModule Members
        /// <summary>
        /// See the <see cref="ISQLiteManagedModule.Create" /> method.
        /// </summary>
        /// <param name="connection">
        /// See the <see cref="ISQLiteManagedModule.Create" /> method.
        /// </param>
        /// <param name="pClientData">
        /// See the <see cref="ISQLiteManagedModule.Create" /> method.
        /// </param>
        /// <param name="arguments">
        /// See the <see cref="ISQLiteManagedModule.Create" /> method.
        /// </param>
        /// <param name="table">
        /// See the <see cref="ISQLiteManagedModule.Create" /> method.
        /// </param>
        /// <param name="error">
        /// See the <see cref="ISQLiteManagedModule.Create" /> method.
        /// </param>
        /// <returns>
        /// See the <see cref="ISQLiteManagedModule.Create" /> method.
        /// </returns>
        public override SQLiteErrorCode Create(
            SQLiteConnection connection,
            IntPtr pClientData,
            string[] arguments,
            ref SQLiteVirtualTable table,
            ref string error
            )
        {
            CheckDisposed();

            if (DeclareTable(
                    connection, GetSqlForDeclareTable(),
                    ref error) == SQLiteErrorCode.Ok)
            {
                table = new SQLiteVirtualTable(arguments);
                return SQLiteErrorCode.Ok;
            }

            return SQLiteErrorCode.Error;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// See the <see cref="ISQLiteManagedModule.Connect" /> method.
        /// </summary>
        /// <param name="connection">
        /// See the <see cref="ISQLiteManagedModule.Connect" /> method.
        /// </param>
        /// <param name="pClientData">
        /// See the <see cref="ISQLiteManagedModule.Connect" /> method.
        /// </param>
        /// <param name="arguments">
        /// See the <see cref="ISQLiteManagedModule.Connect" /> method.
        /// </param>
        /// <param name="table">
        /// See the <see cref="ISQLiteManagedModule.Connect" /> method.
        /// </param>
        /// <param name="error">
        /// See the <see cref="ISQLiteManagedModule.Connect" /> method.
        /// </param>
        /// <returns>
        /// See the <see cref="ISQLiteManagedModule.Connect" /> method.
        /// </returns>
        public override SQLiteErrorCode Connect(
            SQLiteConnection connection,
            IntPtr pClientData,
            string[] arguments,
            ref SQLiteVirtualTable table,
            ref string error
            )
        {
            CheckDisposed();

            if (DeclareTable(
                    connection, GetSqlForDeclareTable(),
                    ref error) == SQLiteErrorCode.Ok)
            {
                table = new SQLiteVirtualTable(arguments);
                return SQLiteErrorCode.Ok;
            }

            return SQLiteErrorCode.Error;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// See the <see cref="ISQLiteManagedModule.BestIndex" /> method.
        /// </summary>
        /// <param name="table">
        /// See the <see cref="ISQLiteManagedModule.BestIndex" /> method.
        /// </param>
        /// <param name="index">
        /// See the <see cref="ISQLiteManagedModule.BestIndex" /> method.
        /// </param>
        /// <returns>
        /// See the <see cref="ISQLiteManagedModule.BestIndex" /> method.
        /// </returns>
        public override SQLiteErrorCode BestIndex(
            SQLiteVirtualTable table,
            SQLiteIndex index
            )
        {
            CheckDisposed();

            if (!table.BestIndex(index))
            {
                SetTableError(table, HelperMethods.StringFormat(
                    CultureInfo.CurrentCulture,
                    "failed to select best index for virtual table \"{0}\"",
                    table.TableName));

                return SQLiteErrorCode.Error;
            }

            return SQLiteErrorCode.Ok;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// See the <see cref="ISQLiteManagedModule.Disconnect" /> method.
        /// </summary>
        /// <param name="table">
        /// See the <see cref="ISQLiteManagedModule.Disconnect" /> method.
        /// </param>
        /// <returns>
        /// See the <see cref="ISQLiteManagedModule.Disconnect" /> method.
        /// </returns>
        public override SQLiteErrorCode Disconnect(
            SQLiteVirtualTable table
            )
        {
            CheckDisposed();

            table.Dispose();
            return SQLiteErrorCode.Ok;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// See the <see cref="ISQLiteManagedModule.Destroy" /> method.
        /// </summary>
        /// <param name="table">
        /// See the <see cref="ISQLiteManagedModule.Destroy" /> method.
        /// </param>
        /// <returns>
        /// See the <see cref="ISQLiteManagedModule.Destroy" /> method.
        /// </returns>
        public override SQLiteErrorCode Destroy(
            SQLiteVirtualTable table
            )
        {
            CheckDisposed();

            table.Dispose();
            return SQLiteErrorCode.Ok;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// See the <see cref="ISQLiteManagedModule.Open" /> method.
        /// </summary>
        /// <param name="table">
        /// See the <see cref="ISQLiteManagedModule.Open" /> method.
        /// </param>
        /// <param name="cursor">
        /// See the <see cref="ISQLiteManagedModule.Open" /> method.
        /// </param>
        /// <returns>
        /// See the <see cref="ISQLiteManagedModule.Open" /> method.
        /// </returns>
        public override SQLiteErrorCode Open(
            SQLiteVirtualTable table,
            ref SQLiteVirtualTableCursor cursor
            )
        {
            CheckDisposed();

            cursor = new SQLiteVirtualTableCursorEnumerator(
                table, enumerable.GetEnumerator());

            return SQLiteErrorCode.Ok;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// See the <see cref="ISQLiteManagedModule.Close" /> method.
        /// </summary>
        /// <param name="cursor">
        /// See the <see cref="ISQLiteManagedModule.Close" /> method.
        /// </param>
        /// <returns>
        /// See the <see cref="ISQLiteManagedModule.Close" /> method.
        /// </returns>
        public override SQLiteErrorCode Close(
            SQLiteVirtualTableCursor cursor
            )
        {
            CheckDisposed();

            SQLiteVirtualTableCursorEnumerator enumeratorCursor =
                cursor as SQLiteVirtualTableCursorEnumerator;

            if (enumeratorCursor == null)
            {
                return CursorTypeMismatchError(cursor,
                    typeof(SQLiteVirtualTableCursorEnumerator));
            }

            enumeratorCursor.Close();
            return SQLiteErrorCode.Ok;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// See the <see cref="ISQLiteManagedModule.Filter" /> method.
        /// </summary>
        /// <param name="cursor">
        /// See the <see cref="ISQLiteManagedModule.Filter" /> method.
        /// </param>
        /// <param name="indexNumber">
        /// See the <see cref="ISQLiteManagedModule.Filter" /> method.
        /// </param>
        /// <param name="indexString">
        /// See the <see cref="ISQLiteManagedModule.Filter" /> method.
        /// </param>
        /// <param name="values">
        /// See the <see cref="ISQLiteManagedModule.Filter" /> method.
        /// </param>
        /// <returns>
        /// See the <see cref="ISQLiteManagedModule.Filter" /> method.
        /// </returns>
        public override SQLiteErrorCode Filter(
            SQLiteVirtualTableCursor cursor,
            int indexNumber,
            string indexString,
            SQLiteValue[] values
            )
        {
            CheckDisposed();

            SQLiteVirtualTableCursorEnumerator enumeratorCursor =
                cursor as SQLiteVirtualTableCursorEnumerator;

            if (enumeratorCursor == null)
            {
                return CursorTypeMismatchError(cursor,
                    typeof(SQLiteVirtualTableCursorEnumerator));
            }

            enumeratorCursor.Filter(indexNumber, indexString, values);
            enumeratorCursor.Reset(); /* NO RESULT */
            enumeratorCursor.MoveNext(); /* IGNORED */

            return SQLiteErrorCode.Ok;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// See the <see cref="ISQLiteManagedModule.Next" /> method.
        /// </summary>
        /// <param name="cursor">
        /// See the <see cref="ISQLiteManagedModule.Next" /> method.
        /// </param>
        /// <returns>
        /// See the <see cref="ISQLiteManagedModule.Next" /> method.
        /// </returns>
        public override SQLiteErrorCode Next(
            SQLiteVirtualTableCursor cursor
            )
        {
            CheckDisposed();

            SQLiteVirtualTableCursorEnumerator enumeratorCursor =
                cursor as SQLiteVirtualTableCursorEnumerator;

            if (enumeratorCursor == null)
            {
                return CursorTypeMismatchError(cursor,
                    typeof(SQLiteVirtualTableCursorEnumerator));
            }

            if (enumeratorCursor.EndOfEnumerator)
                return CursorEndOfEnumeratorError(cursor);

            enumeratorCursor.MoveNext(); /* IGNORED */
            return SQLiteErrorCode.Ok;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// See the <see cref="ISQLiteManagedModule.Eof" /> method.
        /// </summary>
        /// <param name="cursor">
        /// See the <see cref="ISQLiteManagedModule.Eof" /> method.
        /// </param>
        /// <returns>
        /// See the <see cref="ISQLiteManagedModule.Eof" /> method.
        /// </returns>
        public override bool Eof(
            SQLiteVirtualTableCursor cursor
            )
        {
            CheckDisposed();

            SQLiteVirtualTableCursorEnumerator enumeratorCursor =
                cursor as SQLiteVirtualTableCursorEnumerator;

            if (enumeratorCursor == null)
            {
                return ResultCodeToEofResult(CursorTypeMismatchError(
                    cursor, typeof(SQLiteVirtualTableCursorEnumerator)));
            }

            return enumeratorCursor.EndOfEnumerator;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// See the <see cref="ISQLiteManagedModule.Column" /> method.
        /// </summary>
        /// <param name="cursor">
        /// See the <see cref="ISQLiteManagedModule.Column" /> method.
        /// </param>
        /// <param name="context">
        /// See the <see cref="ISQLiteManagedModule.Column" /> method.
        /// </param>
        /// <param name="index">
        /// See the <see cref="ISQLiteManagedModule.Column" /> method.
        /// </param>
        /// <returns>
        /// See the <see cref="ISQLiteManagedModule.Column" /> method.
        /// </returns>
        public override SQLiteErrorCode Column(
            SQLiteVirtualTableCursor cursor,
            SQLiteContext context,
            int index
            )
        {
            CheckDisposed();

            SQLiteVirtualTableCursorEnumerator enumeratorCursor =
                cursor as SQLiteVirtualTableCursorEnumerator;

            if (enumeratorCursor == null)
            {
                return CursorTypeMismatchError(cursor,
                    typeof(SQLiteVirtualTableCursorEnumerator));
            }

            if (enumeratorCursor.EndOfEnumerator)
                return CursorEndOfEnumeratorError(cursor);

            object current = enumeratorCursor.Current;

            if (current != null)
                context.SetString(GetStringFromObject(cursor, current));
            else
                context.SetNull();

            return SQLiteErrorCode.Ok;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// See the <see cref="ISQLiteManagedModule.RowId" /> method.
        /// </summary>
        /// <param name="cursor">
        /// See the <see cref="ISQLiteManagedModule.RowId" /> method.
        /// </param>
        /// <param name="rowId">
        /// See the <see cref="ISQLiteManagedModule.RowId" /> method.
        /// </param>
        /// <returns>
        /// See the <see cref="ISQLiteManagedModule.RowId" /> method.
        /// </returns>
        public override SQLiteErrorCode RowId(
            SQLiteVirtualTableCursor cursor,
            ref long rowId
            )
        {
            CheckDisposed();

            SQLiteVirtualTableCursorEnumerator enumeratorCursor =
                cursor as SQLiteVirtualTableCursorEnumerator;

            if (enumeratorCursor == null)
            {
                return CursorTypeMismatchError(cursor,
                    typeof(SQLiteVirtualTableCursorEnumerator));
            }

            if (enumeratorCursor.EndOfEnumerator)
                return CursorEndOfEnumeratorError(cursor);

            object current = enumeratorCursor.Current;

            rowId = GetRowIdFromObject(cursor, current);
            return SQLiteErrorCode.Ok;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// See the <see cref="ISQLiteManagedModule.Update" /> method.
        /// </summary>
        /// <param name="table">
        /// See the <see cref="ISQLiteManagedModule.Update" /> method.
        /// </param>
        /// <param name="values">
        /// See the <see cref="ISQLiteManagedModule.Update" /> method.
        /// </param>
        /// <param name="rowId">
        /// See the <see cref="ISQLiteManagedModule.Update" /> method.
        /// </param>
        /// <returns>
        /// See the <see cref="ISQLiteManagedModule.Update" /> method.
        /// </returns>
        public override SQLiteErrorCode Update(
            SQLiteVirtualTable table,
            SQLiteValue[] values,
            ref long rowId
            )
        {
            CheckDisposed();

            SetTableError(table, HelperMethods.StringFormat(
                CultureInfo.CurrentCulture,
                "virtual table \"{0}\" is read-only", table.TableName));

            return SQLiteErrorCode.Error;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// See the <see cref="ISQLiteManagedModule.Rename" /> method.
        /// </summary>
        /// <param name="table">
        /// See the <see cref="ISQLiteManagedModule.Rename" /> method.
        /// </param>
        /// <param name="newName">
        /// See the <see cref="ISQLiteManagedModule.Rename" /> method.
        /// </param>
        /// <returns>
        /// See the <see cref="ISQLiteManagedModule.Rename" /> method.
        /// </returns>
        public override SQLiteErrorCode Rename(
            SQLiteVirtualTable table,
            string newName
            )
        {
            CheckDisposed();

            if (!table.Rename(newName))
            {
                SetTableError(table, HelperMethods.StringFormat(
                    CultureInfo.CurrentCulture,
                    "failed to rename virtual table from \"{0}\" to \"{1}\"",
                    table.TableName, newName));

                return SQLiteErrorCode.Error;
            }

            return SQLiteErrorCode.Ok;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        private bool disposed;
        /// <summary>
        /// Throws an <see cref="ObjectDisposedException" /> if this object
        /// instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(SQLiteModuleEnumerable).Name);
            }
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes of this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this method is being called from the
        /// <see cref="IDisposable.Dispose" /> method.  Zero if this method is
        /// being called from the finalizer.
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
}
#endregion

///////////////////////////////////////////////////////////////////////////////

#region Generic Classes
namespace System.Data.SQLite.Generic
{
    #region SQLiteVirtualTableCursorEnumerator<T> Class
    /// <summary>
    /// This class represents a virtual table cursor to be used with the
    /// <see cref="SQLiteModuleEnumerable" /> class.  It is not sealed and may
    /// be used as the base class for any user-defined virtual table cursor
    /// class that wraps an <see cref="IEnumerator{T}" /> object instance.
    /// </summary>
    public class SQLiteVirtualTableCursorEnumerator<T> :
            SQLiteVirtualTableCursorEnumerator, IEnumerator<T> /* NOT SEALED */
    {
        #region Private Data
        /// <summary>
        /// The <see cref="IEnumerator{T}" /> instance provided when this
        /// cursor was created.
        /// </summary>
        private IEnumerator<T> enumerator;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class.
        /// </summary>
        /// <param name="table">
        /// The <see cref="SQLiteVirtualTable" /> object instance associated
        /// with this object instance.
        /// </param>
        /// <param name="enumerator">
        /// The <see cref="IEnumerator{T}" /> instance to expose as a virtual
        /// table cursor.
        /// </param>
        public SQLiteVirtualTableCursorEnumerator(
            SQLiteVirtualTable table,
            IEnumerator<T> enumerator
            )
            : base(table, enumerator as IEnumerator)
        {
            this.enumerator = enumerator;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Members
        /// <summary>
        /// Returns the value for the current row of the virtual table cursor
        /// using the <see cref="IEnumerator{T}.Current" /> property of the
        /// <see cref="IEnumerator{T}" /> object instance.
        /// </summary>
        T IEnumerator<T>.Current
        {
            get
            {
                CheckDisposed();
                CheckClosed();

                if (enumerator == null)
                    return default(T);

                return enumerator.Current;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Closes the virtual table cursor.  This method must not throw any
        /// exceptions.
        /// </summary>
        public override void Close()
        {
            // CheckDisposed();
            // CheckClosed();

            if (enumerator != null)
                enumerator = null;

            base.Close();
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        private bool disposed;
        /// <summary>
        /// Throws an <see cref="ObjectDisposedException" /> if this object
        /// instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(SQLiteVirtualTableCursorEnumerator<T>).Name);
            }
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes of this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this method is being called from the
        /// <see cref="IDisposable.Dispose" /> method.  Zero if this method is
        /// being called from the finalizer.
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

                    Close();
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

    #region SQLiteModuleEnumerable<T> Class
    /// <summary>
    /// This class implements a virtual table module that exposes an
    /// <see cref="IEnumerable{T}" /> object instance as a read-only virtual
    /// table.  It is not sealed and may be used as the base class for any
    /// user-defined virtual table class that wraps an
    /// <see cref="IEnumerable{T}" /> object instance.
    /// </summary>
    public class SQLiteModuleEnumerable<T> :
            SQLiteModuleEnumerable /* NOT SEALED */
    {
        #region Private Data
        /// <summary>
        /// The <see cref="IEnumerable{T}" /> instance containing the backing
        /// data for the virtual table.
        /// </summary>
        private IEnumerable<T> enumerable;
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Public Constructors
        /// <summary>
        /// Constructs an instance of this class.
        /// </summary>
        /// <param name="name">
        /// The name of the module.  This parameter cannot be null.
        /// </param>
        /// <param name="enumerable">
        /// The <see cref="IEnumerable{T}" /> instance to expose as a virtual
        /// table.  This parameter cannot be null.
        /// </param>
        public SQLiteModuleEnumerable(
            string name,
            IEnumerable<T> enumerable
            )
            : base(name, enumerable as IEnumerable)
        {
            this.enumerable = enumerable;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region ISQLiteManagedModule Members
        /// <summary>
        /// See the <see cref="ISQLiteManagedModule.Open" /> method.
        /// </summary>
        /// <param name="table">
        /// See the <see cref="ISQLiteManagedModule.Open" /> method.
        /// </param>
        /// <param name="cursor">
        /// See the <see cref="ISQLiteManagedModule.Open" /> method.
        /// </param>
        /// <returns>
        /// See the <see cref="ISQLiteManagedModule.Open" /> method.
        /// </returns>
        public override SQLiteErrorCode Open(
            SQLiteVirtualTable table,
            ref SQLiteVirtualTableCursor cursor
            )
        {
            CheckDisposed();

            cursor = new SQLiteVirtualTableCursorEnumerator<T>(
                table, enumerable.GetEnumerator());

            return SQLiteErrorCode.Ok;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// See the <see cref="ISQLiteManagedModule.Column" /> method.
        /// </summary>
        /// <param name="cursor">
        /// See the <see cref="ISQLiteManagedModule.Column" /> method.
        /// </param>
        /// <param name="context">
        /// See the <see cref="ISQLiteManagedModule.Column" /> method.
        /// </param>
        /// <param name="index">
        /// See the <see cref="ISQLiteManagedModule.Column" /> method.
        /// </param>
        /// <returns>
        /// See the <see cref="ISQLiteManagedModule.Column" /> method.
        /// </returns>
        public override SQLiteErrorCode Column(
            SQLiteVirtualTableCursor cursor,
            SQLiteContext context,
            int index
            )
        {
            CheckDisposed();

            SQLiteVirtualTableCursorEnumerator<T> enumeratorCursor =
                cursor as SQLiteVirtualTableCursorEnumerator<T>;

            if (enumeratorCursor == null)
            {
                return CursorTypeMismatchError(cursor,
                    typeof(SQLiteVirtualTableCursorEnumerator));
            }

            if (enumeratorCursor.EndOfEnumerator)
                return CursorEndOfEnumeratorError(cursor);

            T current = ((IEnumerator<T>)enumeratorCursor).Current;

            if (current != null)
                context.SetString(GetStringFromObject(cursor, current));
            else
                context.SetNull();

            return SQLiteErrorCode.Ok;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region IDisposable "Pattern" Members
        private bool disposed;
        /// <summary>
        /// Throws an <see cref="ObjectDisposedException" /> if this object
        /// instance has been disposed.
        /// </summary>
        private void CheckDisposed() /* throw */
        {
#if THROW_ON_DISPOSED
            if (disposed)
            {
                throw new ObjectDisposedException(
                    typeof(SQLiteModuleEnumerable<T>).Name);
            }
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes of this object instance.
        /// </summary>
        /// <param name="disposing">
        /// Non-zero if this method is being called from the
        /// <see cref="IDisposable.Dispose" /> method.  Zero if this method is
        /// being called from the finalizer.
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
}
#endregion
