using Native.Csharp.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.App.Event
{
	public class Event_DiscussMessage
	{
		#region --字段--
		private static Lazy<Event_DiscussMessage> _instance = new Lazy<Event_DiscussMessage>(() => new Event_DiscussMessage());
		#endregion

		#region --属性--
		/// <summary>
		/// 获取 Event_DiscussMessage 实例对象
		/// </summary>
		public static Event_DiscussMessage Instance { get { return _instance.Value; } }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 隐藏构造函数
		/// </summary>
		private Event_DiscussMessage()
		{

		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// Type=4 收到讨论组消息
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void ReceiveDiscussMessage(object sender, DiscussMessageEventArgs e)
		{
			//本子程序会在酷Q【线程】中被调用，请注意使用对象等需要初始化(CoInitialize,CoUninitialize)。
			//这里处理消息


			e.Handled = false;   //关于返回说明, 请参见 "Event_ReceiveMessage.ReceiveFriendMessage" 方法
		}
		#endregion
	}
}
