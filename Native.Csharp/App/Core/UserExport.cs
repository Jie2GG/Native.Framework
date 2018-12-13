using Native.Csharp.App.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Native.Csharp.App.Core
{
	public class UserExport
	{
		#region --字段--
		private static Lazy<UserExport> _instance = new Lazy<UserExport>(() => new UserExport());
		#endregion

		#region --属性--
		/// <summary>
		/// 获取 UserExport 实例对象
		/// </summary>
		public static UserExport Instance { get { return _instance.Value; } }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 隐藏构造函数
		/// </summary>
		private UserExport()
		{ }
		#endregion

		/*
		 *	关于导出方法:
		 *		1. 所有需要被酷Q调用的方法需要使用 "DllExport" 进行标记
		 *		2. 请按照规范编写导出方法对应的触发事件, 并将事件实现在 Event_UserExpand 中, 以便将面向过程转换为面向对象
		 *		3. 传递的参数请在 'Model" 文件夹下创建对应的模型名称, 类名以 "EventArgs" 结尾, 并且继承 "EventArgs" 类
		 *		4. 当调用事件时, 请为名称为 "sender" 的参数, 传入 "Instance" 变量作为触发事件的实例对象
		 *		5. 关于返回值的问题, 请使用面向对象的方式进行解决
		 *		
		 *	至此! 
		 */

		#region --回调事件--
		public static event EventHandler<EventArgs> UserOpenConsole = Event_UserExpand.Instance.OpenConsoleWindow;
		#endregion

		#region --导出方法--
		[DllExport(ExportName = "_eventOpenConsole", CallingConvention = CallingConvention.StdCall)]
		[CoolQMenu("打开控制台", "_eventOpenConsole")]
		private static int EventOpenConsole()
		{
			/*
			 * 返回值的问题, 请使用面向对象的方式处理
			 */
			UserOpenConsole(Instance, new EventArgs());
			return 0;
		}
		#endregion
	}
}
