/*
 *	此代码由 T4 引擎根据 LibExport.tt 模板生成, 若您不了解以下代码的用处, 请勿修改!
 *	
 *	此文件包含项目 Json 文件的事件导出函数.
 */
using System;
using System.Runtime.InteropServices;
using System.Text;
using Native.Csharp.App.Event;
using Native.Csharp.App.EventArgs;
using Native.Csharp.App.Interface;
using Native.Csharp.Sdk.Cqp;
using Native.Csharp.Sdk.Cqp.Model;
using Native.Csharp.Sdk.Cqp.Other;
using Unity;

namespace Native.Csharp.App.Core
{
    public class LibExport
    {
		#region --字段--
		private static Encoding _defaultEncoding = null;
		#endregion

		#region --构造函数--
		/// <summary>
		/// 静态构造函数, 注册依赖注入回调
		/// </summary>
		static LibExport ()
		{
			_defaultEncoding = Encoding.GetEncoding ("GB18030");
			
			// 初始化 Costura.Fody
			CosturaUtility.Initialize ();
			
			// 初始化依赖注入容器
			Common.UnityContainer = new UnityContainer ();

			// 程序开始调用方法进行注册
			Event_AppMain.Registbackcall (Common.UnityContainer);

			// 注册完毕调用方法进行分发
			Event_AppMain.Resolvebackcall (Common.UnityContainer);

			// 分发应用内事件
			ResolveAppbackcall ();
		}
		#endregion
		
		#region --核心方法--
		/// <summary>
		/// 返回 AppID 与 ApiVer, 本方法在模板运行后会根据项目名称自动填写 AppID 与 ApiVer
		/// </summary>
		/// <returns></returns>
		[DllExport (ExportName = "AppInfo", CallingConvention = CallingConvention.StdCall)]
		private static string AppInfo ()
		{
			// 请勿随意修改
			// 
			Common.AppName = "酷Q样例应用 for C#";
			Common.AppVersion = Version.Parse ("1.0.0");		

			//
			// 当前项目名称: Native.Csharp
			// Api版本: 9

			return string.Format ("{0},{1}", 9, "Native.Csharp");
		}

		/// <summary>
		/// 接收插件 AutoCode, 注册异常
		/// </summary>
		/// <param name="authCode"></param>
		/// <returns></returns>
		[DllExport (ExportName = "Initialize", CallingConvention = CallingConvention.StdCall)]
		private static int Initialize (int authCode)
		{
			// 酷Q获取应用信息后，如果接受该应用，将会调用这个函数并传递AuthCode。
			Common.CqApi = new CqApi (authCode);

			// AuthCode 传递完毕后将对象加入容器托管, 以便在其它项目中调用
			Common.UnityContainer.RegisterInstance<CqApi> (Common.CqApi);

			// 注册插件全局异常捕获回调, 用于捕获未处理的异常, 回弹给 酷Q 做处理
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			// 本函数【禁止】处理其他任何代码，以免发生异常情况。如需执行初始化代码请在Startup事件中执行（Type=1001）。
			return 0;
		}
		#endregion
		
		#region --私有方法--
		/// <summary>
		/// 全局异常捕获, 用于捕获开发者未处理的异常, 此异常将回弹至酷Q进行处理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void CurrentDomain_UnhandledException (object sender, UnhandledExceptionEventArgs e)
		{
			Exception ex = e.ExceptionObject as Exception;
			if (ex != null)
			{
				StringBuilder innerLog = new StringBuilder ();
				innerLog.AppendLine ("发现未处理的异常!");
				innerLog.AppendLine ("异常堆栈：");
				innerLog.AppendLine (ex.ToString ());
				Common.CqApi.AddFatalError (innerLog.ToString ());      //将未经处理的异常弹回酷Q做处理
			}
		}
		
