using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using Native.Csharp.Tool.Core;

namespace Native.Csharp.Tool.SQLite
{
	public class SQLite3Connection : DbConnection, IDbConnection, IDisposable
	{
		#region --字段--
		private IntPtr _pdb = IntPtr.Zero;
		private SQLite3OpenFlags _flags = SQLite3OpenFlags.SQLITE_OPEN_NONE;

		private string _database = string.Empty;
		private string _dataSource = string.Empty;
		private string _serverVersion = string.Empty;
		private string _connectionString = string.Empty;
		private ConnectionState _state = ConnectionState.Closed;
		#endregion

		#region --属性--
		/// <summary>
		/// 获取要连接到数据库服务器的名称。
		/// </summary>
		public override string ConnectionString
		{
			get { return this._connectionString; }
			set { this._connectionString = value; }
		}

		/// <summary>
		/// 获取打开连接时后, 当前数据库的名称或打开连接前在连接字符串中指定的数据库名称。
		/// </summary>
		public override string Database
		{
			get { return this._database; }
		}

		/// <summary>
		/// 获取要连接到数据库服务器的名称。
		/// </summary>
		public override string DataSource
		{
			get { return this._dataSource; }
		}

		/// <summary>
		/// 获取一个字符串，表示该对象连接到服务器的版本。
		/// </summary>
		public override string ServerVersion
		{
			get { return this._serverVersion; }
		}

		/// <summary>
		/// 获取一个字符串，描述连接状态。
		/// </summary>
		public override ConnectionState State
		{
			get { return _state; }
		}
		#endregion

		#region --构造函数--
		/// <summary>
		/// 使用内存中的位置初始化 <see cref="SQLite3Connection"/> 类的一个新实例
		/// </summary>
		public SQLite3Connection ()
			: this ("Data Source=:memory:;Version=3")
		{
		}

		/// <summary>
		/// 初始化 <see cref="SQLite3Connection"/> 类的一个新实例
		/// </summary>
		/// <param name="connectionString">设置与之关联的连接字符串</param>
		public SQLite3Connection (string connectionString)
		{
			this.ConnectionString = connectionString;
		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// 打开数据库连接指定的设置与 <see cref="ConnectionString"/>
		/// </summary>
		public override void Open ()
		{
			DbConnectionStringBuilder connectionStringBuilder = new DbConnectionStringBuilder ();
			connectionStringBuilder.ConnectionString = this.ConnectionString;

			object result = null;
			if (!connectionStringBuilder.TryGetValue ("Data Source", out result))
			{
				throw new ArgumentException ("数据源不能为空。使用:memory:打开内存中的数据库");
			}

			// 检查是否需要创建数据库
			CheckOpenFlags (connectionStringBuilder, "FailIfMissing", true, SQLite3OpenFlags.SQLITE_OPEN_NONE, SQLite3OpenFlags.SQLITE_OPEN_CREATE);

			// 检查是否需要只读打开
			CheckOpenFlags (connectionStringBuilder, "Read Only", true, SQLite3OpenFlags.SQLITE_OPEN_READONLY, SQLite3OpenFlags.SQLITE_OPEN_READWRITE);

			if (SQLite3.Sqlite3_open_v2 (result.ToString (), out this._pdb, (int)this._flags, 0) == (int)SQLite3ErrorCode.SQLITE_ERROR_OK)
			{

			}
		}

		public override void Close ()
		{
			throw new NotImplementedException ();
		}

		protected override DbCommand CreateDbCommand ()
		{
			throw new NotImplementedException ();
		}

		protected override DbTransaction BeginDbTransaction (IsolationLevel isolationLevel)
		{
			throw new NotImplementedException ();
		}

		public override void ChangeDatabase (string databaseName)
		{
			throw new NotImplementedException ();
		}
		#endregion

		#region --私有方法--
		/// <summary>
		/// 检索指定的打开方式
		/// </summary>
		/// <param name="dbConnectionString">连接字符串</param>
		/// <param name="key">连接字符串中的 Key</param>
		/// <param name="value">连接字符串中结果的值</param>
		/// <param name="successFlags">检索成功传入的值</param>
		/// <param name="defaultFlags">检索失败传入的值</param>
		/// <exception cref="ArgumentNullException">参数: <see cref="dbConnectionString"/> 为 null</exception>
		/// <exception cref="ArgumentException">参数: <see cref="key"/> 为空字符串或为 null</exception>
		private void CheckOpenFlags (DbConnectionStringBuilder dbConnectionString, string key, object value, SQLite3OpenFlags successFlags, SQLite3OpenFlags defaultFlags)
		{
			if (dbConnectionString == null)
			{
				throw new ArgumentNullException ("dbConnectionString", "参数不能为 null");
			}

			if (string.IsNullOrEmpty (key))
			{
				throw new System.ArgumentException ("值不能为空字符串或字符串为 null", "key");
			}

			object result = null;
			if (dbConnectionString.TryGetValue (key, out result))
			{
				try
				{
					if (result.Equals (value.ToString ()))
					{
						this._flags = this._flags | successFlags;
						return;
					}
				}
				catch { }
			}
			this._flags = this._flags | defaultFlags;
		}
		#endregion
	}
}
