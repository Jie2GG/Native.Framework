using Native.Sdk.Cqp.Api;
using Native.Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Native.Demo.App.Model;

namespace Native.Demo.App.Event
{
	public class Event_GroupMessage
	{
		#region --字段--
		private static Event_GroupMessage _instance = new Event_GroupMessage();
		#endregion

		#region --属性--
		/// <summary>
		/// 获取 Event_GroupMessage 实例对象
		/// </summary>
		public static Event_GroupMessage Instance { get => _instance; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 隐藏构造函数
		/// </summary>
		private Event_GroupMessage()
		{

		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// Type=2 群消息
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void ReceiveGroupMessage(object sender, GroupMessageEventArgs e)
		{
			//本子程序会在酷Q【线程】中被调用，请注意使用对象等需要初始化(CoInitialize,CoUninitialize)。
			//这里处理消息
			if (e.FromAnonymous != null)    //如果此属性不为null, 则消息来自于匿名成员
			{
				EnApi.Instance.SendGroupMessage(e.FromGroup, e.FromAnonymous.CodeName + " 你发送了这样的消息: " + e.Msg);
				e.Handled = true;
				return;
			}


			EnApi.Instance.SendGroupMessage(e.FromGroup, EnApi.Instance.CqCode_At(e.FromQQ) + "你发送了这样的消息: " + e.Msg);

			e.Handled = true;   //关于返回说明, 请参见 "Event_ReceiveMessage.ReceiveFriendMessage" 方法
		}

		/// <summary>
		/// Type=11 群文件上传事件
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void ReceiveGroupFileUpload(object sender, FileUploadMessageEventArgs e)
		{
			//本子程序会在酷Q【线程】中被调用，请注意使用对象等需要初始化(CoInitialize,CoUninitialize)。
			//这里处理消息
			//关于文件信息, 触发事件时已经转换完毕, 请直接使用



			e.Handled = false;   //关于返回说明, 请参见 "Event_ReceiveMessage.ReceiveFriendMessage" 方法
		}

		/// <summary>
		/// Type=101 群事件-管理员增加
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void ReceiveGroupManageIncrease(object sender, GroupManageAlterEventArgs e)
		{
			//本子程序会在酷Q【线程】中被调用，请注意使用对象等需要初始化(CoInitialize,CoUninitialize)。
			//这里处理消息



			e.Handled = false;  //关于返回说明, 请参见 "Event_ReceiveMessage.ReceiveFriendMessage" 方法
		}

		/// <summary>
		/// Type=101 群事件-管理员减少
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void ReceiveGroupManageDecrease(object sender, GroupManageAlterEventArgs e)
		{
			//本子程序会在酷Q【线程】中被调用，请注意使用对象等需要初始化(CoInitialize,CoUninitialize)。
			//这里处理消息



			e.Handled = false;  //关于返回说明, 请参见 "Event_ReceiveMessage.ReceiveFriendMessage" 方法
		}

		/// <summary>
		/// Type=103 群事件-群成员增加 - 主动入群
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void ReceiveGroupMemberJoin(object sender, GroupMemberAlterEventArgs e)
		{
			//本子程序会在酷Q【线程】中被调用，请注意使用对象等需要初始化(CoInitialize,CoUninitialize)。
			//这里处理消息



			e.Handled = false;  //关于返回说明, 请参见 "Event_ReceiveMessage.ReceiveFriendMessage" 方法
		}

		/// <summary>
		/// Type=103 群事件-群成员增加 - 被邀请入群
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void ReceiveGroupMemberInvitee(object sender, GroupMemberAlterEventArgs e)
		{
			//本子程序会在酷Q【线程】中被调用, 请注意使用对象等需要初始化(ConIntialize, CoUninitialize).
			//这里处理消息



			e.Handled = false;  //关于返回说明, 请参见 "Event_ReceiveMessage.ReceiveFriendMessage" 方法
		}

		/// <summary>
		/// Type=102 群事件-群成员减少 - 成员离开
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void ReceiveGroupMemberLeave(object sender, GroupMemberAlterEventArgs e)
		{
			//本子程序会在酷Q【线程】中被调用, 请注意使用对象等需要初始化(ConIntialize, CoUninitialize).
			//这里处理消息



			e.Handled = false;  //关于返回说明, 请参见 "Event_ReceiveMessage.ReceiveFriendMessage" 方法
		}

		/// <summary>
		/// Type=102 群事件-群成员减少 - 成员移除
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void ReceiveGroupMemberRemove(object sender, GroupMemberAlterEventArgs e)
		{
			//本子程序会在酷Q【线程】中被调用, 请注意使用对象等需要初始化(ConIntialize, CoUninitialize).
			//这里处理消息



			e.Handled = false;  //关于返回说明, 请参见 "Event_ReceiveMessage.ReceiveFriendMessage" 方法
		}

		/// <summary>
		/// Type=302 请求-群添加 - 申请入群
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void ReceiveGroupAddApply(object sender, GroupAddRequestEventArgs e)
		{
			//本子程序会在酷Q【线程】中被调用, 请注意使用对象等需要初始化(ConIntialize, CoUninitialize).
			//这里处理消息



			e.Handled = false;  //关于返回说明, 请参见 "Event_ReceiveMessage.ReceiveFriendMessage" 方法
		}

		/// <summary>
		/// Type=302 请求-群添加 - (酷Q登录号)被邀请入群
		/// </summary>
		/// <param name="sender">触发此事件的对象</param>
		/// <param name="e">附加的参数</param>
		public void ReceiveGroupAddInvitee(object sender, GroupAddRequestEventArgs e)
		{
			//本子程序会在酷Q【线程】中被调用, 请注意使用对象等需要初始化(ConIntialize, CoUninitialize).
			//这里处理消息



			e.Handled = false;  //关于返回说明, 请参见 "Event_ReceiveMessage.ReceiveFriendMessage" 方法
		}
		#endregion
	}
}
