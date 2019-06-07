/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 *
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
    using System;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;

    /// <summary>
    /// Event data for logging event handlers.
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        /// <summary>
        /// The error code.  The type of this object value should be
        /// <see cref="Int32" /> or <see cref="SQLiteErrorCode" />.
        /// </summary>
        public readonly object ErrorCode;

        /// <summary>
        /// SQL statement text as the statement first begins executing
        /// </summary>
        public readonly string Message;

        /// <summary>
        /// Extra data associated with this event, if any.
        /// </summary>
        public readonly object Data;

        /// <summary>
        /// Constructs the object.
        /// </summary>
        /// <param name="pUserData">Should be null.</param>
        /// <param name="errorCode">
        /// The error code.  The type of this object value should be
        /// <see cref="Int32" /> or <see cref="SQLiteErrorCode" />.
        /// </param>
        /// <param name="message">The error message, if any.</param>
        /// <param name="data">The extra data, if any.</param>
        internal LogEventArgs(
            IntPtr pUserData,
            object errorCode,
            string message,
            object data
            )
        {
            ErrorCode = errorCode;
            Message = message;
            Data = data;
        }
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Raised when a log event occurs.
    /// </summary>
    /// <param name="sender">The current connection</param>
    /// <param name="e">Event arguments of the trace</param>
    public delegate void SQLiteLogEventHandler(object sender, LogEventArgs e);

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Manages the SQLite custom logging functionality and the associated
    /// callback for the whole process.
    /// </summary>
    public static class SQLiteLog
    {
        /// <summary>
        /// Object used to synchronize access to the static instance data
        /// for this class.
        /// </summary>
        private static object syncRoot = new object();

        ///////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
        /// <summary>
        /// Member variable to store the AppDomain.DomainUnload event handler.
        /// </summary>
        private static EventHandler _domainUnload;
#endif

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Member variable to store the application log handler to call.
        /// </summary>
        private static event SQLiteLogEventHandler _handlers;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The default log event handler.
        /// </summary>
        private static SQLiteLogEventHandler _defaultHandler;

        ///////////////////////////////////////////////////////////////////////

#if !USE_INTEROP_DLL || !INTEROP_LOG
        /// <summary>
        /// The log callback passed to native SQLite engine.  This must live
        /// as long as the SQLite library has a pointer to it.
        /// </summary>
        private static SQLiteLogCallback _callback;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The base SQLite object to interop with.
        /// </summary>
        private static SQLiteBase _sql;
#endif

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This will be non-zero if an attempt was already made to initialize
        /// the (managed) logging subsystem.
        /// </summary>
        private static int _attemptedInitialize;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This will be non-zero if logging is currently enabled.
        /// </summary>
        private static bool _enabled;

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes the SQLite logging facilities.
        /// </summary>
        public static void Initialize()
        {
            Initialize(null);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes the SQLite logging facilities.
        /// </summary>
        /// <param name="className">
        /// The name of the managed class that called this method.  This
        /// parameter may be null.
        /// </param>
        internal static void Initialize(
            string className
            )
        {
            //
            // NOTE: First, check if the managed logging subsystem is always
            //       supposed to at least attempt to initialize itself.  In
            //       order to do this, several fairly complex steps must be
            //       taken, including calling a P/Invoke (interop) method;
            //       therefore, by default, attempt to perform these steps
            //       once.
            //
            if (UnsafeNativeMethods.GetSettingValue(
                    "Initialize_SQLiteLog", null) == null)
            {
                if (Interlocked.Increment(ref _attemptedInitialize) > 1)
                {
                    Interlocked.Decrement(ref _attemptedInitialize);
                    return;
                }
            }

            ///////////////////////////////////////////////////////////////////

            //
            // BUFXIX: We cannot initialize the logging interface if the SQLite
            //         core library has already been initialized anywhere in
            //         the process (see ticket [2ce0870fad]).
            //
            if (SQLite3.StaticIsInitialized())
                return;

            ///////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
            //
            // BUGFIX: To avoid nasty situations where multiple AppDomains are
            //         attempting to initialize and/or shutdown what is really
            //         a shared native resource (i.e. the SQLite core library
            //         is loaded per-process and has only one logging callback,
            //         not one per-AppDomain, which it knows nothing about),
            //         prevent all non-default AppDomains from registering a
            //         log handler unless the "Force_SQLiteLog" environment
            //         variable is used to manually override this safety check.
            //
            if (!AppDomain.CurrentDomain.IsDefaultAppDomain() &&
                UnsafeNativeMethods.GetSettingValue("Force_SQLiteLog", null) == null)
            {
                return;
            }
#endif

            ///////////////////////////////////////////////////////////////////

            lock (syncRoot)
            {
#if !PLATFORM_COMPACTFRAMEWORK
                //
                // NOTE: Add an event handler for the DomainUnload event so
                //       that we can unhook our logging managed function
                //       pointer from the native SQLite code prior to it
                //       being invalidated.
                //
                // BUGFIX: Make sure this event handler is only added one
                //         time (per-AppDomain).
                //
                if (_domainUnload == null)
                {
                    _domainUnload = new EventHandler(DomainUnload);
                    AppDomain.CurrentDomain.DomainUnload += _domainUnload;
                }
#endif

                ///////////////////////////////////////////////////////////////

#if USE_INTEROP_DLL && INTEROP_LOG
                //
                // NOTE: Attempt to setup interop assembly log callback.
                //       This may fail, e.g. if the SQLite core library
                //       has somehow been initialized.  An exception will
                //       be raised in that case.
                //
                SQLiteErrorCode rc = SQLite3.ConfigureLogForInterop(
                    className);

                if (rc != SQLiteErrorCode.Ok)
                {
                    throw new SQLiteException(rc,
                        "Failed to configure interop assembly logging.");
                }
#else
                //
                // NOTE: Create an instance of the SQLite wrapper class.
                //
                if (_sql == null)
                {
                    _sql = new SQLite3(
                        SQLiteDateFormats.Default, DateTimeKind.Unspecified,
                        null, IntPtr.Zero, null, false);
                }

                //
                // NOTE: Create a single "global" (i.e. per-process) callback
                //       to register with SQLite.  This callback will pass the
                //       event on to any registered handler.  We only want to
                //       do this once.
                //
                if (_callback == null)
                {
                    _callback = new SQLiteLogCallback(LogCallback);

                    SQLiteErrorCode rc = _sql.SetLogCallback(_callback);

                    if (rc != SQLiteErrorCode.Ok)
                    {
                        throw new SQLiteException(rc,
                            "Failed to configure managed assembly logging.");
                    }
                }
#endif

                ///////////////////////////////////////////////////////////////

                //
                // NOTE: Logging is enabled by default unless the configuration
                //       setting "Disable_SQLiteLog" is present.
                //
                if (UnsafeNativeMethods.GetSettingValue(
                        "Disable_SQLiteLog", null) == null)
                {
                    _enabled = true;
                }

                ///////////////////////////////////////////////////////////////

                //
                // NOTE: For now, always setup the default log event handler.
                //
                AddDefaultHandler();
            }
        }

        ///////////////////////////////////////////////////////////////////////

#if !PLATFORM_COMPACTFRAMEWORK
        /// <summary>
        /// Handles the AppDomain being unloaded.
        /// </summary>
        /// <param name="sender">Should be null.</param>
        /// <param name="e">The data associated with this event.</param>
        private static void DomainUnload(
            object sender,
            EventArgs e
            )
        {
            lock (syncRoot)
            {
                //
                // NOTE: Remove the default log event handler.
                //
                RemoveDefaultHandler();

                //
                // NOTE: Disable logging.  If necessary, it can be re-enabled
                //       later by the Initialize method.
                //
                _enabled = false;

#if !USE_INTEROP_DLL || !INTEROP_LOG
                //
                // BUGBUG: This will cause serious problems if other AppDomains
                //         have any open SQLite connections; however, there is
                //         currently no way around this limitation.
                //
                if (_sql != null)
                {
                    SQLiteErrorCode rc = _sql.Shutdown();

                    if (rc != SQLiteErrorCode.Ok)
                        throw new SQLiteException(rc,
                            "Failed to shutdown interface.");

                    rc = _sql.SetLogCallback(null);

                    if (rc != SQLiteErrorCode.Ok)
                        throw new SQLiteException(rc,
                            "Failed to shutdown logging.");
                }

                //
                // BUGFIX: Make sure to reset the callback for next time.  This
                //         must be done after it has been succesfully removed
                //         as logging callback by the SQLite core library as we
                //         cannot allow native code to refer to a delegate that
                //         has been garbage collected.
                //
                if (_callback != null)
                {
                    _callback = null;
                }
#endif

                //
                // NOTE: Remove the event handler for the DomainUnload event
                //       that we added earlier.
                //
                if (_domainUnload != null)
                {
                    AppDomain.CurrentDomain.DomainUnload -= _domainUnload;
                    _domainUnload = null;
                }
            }
        }
#endif

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This event is raised whenever SQLite raises a logging event.
        /// Note that this should be set as one of the first things in the
        /// application.
        /// </summary>
        public static event SQLiteLogEventHandler Log
        {
            add
            {
                lock (syncRoot)
                {
                    // Remove any copies of this event handler from registered
                    // list.  This essentially means that a handler will be
                    // called only once no matter how many times it is added.
                    _handlers -= value;

                    // Add this to the list of event handlers.
                    _handlers += value;
                }
            }
            remove
            {
                lock (syncRoot)
                {
                    _handlers -= value;
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// If this property is true, logging is enabled; otherwise, logging is
        /// disabled.  When logging is disabled, no logging events will fire.
        /// </summary>
        public static bool Enabled
        {
            get { lock (syncRoot) { return _enabled; } }
            set { lock (syncRoot) { _enabled = value; } }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Log a message to all the registered log event handlers without going
        /// through the SQLite library.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void LogMessage(
            string message
            )
        {
            LogMessage(null, message);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Log a message to all the registered log event handlers without going
        /// through the SQLite library.
        /// </summary>
        /// <param name="errorCode">The SQLite error code.</param>
        /// <param name="message">The message to be logged.</param>
        public static void LogMessage(
            SQLiteErrorCode errorCode,
            string message
            )
        {
            LogMessage((object)errorCode, message);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Log a message to all the registered log event handlers without going
        /// through the SQLite library.
        /// </summary>
        /// <param name="errorCode">The integer error code.</param>
        /// <param name="message">The message to be logged.</param>
        public static void LogMessage(
            int errorCode,
            string message
            )
        {
            LogMessage((object)errorCode, message);
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Log a message to all the registered log event handlers without going
        /// through the SQLite library.
        /// </summary>
        /// <param name="errorCode">
        /// The error code.  The type of this object value should be
        /// System.Int32 or SQLiteErrorCode.
        /// </param>
        /// <param name="message">The message to be logged.</param>
        private static void LogMessage(
            object errorCode,
            string message
            )
        {
            bool enabled;
            SQLiteLogEventHandler handlers;

            lock (syncRoot)
            {
                enabled = _enabled;

                if (_handlers != null)
                    handlers = _handlers.Clone() as SQLiteLogEventHandler;
                else
                    handlers = null;
            }

            if (enabled && (handlers != null))
                handlers(null, new LogEventArgs(
                    IntPtr.Zero, errorCode, message, null));
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates and initializes the default log event handler.
        /// </summary>
        private static void InitializeDefaultHandler()
        {
            lock (syncRoot)
            {
                if (_defaultHandler == null)
                    _defaultHandler = new SQLiteLogEventHandler(LogEventHandler);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Adds the default log event handler to the list of handlers.
        /// </summary>
        public static void AddDefaultHandler()
        {
            InitializeDefaultHandler();
            Log += _defaultHandler;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Removes the default log event handler from the list of handlers.
        /// </summary>
        public static void RemoveDefaultHandler()
        {
            InitializeDefaultHandler();
            Log -= _defaultHandler;
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Internal proxy function that calls any registered application log
        /// event handlers.
        ///
        /// WARNING: This method is used more-or-less directly by native code,
        ///          do not modify its type signature.
        /// </summary>
        /// <param name="pUserData">
        /// The extra data associated with this message, if any.
        /// </param>
        /// <param name="errorCode">
        /// The error code associated with this message.
        /// </param>
        /// <param name="pMessage">
        /// The message string to be logged.
        /// </param>
        private static void LogCallback(
            IntPtr pUserData,
            int errorCode,
            IntPtr pMessage
            )
        {
            bool enabled;
            SQLiteLogEventHandler handlers;

            lock (syncRoot)
            {
                enabled = _enabled;

                if (_handlers != null)
                    handlers = _handlers.Clone() as SQLiteLogEventHandler;
                else
                    handlers = null;
            }

            if (enabled && (handlers != null))
                handlers(null, new LogEventArgs(pUserData, errorCode,
                    SQLiteBase.UTF8ToString(pMessage, -1), null));
        }

        ///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Default logger.  Currently, uses the Trace class (i.e. sends events
        /// to the current trace listeners, if any).
        /// </summary>
        /// <param name="sender">Should be null.</param>
        /// <param name="e">The data associated with this event.</param>
        private static void LogEventHandler(
            object sender,
            LogEventArgs e
            )
        {
#if !NET_COMPACT_20
            if (e == null)
                return;

            string message = e.Message;

            if (message == null)
            {
                message = "<null>";
            }
            else
            {
                message = message.Trim();

                if (message.Length == 0)
                    message = "<empty>";
            }

            object errorCode = e.ErrorCode;
            string type = "error";

            if ((errorCode is SQLiteErrorCode) || (errorCode is int))
            {
                SQLiteErrorCode rc = (SQLiteErrorCode)(int)errorCode;

                rc &= SQLiteErrorCode.NonExtendedMask;

                if (rc == SQLiteErrorCode.Ok)
                {
                    type = "message";
                }
                else if (rc == SQLiteErrorCode.Notice)
                {
                    type = "notice";
                }
                else if (rc == SQLiteErrorCode.Warning)
                {
                    type = "warning";
                }
                else if ((rc == SQLiteErrorCode.Row) ||
                    (rc == SQLiteErrorCode.Done))
                {
                    type = "data";
                }
            }
            else if (errorCode == null)
            {
                type = "trace";
            }

            if ((errorCode != null) &&
                !Object.ReferenceEquals(errorCode, String.Empty))
            {
                Trace.WriteLine(HelperMethods.StringFormat(
                    CultureInfo.CurrentCulture, "SQLite {0} ({1}): {2}",
                    type, errorCode, message));
            }
            else
            {
                Trace.WriteLine(HelperMethods.StringFormat(
                    CultureInfo.CurrentCulture, "SQLite {0}: {1}",
                    type, message));
            }
#endif
        }
    }
}
