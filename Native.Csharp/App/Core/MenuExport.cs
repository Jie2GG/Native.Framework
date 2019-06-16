/*
 *	此代码由 T4 引擎根据 MenuExport.tt 模板生成, 若您不了解以下代码的用处, 请勿修改!
 *	
 *	此文件包含项目 Json 文件的菜单导出函数.
 */
using System;
using System.Runtime.InteropServices;
using System.Text;
using Native.Csharp.App.EventArgs;
using Native.Csharp.App.Interface;
using Unity;

namespace Native.Csharp.App.Core
{
    public class MenuExport
    {
		#region --构造函数--
		/// <summary>
		/// 静态构造函数, 注册依赖注入回调
		/// </summary>
        static MenuExport ()
        {
			// 分发应用内事件
			ResolveAppbackcall ();
        }
        #endregion

		#region --私有方法--
		/// <summary>
		/// 获取所有的注入项, 分发到对应的事件
		/// </summary>
		private static void ResolveAppbackcall ()
		{
			/*
			 * Name: 打开控制台
			 * Function: _eventOpenConsole
			 */
			if (Common.UnityContainer.IsRegistered<ICallMenu> ("打开控制台") == true)
			{
				Menu__eventOpenConsole = Common.UnityContainer.Resolve<ICallMenu> ("打开控制台").CallMenu;
			}


		}
        #endregion

		#region --导出方法--
		/*
		 * Name: 打开控制台
		 * Function: _eventOpenConsole
		 */
		public static event EventHandler<CqCallMenuEventArgs> Menu__eventOpenConsole;
		[DllExport (ExportName = "_eventOpenConsole", CallingConvention = CallingConvention.StdCall)]
		private static int Evnet__eventOpenConsole ()
		{
			if (Menu__eventOpenConsole != null)
			{
				Menu__eventOpenConsole (null, new CqCallMenuEventArgs ("打开控制台"));
			}
			return 0;
		}


		#endregion
    }
}

