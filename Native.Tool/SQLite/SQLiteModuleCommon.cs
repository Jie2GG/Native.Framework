/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Joe Mistachkin (joe@mistachkin.com)
 *
 * Released to the public domain, use at your own risk!
 ********************************************************/

using System.Globalization;

#region Non-Generic Classes
namespace System.Data.SQLite
{
    #region SQLiteModuleCommon Class
    /// <summary>
    /// This class contains some virtual methods that may be useful for other
    /// virtual table classes.  It specifically does NOT implement any of the
    /// <see cref="ISQLiteManagedModule" /> interface methods.
    /// </summary>
    public class SQLiteModuleCommon : SQLiteModuleNoop /* NOT SEALED */
    {
        #region Private Constants
        /// <summary>
        /// The CREATE TABLE statement used to declare the schema for the
        /// virtual table.
        /// </summary>
        private static readonly string declareSql =
            HelperMethods.StringFormat(
                CultureInfo.InvariantCulture, "CREATE TABLE {0}(x);",
                typeof(SQLiteModuleCommon).Name);
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Private Data
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
        public SQLiteModuleCommon(
            string name
            )
            : this(name, false)
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
        /// <param name="objectIdentity">
        /// Non-zero if different object instances with the same value should
        /// generate different row identifiers, where applicable.  This
        /// parameter has no effect on the .NET Compact Framework.
        /// </param>
        public SQLiteModuleCommon(
            string name,
            bool objectIdentity
            )
            : base(name)
        {
            this.objectIdentity = objectIdentity;
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////

        #region Protected Methods
        /// <summary>
        /// Determines the SQL statement used to declare the virtual table.
        /// This method should be overridden in derived classes if they require
        /// a custom virtual table schema.
        /// </summary>
        /// <returns>
        /// The SQL statement used to declare the virtual table -OR- null if it
        /// cannot be determined.
        /// </returns>
        protected virtual string GetSqlForDeclareTable()
        {
            return declareSql;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sets the table error message to one that indicates the virtual
        /// table cursor is of the wrong type.
        /// </summary>
        /// <param name="cursor">
        /// The <see cref="SQLiteVirtualTableCursor" /> object instance.
        /// </param>
        /// <param name="type">
        /// The <see cref="Type" /> that the virtual table cursor should be.
        /// </param>
        /// <returns>
        /// The value of <see cref="SQLiteErrorCode.Error" />.
        /// </returns>
        protected virtual SQLiteErrorCode CursorTypeMismatchError(
            SQLiteVirtualTableCursor cursor,
            Type type
            )
        {
            if (type != null)
            {
                SetCursorError(cursor, HelperMethods.StringFormat(
                    CultureInfo.CurrentCulture, "not a \"{0}\" cursor",
                    type));
            }
            else
            {
                SetCursorError(cursor, "cursor type mismatch");
            }

            return SQLiteErrorCode.Error;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Determines the string to return as the column value for the object
        /// instance value.
        /// </summary>
        /// <param name="cursor">
        /// The <see cref="SQLiteVirtualTableCursor" /> object instance
        /// associated with the previously opened virtual table cursor to be
        /// used.
        /// </param>
        /// <param name="value">
        /// The object instance to return a string representation for.
        /// </param>
        /// <returns>
        /// The string representation of the specified object instance or null
        /// upon failure.
        /// </returns>
        protected virtual string GetStringFromObject(
            SQLiteVirtualTableCursor cursor,
            object value
            )
        {
            if (value == null)
                return null;

            if (value is string)
                return (string)value;

            return value.ToString();
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructs an <see cref="Int64" /> unique row identifier from two
        /// <see cref="Int32" /> values.  The first <see cref="Int32" /> value
        /// must contain the row sequence number for the current row and the
        /// second value must contain the hash code of the key column value
        /// for the current row.
        /// </summary>
        /// <param name="rowIndex">
        /// The integer row sequence number for the current row.
        /// </param>
        /// <param name="hashCode">
        /// The hash code of the key column value for the current row.
        /// </param>
        /// <returns>
        /// The unique row identifier or zero upon failure.
        /// </returns>
        protected virtual long MakeRowId(
            int rowIndex,
            int hashCode
            )
        {
            long result = rowIndex;

            result <<= 32; /* typeof(int) bits */
            result |= (long)(uint)hashCode;

            return result;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Determines the unique row identifier for the current row.
        /// </summary>
        /// <param name="cursor">
        /// The <see cref="SQLiteVirtualTableCursor" /> object instance
        /// associated with the previously opened virtual table cursor to be
        /// used.
        /// </param>
        /// <param name="value">
        /// The object instance to return a unique row identifier for.
        /// </param>
        /// <returns>
        /// The unique row identifier or zero upon failure.
        /// </returns>
        protected virtual long GetRowIdFromObject(
            SQLiteVirtualTableCursor cursor,
            object value
            )
        {
            int rowIndex = (cursor != null) ? cursor.GetRowIndex() : 0;
            int hashCode = SQLiteMarshal.GetHashCode(value, objectIdentity);

            return MakeRowId(rowIndex, hashCode);
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
                    typeof(SQLiteModuleCommon).Name);
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
