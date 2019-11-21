/*
 * 此文件由T4引擎自动生成, 请勿修改此文件中的代码!
 */
using System;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using Native.Csharp.App.Event;
using Native.Csharp.Sdk.Cqp;
using Native.Csharp.Sdk.Cqp.EventArgs;
using Native.Csharp.Sdk.Cqp.Interface;
using Native.Csharp.Sdk.Cqp.Model;
using Native.Csharp.Sdk.Cqp.Expand;
using Unity;
using Unity.Injection;

namespace Native.Csharp.App.Export
{
	public class CQEventExport	
	{	
		#region --构造函数--	
		/// <summary>	
		/// 由托管环境初始化的 <see cref="CQEventExport"/> 的新实例	
		/// </summary>	
		static CQEventExport ()	
		{	
			// 初始化 Costura.Fody	
			CosturaUtility.Initialize ();	
			// 初始化依赖注入容器	
			Common.UnityContainer = new UnityContainer ();	
			// 程序开始调用方法进行注册	
			Event_AppMain.Registbackcall (Common.UnityContainer);	
			// 分发应用内事件	
		}	
		#endregion	
		#region --核心方法--	
		/// <summary>	
		/// 返回酷Q用于识别本应用的 AppID 和 ApiVer	
		/// </summary>	
		/// <returns>酷Q用于识别本应用的 AppID 和 ApiVer</returns>	
		[DllExport (ExportName = "AppInfo", CallingConvention = CallingConvention.StdCall)]	
		private static string AppInfo ()	
		{	
			return "9,Native.Csharp";	
		}	
		
		/// <summary>	
		/// 接收应用 Authcode, 用于注册接口	
		/// </summary>	
		/// <param name="authCode">酷Q应用验证码</param>	
		/// <returns>返回注册结果给酷Q</returns>	
		[DllExport (ExportName = "Initialize", CallingConvention = CallingConvention.StdCall)]	
		private static int Initialize (int authCode)	
		{	
			// 向容器注册一个 CQApi 实例	
			Common.UnityContainer.RegisterType<CQApi> ("Native.Csharp", new InjectionConstructor(authCode));	
			// 向容器注册一个 CQLog 实例	
			Common.UnityContainer.RegisterType<CQLog> ("Native.Csharp", new InjectionConstructor(authCode));	
			return 0;	
		}	
		#endregion	
	}	
}
