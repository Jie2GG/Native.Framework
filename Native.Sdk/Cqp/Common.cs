using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Sdk.Cqp
{
	/// <summary>
	/// 存放Sdk公共数据的类
	/// </summary>
	public class Common
	{
		#region --常量--
		internal const string DllName = "CQP.dll";
		#endregion

		#region --字段--
		private static readonly Lazy<Common> _instace = new Lazy<Common>(() => new Common());
		private string _appDirCache = null;
		private int _authCode = 0;
		#endregion

		#region --属性--
		/// <summary>
		/// 获取 Native.Sdk.Cqp.Common 的实例对象
		/// </summary>
		public static Common Instance
		{
			get { return _instace.Value; }
		}
		/// <summary>
		/// 应用验证码
		/// </summary>
		internal int AuthCode
		{
			get { return _authCode; }
			set { _authCode = value; }
		}
		/// <summary>
		/// 应用数据目录缓存
		/// </summary>
		internal string AppDirCache
		{
			get { return _appDirCache; }
			set { _appDirCache = value; }
		}
		#endregion

		#region --构造函数--
		/// <summary>
		/// 隐藏构造函数
		/// </summary>
		private Common()
		{

		}
		#endregion
	}
}
