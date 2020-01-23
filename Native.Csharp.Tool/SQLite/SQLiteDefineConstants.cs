/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 *
 * Released to the public domain, use at your own risk!
 ********************************************************/

using System.Collections.Generic;

namespace System.Data.SQLite
{
    internal static class SQLiteDefineConstants
    {
        public static readonly IList<string> OptionList = new List<string>(new string[] {
#if CHECK_STATE
            "CHECK_STATE",
#endif

#if COUNT_HANDLE
            "COUNT_HANDLE",
#endif

#if DEBUG
            "DEBUG",
#endif

#if INTEROP_CODEC
            "INTEROP_CODEC",
#endif

#if INTEROP_DEBUG
            "INTEROP_DEBUG",
#endif

#if INTEROP_EXTENSION_FUNCTIONS
            "INTEROP_EXTENSION_FUNCTIONS",
#endif

#if INTEROP_FTS5_EXTENSION
            "INTEROP_FTS5_EXTENSION",
#endif

#if INTEROP_INCLUDE_CEROD
            "INTEROP_INCLUDE_CEROD",
#endif

#if INTEROP_INCLUDE_EXTRA
            "INTEROP_INCLUDE_EXTRA",
#endif

#if INTEROP_INCLUDE_SEE
            "INTEROP_INCLUDE_SEE",
#endif

#if INTEROP_INCLUDE_ZIPVFS
            "INTEROP_INCLUDE_ZIPVFS",
#endif

#if INTEROP_JSON1_EXTENSION
            "INTEROP_JSON1_EXTENSION",
#endif

#if INTEROP_LEGACY_CLOSE
            "INTEROP_LEGACY_CLOSE",
#endif

#if INTEROP_LOG
            "INTEROP_LOG",
#endif

#if INTEROP_PERCENTILE_EXTENSION
            "INTEROP_PERCENTILE_EXTENSION",
#endif

#if INTEROP_REGEXP_EXTENSION
            "INTEROP_REGEXP_EXTENSION",
#endif

#if INTEROP_SESSION_EXTENSION
            "INTEROP_SESSION_EXTENSION",
#endif

#if INTEROP_SHA1_EXTENSION
            "INTEROP_SHA1_EXTENSION",
#endif

#if INTEROP_TEST_EXTENSION
            "INTEROP_TEST_EXTENSION",
#endif

#if INTEROP_TOTYPE_EXTENSION
            "INTEROP_TOTYPE_EXTENSION",
#endif

#if INTEROP_VIRTUAL_TABLE
            "INTEROP_VIRTUAL_TABLE",
#endif

#if NET_20
            "NET_20",
#endif

#if NET_35
            "NET_35",
#endif

#if NET_40
            "NET_40",
#endif

#if NET_45
            "NET_45",
#endif

#if NET_451
            "NET_451",
#endif

#if NET_452
            "NET_452",
#endif

#if NET_46
            "NET_46",
#endif

#if NET_461
            "NET_461",
#endif

#if NET_462
            "NET_462",
#endif

#if NET_47
            "NET_47",
#endif

#if NET_471
            "NET_471",
#endif

#if NET_472
            "NET_472",
#endif

#if NET_COMPACT_20
            "NET_COMPACT_20",
#endif

#if NET_STANDARD_20
            "NET_STANDARD_20",
#endif

#if PLATFORM_COMPACTFRAMEWORK
            "PLATFORM_COMPACTFRAMEWORK",
#endif

#if PRELOAD_NATIVE_LIBRARY
            "PRELOAD_NATIVE_LIBRARY",
#endif

#if RETARGETABLE
            "RETARGETABLE",
#endif

#if SQLITE_STANDARD
            "SQLITE_STANDARD",
#endif

#if THROW_ON_DISPOSED
            "THROW_ON_DISPOSED",
#endif

#if TRACE
            "TRACE",
#endif

#if TRACE_CONNECTION
            "TRACE_CONNECTION",
#endif

#if TRACE_DETECTION
            "TRACE_DETECTION",
#endif

#if TRACE_HANDLE
            "TRACE_HANDLE",
#endif

#if TRACE_PRELOAD
            "TRACE_PRELOAD",
#endif

#if TRACE_SHARED
            "TRACE_SHARED",
#endif

#if TRACE_STATEMENT
            "TRACE_STATEMENT",
#endif

#if TRACE_WARNING
            "TRACE_WARNING",
#endif

#if TRACK_MEMORY_BYTES
            "TRACK_MEMORY_BYTES",
#endif

#if USE_ENTITY_FRAMEWORK_6
            "USE_ENTITY_FRAMEWORK_6",
#endif

#if USE_INTEROP_DLL
            "USE_INTEROP_DLL",
#endif

#if USE_PREPARE_V2
            "USE_PREPARE_V2",
#endif

#if WINDOWS
            "WINDOWS",
#endif

            null
        });
    }
}
