using Native.Demo.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Demo.App.Event
{
	public class Event_FriendMessage
	{
		#region --字段--
		private static readonly Lazy<Event_FriendMessage> _instance = new Lazy<Event_FriendMessage>(() => new Event_FriendMessage());
		#endregion

		#region --属性--
		/// <summary>
		/// 获取 Event_FriendMessage 实例对象
		/// </summary>
		public static Event_FriendMessage Instance { get => _instance.Value; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 隐藏构造函数
		/// </summary>
		private Event_FriendMessage()
		{

		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// Type=201 好友已添加
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void ReceiveFriendIncrease(object sender, FriendIncreaseEventArgs e)
		{
			//本子程序会在酷Q【线程】中被调用，请注意使用对象等需要初始化(CoInitialize,CoUninitialize)。
			//这里处理消息



			e.Handled = false;   //关于返回说明, 请参见 "Event_ReceiveMessage.ReceiveFriendMessage" 方法
		}

		/// <summary>
		/// Type=301 请求-好友添加
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void ReceiveFriednAddRequest(object sender, FriendAddRequestEventArgs e)
		{
			//本子程序会在酷Q【线程】中被调用，请注意使用对象等需要初始化(CoInitialize,CoUninitialize)。
			//这里处理消息



			e.Handled = false;   //关于返回说明, 请参见 "Event_ReceiveMessage.ReceiveFriendMessage" 方法
		}
		#endregion
	}
}
