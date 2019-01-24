using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using Native.Csharp.App.Model;
using Native.Csharp.Sdk.Cqp.Api;

namespace Native.Csharp.App.Event
{
	public class Event_AppInitialize
	{
		#region --字段--
		private static readonly Lazy<Event_AppInitialize> _instance = new Lazy<Event_AppInitialize>(() => new Event_AppInitialize());
		#endregion

		#region --属性--
		/// <summary>
		/// 获取 AppIntitalizeEventHandler 实例对象
		/// </summary>
		public static Event_AppInitialize Instance { get { return _instance.Value; } }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 隐藏构造函数
		/// </summary>
		private Event_AppInitialize()
		{

		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// [请填写] 应用的ApiVer、Appid //请不要在本函数添加其他代码
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void AppInfo(object sender, AppInfoEventArgs e)
		{
			e.ApiVer = 9;                   //Api版本
			e.AppId = "Native.Csharp";      //AppId,  规则见 http://d.cqp.me/Pro/开发/基础信息

			//本函数【禁止】处理其他任何代码，以免发生异常情况。如需执行初始化代码请在Startup事件中执行（Type = 1001）。
		}

		/// <summary>
		/// 应用AuthCode接收 //请不要在本函数添加其他代码
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void Initialize(object sender, AppInitializeEventArgs e)
		{
			//酷Q获取应用信息后，如果接受该应用，将会调用这个函数并传递AuthCode。
			Common.CqApi = new CqApi(e.AuthCode);

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			// 本函数【禁止】处理其他任何代码，以免发生异常情况。如需执行初始化代码请在Startup事件中执行（Type=1001）。
		}
		#endregion

		#region --私有方法--
		/// <summary>
		/// 全局异常捕获, 用于捕获开发者未处理的异常, 此异常将回弹至酷Q进行处理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Exception ex = e.ExceptionObject as Exception;
			if (ex != null)
			{
				StringBuilder innerLog = new StringBuilder ();
				innerLog.AppendLine ("NativeSDK 异常");
				innerLog.AppendFormat ("[异常名称]: {0}{1}", ex.Source.ToString (), Environment.NewLine);
				innerLog.AppendFormat ("[异常消息]: {0}{1}", ex.Message, Environment.NewLine);
				innerLog.AppendFormat ("[异常堆栈]: {0}{1}", ex.StackTrace);

				if (e.IsTerminating)
				{
					Common.CqApi.AddFatalError (innerLog.ToString ());      //将未经处理的异常弹回酷Q做处理
				}
				else
				{
					Common.CqApi.AddLoger (Sdk.Cqp.Enum.LogerLevel.Error, "Native 异常捕捉", innerLog.ToString ());
				}
			}
		}
		#endregion
	}
}
