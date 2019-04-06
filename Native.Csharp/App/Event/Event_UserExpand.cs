using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Native.Csharp.App.Interface;
using Native.Csharp.Sdk.Cqp;

namespace Native.Csharp.App.Event
{
	public class Event_UserExpand : IEvent_UserExpand
	{
		#region --公开方法--
		/// <summary>
		/// 打开控制台窗口
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void OpenConsoleWindow (object sender, EventArgs e)
		{
			// 本事件将会在酷Q【主线程】中被触发, 请注意线程以免卡住酷Q

			// SDK支持载入 WinForm 或 WPF 类型的窗体, 建议您新建一个窗体项目, 引用到本项目中, 
			// 然后 new 窗口进行载入.
			// 所有窗体项目请修改 "输出类型" 为 "类库", 再引用到本项目中. 防止出现错误
		}
		#endregion

		/*
		 *	关于导出方法:
		 *		
		 *		1. 请在此类中完成 IEvent_UserExpand 接口的实现, 以便 SDK 能够更灵活的将此实例注入到导出方法中
		 *		2. 请按照规范在此实现接口以免以后升级时带来不便
		 *	
		 *	至此!
		 */
	}
}