		/// <summary>
		/// 获取所有的注入项, 分发到对应的事件
		/// </summary>
		private static void ResolveAppbackcall ()
		{
			/*
			 * Id: 1
			 * Name: 私聊消息处理
			 */
			if (Common.UnityContainer.IsRegistered<IReceiveFriendMessage> ("私聊消息处理") == true)
			{
				ReceiveFriendMessage_1 = Common.UnityContainer.Resolve<IReceiveFriendMessage> ("私聊消息处理").ReceiveFriendMessage;
			}
			if (Common.UnityContainer.IsRegistered<IReceiveOnlineStatusMessage> ("私聊消息处理") == true)
			{
				ReceiveOnlineStatusMessage_1 = Common.UnityContainer.Resolve<IReceiveOnlineStatusMessage> ("私聊消息处理").ReceiveOnlineStatusMessage;
			}
			if (Common.UnityContainer.IsRegistered<IReceiveGroupPrivateMessage> ("私聊消息处理") == true)
			{
				ReceiveGroupPrivateMessage_1 = Common.UnityContainer.Resolve<IReceiveGroupPrivateMessage> ("私聊消息处理").ReceiveGroupPrivateMessage;
			}
			if (Common.UnityContainer.IsRegistered<IReceiveDiscussPrivateMessage> ("私聊消息处理") == true)
			{
				ReceiveDiscussPrivateMessage_1 = Common.UnityContainer.Resolve<IReceiveDiscussPrivateMessage> ("私聊消息处理").ReceiveDiscussPrivateMessage;
			}
			
			/*
			 * Id: 2
			 * Name: 群消息处理
			 */
			if (Common.UnityContainer.IsRegistered<IReceiveGroupMessage> ("群消息处理") == true)
			{
				ReceiveGroupMessage_2 = Common.UnityContainer.Resolve<IReceiveGroupMessage> ("群消息处理").ReceiveGroupMessage;
			}
			
			/*
			 * Id: 3
			 * Name: 讨论组消息处理
			 */
			if (Common.UnityContainer.IsRegistered<IReceiveDiscussMessage> ("讨论组消息处理") == true)
			{
				ReceiveDiscussMessage_3 = Common.UnityContainer.Resolve<IReceiveDiscussMessage> ("讨论组消息处理").ReceiveDiscussMessage;
			}
			
			/*
			 * Id: 4
			 * Name: 群文件上传事件处理
			 */
			if (Common.UnityContainer.IsRegistered<IReceiveGroupFileUpload> ("群文件上传事件处理") == true)
			{
				ReceiveFileUploadMessage_4 = Common.UnityContainer.Resolve<IReceiveGroupFileUpload> ("群文件上传事件处理").ReceiveGroupFileUpload;
			}
			
			/*
			 * Id: 5
			 * Name: 群管理变动事件处理
			 */
			if (Common.UnityContainer.IsRegistered<IReceiveGroupManageIncrease> ("群管理变动事件处理") == true)
			{
				ReceiveManageIncrease_5 = Common.UnityContainer.Resolve<IReceiveGroupManageIncrease> ("群管理变动事件处理").ReceiveGroupManageIncrease;
			}
			if (Common.UnityContainer.IsRegistered<IReceiveGroupManageDecrease> ("群管理变动事件处理") == true)
			{
				ReceiveManageDecrease_5 = Common.UnityContainer.Resolve<IReceiveGroupManageDecrease> ("群管理变动事件处理").ReceiveGroupManageDecrease;
			}
			
			/*
			 * Id: 6
			 * Name: 群成员减少事件处理
			 */
			if (Common.UnityContainer.IsRegistered<IReceiveGroupMemberLeave> ("群成员减少事件处理") == true)
			{
				ReceiveMemberLeave_6 = Common.UnityContainer.Resolve<IReceiveGroupMemberLeave> ("群成员减少事件处理").ReceiveGroupMemberLeave;
			}
			if (Common.UnityContainer.IsRegistered<IReceiveGroupMemberRemove> ("群成员减少事件处理") == true)
			{
				ReceiveMemberRemove_6 = Common.UnityContainer.Resolve<IReceiveGroupMemberRemove> ("群成员减少事件处理").ReceiveGroupMemberRemove;
			}
			
			/*
			 * Id: 7
			 * Name: 群成员增加事件处理
			 */
			if (Common.UnityContainer.IsRegistered<IReceiveGroupMemberPass> ("群成员增加事件处理") == true)
			{
				ReceiveMemberPass_7 = Common.UnityContainer.Resolve<IReceiveGroupMemberPass> ("群成员增加事件处理").ReceiveGroupMemberPass;
			}
			if (Common.UnityContainer.IsRegistered<IReceiveGroupMemberBeInvitee> ("群成员增加事件处理") == true)
			{
				ReceiveMemberBeInvitee_7 = Common.UnityContainer.Resolve<IReceiveGroupMemberBeInvitee> ("群成员增加事件处理").ReceiveGroupMemberBeInvitee;
			}
			
			/*
			 * Id: 10
			 * Name: 好友已添加事件处理
			 */
			if (Common.UnityContainer.IsRegistered<IReceiveFriendIncrease> ("好友已添加事件处理") == true)
			{
				ReceiveFriendIncrease_10 = Common.UnityContainer.Resolve<IReceiveFriendIncrease> ("好友已添加事件处理").ReceiveFriendIncrease;
			}
			
			/*
			 * Id: 8
			 * Name: 好友添加请求处理
			 */
			if (Common.UnityContainer.IsRegistered<IReceiveFriendAddRequest> ("好友添加请求处理") == true)
			{
				ReceiveFriendAdd_8 = Common.UnityContainer.Resolve<IReceiveFriendAddRequest> ("好友添加请求处理").ReceiveFriendAddRequest;
			}
			
			/*
			 * Id: 9
			 * Name: 群添加请求处理
			 */
			if (Common.UnityContainer.IsRegistered<IReceiveAddGroupRequest> ("群添加请求处理") == true)
			{
				ReceiveAddGroupRequest_9 = Common.UnityContainer.Resolve<IReceiveAddGroupRequest> ("群添加请求处理").ReceiveAddGroupRequest;
			}
			if (Common.UnityContainer.IsRegistered<IReceiveAddGroupBeInvitee> ("群添加请求处理") == true)
			{
				ReceiveAddGroupBeInvitee_9 = Common.UnityContainer.Resolve<IReceiveAddGroupBeInvitee> ("群添加请求处理").ReceiveAddGroupBeInvitee;
			}
			
			/*
			 * Id: 1001
			 * Name: 酷Q启动事件
			 */
			if (Common.UnityContainer.IsRegistered<ICqStartup> ("酷Q启动事件") == true)
			{
				CqStartup_1001 = Common.UnityContainer.Resolve<ICqStartup> ("酷Q启动事件").CqStartup;
			}
			
			/*
			 * Id: 1002
			 * Name: 酷Q关闭事件
			 */
			if (Common.UnityContainer.IsRegistered<ICqExit> ("酷Q关闭事件") == true)
			{
				CqExit_1002 = Common.UnityContainer.Resolve<ICqExit> ("酷Q关闭事件").CqExit;
			}
			
			/*
			 * Id: 1003
			 * Name: 应用已被启用
			 */
			if (Common.UnityContainer.IsRegistered<ICqAppEnable> ("应用已被启用") == true)
			{
				AppEnable_1003 = Common.UnityContainer.Resolve<ICqAppEnable> ("应用已被启用").CqAppEnable;
			}
			
			/*
			 * Id: 1004
			 * Name: 应用将被停用
			 */
			if (Common.UnityContainer.IsRegistered<ICqAppDisable> ("应用将被停用") == true)
			{
				AppDisable_1004 = Common.UnityContainer.Resolve<ICqAppDisable> ("应用将被停用").CqAppDisable;
			}
			

		}
		#endregion
		
