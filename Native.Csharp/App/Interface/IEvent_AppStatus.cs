using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.App.Interface
{
	/// <summary>
	/// 酷Q 应用初始化接口
	/// </summary>
	public interface IEvent_AppStatus
	{
		/// <summary>
		/// Type=1001 酷Q启动<para/>
		/// 当在派生类中重写时, 处理 酷Q 的启动事件回调
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void CqStartup (object sender, EventArgs e);

		/// <summary>
		/// Type=1002 酷Q退出<para/>
		/// 当在派生类中重写时, 处理 酷Q 的退出事件回调
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void CqExit (object sender, EventArgs e);

		/// <summary>
		/// Type=1003 应用被启用<para/>
		/// 当在派生类中重写时, 处理 酷Q 的插件启动事件回调
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void AppEnable (object sender, EventArgs e);

		/// <summary>
		/// Type=1004 应用被禁用<para/>
		/// 当在派生类中重写时, 处理 酷Q 的插件关闭事件回调
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void AppDisable (object sender, EventArgs e);
	}
}
