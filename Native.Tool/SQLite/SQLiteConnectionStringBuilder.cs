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
  using System.ComponentModel;
  using System.Collections;
  using System.Globalization;
  using System.Reflection;

#if !PLATFORM_COMPACTFRAMEWORK
  /// <summary>
  /// SQLite implementation of DbConnectionStringBuilder.
  /// </summary>
  [DefaultProperty("DataSource")]
  [DefaultMember("Item")]
  public sealed class SQLiteConnectionStringBuilder : DbConnectionStringBuilder
  {
    /// <summary>
    /// Properties of this class
    /// </summary>
    private Hashtable _properties;

    /// <overloads>
    /// Constructs a new instance of the class
    /// </overloads>
    /// <summary>
    /// Default constructor
    /// </summary>
    public SQLiteConnectionStringBuilder()
    {
      Initialize(null);
    }

    /// <summary>
    /// Constructs a new instance of the class using the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to parse</param>
    public SQLiteConnectionStringBuilder(string connectionString)
    {
      Initialize(connectionString);
    }

    /// <summary>
    /// Private initializer, which assigns the connection string and resets the builder
    /// </summary>
    /// <param name="cnnString">The connection string to assign</param>
    private void Initialize(string cnnString)
    {
      _properties = new Hashtable(StringComparer.OrdinalIgnoreCase);
      try
      {
        base.GetProperties(_properties);
      }
      catch(NotImplementedException)
      {
        FallbackGetProperties(_properties);
      }

      if (String.IsNullOrEmpty(cnnString) == false)
        ConnectionString = cnnString;
    }

    /// <summary>
    /// Gets/Sets the default version of the SQLite engine to instantiate.  Currently the only valid value is 3, indicating version 3 of the sqlite library.
    /// </summary>
    [Browsable(true)]
    [DefaultValue(3)]
    public int Version
    {
      get
      {
        object value;
        TryGetValue("version", out value);
        return Convert.ToInt32(value, CultureInfo.CurrentCulture);
      }
      set
      {
        if (value != 3)
          throw new NotSupportedException();

        this["version"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the synchronization mode (file flushing) of the connection string.  Default is "Normal".
    /// </summary>
    [DisplayName("Synchronous")]
    [Browsable(true)]
    [DefaultValue(SynchronizationModes.Normal)]
    public SynchronizationModes SyncMode
    {
      get
      {
        object value;
        TryGetValue("synchronous", out value);
        if (value is string)
          return (SynchronizationModes)TypeDescriptor.GetConverter(typeof(SynchronizationModes)).ConvertFrom(value);
        else return (SynchronizationModes)value;
      }
      set
      {
        this["synchronous"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the encoding for the connection string.  The default is "False" which indicates UTF-8 encoding.
    /// </summary>
    [DisplayName("Use UTF-16 Encoding")]
    [Browsable(true)]
    [DefaultValue(false)]
    public bool UseUTF16Encoding
    {
      get
      {
        object value;
        TryGetValue("useutf16encoding", out value);
        return SQLiteConvert.ToBoolean(value);
      }
      set
      {
        this["useutf16encoding"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets whether or not to use connection pooling.  The default is "False"
    /// </summary>
    [Browsable(true)]
    [DefaultValue(false)]
    public bool Pooling
    {
      get
      {
        object value;
        TryGetValue("pooling", out value);
        return SQLiteConvert.ToBoolean(value);
      }
      set
      {
        this["pooling"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets whethor not to store GUID's in binary format.  The default is True
    /// which saves space in the database.
    /// </summary>
    [DisplayName("Binary GUID")]
    [Browsable(true)]
    [DefaultValue(true)]
    public bool BinaryGUID
    {
      get
      {
        object value;
        TryGetValue("binaryguid", out value);
        return SQLiteConvert.ToBoolean(value);
      }
      set
      {
        this["binaryguid"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the filename to open on the connection string.
    /// </summary>
    [DisplayName("Data Source")]
    [Browsable(true)]
    [DefaultValue("")]
    public string DataSource
    {
      get
      {
        object value;
        TryGetValue("data source", out value);
        return (value != null) ? value.ToString() : null;
      }
      set
      {
        this["data source"] = value;
      }
    }

    /// <summary>
    /// An alternate to the data source property
    /// </summary>
    [DisplayName("URI")]
    [Browsable(true)]
    [DefaultValue(null)]
    public string Uri
    {
      get
      {
        object value;
        TryGetValue("uri", out value);
        return (value != null) ? value.ToString() : null;
      }
      set
      {
        this["uri"] = value;
      }
    }

    /// <summary>
    /// An alternate to the data source property that uses the SQLite URI syntax.
    /// </summary>
    [DisplayName("Full URI")]
    [Browsable(true)]
    [DefaultValue(null)]
    public string FullUri
    {
      get
      {
        object value;
        TryGetValue("fulluri", out value);
        return (value != null) ? value.ToString() : null;
      }
      set
      {
        this["fulluri"] = value;
      }
    }

    /// <summary>
    /// Gets/sets the default command timeout for newly-created commands.  This is especially useful for
    /// commands used internally such as inside a SQLiteTransaction, where setting the timeout is not possible.
    /// </summary>
    [DisplayName("Default Timeout")]
    [Browsable(true)]
    [DefaultValue(30)]
    public int DefaultTimeout
    {
      get
      {
        object value;
        TryGetValue("default timeout", out value);
        return Convert.ToInt32(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["default timeout"] = value;
      }
    }

    /// <summary>
    /// Gets/sets the busy timeout to use with the SQLite core library.
    /// </summary>
    [DisplayName("Busy Timeout")]
    [Browsable(true)]
    [DefaultValue(0)]
    public int BusyTimeout
    {
      get
      {
        object value;
        TryGetValue("busytimeout", out value);
        return Convert.ToInt32(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["busytimeout"] = value;
      }
    }

    /// <summary>
    /// <b>EXPERIMENTAL</b> --
    /// The wait timeout to use with
    /// <see cref="SQLiteConnection.WaitForEnlistmentReset" /> method.
    /// This is only used when waiting for the enlistment to be reset
    /// prior to enlisting in a transaction, and then only when the
    /// appropriate connection flag is set.
    /// </summary>
    [DisplayName("Wait Timeout")]
    [Browsable(true)]
    [DefaultValue(30000)]
    public int WaitTimeout
    {
      get
      {
        object value;
        TryGetValue("waittimeout", out value);
        return Convert.ToInt32(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["waittimeout"] = value;
      }
    }

    /// <summary>
    /// Gets/sets the maximum number of retries when preparing SQL to be executed.
    /// This normally only applies to preparation errors resulting from the database
    /// schema being changed.
    /// </summary>
    [DisplayName("Prepare Retries")]
    [Browsable(true)]
    [DefaultValue(3)]
    public int PrepareRetries
    {
        get
        {
            object value;
            TryGetValue("prepareretries", out value);
            return Convert.ToInt32(value, CultureInfo.CurrentCulture);
        }
        set
        {
            this["prepareretries"] = value;
        }
    }

    /// <summary>
    /// Gets/sets the approximate number of virtual machine instructions between
    /// progress events.  In order for progress events to actually fire, the event
    /// handler must be added to the <see cref="SQLiteConnection.Progress" /> event
    /// as well.
    /// </summary>
    [DisplayName("Progress Ops")]
    [Browsable(true)]
    [DefaultValue(0)]
    public int ProgressOps
    {
        get
        {
            object value;
            TryGetValue("progressops", out value);
            return Convert.ToInt32(value, CultureInfo.CurrentCulture);
        }
        set
        {
            this["progressops"] = value;
        }
    }

    /// <summary>
    /// Determines whether or not the connection will automatically participate
    /// in the current distributed transaction (if one exists)
    /// </summary>
    [Browsable(true)]
    [DefaultValue(true)]
    public bool Enlist
    {
      get
      {
        object value;
        TryGetValue("enlist", out value);
        return SQLiteConvert.ToBoolean(value);
      }
      set
      {
        this["enlist"] = value;
      }
    }

    /// <summary>
    /// If set to true, will throw an exception if the database specified in the connection
    /// string does not exist.  If false, the database will be created automatically.
    /// </summary>
    [DisplayName("Fail If Missing")]
    [Browsable(true)]
    [DefaultValue(false)]
    public bool FailIfMissing
    {
      get
      {
        object value;
        TryGetValue("failifmissing", out value);
        return SQLiteConvert.ToBoolean(value);
      }
      set
      {
        this["failifmissing"] = value;
      }
    }

    /// <summary>
    /// If enabled, uses the legacy 3.xx format for maximum compatibility, but results in larger
    /// database sizes.
    /// </summary>
    [DisplayName("Legacy Format")]
    [Browsable(true)]
    [DefaultValue(false)]
    public bool LegacyFormat
    {
      get
      {
        object value;
        TryGetValue("legacy format", out value);
        return SQLiteConvert.ToBoolean(value);
      }
      set
      {
        this["legacy format"] = value;
      }
    }

    /// <summary>
    /// When enabled, the database will be opened for read-only access and writing will be disabled.
    /// </summary>
    [DisplayName("Read Only")]
    [Browsable(true)]
    [DefaultValue(false)]
    public bool ReadOnly
    {
      get
      {
        object value;
        TryGetValue("read only", out value);
        return SQLiteConvert.ToBoolean(value);
      }
      set
      {
        this["read only"] = value;
      }
    }

    /// <summary>
    /// Gets/sets the database encryption password
    /// </summary>
    [Browsable(true)]
    [PasswordPropertyText(true)]
    [DefaultValue("")]
    public string Password
    {
      get
      {
        object value;
        TryGetValue("password", out value);
        return (value != null) ? value.ToString() : null;
      }
      set
      {
        this["password"] = value;
      }
    }

    /// <summary>
    /// Gets/sets the database encryption hexadecimal password
    /// </summary>
    [DisplayName("Hexadecimal Password")]
    [Browsable(true)]
    [PasswordPropertyText(true)]
    [DefaultValue(null)]
    public byte[] HexPassword
    {
        get
        {
            object value;

            if (TryGetValue("hexpassword", out value))
            {
                if (value is string)
                    return SQLiteConnection.FromHexString((string)value);
                else if (value != null)
                    return (byte[])value;
            }

            return null;
        }
        set
        {
            this["hexpassword"] = SQLiteConnection.ToHexString(value);
        }
    }

    /// <summary>
    /// Gets/Sets the page size for the connection.
    /// </summary>
    [DisplayName("Page Size")]
    [Browsable(true)]
    [DefaultValue(4096)]
    public int PageSize
    {
      get
      {
        object value;
        TryGetValue("page size", out value);
        return Convert.ToInt32(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["page size"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the maximum number of pages the database may hold
    /// </summary>
    [DisplayName("Maximum Page Count")]
    [Browsable(true)]
    [DefaultValue(0)]
    public int MaxPageCount
    {
      get
      {
        object value;
        TryGetValue("max page count", out value);
        return Convert.ToInt32(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["max page count"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the cache size for the connection.
    /// </summary>
    [DisplayName("Cache Size")]
    [Browsable(true)]
    [DefaultValue(-2000)]
    public int CacheSize
    {
      get
      {
        object value;
        TryGetValue("cache size", out value);
        return Convert.ToInt32(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["cache size"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the DateTime format for the connection.
    /// </summary>
    [DisplayName("DateTime Format")]
    [Browsable(true)]
    [DefaultValue(SQLiteDateFormats.Default)]
    public SQLiteDateFormats DateTimeFormat
    {
        get
        {
            object value;

            if (TryGetValue("datetimeformat", out value))
            {
                if (value is SQLiteDateFormats)
                    return (SQLiteDateFormats)value;
                else if (value != null)
                    return (SQLiteDateFormats)TypeDescriptor.GetConverter(
                        typeof(SQLiteDateFormats)).ConvertFrom(value);
            }

            return SQLiteDateFormats.Default;
        }
        set
        {
            this["datetimeformat"] = value;
        }
    }

    /// <summary>
    /// Gets/Sets the DateTime kind for the connection.
    /// </summary>
    [DisplayName("DateTime Kind")]
    [Browsable(true)]
    [DefaultValue(DateTimeKind.Unspecified)]
    public DateTimeKind DateTimeKind
    {
        get
        {
            object value;

            if (TryGetValue("datetimekind", out value))
            {
                if (value is DateTimeKind)
                    return (DateTimeKind)value;
                else if (value != null)
                    return (DateTimeKind)TypeDescriptor.GetConverter(
                        typeof(DateTimeKind)).ConvertFrom(value);
            }

            return DateTimeKind.Unspecified;
        }
        set
        {
            this["datetimekind"] = value;
        }
    }

    /// <summary>
    /// Gets/sets the DateTime format string used for formatting
    /// and parsing purposes.
    /// </summary>
    [DisplayName("DateTime Format String")]
    [Browsable(true)]
    [DefaultValue(null)]
    public string DateTimeFormatString
    {
        get
        {
            object value;

            if (TryGetValue("datetimeformatstring", out value))
            {
                if (value is string)
                    return (string)value;
                else if (value != null)
                    return value.ToString();
            }

            return null;
        }
        set
        {
            this["datetimeformatstring"] = value;
        }
    }

    /// <summary>
    /// Gets/Sets the placeholder base schema name used for
    /// .NET Framework compatibility purposes.
    /// </summary>
    [DisplayName("Base Schema Name")]
    [Browsable(true)]
    [DefaultValue(SQLiteConnection.DefaultBaseSchemaName)]
    public string BaseSchemaName
    {
        get
        {
            object value;

            if (TryGetValue("baseschemaname", out value))
            {
                if (value is string)
                    return (string)value;
                else if (value != null)
                    return value.ToString();
            }

            return null;
        }
        set
        {
            this["baseschemaname"] = value;
        }
    }

    /// <summary>
    /// Determines how SQLite handles the transaction journal file.
    /// </summary>
    [Browsable(true)]
    [DefaultValue(SQLiteJournalModeEnum.Default)]
    [DisplayName("Journal Mode")]
    public SQLiteJournalModeEnum JournalMode
    {
      get
      {
        object value;
        TryGetValue("journal mode", out value);
        if (value is string)
          return (SQLiteJournalModeEnum)TypeDescriptor.GetConverter(typeof(SQLiteJournalModeEnum)).ConvertFrom(value);
        else
          return (SQLiteJournalModeEnum)value;
      }
      set
      {
        this["journal mode"] = value;
      }
    }

    /// <summary>
    /// Sets the default isolation level for transactions on the connection.
    /// </summary>
    [Browsable(true)]
    [DefaultValue(IsolationLevel.Serializable)]
    [DisplayName("Default Isolation Level")]
    public IsolationLevel DefaultIsolationLevel
    {
      get
      {
        object value;
        TryGetValue("default isolationlevel", out value);
        if (value is string)
          return (IsolationLevel)TypeDescriptor.GetConverter(typeof(IsolationLevel)).ConvertFrom(value);
        else
          return (IsolationLevel)value;
      }
      set
      {
        this["default isolationlevel"] = value;
      }
    }

    /// <summary>
    /// Gets/sets the default database type for the connection.
    /// </summary>
    [DisplayName("Default Database Type")]
    [Browsable(true)]
    [DefaultValue(SQLiteConnection.BadDbType)]
    public DbType DefaultDbType
    {
        get
        {
            object value;

            if (TryGetValue("defaultdbtype", out value))
            {
                if (value is string)
                    return (DbType)TypeDescriptor.GetConverter(
                        typeof(DbType)).ConvertFrom(value);
                else if (value != null)
                    return (DbType)value;
            }

            return SQLiteConnection.BadDbType;
        }
        set
        {
            this["defaultdbtype"] = value;
        }
    }

    /// <summary>
    /// Gets/sets the default type name for the connection.
    /// </summary>
    [DisplayName("Default Type Name")]
    [Browsable(true)]
    [DefaultValue(null)]
    public string DefaultTypeName
    {
        get
        {
            object value;
            TryGetValue("defaulttypename", out value);
            return (value != null) ? value.ToString() : null;
        }
        set
        {
            this["defaulttypename"] = value;
        }
    }

    /// <summary>
    /// Gets/sets the VFS name for the connection.
    /// </summary>
    [DisplayName("VFS Name")]
    [Browsable(true)]
    [DefaultValue(null)]
    public string VfsName
    {
        get
        {
            object value;
            TryGetValue("vfsname", out value);
            return (value != null) ? value.ToString() : null;
        }
        set
        {
            this["vfsname"] = value;
        }
    }

    /// <summary>
    /// If enabled, use foreign key constraints
    /// </summary>
    [DisplayName("Foreign Keys")]
    [Browsable(true)]
    [DefaultValue(false)]
    public bool ForeignKeys
    {
        get
        {
            object value;
            TryGetValue("foreign keys", out value);
            return SQLiteConvert.ToBoolean(value);
        }
        set
        {
            this["foreign keys"] = value;
        }
    }

    /// <summary>
    /// Enable or disable the recursive trigger capability.
    /// </summary>
    [DisplayName("Recursive Triggers")]
    [Browsable(true)]
    [DefaultValue(false)]
    public bool RecursiveTriggers
    {
        get
        {
            object value;
            TryGetValue("recursive triggers", out value);
            return SQLiteConvert.ToBoolean(value);
        }
        set
        {
            this["recursive triggers"] = value;
        }
    }

    /// <summary>
    /// If non-null, this is the version of ZipVFS to use.  This requires the
    /// System.Data.SQLite interop assembly -AND- primary managed assembly to
    /// be compiled with the INTEROP_INCLUDE_ZIPVFS option; otherwise, this
    /// property does nothing.
    /// </summary>
    [DisplayName("ZipVFS Version")]
    [Browsable(true)]
    [DefaultValue(null)]
    public string ZipVfsVersion
    {
        get
        {
            object value;
            TryGetValue("zipvfsversion", out value);
            return (value != null) ? value.ToString() : null;
        }
        set
        {
            this["zipvfsversion"] = value;
        }
    }

    /// <summary>
    /// Gets/Sets the extra behavioral flags.
    /// </summary>
    [Browsable(true)]
    [DefaultValue(SQLiteConnectionFlags.Default)]
    public SQLiteConnectionFlags Flags
    {
        get
        {
            object value;

            if (TryGetValue("flags", out value))
            {
                if (value is SQLiteConnectionFlags)
                    return (SQLiteConnectionFlags)value;
                else if (value != null)
                    return (SQLiteConnectionFlags)TypeDescriptor.GetConverter(
                        typeof(SQLiteConnectionFlags)).ConvertFrom(value);
            }

            return SQLiteConnectionFlags.Default;
        }
        set
        {
            this["flags"] = value;
        }
    }

    /// <summary>
    /// If enabled, apply the default connection settings to opened databases.
    /// </summary>
    [DisplayName("Set Defaults")]
    [Browsable(true)]
    [DefaultValue(true)]
    public bool SetDefaults
    {
        get
        {
            object value;
            TryGetValue("setdefaults", out value);
            return SQLiteConvert.ToBoolean(value);
        }
        set
        {
            this["setdefaults"] = value;
        }
    }

    /// <summary>
    /// If enabled, attempt to resolve the provided data source file name to a
    /// full path before opening.
    /// </summary>
    [DisplayName("To Full Path")]
    [Browsable(true)]
    [DefaultValue(true)]
    public bool ToFullPath
    {
        get
        {
            object value;
            TryGetValue("tofullpath", out value);
            return SQLiteConvert.ToBoolean(value);
        }
        set
        {
            this["tofullpath"] = value;
        }
    }

    /// <summary>
    /// If enabled, skip using the configured default connection flags.
    /// </summary>
    [DisplayName("No Default Flags")]
    [Browsable(true)]
    [DefaultValue(false)]
    public bool NoDefaultFlags
    {
        get
        {
            object value;
            TryGetValue("nodefaultflags", out value);
            return SQLiteConvert.ToBoolean(value);
        }
        set
        {
            this["nodefaultflags"] = value;
        }
    }

    /// <summary>
    /// If enabled, skip using the configured shared connection flags.
    /// </summary>
    [DisplayName("No Shared Flags")]
    [Browsable(true)]
    [DefaultValue(false)]
    public bool NoSharedFlags
    {
        get
        {
            object value;
            TryGetValue("nosharedflags", out value);
            return SQLiteConvert.ToBoolean(value);
        }
        set
        {
            this["nosharedflags"] = value;
        }
    }

    /// <summary>
    /// Helper function for retrieving values from the connectionstring
    /// </summary>
    /// <param name="keyword">The keyword to retrieve settings for</param>
    /// <param name="value">The resulting parameter value</param>
    /// <returns>Returns true if the value was found and returned</returns>
    public override bool TryGetValue(string keyword, out object value)
    {
      bool b = base.TryGetValue(keyword, out value);

      if (!_properties.ContainsKey(keyword)) return b;

      PropertyDescriptor pd = _properties[keyword] as PropertyDescriptor;

      if (pd == null) return b;

      // Attempt to coerce the value into something more solid
      if (b)
      {
        if (pd.PropertyType == typeof(Boolean))
          value = SQLiteConvert.ToBoolean(value);
        else if (pd.PropertyType != typeof(byte[]))
          value = TypeDescriptor.GetConverter(pd.PropertyType).ConvertFrom(value);
      }
      else
      {
        DefaultValueAttribute att = pd.Attributes[typeof(DefaultValueAttribute)] as DefaultValueAttribute;
        if (att != null)
        {
          value = att.Value;
          b = true;
        }
      }
      return b;
    }

    /// <summary>
    /// Fallback method for MONO, which doesn't implement DbConnectionStringBuilder.GetProperties()
    /// </summary>
    /// <param name="propertyList">The hashtable to fill with property descriptors</param>
    private void FallbackGetProperties(Hashtable propertyList)
    {
      foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this, true))
      {
        if (descriptor.Name != "ConnectionString" && propertyList.ContainsKey(descriptor.DisplayName) == false)
        {
          propertyList.Add(descriptor.DisplayName, descriptor);
        }
      }
    }
  }
#endif
}