		#region --导出方法--
		/*
		 * Id: 1
		 * Type: 21
		 * Name: 私聊消息处理
		 * Function: _eventPrivateMsg
		 */
		public static event EventHandler<CqPrivateMessageEventArgs> ReceiveFriendMessage_1;
		public static event EventHandler<CqPrivateMessageEventArgs> ReceiveOnlineStatusMessage_1;
		public static event EventHandler<CqPrivateMessageEventArgs> ReceiveGroupPrivateMessage_1;
		public static event EventHandler<CqPrivateMessageEventArgs> ReceiveDiscussPrivateMessage_1;
		[DllExport (ExportName = "_eventPrivateMsg", CallingConvention = CallingConvention.StdCall)]
		private static int Evnet__eventPrivateMsg (int subType, int msgId, long fromQQ, IntPtr msg, int font)
		{
			if (ReceiveFriendMessage_1 != null)
			{
				CqPrivateMessageEventArgs args = new CqPrivateMessageEventArgs (1, msgId, fromQQ, msg.ToString (_defaultEncoding));
				if (subType == 11)
				{
					if (ReceiveFriendMessage_1 != null)
					{
						ReceiveFriendMessage_1 (null, args);
					}
				}
				else if (subType == 1)
				{
					if (ReceiveOnlineStatusMessage_1 != null)
					{
						ReceiveOnlineStatusMessage_1 (null, args);
					}
				}
				else if (subType == 2)
				{
					if (ReceiveGroupPrivateMessage_1 != null)
					{
						ReceiveGroupPrivateMessage_1 (null, args);
					}
				}
				else if (subType == 3)
				{
					if (ReceiveDiscussPrivateMessage_1 != null)
					{
						ReceiveDiscussPrivateMessage_1 (null, args);
					}
				}
				return Convert.ToInt32 (args.Handler);
			}
			return -1;
		}

