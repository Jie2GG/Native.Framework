using Native.Demo.App.Event;
using Native.Demo.App.Model;
using Native.Sdk.Cqp.Api;
using Native.Sdk.Cqp.Enum;
using Native.Sdk.Cqp.Model;
using Native.Sdk.Cqp.Tool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Native.Demo.App.Core
{
	public class LibExport
	{
		#region --字段--
		private static LibExport _instance = new LibExport();
		#endregion

		#region --属性--
		/// <summary>
		/// 获取 LibExport 实例对象
		/// </summary>
		public static LibExport Instance { get => _instance; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化资源
		/// </summary>
		static LibExport()
		{
			///**	重要说明!!!
			// * 
			// *	由于此类中声明的私有静态方法会被编译成对外公开的C函数(被酷Q主程序调用), 所以此类的静态构造函数将会被最先创建
			// *	注意: 
			// *		1. 此构造函数相当于易语言的 "_启动子程序" 函数
			// *		2. 请不要在本构造函数内添加任何其它代码, 需要初始化等请使用CQ的初始化事件
			// *		3. 此函数的功能用于加载无法导入的程序集, 请勿随意修改以免发生异常
			// *		4. 请务必检查引用的第三方程序集是否已经导入到 "App.Lib" 文件夹下
			// *		5. 请务必检查引用的第三方程序集的命名空间是否与文件名相同, 并且已设置为 "嵌入式资源"
			// *		
			// *		6. 若前5项正常但是还有异常请查看以下代码获得的名称是否正确
			// */
			//AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs args) =>
			//{
			//	Assembly assembly = Assembly.GetExecutingAssembly();                                //获取当前应用程序集
			//	string assemblyName = new AssemblyName(assembly.FullName).Name;                     //获取当前应用程序集名称
			//	string failName = new AssemblyName(args.Name).Name;                                 //获取加载失败的应用程序集名称
			//	string libPath = string.Format("{0}.App.Lib.{1}.dll", assemblyName, failName);      //合成正确嵌入资源的程序集所在的路径 (相对于本命名空间)

			//	using (Stream stream = assembly.GetManifestResourceStream(libPath))                 //根据命名空间将程序集转为Stream
			//	{
			//		byte[] buffer = new byte[stream.Length];
			//		stream.Read(buffer, 0, buffer.Length);      //读取程序集
			//		return Assembly.Load(buffer);               //加载程序集
			//	}
			//};
		}
		/// <summary>
		/// 隐藏构造函数
		/// </summary>
		private LibExport()
		{ }
		#endregion

		#region --导出方法(事件)--
		/// <summary>
		/// 酷Q事件: AppInfo 回调
		/// <para>[请填写] 应用的ApiVer、Appid</para>
		/// <para>请不要在此事件添加其它代码</para>
		/// </summary>
		public static event EventHandler<AppInfoEventArgs> AppInfoEventHandler = Event_AppInitialize.Instance.AppInfo;
		/// <summary>
		/// 酷Q事件: Initialize 回调
		/// <para>应用AuthCode接收</para>
		/// </summary>
		public static event EventHandler<AppInitializeEventArgs> AppInitializeEventHandler = Event_AppInitialize.Instance.Initialize;
		/// <summary>
		/// 酷Q事件: _eventStartup 回调
		/// <para>Type=1001 酷Q启动</para>
		/// </summary>
		public static event EventHandler<EventArgs> CqStartup = Event_AppStatus.Instance.CqStartup;
		/// <summary>
		/// 酷Q事件: _eventExit
		/// <para>Type=1002 酷Q退出</para>
		/// </summary>
		public static event EventHandler<EventArgs> CqExit = Event_AppStatus.Instance.CqExit;
		/// <summary>
		/// 酷Q事件: _eventEnable
		/// <para>Type=1003 应用已被启用</para>
		/// </summary>
		public static event EventHandler<EventArgs> AppEnable = Event_AppStatus.Instance.AppEnable;
		/// <summary>
		/// 酷Q事件: _eventDisable
		/// <para>Type=1004 应用将被停用</para>
		/// </summary>
		public static event EventHandler<EventArgs> AppDisable = Event_AppStatus.Instance.AppDisable;
		/// <summary>
		/// 酷Q事件: _eventPrivateMsg
		/// <para>Type=21 私聊消息 - 好友</para>
		/// </summary>
		public static event EventHandler<PrivateMessageEventArgs> ReceiveFriendMessage = Event_PrivateMessage.Instance.ReceiveFriendMessage;
		/// <summary>
		/// 酷Q事件: _eventPrivateMsg
		/// <para>Type=21 私聊消息 - 在线状态</para>
		/// </summary>
		public static event EventHandler<PrivateMessageEventArgs> ReceiveQnlineStatusMessage = Event_PrivateMessage.Instance.ReceiveOnlineStatusMessage;
		/// <summary>
		/// 酷Q事件: _eventPrivateMsg
		/// <para>Type=21 私聊消息 - 群私聊</para>
		/// </summary>
		public static event EventHandler<PrivateMessageEventArgs> ReceiveGroupPrivateMessage = Event_PrivateMessage.Instance.ReceiveGroupPrivateMessage;
		/// <summary>
		/// 酷Q事件: _eventPrivateMsg
		/// <para>Type=21 私聊消息 - 讨论组私聊</para>
		/// </summary>
		public static event EventHandler<PrivateMessageEventArgs> ReceiveDiscussPrivateMessage = Event_PrivateMessage.Instance.ReceiveDiscussPrivateMessage;
		/// <summary>
		/// 酷Q事件: _eventGroupMsg
		/// <para>Type=2 群消息</para>
		/// </summary>
		public static event EventHandler<GroupMessageEventArgs> ReceiveGroupMessage = Event_GroupMessage.Instance.ReceiveGroupMessage;
		/// <summary>
		/// 酷Q事件: _eventDiscussMsg
		/// <para>Type=4 讨论组消息</para>
		/// </summary>
		public static event EventHandler<DiscussMessageEventArgs> ReceiveDiscussMessage = Event_DiscussMessage.Instance.ReceiveDiscussMessage;
		/// <summary>
		/// 酷Q事件: _eventGroupUpload
		/// <para>Type=11 群文件上传事件</para>
		/// </summary>
		public static event EventHandler<FileUploadMessageEventArgs> ReceiveFileUploadMessage = Event_GroupMessage.Instance.ReceiveGroupFileUpload;
		/// <summary>
		/// 酷Q事件: _eventSystem_GroupAdmin
		/// <para>Type=101 群事件-管理员变动 - 群管理增加</para>
		/// </summary>
		public static event EventHandler<GroupManageAlterEventArgs> ReceiveManageIncrease = Event_GroupMessage.Instance.ReceiveGroupManageIncrease;
		/// <summary>
		/// 酷Q事件: _eventSystem_GroupAdmin
		/// <para>Type=101 群事件-管理员变动 - 群管理减少</para>
		/// </summary>
		public static event EventHandler<GroupManageAlterEventArgs> ReceiveManageDecrease = Event_GroupMessage.Instance.ReceiveGroupManageDecrease;
		/// <summary>
		/// 酷Q事件: _eventSystem_GroupMemberIncrease
		/// <para>Type=103 群事件-群成员增加 - 主动离开</para>
		/// </summary>
		public static event EventHandler<GroupMemberAlterEventArgs> ReceiveMemberLeave = Event_GroupMessage.Instance.ReceiveGroupMemberLeave;
		/// <summary>
		/// 酷Q事件: _eventSystem_GroupMemberIncrease
		/// <para>Type=103 群事件-群成员增加 - 成员移除</para>
		/// </summary>
		public static event EventHandler<GroupMemberAlterEventArgs> ReceiveMemberRemove = Event_GroupMessage.Instance.ReceiveGroupMemberRemove;
		/// <summary>
		/// 酷Q事件: _eventSystem_GroupMemberIncrease
		/// <para>Type=103 群事件-群成员增加 - 主动加群</para>
		/// </summary>
		public static event EventHandler<GroupMemberAlterEventArgs> ReceiveMemberJoin = Event_GroupMessage.Instance.ReceiveGroupMemberJoin;
		/// <summary>
		/// 酷Q事件: _eventSystem_GroupMemberIncrease
		/// <para>Type=103 群事件-群成员增加 - 邀请入群</para>
		/// </summary>
		public static event EventHandler<GroupMemberAlterEventArgs> ReceiveMemberInvitee = Event_GroupMessage.Instance.ReceiveGroupMemberInvitee;
		/// <summary>
		/// 酷Q事件: _eventFriend_Add
		/// <para>Type=201 好友事件-好友已添加</para>
		/// </summary>
		public static event EventHandler<FriendIncreaseEventArgs> ReceiveFriendIncrease = Event_FriendMessage.Instance.ReceiveFriendIncrease;
		/// <summary>
		/// 酷Q事件: _eventRequest_AddFriend
		/// <para>Type=301 请求-好友添加</para>
		/// </summary>
		public static event EventHandler<FriendAddRequestEventArgs> ReceiveFriendAdd = Event_FriendMessage.Instance.ReceiveFriednAddRequest;
		/// <summary>
		/// 酷Q事件: _eventRequest_AddGroup
		/// <para>Type=302 请求-群添加 - 申请入群</para>
		/// </summary>
		public static event EventHandler<GroupAddRequestEventArgs> ReceiveGroupAddApply = Event_GroupMessage.Instance.ReceiveGroupAddApply;
		/// <summary>
		/// 酷Q事件: _eventRequest_AddGroup
		/// <para>Type=302 请求-群添加 - 被邀入群</para>
		/// </summary>
		public static event EventHandler<GroupAddRequestEventArgs> ReceiveGroupAddInvitee = Event_GroupMessage.Instance.ReceiveGroupAddInvitee;
		#endregion

		#region --导出方法--
		[DllExport(ExportName = "AppInfo", CallingConvention = CallingConvention.StdCall)]
		private static string AppInfo()
		{
			AppInfoEventArgs args = new AppInfoEventArgs();
			AppInfoEventHandler(Instance, args);
			return string.Format("{0},{1}", 9, "top.jiegg.demo");
		}

		[DllExport(ExportName = "Initialize", CallingConvention = CallingConvention.StdCall)]
		private static int Initialize(int authCode)
		{
			AppInitializeEventArgs args = new AppInitializeEventArgs();
			args.AuthCode = authCode;
			AppInitializeEventHandler(Instance, args);
			return 0;
		}

		[DllExport(ExportName = "_eventStartup", CallingConvention = CallingConvention.StdCall)]
		private static int EventStartUp()
		{
			CqStartup(Instance, new EventArgs());
			return 0;
		}

		[DllExport(ExportName = "_eventExit", CallingConvention = CallingConvention.StdCall)]
		private static int EventExit()
		{
			CqExit(Instance, new EventArgs());
			return 0;
		}

		[DllExport(ExportName = "_eventEnable", CallingConvention = CallingConvention.StdCall)]
		private static int EventEnable()
		{
			AppEnable(Instance, new EventArgs());
			return 0;
		}

		[DllExport(ExportName = "_eventDisable", CallingConvention = CallingConvention.StdCall)]
		private static int EventDisable()
		{
			AppDisable(Instance, new EventArgs());
			return 0;
		}

		[DllExport(ExportName = "_eventPrivateMsg", CallingConvention = CallingConvention.StdCall)]
		private static int EventPrivateMsg(int subType, int msgId, long fromQQ, string msg, int font)
		{
			PrivateMessageEventArgs args = new PrivateMessageEventArgs();
			args.MsgId = msgId;
			args.FromQQ = fromQQ;
			args.Msg = msg;
			//args.Font = font;
			args.Handled = false;
			switch (subType)
			{
				case 11:    //来自好友
					ReceiveFriendMessage(Instance, args);
					break;
				case 1:     //来自在线状态
					ReceiveQnlineStatusMessage(Instance, args);
					break;
				case 2:     //来自群
					ReceiveGroupPrivateMessage(Instance, args);
					break;
				case 3:     //来自讨论组
					ReceiveDiscussPrivateMessage(Instance, args);
					break;
				default:    //其它类型
					EnApi.Instance.AddLoger(LogerLevel.Info, "提示", "新私聊类型, 请反馈给开发者或自行添加!");
					break;
			}
			return (int)(args.Handled ? MessageHanding.Intercept : MessageHanding.Ignored); //如果处理过就截断消息
		}

		[DllExport(ExportName = "_eventGroupMsg", CallingConvention = CallingConvention.StdCall)]
		private static int EventGroupMsg(int subType, int msgId, long fromGroup, long fromQQ, string fromAnonymous, string msg, int font)
		{
			GroupMessageEventArgs args = new GroupMessageEventArgs();
			args.MsgId = msgId;
			args.FromGroup = fromGroup;
			args.FromQQ = fromQQ;
			args.Msg = msg;
			//args.Font = font;
			args.Handled = false;

			if (fromQQ == 80000000 && !string.IsNullOrEmpty(fromAnonymous))
			{
				args.FromAnonymous = EnApi.Instance.GetAnonymous(fromAnonymous); //获取匿名成员信息
			}
			else
			{
				args.FromAnonymous = null;
			}

			switch (subType)
			{
				case 1:     //群消息
					ReceiveGroupMessage(Instance, args);
					break;
				default:    //其它类型
					EnApi.Instance.AddLoger(LogerLevel.Info, "提示", "新群消息类型, 请反馈给开发者或自行添加!");
					break;
			}
			return (int)(args.Handled ? MessageHanding.Intercept : MessageHanding.Ignored); //如果处理过就截断消息
		}

		[DllExport(ExportName = "_eventDiscussMsg", CallingConvention = CallingConvention.StdCall)]
		private static int EventDiscussMsg(int subType, int msgId, long fromDiscuss, long fromQQ, string msg, int font)
		{
			DiscussMessageEventArgs args = new DiscussMessageEventArgs();
			args.MsgId = msgId;
			args.FromDiscuss = fromDiscuss;
			args.FromQQ = fromQQ;
			args.Msg = msg;
			//args.Font = font;
			args.Handled = false;
			switch (subType)
			{
				case 1:     //群消息
					ReceiveDiscussMessage(Instance, args);
					break;
				default:    //其它类型
					EnApi.Instance.AddLoger(LogerLevel.Info, "提示", "新讨论组消息类型, 请反馈给开发者或自行添加!");
					break;
			}
			return (int)(args.Handled ? MessageHanding.Intercept : MessageHanding.Ignored); //如果处理过就截断消息
		}

		[DllExport(ExportName = "_eventGroupUpload", CallingConvention = CallingConvention.StdCall)]
		private static int EventGroupUpload(int subType, int sendTime, long fromGroup, long fromQQ, string file)
		{
			FileUploadMessageEventArgs args = new FileUploadMessageEventArgs();
			args.SendTime = Time.FormatUnixTime(sendTime.ToString());
			args.FromGroup = fromGroup;
			args.FromQQ = fromQQ;
			args.File = EnApi.Instance.GetFile(file);
			ReceiveFileUploadMessage(Instance, args);
			return (int)(args.Handled ? MessageHanding.Intercept : MessageHanding.Ignored); //如果处理过就截断消息
		}

		[DllExport(ExportName = "_eventSystem_GroupAdmin", CallingConvention = CallingConvention.StdCall)]
		private static int EventSystemGroupAdmin(int subType, int sendTime, long fromGroup, long beingOperateQQ)
		{
			GroupManageAlterEventArgs args = new GroupManageAlterEventArgs();
			args.SendTime = Time.FormatUnixTime(sendTime.ToString());
			args.FromGroup = fromGroup;
			args.BeingOperateQQ = beingOperateQQ;
			args.Handled = false;
			switch (subType)
			{
				case 1:     //被取消管理员
					ReceiveManageDecrease(Instance, args);
					break;
				case 2:     //被设置管理员
					ReceiveManageIncrease(Instance, args);
					break;
				default:
					EnApi.Instance.AddLoger(LogerLevel.Info, "提示", "新管理事件类型, 请反馈给开发者或自行添加!");
					break;
			}
			return (int)(args.Handled ? MessageHanding.Intercept : MessageHanding.Ignored); //如果处理过就截断消息
		}

		[DllExport(ExportName = "_eventSystem_GroupMemberDecrease", CallingConvention = CallingConvention.StdCall)]
		private static int EventSystemGroupMemberDecrease(int subType, int sendTime, long fromGroup, long fromQQ, long beingOperateQQ)
		{
			GroupMemberAlterEventArgs args = new GroupMemberAlterEventArgs();
			args.SendTime = Time.FormatUnixTime(sendTime.ToString());
			args.FromGroup = fromGroup;
			args.FromQQ = fromQQ;
			args.BeingOperateQQ = beingOperateQQ;
			args.Handled = false;
			switch (subType)
			{
				case 1:     //群员离开
					fromQQ = beingOperateQQ;    //操作者QQ此时为离开者
					ReceiveMemberLeave(Instance, args);
					break;
				case 2:     //群员被踢
					ReceiveMemberRemove(Instance, args);
					break;
				default:
					EnApi.Instance.AddLoger(LogerLevel.Info, "提示", "新群成员减少事件类型, 请反馈给开发者或自行添加!");
					break;
			}
			return (int)(args.Handled ? MessageHanding.Intercept : MessageHanding.Ignored); //如果处理过就截断消息
		}

		[DllExport(ExportName = "_eventSystem_GroupMemberIncrease", CallingConvention = CallingConvention.StdCall)]
		private static int EventSystemGroupMemberIncrease(int subType, int sendTime, long fromGroup, long fromQQ, long beingOperateQQ)
		{
			GroupMemberAlterEventArgs args = new GroupMemberAlterEventArgs();
			args.SendTime = Time.FormatUnixTime(sendTime.ToString());
			args.FromGroup = fromGroup;
			args.FromQQ = fromQQ;
			args.BeingOperateQQ = beingOperateQQ;
			args.Handled = false;
			switch (subType)
			{
				case 1:     //管理员已同意
					ReceiveMemberJoin(Instance, args);
					break;
				case 2:     //管理员邀请
					ReceiveMemberInvitee(Instance, args);
					break;
				default:    //其它类型
					EnApi.Instance.AddLoger(LogerLevel.Info, "提示", "新群成员增加事件类型, 请反馈给开发者或自行添加!");
					break;
			}
			return (int)(args.Handled ? MessageHanding.Intercept : MessageHanding.Ignored); //如果处理过就截断消息
		}

		[DllExport(ExportName = "_eventFriend_Add", CallingConvention = CallingConvention.StdCall)]
		private static int EventFriendAdd(int subType, int sendTime, long fromQQ)
		{
			FriendIncreaseEventArgs args = new FriendIncreaseEventArgs();
			args.SendTime = Time.FormatUnixTime(sendTime.ToString());
			args.FromQQ = fromQQ;
			args.Handled = false;
			switch (subType)
			{
				case 1:     //好友已添加
					ReceiveFriendIncrease(Instance, args);
					break;
				default:    //其它类型
					EnApi.Instance.AddLoger(LogerLevel.Info, "提示", "新好友事件类型, 请反馈给开发者或自行添加!");
					break;
			}
			return (int)(args.Handled ? MessageHanding.Intercept : MessageHanding.Ignored); //如果处理过就截断消息
		}

		[DllExport(ExportName = "_eventRequest_AddFriend", CallingConvention = CallingConvention.StdCall)]
		private static int EventRequestAddFriend(int subType, int sendTime, long fromQQ, string msg, string responseFlag)
		{
			FriendAddRequestEventArgs args = new FriendAddRequestEventArgs();
			args.SendTime = Time.FormatUnixTime(sendTime.ToString());
			args.FromQQ = fromQQ;
			args.AppendMsg = msg;
			args.Tag = responseFlag;
			args.Handled = false;
			switch (subType)
			{
				case 1:     //好友添加请求
					ReceiveFriendAdd(Instance, args);
					break;
				default:    //其它类型
					EnApi.Instance.AddLoger(LogerLevel.Info, "提示", "新好友添加请求事件类型, 请反馈给开发者或自行添加!");
					break;
			}
			return (int)(args.Handled ? MessageHanding.Intercept : MessageHanding.Ignored); //如果处理过就截断消息
		}

		[DllExport(ExportName = "_eventRequest_AddGroup", CallingConvention = CallingConvention.StdCall)]
		private static int EventRequestAddGroup(int subType, int sendTime, long fromGroup, long fromQQ, string msg, string responseFlag)
		{
			GroupAddRequestEventArgs args = new GroupAddRequestEventArgs();
			args.SednTime = Time.FormatUnixTime(sendTime.ToString());
			args.FromGroup = fromGroup;
			args.FromQQ = fromQQ;
			args.AppendMsg = msg;
			args.Tag = responseFlag;
			args.Handled = false;
			switch (subType)
			{
				case 1:     //他人申请加群
					ReceiveGroupAddApply(Instance, args);
					break;
				case 2:     //登录号受邀入群
					ReceiveGroupAddInvitee(Instance, args);
					break;
				default:
					EnApi.Instance.AddLoger(LogerLevel.Info, "提示", "新群添加请求事件类型, 请反馈给开发者或自行添加!");
					break;
			}
			return 0;
		}
		#endregion

		/*
		 *	关于拓展方法:
		 *		1. 所有拓展方法需要需要使用 "DllExport" 进行标记公开
		 *		2. 所有拓展方法请遵循规划写相应的触发事件, 并将事件实现在 Event_UserExpand 中, 以便将面向过程转化为面向对象
		 *		3. 传递的参数请在 "App.Model" 下创建对应的模型, 并继承 EventArgs 
		 *		4. 所有引用到的第三方 "程序集" 请将其 "库文件名" 命名为其程序集的命名空间, 方便CLR加载
		 *		5. 第三方程序集请统一放在 "App.Lib" 文件夹下
		 *	
		 *	至此! 
		 */

		#region --拓展方法(事件)--
		/// <summary>
		/// 用户事件: 打开控制台
		/// </summary>
		public static event EventHandler<EventArgs> UserOpenConsole = Event_UserExpand.Instance.OpenConsoleWindow;
		#endregion

		#region --拓展方法--
		[DllExport(ExportName = "_eventOpenConsole", CallingConvention = CallingConvention.StdCall)]
		private static int EventOpenConsole()
		{
			UserOpenConsole(Instance, new EventArgs());
			return 0;
		}
		#endregion
	}
}
