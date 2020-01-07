using Native.Csharp.Sdk.Cqp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Native.Csharp.App.Common
{
	/// <summary>
	/// 用于存放应用公共数据的类
	/// </summary>
	public static class AppData
	{
		/// <summary>
		/// 获取当前 App 使用的 酷Q Api 接口实例
		/// </summary>
		public static CQApi CQApi { get { return AppInfo.UnityContainer.Resolve<CQApi> (AppInfo.Id); } }

		/// <summary>
		/// 获取当前 App 使用的 酷Q Log 接口实例
		/// </summary>
		public static CQLog CQLog { get { return AppInfo.UnityContainer.Resolve<CQLog> (AppInfo.Id); } }
	}
}
