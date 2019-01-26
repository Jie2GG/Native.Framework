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
		/*
		 *	关于导出方法:
		 *	
		 *		1. 需要导出给 酷Q 调用的方法都需要使用 "DllExport" 进行标记
		 *		2. 使用 "DllExport" 标记时请注意 "ExportName" (导出函数名, 应与 Json 文件中的函数名一致), 调用方式为 StdCall 
		 *		3. 请按照规范编写导出方法对外的静态触发事件, 在 IEvent_UserExpand 中添加事件接口, 并将事件的实现在 Event_UserExpand 中, 一遍将面向过程转换为面向对象
		 *		4. 相关的参数传递请使用面向对象的方式传递, 并在 "Model" 文件夹下创建对应的模型, 模型以 "EventArgs" 结尾, 并且继承 "System.EventArgs" 类
		 *		5. 最后请在 "Event_Main" 中注入有关该事件实现的依赖项
		 *		
		 *		
		 *	至此! 
		 */

		#region --回调事件--
		/// <summary>
		/// 用户打开控制台事件
		/// </summary>
		public static event EventHandler<EventArgs> UserOpenConsole = (sender, e) => { };
		#endregion

		#region --导出方法--
		[DllExport (ExportName = "_eventOpenConsole", CallingConvention = CallingConvention.StdCall)]
		private static int EventOpenConsole ()
		{
			UserOpenConsole (null, new EventArgs ());
			return 0;
		}
		#endregion
	}
}
