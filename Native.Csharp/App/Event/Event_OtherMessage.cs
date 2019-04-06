using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Native.Csharp.App.Model;
using Native.Csharp.App.Interface;
using Native.Csharp.Sdk.Cqp;

namespace Native.Csharp.App.Event
{
	public class Event_OtherMessage : IEvent_OtherMessage
	{
		#region --公开方法--
		/// <summary>
		/// Type=21 在线状态消息<para/>
		/// 处理收到的在线状态消息
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		public void ReceiveOnlineStatusMessage (object sender, PrivateMessageEventArgs e)
		{
			// 本子程序会在酷Q【线程】中被调用，请注意使用对象等需要初始化(CoInitialize,CoUninitialize)。
			// 这里处理消息



			e.Handled = false;  // 关于返回说明, 请参见 "Event_FriendMessage.ReceiveFriendMessage" 方法
		}
		#endregion
	}
}
