/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace System.Data.SQLite
{
  using System;
  using System.Data;
  using System.Data.Common;
  using System.ComponentModel;

  /// <summary>
  /// SQLite implementation of DbParameter.
  /// </summary>
  public sealed class SQLiteParameter : DbParameter, ICloneable
  {
    /// <summary>
    /// This value represents an "unknown" <see cref="DbType" />.
    /// </summary>
    private const DbType UnknownDbType = (DbType)(-1);

    /// <summary>
    /// The command associated with this parameter.
    /// </summary>
    private IDbCommand     _command;
    /// <summary>
    /// The data type of the parameter
    /// </summary>
    internal DbType        _dbType;
    /// <summary>
    /// The version information for mapping the parameter
    /// </summary>
    private DataRowVersion _rowVersion;
    /// <summary>
    /// The value of the data in the parameter
    /// </summary>
    private Object         _objValue;
    /// <summary>
    /// The source column for the parameter
    /// </summary>
    private string         _sourceColumn;
    /// <summary>
    /// The column name
    /// </summary>
    private string         _parameterName;
    /// <summary>
    /// The data size, unused by SQLite
    /// </summary>
    private int            _dataSize;

    private bool           _nullable;
    private bool           _nullMapping;

    /// <summary>
    /// The database type name associated with this parameter, if any.
    /// </summary>
    private string         _typeName;

    /// <summary>
    /// Constructor used when creating for use with a specific command.
    /// </summary>
    /// <param name="command">
    /// The command associated with this parameter.
    /// </param>
    internal SQLiteParameter(
        IDbCommand command
        )
        : this()
    {
        _command = command;
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public SQLiteParameter() 
      : this(null, UnknownDbType, 0, null, DataRowVersion.Current)
    {
    }

    /// <summary>
    /// Constructs a named parameter given the specified parameter name
    /// </summary>
    /// <param name="parameterName">The parameter name</param>
    public SQLiteParameter(string parameterName)
      : this(parameterName, UnknownDbType, 0, null, DataRowVersion.Current)
    {
    }

    /// <summary>
    /// Constructs a named parameter given the specified parameter name and initial value
    /// </summary>
    /// <param name="parameterName">The parameter name</param>
    /// <param name="value">The initial value of the parameter</param>
    public SQLiteParameter(string parameterName, object value)
      : this(parameterName, UnknownDbType, 0, null, DataRowVersion.Current)
    {
      Value = value;
    }

    /// <summary>
    /// Constructs a named parameter of the specified type
    /// </summary>
    /// <param name="parameterName">The parameter name</param>
    /// <param name="dbType">The datatype of the parameter</param>
    public SQLiteParameter(string parameterName, DbType dbType)
      : this(parameterName, dbType, 0, null, DataRowVersion.Current)
    {
    }

    /// <summary>
    /// Constructs a named parameter of the specified type and source column reference
    /// </summary>
    /// <param name="parameterName">The parameter name</param>
    /// <param name="dbType">The data type</param>
    /// <param name="sourceColumn">The source column</param>
    public SQLiteParameter(string parameterName, DbType dbType, string sourceColumn)
      : this(parameterName, dbType, 0, sourceColumn, DataRowVersion.Current)
    {
    }

    /// <summary>
    /// Constructs a named parameter of the specified type, source column and row version
    /// </summary>
    /// <param name="parameterName">The parameter name</param>
    /// <param name="dbType">The data type</param>
    /// <param name="sourceColumn">The source column</param>
    /// <param name="rowVersion">The row version information</param>
    public SQLiteParameter(string parameterName, DbType dbType, string sourceColumn, DataRowVersion rowVersion)
      : this(parameterName, dbType, 0, sourceColumn, rowVersion)
    {
    }

    /// <summary>
    /// Constructs an unnamed parameter of the specified data type
    /// </summary>
    /// <param name="dbType">The datatype of the parameter</param>
    public SQLiteParameter(DbType dbType)
      : this(null, dbType, 0, null, DataRowVersion.Current)
    {
    }

    /// <summary>
    /// Constructs an unnamed parameter of the specified data type and sets the initial value
    /// </summary>
    /// <param name="dbType">The datatype of the parameter</param>
    /// <param name="value">The initial value of the parameter</param>
    public SQLiteParameter(DbType dbType, object value)
      : this(null, dbType, 0, null, DataRowVersion.Current)
    {
      Value = value;
    }

    /// <summary>
    /// Constructs an unnamed parameter of the specified data type and source column
    /// </summary>
    /// <param name="dbType">The datatype of the parameter</param>
    /// <param name="sourceColumn">The source column</param>
    public SQLiteParameter(DbType dbType, string sourceColumn)
      : this(null, dbType, 0, sourceColumn, DataRowVersion.Current)
    {
    }

    /// <summary>
    /// Constructs an unnamed parameter of the specified data type, source column and row version
    /// </summary>
    /// <param name="dbType">The data type</param>
    /// <param name="sourceColumn">The source column</param>
    /// <param name="rowVersion">The row version information</param>
    public SQLiteParameter(DbType dbType, string sourceColumn, DataRowVersion rowVersion)
      : this(null, dbType, 0, sourceColumn, rowVersion)
    {
    }

    /// <summary>
    /// Constructs a named parameter of the specified type and size
    /// </summary>
    /// <param name="parameterName">The parameter name</param>
    /// <param name="parameterType">The data type</param>
    /// <param name="parameterSize">The size of the parameter</param>
    public SQLiteParameter(string parameterName, DbType parameterType, int parameterSize)
      : this(parameterName, parameterType, parameterSize, null, DataRowVersion.Current)
    {
    }

    /// <summary>
    /// Constructs a named parameter of the specified type, size and source column
    /// </summary>
    /// <param name="parameterName">The name of the parameter</param>
    /// <param name="parameterType">The data type</param>
    /// <param name="parameterSize">The size of the parameter</param>
    /// <param name="sourceColumn">The source column</param>
    public SQLiteParameter(string parameterName, DbType parameterType, int parameterSize, string sourceColumn)
      : this(parameterName, parameterType, parameterSize, sourceColumn, DataRowVersion.Current)
    {
    }

    /// <summary>
    /// Constructs a named parameter of the specified type, size, source column and row version
    /// </summary>
    /// <param name="parameterName">The name of the parameter</param>
    /// <param name="parameterType">The data type</param>
    /// <param name="parameterSize">The size of the parameter</param>
    /// <param name="sourceColumn">The source column</param>
    /// <param name="rowVersion">The row version information</param>
    public SQLiteParameter(string parameterName, DbType parameterType, int parameterSize, string sourceColumn, DataRowVersion rowVersion)      
    {
      _parameterName = parameterName;
      _dbType = parameterType;
      _sourceColumn = sourceColumn;
      _rowVersion = rowVersion;
      _dataSize = parameterSize;
      _nullable = true;
    }

    private SQLiteParameter(SQLiteParameter source)
      : this(source.ParameterName, source._dbType, 0, source.Direction, source.IsNullable, 0, 0, source.SourceColumn, source.SourceVersion, source.Value)
    {
      _nullMapping = source._nullMapping;
    }

    /// <summary>
    /// Constructs a named parameter of the specified type, size, source column and row version
    /// </summary>
    /// <param name="parameterName">The name of the parameter</param>
    /// <param name="parameterType">The data type</param>
    /// <param name="parameterSize">The size of the parameter</param>
    /// <param name="direction">Only input parameters are supported in SQLite</param>
    /// <param name="isNullable">Ignored</param>
    /// <param name="precision">Ignored</param>
    /// <param name="scale">Ignored</param>
    /// <param name="sourceColumn">The source column</param>
    /// <param name="rowVersion">The row version information</param>
    /// <param name="value">The initial value to assign the parameter</param>   
#if !PLATFORM_COMPACTFRAMEWORK
    [EditorBrowsable(EditorBrowsableState.Advanced)]
#endif
    public SQLiteParameter(string parameterName, DbType parameterType, int parameterSize, ParameterDirection direction, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion rowVersion, object value)
      : this(parameterName, parameterType, parameterSize, sourceColumn, rowVersion)
    {
      Direction = direction;
      IsNullable = isNullable;
      Value = value;
    }

    /// <summary>
    /// Constructs a named parameter, yet another flavor
    /// </summary>
    /// <param name="parameterName">The name of the parameter</param>
    /// <param name="parameterType">The data type</param>
    /// <param name="parameterSize">The size of the parameter</param>
    /// <param name="direction">Only input parameters are supported in SQLite</param>
    /// <param name="precision">Ignored</param>
    /// <param name="scale">Ignored</param>
    /// <param name="sourceColumn">The source column</param>
    /// <param name="rowVersion">The row version information</param>
    /// <param name="sourceColumnNullMapping">Whether or not this parameter is for comparing NULL's</param>
    /// <param name="value">The intial value to assign the parameter</param>
#if !PLATFORM_COMPACTFRAMEWORK
    [EditorBrowsable(EditorBrowsableState.Advanced)]
#endif
    public SQLiteParameter(string parameterName, DbType parameterType, int parameterSize, ParameterDirection direction, byte precision, byte scale, string sourceColumn, DataRowVersion rowVersion, bool sourceColumnNullMapping, object value)
      : this(parameterName, parameterType, parameterSize, sourceColumn, rowVersion)
    {
      Direction = direction;
      SourceColumnNullMapping = sourceColumnNullMapping;
      Value = value;
    }

    /// <summary>
    /// Constructs an unnamed parameter of the specified type and size
    /// </summary>
    /// <param name="parameterType">The data type</param>
    /// <param name="parameterSize">The size of the parameter</param>
    public SQLiteParameter(DbType parameterType, int parameterSize)
      : this(null, parameterType, parameterSize, null, DataRowVersion.Current)
    {
    }

    /// <summary>
    /// Constructs an unnamed parameter of the specified type, size, and source column
    /// </summary>
    /// <param name="parameterType">The data type</param>
    /// <param name="parameterSize">The size of the parameter</param>
    /// <param name="sourceColumn">The source column</param>
    public SQLiteParameter(DbType parameterType, int parameterSize, string sourceColumn)
      : this(null, parameterType, parameterSize, sourceColumn, DataRowVersion.Current)
    {
    }

    /// <summary>
    /// Constructs an unnamed parameter of the specified type, size, source column and row version
    /// </summary>
    /// <param name="parameterType">The data type</param>
    /// <param name="parameterSize">The size of the parameter</param>
    /// <param name="sourceColumn">The source column</param>
    /// <param name="rowVersion">The row version information</param>
    public SQLiteParameter(DbType parameterType, int parameterSize, string sourceColumn, DataRowVersion rowVersion)
      : this(null, parameterType, parameterSize, sourceColumn, rowVersion)
    {
    }

    /// <summary>
    /// The command associated with this parameter.
    /// </summary>
    public IDbCommand Command
    {
      get
      {
        return _command;
      }
      set 
      {
        _command = value;
      }
    }

    /// <summary>
    /// Whether or not the parameter can contain a null value
    /// </summary>
    public override bool IsNullable
    {
      get
      {
        return _nullable;
      }
      set 
      {
        _nullable = value;
      }
    }

    /// <summary>
    /// Returns the datatype of the parameter
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [DbProviderSpecificTypeProperty(true)]
    [RefreshProperties(RefreshProperties.All)]
#endif
    public override DbType DbType
    {
      get
      {
        if (_dbType == UnknownDbType)
        {
          if (_objValue != null && _objValue != DBNull.Value)
          {
            return SQLiteConvert.TypeToDbType(_objValue.GetType());
          }
          return DbType.String; // Unassigned default value is String
        }
        return _dbType;
      }
      set
      {
        _dbType = value;
      }
    }

    /// <summary>
    /// Supports only input parameters
    /// </summary>
    public override ParameterDirection Direction
    {
      get
      {
        return ParameterDirection.Input;
      }
      set
      {
        if (value != ParameterDirection.Input)
          throw new NotSupportedException();
      }
    }

    /// <summary>
    /// Returns the parameter name
    /// </summary>
    public override string ParameterName
    {
      get
      {
        return _parameterName;
      }
      set
      {
        _parameterName = value;
      }
    }

    /// <summary>
    /// Resets the DbType of the parameter so it can be inferred from the value
    /// </summary>
    public override void ResetDbType()
    {
      _dbType = UnknownDbType;
    }

    /// <summary>
    /// Returns the size of the parameter
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [DefaultValue((int)0)]
#endif
    public override int Size
    {
      get
      {
        return _dataSize;
      }
      set
      {
        _dataSize = value;
      }
    }

    /// <summary>
    /// Gets/sets the source column
    /// </summary>
    public override string SourceColumn
    {
      get
      {
        return _sourceColumn;
      }
      set
      {
        _sourceColumn = value;
      }
    }

    /// <summary>
    /// Used by DbCommandBuilder to determine the mapping for nullable fields
    /// </summary>
    public override bool SourceColumnNullMapping
    {
      get
      {
        return _nullMapping;
      }
      set
      {
        _nullMapping = value;
      }
    }

    /// <summary>
    /// Gets and sets the row version
    /// </summary>
    public override DataRowVersion SourceVersion
    {
      get
      {
        return _rowVersion;
      }
      set
      {
        _rowVersion = value;
      }
    }

    /// <summary>
    /// Gets and sets the parameter value.  If no datatype was specified, the datatype will assume the type from the value given.
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [TypeConverter(typeof(StringConverter)), RefreshProperties(RefreshProperties.All)]
#endif
    public override object Value
    {
      get
      {
        return _objValue;
      }
      set
      {
        _objValue = value;
        if (_dbType == UnknownDbType && _objValue != null && _objValue != DBNull.Value) // If the DbType has never been assigned, try to glean one from the value's datatype
          _dbType = SQLiteConvert.TypeToDbType(_objValue.GetType());
      }
    }

    /// <summary>
    /// The database type name associated with this parameter, if any.
    /// </summary>
    public string TypeName
    {
        get
        {
            return _typeName;
        }
        set
        {
            _typeName = value;
        }
    }

    /// <summary>
    /// Clones a parameter
    /// </summary>
    /// <returns>A new, unassociated SQLiteParameter</returns>
    public object Clone()
    {
      SQLiteParameter newparam = new SQLiteParameter(this);

      return newparam;
    }
  }
}
