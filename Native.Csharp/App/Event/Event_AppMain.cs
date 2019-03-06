using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using Unity;
using Native.Csharp.App.Core;
using Native.Csharp.App.Interface;
using Native.Csharp.App.Model;
using Native.Csharp.Sdk.Cqp.Api;

namespace Native.Csharp.App.Event
{
	public static class Event_AppMain
	{
		/// <summary>
		/// 回调注册
		/// </summary>
		/// <param name="builder"></param>
		public static void Registbackcall (IUnityContainer container)
		{
			#region --回调注入
			// 注册 Event_AppStatus 类, 继承于 IEvent_AppStatus
			container.RegisterType<IEvent_AppStatus, Event_AppStatus> ();

			// 注册 Event_DiscussMessage 类, 继承于 IEvent_DiscussMessage
			container.RegisterType<IEvent_DiscussMessage, Event_DiscussMessage> ();

			// 注册 IEvent_FriendMessage 类, 继承于 Event_FriendMessage
			container.RegisterType<IEvent_FriendMessage, Event_FriendMessage> ();

			// 注册 IEvent_GroupMessage 类, 继承于 Event_GroupMessage
			container.RegisterType<IEvent_GroupMessage, Event_GroupMessage> ();

			// 注册 IEvent_OtherMessage 类, 继承于 Event_OtherMessage
			container.RegisterType<IEvent_OtherMessage, Event_OtherMessage> ();
			#endregion

			// 当需要新注册回调类型时
			// 在此写上需要注册的回调类型, 以 <接口, 实现类> 的方式进行注册
			container.RegisterType<IEvent_UserExpand, Event_UserExpand> ();
		}

		/// <summary>
		/// 回调分发
		/// </summary>
		/// <param name="container"></param>
		public static void Resolvebackcall (IUnityContainer container)
		{
			#region --IEvent_AppStatus--
			// 解析 IEvent_AppStatus 接口
			IEvent_AppStatus appStatus = container.Resolve<IEvent_AppStatus> ();

			// 分发 IEvent_AppStatus 接口到事件
			LibExport.CqStartup += appStatus.CqStartup;
			LibExport.CqExit += appStatus.CqExit;
			LibExport.AppEnable += appStatus.AppEnable;
			LibExport.AppDisable += appStatus.AppDisable;
			#endregion

			#region --IEvent_DiscussMessage--
			// 解析 IEvent_DiscussMessage 接口
			IEvent_DiscussMessage discussMessage = container.Resolve<IEvent_DiscussMessage> ();

			// 分发 IEvent_DiscussMessage 接口到事件
			LibExport.ReceiveDiscussMessage += discussMessage.ReceiveDiscussMessage;
			LibExport.ReceiveDiscussPrivateMessage += discussMessage.ReceiveDiscussPrivateMessage;
			#endregion

			#region --IEvent_FriendMessage--
			// 解析 IEvent_FriendMessage 接口
			IEvent_FriendMessage friendMessage = container.Resolve<IEvent_FriendMessage> ();

			// 分发 IEvent_FriendMessage 接口到事件
			LibExport.ReceiveFriendAdd += friendMessage.ReceiveFriendAddRequest;
			LibExport.ReceiveFriendIncrease += friendMessage.ReceiveFriendIncrease;
			LibExport.ReceiveFriendMessage += friendMessage.ReceiveFriendMessage;
			#endregion

			#region --IEvent_GroupMessage--
			// 解析 IEvent_GroupMessage 接口
			IEvent_GroupMessage groupMessage = container.Resolve<IEvent_GroupMessage> ();

			// 分发 IEvent_GroupMessage 接口到事件
			LibExport.ReceiveGroupMessage += groupMessage.ReceiveGroupMessage;
			LibExport.ReceiveGroupPrivateMessage += groupMessage.ReceiveGroupPrivateMessage;
			LibExport.ReceiveFileUploadMessage += groupMessage.ReceiveGroupFileUpload;
			LibExport.ReceiveManageIncrease += groupMessage.ReceiveGroupManageIncrease;
			LibExport.ReceiveManageDecrease += groupMessage.ReceiveGroupManageDecrease;
			LibExport.ReceiveMemberJoin += groupMessage.ReceiveGroupMemberJoin;
			LibExport.ReceiveMemberInvitee += groupMessage.ReceiveGroupMemberInvitee;
			LibExport.ReceiveMemberLeave += groupMessage.ReceiveGroupMemberLeave;
			LibExport.ReceiveMemberRemove += groupMessage.ReceiveGroupMemberRemove;
			LibExport.ReceiveGroupAddApply += groupMessage.ReceiveGroupAddApply;
			LibExport.ReceiveGroupAddInvitee += groupMessage.ReceiveGroupAddInvitee;
			#endregion

			#region --IEvent_OtherMessage--
			// 解析 IEvent_OtherMessage 接口
			IEvent_OtherMessage otherMessage = container.Resolve<IEvent_OtherMessage> ();

			// 分发 IEvent_OtherMessage 接口到事件
			LibExport.ReceiveQnlineStatusMessage += otherMessage.ReceiveOnlineStatusMessage;
			#endregion

			// 当已经注入了新的回调类型时
			// 在此分发已经注册的回调类型, 解析完毕后分发到导出的事件进行注册
			IEvent_UserExpand userExpand = container.Resolve<IEvent_UserExpand> ();
			UserExport.UserOpenConsole += userExpand.OpenConsoleWindow;
		}

		/// <summary>
		/// 当前回调事件的注册和分发完成之后将调用此方法
		/// </summary>
		public static void Initialize ()
		{

		}
	}
}
