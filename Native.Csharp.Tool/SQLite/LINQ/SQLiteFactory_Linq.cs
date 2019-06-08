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
  using System.Reflection;
  using System.Security.Permissions;

  /// <summary>
  /// SQLite implementation of <see cref="IServiceProvider" />.
  /// </summary>
  public sealed partial class SQLiteFactory : IServiceProvider
  {
    //
    // TODO: This points to the legacy "System.Data.SQLite.Linq" assembly
    //       (i.e. the one that does not support Entity Framework 6).
    //       Currently, this class and its containing assembly (i.e.
    //       "System.Data.SQLite") know nothing about the Entity Framework
    //       6 compatible assembly (i.e. "System.Data.SQLite.EF6").  This
    //       situation may need to change.
    //
    private static readonly string DefaultTypeName =
      "System.Data.SQLite.Linq.SQLiteProviderServices, System.Data.SQLite.Linq, " +
      "Version={0}, Culture=neutral, PublicKeyToken=db937bc2d44ff139";

    private static readonly BindingFlags DefaultBindingFlags =
        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

    ///////////////////////////////////////////////////////////////////////////

    private static Type _dbProviderServicesType;
    private static object _sqliteServices;

    static SQLiteFactory()
    {
#if (SQLITE_STANDARD || USE_INTEROP_DLL || PLATFORM_COMPACTFRAMEWORK) && PRELOAD_NATIVE_LIBRARY
        UnsafeNativeMethods.Initialize();
#endif

        SQLiteLog.Initialize(typeof(SQLiteFactory).Name);

        string version =
#if NET_40 || NET_45 || NET_451 || NET_452 || NET_46 || NET_461 || NET_462 || NET_47 || NET_471 || NET_472
            "4.0.0.0";
#else
            "3.5.0.0";
#endif

        _dbProviderServicesType = Type.GetType(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "System.Data.Common.DbProviderServices, System.Data.Entity, Version={0}, Culture=neutral, PublicKeyToken=b77a5c561934e089", version), false);
    }

    /// <summary>
    /// Will provide a <see cref="IServiceProvider" /> object in .NET 3.5.
    /// </summary>
    /// <param name="serviceType">The class or interface type to query for.</param>
    /// <returns></returns>
    object IServiceProvider.GetService(Type serviceType)
    {
      if (serviceType == typeof(ISQLiteSchemaExtensions) ||
        (_dbProviderServicesType != null && serviceType == _dbProviderServicesType))
      {
        return GetSQLiteProviderServicesInstance();
      }
      return null;
    }

#if !NET_STANDARD_20
    [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
#endif
    private object GetSQLiteProviderServicesInstance()
    {
        if (_sqliteServices == null)
        {
            string typeName = UnsafeNativeMethods.GetSettingValue(
                "TypeName_SQLiteProviderServices", null);

            Version version = this.GetType().Assembly.GetName().Version;

            if (typeName != null)
            {
                typeName = HelperMethods.StringFormat(
                    CultureInfo.InvariantCulture, typeName, version);
            }
            else
            {
                typeName = HelperMethods.StringFormat(
                    CultureInfo.InvariantCulture, DefaultTypeName, version);
            }

            Type type = Type.GetType(typeName, false);

            if (type != null)
            {
                FieldInfo field = type.GetField(
                    "Instance", DefaultBindingFlags);

                if (field != null)
                    _sqliteServices = field.GetValue(null);
            }
        }
        return _sqliteServices;
    }
  }
}
