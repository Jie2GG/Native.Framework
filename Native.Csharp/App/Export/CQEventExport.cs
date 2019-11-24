/*
 * 此文件由T4引擎自动生成, 请勿修改此文件中的代码!
 */
using System;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using Native.Csharp.App.Event;
using Native.Csharp.Sdk.Cqp;
using Native.Csharp.Sdk.Cqp.EventArgs;
using Native.Csharp.Sdk.Cqp.Interface;
using Native.Csharp.Sdk.Cqp.Expand;
using Unity;
using Unity.Injection;

namespace Native.Csharp.App.Export
{
	/// <summary>	
	/// 表示酷Q事件导出的类	
	/// </summary>	
	public class CQEventExport	
	{	
		#region --构造函数--	
		/// <summary>	
		/// 由托管环境初始化的 <see cref="CQEventExport"/> 的新实例	
		/// </summary>	
		static CQEventExport ()	
		{	
			// 初始化 Costura.Fody	
			CosturaUtility.Initialize ();	
			
			Type type = typeof (App.AppInfo);	// 反射初始化容器	
			type.GetProperty ("Id", BindingFlags.Public | BindingFlags.Static).SetMethod.Invoke (null, new object[] { "Native.Csharp" });	
			type.GetProperty ("ResultCode", BindingFlags.Public | BindingFlags.Static).SetMethod.Invoke (null, new object[] { 1 });	
			type.GetProperty ("ApiVersion", BindingFlags.Public | BindingFlags.Static).SetMethod.Invoke (null, new object[] { 9 });	
			type.GetProperty ("Name", BindingFlags.Public | BindingFlags.Static).SetMethod.Invoke (null, new object[] { "酷Q样例应用" });	
			type.GetProperty ("Version", BindingFlags.Public | BindingFlags.Static).SetMethod.Invoke (null, new object[] { new Version ("1.0.0") });	
			type.GetProperty ("VersionId", BindingFlags.Public | BindingFlags.Static).SetMethod.Invoke (null, new object[] { 1 });	
			type.GetProperty ("Author", BindingFlags.Public | BindingFlags.Static).SetMethod.Invoke (null, new object[] { "Example" });	
			type.GetProperty ("Description", BindingFlags.Public | BindingFlags.Static).SetMethod.Invoke (null, new object[] { "酷Q样例应用(V9应用机制)" });	
			type.GetProperty ("UnityContainer", BindingFlags.Public | BindingFlags.Static).SetMethod.Invoke (null, new object[] { new UnityContainer () });	
			
			// 调用方法进行注册	
			Event_AppMain.RegistEventBackcall (App.AppInfo.UnityContainer);	
			
			// 调用方法进行实例化	
			ResolveBackcall ();	
		}	
		#endregion	
		
		#region --核心方法--	
		/// <summary>	
		/// 返回酷Q用于识别本应用的 AppID 和 ApiVer	
		/// </summary>	
		/// <returns>酷Q用于识别本应用的 AppID 和 ApiVer</returns>	
		[DllExport (ExportName = "AppInfo", CallingConvention = CallingConvention.StdCall)]	
		private static string AppInfo ()	
		{	
			return "9,Native.Csharp";	
		}	
		
