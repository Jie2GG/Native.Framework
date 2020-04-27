/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Joe Mistachkin (joe@mistachkin.com)
 *
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
    #region ISQLiteNativeModule Interface
    /// <summary>
    /// This interface represents a virtual table implementation written in
    /// native code.
    /// </summary>
    public interface ISQLiteNativeModule
    {
        /// <summary>
        /// <para><code>
        /// int (*xCreate)(sqlite3 *db, void *pAux,
        ///              int argc, char *const*argv,
        ///              sqlite3_vtab **ppVTab,
        ///              char **pzErr);
        /// </code></para>
        /// <para>
        /// The xCreate method is called to create a new instance of a virtual table 
        /// in response to a CREATE VIRTUAL TABLE statement.
        /// If the xCreate method is the same pointer as the xConnect method, then the
        /// virtual table is an eponymous virtual table.
        /// If the xCreate method is omitted (if it is a NULL pointer) then the virtual 
        /// table is an eponymous-only virtual table.
        /// </para>
        /// <para>
        /// The db parameter is a pointer to the SQLite database connection that 
        /// is executing the CREATE VIRTUAL TABLE statement. 
        /// The pAux argument is the copy of the client data pointer that was the 
        /// fourth argument to the sqlite3_create_module() or
        /// sqlite3_create_module_v2() call that registered the 
        /// virtual table module. 
        /// The argv parameter is an array of argc pointers to null terminated strings. 
        /// The first string, argv[0], is the name of the module being invoked.   The
        /// module name is the name provided as the second argument to 
        /// sqlite3_create_module() and as the argument to the USING clause of the
        /// CREATE VIRTUAL TABLE statement that is running.
        /// The second, argv[1], is the name of the database in which the new virtual table is being created. The database name is "main" for the primary database, or
        /// "temp" for TEMP database, or the name given at the end of the ATTACH
        /// statement for attached databases.  The third element of the array, argv[2], 
        /// is the name of the new virtual table, as specified following the TABLE
        /// keyword in the CREATE VIRTUAL TABLE statement.
        /// If present, the fourth and subsequent strings in the argv[] array report 
        /// the arguments to the module name in the CREATE VIRTUAL TABLE statement.
        /// </para>
        /// <para>
        /// The job of this method is to construct the new virtual table object
        /// (an sqlite3_vtab object) and return a pointer to it in *ppVTab.
        /// </para>
        /// <para>
        /// As part of the task of creating a new sqlite3_vtab structure, this 
        /// method <u>must</u> invoke sqlite3_declare_vtab() to tell the SQLite 
        /// core about the columns and datatypes in the virtual table. 
        /// The sqlite3_declare_vtab() API has the following prototype:
        /// </para>
        /// <para><code>
        /// int sqlite3_declare_vtab(sqlite3 *db, const char *zCreateTable)
        /// </code></para>
        /// <para>
        /// The first argument to sqlite3_declare_vtab() must be the same 
        /// database connection pointer as the first parameter to this method.
        /// The second argument to sqlite3_declare_vtab() must a zero-terminated 
        /// UTF-8 string that contains a well-formed CREATE TABLE statement that 
        /// defines the columns in the virtual table and their data types. 
        /// The name of the table in this CREATE TABLE statement is ignored, 
        /// as are all constraints. Only the column names and datatypes matter.
        /// The CREATE TABLE statement string need not to be 
        /// held in persistent memory.  The string can be
        /// deallocated and/or reused as soon as the sqlite3_declare_vtab()
        /// routine returns.
        /// </para>
        /// <para>
        /// The xCreate method need not initialize the pModule, nRef, and zErrMsg
        /// fields of the sqlite3_vtab object.  The SQLite core will take care of 
        /// that chore.
        /// </para>
        /// <para>
        /// The xCreate should return SQLITE_OK if it is successful in 
        /// creating the new virtual table, or SQLITE_ERROR if it is not successful.
        /// If not successful, the sqlite3_vtab structure must not be allocated. 
        /// An error message may optionally be returned in *pzErr if unsuccessful.
        /// Space to hold the error message string must be allocated using
        /// an SQLite memory allocation function like 
        /// sqlite3_malloc() or sqlite3_mprintf() as the SQLite core will
        /// attempt to free the space using sqlite3_free() after the error has
        /// been reported up to the application.
        /// </para>
        /// <para>
        /// If the xCreate method is omitted (left as a NULL pointer) then the
        /// virtual table is an eponymous-only virtual table.  New instances of
        /// the virtual table cannot be created using CREATE VIRTUAL TABLE and the
        /// virtual table can only be used via its module name.
        /// Note that SQLite versions prior to 3.9.0 (2015-10-14) do not understand
        /// eponymous-only virtual tables and will segfault if an attempt is made
        /// to CREATE VIRTUAL TABLE on an eponymous-only virtual table because
        /// the xCreate method was not checked for null.
        /// </para>
        /// <para>
        /// If the xCreate method is the exact same pointer as the xConnect method,
        /// that indicates that the virtual table does not need to initialize backing
        /// store.  Such a virtual table can be used as an eponymous virtual table
        /// or as a named virtual table using CREATE VIRTUAL TABLE or both.
        /// </para>
        /// <para>
        /// If a column datatype contains the special keyword "HIDDEN"
        /// (in any combination of upper and lower case letters) then that keyword
        /// it is omitted from the column datatype name and the column is marked 
        /// as a hidden column internally. 
        /// A hidden column differs from a normal column in three respects:
        /// </para>
        /// <para>
        /// <![CDATA[<ul>]]>
        /// <![CDATA[<li>]]> Hidden columns are not listed in the dataset returned by 
        ///      "PRAGMA table_info",
        /// <![CDATA[</li>]]><![CDATA[<li>]]> Hidden columns are not included in the expansion of a "*"
        ///      expression in the result set of a SELECT, and
        /// <![CDATA[</li>]]><![CDATA[<li>]]> Hidden columns are not included in the implicit column-list 
        ///      used by an INSERT statement that lacks an explicit column-list. 
        /// <![CDATA[</li>]]><![CDATA[</ul>]]>
        /// </para>
        /// <para>
        /// For example, if the following SQL is passed to sqlite3_declare_vtab():
        /// </para>
        /// <para><code>
        /// CREATE TABLE x(a HIDDEN VARCHAR(12), b INTEGER, c INTEGER Hidden);
        /// </code></para>
        /// <para>
        /// Then the virtual table would be created with two hidden columns,
        /// and with datatypes of "VARCHAR(12)" and "INTEGER".
        /// </para>
        /// <para>
        /// An example use of hidden columns can be seen in the FTS3 virtual 
        /// table implementation, where every FTS virtual table
        /// contains an FTS hidden column that is used to pass information from the
        /// virtual table into FTS auxiliary functions and to the FTS MATCH operator.
        /// </para>
        /// <para>
        /// A virtual table that contains hidden columns can be used like
        /// a table-valued function in the FROM clause of a SELECT statement.
        /// The arguments to the table-valued function become constraints on 
        /// the HIDDEN columns of the virtual table.
        /// </para>
        /// <para>
        /// For example, the "generate_series" extension (located in the
        /// ext/misc/series.c
        /// file in the source tree)
        /// implements an eponymous virtual table with the following schema:
        /// </para>
        /// <para><code>
        /// CREATE TABLE generate_series(
        ///   value,
        ///   start HIDDEN,
        ///   stop HIDDEN,
        ///   step HIDDEN
        /// );
        /// </code></para>
        /// <para>
        /// The sqlite3_module.xBestIndex method in the implementation of this
        /// table checks for equality constraints against the HIDDEN columns, and uses
        /// those as input parameters to determine the range of integer "value" outputs
        /// to generate.  Reasonable defaults are used for any unconstrained columns.
        /// For example, to list all integers between 5 and 50:
        /// </para>
        /// <para><code>
        /// SELECT value FROM generate_series(5,50);
        /// </code></para>
        /// <para>
        /// The previous query is equivalent to the following:
        /// </para>
        /// <para><code>
        /// SELECT value FROM generate_series WHERE start=5 AND stop=50;
        /// </code></para>
        /// <para>
        /// Arguments on the virtual table name are matched to hidden columns
        /// in order.  The number of arguments can be less than the
        /// number of hidden columns, in which case the latter hidden columns are
        /// unconstrained.  However, an error results if there are more arguments
        /// than there are hidden columns in the virtual table.
        /// </para>
        /// <para>
        /// Beginning with SQLite version 3.14.0 (2016-08-08), 
        /// the CREATE TABLE statement that
        /// is passed into sqlite3_declare_vtab() may contain a WITHOUT ROWID clause.
        /// This is useful for cases where the virtual table rows 
        /// cannot easily be mapped into unique integers.  A CREATE TABLE
        /// statement that includes WITHOUT ROWID must define one or more columns as
        /// the PRIMARY KEY.  Every column of the PRIMARY KEY must individually be
        /// NOT NULL and all columns for each row must be collectively unique.
        /// </para>
        /// <para>
        /// Note that SQLite does not enforce the PRIMARY KEY for a WITHOUT ROWID
        /// virtual table.  Enforcement is the responsibility of the underlying
        /// virtual table implementation.  But SQLite does assume that the PRIMARY KEY
        /// constraint is valid - that the identified columns really are UNIQUE and
        /// NOT NULL - and it uses that assumption to optimize queries against the
        /// virtual table.
        /// </para>
        /// <para>
        /// The rowid column is not accessible on a
        /// WITHOUT ROWID virtual table (of course).
        /// </para>
        /// <para>
        /// The xUpdate method was originally designed around having a
        /// ROWID as a single value.  The xUpdate method has been expanded to
        /// accommodate an arbitrary PRIMARY KEY in place of the ROWID, but the
        /// PRIMARY KEY must still be only one column.  For this reason, SQLite
        /// will reject any WITHOUT ROWID virtual table that has more than one
        /// PRIMARY KEY column and a non-NULL xUpdate method.
        /// </para>
        /// </summary>
        /// <param name="pDb">
        /// The native database connection handle.
        /// </param>
        /// <param name="pAux">
        /// The original native pointer value that was provided to the
        /// sqlite3_create_module(), sqlite3_create_module_v2() or
        /// sqlite3_create_disposable_module() functions.
        /// </param>
        /// <param name="argc">
        /// The number of arguments from the CREATE VIRTUAL TABLE statement.
        /// </param>
        /// <param name="argv">
        /// The array of string arguments from the CREATE VIRTUAL TABLE
        /// statement.
        /// </param>
        /// <param name="pVtab">
        /// Upon success, this parameter must be modified to point to the newly
        /// created native sqlite3_vtab derived structure.
        /// </param>
        /// <param name="pError">
        /// Upon failure, this parameter must be modified to point to the error
        /// message, with the underlying memory having been obtained from the
        /// sqlite3_malloc() function.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xCreate(
            IntPtr pDb,
            IntPtr pAux,
            int argc,
            IntPtr argv,
            ref IntPtr pVtab,
            ref IntPtr pError
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xConnect)(sqlite3*, void *pAux,
        ///              int argc, char *const*argv,
        ///              sqlite3_vtab **ppVTab,
        ///              char **pzErr);
        /// </code></para>
        /// <para>
        /// The xConnect method is very similar to xCreate. 
        /// It has the same parameters and constructs a new sqlite3_vtab structure 
        /// just like xCreate. 
        /// And it must also call sqlite3_declare_vtab() like xCreate.
        /// </para>
        /// <para>
        /// The difference is that xConnect is called to establish a new 
        /// connection to an existing virtual table whereas xCreate is called 
        /// to create a new virtual table from scratch.
        /// </para>
        /// <para>
        /// The xCreate and xConnect methods are only different when the
        /// virtual table has some kind of backing store that must be initialized 
        /// the first time the virtual table is created. The xCreate method creates 
        /// and initializes the backing store. The xConnect method just connects 
        /// to an existing backing store.  When xCreate and xConnect are the same,
        /// the table is an eponymous virtual table.
        /// </para>
        /// <para>
        /// As an example, consider a virtual table implementation that 
        /// provides read-only access to existing comma-separated-value (CSV)
        /// files on disk. There is no backing store that needs to be created 
        /// or initialized for such a virtual table (since the CSV files already 
        /// exist on disk) so the xCreate and xConnect methods will be identical 
        /// for that module.
        /// </para>
        /// <para>
        /// Another example is a virtual table that implements a full-text index. 
        /// The xCreate method must create and initialize data structures to hold 
        /// the dictionary and posting lists for that index. The xConnect method,
        /// on the other hand, only has to locate and use an existing dictionary 
        /// and posting lists that were created by a prior xCreate call.
        /// </para>
        /// <para>
        /// The xConnect method must return SQLITE_OK if it is successful 
        /// in creating the new virtual table, or SQLITE_ERROR if it is not 
        /// successful. If not successful, the sqlite3_vtab structure must not be 
        /// allocated. An error message may optionally be returned in *pzErr if 
        /// unsuccessful. 
        /// Space to hold the error message string must be allocated using
        /// an SQLite memory allocation function like 
        /// sqlite3_malloc() or sqlite3_mprintf() as the SQLite core will
        /// attempt to free the space using sqlite3_free() after the error has
        /// been reported up to the application.
        /// </para>
        /// <para>
        /// The xConnect method is required for every virtual table implementation, 
        /// though the xCreate and xConnect pointers of the sqlite3_module object
        /// may point to the same function if the virtual table does not need to
        /// initialize backing store.
        /// </para>
        /// </summary>
        /// <param name="pDb">
        /// The native database connection handle.
        /// </param>
        /// <param name="pAux">
        /// The original native pointer value that was provided to the
        /// sqlite3_create_module(), sqlite3_create_module_v2() or
        /// sqlite3_create_disposable_module() functions.
        /// </param>
        /// <param name="argc">
        /// The number of arguments from the CREATE VIRTUAL TABLE statement.
        /// </param>
        /// <param name="argv">
        /// The array of string arguments from the CREATE VIRTUAL TABLE
        /// statement.
        /// </param>
        /// <param name="pVtab">
        /// Upon success, this parameter must be modified to point to the newly
        /// created native sqlite3_vtab derived structure.
        /// </param>
        /// <param name="pError">
        /// Upon failure, this parameter must be modified to point to the error
        /// message, with the underlying memory having been obtained from the
        /// sqlite3_malloc() function.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xConnect(
            IntPtr pDb,
            IntPtr pAux,
            int argc,
            IntPtr argv,
            ref IntPtr pVtab,
            ref IntPtr pError
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para>
        /// SQLite uses the xBestIndex method of a virtual table module to determine
        /// the best way to access the virtual table. 
        /// The xBestIndex method has a prototype like this:
        /// </para>
        /// <para><code>
        /// int (*xBestIndex)(sqlite3_vtab *pVTab, sqlite3_index_info*);
        /// </code></para>
        /// <para>
        /// The SQLite core communicates with the xBestIndex method by filling 
        /// in certain fields of the sqlite3_index_info structure and passing a 
        /// pointer to that structure into xBestIndex as the second parameter. 
        /// The xBestIndex method fills out other fields of this structure which
        /// forms the reply. The sqlite3_index_info structure looks like this:
        /// </para>
        /// <para><code>
        /// struct sqlite3_index_info {
        ///   /* Inputs */
        ///   const int nConstraint;     /* Number of entries in aConstraint */
        ///   const struct sqlite3_index_constraint {
        ///      int iColumn;              /* Column constrained.  -1 for ROWID */
        ///      unsigned char op;         /* Constraint operator */
        ///      unsigned char usable;     /* True if this constraint is usable */
        ///      int iTermOffset;          /* Used internally - xBestIndex should ignore */
        ///   } *const aConstraint;      /* Table of WHERE clause constraints */
        ///   const int nOrderBy;        /* Number of terms in the ORDER BY clause */
        ///   const struct sqlite3_index_orderby {
        ///      int iColumn;              /* Column number */
        ///      unsigned char desc;       /* True for DESC.  False for ASC. */
        ///   } *const aOrderBy;         /* The ORDER BY clause */
        ///   /* Outputs */
        ///   struct sqlite3_index_constraint_usage {
        ///     int argvIndex;           /* if >0, constraint is part of argv to xFilter */
        ///     unsigned char omit;      /* Do not code a test for this constraint */
        ///   } *const aConstraintUsage;
        ///   int idxNum;                /* Number used to identify the index */
        ///   char *idxStr;              /* String, possibly obtained from sqlite3_malloc */
        ///   int needToFreeIdxStr;      /* Free idxStr using sqlite3_free() if true */
        ///   int orderByConsumed;       /* True if output is already ordered */
        ///   double estimatedCost;      /* Estimated cost of using this index */
        ///   <![CDATA[<b>]]>/* Fields below are only available in SQLite 3.8.2 and later */<![CDATA[</b>]]>
        ///   sqlite3_int64 estimatedRows;    /* Estimated number of rows returned */
        ///   <![CDATA[<b>]]>/* Fields below are only available in SQLite 3.9.0 and later */<![CDATA[</b>]]>
        ///   int idxFlags;              /* Mask of SQLITE_INDEX_SCAN_* flags */
        ///   <![CDATA[<b>]]>/* Fields below are only available in SQLite 3.10.0 and later */<![CDATA[</b>]]>
        ///   sqlite3_uint64 colUsed;    /* Input: Mask of columns used by statement */
        /// };
        /// </code></para>
        /// <para>
        /// Note the warnings on the "estimatedRows", "idxFlags", and colUsed fields.
        /// These fields were added with SQLite versions 3.8.2, 3.9.0, and 3.10.0, respectively. 
        /// Any extension that reads or writes these fields must first check that the 
        /// version of the SQLite library in use is greater than or equal to appropriate
        /// version - perhaps comparing the value returned from sqlite3_libversion_number()
        /// against constants 3008002, 3009000, and/or 3010000. The result of attempting 
        /// to access these fields in an sqlite3_index_info structure created by an 
        /// older version of SQLite are undefined.
        /// </para>
        /// <para>
        /// In addition, there are some defined constants:
        /// </para>
        /// <para><code>
        /// #define SQLITE_INDEX_CONSTRAINT_EQ         2
        /// #define SQLITE_INDEX_CONSTRAINT_GT         4
        /// #define SQLITE_INDEX_CONSTRAINT_LE         8
        /// #define SQLITE_INDEX_CONSTRAINT_LT        16
        /// #define SQLITE_INDEX_CONSTRAINT_GE        32
        /// #define SQLITE_INDEX_CONSTRAINT_MATCH     64
        /// #define SQLITE_INDEX_CONSTRAINT_LIKE      65  /* 3.10.0 and later */
        /// #define SQLITE_INDEX_CONSTRAINT_GLOB      66  /* 3.10.0 and later */
        /// #define SQLITE_INDEX_CONSTRAINT_REGEXP    67  /* 3.10.0 and later */
        /// #define SQLITE_INDEX_CONSTRAINT_NE        68  /* 3.21.0 and later */
        /// #define SQLITE_INDEX_CONSTRAINT_ISNOT     69  /* 3.21.0 and later */
        /// #define SQLITE_INDEX_CONSTRAINT_ISNOTNULL 70  /* 3.21.0 and later */
        /// #define SQLITE_INDEX_CONSTRAINT_ISNULL    71  /* 3.21.0 and later */
        /// #define SQLITE_INDEX_CONSTRAINT_IS        72  /* 3.21.0 and later */
        /// #define SQLITE_INDEX_CONSTRAINT_FUNCTION 150  /* 3.25.0 and later */
        /// #define SQLITE_INDEX_SCAN_UNIQUE           1  /* Scan visits at most 1 row */
        /// </code></para>
        /// <para>
        /// The SQLite core calls the xBestIndex method when it is compiling a query
        /// that involves a virtual table. In other words, SQLite calls this method 
        /// when it is running sqlite3_prepare() or the equivalent. 
        /// By calling this method, the 
        /// SQLite core is saying to the virtual table that it needs to access 
        /// some subset of the rows in the virtual table and it wants to know the
        /// most efficient way to do that access. The xBestIndex method replies 
        /// with information that the SQLite core can then use to conduct an 
        /// efficient search of the virtual table.
        /// </para>
        /// <para>
        /// While compiling a single SQL query, the SQLite core might call 
        /// xBestIndex multiple times with different settings in sqlite3_index_info.
        /// The SQLite core will then select the combination that appears to 
        /// give the best performance.
        /// </para>
        /// <para>
        /// Before calling this method, the SQLite core initializes an instance 
        /// of the sqlite3_index_info structure with information about the
        /// query that it is currently trying to process. This information 
        /// derives mainly from the WHERE clause and ORDER BY or GROUP BY clauses 
        /// of the query, but also from any ON or USING clauses if the query is a 
        /// join. The information that the SQLite core provides to the xBestIndex 
        /// method is held in the part of the structure that is marked as "Inputs". 
        /// The "Outputs" section is initialized to zero.
        /// </para>
        /// <para>
        /// The information in the sqlite3_index_info structure is ephemeral
        /// and may be overwritten or deallocated as soon as the xBestIndex method
        /// returns.  If the xBestIndex method needs to remember any part of the
        /// sqlite3_index_info structure, it should make a copy.  Care must be
        /// take to store the copy in a place where it will be deallocated, such
        /// as in the idxStr field with needToFreeIdxStr set to 1.
        /// </para>
        /// <para>
        /// Note that xBestIndex will always be called before xFilter, since
        /// the idxNum and idxStr outputs from xBestIndex are required inputs to
        /// xFilter.  However, there is no guarantee that xFilter will be called
        /// following a successful xBestIndex.
        /// </para>
        /// <para>
        /// The xBestIndex method is required for every virtual table implementation.
        /// </para>
        /// <para>
        /// The main thing that the SQLite core is trying to communicate to 
        /// the virtual table is the constraints that are available to limit 
        /// the number of rows that need to be searched. The aConstraint[] array 
        /// contains one entry for each constraint. There will be exactly 
        /// nConstraint entries in that array.
        /// </para>
        /// <para>
        /// Each constraint will usually correspond to a term in the WHERE clause
        /// or in a USING or ON clause that is of the form
        /// </para>
        /// <para><code>
        ///      column  OP  EXPR
        /// </code></para>
        /// <para>
        /// Where "column" is a column in the virtual table, OP is an operator 
        /// like "=" or "&lt;", and EXPR is an arbitrary expression. So, for example,
        /// if the WHERE clause contained a term like this:
        /// </para>
        /// <para><code>
        /// a = 5
        /// </code></para>
        /// <para>
        /// Then one of the constraints would be on the "a" column with 
        /// operator "=" and an expression of "5". Constraints need not have a
        /// literal representation of the WHERE clause. The query optimizer might
        /// make transformations to the 
        /// WHERE clause in order to extract as many constraints 
        /// as it can. So, for example, if the WHERE clause contained something 
        /// like this:
        /// </para>
        /// <para><code>
        /// x BETWEEN 10 AND 100 AND 999&gt;y
        /// </code></para>
        /// <para>
        /// The query optimizer might translate this into three separate constraints:
        /// </para>
        /// <para><code>
        /// x &gt;= 10
        /// x &lt;= 100
        /// y &lt; 999
        /// </code></para>
        /// <para>
        /// For each such constraint, the aConstraint[].iColumn field indicates which 
        /// column appears on the left-hand side of the constraint.
        /// The first column of the virtual table is column 0. 
        /// The rowid of the virtual table is column -1. 
        /// The aConstraint[].op field indicates which operator is used. 
        /// The SQLITE_INDEX_CONSTRAINT_* constants map integer constants 
        /// into operator values.
        /// Columns occur in the order they were defined by the call to
        /// sqlite3_declare_vtab() in the xCreate or xConnect method.
        /// Hidden columns are counted when determining the column index.
        /// </para>
        /// <para>
        /// If the xFindFunction() method for the virtual table is defined, and 
        /// if xFindFunction() sometimes returns SQLITE_INDEX_CONSTRAINT_FUNCTION or
        /// larger, then the constraints might also be of the form:
        /// </para>
        /// <para><code>
        ///      FUNCTION( column, EXPR)
        /// </code></para>
        /// <para>
        /// In this case the aConstraint[].op value is the same as the value
        /// returned by xFindFunction() for FUNCTION.
        /// </para>
        /// <para>
        /// The aConstraint[] array contains information about all constraints 
        /// that apply to the virtual table. But some of the constraints might
        /// not be usable because of the way tables are ordered in a join. 
        /// The xBestIndex method must therefore only consider constraints 
        /// that have an aConstraint[].usable flag which is true.
        /// </para>
        /// <para>
        /// In addition to WHERE clause constraints, the SQLite core also 
        /// tells the xBestIndex method about the ORDER BY clause. 
        /// (In an aggregate query, the SQLite core might put in GROUP BY clause 
        /// information in place of the ORDER BY clause information, but this fact
        /// should not make any difference to the xBestIndex method.) 
        /// If all terms of the ORDER BY clause are columns in the virtual table, 
        /// then nOrderBy will be the number of terms in the ORDER BY clause 
        /// and the aOrderBy[] array will identify the column for each term 
        /// in the order by clause and whether or not that column is ASC or DESC.
        /// </para>
        /// <para>
        /// In SQLite version 3.10.0 (2016-01-06) and later, 
        /// the colUsed field is available
        /// to indicate which fields of the virtual table are actually used by the
        /// statement being prepared.  If the lowest bit of colUsed is set, that
        /// means that the first column is used.  The second lowest bit corresponds
        /// to the second column.  And so forth.  If the most significant bit of
        /// colUsed is set, that means that one or more columns other than the 
        /// first 63 columns are used.  If column usage information is needed by the
        /// xFilter method, then the required bits must be encoded into either
        /// the idxNum or idxStr output fields.
        /// </para>
        /// <para>
        /// Given all of the information above, the job of the xBestIndex 
        /// method it to figure out the best way to search the virtual table.
        /// </para>
        /// <para>
        /// The xBestIndex method fills the idxNum and idxStr fields with 
        /// information that communicates an indexing strategy to the xFilter 
        /// method. The information in idxNum and idxStr is arbitrary as far 
        /// as the SQLite core is concerned. The SQLite core just copies the 
        /// information through to the xFilter method. Any desired meaning can 
        /// be assigned to idxNum and idxStr as long as xBestIndex and xFilter 
        /// agree on what that meaning is.
        /// </para>
        /// <para>
        /// The idxStr value may be a string obtained from an SQLite
        /// memory allocation function such as sqlite3_mprintf(). 
        /// If this is the case, then the needToFreeIdxStr flag must be set to 
        /// true so that the SQLite core will know to call sqlite3_free() on 
        /// that string when it has finished with it, and thus avoid a memory leak.
        /// The idxStr value may also be a static constant string, in which case
        /// the needToFreeIdxStr boolean should remain false.
        /// </para>
        /// <para>
        /// If the virtual table will output rows in the order specified by 
        /// the ORDER BY clause, then the orderByConsumed flag may be set to 
        /// true. If the output is not automatically in the correct order 
        /// then orderByConsumed must be left in its default false setting. 
        /// This will indicate to the SQLite core that it will need to do a 
        /// separate sorting pass over the data after it comes out of the virtual table.
        /// </para>
        /// <para>
        /// The estimatedCost field should be set to the estimated number
        /// of disk access operations required to execute this query against 
        /// the virtual table. The SQLite core will often call xBestIndex 
        /// multiple times with different constraints, obtain multiple cost
        /// estimates, then choose the query plan that gives the lowest estimate.
        /// The SQLite core initializes estimatedCost to a very large value
        /// prior to invoking xBestIndex, so if xBestIndex determines that the
        /// current combination of parameters is undesirable, it can leave the
        /// estimatedCost field unchanged to discourage its use.
        /// </para>
        /// <para>
        /// If the current version of SQLite is 3.8.2 or greater, the estimatedRows
        /// field may be set to an estimate of the number of rows returned by the
        /// proposed query plan. If this value is not explicitly set, the default 
        /// estimate of 25 rows is used.
        /// </para>
        /// <para>
        /// If the current version of SQLite is 3.9.0 or greater, the idxFlags field
        /// may be set to SQLITE_INDEX_SCAN_UNIQUE to indicate that the virtual table
        /// will return only zero or one rows given the input constraints.  Additional
        /// bits of the idxFlags field might be understood in later versions of SQLite.
        /// </para>
        /// <para>
        /// The aConstraintUsage[] array contains one element for each of 
        /// the nConstraint constraints in the inputs section of the 
        /// sqlite3_index_info structure. 
        /// The aConstraintUsage[] array is used by xBestIndex to tell the 
        /// core how it is using the constraints.
        /// </para>
        /// <para>
        /// The xBestIndex method may set aConstraintUsage[].argvIndex 
        /// entries to values greater than zero. 
        /// Exactly one entry should be set to 1, another to 2, another to 3, 
        /// and so forth up to as many or as few as the xBestIndex method wants. 
        /// The EXPR of the corresponding constraints will then be passed 
        /// in as the argv[] parameters to xFilter.
        /// </para>
        /// <para>
        /// For example, if the aConstraint[3].argvIndex is set to 1, then 
        /// when xFilter is called, the argv[0] passed to xFilter will have 
        /// the EXPR value of the aConstraint[3] constraint.
        /// </para>
        /// <para>
        /// By default, the SQLite core double checks all constraints on 
        /// each row of the virtual table that it receives. If such a check 
        /// is redundant, the xBestFilter method can suppress that double-check by 
        /// setting aConstraintUsage[].omit.
        /// </para>
        /// <para>
        /// The xBestIndex method should return SQLITE_OK on success.  If any
        /// kind of fatal error occurs, an appropriate error code (ex: SQLITE_NOMEM)
        /// should be returned instead.
        /// </para>
        /// <para>
        /// If xBestIndex returns SQLITE_CONSTRAINT, that does not indicate an
        /// error.  Rather, SQLITE_CONSTRAINT indicates that the particular combination
        /// of input parameters specified should not be used in the query plan.
        /// The SQLITE_CONSTRAINT return is useful for table-valued functions that
        /// have required parameters.  If the aConstraint[].usable field is false
        /// for one of the required parameter, then the xBestIndex method should
        /// return SQLITE_CONSTRAINT.
        /// </para>
        /// <para>
        /// The following example will better illustrate the use of SQLITE_CONSTRAINT
        /// as a return value from xBestIndex:
        /// </para>
        /// <para><code>
        /// SELECT * FROM realtab, tablevaluedfunc(realtab.x);
        /// </code></para>
        /// <para>
        /// Assuming that the first hidden column of "tablevaluedfunc" is "param1",
        /// the query above is semantically equivalent to this:
        /// </para>
        /// <para><code>
        /// SELECT * FROM realtab, tablevaluedfunc
        ///  WHERE tablevaluedfunc.param1 = realtab.x;
        /// </code></para>
        /// <para>
        /// The query planner must decide between many possible implementations
        /// of this query, but two plans in particular are of note:
        /// </para>
        /// <![CDATA[<ol>]]>
        /// <![CDATA[<li>]]>Scan all
        /// rows of realtab and for each row, find rows in tablevaluedfunc where
        /// param1 is equal to realtab.x
        /// <![CDATA[</li>]]><![CDATA[<li>]]>Scan all rows of tablevalued func and for each row find rows
        /// in realtab where x is equal to tablevaluedfunc.param1.
        /// <![CDATA[</li>]]><![CDATA[</ol>]]>
        /// <para>
        /// The xBestIndex method will be invoked once for each of the potential
        /// plans above.  For plan 1, the aConstraint[].usable flag for for the
        /// SQLITE_CONSTRAINT_EQ constraint on the param1 column will be true because
        /// the right-hand side value for the "param1 = ?" constraint will be known,
        /// since it is determined by the outer realtab loop.
        /// But for plan 2, the aConstraint[].usable flag for "param1 = ?" will be false
        /// because the right-hand side value is determined by an inner loop and is thus
        /// an unknown quantity.  Because param1 is a required input to the table-valued
        /// functions, the xBestIndex method should return SQLITE_CONSTRAINT when presented 
        /// with plan 2, indicating that a required input is missing.  This forces the
        /// query planner to select plan 1.
        /// </para>
        /// </summary>
        /// <param name="pVtab">
        /// The native pointer to the sqlite3_vtab derived structure.
        /// </param>
        /// <param name="pIndex">
        /// The native pointer to the sqlite3_index_info structure.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xBestIndex(
            IntPtr pVtab,
            IntPtr pIndex
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xDisconnect)(sqlite3_vtab *pVTab);
        /// </code></para>
        /// <para>
        /// This method releases a connection to a virtual table. 
        /// Only the sqlite3_vtab object is destroyed.
        /// The virtual table is not destroyed and any backing store 
        /// associated with the virtual table persists. 
        /// </para>
        /// This method undoes the work of xConnect.
        /// <para>
        /// This method is a destructor for a connection to the virtual table.
        /// Contrast this method with xDestroy.  The xDestroy is a destructor
        /// for the entire virtual table.
        /// </para>
        /// <para>
        /// The xDisconnect method is required for every virtual table implementation,
        /// though it is acceptable for the xDisconnect and xDestroy methods to be
        /// the same function if that makes sense for the particular virtual table.
        /// </para>
        /// </summary>
        /// <param name="pVtab">
        /// The native pointer to the sqlite3_vtab derived structure.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xDisconnect(
            IntPtr pVtab
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xDestroy)(sqlite3_vtab *pVTab);
        /// </code></para>
        /// <para>
        /// This method releases a connection to a virtual table, just like 
        /// the xDisconnect method, and it also destroys the underlying 
        /// table implementation. This method undoes the work of xCreate.
        /// </para>
        /// <para>
        /// The xDisconnect method is called whenever a database connection
        /// that uses a virtual table is closed. The xDestroy method is only 
        /// called when a DROP TABLE statement is executed against the virtual table.
        /// </para>
        /// <para>
        /// The xDestroy method is required for every virtual table implementation,
        /// though it is acceptable for the xDisconnect and xDestroy methods to be
        /// the same function if that makes sense for the particular virtual table.
        /// </para>
        /// </summary>
        /// <param name="pVtab">
        /// The native pointer to the sqlite3_vtab derived structure.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xDestroy(
            IntPtr pVtab
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xOpen)(sqlite3_vtab *pVTab, sqlite3_vtab_cursor **ppCursor);
        /// </code></para>
        /// <para>
        /// The xOpen method creates a new cursor used for accessing (read and/or
        /// writing) a virtual table.  A successful invocation of this method 
        /// will allocate the memory for the sqlite3_vtab_cursor (or a subclass),
        /// initialize the new object, and make *ppCursor point to the new object.
        /// The successful call then returns SQLITE_OK.
        /// </para>
        /// <para>
        /// For every successful call to this method, the SQLite core will
        /// later invoke the xClose method to destroy 
        /// the allocated cursor.
        /// </para>
        /// <para>
        /// The xOpen method need not initialize the pVtab field of the
        /// sqlite3_vtab_cursor structure.  The SQLite core will take care
        /// of that chore automatically.
        /// </para>
        /// <para>
        /// A virtual table implementation must be able to support an arbitrary
        /// number of simultaneously open cursors.
        /// </para>
        /// <para>
        /// When initially opened, the cursor is in an undefined state.
        /// The SQLite core will invoke the xFilter method
        /// on the cursor prior to any attempt to position or read from the cursor.
        /// </para>
        /// <para>
        /// The xOpen method is required for every virtual table implementation.
        /// </para>
        /// </summary>
        /// <param name="pVtab">
        /// The native pointer to the sqlite3_vtab derived structure.
        /// </param>
        /// <param name="pCursor">
        /// Upon success, this parameter must be modified to point to the newly
        /// created native sqlite3_vtab_cursor derived structure.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xOpen(
            IntPtr pVtab,
            ref IntPtr pCursor
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xClose)(sqlite3_vtab_cursor*);
        /// </code></para>
        /// <para>
        /// The xClose method closes a cursor previously opened by 
        /// xOpen. 
        /// The SQLite core will always call xClose once for each cursor opened 
        /// using xOpen.
        /// </para>
        /// <para>
        /// This method must release all resources allocated by the
        /// corresponding xOpen call. The routine will not be called again even if it
        /// returns an error.  The SQLite core will not use the
        /// sqlite3_vtab_cursor again after it has been closed.
        /// </para>
        /// <para>
        /// The xClose method is required for every virtual table implementation.
        /// </para>
        /// </summary>
        /// <param name="pCursor">
        /// The native pointer to the sqlite3_vtab_cursor derived structure.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xClose(
            IntPtr pCursor
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xFilter)(sqlite3_vtab_cursor*, int idxNum, const char *idxStr,
        ///               int argc, sqlite3_value **argv);
        /// </code></para>
        /// <para>
        /// This method begins a search of a virtual table. 
        /// The first argument is a cursor opened by xOpen. 
        /// The next two arguments define a particular search index previously 
        /// chosen by xBestIndex. The specific meanings of idxNum and idxStr 
        /// are unimportant as long as xFilter and xBestIndex agree on what 
        /// that meaning is.
        /// </para>
        /// <para>
        /// The xBestIndex function may have requested the values of 
        /// certain expressions using the aConstraintUsage[].argvIndex values 
        /// of the sqlite3_index_info structure. 
        /// Those values are passed to xFilter using the argc and argv parameters.
        /// </para>
        /// <para>
        /// If the virtual table contains one or more rows that match the
        /// search criteria, then the cursor must be left point at the first row.
        /// Subsequent calls to xEof must return false (zero).
        /// If there are no rows match, then the cursor must be left in a state 
        /// that will cause the xEof to return true (non-zero).
        /// The SQLite engine will use
        /// the xColumn and xRowid methods to access that row content.
        /// The xNext method will be used to advance to the next row.
        /// </para>
        /// <para>
        /// This method must return SQLITE_OK if successful, or an sqlite 
        /// error code if an error occurs.
        /// </para>
        /// <para>
        /// The xFilter method is required for every virtual table implementation.
        /// </para>
        /// </summary>
        /// <param name="pCursor">
        /// The native pointer to the sqlite3_vtab_cursor derived structure.
        /// </param>
        /// <param name="idxNum">
        /// Number used to help identify the selected index.
        /// </param>
        /// <param name="idxStr">
        /// The native pointer to the UTF-8 encoded string containing the
        /// string used to help identify the selected index.
        /// </param>
        /// <param name="argc">
        /// The number of native pointers to sqlite3_value structures specified
        /// in <paramref name="argv" />.
        /// </param>
        /// <param name="argv">
        /// An array of native pointers to sqlite3_value structures containing
        /// filtering criteria for the selected index.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xFilter(
            IntPtr pCursor,
            int idxNum,
            IntPtr idxStr,
            int argc,
            IntPtr argv
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xNext)(sqlite3_vtab_cursor*);
        /// </code></para>
        /// <para>
        /// The xNext method advances a virtual table cursor
        /// to the next row of a result set initiated by xFilter. 
        /// If the cursor is already pointing at the last row when this 
        /// routine is called, then the cursor no longer points to valid 
        /// data and a subsequent call to the xEof method must return true (non-zero). 
        /// If the cursor is successfully advanced to another row of content, then
        /// subsequent calls to xEof must return false (zero).
        /// </para>
        /// <para>
        /// This method must return SQLITE_OK if successful, or an sqlite 
        /// error code if an error occurs.
        /// </para>
        /// <para>
        /// The xNext method is required for every virtual table implementation.
        /// </para>
        /// </summary>
        /// <param name="pCursor">
        /// The native pointer to the sqlite3_vtab_cursor derived structure.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xNext(
            IntPtr pCursor
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xEof)(sqlite3_vtab_cursor*);
        /// </code></para>
        /// <para>
        /// The xEof method must return false (zero) if the specified cursor 
        /// currently points to a valid row of data, or true (non-zero) otherwise. 
        /// This method is called by the SQL engine immediately after each 
        /// xFilter and xNext invocation.
        /// </para>
        /// <para>
        /// The xEof method is required for every virtual table implementation.
        /// </para>
        /// </summary>
        /// <param name="pCursor">
        /// The native pointer to the sqlite3_vtab_cursor derived structure.
        /// </param>
        /// <returns>
        /// Non-zero if no more rows are available; zero otherwise.
        /// </returns>
        int xEof(
            IntPtr pCursor
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xColumn)(sqlite3_vtab_cursor*, sqlite3_context*, int N);
        /// </code></para>
        /// <para>
        /// The SQLite core invokes this method in order to find the value for 
        /// the N-th column of the current row. N is zero-based so the first column 
        /// is numbered 0. 
        /// The xColumn method may return its result back to SQLite using one of the
        /// following interface:
        /// </para>
        /// <para>
        /// <![CDATA[<ul>]]>
        /// <![CDATA[<li>]]> sqlite3_result_blob()
        /// <![CDATA[</li>]]><![CDATA[<li>]]> sqlite3_result_double()
        /// <![CDATA[</li>]]><![CDATA[<li>]]> sqlite3_result_int()
        /// <![CDATA[</li>]]><![CDATA[<li>]]> sqlite3_result_int64()
        /// <![CDATA[</li>]]><![CDATA[<li>]]> sqlite3_result_null()
        /// <![CDATA[</li>]]><![CDATA[<li>]]> sqlite3_result_text()
        /// <![CDATA[</li>]]><![CDATA[<li>]]> sqlite3_result_text16()
        /// <![CDATA[</li>]]><![CDATA[<li>]]> sqlite3_result_text16le()
        /// <![CDATA[</li>]]><![CDATA[<li>]]> sqlite3_result_text16be()
        /// <![CDATA[</li>]]><![CDATA[<li>]]> sqlite3_result_zeroblob()
        /// <![CDATA[</li>]]><![CDATA[</ul>]]>
        /// </para>
        /// <para>
        /// If the xColumn method implementation calls none of the functions above,
        /// then the value of the column defaults to an SQL NULL.
        /// </para>
        /// <para>
        /// To raise an error, the xColumn method should use one of the result_text() 
        /// methods to set the error message text, then return an appropriate
        /// error code.  The xColumn method must return SQLITE_OK on success.
        /// </para>
        /// <para>
        /// The xColumn method is required for every virtual table implementation.
        /// </para>
        /// </summary>
        /// <param name="pCursor">
        /// The native pointer to the sqlite3_vtab_cursor derived structure.
        /// </param>
        /// <param name="pContext">
        /// The native pointer to the sqlite3_context structure to be used
        /// for returning the specified column value to the SQLite core
        /// library.
        /// </param>
        /// <param name="index">
        /// The zero-based index corresponding to the column containing the
        /// value to be returned.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xColumn(
            IntPtr pCursor,
            IntPtr pContext,
            int index
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xRowid)(sqlite3_vtab_cursor *pCur, sqlite_int64 *pRowid);
        /// </code></para>
        /// <para>
        /// A successful invocation of this method will cause *pRowid to be
        /// filled with the rowid of row that the
        /// virtual table cursor pCur is currently pointing at.
        /// This method returns SQLITE_OK on success.
        /// It returns an appropriate error code on failure.
        /// </para>
        /// <para>
        /// The xRowid method is required for every virtual table implementation.
        /// </para>
        /// </summary>
        /// <param name="pCursor">
        /// The native pointer to the sqlite3_vtab_cursor derived structure.
        /// </param>
        /// <param name="rowId">
        /// Upon success, this parameter must be modified to contain the unique
        /// integer row identifier for the current row for the specified cursor.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xRowId(
            IntPtr pCursor,
            ref long rowId
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xUpdate)(
        ///   sqlite3_vtab *pVTab,
        ///   int argc,
        ///   sqlite3_value **argv,
        ///   sqlite_int64 *pRowid
        /// );
        /// </code></para>
        /// <para>
        /// All changes to a virtual table are made using the xUpdate method.
        /// This one method can be used to insert, delete, or update.
        /// </para>
        /// <para>
        /// The argc parameter specifies the number of entries in the argv array. 
        /// The value of argc will be 1 for a pure delete operation or N+2 for an insert
        /// or replace or update where N is the number of columns in the table.  
        /// In the previous sentence, N includes any hidden columns.
        /// </para>
        /// <para>
        /// Every argv entry will have a non-NULL value in C but may contain the 
        /// SQL value NULL.  In other words, it is always true that
        /// <![CDATA[<tt>]]>argv[i]!=0<![CDATA[</tt>]]> for <![CDATA[<b>]]>i<![CDATA[</b>]]> between 0 and <![CDATA[<tt>]]>argc-1<![CDATA[</tt>]]>.
        /// However, it might be the case that
        /// <![CDATA[<tt>]]>sqlite3_value_type(argv[i])==SQLITE_NULL<![CDATA[</tt>]]>.
        /// </para>
        /// <para>
        /// The argv[0] parameter is the rowid of a row in the virtual table 
        /// to be deleted. If argv[0] is an SQL NULL, then no deletion occurs.
        /// </para>
        /// <para>
        /// The argv[1] parameter is the rowid of a new row to be inserted 
        /// into the virtual table. If argv[1] is an SQL NULL, then the implementation 
        /// must choose a rowid for the newly inserted row. Subsequent argv[] 
        /// entries contain values of the columns of the virtual table, in the 
        /// order that the columns were declared. The number of columns will
        /// match the table declaration that the xConnect or xCreate method made 
        /// using the sqlite3_declare_vtab() call.  All hidden columns are included.
        /// </para>
        /// <para>
        /// When doing an insert without a rowid (argc>1, argv[1] is an SQL NULL),
        /// on a virtual table that uses ROWID (but not on a WITHOUT ROWID virtual table),
        /// the implementation must set *pRowid to the rowid of the newly inserted row; 
        /// this will become the value returned by the sqlite3_last_insert_rowid()
        /// function. Setting this value in all the other cases is a harmless no-op;
        /// the SQLite engine ignores the *pRowid return value if argc==1 or 
        /// argv[1] is not an SQL NULL.
        /// </para>
        /// <para>
        /// Each call to xUpdate will fall into one of cases shown below.
        /// Not that references to <![CDATA[<b>]]>argv[i]<![CDATA[</b>]]> mean the SQL value
        /// held within the argv[i] object, not the argv[i]
        /// object itself.
        /// </para>
        /// <para><code>
        /// <![CDATA[<dl>]]>
        /// <![CDATA[<dt>]]><![CDATA[<b>]]>argc = 1 <![CDATA[<br>]]> argv[0] &#8800; NULL<![CDATA[</b>]]>
        /// <![CDATA[</dt>]]><![CDATA[<dd>]]>
        /// DELETE: The single row with rowid or PRIMARY KEY equal to argv[0] is deleted. 
        /// No insert occurs.
        /// <![CDATA[</dd>]]><![CDATA[<dt>]]><![CDATA[<b>]]>argc &gt; 1 <![CDATA[<br>]]> argv[0] = NULL<![CDATA[</b>]]>
        /// <![CDATA[</dt>]]><![CDATA[<dd>]]>
        /// INSERT: A new row is inserted with column values taken from
        /// argv[2] and following.  In a rowid virtual table, if argv[1] is an SQL NULL,
        /// then a new unique rowid is generated automatically.  The argv[1] will be NULL
        /// for a WITHOUT ROWID virtual table, in which case the implementation should
        /// take the PRIMARY KEY value from the appropriate column in argv[2] and following.
        /// <![CDATA[</dd>]]><![CDATA[<dt>]]><![CDATA[<b>]]>argc &gt; 1 <![CDATA[<br>]]> argv[0] &#8800; NULL <![CDATA[<br>]]> argv[0] = argv[1]<![CDATA[</b>]]>
        /// <![CDATA[</dt>]]><![CDATA[<dd>]]>
        /// UPDATE:
        /// The row with rowid or PRIMARY KEY argv[0] is updated with new values 
        /// in argv[2] and following parameters.
        /// <![CDATA[</dd>]]><![CDATA[<dt>]]><![CDATA[<b>]]>argc &gt; 1 <![CDATA[<br>]]> argv[0] &#8800; NULL <![CDATA[<br>]]> argv[0] &#8800; argv[1]<![CDATA[</b>]]>
        /// <![CDATA[</dt>]]><![CDATA[<dd>]]>
        /// UPDATE with rowid or PRIMARY KEY change:
        /// The row with rowid or PRIMARY KEY argv[0] is updated with 
        /// the rowid or PRIMARY KEY in argv[1] 
        /// and new values in argv[2] and following parameters. This will occur 
        /// when an SQL statement updates a rowid, as in the statement:
        /// <para><code>
        ///    UPDATE table SET rowid=rowid+1 WHERE ...; 
        /// </code></para>
        /// <![CDATA[</dd>]]><![CDATA[</dl>]]>
        /// </code></para>
        /// <para>
        /// The xUpdate method must return SQLITE_OK if and only if it is
        /// successful.  If a failure occurs, the xUpdate must return an appropriate
        /// error code.  On a failure, the pVTab->zErrMsg element may optionally
        /// be replaced with error message text stored in memory allocated from SQLite 
        /// using functions such as sqlite3_mprintf() or sqlite3_malloc().
        /// </para>
        /// <para>
        /// If the xUpdate method violates some constraint of the virtual table
        /// (including, but not limited to, attempting to store a value of the wrong 
        /// datatype, attempting to store a value that is too
        /// large or too small, or attempting to change a read-only value) then the
        /// xUpdate must fail with an appropriate error code.
        /// </para>
        /// <para>
        /// If the xUpdate method is performing an UPDATE, then
        /// sqlite3_value_nochange(X) can be used to discover which columns
        /// of the virtual table were actually modified by the UPDATE
        /// statement.  The sqlite3_value_nochange(X) interface returns
        /// true for columns that do not change.
        /// On every UPDATE, SQLite will first invoke
        /// xColumn separately for each unchanging column in the table to 
        /// obtain the value for that column.  The xColumn method can
        /// check to see if the column is unchanged at the SQL level
        /// by invoking sqlite3_vtab_nochange().  If xColumn sees that
        /// the column is not being modified, it should return without setting 
        /// a result using one of the sqlite3_result_xxxxx()
        /// interfaces.  Only in that case sqlite3_value_nochange() will be
        /// true within the xUpdate method.  If xColumn does
        /// invoke one or more sqlite3_result_xxxxx()
        /// interfaces, then SQLite understands that as a change in the value
        /// of the column and the sqlite3_value_nochange() call for that
        /// column within xUpdate will return false.
        /// </para>
        /// <para>
        /// There might be one or more sqlite3_vtab_cursor objects open and in use 
        /// on the virtual table instance and perhaps even on the row of the virtual
        /// table when the xUpdate method is invoked.  The implementation of
        /// xUpdate must be prepared for attempts to delete or modify rows of the table
        /// out from other existing cursors.  If the virtual table cannot accommodate
        /// such changes, the xUpdate method must return an error code.
        /// </para>
        /// <para>
        /// The xUpdate method is optional.
        /// If the xUpdate pointer in the sqlite3_module for a virtual table
        /// is a NULL pointer, then the virtual table is read-only.
        /// </para>
        /// </summary>
        /// <param name="pVtab">
        /// The native pointer to the sqlite3_vtab derived structure.
        /// </param>
        /// <param name="argc">
        /// The number of new or modified column values contained in
        /// <paramref name="argv" />.
        /// </param>
        /// <param name="argv">
        /// The array of native pointers to sqlite3_value structures containing
        /// the new or modified column values, if any.
        /// </param>
        /// <param name="rowId">
        /// Upon success, this parameter must be modified to contain the unique
        /// integer row identifier for the row that was inserted, if any.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xUpdate(
            IntPtr pVtab,
            int argc,
            IntPtr argv,
            ref long rowId
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xBegin)(sqlite3_vtab *pVTab);
        /// </code></para>
        /// <para>
        /// This method begins a transaction on a virtual table.
        /// This is method is optional.  The xBegin pointer of sqlite3_module
        /// may be NULL.
        /// </para>
        /// <para>
        /// This method is always followed by one call to either the
        /// xCommit or xRollback method.  Virtual table transactions do
        /// not nest, so the xBegin method will not be invoked more than once
        /// on a single virtual table
        /// without an intervening call to either xCommit or xRollback.
        /// Multiple calls to other methods can and likely will occur in between
        /// the xBegin and the corresponding xCommit or xRollback.
        /// </para>
        /// </summary>
        /// <param name="pVtab">
        /// The native pointer to the sqlite3_vtab derived structure.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xBegin(
            IntPtr pVtab
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xSync)(sqlite3_vtab *pVTab);
        /// </code></para>
        /// <para>
        /// This method signals the start of a two-phase commit on a virtual
        /// table.
        /// This is method is optional.  The xSync pointer of sqlite3_module
        /// may be NULL.
        /// </para>
        /// <para>
        /// This method is only invoked after call to the xBegin method and
        /// prior to an xCommit or xRollback.  In order to implement two-phase
        /// commit, the xSync method on all virtual tables is invoked prior to
        /// invoking the xCommit method on any virtual table.  If any of the 
        /// xSync methods fail, the entire transaction is rolled back.
        /// </para>
        /// </summary>
        /// <param name="pVtab">
        /// The native pointer to the sqlite3_vtab derived structure.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xSync(
            IntPtr pVtab
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xCommit)(sqlite3_vtab *pVTab);
        /// </code></para>
        /// <para>
        /// This method causes a virtual table transaction to commit.
        /// This is method is optional.  The xCommit pointer of sqlite3_module
        /// may be NULL.
        /// </para>
        /// <para>
        /// A call to this method always follows a prior call to xBegin and
        /// xSync.
        /// </para>
        /// </summary>
        /// <param name="pVtab">
        /// The native pointer to the sqlite3_vtab derived structure.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xCommit(
            IntPtr pVtab
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xRollback)(sqlite3_vtab *pVTab);
        /// </code></para>
        /// <para>
        /// This method causes a virtual table transaction to rollback.
        /// This is method is optional.  The xRollback pointer of sqlite3_module
        /// may be NULL.
        /// </para>
        /// <para>
        /// A call to this method always follows a prior call to xBegin.
        /// </para>
        /// </summary>
        /// <param name="pVtab">
        /// The native pointer to the sqlite3_vtab derived structure.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xRollback(
            IntPtr pVtab
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xFindFunction)(
        ///   sqlite3_vtab *pVtab,
        ///   int nArg,
        ///   const char *zName,
        ///   void (**pxFunc)(sqlite3_context*,int,sqlite3_value**),
        ///   void **ppArg
        /// );
        /// </code></para>
        /// <para>
        /// This method is called during sqlite3_prepare() to give the virtual
        /// table implementation an opportunity to overload functions. 
        /// This method may be set to NULL in which case no overloading occurs.
        /// </para>
        /// <para>
        /// When a function uses a column from a virtual table as its first 
        /// argument, this method is called to see if the virtual table would 
        /// like to overload the function. The first three parameters are inputs: 
        /// the virtual table, the number of arguments to the function, and the 
        /// name of the function. If no overloading is desired, this method
        /// returns 0. To overload the function, this method writes the new 
        /// function implementation into *pxFunc and writes user data into *ppArg 
        /// and returns either 1 or a number between
        /// SQLITE_INDEX_CONSTRAINT_FUNCTION and 255.
        /// </para>
        /// <para>
        /// Historically, the return value from xFindFunction() was either zero
        /// or one.  Zero means that the function is not overloaded and one means that
        /// it is overload.  The ability to return values of 
        /// SQLITE_INDEX_CONSTRAINT_FUNCTION or greater was added in
        /// version 3.25.0 (2018-09-15).  If xFindFunction returns
        /// SQLITE_INDEX_CONSTRAINT_FUNCTION or greater, than means that the function
        /// takes two arguments and the function
        /// can be used as a boolean in the WHERE clause of a query and that
        /// the virtual table is able to exploit that function to speed up the query
        /// result.  When xFindFunction returns SQLITE_INDEX_CONSTRAINT_FUNCTION or 
        /// larger, the value returned becomes the sqlite3_index_info.aConstraint.op
        /// value for one of the constraints passed into xBestIndex() and the second
        /// argument becomes the value corresponding to that constraint that is passed
        /// to xFilter().  This enables the
        /// xBestIndex()/xFilter implementations to use the function to speed
        /// its search.
        /// </para>
        /// <para>
        /// The technique of having xFindFunction() return values of
        /// SQLITE_INDEX_CONSTRAINT_FUNCTION was initially used in the implementation
        /// of the Geopoly module.  The xFindFunction() method of that module returns
        /// SQLITE_INDEX_CONSTRAINT_FUNCTION for the geopoly_overlap() SQL function
        /// and it returns
        /// SQLITE_INDEX_CONSTRAINT_FUNCTION+1 for the geopoly_within() SQL function.
        /// This permits search optimizations for queries such as:
        /// </para>
        /// <para><code>
        /// SELECT * FROM geopolytab WHERE geopoly_overlap(_shape, $query_polygon);
        /// </code></para>
        /// <para>
        /// Note that infix functions (LIKE, GLOB, REGEXP, and MATCH) reverse 
        /// the order of their arguments. So "like(A,B)" is equivalent to "B like A". 
        /// For the form "B like A" the B term is considered the first argument 
        /// to the function. But for "like(A,B)" the A term is considered the 
        /// first argument.
        /// </para>
        /// <para>
        /// The function pointer returned by this routine must be valid for
        /// the lifetime of the sqlite3_vtab object given in the first parameter.
        /// </para>
        /// </summary>
        /// <param name="pVtab">
        /// The native pointer to the sqlite3_vtab derived structure.
        /// </param>
        /// <param name="nArg">
        /// The number of arguments to the function being sought.
        /// </param>
        /// <param name="zName">
        /// The name of the function being sought.
        /// </param>
        /// <param name="callback">
        /// Upon success, this parameter must be modified to contain the
        /// delegate responsible for implementing the specified function.
        /// </param>
        /// <param name="pClientData">
        /// Upon success, this parameter must be modified to contain the
        /// native user-data pointer associated with
        /// <paramref name="callback" />.
        /// </param>
        /// <returns>
        /// Non-zero if the specified function was found; zero otherwise.
        /// </returns>
        int xFindFunction(
            IntPtr pVtab,
            int nArg,
            IntPtr zName,
            ref SQLiteCallback callback,
            ref IntPtr pClientData
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xRename)(sqlite3_vtab *pVtab, const char *zNew);
        /// </code></para>
        /// <para>
        /// This method provides notification that the virtual table implementation
        /// that the virtual table will be given a new name. 
        /// If this method returns SQLITE_OK then SQLite renames the table.
        /// If this method returns an error code then the renaming is prevented.
        /// </para>
        /// <para>
        /// The xRename method is optional.  If omitted, then the virtual
        /// table may not be renamed using the ALTER TABLE RENAME command.
        /// </para>
        /// <para>
        /// The PRAGMA legacy_alter_table setting is enabled prior to invoking this
        /// method, and the value for legacy_alter_table is restored after this
        /// method finishes.  This is necessary for the correct operation of virtual
        /// tables that make use of shadow tables where the shadow tables must be
        /// renamed to match the new virtual table name.  If the legacy_alter_format is
        /// off, then the xConnect method will be invoked for the virtual table every
        /// time the xRename method tries to change the name of the shadow table.
        /// </para>
        /// </summary>
        /// <param name="pVtab">
        /// The native pointer to the sqlite3_vtab derived structure.
        /// </param>
        /// <param name="zNew">
        /// The native pointer to the UTF-8 encoded string containing the new
        /// name for the virtual table.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xRename(
            IntPtr pVtab,
            IntPtr zNew
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xSavepoint)(sqlite3_vtab *pVtab, int);
        /// int (*xRelease)(sqlite3_vtab *pVtab, int);
        /// int (*xRollbackTo)(sqlite3_vtab *pVtab, int);
        /// </code></para>
        /// <para>
        /// These methods provide the virtual table implementation an opportunity to
        /// implement nested transactions.  They are always optional and will only be
        /// called in SQLite version 3.7.7 (2011-06-23) and later.
        /// </para>
        /// <para>
        /// When xSavepoint(X,N) is invoked, that is a signal to the virtual table X
        /// that it should save its current state as savepoint N.  
        /// A subsequent call
        /// to xRollbackTo(X,R) means that the state of the virtual table should return
        /// to what it was when xSavepoint(X,R) was last called.  
        /// The call
        /// to xRollbackTo(X,R) will invalidate all savepoints with N>R; none of the
        /// invalided savepoints will be rolled back or released without first
        /// being reinitialized by a call to xSavepoint().  
        /// A call to xRelease(X,M) invalidates all savepoints where N>=M.
        /// </para>
        /// <para>
        /// None of the xSavepoint(), xRelease(), or xRollbackTo() methods will ever
        /// be called except in between calls to xBegin() and 
        /// either xCommit() or xRollback().
        /// </para>
        /// </summary>
        /// <param name="pVtab">
        /// The native pointer to the sqlite3_vtab derived structure.
        /// </param>
        /// <param name="iSavepoint">
        /// This is an integer identifier under which the the current state of
        /// the virtual table should be saved.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xSavepoint(
            IntPtr pVtab,
            int iSavepoint
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xSavepoint)(sqlite3_vtab *pVtab, int);
        /// int (*xRelease)(sqlite3_vtab *pVtab, int);
        /// int (*xRollbackTo)(sqlite3_vtab *pVtab, int);
        /// </code></para>
        /// <para>
        /// These methods provide the virtual table implementation an opportunity to
        /// implement nested transactions.  They are always optional and will only be
        /// called in SQLite version 3.7.7 (2011-06-23) and later.
        /// </para>
        /// <para>
        /// When xSavepoint(X,N) is invoked, that is a signal to the virtual table X
        /// that it should save its current state as savepoint N.  
        /// A subsequent call
        /// to xRollbackTo(X,R) means that the state of the virtual table should return
        /// to what it was when xSavepoint(X,R) was last called.  
        /// The call
        /// to xRollbackTo(X,R) will invalidate all savepoints with N>R; none of the
        /// invalided savepoints will be rolled back or released without first
        /// being reinitialized by a call to xSavepoint().  
        /// A call to xRelease(X,M) invalidates all savepoints where N>=M.
        /// </para>
        /// <para>
        /// None of the xSavepoint(), xRelease(), or xRollbackTo() methods will ever
        /// be called except in between calls to xBegin() and 
        /// either xCommit() or xRollback().
        /// </para>
        /// </summary>
        /// <param name="pVtab">
        /// The native pointer to the sqlite3_vtab derived structure.
        /// </param>
        /// <param name="iSavepoint">
        /// This is an integer used to indicate that any saved states with an
        /// identifier greater than or equal to this should be deleted by the
        /// virtual table.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xRelease(
            IntPtr pVtab,
            int iSavepoint
            );

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// <para><code>
        /// int (*xSavepoint)(sqlite3_vtab *pVtab, int);
        /// int (*xRelease)(sqlite3_vtab *pVtab, int);
        /// int (*xRollbackTo)(sqlite3_vtab *pVtab, int);
        /// </code></para>
        /// <para>
        /// These methods provide the virtual table implementation an opportunity to
        /// implement nested transactions.  They are always optional and will only be
        /// called in SQLite version 3.7.7 (2011-06-23) and later.
        /// </para>
        /// <para>
        /// When xSavepoint(X,N) is invoked, that is a signal to the virtual table X
        /// that it should save its current state as savepoint N.  
        /// A subsequent call
        /// to xRollbackTo(X,R) means that the state of the virtual table should return
        /// to what it was when xSavepoint(X,R) was last called.  
        /// The call
        /// to xRollbackTo(X,R) will invalidate all savepoints with N>R; none of the
        /// invalided savepoints will be rolled back or released without first
        /// being reinitialized by a call to xSavepoint().  
        /// A call to xRelease(X,M) invalidates all savepoints where N>=M.
        /// </para>
        /// <para>
        /// None of the xSavepoint(), xRelease(), or xRollbackTo() methods will ever
        /// be called except in between calls to xBegin() and 
        /// either xCommit() or xRollback().
        /// </para>
        /// </summary>
        /// <param name="pVtab">
        /// The native pointer to the sqlite3_vtab derived structure.
        /// </param>
        /// <param name="iSavepoint">
        /// This is an integer identifier used to specify a specific saved
        /// state for the virtual table for it to restore itself back to, which
        /// should also have the effect of deleting all saved states with an
        /// integer identifier greater than this one.
        /// </param>
        /// <returns>
        /// A standard SQLite return code.
        /// </returns>
        SQLiteErrorCode xRollbackTo(
            IntPtr pVtab,
            int iSavepoint
            );
    }
    #endregion
}
