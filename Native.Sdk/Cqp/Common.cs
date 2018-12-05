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
		private static string _appDirCache = null;
		private static int _authCode;
		#endregion

		#region --属性--
		/// <summary>
		/// 应用验证码
		/// </summary>
		internal static int AuthCode
		{
			get { return _authCode; }
			set { _authCode = value; }
		}
		/// <summary>
		/// 应用数据目录缓存
		/// </summary>
		internal static string AppDirCache
		{
			get { return _appDirCache; }
			set { _appDirCache = value; }
		}
		#endregion
	}
}