		/// <summary>	
		/// 接收应用 Authcode, 用于注册接口	
		/// </summary>	
		/// <param name="authCode">酷Q应用验证码</param>	
		/// <returns>返回注册结果给酷Q</returns>	
		[DllExport (ExportName = "Initialize", CallingConvention = CallingConvention.StdCall)]	
		private static int Initialize (int authCode)	
		{	
			// 向容器注册一个 CQApi 实例	
			App.AppInfo.UnityContainer.RegisterType<CQApi> ("Native.Csharp", new InjectionConstructor(authCode));	
			// 向容器注册一个 CQLog 实例	
			App.AppInfo.UnityContainer.RegisterType<CQLog> ("Native.Csharp", new InjectionConstructor(authCode));	
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
		/// <param name="sender">事件来源对象</param>	
		/// <param name="e">附加的事件参数</param>	
		private static void CurrentDomain_UnhandledException (object sender, UnhandledExceptionEventArgs e)	
		{	
			Exception ex = e.ExceptionObject as Exception;	
			if (ex != null)	
			{	
				StringBuilder innerLog = new StringBuilder ();	
				innerLog.AppendLine ("发现未处理的异常!");	
				innerLog.AppendLine (ex.ToString ());	
				App.AppInfo.CQLog.SetFatalMessage (innerLog.ToString ());	
			}	
		}	
		/// <summary>	
		/// 读取容器中的注册项, 进行事件分发	
		/// </summary>	
		private static void ResolveBackcall ()	
		{	
			/*	
			 * Id: 1	
			 * Type: 21	
			 * Name: 私聊消息处理	
			 * Function: _eventPrivateMsg	
			 * Priority: 30000	
			 */	
			foreach (IPrivateMessage item in App.AppInfo.UnityContainer.ResolveAll<IPrivateMessage> ())	
			{	
				Event_eventPrivateMsgHandler += item.PrivateMessage;	
			}	
			
			/*	
			 * Id: 2	
			 * Type: 2	
			 * Name: 群消息处理	
			 * Function: _eventGroupMsg	
			 * Priority: 30000	
			 */	
			foreach (IGroupMessage item in App.AppInfo.UnityContainer.ResolveAll<IGroupMessage> ())	
			{	
				Event_eventGroupMsgHandler += item.GroupMessage;	
			}	
			
			/*	
			 * Id: 3	
			 * Type: 4	
			 * Name: 讨论组消息处理	
			 * Function: _eventDiscussMsg	
			 * Priority: 30000	
			 */	
			foreach (IDiscussMessage item in App.AppInfo.UnityContainer.ResolveAll<IDiscussMessage> ())	
			{	
				Event_eventDiscussMsgHandler += item.DiscussMessage;	
			}	
			
			/*	
			 * Id: 4	
			 * Type: 11	
			 * Name: 群文件上传事件处理	
			 * Function: _eventGroupUpload	
			 * Priority: 30000	
			 */	
			foreach (IGroupUpload item in App.AppInfo.UnityContainer.ResolveAll<IGroupUpload> ())	
			{	
				Event_eventGroupUploadHandler += item.GroupUpload;	
			}	
			
			/*	
			 * Id: 5	
			 * Type: 101	
			 * Name: 群管理变动事件处理	
			 * Function: _eventSystem_GroupAdmin	
			 * Priority: 30000	
			 */	
			foreach (IGroupManageChange item in App.AppInfo.UnityContainer.ResolveAll<IGroupManageChange> ())	
			{	
				Event_eventSystem_GroupAdminHandler += item.GroupManageChange;	
			}	
			
			/*	
			 * Id: 6	
			 * Type: 102	
			 * Name: 群成员减少事件处理	
			 * Function: _eventSystem_GroupMemberDecrease	
			 * Priority: 30000	
			 */	
			foreach (IGroupMemberDecrease item in App.AppInfo.UnityContainer.ResolveAll<IGroupMemberDecrease> ())	
			{	
				Event_eventSystem_GroupMemberDecreaseHandler += item.GroupMemberDecrease;	
			}	
			
			/*	
			 * Id: 7	
			 * Type: 103	
			 * Name: 群成员增加事件处理	
			 * Function: _eventSystem_GroupMemberIncrease	
			 * Priority: 30000	
			 */	
			foreach (IGroupMemberIncrease item in App.AppInfo.UnityContainer.ResolveAll<IGroupMemberIncrease> ())	
			{	
				Event_eventSystem_GroupMemberIncreaseHandler += item.GroupMemberIncrease;	
			}	
			
			/*	
			 * Id: 8	
			 * Type: 104	
			 * Name: 群禁言事件处理	
			 * Function: _eventSystem_GroupBan	
			 * Priority: 30000	
			 */	
			foreach (IGroupBanSpeak item in App.AppInfo.UnityContainer.ResolveAll<IGroupBanSpeak> ())	
			{	
				Event_eventSystem_GroupBanHandler += item.GroupBanSpeak;	
			}	
			
			/*	
			 * Id: 10	
			 * Type: 201	
			 * Name: 好友已添加事件处理	
			 * Function: _eventFriend_Add	
			 * Priority: 30000	
			 */	
			foreach (IFriendAdd item in App.AppInfo.UnityContainer.ResolveAll<IFriendAdd> ())	
			{	
				Event_eventFriend_AddHandler += item.FriendAdd;	
			}	
			
			/*	
			 * Id: 11	
			 * Type: 301	
			 * Name: 好友添加请求处理	
			 * Function: _eventRequest_AddFriend	
			 * Priority: 30000	
			 */	
			foreach (IFriendAddRequest item in App.AppInfo.UnityContainer.ResolveAll<IFriendAddRequest> ())	
			{	
				Event_eventRequest_AddFriendHandler += item.FriendAddRequest;	
			}	
			
			/*	
			 * Id: 12	
			 * Type: 302	
			 * Name: 群添加请求处理	
			 * Function: _eventRequest_AddGroup	
			 * Priority: 30000	
			 */	
			foreach (IGroupAddRequest item in App.AppInfo.UnityContainer.ResolveAll<IGroupAddRequest> ())	
			{	
				Event_eventRequest_AddGroupHandler += item.GroupAddRequest;	
			}	
			
			/*	
			 * Id: 1001	
			 * Type: 1001	
			 * Name: 酷Q启动事件	
			 * Function: _eventStartup	
			 * Priority: 30000	
			 */	
			foreach (ICQStartup item in App.AppInfo.UnityContainer.ResolveAll<ICQStartup> ())	
			{	
				Event_eventStartupHandler += item.CQStartup;	
			}	
			
			/*	
			 * Id: 1002	
			 * Type: 1002	
			 * Name: 酷Q关闭事件	
			 * Function: _eventExit	
			 * Priority: 30000	
			 */	
			foreach (ICQExit item in App.AppInfo.UnityContainer.ResolveAll<ICQExit> ())	
			{	
				Event_eventExitHandler += item.CQExit;	
			}	
			
			/*	
			 * Id: 1003	
			 * Type: 1003	
			 * Name: 应用已被启用	
			 * Function: _eventEnable	
			 * Priority: 30000	
			 */	
			foreach (IAppEnable item in App.AppInfo.UnityContainer.ResolveAll<IAppEnable> ())	
			{	
				Event_eventEnableHandler += item.AppEnable;	
			}	
			
			/*	
			 * Id: 1004	
			 * Type: 1004	
			 * Name: 应用将被停用	
			 * Function: _eventDisable	
			 * Priority: 30000	
			 */	
			foreach (IAppDisable item in App.AppInfo.UnityContainer.ResolveAll<IAppDisable> ())	
			{	
				Event_eventDisableHandler += item.AppDisable;	
			}	
			
		}	
		#endregion	
		
		#region --导出方法--	
		/*	
		 * Id: 1	
		 * Type: 21	
		 * Name: 私聊消息处理	
		 * Function: _eventPrivateMsg	
		 * Priority: 30000	
		 * IsRegex: False	
		 */	
		public static event EventHandler<CQPrivateMessageEventArgs> Event_eventPrivateMsgHandler;	
		[DllExport (ExportName = "_eventPrivateMsg", CallingConvention = CallingConvention.StdCall)]	
		public static int Event_eventPrivateMsg (int subType, int msgId, long fromQQ, IntPtr msg, int font)	
		{	
			if (Event_eventPrivateMsgHandler != null)	
			{	
				CQPrivateMessageEventArgs args = new CQPrivateMessageEventArgs (1, 21, "私聊消息处理", "_eventPrivateMsg", 30000, subType, msgId, fromQQ, msg.ToString(CQApi.DefaultEncoding), false, App.AppInfo.CQApi);	
				Event_eventPrivateMsgHandler (typeof (CQEventExport), args);	
			}	
			return 0;	
		}	
		
		/*	
		 * Id: 2	
		 * Type: 2	
		 * Name: 群消息处理	
		 * Function: _eventGroupMsg	
		 * Priority: 30000	
		 * IsRegex: False	
		 */	
		public static event EventHandler<CQGroupMessageEventArgs> Event_eventGroupMsgHandler;	
		[DllExport (ExportName = "_eventGroupMsg", CallingConvention = CallingConvention.StdCall)]	
		public static int Event_eventGroupMsg (int subType, int msgId, long fromGroup, long fromQQ, string fromAnonymous, IntPtr msg, int font)	
		{	
			if (Event_eventGroupMsgHandler != null)	
			{	
				CQGroupMessageEventArgs args = new CQGroupMessageEventArgs (2, 2, "群消息处理", "_eventGroupMsg", 30000, subType, msgId, fromGroup, fromQQ, fromAnonymous, msg.ToString(CQApi.DefaultEncoding), false, App.AppInfo.CQApi);	
				Event_eventGroupMsgHandler (typeof (CQEventExport), args);	
			}	
			return 0;	
		}	
		
		/*	
		 * Id: 3	
		 * Type: 4	
		 * Name: 讨论组消息处理	
		 * Function: _eventDiscussMsg	
		 * Priority: 30000	
		 * IsRegex: False	
		 */	
		public static event EventHandler<CQDiscussMessageEventArgs> Event_eventDiscussMsgHandler;	
		[DllExport (ExportName = "_eventDiscussMsg", CallingConvention = CallingConvention.StdCall)]	
		public static int Event_eventDiscussMsg (int subType, int msgId, long fromDiscuss, long fromQQ, IntPtr msg, int font)	
		{	
			if (Event_eventDiscussMsgHandler != null)	
			{	
				CQDiscussMessageEventArgs args = new CQDiscussMessageEventArgs (3, 4, "讨论组消息处理", "_eventDiscussMsg", 30000, subType, msgId, fromDiscuss, fromQQ, msg.ToString(CQApi.DefaultEncoding), false, App.AppInfo.CQApi);	
				Event_eventDiscussMsgHandler (typeof (CQEventExport), args);	
			}	
			return 0;	
		}	
		
		/*	
		 * Id: 4	
		 * Type: 11	
		 * Name: 群文件上传事件处理	
		 * Function: _eventGroupUpload	
		 * Priority: 30000	
		 * IsRegex: False	
		 */	
		public static event EventHandler<CQGroupUploadEventArgs> Event_eventGroupUploadHandler;	
		[DllExport (ExportName = "_eventGroupUpload", CallingConvention = CallingConvention.StdCall)]	
		public static int Event_eventGroupUpload (int subType, int sendTime, long fromGroup, long fromQQ, string file)	
		{	
			if (Event_eventGroupUploadHandler != null)	
			{	
				CQGroupUploadEventArgs args = new CQGroupUploadEventArgs (4, 11, "群文件上传事件处理", "_eventGroupUpload", 30000, subType, sendTime, fromQQ, fromQQ, file, App.AppInfo.CQApi);	
				Event_eventGroupUploadHandler (typeof (CQEventExport), args);	
			}	
			return 0;	
		}	
		
		/*	
		 * Id: 5	
		 * Type: 101	
		 * Name: 群管理变动事件处理	
		 * Function: _eventSystem_GroupAdmin	
		 * Priority: 30000	
		 * IsRegex: False	
		 */	
		public static event EventHandler<CQGroupManageChangeEventArgs> Event_eventSystem_GroupAdminHandler;	
		[DllExport (ExportName = "_eventSystem_GroupAdmin", CallingConvention = CallingConvention.StdCall)]	
		public static int Event_eventSystem_GroupAdmin (int subType, int sendTime, long fromGroup, long beingOperateQQ)	
		{	
			if (Event_eventSystem_GroupAdminHandler != null)	
			{	
				CQGroupManageChangeEventArgs args = new CQGroupManageChangeEventArgs (5, 101, "群管理变动事件处理", "_eventSystem_GroupAdmin", 30000, subType, sendTime, fromGroup, beingOperateQQ, App.AppInfo.CQApi);	
				Event_eventSystem_GroupAdminHandler (typeof (CQEventExport), args);	
			}	
			return 0;	
		}	
		
		/*	
		 * Id: 6	
		 * Type: 102	
		 * Name: 群成员减少事件处理	
		 * Function: _eventSystem_GroupMemberDecrease	
		 * Priority: 30000	
		 * IsRegex: False	
		 */	
		public static event EventHandler<CQGroupMemberDecreaseEventArgs> Event_eventSystem_GroupMemberDecreaseHandler;	
		[DllExport (ExportName = "_eventSystem_GroupMemberDecrease", CallingConvention = CallingConvention.StdCall)]	
		public static int Event_eventSystem_GroupMemberDecrease (int subType, int sendTime, long fromGroup, long fromQQ, long beingOperateQQ)	
		{	
			if (Event_eventSystem_GroupMemberDecreaseHandler != null)	
			{	
				CQGroupMemberDecreaseEventArgs args = new CQGroupMemberDecreaseEventArgs (6, 102, "群成员减少事件处理", "_eventSystem_GroupMemberDecrease", 30000, subType, sendTime, fromGroup, fromQQ, beingOperateQQ, App.AppInfo.CQApi);	
				Event_eventSystem_GroupMemberDecreaseHandler (typeof (CQEventExport), args);	
			}	
			return 0;	
		}	
		
		/*	
		 * Id: 7	
		 * Type: 103	
		 * Name: 群成员增加事件处理	
		 * Function: _eventSystem_GroupMemberIncrease	
		 * Priority: 30000	
		 * IsRegex: False	
		 */	
		public static event EventHandler<CQGroupMemberIncreaseEventArgs> Event_eventSystem_GroupMemberIncreaseHandler;	
		[DllExport (ExportName = "_eventSystem_GroupMemberIncrease", CallingConvention = CallingConvention.StdCall)]	
		public static int Event_eventSystem_GroupMemberIncrease (int subType, int sendTime, long fromGroup, long fromQQ, long beingOperateQQ)	
		{	
			if (Event_eventSystem_GroupMemberIncreaseHandler != null)	
			{	
				CQGroupMemberIncreaseEventArgs args = new CQGroupMemberIncreaseEventArgs (7, 103, "群成员增加事件处理", "_eventSystem_GroupMemberIncrease", 30000, subType, sendTime, fromGroup, fromQQ, beingOperateQQ, App.AppInfo.CQApi);	
				Event_eventSystem_GroupMemberIncreaseHandler (typeof (CQEventExport), args);	
			}	
			return 0;	
		}	
		
		/*	
		 * Id: 8	
		 * Type: 104	
		 * Name: 群禁言事件处理	
		 * Function: _eventSystem_GroupBan	
		 * Priority: 30000	
		 * IsRegex: False	
		 */	
		public static event EventHandler<CQGroupBanSpeakEventArgs> Event_eventSystem_GroupBanHandler;	
		[DllExport (ExportName = "_eventSystem_GroupBan", CallingConvention = CallingConvention.StdCall)]	
		public static int Event_eventSystem_GroupBan (int subType, int sendTime, long fromGroup, long fromQQ, long beingOperateQQ, long duration)	
		{	
			if (Event_eventSystem_GroupBanHandler != null)	
			{	
				CQGroupBanSpeakEventArgs args = new CQGroupBanSpeakEventArgs (8, 104, "群禁言事件处理", "_eventSystem_GroupBan", 30000, subType, sendTime, fromGroup, fromQQ, beingOperateQQ, duration, App.AppInfo.CQApi);	
				Event_eventSystem_GroupBanHandler (typeof (CQEventExport), args);	
			}	
			return 0;	
		}	
		
		/*	
		 * Id: 10	
		 * Type: 201	
		 * Name: 好友已添加事件处理	
		 * Function: _eventFriend_Add	
		 * Priority: 30000	
		 * IsRegex: False	
		 */	
		public static event EventHandler<CQFriendAddEventArgs> Event_eventFriend_AddHandler;	
		[DllExport (ExportName = "_eventFriend_Add", CallingConvention = CallingConvention.StdCall)]	
		public static int Event_eventFriend_Add (int subType, int sendTime, long fromQQ)	
		{	
			if (Event_eventFriend_AddHandler != null)	
			{	
				CQFriendAddEventArgs args = new CQFriendAddEventArgs (10, 201, "好友已添加事件处理", "_eventFriend_Add", 30000, subType, sendTime, fromQQ, App.AppInfo.CQApi);	
				Event_eventFriend_AddHandler (typeof (CQEventExport), args);	
			}	
			return 0;	
		}	
		
		/*	
		 * Id: 11	
		 * Type: 301	
		 * Name: 好友添加请求处理	
		 * Function: _eventRequest_AddFriend	
		 * Priority: 30000	
		 * IsRegex: False	
		 */	
		public static event EventHandler<CQFriendAddRequestEventArgs> Event_eventRequest_AddFriendHandler;	
		[DllExport (ExportName = "_eventRequest_AddFriend", CallingConvention = CallingConvention.StdCall)]	
		public static int Event_eventRequest_AddFriend (int subType, int sendTime, long fromQQ, IntPtr msg, string responseFlag)	
		{	
			if (Event_eventRequest_AddFriendHandler != null)	
			{	
				CQFriendAddRequestEventArgs args = new CQFriendAddRequestEventArgs (11, 301, "好友添加请求处理", "_eventRequest_AddFriend", 30000, subType, sendTime, fromQQ, msg.ToString (CQApi.DefaultEncoding), responseFlag, App.AppInfo.CQApi);	
				Event_eventRequest_AddFriendHandler (typeof (CQEventExport), args);	
			}	
			return 0;	
		}	
		
		/*	
		 * Id: 12	
		 * Type: 302	
		 * Name: 群添加请求处理	
		 * Function: _eventRequest_AddGroup	
		 * Priority: 30000	
		 * IsRegex: False	
		 */	
		public static event EventHandler<CQGroupAddRequestEventArgs> Event_eventRequest_AddGroupHandler;	
		[DllExport (ExportName = "_eventRequest_AddGroup", CallingConvention = CallingConvention.StdCall)]	
		public static int Event_eventRequest_AddGroup (int subType, int sendTime, long fromGroup, long fromQQ, IntPtr msg, string responseFlag)	
		{	
			if (Event_eventRequest_AddGroupHandler != null)	
			{	
				CQGroupAddRequestEventArgs args = new CQGroupAddRequestEventArgs (12, 302, "群添加请求处理", "_eventRequest_AddGroup", 30000, subType, sendTime, fromGroup, fromQQ, msg.ToString (CQApi.DefaultEncoding), responseFlag, App.AppInfo.CQApi);	
				Event_eventRequest_AddGroupHandler (typeof (CQEventExport), args);	
			}	
			return 0;	
		}	
		
		/*	
		 * Id: 1001	
		 * Type: 1001	
		 * Name: 酷Q启动事件	
		 * Function: _eventStartup	
		 * Priority: 30000	
		 * IsRegex: False	
		 */	
		public static event EventHandler<CQStartupEventArgs> Event_eventStartupHandler;	
		[DllExport (ExportName = "_eventStartup", CallingConvention = CallingConvention.StdCall)]	
		public static int Event_eventStartup ()	
		{	
			if (Event_eventStartupHandler != null)	
			{	
				CQStartupEventArgs args = new CQStartupEventArgs (1001, 1001, "酷Q启动事件", "_eventStartup", 30000);	
				Event_eventStartupHandler (typeof (CQEventExport), args);	
			}	
			return 0;	
		}	
		
		/*	
		 * Id: 1002	
		 * Type: 1002	
		 * Name: 酷Q关闭事件	
		 * Function: _eventExit	
		 * Priority: 30000	
		 * IsRegex: False	
		 */	
		public static event EventHandler<CQExitEventArgs> Event_eventExitHandler;	
		[DllExport (ExportName = "_eventExit", CallingConvention = CallingConvention.StdCall)]	
		public static int Event_eventExit ()	
		{	
			if (Event_eventExitHandler != null)	
			{	
				CQExitEventArgs args = new CQExitEventArgs (1002, 1002, "酷Q关闭事件", "_eventExit", 30000);	
				Event_eventExitHandler (typeof (CQEventExport), args);	
			}	
			return 0;	
		}	
		
		/*	
		 * Id: 1003	
		 * Type: 1003	
		 * Name: 应用已被启用	
		 * Function: _eventEnable	
		 * Priority: 30000	
		 * IsRegex: False	
		 */	
		public static event EventHandler<CQAppEnableEventArgs> Event_eventEnableHandler;	
		[DllExport (ExportName = "_eventEnable", CallingConvention = CallingConvention.StdCall)]	
		public static int Event_eventEnable ()	
		{	
			if (Event_eventEnableHandler != null)	
			{	
				CQAppEnableEventArgs args = new CQAppEnableEventArgs (1003, 1003, "应用已被启用", "_eventEnable", 30000);	
				Event_eventEnableHandler (typeof (CQEventExport), args);	
			}	
			return 0;	
		}	
		
		/*	
		 * Id: 1004	
		 * Type: 1004	
		 * Name: 应用将被停用	
		 * Function: _eventDisable	
		 * Priority: 30000	
		 * IsRegex: False	
		 */	
		public static event EventHandler<CQAppDisableEventArgs> Event_eventDisableHandler;	
		[DllExport (ExportName = "_eventDisable", CallingConvention = CallingConvention.StdCall)]	
		public static int Event_eventDisable ()	
		{	
			if (Event_eventDisableHandler != null)	
			{	
				CQAppDisableEventArgs args = new CQAppDisableEventArgs (1004, 1004, "应用将被停用", "_eventDisable", 30000);	
				Event_eventDisableHandler (typeof (CQEventExport), args);	
			}	
			return 0;	
		}	
		
		#endregion	
	}	
}
