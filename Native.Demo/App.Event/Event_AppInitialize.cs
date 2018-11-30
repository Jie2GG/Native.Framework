using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using Native.Demo.App.Model;
using Native.Sdk.Cqp.Api;

namespace Native.Demo.App.Event
{
	public class Event_AppInitialize
	{
		#region --字段--
		private static Event_AppInitialize _instance = new Event_AppInitialize();
		#endregion

		#region --属性--
		/// <summary>
		/// 获取 AppIntitalizeEventHandler 实例对象
		/// </summary>
		public static Event_AppInitialize Instance { get => _instance; }
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
			e.AppId = "top.jiegg.demo";     //AppId
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
			EnApi.Instance.SetAuthCode(e.AuthCode);

			AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
			// 本函数【禁止】处理其他任何代码，以免发生异常情况。如需执行初始化代码请在Startup事件中执行（Type=1001）。
		}
		#endregion

		#region --私有方法--
		/// <summary>
		/// 异常捕获机制
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
		{
			StringBuilder expStr = new StringBuilder();
			//expStr.AppendFormat("[异常]: {1}\n", e.Exception.Message);
			expStr.AppendFormat("[异常类型]: {0}\n", e.Exception.Source.ToString());
			expStr.AppendFormat("[异常名称]: {0}\n", e.Exception.Message);
			expStr.AppendFormat("[异常堆栈]: {0}\n", e.Exception.StackTrace);
			EnApi.Instance.AddFatalError(expStr.ToString()); //将异常弹回酷Q处理
		}
		#endregion
	}
}