		/*
		 * Id: 2
		 * Type: 2
		 * Name: 群消息处理
		 * Function: _eventGroupMsg
		 */
		public static event EventHandler<CqGroupMessageEventArgs> ReceiveGroupMessage_2;
		[DllExport (ExportName = "_eventGroupMsg", CallingConvention = CallingConvention.StdCall)]
		private static int Evnet__eventGroupMsg (int subType, int msgId, long fromGroup, long fromQQ, string fromAnonymous, IntPtr msg, int font)
		{
			GroupAnonymous anonymous = null;
			if (fromQQ == 80000000 && !string.IsNullOrEmpty (fromAnonymous))
			{
				anonymous = Common.CqApi.GetAnonymous (fromAnonymous);
			}
			CqGroupMessageEventArgs args = new CqGroupMessageEventArgs (2, msgId, fromGroup, fromQQ, anonymous, msg.ToString (_defaultEncoding));
			if (subType == 1)
			{
				if (ReceiveGroupMessage_2 != null)
				{
					ReceiveGroupMessage_2 (null, args);
				}
			}
			return Convert.ToInt32 (args.Handler);
		}

		/*
		 * Id: 3
		 * Type: 4
		 * Name: 讨论组消息处理
		 * Function: _eventDiscussMsg
		 */
		public static event EventHandler<CqDiscussMessageEventArgs> ReceiveDiscussMessage_3;
		[DllExport (ExportName = "_eventDiscussMsg", CallingConvention = CallingConvention.StdCall)]
		private static int Evnet__eventDiscussMsg (int subType, int msgId, long fromDiscuss, long fromQQ, IntPtr msg, int font)
		{
			CqDiscussMessageEventArgs args = new CqDiscussMessageEventArgs (3, msgId, fromDiscuss, fromQQ, msg.ToString (_defaultEncoding));
			if (subType == 1)
			{
				if (ReceiveDiscussMessage_3 != null)
				{
					ReceiveDiscussMessage_3 (null, args);
				}
			}
			return Convert.ToInt32 (args.Handler);
		}

		/*
		 * Id: 4
		 * Type: 11
		 * Name: 群文件上传事件处理
		 * Function: _eventGroupUpload
		 */
		public static event EventHandler<CqGroupFileUploadEventArgs> ReceiveFileUploadMessage_4;
		[DllExport (ExportName = "_eventGroupUpload", CallingConvention = CallingConvention.StdCall)]
		private static int Evnet__eventGroupUpload (int subType, int sendTime, long fromGroup, long fromQQ, string file)
		{
			CqGroupFileUploadEventArgs args = new CqGroupFileUploadEventArgs (4, sendTime.ToDateTime (), fromGroup, fromQQ, Common.CqApi.GetFile (file));
			if (subType == 1)
			{
				if (ReceiveFileUploadMessage_4 != null)
				{
					ReceiveFileUploadMessage_4 (null, args);
				}
			}
			return Convert.ToInt32 (args.Handler);
		}

