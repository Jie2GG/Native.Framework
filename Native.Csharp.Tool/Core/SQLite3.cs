using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Native.Csharp.Tool.Core
{
	public static class SQLite3
	{
		#region --常量--
		private const string DllName = "sqlite3.dll";
		#endregion

		[DllImport (DllName, EntryPoint = "sqlite3_errmsg", CallingConvention = CallingConvention.Cdecl)]
		public static extern String Sqlite3_errmsg (IntPtr qDb);

		[DllImport (DllName, EntryPoint = "sqlite3_errcode", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_errcode (IntPtr pDb);

		[DllImport (DllName, EntryPoint = "sqlite3_open_v2", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_open_v2 (String szFileName, out IntPtr pDb, Int32 flags, Int32 zVfs);

		[DllImport (DllName, EntryPoint = "sqlite3_close", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_close (IntPtr pDb);

		[DllImport (DllName, EntryPoint = "sqlite3_free", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_free (IntPtr pMem);

		[DllImport (DllName, EntryPoint = "sqlite3_libversion", CallingConvention = CallingConvention.Cdecl)]
		public static extern string Sqlite3_libversion ();

		[DllImport (DllName, EntryPoint = "sqlite3_busy_timeout", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_busy_timeout (IntPtr pDb, Int32 ms);

		// @param pCallback: int (*callback)(void*,int,char**,char**)
		[DllImport (DllName, EntryPoint = "sqlite3_exec", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_exec (IntPtr pDb, String szSql, IntPtr pCallback, Int32 pData, IntPtr errMsg);

		[DllImport (DllName, EntryPoint = "sqlite3_prepare_v2", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_prepare_v2 (IntPtr pDb, String szSql, Int32 nBytes, IntPtr ppStmt, IntPtr pszTail);

		[DllImport (DllName, EntryPoint = "sqlite3_interrupt", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_interrupt (IntPtr pDb);

		[DllImport (DllName, EntryPoint = "sqlite3_changes", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_changes (IntPtr pDb);

		[DllImport (DllName, EntryPoint = "sqlite3_last_insert_rowid", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int64 Sqlite3_last_insert_rowid (IntPtr pDb);

		[DllImport (DllName, EntryPoint = "sqlite3_column_type", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_column_type (IntPtr pStmt, Int32 nCol);

		[DllImport (DllName, EntryPoint = "sqlite3_column_text", CallingConvention = CallingConvention.Cdecl)]
		public static extern String Sqlite3_column_text (IntPtr pStmt, Int32 nCol);

		[DllImport (DllName, EntryPoint = "sqlite3_column_bytes", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_column_bytes (IntPtr pStmt, Int32 nCol);

		[DllImport (DllName, EntryPoint = "sqlite3_column_int", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_column_int (IntPtr pStmt, Int32 nCol);

		[DllImport (DllName, EntryPoint = "sqlite3_column_int64", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int64 Sqlite3_column_int64 (IntPtr pStmt, Int32 nCol);

		[DllImport (DllName, EntryPoint = "sqlite3_column_double", CallingConvention = CallingConvention.Cdecl)]
		public static extern Double Sqlite3_column_double (IntPtr pStmt, Int32 nCol);

		[DllImport (DllName, EntryPoint = "sqlite3_column_blob", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr Sqlite3_column_blob (IntPtr pStmt, Int32 nCol);

		[DllImport (DllName, EntryPoint = "sqlite3_reset", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_reset (IntPtr pStmt);

		[DllImport (DllName, EntryPoint = "sqlite3_step", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_step (IntPtr pStmt);

		[DllImport (DllName, EntryPoint = "sqlite3_finalize", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_finalize (IntPtr pStmt);

		[DllImport (DllName, EntryPoint = "sqlite3_column_name", CallingConvention = CallingConvention.Cdecl)]
		public static extern String Sqlite3_column_name (IntPtr pStmt, Int32 colNumber);

		[DllImport (DllName, EntryPoint = "sqlite3_bind_parameter_count", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_bind_parameter_count (IntPtr pStmt);

		[DllImport (DllName, EntryPoint = "sqlite3_column_count", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_column_count (IntPtr pStmt);

		[DllImport (DllName, EntryPoint = "sqlite3_clear_bindings", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_clear_bindings (IntPtr pStmt);

		[DllImport (DllName, EntryPoint = "sqlite3_bind_int", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_bind_int (IntPtr pStmt, Int32 index, Int32 value);

		[DllImport (DllName, EntryPoint = "sqlite3_bind_int64", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_bind_int64 (IntPtr pStmt, Int32 index, Int64 value);

		[DllImport (DllName, EntryPoint = "sqlite3_bind_double", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_bind_double (IntPtr pStmt, Int32 index, Double value);

		// @param pDestructor: void (*)(void*)
		[DllImport (DllName, EntryPoint = "sqlite3_bind_blob", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_bind_blob (IntPtr pStmt, Int32 index, Byte[] value, Int32 numBytes, IntPtr pDestructor);

		// @param pDestructor: void (*)(void*)
		[DllImport (DllName, EntryPoint = "sqlite3_bind_text", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_bind_text (IntPtr pStmt, Int32 index, String value, Int32 numBytes, IntPtr pDestructor);

		[DllImport (DllName, EntryPoint = "sqlite3_bind_null", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_bind_null (IntPtr pStmt, Int32 index);

		[DllImport (DllName, EntryPoint = "sqlite3_bind_parameter_name", CallingConvention = CallingConvention.Cdecl)]
		public static extern String Sqlite3_bind_parameter_name (IntPtr pStmt, Int32 idnex);

		[DllImport (DllName, EntryPoint = "sqlite3_bind_parameter_index", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_bind_parameter_index (IntPtr pStmt, String szName);

		[DllImport (DllName, EntryPoint = "sqlite3_sql", CallingConvention = CallingConvention.Cdecl)]
		public static extern String Sqlite3_sql (IntPtr pStmt);

		[DllImport (DllName, EntryPoint = "sqlite3_column_decltype", CallingConvention = CallingConvention.Cdecl)]
		public static extern String Sqlite3_column_decltype (IntPtr pStmt, Int32 colNumber);

		[DllImport (DllName, EntryPoint = "sqlite3_get_table", CallingConvention = CallingConvention.Cdecl)]
		public static extern Int32 Sqlite3_get_table (IntPtr pDb, String zSql, IntPtr pazResult, IntPtr pnRow, IntPtr pnColumn, IntPtr pzErrmsg);

		[DllImport (DllName, EntryPoint = "sqlite3_free_table", CallingConvention = CallingConvention.Cdecl)]
		public static extern void Sqlite3_free_table (IntPtr pazResult);
	}
}
