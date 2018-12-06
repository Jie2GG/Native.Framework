using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp
{
	/// <summary>
	/// 用于存放SDK数据的公共类
	/// </summary>
	public static class Common
	{
		#region --常量--
		public const string DllName = "CQP.dll";
		#endregion

		#region --字段--
		private static int _authCode = 0;
		private static string _appDirCache = null;
		#endregion

		#region --属性--
		/// <summary>
		/// 获取或设置一个值, 该值指示 Native.Csharp.App 应用验证码
		/// </summary>
		public static int AuthCode
		{
			get { return _authCode; }
			set { _authCode = value; }
		}
		/// <summary>
		/// 获取或设置一个值, 指示该值 Native.Csharp.App 应用数据目录缓存
		/// </summary>
		internal static string AppDirCache
		{
			get { return _appDirCache; }
			set { _appDirCache = value; }
		}
		#endregion
	}
}
