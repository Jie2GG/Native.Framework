using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using Unity;
using Native.Csharp.App.Core;
using Native.Csharp.App.Interface;
using Native.Csharp.App.Model;
using Native.Csharp.Sdk.Cqp;

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
			// 当需要注册自己的回调类型时
			// 在此写上需要注册的回调类型, 以 <接口, 实现类> 的方式进行注册, 同时需要给所有注入的类进行命名
			// 
			// 如果注入时没有提供名称, 则 SDK 在分发事件时将无法获取到对应的类型的实例!!!
			// 如果注入时没有提供名称, 则 SDK 在分发事件时将无法获取到对应的类型的实例!!!
			// 如果注入时没有提供名称, 则 SDK 在分发事件时将无法获取到对应的类型的实例!!!
			// 重要的事情说三遍!!!
			// 
			// 下列代码演示的是如何将 SDK 预置的实现类注入到容器中
			#region --回调注入
			// 注册 Event_AppStatus 类, 继承于 IEvent_AppStatus
			container.RegisterType<IEvent_AppStatus, Event_AppStatus> ("Default_AppStatus");

			// 注册 Event_DiscussMessage 类, 继承于 IEvent_DiscussMessage
			container.RegisterType<IEvent_DiscussMessage, Event_DiscussMessage> ("Default_DiscussMessage");

			// 注册 IEvent_FriendMessage 类, 继承于 Event_FriendMessage
			container.RegisterType<IEvent_FriendMessage, Event_FriendMessage> ("Default_FriendMessage");

			// 注册 IEvent_GroupMessage 类, 继承于 Event_GroupMessage
			container.RegisterType<IEvent_GroupMessage, Event_GroupMessage> ("Default_GroupMessage");

			// 注册 IEvent_OtherMessage 类, 继承于 Event_OtherMessage
			container.RegisterType<IEvent_OtherMessage, Event_OtherMessage> ("Default_OtherMessage");
			#endregion

			// 当需要新注册回调类型时
			// 在此写上需要注册的回调类型, 以 <接口, 实现类> 的方式进行注册
			// 下列代码演示的是如何将 IEvent_UserExpand 的实现类 Event_UserExpand 类注入到容器中
			container.RegisterType<IEvent_UserExpand, Event_UserExpand> ();
		}

		/// <summary>
		/// 回调分发
		/// </summary>
		/// <param name="container"></param>
		public static void Resolvebackcall (IUnityContainer container)
		{
			// 当已经注入了新的回调类型时
			// 在此分发已经注册的回调类型, 解析完毕后分发到导出的事件进行注册
			// 下列代码演示如何将 IEvent_UserExpand 接口实例化并拿到对应的实例
			IEvent_UserExpand userExpand = container.Resolve<IEvent_UserExpand> ();
			UserExport.UserOpenConsole += userExpand.OpenConsoleWindow;
		}
	}
}
