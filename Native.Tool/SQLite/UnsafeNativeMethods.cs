/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 *
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
  using System;
  using System.Globalization;

#if TRACE_DETECTION || TRACE_SHARED || TRACE_PRELOAD || TRACE_HANDLE
  using System.Diagnostics;
#endif

  using System.Collections.Generic;
  using System.IO;
  using System.Reflection;

#if !PLATFORM_COMPACTFRAMEWORK
  using System.Security;
#endif

  using System.Runtime.InteropServices;

#if (NET_40 || NET_45 || NET_451 || NET_452 || NET_46 || NET_461 || NET_462 || NET_47 || NET_471 || NET_472 || NET_STANDARD_20 || NET_STANDARD_21) && !PLATFORM_COMPACTFRAMEWORK
  using System.Runtime.Versioning;
#endif

  using System.Text;

#if !PLATFORM_COMPACTFRAMEWORK || COUNT_HANDLE
  using System.Threading;
#endif

  using System.Xml;

  #region Debug Data Static Class
#if COUNT_HANDLE || DEBUG
  /// <summary>
  /// This class encapsulates some tracking data that is used for debugging
  /// and testing purposes.
  /// </summary>
  internal static class DebugData
  {
      #region Private Data
#if DEBUG
      /// <summary>
      /// This lock is used to protect several static fields.
      /// </summary>
      private static readonly object staticSyncRoot = new object();
#endif

      /////////////////////////////////////////////////////////////////////////

      #region Critical Handle Counts (Debug Build Only)
#if COUNT_HANDLE
      //
      // NOTE: These counts represent the total number of outstanding
      //       (non-disposed) CriticalHandle derived object instances
      //       created by this library and are primarily for use by
      //       the test suite.  These counts are incremented by the
      //       associated constructors and are decremented upon the
      //       successful completion of the associated ReleaseHandle
      //       methods.
      //
      internal static int connectionCount;
      internal static int statementCount;
      internal static int backupCount;
      internal static int blobCount;
#endif
      #endregion

      /////////////////////////////////////////////////////////////////////////

      #region Settings Read Counts (Debug Build Only)
#if DEBUG
      /// <summary>
      /// This dictionary stores the read counts for the runtime configuration
      /// settings.  This information is only recorded when compiled in the
      /// "Debug" build configuration.
      /// </summary>
      private static Dictionary<string, int> settingReadCounts;

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// This dictionary stores the read counts for the runtime configuration
      /// settings via the XML configuration file.  This information is only
      /// recorded when compiled in the "Debug" build configuration.
      /// </summary>
      private static Dictionary<string, int> settingFileReadCounts;
#endif
      #endregion

      /////////////////////////////////////////////////////////////////////////

      #region Other Counts (Debug Build Only)
#if DEBUG
      /// <summary>
      /// This dictionary stores miscellaneous counts used for debugging
      /// purposes.  This information is only recorded when compiled in the
      /// "Debug" build configuration.
      /// </summary>
      private static Dictionary<string, int> otherCounts;
#endif
      #endregion
      #endregion

      /////////////////////////////////////////////////////////////////////////

      #region Public Methods
#if DEBUG
      /// <summary>
      /// Creates dictionaries used to store the read counts for each of
      /// the runtime configuration settings.  These numbers are used for
      /// debugging and testing purposes only.
      /// </summary>
      public static void Initialize()
      {
          lock (staticSyncRoot)
          {
              //
              // NOTE: Create the dictionaries of statistics that will
              //       contain the number of times each setting value
              //       has been read.
              //
              if (settingReadCounts == null)
                  settingReadCounts = new Dictionary<string, int>();

              if (settingFileReadCounts == null)
                  settingFileReadCounts = new Dictionary<string, int>();

              if (otherCounts == null)
                  otherCounts = new Dictionary<string, int>();
          }
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// Queries the read counts for the runtime configuration settings.
      /// These numbers are used for debugging and testing purposes only.
      /// </summary>
      /// <param name="viaFile">
      /// Non-zero if the specified settings were read from the XML
      /// configuration file.
      /// </param>
      /// <returns>
      /// A copy of the statistics for the specified runtime configuration
      /// settings -OR- null if they are not available.
      /// </returns>
      public static object GetSettingReadCounts(
          bool viaFile
          )
      {
          lock (staticSyncRoot)
          {
              if (viaFile)
              {
                  if (settingFileReadCounts == null)
                      return null;

                  return new Dictionary<string, int>(settingFileReadCounts);
              }
              else
              {
                  if (settingReadCounts == null)
                      return null;

                  return new Dictionary<string, int>(settingReadCounts);
              }
          }
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// Clears the read counts for the runtime configuration settings.
      /// These numbers are used for debugging and testing purposes only.
      /// </summary>
      /// <param name="viaFile">
      /// Non-zero if the specified settings were read from the XML
      /// configuration file.
      /// </param>
      public static void ClearSettingReadCounts(
          bool viaFile
          )
      {
          lock (staticSyncRoot)
          {
              if (viaFile)
              {
                  if (settingFileReadCounts != null)
                      settingFileReadCounts.Clear();
              }
              else
              {
                  if (settingReadCounts != null)
                      settingReadCounts.Clear();
              }
          }
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// Increments the read count for the specified runtime configuration
      /// setting.  These numbers are used for debugging and testing purposes
      /// only.
      /// </summary>
      /// <param name="name">
      /// The name of the setting being read.
      /// </param>
      /// <param name="viaFile">
      /// Non-zero if the specified setting is being read from the XML
      /// configuration file.
      /// </param>
      public static void IncrementSettingReadCount(
          string name,
          bool viaFile
          )
      {
          lock (staticSyncRoot)
          {
              //
              // NOTE: Update statistics for this setting value.
              //
              if (viaFile)
              {
                  if (settingFileReadCounts != null)
                  {
                      int count;

                      if (settingFileReadCounts.TryGetValue(name, out count))
                          settingFileReadCounts[name] = count + 1;
                      else
                          settingFileReadCounts.Add(name, 1);
                  }
              }
              else
              {
                  if (settingReadCounts != null)
                  {
                      int count;

                      if (settingReadCounts.TryGetValue(name, out count))
                          settingReadCounts[name] = count + 1;
                      else
                          settingReadCounts.Add(name, 1);
                  }
              }
          }
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// Queries the counters.  These numbers are used for debugging and
      /// testing purposes only.
      /// </summary>
      /// <returns>
      /// A copy of the counters -OR- null if they are not available.
      /// </returns>
      public static object GetOtherCounts()
      {
          lock (staticSyncRoot)
          {
              if (otherCounts == null)
                  return null;

              return new Dictionary<string, int>(otherCounts);
          }
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// Clears the counters.  These numbers are used for debugging and
      /// testing purposes only.
      /// </summary>
      public static void ClearOtherCounts()
      {
          lock (staticSyncRoot)
          {
              if (otherCounts != null)
                  otherCounts.Clear();
          }
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// Increments the specified counter.
      /// </summary>
      /// <param name="name">
      /// The name of the counter being incremented.
      /// </param>
      public static void IncrementOtherCount(
          string name
          )
      {
          lock (staticSyncRoot)
          {
              if (otherCounts != null)
              {
                  int count;

                  if (otherCounts.TryGetValue(name, out count))
                      otherCounts[name] = count + 1;
                  else
                      otherCounts.Add(name, 1);
              }
          }
      }
#endif
      #endregion
  }
#endif
  #endregion

  /////////////////////////////////////////////////////////////////////////////

  #region Helper Methods Static Class
  /// <summary>
  /// This static class provides some methods that are shared between the
  /// native library pre-loader and other classes.
  /// </summary>
  internal static class HelperMethods
  {
      #region Private Constants
      private const string DisplayNullObject = "<nullObject>";
      private const string DisplayEmptyString = "<emptyString>";
      private const string DisplayStringFormat = "\"{0}\"";

      /////////////////////////////////////////////////////////////////////////

      private const string DisplayNullArray = "<nullArray>";
      private const string DisplayEmptyArray = "<emptyArray>";

      /////////////////////////////////////////////////////////////////////////

      private const char ArrayOpen = '[';
      private const string ElementSeparator = ", ";
      private const char ArrayClose = ']';

      /////////////////////////////////////////////////////////////////////////

      private static readonly char[] SpaceChars = {
          '\t', '\n', '\r', '\v', '\f', ' '
      };
      #endregion

      /////////////////////////////////////////////////////////////////////////

      #region Private Data
      /// <summary>
      /// This lock is used to protect the static <see cref="isMono" /> and
      /// <see cref="isDotNetCore" /> fields.
      /// </summary>
      private static readonly object staticSyncRoot = new object();

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// This type is only present when running on Mono.
      /// </summary>
      private static readonly string MonoRuntimeType = "Mono.Runtime";

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// This type is only present when running on .NET Core.
      /// </summary>
      private static readonly string DotNetCoreLibType = "System.CoreLib";

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Keeps track of whether we are running on Mono.  Initially null, it is
      /// set by the <see cref="IsMono" /> method on its first call.  Later, it
      /// is returned verbatim by the <see cref="IsMono" /> method.
      /// </summary>
      private static bool? isMono = null;

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Keeps track of whether we are running on .NET Core.  Initially null,
      /// it is set by the <see cref="IsDotNetCore" /> method on its first
      /// call.  Later, it is returned verbatim by the
      /// <see cref="IsDotNetCore" /> method.
      /// </summary>
      private static bool? isDotNetCore = null;

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Keeps track of whether we successfully invoked the
      /// <see cref="Debugger.Break" /> method.  Initially null, it is set by
      /// the <see cref="MaybeBreakIntoDebugger" /> method on its first call.
      /// </summary>
      private static bool? debuggerBreak = null;
      #endregion

      /////////////////////////////////////////////////////////////////////////

      #region Private Methods
      /// <summary>
      /// Determines the ID of the current process.  Only used for debugging.
      /// </summary>
      /// <returns>
      /// The ID of the current process -OR- zero if it cannot be determined.
      /// </returns>
      private static int GetProcessId()
      {
          Process process = Process.GetCurrentProcess();

          if (process == null)
              return 0;

          return process.Id;
      }

      ///////////////////////////////////////////////////////////////////////

      /// <summary>
      /// Determines whether or not this assembly is running on Mono.
      /// </summary>
      /// <returns>
      /// Non-zero if this assembly is running on Mono.
      /// </returns>
      private static bool IsMono()
      {
          try
          {
              lock (staticSyncRoot)
              {
                  if (isMono == null)
                      isMono = (Type.GetType(MonoRuntimeType) != null);

                  return (bool)isMono;
              }
          }
          catch
          {
              // do nothing.
          }

          return false;
      }

      ///////////////////////////////////////////////////////////////////////

      /// <summary>
      /// Determines whether or not this assembly is running on .NET Core.
      /// </summary>
      /// <returns>
      /// Non-zero if this assembly is running on .NET Core.
      /// </returns>
      public static bool IsDotNetCore()
      {
          try
          {
              lock (staticSyncRoot)
              {
                  if (isDotNetCore == null)
                  {
                      isDotNetCore = (Type.GetType(
                          DotNetCoreLibType) != null);
                  }

                  return (bool)isDotNetCore;
              }
          }
          catch
          {
              // do nothing.
          }

          return false;
      }
      #endregion

      /////////////////////////////////////////////////////////////////////////

      #region Internal Methods
      /// <summary>
      /// Resets the cached value for the "PreLoadSQLite_BreakIntoDebugger"
      /// configuration setting.
      /// </summary>
      internal static void ResetBreakIntoDebugger()
      {
          lock (staticSyncRoot)
          {
              debuggerBreak = null;
          }
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// If the "PreLoadSQLite_BreakIntoDebugger" configuration setting is
      /// present (e.g. via the environment), give the interactive user an
      /// opportunity to attach a debugger to the current process; otherwise,
      /// do nothing.
      /// </summary>
      internal static void MaybeBreakIntoDebugger()
      {
          lock (staticSyncRoot)
          {
              if (debuggerBreak != null)
                  return;
          }

          if (UnsafeNativeMethods.GetSettingValue(
                "PreLoadSQLite_BreakIntoDebugger", null) != null)
          {
              //
              // NOTE: Attempt to use the Console in order to prompt the
              //       interactive user (if any).  This may fail for any
              //       number of reasons.  Even in those cases, we still
              //       want to issue the actual request to break into the
              //       debugger.
              //
              try
              {
                  Console.WriteLine(StringFormat(
                      CultureInfo.CurrentCulture,
                      "Attach a debugger to process {0} " +
                      "and press any key to continue.",
                      GetProcessId()));

#if PLATFORM_COMPACTFRAMEWORK
                  Console.ReadLine();
#else
                  Console.ReadKey();
#endif
              }
#if !NET_COMPACT_20 && TRACE_SHARED
              catch (Exception e)
#else
              catch (Exception)
#endif
              {
#if !NET_COMPACT_20 && TRACE_SHARED
                  try
                  {
                      Trace.WriteLine(HelperMethods.StringFormat(
                          CultureInfo.CurrentCulture,
                          "Failed to issue debugger prompt, " +
                          "{0} may be unusable: {1}",
                          typeof(Console), e)); /* throw */
                  }
                  catch
                  {
                      // do nothing.
                  }
#endif
              }

              try
              {
                  Debugger.Break();

                  lock (staticSyncRoot)
                  {
                      debuggerBreak = true;
                  }
              }
              catch
              {
                  lock (staticSyncRoot)
                  {
                      debuggerBreak = false;
                  }

                  throw;
              }
          }
          else
          {
              //
              // BUGFIX: There is (almost) no point in checking for the
              //         associated configuration setting repeatedly.
              //         Prevent that here by setting the cached value
              //         to false.
              //
              lock (staticSyncRoot)
              {
                  debuggerBreak = false;
              }
          }
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// Determines the ID of the current thread.  Only used for debugging.
      /// </summary>
      /// <returns>
      /// The ID of the current thread -OR- zero if it cannot be determined.
      /// </returns>
      internal static int GetThreadId()
      {
#if !PLATFORM_COMPACTFRAMEWORK
          return AppDomain.GetCurrentThreadId();
#else
          return 0;
#endif
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// Determines if the specified flags are present within the flags
      /// associated with the parent connection object.
      /// </summary>
      /// <param name="flags">
      /// The flags associated with the parent connection object.
      /// </param>
      /// <param name="hasFlags">
      /// The flags to check for.
      /// </param>
      /// <returns>
      /// Non-zero if the specified flag or flags were present; otherwise,
      /// zero.
      /// </returns>
      internal static bool HasFlags(
          SQLiteConnectionFlags flags,
          SQLiteConnectionFlags hasFlags
          )
      {
          return ((flags & hasFlags) == hasFlags);
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// Determines if preparing a query should be logged.
      /// </summary>
      /// <param name="flags">
      /// The flags associated with the parent connection object.
      /// </param>
      /// <returns>
      /// Non-zero if the query preparation should be logged; otherwise, zero.
      /// </returns>
      internal static bool LogPrepare(
          SQLiteConnectionFlags flags
          )
      {
          return HasFlags(flags, SQLiteConnectionFlags.LogPrepare);
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Determines if pre-parameter binding should be logged.
      /// </summary>
      /// <param name="flags">
      /// The flags associated with the parent connection object.
      /// </param>
      /// <returns>
      /// Non-zero if the pre-parameter binding should be logged; otherwise,
      /// zero.
      /// </returns>
      internal static bool LogPreBind(
          SQLiteConnectionFlags flags
          )
      {
          return HasFlags(flags, SQLiteConnectionFlags.LogPreBind);
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Determines if parameter binding should be logged.
      /// </summary>
      /// <param name="flags">
      /// The flags associated with the parent connection object.
      /// </param>
      /// <returns>
      /// Non-zero if the parameter binding should be logged; otherwise, zero.
      /// </returns>
      internal static bool LogBind(
          SQLiteConnectionFlags flags
          )
      {
          return HasFlags(flags, SQLiteConnectionFlags.LogBind);
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Determines if an exception in a native callback should be logged.
      /// </summary>
      /// <param name="flags">
      /// The flags associated with the parent connection object.
      /// </param>
      /// <returns>
      /// Non-zero if the exception should be logged; otherwise, zero.
      /// </returns>
      internal static bool LogCallbackExceptions(
          SQLiteConnectionFlags flags
          )
      {
          return HasFlags(flags, SQLiteConnectionFlags.LogCallbackException);
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Determines if backup API errors should be logged.
      /// </summary>
      /// <param name="flags">
      /// The flags associated with the parent connection object.
      /// </param>
      /// <returns>
      /// Non-zero if the backup API error should be logged; otherwise, zero.
      /// </returns>
      internal static bool LogBackup(
          SQLiteConnectionFlags flags
          )
      {
          return HasFlags(flags, SQLiteConnectionFlags.LogBackup);
      }

#if INTEROP_VIRTUAL_TABLE
      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Determines if logging for the <see cref="SQLiteModule" /> class is
      /// disabled.
      /// </summary>
      /// <param name="flags">
      /// The flags associated with the parent connection object.
      /// </param>
      /// <returns>
      /// Non-zero if logging for the <see cref="SQLiteModule" /> class is
      /// disabled; otherwise, zero.
      /// </returns>
      internal static bool NoLogModule(
          SQLiteConnectionFlags flags
          )
      {
          return HasFlags(flags, SQLiteConnectionFlags.NoLogModule);
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Determines if <see cref="SQLiteModule" /> errors should be logged.
      /// </summary>
      /// <param name="flags">
      /// The flags associated with the parent connection object.
      /// </param>
      /// <returns>
      /// Non-zero if the <see cref="SQLiteModule" /> error should be logged;
      /// otherwise, zero.
      /// </returns>
      internal static bool LogModuleError(
          SQLiteConnectionFlags flags
          )
      {
          return HasFlags(flags, SQLiteConnectionFlags.LogModuleError);
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Determines if <see cref="SQLiteModule" /> exceptions should be
      /// logged.
      /// </summary>
      /// <param name="flags">
      /// The flags associated with the parent connection object.
      /// </param>
      /// <returns>
      /// Non-zero if the <see cref="SQLiteModule" /> exception should be
      /// logged; otherwise, zero.
      /// </returns>
      internal static bool LogModuleException(
          SQLiteConnectionFlags flags
          )
      {
          return HasFlags(flags, SQLiteConnectionFlags.LogModuleException);
      }
#endif

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Determines if the current process is running on one of the Windows
      /// [sub-]platforms.
      /// </summary>
      /// <returns>
      /// Non-zero when running on Windows; otherwise, zero.
      /// </returns>
      internal static bool IsWindows()
      {
          PlatformID platformId = Environment.OSVersion.Platform;

          if ((platformId == PlatformID.Win32S) ||
              (platformId == PlatformID.Win32Windows) ||
              (platformId == PlatformID.Win32NT) ||
              (platformId == PlatformID.WinCE))
          {
              return true;
          }

          return false;
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// This is a wrapper around the
      /// <see cref="String.Format(IFormatProvider,String,Object[])" /> method.
      /// On Mono, it has to call the method overload without the
      /// <see cref="IFormatProvider" /> parameter, due to a bug in Mono.
      /// </summary>
      /// <param name="provider">
      /// This is used for culture-specific formatting.
      /// </param>
      /// <param name="format">
      /// The format string.
      /// </param>
      /// <param name="args">
      /// An array the objects to format.
      /// </param>
      /// <returns>
      /// The resulting string.
      /// </returns>
      internal static string StringFormat(
          IFormatProvider provider,
          string format,
          params object[] args
          )
      {
          if (IsMono())
              return String.Format(format, args);
          else
              return String.Format(provider, format, args);
      }
      #endregion

      /////////////////////////////////////////////////////////////////////////

      #region Public Methods
      public static string ToDisplayString(
          object value
          )
      {
          if (value == null)
              return DisplayNullObject;

          string stringValue = value.ToString();

          if (stringValue.Length == 0)
              return DisplayEmptyString;

          if (stringValue.IndexOfAny(SpaceChars) < 0)
              return stringValue;

          return HelperMethods.StringFormat(
              CultureInfo.InvariantCulture, DisplayStringFormat,
              stringValue);
      }

      /////////////////////////////////////////////////////////////////////////

      public static string ToDisplayString(
          Array array
          )
      {
          if (array == null)
              return DisplayNullArray;

          if (array.Length == 0)
              return DisplayEmptyArray;

          StringBuilder result = new StringBuilder();

          foreach (object value in array)
          {
              if (result.Length > 0)
                  result.Append(ElementSeparator);

              result.Append(ToDisplayString(value));
          }

          if (result.Length > 0)
          {
#if PLATFORM_COMPACTFRAMEWORK
              result.Insert(0, ArrayOpen.ToString());
#else
              result.Insert(0, ArrayOpen);
#endif

              result.Append(ArrayClose);
          }

          return result.ToString();
      }
      #endregion
  }
  #endregion

  /////////////////////////////////////////////////////////////////////////////

  #region Native Library Helper Class
  /// <summary>
  /// This static class provides a thin wrapper around the native library
  /// loading features of the underlying platform.
  /// </summary>
  internal static class NativeLibraryHelper
  {
      #region Private Delegates
      /// <summary>
      /// This delegate is used to wrap the concept of loading a native
      /// library, based on a file name, and returning the loaded module
      /// handle.
      /// </summary>
      /// <param name="fileName">
      /// The file name of the native library to load.
      /// </param>
      /// <returns>
      /// The native module handle upon success -OR- IntPtr.Zero on failure.
      /// </returns>
      private delegate IntPtr LoadLibraryCallback(
          string fileName
      );

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// This delegate is used to wrap the concept of querying the machine
      /// name of the current process.
      /// </summary>
      /// <returns>
      /// The machine name for the current process -OR- null on failure.
      /// </returns>
      private delegate string GetMachineCallback();
      #endregion

      /////////////////////////////////////////////////////////////////////////

      #region Private Methods
      /// <summary>
      /// Attempts to load the specified native library file using the Win32
      /// API.
      /// </summary>
      /// <param name="fileName">
      /// The file name of the native library to load.
      /// </param>
      /// <returns>
      /// The native module handle upon success -OR- IntPtr.Zero on failure.
      /// </returns>
      private static IntPtr LoadLibraryWin32(
          string fileName
          )
      {
          return UnsafeNativeMethodsWin32.LoadLibrary(fileName);
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// Attempts to determine the machine name of the current process using
      /// the Win32 API.
      /// </summary>
      /// <returns>
      /// The machine name for the current process -OR- null on failure.
      /// </returns>
      private static string GetMachineWin32()
      {
          //
          // NOTE: When running on Windows, attempt to use the native Win32
          //       API function (via P/Invoke) that can provide us with the
          //       processor architecture.
          //
          try
          {
              UnsafeNativeMethodsWin32.SYSTEM_INFO systemInfo;

              //
              // NOTE: Query the system information via P/Invoke, thus
              //       filling the structure.
              //
              UnsafeNativeMethodsWin32.GetSystemInfo(out systemInfo);

              //
              // NOTE: Return the processor architecture value as a string.
              //
              return systemInfo.wProcessorArchitecture.ToString();
          }
          catch
          {
              // do nothing.
          }

          return null;
      }

      /////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
      /// <summary>
      /// Attempts to load the specified native library file using the POSIX
      /// API.
      /// </summary>
      /// <param name="fileName">
      /// The file name of the native library to load.
      /// </param>
      /// <returns>
      /// The native module handle upon success -OR- IntPtr.Zero on failure.
      /// </returns>
      private static IntPtr LoadLibraryPosix(
          string fileName
          )
      {
          return UnsafeNativeMethodsPosix.dlopen(
              fileName, UnsafeNativeMethodsPosix.RTLD_DEFAULT);
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// Attempts to determine the machine name of the current process using
      /// the POSIX API.
      /// </summary>
      /// <returns>
      /// The machine name for the current process -OR- null on failure.
      /// </returns>
      private static string GetMachinePosix()
      {
          //
          // NOTE: When running on POSIX (non-Windows), attempt to query the
          //       machine from the operating system via uname().
          //
          try
          {
              UnsafeNativeMethodsPosix.utsname utsName = null;

              if (UnsafeNativeMethodsPosix.GetOsVersionInfo(ref utsName) &&
                  (utsName != null))
              {
                  return utsName.machine;
              }
          }
          catch
          {
              // do nothing.
          }

          return null;
      }
#endif
      #endregion

      /////////////////////////////////////////////////////////////////////////

      #region Public Methods
      /// <summary>
      /// Attempts to load the specified native library file.
      /// </summary>
      /// <param name="fileName">
      /// The file name of the native library to load.
      /// </param>
      /// <returns>
      /// The native module handle upon success -OR- IntPtr.Zero on failure.
      /// </returns>
      public static IntPtr LoadLibrary(
          string fileName
          )
      {
          LoadLibraryCallback callback = LoadLibraryWin32;

#if !PLATFORM_COMPACTFRAMEWORK
          if (!HelperMethods.IsWindows())
              callback = LoadLibraryPosix;
#endif

          return callback(fileName);
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// Attempts to determine the machine name of the current process.
      /// </summary>
      /// <returns>
      /// The machine name for the current process -OR- null on failure.
      /// </returns>
      public static string GetMachine()
      {
          GetMachineCallback callback = GetMachineWin32;

#if !PLATFORM_COMPACTFRAMEWORK
          if (!HelperMethods.IsWindows())
              callback = GetMachinePosix;
#endif

          return callback();
      }
      #endregion
  }
  #endregion

  /////////////////////////////////////////////////////////////////////////////

  #region Unmanaged Interop Methods Static Class (POSIX)
#if !PLATFORM_COMPACTFRAMEWORK
  /// <summary>
  /// This class declares P/Invoke methods to call native POSIX APIs.
  /// </summary>
  [SuppressUnmanagedCodeSecurity]
  internal static class UnsafeNativeMethodsPosix
  {
      /// <summary>
      /// This structure is used when running on POSIX operating systems
      /// to store information about the current machine, including the
      /// human readable name of the operating system as well as that of
      /// the underlying hardware.
      /// </summary>
      internal sealed class utsname
      {
          public string sysname;  /* Name of this implementation of
                                   * the operating system. */
          public string nodename; /* Name of this node within the
                                   * communications network to which
                                   * this node is attached, if any. */
          public string release;  /* Current release level of this
                                   * implementation. */
          public string version;  /* Current version level of this
                                   * release. */
          public string machine;  /* Name of the hardware type on
                                   * which the system is running. */
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// This structure is passed directly to the P/Invoke method to
      /// obtain the information about the current machine, including
      /// the human readable name of the operating system as well as
      /// that of the underlying hardware.
      /// </summary>
      [StructLayout(LayoutKind.Sequential)]
      private struct utsname_interop
      {
          //
          // NOTE: The following string fields should be present in
          //       this buffer, all of which will be zero-terminated:
          //
          //                      sysname
          //                      nodename
          //                      release
          //                      version
          //                      machine
          //
          [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)]
          public byte[] buffer;
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// This is the P/Invoke method that wraps the native Unix uname
      /// function.  See the POSIX documentation for full details on what it
      /// does.
      /// </summary>
      /// <param name="name">
      /// Structure containing a preallocated byte buffer to fill with the
      /// requested information.
      /// </param>
      /// <returns>
      /// Zero for success and less than zero upon failure.
      /// </returns>
#if NET_STANDARD_20 || NET_STANDARD_21
      [DllImport("libc",
#else
      [DllImport("__Internal",
#endif
          CallingConvention = CallingConvention.Cdecl)]
      private static extern int uname(out utsname_interop name);

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// This is the P/Invoke method that wraps the native Unix dlopen
      /// function.  See the POSIX documentation for full details on what it
      /// does.
      /// </summary>
      /// <param name="fileName">
      /// The name of the executable library.
      /// </param>
      /// <param name="mode">
      /// This must be a combination of the individual bit flags RTLD_LAZY,
      /// RTLD_NOW, RTLD_GLOBAL, and/or RTLD_LOCAL.
      /// </param>
      /// <returns>
      /// The native module handle upon success -OR- IntPtr.Zero on failure.
      /// </returns>
#if NET_STANDARD_20 || NET_STANDARD_21
      [DllImport("libdl",
#else
      [DllImport("__Internal",
#endif
          EntryPoint = "dlopen",
          CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi,
          BestFitMapping = false, ThrowOnUnmappableChar = true,
          SetLastError = true)]
      internal static extern IntPtr dlopen(string fileName, int mode);

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// This is the P/Invoke method that wraps the native Unix dlclose
      /// function.  See the POSIX documentation for full details on what it
      /// does.
      /// </summary>
      /// <param name="module">
      /// The handle to the loaded native library.
      /// </param>
      /// <returns>
      /// Zero upon success -OR- non-zero on failure.
      /// </returns>
#if NET_STANDARD_20 || NET_STANDARD_21
      [DllImport("libdl",
#else
      [DllImport("__Internal",
#endif
          EntryPoint = "dlclose",
          CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
      internal static extern int dlclose(IntPtr module);

      /////////////////////////////////////////////////////////////////////////

      #region Private Constants
      /// <summary>
      /// For use with dlopen(), bind function calls lazily.
      /// </summary>
      internal const int RTLD_LAZY = 0x1;

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// For use with dlopen(), bind function calls immediately.
      /// </summary>
      internal const int RTLD_NOW = 0x2;

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// For use with dlopen(), make symbols globally available.
      /// </summary>
      internal const int RTLD_GLOBAL = 0x100;

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// For use with dlopen(), opposite of RTLD_GLOBAL, and the default.
      /// </summary>
      internal const int RTLD_LOCAL = 0x000;

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// For use with dlopen(), the defaults used by this class.
      /// </summary>
      internal const int RTLD_DEFAULT = RTLD_NOW | RTLD_GLOBAL;
      #endregion

      /////////////////////////////////////////////////////////////////////////

      #region Private Data
      /// <summary>
      /// These are the characters used to separate the string fields within
      /// the raw buffer returned by the <see cref="uname" /> P/Invoke method.
      /// </summary>
      private static readonly char[] utsNameSeparators = { '\0' };
      #endregion

      /////////////////////////////////////////////////////////////////////////

      #region Private Methods
      /// <summary>
      /// This method is a wrapper around the <see cref="uname" /> P/Invoke
      /// method that extracts and returns the human readable strings from
      /// the raw buffer.
      /// </summary>
      /// <param name="utsName">
      /// This structure, which contains strings, will be filled based on the
      /// data placed in the raw buffer returned by the <see cref="uname" />
      /// P/Invoke method.
      /// </param>
      /// <returns>
      /// Non-zero upon success; otherwise, zero.
      /// </returns>
      internal static bool GetOsVersionInfo(
          ref utsname utsName
          )
      {
          try
          {
              utsname_interop utfNameInterop;

              if (uname(out utfNameInterop) < 0)
                  return false;

              if (utfNameInterop.buffer == null)
                  return false;

              string bufferAsString = Encoding.UTF8.GetString(
                  utfNameInterop.buffer);

              if ((bufferAsString == null) || (utsNameSeparators == null))
                  return false;

              bufferAsString = bufferAsString.Trim(utsNameSeparators);

              string[] parts = bufferAsString.Split(
                  utsNameSeparators, StringSplitOptions.RemoveEmptyEntries);

              if (parts == null)
                  return false;

              utsname localUtsName = new utsname();

              if (parts.Length >= 1)
                  localUtsName.sysname = parts[0];

              if (parts.Length >= 2)
                  localUtsName.nodename = parts[1];

              if (parts.Length >= 3)
                  localUtsName.release = parts[2];

              if (parts.Length >= 4)
                  localUtsName.version = parts[3];

              if (parts.Length >= 5)
                  localUtsName.machine = parts[4];

              utsName = localUtsName;
              return true;
          }
          catch
          {
              // do nothing.
          }

          return false;
      }
      #endregion
  }
#endif
  #endregion

  /////////////////////////////////////////////////////////////////////////////

  #region Unmanaged Interop Methods Static Class (Win32)
  /// <summary>
  /// This class declares P/Invoke methods to call native Win32 APIs.
  /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
  [SuppressUnmanagedCodeSecurity]
#endif
  internal static class UnsafeNativeMethodsWin32
  {
      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// This is the P/Invoke method that wraps the native Win32 LoadLibrary
      /// function.  See the MSDN documentation for full details on what it
      /// does.
      /// </summary>
      /// <param name="fileName">
      /// The name of the executable library.
      /// </param>
      /// <returns>
      /// The native module handle upon success -OR- IntPtr.Zero on failure.
      /// </returns>
#if !PLATFORM_COMPACTFRAMEWORK
      [DllImport("kernel32",
#else
      [DllImport("coredll",
#endif
          CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto,
#if !PLATFORM_COMPACTFRAMEWORK
          BestFitMapping = false, ThrowOnUnmappableChar = true,
#endif
          SetLastError = true)]
      internal static extern IntPtr LoadLibrary(string fileName);

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// This is the P/Invoke method that wraps the native Win32 GetSystemInfo
      /// function.  See the MSDN documentation for full details on what it
      /// does.
      /// </summary>
      /// <param name="systemInfo">
      /// The system information structure to be filled in by the function.
      /// </param>
#if !PLATFORM_COMPACTFRAMEWORK
      [DllImport("kernel32",
#else
      [DllImport("coredll",
#endif
          CallingConvention = CallingConvention.Winapi)]
      internal static extern void GetSystemInfo(out SYSTEM_INFO systemInfo);

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// This enumeration contains the possible values for the processor
      /// architecture field of the system information structure.
      /// </summary>
      internal enum ProcessorArchitecture : ushort /* COMPAT: Win32. */
      {
          Intel = 0,
          MIPS = 1,
          Alpha = 2,
          PowerPC = 3,
          SHx = 4,
          ARM = 5,
          IA64 = 6,
          Alpha64 = 7,
          MSIL = 8,
          AMD64 = 9,
          IA32_on_Win64 = 10,
          Unknown = 0xFFFF
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// This structure contains information about the current computer. This
      /// includes the processor type, page size, memory addresses, etc.
      /// </summary>
      [StructLayout(LayoutKind.Sequential)]
      internal struct SYSTEM_INFO
      {
          public ProcessorArchitecture wProcessorArchitecture;
          public ushort wReserved; /* NOT USED */
          public uint dwPageSize; /* NOT USED */
          public IntPtr lpMinimumApplicationAddress; /* NOT USED */
          public IntPtr lpMaximumApplicationAddress; /* NOT USED */
#if PLATFORM_COMPACTFRAMEWORK
          public uint dwActiveProcessorMask; /* NOT USED */
#else
          public IntPtr dwActiveProcessorMask; /* NOT USED */
#endif
          public uint dwNumberOfProcessors; /* NOT USED */
          public uint dwProcessorType; /* NOT USED */
          public uint dwAllocationGranularity; /* NOT USED */
          public ushort wProcessorLevel; /* NOT USED */
          public ushort wProcessorRevision; /* NOT USED */
      }
  }
  #endregion

  /////////////////////////////////////////////////////////////////////////////

  #region Unmanaged Interop Methods Static Class (SQLite)
  /// <summary>
  /// This class declares P/Invoke methods to call native SQLite APIs.
  /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
  [SuppressUnmanagedCodeSecurity]
#endif
  internal static class UnsafeNativeMethods
  {
      public const string ExceptionMessageFormat =
          "Caught exception in \"{0}\" method: {1}";

      /////////////////////////////////////////////////////////////////////////

      #region Shared Native SQLite Library Pre-Loading Code
      #region Private Constants
      /// <summary>
      /// The file extension used for dynamic link libraries.
      /// </summary>
      private static readonly string DllFileExtension = ".dll";

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// The file extension used for the XML configuration file.
      /// </summary>
      private static readonly string ConfigFileExtension = ".config";

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// This is the name of the XML configuration file specific to the
      /// System.Data.SQLite assembly.
      /// </summary>
      private static readonly string XmlConfigFileName =
          typeof(UnsafeNativeMethods).Namespace + DllFileExtension +
          ConfigFileExtension;

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// This is the XML configuratrion file token that will be replaced with
      /// the qualified path to the directory containing the XML configuration
      /// file.
      /// </summary>
      private static readonly string XmlConfigDirectoryToken =
          "%PreLoadSQLite_XmlConfigDirectory%";
      #endregion

      /////////////////////////////////////////////////////////////////////////

      #region Private Constants (Desktop Framework Only)
#if !PLATFORM_COMPACTFRAMEWORK
      /// <summary>
      /// This is the environment variable token that will be replaced with
      /// the qualified path to the directory containing this assembly.
      /// </summary>
      private static readonly string AssemblyDirectoryToken =
          "%PreLoadSQLite_AssemblyDirectory%";

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// This is the environment variable token that will be replaced with an
      /// abbreviation of the target framework attribute value associated with
      /// this assembly.
      /// </summary>
      private static readonly string TargetFrameworkToken =
          "%PreLoadSQLite_TargetFramework%";
#endif
      #endregion

      /////////////////////////////////////////////////////////////////////////

      #region Private Data
      /// <summary>
      /// This lock is used to protect the static _SQLiteNativeModuleFileName,
      /// _SQLiteNativeModuleHandle, and processorArchitecturePlatforms fields.
      /// </summary>
      private static readonly object staticSyncRoot = new object();

      /////////////////////////////////////////////////////////////////////////
#if !PLATFORM_COMPACTFRAMEWORK
      /// <summary>
      /// This dictionary stores the mappings between target framework names
      /// and their associated (NuGet) abbreviations.  These mappings are only
      /// used by the <see cref="AbbreviateTargetFramework" /> method.
      /// </summary>
      private static Dictionary<string, string> targetFrameworkAbbreviations;
#endif

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// This dictionary stores the mappings between processor architecture
      /// names and platform names.  These mappings are now used for two
      /// purposes.  First, they are used to determine if the assembly code
      /// base should be used instead of the location, based upon whether one
      /// or more of the named sub-directories exist within the assembly code
      /// base.  Second, they are used to assist in loading the appropriate
      /// SQLite interop assembly into the current process.
      /// </summary>
      private static Dictionary<string, string> processorArchitecturePlatforms;

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// This is the cached return value from the
      /// <see cref="GetAssemblyDirectory" /> method -OR- null if that method
      /// has never returned a valid value.
      /// </summary>
      private static string cachedAssemblyDirectory;

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// When this field is non-zero, it indicates the
      /// <see cref="GetAssemblyDirectory" /> method was not able to locate a
      /// suitable assembly directory.  The
      /// <see cref="GetCachedAssemblyDirectory" /> method will check this
      /// field and skips calls into the <see cref="GetAssemblyDirectory" />
      /// method whenever it is non-zero.
      /// </summary>
      private static bool noAssemblyDirectory;

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// This is the cached return value from the
      /// <see cref="GetXmlConfigFileName" /> method -OR- null if that method
      /// has never returned a valid value.
      /// </summary>
      private static string cachedXmlConfigFileName;

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// When this field is non-zero, it indicates the
      /// <see cref="GetXmlConfigFileName" /> method was not able to locate a
      /// suitable XML configuration file name.  The
      /// <see cref="GetCachedXmlConfigFileName" /> method will check this
      /// field and skips calls into the <see cref="GetXmlConfigFileName" />
      /// method whenever it is non-zero.
      /// </summary>
      private static bool noXmlConfigFileName;
      #endregion

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// For now, this method simply calls the Initialize method.
      /// </summary>
      static UnsafeNativeMethods()
      {
          Initialize();
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Attempts to initialize this class by pre-loading the native SQLite
      /// library for the processor architecture of the current process.
      /// </summary>
      internal static void Initialize()
      {
          #region Debug Build Only
#if DEBUG
          //
          // NOTE: Create the lists of statistics that will contain
          //       various counts used in debugging, including the
          //       number of times each setting value has been read.
          //
          DebugData.Initialize();
#endif
          #endregion

          //
          // NOTE: Check if a debugger needs to be attached before doing any
          //       real work.
          //
          HelperMethods.MaybeBreakIntoDebugger();

#if SQLITE_STANDARD || USE_INTEROP_DLL || PLATFORM_COMPACTFRAMEWORK
#if PRELOAD_NATIVE_LIBRARY
          //
          // NOTE: If the "No_PreLoadSQLite" environment variable is set (to
          //       anything), skip all of our special code and simply return.
          //
          if (GetSettingValue("No_PreLoadSQLite", null) != null)
              return;
#endif
#endif

          lock (staticSyncRoot)
          {
#if !PLATFORM_COMPACTFRAMEWORK
              //
              // TODO: Make sure to keep these lists updated when the
              //       target framework names (or their abbreviations)
              //       -OR- the processor architecture names (or their
              //       platform names) change.
              //
              if (targetFrameworkAbbreviations == null)
              {
                  targetFrameworkAbbreviations =
                      new Dictionary<string, string>(
                          StringComparer.OrdinalIgnoreCase);

                  targetFrameworkAbbreviations.Add(
                      ".NETFramework,Version=v2.0", "net20");

                  targetFrameworkAbbreviations.Add(
                      ".NETFramework,Version=v3.5", "net35");

                  targetFrameworkAbbreviations.Add(
                      ".NETFramework,Version=v4.0", "net40");

                  targetFrameworkAbbreviations.Add(
                      ".NETFramework,Version=v4.5", "net45");

                  targetFrameworkAbbreviations.Add(
                      ".NETFramework,Version=v4.5.1", "net451");

                  targetFrameworkAbbreviations.Add(
                      ".NETFramework,Version=v4.5.2", "net452");

                  targetFrameworkAbbreviations.Add(
                      ".NETFramework,Version=v4.6", "net46");

                  targetFrameworkAbbreviations.Add(
                      ".NETFramework,Version=v4.6.1", "net461");

                  targetFrameworkAbbreviations.Add(
                      ".NETFramework,Version=v4.6.2", "net462");

                  targetFrameworkAbbreviations.Add(
                      ".NETFramework,Version=v4.7", "net47");

                  targetFrameworkAbbreviations.Add(
                      ".NETFramework,Version=v4.7.1", "net471");

                  targetFrameworkAbbreviations.Add(
                      ".NETFramework,Version=v4.7.2", "net472");

                  targetFrameworkAbbreviations.Add(
                      ".NETFramework,Version=v4.8", "net48");

                  targetFrameworkAbbreviations.Add(
                      ".NETStandard,Version=v2.0", "netstandard2.0");

                  targetFrameworkAbbreviations.Add(
                      ".NETStandard,Version=v2.1", "netstandard2.1");
              }
#endif

              if (processorArchitecturePlatforms == null)
              {
                  //
                  // NOTE: Create the map of processor architecture names
                  //       to platform names using a case-insensitive string
                  //       comparer.
                  //
                  processorArchitecturePlatforms =
                      new Dictionary<string, string>(
                          StringComparer.OrdinalIgnoreCase);

                  //
                  // NOTE: Setup the list of platform names associated with
                  //       the supported processor architectures.
                  //
                  processorArchitecturePlatforms.Add("x86", "Win32");
                  processorArchitecturePlatforms.Add("x86_64", "x64");
                  processorArchitecturePlatforms.Add("AMD64", "x64");
                  processorArchitecturePlatforms.Add("IA64", "Itanium");
                  processorArchitecturePlatforms.Add("ARM", "WinCE");
              }

#if SQLITE_STANDARD || USE_INTEROP_DLL || PLATFORM_COMPACTFRAMEWORK
#if PRELOAD_NATIVE_LIBRARY
              //
              // BUGBUG: What about other application domains?
              //
              if (_SQLiteNativeModuleHandle == IntPtr.Zero)
              {
                  string baseDirectory = null;
                  string processorArchitecture = null;
                  bool allowBaseDirectoryOnly = false;

                  /* IGNORED */
                  SearchForDirectory(
                      ref baseDirectory, ref processorArchitecture,
                      ref allowBaseDirectoryOnly);

                  //
                  // NOTE: Attempt to pre-load the SQLite core library (or
                  //       interop assembly) and store both the file name
                  //       and native module handle for later usage.
                  //
                  /* IGNORED */
                  PreLoadSQLiteDll(baseDirectory,
                      processorArchitecture, allowBaseDirectoryOnly,
                      ref _SQLiteNativeModuleFileName,
                      ref _SQLiteNativeModuleHandle);
              }
#endif
#endif
          }
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Combines two path strings.
      /// </summary>
      /// <param name="path1">
      /// The first path -OR- null.
      /// </param>
      /// <param name="path2">
      /// The second path -OR- null.
      /// </param>
      /// <returns>
      /// The combined path string -OR- null if both of the original path
      /// strings are null.
      /// </returns>
      private static string MaybeCombinePath(
          string path1,
          string path2
          )
      {
          if (path1 != null)
          {
              if (path2 != null)
                  return Path.Combine(path1, path2);
              else
                  return path1;
          }
          else
          {
              if (path2 != null)
                  return path2;
              else
                  return null;
          }
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Resets the cached XML configuration file name value, thus forcing the
      /// next call to <see cref="GetCachedXmlConfigFileName" /> method to rely
      /// upon the <see cref="GetXmlConfigFileName" /> method to fetch the
      /// XML configuration file name.
      /// </summary>
      private static void ResetCachedXmlConfigFileName()
      {
          #region Debug Build Only
#if DEBUG
          DebugData.IncrementOtherCount("Method_ResetCachedXmlConfigFileName");
#endif
          #endregion

          lock (staticSyncRoot)
          {
              cachedXmlConfigFileName = null;
              noXmlConfigFileName = false;
          }
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Queries and returns the cached XML configuration file name for the
      /// assembly containing the managed System.Data.SQLite components, if
      /// available.  If the cached XML configuration file name value is not
      /// available, the <see cref="GetXmlConfigFileName" /> method will
      /// be used to obtain the XML configuration file name.
      /// </summary>
      /// <returns>
      /// The XML configuration file name -OR- null if it cannot be determined
      /// or does not exist.
      /// </returns>
      private static string GetCachedXmlConfigFileName()
      {
          #region Debug Build Only
#if DEBUG
          DebugData.IncrementOtherCount("Method_GetCachedXmlConfigFileName");
#endif
          #endregion

          lock (staticSyncRoot)
          {
              if (cachedXmlConfigFileName != null)
                  return cachedXmlConfigFileName;

              if (noXmlConfigFileName)
                  return null;
          }

          return GetXmlConfigFileName();
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Queries and returns the XML configuration file name for the assembly
      /// containing the managed System.Data.SQLite components.
      /// </summary>
      /// <returns>
      /// The XML configuration file name -OR- null if it cannot be determined
      /// or does not exist.
      /// </returns>
      private static string GetXmlConfigFileName()
      {
          #region Debug Build Only
#if DEBUG
          DebugData.IncrementOtherCount("Method_GetXmlConfigFileName");
#endif
          #endregion

          string directory;
          string fileName;

#if !PLATFORM_COMPACTFRAMEWORK
          directory = AppDomain.CurrentDomain.BaseDirectory;
          fileName = MaybeCombinePath(directory, XmlConfigFileName);

          if (File.Exists(fileName))
          {
              lock (staticSyncRoot)
              {
                  cachedXmlConfigFileName = fileName;
              }

              return fileName;
          }
#endif

          directory = GetCachedAssemblyDirectory();
          fileName = MaybeCombinePath(directory, XmlConfigFileName);

          if (File.Exists(fileName))
          {
              lock (staticSyncRoot)
              {
                  cachedXmlConfigFileName = fileName;
              }

              return fileName;
          }

          lock (staticSyncRoot)
          {
              noXmlConfigFileName = true;
          }

          return null;
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// If necessary, replaces all supported XML configuration file tokens
      /// with their associated values.
      /// </summary>
      /// <param name="fileName">
      /// The name of the XML configuration file being read.
      /// </param>
      /// <param name="value">
      /// A setting value read from the XML configuration file.
      /// </param>
      /// <returns>
      /// The value of the <paramref name="value" /> will all supported XML
      /// configuration file tokens replaced.  No return value is reserved
      /// to indicate an error.  This method cannot fail.
      /// </returns>
      private static string ReplaceXmlConfigFileTokens(
          string fileName,
          string value
          )
      {
          if (!String.IsNullOrEmpty(value))
          {
              if (!String.IsNullOrEmpty(fileName))
              {
                  try
                  {
                      string directory = Path.GetDirectoryName(fileName);

                      if (!String.IsNullOrEmpty(directory))
                      {
                          value = value.Replace(
                              XmlConfigDirectoryToken, directory);
                      }
                  }
#if !NET_COMPACT_20 && TRACE_SHARED
                  catch (Exception e)
#else
                  catch (Exception)
#endif
                  {
#if !NET_COMPACT_20 && TRACE_SHARED
                      try
                      {
                          Trace.WriteLine(HelperMethods.StringFormat(
                              CultureInfo.CurrentCulture, "Native library " +
                              "pre-loader failed to replace XML " +
                              "configuration file \"{0}\" tokens: {1}",
                              fileName, e)); /* throw */
                      }
                      catch
                      {
                          // do nothing.
                      }
#endif
                  }
              }
          }

          return value;
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// Queries and returns the value of the specified setting, using the
      /// specified XML configuration file.
      /// </summary>
      /// <param name="fileName">
      /// The name of the XML configuration file to read.
      /// </param>
      /// <param name="name">
      /// The name of the setting.
      /// </param>
      /// <param name="default">
      /// The value to be returned if the setting has not been set explicitly
      /// or cannot be determined.
      /// </param>
      /// <param name="expand">
      /// Non-zero to expand any environment variable references contained in
      /// the setting value to be returned.  This has no effect on the .NET
      /// Compact Framework.
      /// </param>
      /// <returns>
      /// The value of the setting -OR- the default value specified by
      /// <paramref name="default" /> if it has not been set explicitly or
      /// cannot be determined.
      /// </returns>
      private static string GetSettingValueViaXmlConfigFile(
          string fileName, /* in */
          string name,     /* in */
          string @default, /* in */
          bool expand      /* in */
          )
      {
          try
          {
              if ((fileName == null) || (name == null))
                  return @default;

              XmlDocument document = new XmlDocument();

              document.Load(fileName); /* throw */

              XmlElement element = document.SelectSingleNode(
                  HelperMethods.StringFormat(CultureInfo.InvariantCulture,
                  "/configuration/appSettings/add[@key='{0}']", name)) as
                  XmlElement; /* throw */

              if (element != null)
              {
                  string value = null;

                  if (element.HasAttribute("value"))
                      value = element.GetAttribute("value");

                  if (!String.IsNullOrEmpty(value))
                  {
#if !PLATFORM_COMPACTFRAMEWORK
                      if (expand)
                          value = Environment.ExpandEnvironmentVariables(value);

                      value = ReplaceEnvironmentVariableTokens(value);
#endif

                      value = ReplaceXmlConfigFileTokens(fileName, value);
                  }

                  if (value != null)
                      return value;
              }
          }
#if !NET_COMPACT_20 && TRACE_SHARED
          catch (Exception e)
#else
          catch (Exception)
#endif
          {
#if !NET_COMPACT_20 && TRACE_SHARED
              try
              {
                  Trace.WriteLine(HelperMethods.StringFormat(
                      CultureInfo.CurrentCulture, "Native library " +
                      "pre-loader failed to get setting \"{0}\" value " +
                      "from XML configuration file \"{1}\": {2}", name,
                      fileName, e)); /* throw */
              }
              catch
              {
                  // do nothing.
              }
#endif
          }

          return @default;
      }

      /////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
      /// <summary>
      /// Attempts to determine the target framework attribute value that is
      /// associated with the specified managed assembly, if applicable.
      /// </summary>
      /// <param name="assembly">
      /// The managed assembly to read the target framework attribute value
      /// from.
      /// </param>
      /// <returns>
      /// The value of the target framework attribute value for the specified
      /// managed assembly -OR- null if it cannot be determined.  If this
      /// assembly was compiled with a version of the .NET Framework prior to
      /// version 4.0, the value returned MAY reflect that version of the .NET
      /// Framework instead of the one associated with the specified managed
      /// assembly.
      /// </returns>
      private static string GetAssemblyTargetFramework(
          Assembly assembly
          )
      {
          if (assembly != null)
          {
#if NET_40 || NET_45 || NET_451 || NET_452 || NET_46 || NET_461 || NET_462 || NET_47 || NET_471 || NET_472 || NET_STANDARD_20 || NET_STANDARD_21
              try
              {
                  if (assembly.IsDefined(
                          typeof(TargetFrameworkAttribute), false))
                  {
                      TargetFrameworkAttribute targetFramework =
                          (TargetFrameworkAttribute)
                          assembly.GetCustomAttributes(
                              typeof(TargetFrameworkAttribute), false)[0];

                      return targetFramework.FrameworkName;
                  }
              }
              catch
              {
                  // do nothing.
              }
#elif NET_35
              return ".NETFramework,Version=v3.5";
#elif NET_20
              return ".NETFramework,Version=v2.0";
#endif
          }

          return null;
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// Accepts a long target framework attribute value and makes it into a
      /// much shorter version, suitable for use with NuGet packages.
      /// </summary>
      /// <param name="targetFramework">
      /// The long target framework attribute value to convert.
      /// </param>
      /// <returns>
      /// The short target framework attribute value -OR- null if it cannot
      /// be determined or converted.
      /// </returns>
      private static string AbbreviateTargetFramework(
          string targetFramework
          )
      {
          if (!String.IsNullOrEmpty(targetFramework))
          {
              string abbreviation;

              lock (staticSyncRoot)
              {
                  if (targetFrameworkAbbreviations != null)
                  {
                      if (targetFrameworkAbbreviations.TryGetValue(
                              targetFramework, out abbreviation))
                      {
                          return abbreviation;
                      }
                  }
              }

              //
              // HACK: *LEGACY* Fallback to the old method of
              //       abbreviating target framework names.
              //
              int index = targetFramework.IndexOf(
                  ".NETFramework,Version=v");

              if (index != -1)
              {
                  abbreviation = targetFramework;

                  abbreviation = abbreviation.Replace(
                      ".NETFramework,Version=v", "net");

                  abbreviation = abbreviation.Replace(
                      ".", String.Empty);

                  index = abbreviation.IndexOf(',');

                  if (index != -1)
                      return abbreviation.Substring(0, index);
                  else
                      return abbreviation;
              }
          }

          return targetFramework;
      }

      /////////////////////////////////////////////////////////////////////////

      /// <summary>
      /// If necessary, replaces all supported environment variable tokens
      /// with their associated values.
      /// </summary>
      /// <param name="value">
      /// A setting value read from an environment variable.
      /// </param>
      /// <returns>
      /// The value of the <paramref name="value" /> will all supported
      /// environment variable tokens replaced.  No return value is reserved
      /// to indicate an error.  This method cannot fail.
      /// </returns>
      private static string ReplaceEnvironmentVariableTokens(
          string value
          )
      {
          if (!String.IsNullOrEmpty(value))
          {
              string directory = GetCachedAssemblyDirectory();

              if (!String.IsNullOrEmpty(directory))
              {
                  try
                  {
                      value = value.Replace(
                          AssemblyDirectoryToken, directory);
                  }
#if !NET_COMPACT_20 && TRACE_SHARED
                  catch (Exception e)
#else
                  catch (Exception)
#endif
                  {
#if !NET_COMPACT_20 && TRACE_SHARED
                      try
                      {
                          Trace.WriteLine(HelperMethods.StringFormat(
                              CultureInfo.CurrentCulture, "Native library " +
                              "pre-loader failed to replace assembly " +
                              "directory token: {0}", e)); /* throw */
                      }
                      catch
                      {
                          // do nothing.
                      }
#endif
                  }
              }

              Assembly assembly = null;

              try
              {
                  assembly = Assembly.GetExecutingAssembly();
              }
#if !NET_COMPACT_20 && TRACE_SHARED
              catch (Exception e)
#else
              catch (Exception)
#endif
              {
#if !NET_COMPACT_20 && TRACE_SHARED
                  try
                  {
                      Trace.WriteLine(HelperMethods.StringFormat(
                          CultureInfo.CurrentCulture, "Native library " +
                          "pre-loader failed to obtain executing " +
                          "assembly: {0}", e)); /* throw */
                  }
                  catch
                  {
                      // do nothing.
                  }
#endif
              }

              string targetFramework = AbbreviateTargetFramework(
                  GetAssemblyTargetFramework(assembly));

              if (!String.IsNullOrEmpty(targetFramework))
              {
                  try
                  {
                      value = value.Replace(
                          TargetFrameworkToken, targetFramework);
                  }
#if !NET_COMPACT_20 && TRACE_SHARED
                  catch (Exception e)
#else
                  catch (Exception)
#endif
                  {
#if !NET_COMPACT_20 && TRACE_SHARED
                      try
                      {
                          Trace.WriteLine(HelperMethods.StringFormat(
                              CultureInfo.CurrentCulture, "Native library " +
                              "pre-loader failed to replace target " +
                              "framework token: {0}", e)); /* throw */
                      }
                      catch
                      {
                          // do nothing.
                      }
#endif
                  }
              }
          }

          return value;
      }
#endif

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Queries and returns the value of the specified setting, using the XML
      /// configuration file and/or the environment variables for the current
      /// process and/or the current system, when available.
      /// </summary>
      /// <param name="name">
      /// The name of the setting.
      /// </param>
      /// <param name="default">
      /// The value to be returned if the setting has not been set explicitly
      /// or cannot be determined.
      /// </param>
      /// <returns>
      /// The value of the setting -OR- the default value specified by
      /// <paramref name="default" /> if it has not been set explicitly or
      /// cannot be determined.  By default, all references to existing
      /// environment variables will be expanded to their corresponding values
      /// within the value to be returned unless either the "No_Expand" or
      /// "No_Expand_<paramref name="name" />" environment variable is set [to
      /// anything].
      /// </returns>
      internal static string GetSettingValue(
          string name,    /* in */
          string @default /* in */
          )
      {
#if !PLATFORM_COMPACTFRAMEWORK
          //
          // NOTE: If the special "No_SQLiteGetSettingValue" environment
          //       variable is set [to anything], this method will always
          //       return the default value.
          //
          if (Environment.GetEnvironmentVariable(
                "No_SQLiteGetSettingValue") != null)
          {
              return @default;
          }
#endif

          /////////////////////////////////////////////////////////////////////

          if (name == null)
              return @default;

          /////////////////////////////////////////////////////////////////////

          #region Debug Build Only
#if DEBUG
          //
          // NOTE: We are about to read a setting value from the environment
          //       or possibly from the XML configuration file; create or
          //       increment the appropriate statistic now.
          //
          DebugData.IncrementSettingReadCount(name, false);
#endif
          #endregion

          /////////////////////////////////////////////////////////////////////

          bool expand = true; /* SHARED: Environment -AND- XML config file. */

          /////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
          string value = null;

          if (Environment.GetEnvironmentVariable("No_Expand") != null)
          {
              expand = false;
          }
          else if (Environment.GetEnvironmentVariable(
                  HelperMethods.StringFormat(CultureInfo.InvariantCulture,
                  "No_Expand_{0}", name)) != null)
          {
              expand = false;
          }

          value = Environment.GetEnvironmentVariable(name);

          if (!String.IsNullOrEmpty(value))
          {
              if (expand)
                  value = Environment.ExpandEnvironmentVariables(value);

              value = ReplaceEnvironmentVariableTokens(value);
          }

          if (value != null)
              return value;

          //
          // NOTE: If the "No_SQLiteXmlConfigFile" environment variable is
          //       set [to anything], this method will NEVER read from the
          //       XML configuration file.
          //
          if (Environment.GetEnvironmentVariable(
                "No_SQLiteXmlConfigFile") != null)
          {
              return @default;
          }
#endif

          /////////////////////////////////////////////////////////////////////

          #region Debug Build Only
#if DEBUG
          //
          // NOTE: We are about to read a setting value from the XML
          //       configuration file; create or increment the appropriate
          //       statistic now.
          //
          DebugData.IncrementSettingReadCount(name, true);
#endif
          #endregion

          /////////////////////////////////////////////////////////////////////

          return GetSettingValueViaXmlConfigFile(
              GetCachedXmlConfigFileName(), name, @default, expand);
      }

      /////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
      private static string ListToString(IList<string> list)
      {
          if (list == null)
              return null;

          StringBuilder result = new StringBuilder();

          foreach (string element in list)
          {
              if (element == null)
                  continue;

              if (result.Length > 0)
                  result.Append(' ');

              result.Append(element);
          }

          return result.ToString();
      }

      /////////////////////////////////////////////////////////////////////////

      private static int CheckForArchitecturesAndPlatforms(
          string directory,
          ref List<string> matches
          )
      {
          int result = 0;

          if (matches == null)
              matches = new List<string>();

          lock (staticSyncRoot)
          {
              if (!String.IsNullOrEmpty(directory) &&
                  (processorArchitecturePlatforms != null))
              {
                  foreach (KeyValuePair<string, string> pair
                            in processorArchitecturePlatforms)
                  {
                      if (Directory.Exists(MaybeCombinePath(directory, pair.Key)))
                      {
                          matches.Add(pair.Key);
                          result++;
                      }

                      string value = pair.Value;

                      if (value == null)
                          continue;

                      if (Directory.Exists(MaybeCombinePath(directory, value)))
                      {
                          matches.Add(value);
                          result++;
                      }
                  }
              }
          }

          return result;
      }

      /////////////////////////////////////////////////////////////////////////

      private static bool CheckAssemblyCodeBase(
          Assembly assembly,
          ref string fileName
          )
      {
          try
          {
              if (assembly == null)
                  return false;

              string codeBase = assembly.CodeBase;

              if (String.IsNullOrEmpty(codeBase))
                  return false;

              Uri uri = new Uri(codeBase);
              string localFileName = uri.LocalPath;

              if (!File.Exists(localFileName))
                  return false;

              string directory = Path.GetDirectoryName(
                  localFileName); /* throw */

              string xmlConfigFileName = MaybeCombinePath(
                  directory, XmlConfigFileName);

              if (File.Exists(xmlConfigFileName))
              {
#if !NET_COMPACT_20 && TRACE_DETECTION
                  try
                  {
                      Trace.WriteLine(HelperMethods.StringFormat(
                          CultureInfo.CurrentCulture,
                          "Native library pre-loader found XML configuration file " +
                          "via code base for currently executing assembly: \"{0}\"",
                          xmlConfigFileName)); /* throw */
                  }
                  catch
                  {
                      // do nothing.
                  }
#endif

                  fileName = localFileName;
                  return true;
              }

              List<string> matches = null;

              if (CheckForArchitecturesAndPlatforms(directory, ref matches) > 0)
              {
#if !NET_COMPACT_20 && TRACE_DETECTION
                  try
                  {
                      Trace.WriteLine(HelperMethods.StringFormat(
                          CultureInfo.CurrentCulture,
                          "Native library pre-loader found native sub-directories " +
                          "via code base for currently executing assembly: \"{0}\"",
                          ListToString(matches))); /* throw */
                  }
                  catch
                  {
                      // do nothing.
                  }
#endif

                  fileName = localFileName;
                  return true;
              }

              return false;
          }
#if !NET_COMPACT_20 && TRACE_SHARED
          catch (Exception e)
#else
          catch (Exception)
#endif
          {
#if !NET_COMPACT_20 && TRACE_SHARED
              try
              {
                  Trace.WriteLine(HelperMethods.StringFormat(
                      CultureInfo.CurrentCulture,
                      "Native library pre-loader failed to check code base " +
                      "for currently executing assembly: {0}", e)); /* throw */
              }
              catch
              {
                  // do nothing.
              }
#endif
          }

          return false;
      }
#endif

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Resets the cached assembly directory value, thus forcing the next
      /// call to <see cref="GetCachedAssemblyDirectory" /> method to rely
      /// upon the <see cref="GetAssemblyDirectory" /> method to fetch the
      /// assembly directory.
      /// </summary>
      private static void ResetCachedAssemblyDirectory()
      {
          #region Debug Build Only
#if DEBUG
          DebugData.IncrementOtherCount("Method_ResetCachedAssemblyDirectory");
#endif
          #endregion

          lock (staticSyncRoot)
          {
              cachedAssemblyDirectory = null;
              noAssemblyDirectory = false;
          }
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Queries and returns the cached directory for the assembly currently
      /// being executed, if available.  If the cached assembly directory value
      /// is not available, the <see cref="GetAssemblyDirectory" /> method will
      /// be used to obtain the assembly directory.
      /// </summary>
      /// <returns>
      /// The directory for the assembly currently being executed -OR- null if
      /// it cannot be determined.
      /// </returns>
      private static string GetCachedAssemblyDirectory()
      {
          #region Debug Build Only
#if DEBUG
          DebugData.IncrementOtherCount("Method_GetCachedAssemblyDirectory");
#endif
          #endregion

          lock (staticSyncRoot)
          {
              if (cachedAssemblyDirectory != null)
                  return cachedAssemblyDirectory;

              if (noAssemblyDirectory)
                  return null;
          }

          return GetAssemblyDirectory();
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Queries and returns the directory for the assembly currently being
      /// executed.
      /// </summary>
      /// <returns>
      /// The directory for the assembly currently being executed -OR- null if
      /// it cannot be determined.
      /// </returns>
      private static string GetAssemblyDirectory()
      {
          #region Debug Build Only
#if DEBUG
          DebugData.IncrementOtherCount("Method_GetAssemblyDirectory");
#endif
          #endregion

          try
          {
              Assembly assembly = Assembly.GetExecutingAssembly();

              if (assembly == null)
              {
                  lock (staticSyncRoot)
                  {
                      noAssemblyDirectory = true;
                  }

                  return null;
              }

              string fileName = null;

#if PLATFORM_COMPACTFRAMEWORK
              AssemblyName assemblyName = assembly.GetName();

              if (assemblyName == null)
              {
                  lock (staticSyncRoot)
                  {
                      noAssemblyDirectory = true;
                  }

                  return null;
              }

              fileName = assemblyName.CodeBase;
#else
              if (!CheckAssemblyCodeBase(assembly, ref fileName))
                  fileName = assembly.Location;
#endif

              if (String.IsNullOrEmpty(fileName))
              {
                  lock (staticSyncRoot)
                  {
                      noAssemblyDirectory = true;
                  }

                  return null;
              }

              string directory = Path.GetDirectoryName(fileName);

              if (String.IsNullOrEmpty(directory))
              {
                  lock (staticSyncRoot)
                  {
                      noAssemblyDirectory = true;
                  }

                  return null;
              }

              lock (staticSyncRoot)
              {
                  cachedAssemblyDirectory = directory;
              }

              return directory;
          }
#if !NET_COMPACT_20 && TRACE_SHARED
          catch (Exception e)
#else
          catch (Exception)
#endif
          {
#if !NET_COMPACT_20 && TRACE_SHARED
              try
              {
                  Trace.WriteLine(HelperMethods.StringFormat(
                      CultureInfo.CurrentCulture,
                      "Native library pre-loader failed to get directory " +
                      "for currently executing assembly: {0}", e)); /* throw */
              }
              catch
              {
                  // do nothing.
              }
#endif
          }

          lock (staticSyncRoot)
          {
              noAssemblyDirectory = true;
          }

          return null;
      }
      #endregion

      /////////////////////////////////////////////////////////////////////////

      #region Optional Native SQLite Library Pre-Loading Code
      //
      // NOTE: If we are looking for the standard SQLite DLL ("sqlite3.dll"),
      //       the interop DLL ("SQLite.Interop.dll"), or we are running on the
      //       .NET Compact Framework, we should include this code (only if the
      //       feature has actually been enabled).  This code would be totally
      //       redundant if this module has been bundled into the mixed-mode
      //       assembly.
      //
#if SQLITE_STANDARD || USE_INTEROP_DLL || PLATFORM_COMPACTFRAMEWORK

      //
      // NOTE: Only compile in the native library pre-load code if the feature
      //       has been enabled for this build.
      //
#if PRELOAD_NATIVE_LIBRARY
      /// <summary>
      /// The name of the environment variable containing the processor
      /// architecture of the current process.
      /// </summary>
      private static readonly string PROCESSOR_ARCHITECTURE =
          "PROCESSOR_ARCHITECTURE";

      /////////////////////////////////////////////////////////////////////////

      #region Private Data
      /// <summary>
      /// The native module file name for the native SQLite library or null.
      /// </summary>
      internal static string _SQLiteNativeModuleFileName = null;

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// The native module handle for the native SQLite library or the value
      /// IntPtr.Zero.
      /// </summary>
      private static IntPtr _SQLiteNativeModuleHandle = IntPtr.Zero;
      #endregion

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Determines the base file name (without any directory information)
      /// for the native SQLite library to be pre-loaded by this class.
      /// </summary>
      /// <returns>
      /// The base file name for the native SQLite library to be pre-loaded by
      /// this class -OR- null if its value cannot be determined.
      /// </returns>
      internal static string GetNativeLibraryFileNameOnly()
      {
          string fileNameOnly = GetSettingValue(
              "PreLoadSQLite_LibraryFileNameOnly", null);

          if (fileNameOnly != null)
              return fileNameOnly;

          return SQLITE_DLL; /* COMPAT */
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Searches for the native SQLite library in the directory containing
      /// the assembly currently being executed as well as the base directory
      /// for the current application domain.
      /// </summary>
      /// <param name="baseDirectory">
      /// Upon success, this parameter will be modified to refer to the base
      /// directory containing the native SQLite library.
      /// </param>
      /// <param name="processorArchitecture">
      /// Upon success, this parameter will be modified to refer to the name
      /// of the immediate directory (i.e. the offset from the base directory)
      /// containing the native SQLite library.
      /// </param>
      /// <param name="allowBaseDirectoryOnly">
      /// Upon success, this parameter will be modified to non-zero only if
      /// the base directory itself should be allowed for loading the native
      /// library.
      /// </param>
      /// <returns>
      /// Non-zero (success) if the native SQLite library was found; otherwise,
      /// zero (failure).
      /// </returns>
      private static bool SearchForDirectory(
          ref string baseDirectory,         /* out */
          ref string processorArchitecture, /* out */
          ref bool allowBaseDirectoryOnly   /* out */
          )
      {
          if (GetSettingValue(
                "PreLoadSQLite_NoSearchForDirectory", null) != null)
          {
              return false; /* DISABLED */
          }

          //
          // NOTE: Determine the base file name for the native SQLite library.
          //       If this is not known by this class, we cannot continue.
          //
          string fileNameOnly = GetNativeLibraryFileNameOnly();

          if (fileNameOnly == null)
              return false;

          //
          // NOTE: Build the list of base directories and processor/platform
          //       names.  These lists will be used to help locate the native
          //       SQLite core library (or interop assembly) to pre-load into
          //       this process.
          //
          string[] directories = {
              GetAssemblyDirectory(),
#if !PLATFORM_COMPACTFRAMEWORK
              AppDomain.CurrentDomain.BaseDirectory,
#endif
          };

          string extraSubDirectory = null;

          if ((GetSettingValue(
                  "PreLoadSQLite_AllowBaseDirectoryOnly", null) != null) ||
              (HelperMethods.IsDotNetCore() && !HelperMethods.IsWindows()))
          {
              extraSubDirectory = String.Empty; /* .NET Core on POSIX */
          }

          string[] subDirectories = {
              GetProcessorArchitecture(), /* e.g. "x86" */
              GetPlatformName(null),      /* e.g. "Win32" */
              extraSubDirectory           /* base directory only? */
          };

          foreach (string directory in directories)
          {
              if (directory == null)
                  continue;

              foreach (string subDirectory in subDirectories)
              {
                  if (subDirectory == null)
                      continue;

                  string fileName = FixUpDllFileName(MaybeCombinePath(
                      MaybeCombinePath(directory, subDirectory),
                      fileNameOnly));

                  //
                  // NOTE: If the SQLite DLL file exists, return success.
                  //       Prior to returning, set the base directory and
                  //       processor architecture to reflect the location
                  //       where it was found.
                  //
                  if (File.Exists(fileName))
                  {
#if !NET_COMPACT_20 && TRACE_DETECTION
                      try
                      {
                          Trace.WriteLine(HelperMethods.StringFormat(
                              CultureInfo.CurrentCulture,
                              "Native library pre-loader found native file " +
                              "name \"{0}\", returning directory \"{1}\" and " +
                              "sub-directory \"{2}\"...", fileName, directory,
                              subDirectory)); /* throw */
                      }
                      catch
                      {
                          // do nothing.
                      }
#endif

                      baseDirectory = directory;
                      processorArchitecture = subDirectory;
                      allowBaseDirectoryOnly = (subDirectory.Length == 0);

                      return true; /* FOUND */
                  }
              }
          }

          return false; /* NOT FOUND */
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Queries and returns the base directory of the current application
      /// domain.
      /// </summary>
      /// <returns>
      /// The base directory for the current application domain -OR- null if it
      /// cannot be determined.
      /// </returns>
      private static string GetBaseDirectory()
      {
          //
          // NOTE: If the "PreLoadSQLite_BaseDirectory" environment variable
          //       is set, use it verbatim for the base directory.
          //
          string directory = GetSettingValue("PreLoadSQLite_BaseDirectory",
              null);

          if (directory != null)
              return directory;

#if !PLATFORM_COMPACTFRAMEWORK
          //
          // NOTE: If the "PreLoadSQLite_UseAssemblyDirectory" environment
          //       variable is set (to anything), then attempt to use the
          //       directory containing the currently executing assembly
          //       (i.e. System.Data.SQLite) intsead of the application
          //       domain base directory.
          //
          if (GetSettingValue(
                  "PreLoadSQLite_UseAssemblyDirectory", null) != null)
          {
              directory = GetAssemblyDirectory();

              if (directory != null)
                  return directory;
          }

          //
          // NOTE: Otherwise, fallback on using the base directory of the
          //       current application domain.
          //
          return AppDomain.CurrentDomain.BaseDirectory;
#else
          //
          // NOTE: Otherwise, fallback on using the directory containing
          //       the currently executing assembly.
          //
          return GetAssemblyDirectory();
#endif
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Determines if the dynamic link library file name requires a suffix
      /// and adds it if necessary.
      /// </summary>
      /// <param name="fileName">
      /// The original dynamic link library file name to inspect.
      /// </param>
      /// <returns>
      /// The dynamic link library file name, possibly modified to include an
      /// extension.
      /// </returns>
      private static string FixUpDllFileName(
          string fileName /* in */
          )
      {
          if (!String.IsNullOrEmpty(fileName))
          {
              if (HelperMethods.IsWindows())
              {
                  if (!fileName.EndsWith(DllFileExtension,
                          StringComparison.OrdinalIgnoreCase))
                  {
                      return fileName + DllFileExtension;
                  }
              }
          }

          return fileName;
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Queries and returns the processor architecture of the current
      /// process.
      /// </summary>
      /// <returns>
      /// The processor architecture of the current process -OR- null if it
      /// cannot be determined.
      /// </returns>
      private static string GetProcessorArchitecture()
      {
          //
          // NOTE: If the "PreLoadSQLite_ProcessorArchitecture" environment
          //       variable is set, use it verbatim for the current processor
          //       architecture.
          //
          string processorArchitecture = GetSettingValue(
              "PreLoadSQLite_ProcessorArchitecture", null);

          if (processorArchitecture != null)
              return processorArchitecture;

          //
          // BUGBUG: Will this always be reliable?
          //
          processorArchitecture = GetSettingValue(PROCESSOR_ARCHITECTURE, null);

          /////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
          //
          // HACK: Check for an "impossible" situation.  If the pointer size
          //       is 32-bits, the processor architecture cannot be "AMD64".
          //       In that case, we are almost certainly hitting a bug in the
          //       operating system and/or Visual Studio that causes the
          //       PROCESSOR_ARCHITECTURE environment variable to contain the
          //       wrong value in some circumstances.  Please refer to ticket
          //       [9ac9862611] for further information.
          //
          if ((IntPtr.Size == sizeof(int)) &&
              String.Equals(processorArchitecture, "AMD64",
                  StringComparison.OrdinalIgnoreCase))
          {
#if !NET_COMPACT_20 && TRACE_DETECTION
              //
              // NOTE: When tracing is enabled, save the originally detected
              //       processor architecture before changing it.
              //
              string savedProcessorArchitecture = processorArchitecture;
#endif

              //
              // NOTE: We know that operating systems that return "AMD64" as
              //       the processor architecture are actually a superset of
              //       the "x86" processor architecture; therefore, return
              //       "x86" when the pointer size is 32-bits.
              //
              processorArchitecture = "x86";

#if !NET_COMPACT_20 && TRACE_DETECTION
              try
              {
                  //
                  // NOTE: Show that we hit a fairly unusual situation (i.e.
                  //       the "wrong" processor architecture was detected).
                  //
                  Trace.WriteLine(HelperMethods.StringFormat(
                      CultureInfo.CurrentCulture,
                      "Native library pre-loader detected {0}-bit pointer " +
                      "size with processor architecture \"{1}\", using " +
                      "processor architecture \"{2}\" instead...",
                      IntPtr.Size * 8 /* bits */, savedProcessorArchitecture,
                      processorArchitecture)); /* throw */
              }
              catch
              {
                  // do nothing.
              }
#endif
          }
#endif

          /////////////////////////////////////////////////////////////////////

          if (processorArchitecture == null)
          {
              //
              // NOTE: Default to the processor architecture reported by the
              //       appropriate native operating system API, if any.
              //
              processorArchitecture = NativeLibraryHelper.GetMachine();

              //
              // NOTE: Upon failure, return empty string.  This will prevent
              //       the calling method from considering this method call
              //       a "failure".
              //
              if (processorArchitecture == null)
                  processorArchitecture = String.Empty;
          }

          /////////////////////////////////////////////////////////////////////

          return processorArchitecture;
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Given the processor architecture, returns the name of the platform.
      /// </summary>
      /// <param name="processorArchitecture">
      /// The processor architecture to be translated to a platform name.
      /// </param>
      /// <returns>
      /// The platform name for the specified processor architecture -OR- null
      /// if it cannot be determined.
      /// </returns>
      private static string GetPlatformName(
          string processorArchitecture /* in */
          )
      {
          if (processorArchitecture == null)
              processorArchitecture = GetProcessorArchitecture();

          if (String.IsNullOrEmpty(processorArchitecture))
              return null;

          lock (staticSyncRoot)
          {
              if (processorArchitecturePlatforms == null)
                  return null;

              string platformName;

              if (processorArchitecturePlatforms.TryGetValue(
                      processorArchitecture, out platformName))
              {
                  return platformName;
              }
          }

          return null;
      }

      /////////////////////////////////////////////////////////////////////////
      /// <summary>
      /// Attempts to load the native SQLite library based on the specified
      /// directory and processor architecture.
      /// </summary>
      /// <param name="baseDirectory">
      /// The base directory to use, null for default (the base directory of
      /// the current application domain).  This directory should contain the
      /// processor architecture specific sub-directories.
      /// </param>
      /// <param name="processorArchitecture">
      /// The requested processor architecture, null for default (the
      /// processor architecture of the current process).  This caller should
      /// almost always specify null for this parameter.
      /// </param>
      /// <param name="allowBaseDirectoryOnly">
      /// Non-zero indicates that the native SQLite library can be loaded
      /// from the base directory itself.
      /// </param>
      /// <param name="nativeModuleFileName">
      /// The candidate native module file name to load will be stored here,
      /// if necessary.
      /// </param>
      /// <param name="nativeModuleHandle">
      /// The native module handle as returned by LoadLibrary will be stored
      /// here, if necessary.  This value will be IntPtr.Zero if the call to
      /// LoadLibrary fails.
      /// </param>
      /// <returns>
      /// Non-zero if the native module was loaded successfully; otherwise,
      /// zero.
      /// </returns>
      private static bool PreLoadSQLiteDll(
          string baseDirectory,            /* in */
          string processorArchitecture,    /* in */
          bool allowBaseDirectoryOnly,     /* in */
          ref string nativeModuleFileName, /* out */
          ref IntPtr nativeModuleHandle    /* out */
          )
      {
          //
          // NOTE: If the specified base directory is null, use the default
          //       (i.e. attempt to automatically detect it).
          //
          if (baseDirectory == null)
              baseDirectory = GetBaseDirectory();

          //
          // NOTE: If we failed to query the base directory, stop now.
          //
          if (baseDirectory == null)
              return false;

          //
          // NOTE: Determine the base file name for the native SQLite library.
          //       If this is not known by this class, we cannot continue.
          //
          string fileNameOnly = GetNativeLibraryFileNameOnly();

          if (fileNameOnly == null)
              return false;

          //
          // NOTE: If the native SQLite library exists in the base directory
          //       itself, possibly stop now.
          //
          string fileName = FixUpDllFileName(MaybeCombinePath(baseDirectory,
              fileNameOnly));

          if (File.Exists(fileName))
          {
              //
              // NOTE: If the caller is allowing the base directory itself
              //       to be used, also make sure a processor architecture
              //       was not specified; if either condition is false just
              //       stop now and return failure.
              //
              if (allowBaseDirectoryOnly &&
                  String.IsNullOrEmpty(processorArchitecture))
              {
                  goto baseDirOnly;
              }
              else
              {
                  return false;
              }
          }

          //
          // NOTE: If the specified processor architecture is null, use the
          //       default.
          //
          if (processorArchitecture == null)
              processorArchitecture = GetProcessorArchitecture();

          //
          // NOTE: If we failed to query the processor architecture, stop now.
          //
          if (processorArchitecture == null)
              return false;

          //
          // NOTE: Build the full path and file name for the native SQLite
          //       library using the processor architecture name.
          //
          fileName = FixUpDllFileName(MaybeCombinePath(MaybeCombinePath(
              baseDirectory, processorArchitecture), fileNameOnly));

          //
          // NOTE: If the file name based on the processor architecture name
          // is not found, try using the associated platform name.
          //
          if (!File.Exists(fileName))
          {
              //
              // NOTE: Attempt to translate the processor architecture to a
              //       platform name.
              //
              string platformName = GetPlatformName(processorArchitecture);

              //
              // NOTE: If we failed to translate the platform name, stop now.
              //
              if (platformName == null)
                  return false;

              //
              // NOTE: Build the full path and file name for the native SQLite
              //       library using the platform name.
              //
              fileName = FixUpDllFileName(MaybeCombinePath(MaybeCombinePath(
                  baseDirectory, platformName), fileNameOnly));

              //
              // NOTE: If the file does not exist, skip trying to load it.
              //
              if (!File.Exists(fileName))
                  return false;
          }

      baseDirOnly:

          try
          {
#if !NET_COMPACT_20 && TRACE_PRELOAD
              try
              {
                  //
                  // NOTE: Show exactly where we are trying to load the native
                  //       SQLite library from.
                  //
                  Trace.WriteLine(HelperMethods.StringFormat(
                      CultureInfo.CurrentCulture,
                      "Native library pre-loader is trying to load native " +
                      "SQLite library \"{0}\"...", fileName)); /* throw */
              }
              catch
              {
                  // do nothing.
              }
#endif

              //
              // NOTE: Attempt to load the native library.  This will either
              //       return a valid native module handle, return IntPtr.Zero,
              //       or throw an exception.  This must use the appropriate
              //       P/Invoke method for the current operating system.
              //
              nativeModuleFileName = fileName;
              nativeModuleHandle = NativeLibraryHelper.LoadLibrary(fileName);

              return (nativeModuleHandle != IntPtr.Zero);
          }
#if !NET_COMPACT_20 && TRACE_PRELOAD
          catch (Exception e)
#else
          catch (Exception)
#endif
          {
#if !NET_COMPACT_20 && TRACE_PRELOAD
              try
              {
                  //
                  // NOTE: First, grab the last Win32 error number.
                  //
                  int lastError = Marshal.GetLastWin32Error(); /* throw */

                  //
                  // NOTE: Show where we failed to load the native SQLite
                  //       library from along with the Win32 error code and
                  //       exception information.
                  //
                  Trace.WriteLine(HelperMethods.StringFormat(
                      CultureInfo.CurrentCulture,
                      "Native library pre-loader failed to load native " +
                      "SQLite library \"{0}\" (getLastError = {1}): {2}",
                      fileName, lastError, e)); /* throw */
              }
              catch
              {
                  // do nothing.
              }
#endif
          }

          return false;
      }
#endif
#endif
      #endregion

      /////////////////////////////////////////////////////////////////////////

#if PLATFORM_COMPACTFRAMEWORK
    //
    // NOTE: On the .NET Compact Framework, the native interop assembly must
    //       be used because it provides several workarounds to .NET Compact
    //       Framework limitations important for proper operation of the core
    //       System.Data.SQLite functionality (e.g. being able to bind
    //       parameters and handle column values of types Int64 and Double).
    //
    internal const string SQLITE_DLL = "SQLite.Interop.112.dll";
#elif SQLITE_STANDARD
    //
    // NOTE: Otherwise, if the standard SQLite library is enabled, use it.
    //
    internal const string SQLITE_DLL = "sqlite3";
#elif USE_INTEROP_DLL
    //
    // NOTE: Otherwise, if the native SQLite interop assembly is enabled,
    //       use it.
    //
    internal const string SQLITE_DLL = "SQLite.Interop.dll";
#else
    //
    // NOTE: Finally, assume that the mixed-mode assembly is being used.
    //
    internal const string SQLITE_DLL = "System.Data.SQLite.dll";
#endif

    // This section uses interop calls that also fetch text length to optimize conversion.
    // When using the standard dll, we can replace these calls with normal sqlite calls and
    // do unoptimized conversions instead afterwards
    #region interop added textlength calls

#if !SQLITE_STANDARD

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_bind_parameter_name_interop(IntPtr stmt, int index, ref int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_database_name_interop(IntPtr stmt, int index, ref int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_database_name16_interop(IntPtr stmt, int index, ref int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_decltype_interop(IntPtr stmt, int index, ref int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_decltype16_interop(IntPtr stmt, int index, ref int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_name_interop(IntPtr stmt, int index, ref int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_name16_interop(IntPtr stmt, int index, ref int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_origin_name_interop(IntPtr stmt, int index, ref int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_origin_name16_interop(IntPtr stmt, int index, ref int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_table_name_interop(IntPtr stmt, int index, ref int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_table_name16_interop(IntPtr stmt, int index, ref int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_text_interop(IntPtr stmt, int index, ref int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_text16_interop(IntPtr stmt, int index, ref int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_errmsg_interop(IntPtr db, ref int len);

    [DllImport(SQLITE_DLL)]
    internal static extern SQLiteErrorCode sqlite3_prepare_interop(IntPtr db, IntPtr pSql, int nBytes, ref IntPtr stmt, ref IntPtr ptrRemain, ref int nRemain);

    [DllImport(SQLITE_DLL)]
    internal static extern SQLiteErrorCode sqlite3_table_column_metadata_interop(IntPtr db, byte[] dbName, byte[] tblName, byte[] colName, ref IntPtr ptrDataType, ref IntPtr ptrCollSeq, ref int notNull, ref int primaryKey, ref int autoInc, ref int dtLen, ref int csLen);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_value_text_interop(IntPtr p, ref int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_value_text16_interop(IntPtr p, ref int len);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_malloc_size_interop(IntPtr p);

#if INTEROP_LOG
    [DllImport(SQLITE_DLL)]
    internal static extern SQLiteErrorCode sqlite3_config_log_interop();
#endif
#endif
// !SQLITE_STANDARD

    #endregion

    // These functions add existing functionality on top of SQLite and require a little effort to
    // get working when using the standard SQLite library.
    #region interop added functionality

#if !SQLITE_STANDARD

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr interop_libversion();

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr interop_sourceid();

    [DllImport(SQLITE_DLL)]
    internal static extern int interop_compileoption_used(IntPtr zOptName);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr interop_compileoption_get(int N);

    [DllImport(SQLITE_DLL)]
    internal static extern SQLiteErrorCode sqlite3_close_interop(IntPtr db);

    [DllImport(SQLITE_DLL)]
    internal static extern SQLiteErrorCode sqlite3_create_function_interop(IntPtr db, byte[] strName, int nArgs, int nType, IntPtr pvUser, SQLiteCallback func, SQLiteCallback fstep, SQLiteFinalCallback ffinal, int needCollSeq);

    [DllImport(SQLITE_DLL)]
    internal static extern SQLiteErrorCode sqlite3_finalize_interop(IntPtr stmt);

    [DllImport(SQLITE_DLL)]
    internal static extern SQLiteErrorCode sqlite3_backup_finish_interop(IntPtr backup);

    [DllImport(SQLITE_DLL)]
    internal static extern SQLiteErrorCode sqlite3_blob_close_interop(IntPtr blob);

    [DllImport(SQLITE_DLL)]
    internal static extern SQLiteErrorCode sqlite3_open_interop(byte[] utf8Filename, byte[] vfsName, SQLiteOpenFlagsEnum flags, int extFuncs, ref IntPtr db);

    [DllImport(SQLITE_DLL)]
    internal static extern SQLiteErrorCode sqlite3_open16_interop(byte[] utf8Filename, byte[] vfsName, SQLiteOpenFlagsEnum flags, int extFuncs, ref IntPtr db);

    [DllImport(SQLITE_DLL)]
    internal static extern SQLiteErrorCode sqlite3_reset_interop(IntPtr stmt);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_changes_interop(IntPtr db);
#endif
// !SQLITE_STANDARD

    #endregion

    // The standard api call equivalents of the above interop calls
    #region standard versions of interop functions

#if SQLITE_STANDARD

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_close(IntPtr db);

#if !INTEROP_LEGACY_CLOSE
#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_close_v2(IntPtr db); /* 3.7.14+ */
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_create_function(IntPtr db, byte[] strName, int nArgs, int nType, IntPtr pvUser, SQLiteCallback func, SQLiteCallback fstep, SQLiteFinalCallback ffinal);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_finalize(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_backup_finish(IntPtr backup);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_reset(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_bind_parameter_name(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_database_name(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_database_name16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_decltype(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_decltype16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_name(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_name16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_origin_name(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_origin_name16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_table_name(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_table_name16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_text(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_text16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_errmsg(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_prepare(IntPtr db, IntPtr pSql, int nBytes, ref IntPtr stmt, ref IntPtr ptrRemain);

#if USE_PREPARE_V2
#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_prepare_v2(IntPtr db, IntPtr pSql, int nBytes, ref IntPtr stmt, ref IntPtr ptrRemain);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_table_column_metadata(IntPtr db, byte[] dbName, byte[] tblName, byte[] colName, ref IntPtr ptrDataType, ref IntPtr ptrCollSeq, ref int notNull, ref int primaryKey, ref int autoInc);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_value_text(IntPtr p);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_value_text16(IntPtr p);

#endif
    // SQLITE_STANDARD

    #endregion

    // These functions are custom and have no equivalent standard library method.
    // All of them are "nice to haves" and not necessarily "need to haves".
    #region no equivalent standard method

#if !SQLITE_STANDARD

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_context_collseq_interop(IntPtr context, ref int type, ref int enc, ref int len);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_context_collcompare_interop(IntPtr context, byte[] p1, int p1len, byte[] p2, int p2len);

    [DllImport(SQLITE_DLL)]
    internal static extern SQLiteErrorCode sqlite3_cursor_rowid_interop(IntPtr stmt, int cursor, ref long rowid);

    [DllImport(SQLITE_DLL)]
    internal static extern SQLiteErrorCode sqlite3_index_column_info_interop(IntPtr db, byte[] catalog, byte[] IndexName, byte[] ColumnName, ref int sortOrder, ref int onError, ref IntPtr Collation, ref int colllen);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_table_cursor_interop(IntPtr stmt, int db, int tableRootPage);

#endif
// !SQLITE_STANDARD

    #endregion

    // Standard API calls global across versions.  There are a few instances of interop calls
    // scattered in here, but they are only active when PLATFORM_COMPACTFRAMEWORK is declared.
    #region standard sqlite api calls

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_libversion();

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_libversion_number();

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_sourceid();

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_compileoption_used(IntPtr zOptName);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_compileoption_get(int N);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_enable_shared_cache(
        int enable);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_enable_load_extension(
        IntPtr db, int enable);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_load_extension(
        IntPtr db, byte[] fileName, byte[] procName, ref IntPtr pError);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_overload_function(IntPtr db, IntPtr zName, int nArgs);

#if WINDOWS
#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
#else
    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
#endif
    //
    // NOTE: The "sqlite3_win32_set_directory" SQLite core library function is
    //       only supported on Windows.
    //
    internal static extern SQLiteErrorCode sqlite3_win32_set_directory(uint type, string value);

#if !DEBUG // NOTE: Should be "WIN32HEAP && !MEMDEBUG"
#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    //
    // NOTE: The "sqlite3_win32_reset_heap" SQLite core library function is
    //       only supported on Windows when the Win32 native allocator is in
    //       use (i.e. by default, in "Release" builds of System.Data.SQLite
    //       only).  By default, in "Debug" builds of System.Data.SQLite, the
    //       MEMDEBUG allocator is used.
    //
    internal static extern SQLiteErrorCode sqlite3_win32_reset_heap();

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    //
    // NOTE: The "sqlite3_win32_compact_heap" SQLite core library function is
    //       only supported on Windows when the Win32 native allocator is in
    //       use (i.e. by default, in "Release" builds of System.Data.SQLite
    //       only).  By default, in "Debug" builds of System.Data.SQLite, the
    //       MEMDEBUG allocator is used.
    //
    internal static extern SQLiteErrorCode sqlite3_win32_compact_heap(ref uint largest);
#endif
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_malloc(int n);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_malloc64(ulong n);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_realloc(IntPtr p, int n);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_realloc64(IntPtr p, ulong n);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern ulong sqlite3_msize(IntPtr p);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_free(IntPtr p);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_open_v2(byte[] utf8Filename, ref IntPtr db, SQLiteOpenFlagsEnum flags, byte[] vfsName);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
#else
    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
#endif
    internal static extern SQLiteErrorCode sqlite3_open16(string fileName, ref IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_interrupt(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern long sqlite3_last_insert_rowid(IntPtr db);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_changes(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern long sqlite3_memory_used();
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern long sqlite3_memory_highwater(int resetFlag);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_shutdown();

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_busy_timeout(IntPtr db, int ms);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_clear_bindings(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_bind_blob(IntPtr stmt, int index, Byte[] value, int nSize, IntPtr nTransient);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern SQLiteErrorCode sqlite3_bind_double(IntPtr stmt, int index, double value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_bind_int(IntPtr stmt, int index, int value);

    //
    // NOTE: This really just calls "sqlite3_bind_int"; however, it has the
    //       correct type signature for an unsigned (32-bit) integer.
    //
#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_bind_int", CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_bind_int")]
#endif
    internal static extern SQLiteErrorCode sqlite3_bind_uint(IntPtr stmt, int index, uint value);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern SQLiteErrorCode sqlite3_bind_int64(IntPtr stmt, int index, long value);
#endif

    //
    // NOTE: This really just calls "sqlite3_bind_int64"; however, it has the
    //       correct type signature for an unsigned long (64-bit) integer.
    //
#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_bind_int64", CallingConvention = CallingConvention.Cdecl)]
    internal static extern SQLiteErrorCode sqlite3_bind_uint64(IntPtr stmt, int index, ulong value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_bind_null(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_bind_text(IntPtr stmt, int index, byte[] value, int nlen, IntPtr pvReserved);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_bind_parameter_count(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_bind_parameter_index(IntPtr stmt, byte[] strName);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_column_count(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_step(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_stmt_readonly(IntPtr stmt); /* 3.7.4+ */

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern double sqlite3_column_double(IntPtr stmt, int index);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_column_int(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern long sqlite3_column_int64(IntPtr stmt, int index);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_blob(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_column_bytes(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_column_bytes16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern TypeAffinity sqlite3_column_type(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_create_collation(IntPtr db, byte[] strName, int nType, IntPtr pvUser, SQLiteCollation func);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_aggregate_count(IntPtr context);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_value_blob(IntPtr p);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_value_bytes(IntPtr p);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_value_bytes16(IntPtr p);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern double sqlite3_value_double(IntPtr p);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_value_int(IntPtr p);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern long sqlite3_value_int64(IntPtr p);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern TypeAffinity sqlite3_value_type(IntPtr p);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_result_blob(IntPtr context, byte[] value, int nSize, IntPtr pvReserved);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void sqlite3_result_double(IntPtr context, double value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_result_error(IntPtr context, byte[] strErr, int nLen);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_result_error_code(IntPtr context, SQLiteErrorCode value);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_result_error_toobig(IntPtr context);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_result_error_nomem(IntPtr context);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_result_value(IntPtr context, IntPtr value);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_result_zeroblob(IntPtr context, int nLen);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_result_int(IntPtr context, int value);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void sqlite3_result_int64(IntPtr context, long value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_result_null(IntPtr context);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_result_text(IntPtr context, byte[] value, int nLen, IntPtr pvReserved);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_aggregate_context(IntPtr context, int nBytes);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
#else
    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
#endif
    internal static extern SQLiteErrorCode sqlite3_bind_text16(IntPtr stmt, int index, string value, int nlen, IntPtr pvReserved);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
#else
    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
#endif
    internal static extern void sqlite3_result_error16(IntPtr context, string strName, int nLen);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
#else
    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
#endif
    internal static extern void sqlite3_result_text16(IntPtr context, string strName, int nLen, IntPtr pvReserved);

#if INTEROP_CODEC || INTEROP_INCLUDE_SEE
#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_key(IntPtr db, byte[] key, int keylen);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_rekey(IntPtr db, byte[] key, int keylen);
#endif

#if INTEROP_INCLUDE_ZIPVFS
#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void zipvfsInit_v2();

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void zipvfsInit_v3(int regDflt);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_progress_handler(IntPtr db, int ops, SQLiteProgressCallback func, IntPtr pvUser);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_set_authorizer(IntPtr db, SQLiteAuthorizerCallback func, IntPtr pvUser);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_update_hook(IntPtr db, SQLiteUpdateCallback func, IntPtr pvUser);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_commit_hook(IntPtr db, SQLiteCommitCallback func, IntPtr pvUser);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_trace(IntPtr db, SQLiteTraceCallback func, IntPtr pvUser);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_trace_v2(IntPtr db, SQLiteTraceFlags mask, SQLiteTraceCallback2 func, IntPtr pvUser);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_limit(IntPtr db, SQLiteLimitOpsEnum op, int value);

    // Since sqlite3_config() takes a variable argument list, we have to overload declarations
    // for all possible calls that we want to use.
#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_config", CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_config")]
#endif
    internal static extern SQLiteErrorCode sqlite3_config_none(SQLiteConfigOpsEnum op);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_config", CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_config")]
#endif
    internal static extern SQLiteErrorCode sqlite3_config_int(SQLiteConfigOpsEnum op, int value);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_config", CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_config")]
#endif
    internal static extern SQLiteErrorCode sqlite3_config_log(SQLiteConfigOpsEnum op, SQLiteLogCallback func, IntPtr pvUser);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_db_config", CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_db_config")]
#endif
    internal static extern SQLiteErrorCode sqlite3_db_config_charptr(IntPtr db, SQLiteConfigDbOpsEnum op, IntPtr charPtr);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_db_config", CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_db_config")]
#endif
    internal static extern SQLiteErrorCode sqlite3_db_config_int_refint(IntPtr db, SQLiteConfigDbOpsEnum op, int value, ref int result);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_db_config", CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_db_config")]
#endif
    internal static extern SQLiteErrorCode sqlite3_db_config_intptr_two_ints(IntPtr db, SQLiteConfigDbOpsEnum op, IntPtr ptr, int int0, int int1);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_db_status(IntPtr db, SQLiteStatusOpsEnum op, ref int current, ref int highwater, int resetFlag);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_rollback_hook(IntPtr db, SQLiteRollbackCallback func, IntPtr pvUser);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_db_handle(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_db_release_memory(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_db_filename(IntPtr db, IntPtr dbName);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_db_readonly(IntPtr db, IntPtr dbName);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_db_filename", CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_db_filename")]
#endif
    internal static extern IntPtr sqlite3_db_filename_bytes(IntPtr db, byte[] dbName);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_next_stmt(IntPtr db, IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_exec(IntPtr db, byte[] strSql, IntPtr pvCallback, IntPtr pvParam, ref IntPtr errMsg);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_release_memory(int nBytes);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_get_autocommit(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_extended_result_codes(IntPtr db, int onoff);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_errcode(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_extended_errcode(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_errstr(SQLiteErrorCode rc); /* 3.7.15+ */

    // Since sqlite3_log() takes a variable argument list, we have to overload declarations
    // for all possible calls.  For now, we are only exposing a single string, and
    // depend on the caller to format the string.
#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_log(SQLiteErrorCode iErrCode, byte[] zFormat);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_file_control(IntPtr db, byte[] zDbName, int op, IntPtr pArg);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_backup_init(IntPtr destDb, byte[] zDestName, IntPtr sourceDb, byte[] zSourceName);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_backup_step(IntPtr backup, int nPage);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_backup_remaining(IntPtr backup);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_backup_pagecount(IntPtr backup);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_blob_close(IntPtr blob);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_blob_bytes(IntPtr blob);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_blob_open(IntPtr db, byte[] dbName, byte[] tblName, byte[] colName, long rowId, int flags, ref IntPtr ptrBlob);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_blob_read(IntPtr blob, [MarshalAs(UnmanagedType.LPArray)] byte[] buffer, int count, int offset);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_blob_reopen(IntPtr blob, long rowId);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_blob_write(IntPtr blob, [MarshalAs(UnmanagedType.LPArray)] byte[] buffer, int count, int offset);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3_declare_vtab(IntPtr db, IntPtr zSQL);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_mprintf(IntPtr format, __arglist);
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    // SQLite API calls that are provided by "well-known" extensions that may be statically
    // linked with the SQLite core native library currently in use.
    #region extension sqlite api calls
    #region virtual table
#if INTEROP_VIRTUAL_TABLE
#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_create_disposable_module(IntPtr db, IntPtr name, ref sqlite3_module module, IntPtr pClientData, xDestroyModule xDestroy);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_dispose_module(IntPtr pModule);
#endif
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region session extension
#if INTEROP_SESSION_EXTENSION
#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    internal delegate int xSessionFilter(IntPtr context, IntPtr pTblName);

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    internal delegate SQLiteChangeSetConflictResult xSessionConflict(IntPtr context, SQLiteChangeSetConflictType type, IntPtr iterator);

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    internal delegate SQLiteErrorCode xSessionInput(IntPtr context, IntPtr pData, ref int nData);

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    internal delegate SQLiteErrorCode xSessionOutput(IntPtr context, IntPtr pData, int nData);

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3session_create(IntPtr db, byte[] dbName, ref IntPtr session);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3session_delete(IntPtr session);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3session_enable(IntPtr session, int enable);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3session_indirect(IntPtr session, int indirect);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3session_attach(IntPtr session, byte[] tblName);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3session_table_filter(IntPtr session, xSessionFilter xFilter, IntPtr context);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3session_changeset(IntPtr session, ref int nChangeSet, ref IntPtr pChangeSet);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3session_diff(IntPtr session, byte[] fromDbName, byte[] tblName, ref IntPtr errMsg);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3session_patchset(IntPtr session, ref int nPatchSet, ref IntPtr pPatchSet);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3session_isempty(IntPtr session);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changeset_start(ref IntPtr iterator, int nChangeSet, IntPtr pChangeSet);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changeset_start_v2(ref IntPtr iterator, int nChangeSet, IntPtr pChangeSet, SQLiteChangeSetStartFlags flags);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changeset_next(IntPtr iterator);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changeset_op(IntPtr iterator, ref IntPtr pTblName, ref int nColumns, ref SQLiteAuthorizerActionCode op, ref int bIndirect);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changeset_pk(IntPtr iterator, ref IntPtr pPrimaryKeys, ref int nColumns);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changeset_old(IntPtr iterator, int columnIndex, ref IntPtr pValue);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changeset_new(IntPtr iterator, int columnIndex, ref IntPtr pValue);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changeset_conflict(IntPtr iterator, int columnIndex, ref IntPtr pValue);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changeset_fk_conflicts(IntPtr iterator, ref int conflicts);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changeset_finalize(IntPtr iterator);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changeset_invert(int nIn, IntPtr pIn, ref int nOut, ref IntPtr pOut);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changeset_concat(int nA, IntPtr pA, int nB, IntPtr pB, ref int nOut, ref IntPtr pOut);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changegroup_new(ref IntPtr changeGroup);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changegroup_add(IntPtr changeGroup, int nData, IntPtr pData);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changegroup_output(IntPtr changeGroup, ref int nData, ref IntPtr pData);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3changegroup_delete(IntPtr changeGroup);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changeset_apply(IntPtr db, int nChangeSet, IntPtr pChangeSet, xSessionFilter xFilter, xSessionConflict xConflict, IntPtr context);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changeset_apply_strm(IntPtr db, xSessionInput xInput, IntPtr pIn, xSessionFilter xFilter, xSessionConflict xConflict, IntPtr context);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changeset_concat_strm(xSessionInput xInputA, IntPtr pInA, xSessionInput xInputB, IntPtr pInB, xSessionOutput xOutput, IntPtr pOut);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changeset_invert_strm(xSessionInput xInput, IntPtr pIn, xSessionOutput xOutput, IntPtr pOut);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changeset_start_strm(ref IntPtr iterator, xSessionInput xInput, IntPtr pIn);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changeset_start_v2_strm(ref IntPtr iterator, xSessionInput xInput, IntPtr pIn, SQLiteChangeSetStartFlags flags);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3session_changeset_strm(IntPtr session, xSessionOutput xOutput, IntPtr pOut);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3session_patchset_strm(IntPtr session, xSessionOutput xOutput, IntPtr pOut);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changegroup_add_strm(IntPtr changeGroup, xSessionInput xInput, IntPtr pIn);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern SQLiteErrorCode sqlite3changegroup_output_strm(IntPtr changeGroup, xSessionOutput xOutput, IntPtr pOut);
#endif
    #endregion
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region sqlite interop api calls (.NET Compact Framework only)
#if PLATFORM_COMPACTFRAMEWORK && !SQLITE_STANDARD
    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_last_insert_rowid_interop(IntPtr db, ref long rowId);

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_memory_used_interop(ref long bytes);

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_memory_highwater_interop(int resetFlag, ref long bytes);

    [DllImport(SQLITE_DLL)]
    internal static extern SQLiteErrorCode sqlite3_bind_double_interop(IntPtr stmt, int index, ref double value);

    [DllImport(SQLITE_DLL)]
    internal static extern SQLiteErrorCode sqlite3_bind_int64_interop(IntPtr stmt, int index, ref long value);

    [DllImport(SQLITE_DLL, EntryPoint = "sqlite3_bind_int64_interop")]
    internal static extern SQLiteErrorCode sqlite3_bind_uint64_interop(IntPtr stmt, int index, ref ulong value);

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_column_double_interop(IntPtr stmt, int index, ref double value);

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_column_int64_interop(IntPtr stmt, int index, ref long value);

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_value_double_interop(IntPtr p, ref double value);

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_value_int64_interop(IntPtr p, ref Int64 value);

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_result_double_interop(IntPtr context, ref double value);

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_result_int64_interop(IntPtr context, ref Int64 value);

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_msize_interop(IntPtr p, ref ulong size);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_create_disposable_module_interop(
        IntPtr db, IntPtr name, IntPtr pModule, int iVersion, xCreate xCreate,
        xConnect xConnect, xBestIndex xBestIndex, xDisconnect xDisconnect,
        xDestroy xDestroy, xOpen xOpen, xClose xClose, xFilter xFilter,
        xNext xNext, xEof xEof, xColumn xColumn, xRowId xRowId, xUpdate xUpdate,
        xBegin xBegin, xSync xSync, xCommit xCommit, xRollback xRollback,
        xFindFunction xFindFunction, xRename xRename, xSavepoint xSavepoint,
        xRelease xRelease, xRollbackTo xRollbackTo, IntPtr pClientData,
        xDestroyModule xDestroyModule);
#endif
    // PLATFORM_COMPACTFRAMEWORK && !SQLITE_STANDARD
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region Native Delegates
#if INTEROP_VIRTUAL_TABLE
#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xCreate(
        IntPtr pDb,
        IntPtr pAux,
        int argc,
        IntPtr argv,
        ref IntPtr pVtab,
        ref IntPtr pError
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xConnect(
        IntPtr pDb,
        IntPtr pAux,
        int argc,
        IntPtr argv,
        ref IntPtr pVtab,
        ref IntPtr pError
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xBestIndex(
        IntPtr pVtab,
        IntPtr pIndex
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xDisconnect(
        IntPtr pVtab
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xDestroy(
        IntPtr pVtab
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xOpen(
        IntPtr pVtab,
        ref IntPtr pCursor
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xClose(
        IntPtr pCursor
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xFilter(
        IntPtr pCursor,
        int idxNum,
        IntPtr idxStr,
        int argc,
        IntPtr argv
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xNext(
        IntPtr pCursor
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate int xEof(
        IntPtr pCursor
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xColumn(
        IntPtr pCursor,
        IntPtr pContext,
        int index
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xRowId(
        IntPtr pCursor,
        ref long rowId
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xUpdate(
        IntPtr pVtab,
        int argc,
        IntPtr argv,
        ref long rowId
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xBegin(
        IntPtr pVtab
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xSync(
        IntPtr pVtab
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xCommit(
        IntPtr pVtab
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xRollback(
        IntPtr pVtab
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate int xFindFunction(
        IntPtr pVtab,
        int nArg,
        IntPtr zName,
        ref SQLiteCallback callback,
        ref IntPtr pUserData
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xRename(
        IntPtr pVtab,
        IntPtr zNew
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xSavepoint(
        IntPtr pVtab,
        int iSavepoint
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xRelease(
        IntPtr pVtab,
        int iSavepoint
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate SQLiteErrorCode xRollbackTo(
        IntPtr pVtab,
        int iSavepoint
    );

    ///////////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate void xDestroyModule(IntPtr pClientData);
#endif
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region Native Structures
#if INTEROP_VIRTUAL_TABLE
    [StructLayout(LayoutKind.Sequential)]
    internal struct sqlite3_module
    {
        /*   0 */ public int iVersion;
        /*   8 */ public xCreate xCreate;
        /*  16 */ public xConnect xConnect;
        /*  24 */ public xBestIndex xBestIndex;
        /*  32 */ public xDisconnect xDisconnect;
        /*  40 */ public xDestroy xDestroy;
        /*  48 */ public xOpen xOpen;
        /*  56 */ public xClose xClose;
        /*  64 */ public xFilter xFilter;
        /*  72 */ public xNext xNext;
        /*  80 */ public xEof xEof;
        /*  88 */ public xColumn xColumn;
        /*  96 */ public xRowId xRowId;
        /* 104 */ public xUpdate xUpdate;
        /* 112 */ public xBegin xBegin;
        /* 120 */ public xSync xSync;
        /* 128 */ public xCommit xCommit;
        /* 136 */ public xRollback xRollback;
        /* 144 */ public xFindFunction xFindFunction;
        /* 152 */ public xRename xRename;
        /* The methods above are in version 1 of the sqlite3_module
         * object.  Those below are for version 2 and greater. */
        /* 160 */ public xSavepoint xSavepoint;
        /* 168 */ public xRelease xRelease;
        /* 176 */ public xRollbackTo xRollbackTo;
    }

    ///////////////////////////////////////////////////////////////////////////

    [StructLayout(LayoutKind.Sequential)]
    internal struct sqlite3_vtab
    {
        /*  0 */ public IntPtr pModule;
        /*  8 */ public int nRef; /* NO LONGER USED */
        /* 16 */ public IntPtr zErrMsg;
    }

    ///////////////////////////////////////////////////////////////////////////

    [StructLayout(LayoutKind.Sequential)]
    internal struct sqlite3_vtab_cursor
    {
        /* 0 */ public IntPtr pVTab;
    }

    ///////////////////////////////////////////////////////////////////////////

    [StructLayout(LayoutKind.Sequential)]
    internal struct sqlite3_index_constraint
    {
        public sqlite3_index_constraint(
            SQLiteIndexConstraint constraint
            )
            : this()
        {
            if (constraint != null)
            {
                iColumn = constraint.iColumn;
                op = constraint.op;
                usable = constraint.usable;
                iTermOffset = constraint.iTermOffset;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /* 0 */ public int iColumn;
        /* 4 */ public SQLiteIndexConstraintOp op;
        /* 5 */ public byte usable;
        /* 8 */ public int iTermOffset;
    }

    ///////////////////////////////////////////////////////////////////////////

    [StructLayout(LayoutKind.Sequential)]
    internal struct sqlite3_index_orderby
    {
        public sqlite3_index_orderby(
            SQLiteIndexOrderBy orderBy
            )
            : this()
        {
            if (orderBy != null)
            {
                iColumn = orderBy.iColumn;
                desc = orderBy.desc;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /* 0 */ public int iColumn; /* Column number */
        /* 4 */ public byte desc;   /* True for DESC.  False for ASC. */
    }

    ///////////////////////////////////////////////////////////////////////////

    [StructLayout(LayoutKind.Sequential)]
    internal struct sqlite3_index_constraint_usage
    {
        public sqlite3_index_constraint_usage(
            SQLiteIndexConstraintUsage constraintUsage
            )
            : this()
        {
            if (constraintUsage != null)
            {
                argvIndex = constraintUsage.argvIndex;
                omit = constraintUsage.omit;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        public int argvIndex; /* if >0, constraint is part of argv to xFilter */
        public byte omit;     /* Do not code a test for this constraint */
    }

    ///////////////////////////////////////////////////////////////////////////

    [StructLayout(LayoutKind.Sequential)]
    internal struct sqlite3_index_info
    {
        /* Inputs */
        /*  0 */ public int nConstraint; /* Number of entries in aConstraint */
        /*  8 */ public IntPtr aConstraint;
        /* 16 */ public int nOrderBy;    /* Number of entries in aOrderBy */
        /* 24 */ public IntPtr aOrderBy;
        /* Outputs */
        /* 32 */ public IntPtr aConstraintUsage;
        /* 40 */ public int idxNum;           /* Number used to identify the index */
        /* 48 */ public string idxStr;        /* String, possibly obtained from sqlite3_malloc */
        /* 56 */ public int needToFreeIdxStr; /* Free idxStr using sqlite3_free() if true */
        /* 60 */ public int orderByConsumed;  /* True if output is already ordered */
        /* 64 */ public double estimatedCost; /* Estimated cost of using this index */
        /* 72 */ public long estimatedRows;   /* Estimated number of rows returned */
        /* 80 */ public SQLiteIndexFlags idxFlags; /* Mask of SQLITE_INDEX_SCAN_* flags */
        /* 88 */ public long colUsed;         /* Input: Mask of columns used by statement */
    }
#endif
    #endregion
  }
  #endregion

  /////////////////////////////////////////////////////////////////////////////

  #region .NET Compact Framework (only) CriticalHandle Class
#if PLATFORM_COMPACTFRAMEWORK
  internal abstract class CriticalHandle : IDisposable
  {
    private bool _isClosed;
    protected IntPtr handle;

    protected CriticalHandle(IntPtr invalidHandleValue)
    {
      handle = invalidHandleValue;
      _isClosed = false;
    }

    ~CriticalHandle()
    {
      Dispose(false);
    }

    private void Cleanup()
    {
      if (!IsClosed)
      {
        this._isClosed = true;
        if (!IsInvalid)
        {
          ReleaseHandle();
          GC.SuppressFinalize(this);
        }
      }
    }

    public void Close()
    {
      Dispose(true);
    }

    public void Dispose()
    {
      Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
      Cleanup();
    }

    protected abstract bool ReleaseHandle();

    protected void SetHandle(IntPtr value)
    {
      handle = value;
    }

    public void SetHandleAsInvalid()
    {
      _isClosed = true;
      GC.SuppressFinalize(this);
    }

    public bool IsClosed
    {
      get { return _isClosed; }
    }

    public abstract bool IsInvalid
    {
      get;
    }

  }
#endif
  #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteConnectionHandle Class
    // Handles the unmanaged database pointer, and provides finalization
    // support for it.
    internal sealed class SQLiteConnectionHandle : CriticalHandle
    {
#if SQLITE_STANDARD && !PLATFORM_COMPACTFRAMEWORK
        internal delegate void CloseConnectionCallback(
            SQLiteConnectionHandle hdl, IntPtr db);

        internal static CloseConnectionCallback closeConnection =
            SQLiteBase.CloseConnection;
#endif

        ///////////////////////////////////////////////////////////////////////

#if PLATFORM_COMPACTFRAMEWORK
        internal readonly object syncRoot = new object();
#endif

        ///////////////////////////////////////////////////////////////////////

        private bool ownHandle;

        ///////////////////////////////////////////////////////////////////////

        public static implicit operator IntPtr(SQLiteConnectionHandle db)
        {
            if (db != null)
            {
#if PLATFORM_COMPACTFRAMEWORK
                lock (db.syncRoot)
#endif
                {
                    return db.handle;
                }
            }
            return IntPtr.Zero;
        }

        ///////////////////////////////////////////////////////////////////////

        internal SQLiteConnectionHandle(IntPtr db, bool ownHandle)
            : this(ownHandle)
        {
#if PLATFORM_COMPACTFRAMEWORK
            lock (syncRoot)
#endif
            {
                this.ownHandle = ownHandle;
                SetHandle(db);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        private SQLiteConnectionHandle(bool ownHandle)
            : base(IntPtr.Zero)
        {
#if COUNT_HANDLE
            if (ownHandle)
                Interlocked.Increment(ref DebugData.connectionCount);
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        protected override bool ReleaseHandle()
        {
#if PLATFORM_COMPACTFRAMEWORK
            lock (syncRoot)
#endif
            {
                if (!ownHandle) return true;
            }

            try
            {
#if !PLATFORM_COMPACTFRAMEWORK
                IntPtr localHandle = Interlocked.Exchange(
                    ref handle, IntPtr.Zero);

#if SQLITE_STANDARD
                if (localHandle != IntPtr.Zero)
                    closeConnection(this, localHandle);
#else
                if (localHandle != IntPtr.Zero)
                    SQLiteBase.CloseConnection(this, localHandle);
#endif

#if !NET_COMPACT_20 && TRACE_HANDLE
                try
                {
                    Trace.WriteLine(HelperMethods.StringFormat(
                        CultureInfo.CurrentCulture,
                        "CloseConnection: {0}", localHandle)); /* throw */
                }
                catch
                {
                }
#endif
#else
                lock (syncRoot)
                {
                    if (handle != IntPtr.Zero)
                    {
                        SQLiteBase.CloseConnection(this, handle);
                        SetHandle(IntPtr.Zero);
                    }
                }
#endif
#if COUNT_HANDLE
                Interlocked.Decrement(ref DebugData.connectionCount);
#endif
#if DEBUG
                return true;
#endif
            }
#if !NET_COMPACT_20 && TRACE_HANDLE
            catch (SQLiteException e)
#else
            catch (SQLiteException)
#endif
            {
#if !NET_COMPACT_20 && TRACE_HANDLE
                try
                {
                    Trace.WriteLine(HelperMethods.StringFormat(
                        CultureInfo.CurrentCulture,
                        "CloseConnection: {0}, exception: {1}",
                        handle, e)); /* throw */
                }
                catch
                {
                }
#endif
            }
            finally
            {
#if PLATFORM_COMPACTFRAMEWORK
                lock (syncRoot)
#endif
                {
                    SetHandleAsInvalid();
                }
            }
#if DEBUG
            return false;
#else
            return true;
#endif
        }

        ///////////////////////////////////////////////////////////////////////

#if COUNT_HANDLE
        public int WasReleasedOk()
        {
            return Interlocked.Decrement(ref DebugData.connectionCount);
        }
#endif

        ///////////////////////////////////////////////////////////////////////

        public bool OwnHandle
        {
            get
            {
#if PLATFORM_COMPACTFRAMEWORK
                lock (syncRoot)
#endif
                {
                    return ownHandle;
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        public override bool IsInvalid
        {
            get
            {
#if PLATFORM_COMPACTFRAMEWORK
                lock (syncRoot)
#endif
                {
                    return (handle == IntPtr.Zero);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

#if DEBUG
        public override string ToString()
        {
#if PLATFORM_COMPACTFRAMEWORK
            lock (syncRoot)
#endif
            {
                return handle.ToString();
            }
        }
#endif
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteStatementHandle Class
    // Provides finalization support for unmanaged SQLite statements.
    internal sealed class SQLiteStatementHandle : CriticalHandle
    {
#if PLATFORM_COMPACTFRAMEWORK
        internal readonly object syncRoot = new object();
#endif

        ///////////////////////////////////////////////////////////////////////

        private SQLiteConnectionHandle cnn;

        ///////////////////////////////////////////////////////////////////////

        public static implicit operator IntPtr(SQLiteStatementHandle stmt)
        {
            if (stmt != null)
            {
#if PLATFORM_COMPACTFRAMEWORK
                lock (stmt.syncRoot)
#endif
                {
                    return stmt.handle;
                }
            }
            return IntPtr.Zero;
        }

        ///////////////////////////////////////////////////////////////////////

        internal SQLiteStatementHandle(SQLiteConnectionHandle cnn, IntPtr stmt)
            : this()
        {
#if PLATFORM_COMPACTFRAMEWORK
            lock (syncRoot)
#endif
            {
                this.cnn = cnn;
                SetHandle(stmt);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        private SQLiteStatementHandle()
            : base(IntPtr.Zero)
        {
#if COUNT_HANDLE
            Interlocked.Increment(ref DebugData.statementCount);
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        protected override bool ReleaseHandle()
        {
            try
            {
#if !PLATFORM_COMPACTFRAMEWORK
                IntPtr localHandle = Interlocked.Exchange(
                    ref handle, IntPtr.Zero);

                if (localHandle != IntPtr.Zero)
                    SQLiteBase.FinalizeStatement(cnn, localHandle);

#if !NET_COMPACT_20 && TRACE_HANDLE
                try
                {
                    Trace.WriteLine(HelperMethods.StringFormat(
                        CultureInfo.CurrentCulture,
                        "FinalizeStatement: {0}", localHandle)); /* throw */
                }
                catch
                {
                }
#endif
#else
                lock (syncRoot)
                {
                    if (handle != IntPtr.Zero)
                    {
                        SQLiteBase.FinalizeStatement(cnn, handle);
                        SetHandle(IntPtr.Zero);
                    }
                }
#endif
#if COUNT_HANDLE
                Interlocked.Decrement(ref DebugData.statementCount);
#endif
#if DEBUG
                return true;
#endif
            }
#if !NET_COMPACT_20 && TRACE_HANDLE
            catch (SQLiteException e)
#else
            catch (SQLiteException)
#endif
            {
#if !NET_COMPACT_20 && TRACE_HANDLE
                try
                {
                    Trace.WriteLine(HelperMethods.StringFormat(
                        CultureInfo.CurrentCulture,
                        "FinalizeStatement: {0}, exception: {1}",
                        handle, e)); /* throw */
                }
                catch
                {
                }
#endif
            }
            finally
            {
#if PLATFORM_COMPACTFRAMEWORK
                lock (syncRoot)
#endif
                {
                    SetHandleAsInvalid();
                }
            }
#if DEBUG
            return false;
#else
            return true;
#endif
        }

        ///////////////////////////////////////////////////////////////////////

#if COUNT_HANDLE
        public int WasReleasedOk()
        {
            return Interlocked.Decrement(ref DebugData.statementCount);
        }
#endif

        ///////////////////////////////////////////////////////////////////////

        public override bool IsInvalid
        {
            get
            {
#if PLATFORM_COMPACTFRAMEWORK
                lock (syncRoot)
#endif
                {
                    return (handle == IntPtr.Zero);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

#if DEBUG
        public override string ToString()
        {
#if PLATFORM_COMPACTFRAMEWORK
            lock (syncRoot)
#endif
            {
                return handle.ToString();
            }
        }
#endif
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteBackupHandle Class
    // Provides finalization support for unmanaged SQLite backup objects.
    internal sealed class SQLiteBackupHandle : CriticalHandle
    {
#if PLATFORM_COMPACTFRAMEWORK
        internal readonly object syncRoot = new object();
#endif

        ///////////////////////////////////////////////////////////////////////

        private SQLiteConnectionHandle cnn;

        ///////////////////////////////////////////////////////////////////////

        public static implicit operator IntPtr(SQLiteBackupHandle backup)
        {
            if (backup != null)
            {
#if PLATFORM_COMPACTFRAMEWORK
                lock (backup.syncRoot)
#endif
                {
                    return backup.handle;
                }
            }
            return IntPtr.Zero;
        }

        ///////////////////////////////////////////////////////////////////////

        internal SQLiteBackupHandle(SQLiteConnectionHandle cnn, IntPtr backup)
            : this()
        {
#if PLATFORM_COMPACTFRAMEWORK
            lock (syncRoot)
#endif
            {
                this.cnn = cnn;
                SetHandle(backup);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        private SQLiteBackupHandle()
            : base(IntPtr.Zero)
        {
#if COUNT_HANDLE
            Interlocked.Increment(ref DebugData.backupCount);
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        protected override bool ReleaseHandle()
        {
            try
            {
#if !PLATFORM_COMPACTFRAMEWORK
                IntPtr localHandle = Interlocked.Exchange(
                    ref handle, IntPtr.Zero);

                if (localHandle != IntPtr.Zero)
                    SQLiteBase.FinishBackup(cnn, localHandle);

#if !NET_COMPACT_20 && TRACE_HANDLE
                try
                {
                    Trace.WriteLine(HelperMethods.StringFormat(
                        CultureInfo.CurrentCulture,
                        "FinishBackup: {0}", localHandle)); /* throw */
                }
                catch
                {
                }
#endif
#else
                lock (syncRoot)
                {
                    if (handle != IntPtr.Zero)
                    {
                        SQLiteBase.FinishBackup(cnn, handle);
                        SetHandle(IntPtr.Zero);
                    }
                }
#endif
#if COUNT_HANDLE
                Interlocked.Decrement(ref DebugData.backupCount);
#endif
#if DEBUG
                return true;
#endif
            }
#if !NET_COMPACT_20 && TRACE_HANDLE
            catch (SQLiteException e)
#else
            catch (SQLiteException)
#endif
            {
#if !NET_COMPACT_20 && TRACE_HANDLE
                try
                {
                    Trace.WriteLine(HelperMethods.StringFormat(
                        CultureInfo.CurrentCulture,
                        "FinishBackup: {0}, exception: {1}",
                        handle, e)); /* throw */
                }
                catch
                {
                }
#endif
            }
            finally
            {
#if PLATFORM_COMPACTFRAMEWORK
                lock (syncRoot)
#endif
                {
                    SetHandleAsInvalid();
                }
            }
#if DEBUG
            return false;
#else
            return true;
#endif
        }

        ///////////////////////////////////////////////////////////////////////

#if COUNT_HANDLE
        public int WasReleasedOk()
        {
            return Interlocked.Decrement(ref DebugData.backupCount);
        }
#endif

        ///////////////////////////////////////////////////////////////////////

        public override bool IsInvalid
        {
            get
            {
#if PLATFORM_COMPACTFRAMEWORK
                lock (syncRoot)
#endif
                {
                    return (handle == IntPtr.Zero);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

#if DEBUG
        public override string ToString()
        {
#if PLATFORM_COMPACTFRAMEWORK
            lock (syncRoot)
#endif
            {
                return handle.ToString();
            }
        }
#endif
    }
    #endregion

    ///////////////////////////////////////////////////////////////////////////

    #region SQLiteBlobHandle Class
    // Provides finalization support for unmanaged SQLite blob objects.
    internal sealed class SQLiteBlobHandle : CriticalHandle
    {
#if PLATFORM_COMPACTFRAMEWORK
        internal readonly object syncRoot = new object();
#endif

        ///////////////////////////////////////////////////////////////////////

        private SQLiteConnectionHandle cnn;

        ///////////////////////////////////////////////////////////////////////

        public static implicit operator IntPtr(SQLiteBlobHandle blob)
        {
            if (blob != null)
            {
#if PLATFORM_COMPACTFRAMEWORK
                lock (blob.syncRoot)
#endif
                {
                    return blob.handle;
                }
            }
            return IntPtr.Zero;
        }

        ///////////////////////////////////////////////////////////////////////

        internal SQLiteBlobHandle(SQLiteConnectionHandle cnn, IntPtr blob)
            : this()
        {
#if PLATFORM_COMPACTFRAMEWORK
            lock (syncRoot)
#endif
            {
                this.cnn = cnn;
                SetHandle(blob);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        private SQLiteBlobHandle()
            : base(IntPtr.Zero)
        {
#if COUNT_HANDLE
            Interlocked.Increment(ref DebugData.blobCount);
#endif
        }

        ///////////////////////////////////////////////////////////////////////

        protected override bool ReleaseHandle()
        {
            try
            {
#if !PLATFORM_COMPACTFRAMEWORK
                IntPtr localHandle = Interlocked.Exchange(
                    ref handle, IntPtr.Zero);

                if (localHandle != IntPtr.Zero)
                    SQLiteBase.CloseBlob(cnn, localHandle);

#if !NET_COMPACT_20 && TRACE_HANDLE
                try
                {
                    Trace.WriteLine(HelperMethods.StringFormat(
                        CultureInfo.CurrentCulture,
                        "CloseBlob: {0}", localHandle)); /* throw */
                }
                catch
                {
                }
#endif
#else
                lock (syncRoot)
                {
                    if (handle != IntPtr.Zero)
                    {
                        SQLiteBase.CloseBlob(cnn, handle);
                        SetHandle(IntPtr.Zero);
                    }
                }
#endif
#if COUNT_HANDLE
                Interlocked.Decrement(ref DebugData.blobCount);
#endif
#if DEBUG
                return true;
#endif
            }
#if !NET_COMPACT_20 && TRACE_HANDLE
            catch (SQLiteException e)
#else
            catch (SQLiteException)
#endif
            {
#if !NET_COMPACT_20 && TRACE_HANDLE
                try
                {
                    Trace.WriteLine(HelperMethods.StringFormat(
                        CultureInfo.CurrentCulture,
                        "CloseBlob: {0}, exception: {1}",
                        handle, e)); /* throw */
                }
                catch
                {
                }
#endif
            }
            finally
            {
#if PLATFORM_COMPACTFRAMEWORK
                lock (syncRoot)
#endif
                {
                    SetHandleAsInvalid();
                }
            }
#if DEBUG
            return false;
#else
            return true;
#endif
        }

        ///////////////////////////////////////////////////////////////////////

#if COUNT_HANDLE
        public int WasReleasedOk()
        {
            return Interlocked.Decrement(ref DebugData.blobCount);
        }
#endif

        ///////////////////////////////////////////////////////////////////////

        public override bool IsInvalid
        {
            get
            {
#if PLATFORM_COMPACTFRAMEWORK
                lock (syncRoot)
#endif
                {
                    return (handle == IntPtr.Zero);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

#if DEBUG
        public override string ToString()
        {
#if PLATFORM_COMPACTFRAMEWORK
            lock (syncRoot)
#endif
            {
                return handle.ToString();
            }
        }
#endif
    }
    #endregion
}
