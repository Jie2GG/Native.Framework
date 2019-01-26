using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.App.Interface
{
	/// <summary>
	/// 酷Q 用户事件处理接口
	/// </summary>
	public interface IEvent_UserExpand
	{
		/// <summary>
		/// 当在派生类中重写时, 打开控制台窗口
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void OpenConsoleWindow (object sender, EventArgs e);

		/*
		 *	关于导出方法: 
		 *	
		 *		1. 请在此接口类中添加要实现的接口, 用于在 "Event_Main" 中进行注册
		 *		2. 导出方法的事件调用将使用接口中的方法, 以保证最大可扩展性
		 *		3. 请按照规范在此添加需要的接口以免以后升级时带来不便
		 *		
		 *	至此!
		 */
	}
}
