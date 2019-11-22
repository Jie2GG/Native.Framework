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
			
			Type type = typeof (App.AppInfo);	// 反射初始化容器	
			type.GetProperty ("Id", BindingFlags.Public | BindingFlags.Static).SetMethod.Invoke (null, new object[] { "Native.Csharp" });	
			type.GetProperty ("ResultCode", BindingFlags.Public | BindingFlags.Static).SetMethod.Invoke (null, new object[] { 1 });	
			type.GetProperty ("ApiVersion", BindingFlags.Public | BindingFlags.Static).SetMethod.Invoke (null, new object[] { 9 });	
			type.GetProperty ("Name", BindingFlags.Public | BindingFlags.Static).SetMethod.Invoke (null, new object[] { "酷Q样例应用" });	
			type.GetProperty ("Version", BindingFlags.Public | BindingFlags.Static).SetMethod.Invoke (null, new object[] { new Version ("1.0.0") });	
			type.GetProperty ("VersionId", BindingFlags.Public | BindingFlags.Static).SetMethod.Invoke (null, new object[] { 1 });	
			type.GetProperty ("Author", BindingFlags.Public | BindingFlags.Static).SetMethod.Invoke (null, new object[] { "Example" });	
			type.GetProperty ("Description", BindingFlags.Public | BindingFlags.Static).SetMethod.Invoke (null, new object[] { "酷Q样例应用(V9应用机制)" });	
			type.GetProperty ("UnityContainer", BindingFlags.Public | BindingFlags.Static).SetMethod.Invoke (null, new object[] { new UnityContainer () });	
				
			// 程序开始调用方法进行注册	
			Event_AppMain.Registbackcall (App.AppInfo.UnityContainer);	
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
			App.AppInfo.UnityContainer.RegisterType<CQApi> ("Native.Csharp", new InjectionConstructor(authCode));	
			// 向容器注册一个 CQLog 实例	
			App.AppInfo.UnityContainer.RegisterType<CQLog> ("Native.Csharp", new InjectionConstructor(authCode));	
			// 注册插件全局异常捕获回调, 用于捕获未处理的异常, 回弹给 酷Q 做处理	
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;	
			// 本函数【禁止】处理其他任何代码，以免发生异常情况。如需执行初始化代码请在Startup事件中执行（Type=1001）。	
			return 0;	
		}	
		#endregion	
		
		#region --私有方法--	
		/// <summary>	
		/// 全局异常捕获, 用于捕获开发者未处理的异常, 此异常将回弹至酷Q进行处理	
		/// </summary>	
		/// <param name="sender">事件来源对象</param>	
		/// <param name="e">附加的事件参数</param>	
		private static void CurrentDomain_UnhandledException (object sender, UnhandledExceptionEventArgs e)	
		{	
			Exception ex = e.ExceptionObject as Exception;	
			if (ex != null)	
			{	
				StringBuilder innerLog = new StringBuilder ();	
				innerLog.AppendLine ("发现未处理的异常!");	
				innerLog.AppendLine (ex.ToString ());	
				App.AppInfo.CQLog.SetFatalMessage (innerLog.ToString ());	
			}	
		}	
		/// <summary>	
		/// 读取容器中的注册项, 进行事件分发	
		/// </summary>	
		private static void ResolveBackcall ()	
		{	
		}	
		#endregion	
	}	
}
