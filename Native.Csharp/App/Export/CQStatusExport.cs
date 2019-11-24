/*
 * 此文件由T4引擎自动生成, 请勿修改此文件中的代码!
 */
using System;
using System.Runtime.InteropServices;
using Native.Csharp.Sdk.Cqp.EventArgs;
using Native.Csharp.Sdk.Cqp.Interface;
using Native.Csharp.Sdk.Cqp.Model;
using Unity;

namespace Native.Csharp.App.Export
{
	/// <summary>	
	/// 表示酷Q状态导出的类	
	/// </summary>	
	public class CQStatusExport	
	{	
		#region --构造函数--	
		/// <summary>	
		/// 由托管环境初始化的 <see cref="CQStatusExport"/> 的新实例	
		/// </summary>	
		static CQStatusExport ()	
		{	
			// 调用方法进行实例化	
			ResolveBackcall ();	
		}	
		#endregion	
		
		#region --私有方法--	
		/// <summary>	
		/// 读取容器中的注册项, 进行事件分发	
		/// </summary>	
		private static void ResolveBackcall ()	
		{	
			/*	
			 * Id: 1	
			 * Name: 运行时间	
			 * Title: UPTIME	
			 * Function: _statusUptime	
			 * Period: 1000	
			 */	
			foreach (IStatusUpdate item in App.AppInfo.UnityContainer.ResolveAll<IStatusUpdate> ())	
			{	
				Status_statusUptimeHandler += item.StatusUpdate;	
			}	
			
		}	
		#endregion	
		
		#region --导出方法--	
		/*	
		 * Id: 1	
		 * Name: 运行时间	
		 * Title: UPTIME	
		 * Function: _statusUptime	
		 * Period: 1000	
		 */	
		public static event Func<object, CQStatusUpdateEventArgs, CQFloatWindow> Status_statusUptimeHandler;	
		[DllExport (ExportName = "_statusUptime", CallingConvention = CallingConvention.StdCall)]	
		public static string Status_statusUptime ()	
		{	
			CQStatusUpdateEventArgs args = new CQStatusUpdateEventArgs (1, "运行时间", "UPTIME", "_statusUptime", 1000);	
			if (Status_statusUptimeHandler != null)	
			{	
				return Status_statusUptimeHandler (typeof (CQStatusExport), args).GetFloatWindowData ();	
			}	
			return new CQFloatWindow ().GetFloatWindowData (); // 返回默认悬浮窗样式	
		}	
		
		#endregion	
	}	
}
