using Native.Csharp.Sdk.Cqp.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Native.Csharp.App.Model;

namespace Native.Csharp.App.Event
{
	public class Event_PrivateMessage
	{
		#region --字段--
		private static readonly Lazy<Event_PrivateMessage> _instance = new Lazy<Event_PrivateMessage>(() => new Event_PrivateMessage());
		#endregion

		#region --属性--
		/// <summary>
		/// 获取 Event_ReceiveMessage 实例对象
		/// </summary>
		public static Event_PrivateMessage Instance { get { return _instance.Value; } }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 隐藏构造函数
		/// </summary>
		private Event_PrivateMessage()
		{

		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// Type=21 好友消息
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void ReceiveFriendMessage(object sender, PrivateMessageEventArgs e)
		{
			//本子程序会在酷Q【线程】中被调用，请注意使用对象等需要初始化(CoInitialize,CoUninitialize)。
			//这里处理消息

			Common.CqApi.SendPrivateMessage(e.FromQQ, Common.CqApi.CqCode_At(e.FromQQ) + "你发送了这样的消息:" + e.Msg);

			e.Handled = true;
			// e.Handled 相当于 原酷Q事件的返回值
			// 如果要回复消息，请调用api发送，并且置 true - 截断本条消息，不再继续处理 //注意：应用优先级设置为"最高"(10000)时，不得置 true
			// 如果不回复消息，交由之后的应用/过滤器处理，这里置 false  - 忽略本条消息
		}

		/// <summary>
		/// Type=21 在线状态消息
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void ReceiveOnlineStatusMessage(object sender, PrivateMessageEventArgs e)
		{
			//本子程序会在酷Q【线程】中被调用，请注意使用对象等需要初始化(CoInitialize,CoUninitialize)。
			//这里处理消息



			e.Handled = false;  //关于返回说明, 请参见 "Event_ReceiveMessage.ReceiveFriendMessage" 方法
		}

		/// <summary>
		/// Type=21 群私聊消息
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void ReceiveGroupPrivateMessage(object sender, PrivateMessageEventArgs e)
		{
			//本子程序会在酷Q【线程】中被调用，请注意使用对象等需要初始化(CoInitialize,CoUninitialize)。
			//这里处理消息


			e.Handled = false;  //关于返回说明, 请参见 "Event_ReceiveMessage.ReceiveFriendMessage" 方法
		}

		/// <summary>
		/// Type=21 讨论组私聊消息
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void ReceiveDiscussPrivateMessage(object sender, PrivateMessageEventArgs e)
		{
			//本子程序会在酷Q【线程】中被调用，请注意使用对象等需要初始化(CoInitialize,CoUninitialize)。
			//这里处理消息



			e.Handled = false;  //关于返回说明, 请参见 "Event_ReceiveMessage.ReceiveFriendMessage" 方法
		}
		#endregion
	}
}
