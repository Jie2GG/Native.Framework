using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Native.Csharp.App.Model;

namespace Native.Csharp.App.Interface
{
	/// <summary>
	/// 酷Q 群消息接口
	/// </summary>
	public interface IEvent_GroupMessage
	{
		/// <summary>
		/// Type=2 群消息<para/>
		/// 当在派生类中重写时, 处理收到的群消息
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void ReceiveGroupMessage (object sender, GroupMessageEventArgs e);

		/// <summary>
		/// Type=21 群私聊<para/>
		/// 当在派生类中重写时, 处理收到的群私聊消息
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void ReceiveGroupPrivateMessage (object sender, PrivateMessageEventArgs e);

		/// <summary>
		/// Type=11 群文件上传事件<para/>
		/// 当在派生类中重写时, 处理收到的群文件上传结果
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void ReceiveGroupFileUpload (object sender, FileUploadMessageEventArgs e);

		/// <summary>
		/// Type=101 群事件 - 管理员增加<para/>
		/// 当在派生类中重写时, 处理收到的群管理员增加事件
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void ReceiveGroupManageIncrease (object sender, GroupManageAlterEventArgs e);

		/// <summary>
		/// Type=101 群事件 - 管理员减少<para/>
		/// 当在派生类中重写时, 处理收到的群管理员减少事件
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void ReceiveGroupManageDecrease (object sender, GroupManageAlterEventArgs e);

		/// <summary>
		/// Type=103 群事件 - 群成员增加 - 主动入群<para/>
		/// 当在派生类中重写时, 处理收到的群成员增加 (主动入群) 事件
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void ReceiveGroupMemberJoin (object sender, GroupMemberAlterEventArgs e);

		/// <summary>
		/// Type=103 群事件 - 群成员增加 - 被邀入群<para/>
		/// 当在派生类中重写时, 处理收到的群成员增加 (被邀入群) 事件
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void ReceiveGroupMemberInvitee (object sender, GroupMemberAlterEventArgs e);

		/// <summary>
		/// Type=102 群事件 - 群成员减少 - 成员离开<para/>
		/// 当在派生类中重写时, 处理收到的群成员减少 (成员离开) 事件
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void ReceiveGroupMemberLeave (object sender, GroupMemberAlterEventArgs e);

		/// <summary>
		/// Type=102 群事件 - 群成员减少 - 成员移除<para/>
		/// 当在派生类中重写时, 处理收到的群成员减少 (成员移除) 事件
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void ReceiveGroupMemberRemove (object sender, GroupMemberAlterEventArgs e);

		/// <summary>
		/// Type=302 群事件 - 群请求 - 申请入群<para/>
		/// 当在派生类中重写时, 处理收到的群请求 (申请入群) 事件
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void ReceiveGroupAddApply (object sender, GroupAddRequestEventArgs e);

		/// <summary>
		/// Type=302 群事件 - 群请求 - 被邀入群 (机器人被邀)<para/>
		/// 当在派生类中重写时, 处理收到的群请求 (被邀入群) 事件
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void ReceiveGroupAddInvitee (object sender, GroupAddRequestEventArgs e);
	}
}