		/*
		 * Id: 5
		 * Type: 101
		 * Name: 群管理变动事件处理
		 * Function: _eventSystem_GroupAdmin
		 */
		public static event EventHandler<CqGroupManageChangeEventArgs> ReceiveManageIncrease_5;
		public static event EventHandler<CqGroupManageChangeEventArgs> ReceiveManageDecrease_5;
		[DllExport (ExportName = "_eventSystem_GroupAdmin", CallingConvention = CallingConvention.StdCall)]
		private static int Evnet__eventSystem_GroupAdmin (int subType, int sendTime, long fromGroup, long beingOperateQQ)
		{
			CqGroupManageChangeEventArgs args = new CqGroupManageChangeEventArgs (5, sendTime.ToDateTime (), fromGroup, beingOperateQQ);
			if (subType == 1)
			{
				if (ReceiveManageDecrease_5 != null)
				{
					ReceiveManageDecrease_5 (null, args);
				}
			}
			else if (subType == 2)
			{
				if (ReceiveManageIncrease_5 != null)
				{
					ReceiveManageIncrease_5 (null, args);
				}
			}
			return Convert.ToInt32 (args.Handler);
		}

		/*
		 * Id: 6
		 * Type: 102
		 * Name: 群成员减少事件处理
		 * Function: _eventSystem_GroupMemberDecrease
		 */
		public static event EventHandler<CqGroupMemberDecreaseEventArgs> ReceiveMemberLeave_6;
		public static event EventHandler<CqGroupMemberDecreaseEventArgs> ReceiveMemberRemove_6;
		[DllExport (ExportName = "_eventSystem_GroupMemberDecrease", CallingConvention = CallingConvention.StdCall)]
		private static int Evnet__eventSystem_GroupMemberDecrease (int subType, int sendTime, long fromGroup, long fromQQ, long beingOperateQQ)
		{
			CqGroupMemberDecreaseEventArgs args = new CqGroupMemberDecreaseEventArgs (6, sendTime.ToDateTime (), fromGroup, fromQQ, beingOperateQQ);
			if (subType == 1)
			{
				if (ReceiveMemberLeave_6 != null)
				{
					ReceiveMemberLeave_6 (null, args);
				}
			}
			else if (subType == 2)
			{
				if (ReceiveMemberRemove_6 != null)
				{
					ReceiveMemberRemove_6 (null, args);
				}
			}
			return Convert.ToInt32 (args.Handler);
		}

		/*
		 * Id: 7
		 * Type: 103
		 * Name: 群成员增加事件处理
		 * Function: _eventSystem_GroupMemberIncrease
		 */
		public static event EventHandler<CqGroupMemberIncreaseEventArgs> ReceiveMemberPass_7;
		public static event EventHandler<CqGroupMemberIncreaseEventArgs> ReceiveMemberBeInvitee_7;
		[DllExport (ExportName = "_eventSystem_GroupMemberIncrease", CallingConvention = CallingConvention.StdCall)]
		private static int Evnet__eventSystem_GroupMemberIncrease (int subType, int sendTime, long fromGroup, long fromQQ, long beingOperateQQ)
		{
			CqGroupMemberIncreaseEventArgs args = new CqGroupMemberIncreaseEventArgs (7, sendTime.ToDateTime (), fromGroup, fromQQ, beingOperateQQ);
			if (subType == 1)
			{
				if (ReceiveMemberPass_7 != null)
				{
					ReceiveMemberPass_7 (null, args);
				}
			}
			else if (subType == 2)
			{
				if (ReceiveMemberBeInvitee_7 != null)
				{
					ReceiveMemberBeInvitee_7 (null, args);
				}
			}
			return Convert.ToInt32 (args.Handler);
		}

		/*
		 * Id: 10
		 * Type: 201
		 * Name: 好友已添加事件处理
		 * Function: _eventFriend_Add
		 */
		public static event EventHandler<CqFriendIncreaseEventArgs> ReceiveFriendIncrease_10;
		[DllExport (ExportName = "_eventFriend_Add", CallingConvention = CallingConvention.StdCall)]
		private static int Evnet__eventFriend_Add (int subType, int sendTime, long fromQQ)
		{
			CqFriendIncreaseEventArgs args = new CqFriendIncreaseEventArgs (10, sendTime.ToDateTime (), fromQQ);
			if (subType == 1)
			{
				if (ReceiveFriendIncrease_10 != null)
				{
					ReceiveFriendIncrease_10 (null, args);
				}
			}
			return Convert.ToInt32 (args.Handler);
		}

