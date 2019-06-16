/*
 *	此代码由 T4 引擎根据 StatusExport.tt 模板生成, 若您不了解以下代码的用处, 请勿修改!
 *	
 *	此文件包含项目 Json 文件的悬浮窗导出函数.
 */
using System;
using System.Runtime.InteropServices;
using System.Text;
using Native.Csharp.App.EventArgs;
using Native.Csharp.App.Interface;
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
			 * Name: CPU使用率
			 * Function: _statusCPU
			 */
			if (Common.UnityContainer.IsRegistered<ICqStatus> ("CPU使用率") == true)
			{
				Status_CPU = Common.UnityContainer.Resolve<ICqStatus> ("CPU使用率").CqStatus;
			}


		}
        #endregion

		#region --导出方法--
		/*
		 * Id: 1
		 * Name: CPU使用率
		 * Title: CPU
		 * Function: _statusCPU
		 * Period: 1000
		 */
		public static event EventHandler<CqStatusEventArgs> Status_CPU;
		[DllExport (ExportName = "_statusCPU", CallingConvention = CallingConvention.StdCall)]
		private static string Evnet__statusCPU ()
		{
			CqStatusEventArgs args = new CqStatusEventArgs (1, "CPU使用率", "CPU", 1000);
			if (Status_CPU != null)
			{
				Status_CPU (null, args);
			}
			return args.FloatWindowData;
		}


        #endregion
	}
}

