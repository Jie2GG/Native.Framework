/*
 *	此代码由 T4 引擎根据 StatusExport.tt 模板生成, 若您不了解以下代码的用处, 请勿修改!
 *	
 *	此文件包含项目 Json 文件的悬浮窗导出函数.
 */
using System;
using System.Runtime.InteropServices;
using System.Text;
using Native.Csharp.Sdk.Cqp.EventArgs;
using Native.Csharp.Sdk.Cqp.Interface;
using Unity;

namespace Native.Csharp.App.Core
{
    public class StatusExport
    {
		#region --构造函数--
		/// <summary>
		/// 静态构造函数, 注册依赖注入回调
		/// </summary>
        static StatusExport ()
        {
			// 分发应用内事件
			ResolveAppbackcall ();
        }
        #endregion

		#region --私有方法--
		/// <summary>
		/// 获取所有的注入项, 分发到对应的事件
		/// </summary>
		private static void ResolveAppbackcall ()
		{
			/*
			 * Name: 运行时间
			 * Function: _statusUptime
			 */
			if (Common.UnityContainer.IsRegistered<ICqStatus> ("运行时间") == true)
			{
				Status_UPTIME = Common.UnityContainer.Resolve<ICqStatus> ("运行时间").CqStatus;
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
		public static event EventHandler<CqStatusEventArgs> Status_UPTIME;
		[DllExport (ExportName = "_statusUptime", CallingConvention = CallingConvention.StdCall)]
		private static string Evnet__statusUptime ()
		{
			CqStatusEventArgs args = new CqStatusEventArgs (1, "运行时间", "UPTIME", 1000);
			if (Status_UPTIME != null)
			{
				Status_UPTIME (null, args);
			}
			return args.FloatWindowData;
		}


        #endregion
	}
}

