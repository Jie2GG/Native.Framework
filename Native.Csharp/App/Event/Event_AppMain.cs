using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using Unity;
using Native.Csharp.Sdk.Cqp.Interface;
using Native.Csharp.Sdk.Cqp.EventArgs;
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
            // 此方法的参数 container 是于插件加载时初始化好的反向注入容器 (IOC 容器)
            // 在注入之前请先运行在 Core 文件夹下的所有 .tt 文件, 保证这里的注入能成功
            // 
            // 注入说明: 
            //  1. 消息类 (Json 文件中的 event 节点): 使用 container.RegisterType<接口, 对应实现类> ("Json 文件对应事件的 name 字段的值") 的方式进行注入
            //  2. 菜单类 (Json 文件中的 menu 节点):  使用 container.RegisterType<接口, 对应实现类> ("Json 文件对应事件的 name 字段的值") 的方式进行注入
            //  3. 状态类 (Json 文件中的 status 节点):使用 container.RegisterType<接口, 对应实现类> ("Json 文件对应事件的 name 字段的值") 的方式进行注入
            //
            // 以下为 Json 文件中的 1001, 1002, 1003, 1004 事件的注入

            // 注入 Type=1001 的回调
            container.RegisterType<ICqStartup, Event_CqStartup> ("酷Q启动事件");
            // 注入 Type=1002 的回调
            container.RegisterType<ICqExit, Event_CqExit> ("酷Q关闭事件");
            // 注入 Type=1003 的回调
            container.RegisterType<ICqAppEnable, Event_CqAppEnable> ("应用已被启用");
            // 注入 Type=1004 的回调
            container.RegisterType<ICqAppDisable, Event_CqAppDisable> ("应用将被停用");
        }

		/// <summary>
		/// 回调分发
		/// </summary>
		/// <param name="container"></param>
		public static void Resolvebackcall (IUnityContainer container)
		{
            // 此方法的参数 container 是于插件加载时初始化好的反向注入容器 (IOC 容器)
            // 在此分发需要将指定的对象通过容器进行实例化然后发往对应的位置
            //
            // 说明: 
            //      由于采用了新的容器解析机制, 所以此方法不需要写任何的分发过程
            //      此方法的使用需要熟悉 Unity 框架 (IOC 框架)
		}
	}
}
