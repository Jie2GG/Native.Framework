using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Tool.SQLite
{
	[Flags]
	public enum SQLite3OpenFlags
	{
		SQLITE_OPEN_NONE = 0x00000,
		SQLITE_OPEN_READONLY = 0x00001,  /* Ok for sqlite3_open_v2() */
		SQLITE_OPEN_READWRITE = 0x00002,  /* Ok for sqlite3_open_v2() */
		SQLITE_OPEN_CREATE = 0x00004,  /* Ok for sqlite3_open_v2() */
		SQLITE_OPEN_DELETEONCLOSE = 0x00008,  /* VFS only */
		SQLITE_OPEN_EXCLUSIVE = 0x00010,  /* VFS only */
		SQLITE_OPEN_AUTOPROXY = 0x00020,  /* VFS only */
		SQLITE_OPEN_URI = 0x00040,  /* Ok for sqlite3_open_v2() */
		SQLITE_OPEN_MEMORY = 0x00080,  /* Ok for sqlite3_open_v2() */
		SQLITE_OPEN_MAIN_DB = 0x00100,  /* VFS only */
		SQLITE_OPEN_TEMP_DB = 0x00200,  /* VFS only */
		SQLITE_OPEN_TRANSIENT_DB = 0x00400,  /* VFS only */
		SQLITE_OPEN_MAIN_JOURNAL = 0x00800,  /* VFS only */
		SQLITE_OPEN_TEMP_JOURNAL = 0x01000,  /* VFS only */
		SQLITE_OPEN_SUBJOURNAL = 0x02000,  /* VFS only */
		SQLITE_OPEN_MASTER_JOURNAL = 0x04000,  /* VFS only */
		SQLITE_OPEN_NOMUTEX = 0x08000,  /* Ok for sqlite3_open_v2() */
		SQLITE_OPEN_FULLMUTEX = 0x10000,  /* Ok for sqlite3_open_v2() */
		SQLITE_OPEN_SHAREDCACHE = 0x20000,  /* Ok for sqlite3_open_v2() */
		SQLITE_OPEN_PRIVATECACHE = 0x40000,  /* Ok for sqlite3_open_v2() */
		SQLITE_OPEN_WAL = 0x80000  /* VFS only */
	}
}