		/*
		 * Id: 8
		 * Type: 301
		 * Name: 好友添加请求处理
		 * Function: _eventRequest_AddFriend
		 */
		public static event EventHandler<CqAddFriendRequestEventArgs> ReceiveFriendAdd_8;
		[DllExport (ExportName = "_eventRequest_AddFriend", CallingConvention = CallingConvention.StdCall)]
		private static int Evnet__eventRequest_AddFriend (int subType, int sendTime, long fromQQ, IntPtr msg, string responseFlag)
		{
			CqAddFriendRequestEventArgs args = new CqAddFriendRequestEventArgs (8, sendTime.ToDateTime (), fromQQ, msg.ToString (_defaultEncoding), responseFlag);
			if (subType == 1)
			{
				if (ReceiveFriendAdd_8 != null)
				{
					ReceiveFriendAdd_8 (null, args);
				}
			}
			return Convert.ToInt32 (args.Handler);
		}

		/*
		 * Id: 9
		 * Type: 302
		 * Name: 群添加请求处理
		 * Function: _eventRequest_AddGroup
		 */
		public static event EventHandler<CqAddGroupRequestEventArgs> ReceiveAddGroupRequest_9;
		public static event EventHandler<CqAddGroupRequestEventArgs> ReceiveAddGroupBeInvitee_9;
		[DllExport (ExportName = "_eventRequest_AddGroup", CallingConvention = CallingConvention.StdCall)]
		private static int Evnet__eventRequest_AddGroup (int subType, int sendTime, long fromGroup, long fromQQ, IntPtr msg, string responseFlag)
		{
			CqAddGroupRequestEventArgs args = new CqAddGroupRequestEventArgs (9, sendTime.ToDateTime (), fromGroup, fromQQ, msg.ToString (_defaultEncoding), responseFlag);
			if (subType == 1)
			{
				if (ReceiveAddGroupRequest_9 != null)
				{
					ReceiveAddGroupRequest_9 (null, args);
				}
			}
			else if (subType == 2)
			{
				if (ReceiveAddGroupBeInvitee_9 != null)
				{
					ReceiveAddGroupBeInvitee_9 (null, args);
				}
			}
			return Convert.ToInt32 (args.Handler);
		}

		/*
		 * Id: 1001
		 * Type: 1001
		 * Name: 酷Q启动事件
		 * Function: _eventStartup
		 */
		public static event EventHandler<CqStartupEventArgs> CqStartup_1001;
		[DllExport (ExportName = "_eventStartup", CallingConvention = CallingConvention.StdCall)]
		private static int Evnet__eventStartup ()
		{
			if (CqStartup_1001 != null)
			{
				CqStartup_1001 (null, new CqStartupEventArgs (1001));
			}
			return 0;
		}

		/*
		 * Id: 1002
		 * Type: 1002
		 * Name: 酷Q关闭事件
		 * Function: _eventExit
		 */
		public static event EventHandler<CqExitEventArgs> CqExit_1002;
		[DllExport (ExportName = "_eventExit", CallingConvention = CallingConvention.StdCall)]
		private static int Evnet__eventExit ()
		{
			if (CqExit_1002 != null)
			{
				CqExit_1002 (null, new CqExitEventArgs (1002));
			}
			return 0;
		}

		/*
		 * Id: 1003
		 * Type: 1003
		 * Name: 应用已被启用
		 * Function: _eventEnable
		 */
		public static event EventHandler<CqAppEnableEventArgs> AppEnable_1003;
		[DllExport (ExportName = "_eventEnable", CallingConvention = CallingConvention.StdCall)]
		private static int Evnet__eventEnable ()
		{
			if (AppEnable_1003 != null)
			{
				AppEnable_1003 (null, new CqAppEnableEventArgs (1003));
			}
			return 0;
		}

		/*
		 * Id: 1004
		 * Type: 1004
		 * Name: 应用将被停用
		 * Function: _eventDisable
		 */
		public static event EventHandler<CqAppDisableEventArgs> AppDisable_1004;
		[DllExport (ExportName = "_eventDisable", CallingConvention = CallingConvention.StdCall)]
		private static int Evnet__eventDisable ()
		{
			if (AppDisable_1004 != null)
			{
				AppDisable_1004 (null, new CqAppDisableEventArgs (1004));
			}
			return 0;
		}


		#endregion
    }
}

