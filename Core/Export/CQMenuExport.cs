/*
 * 此文件由T4引擎自动生成, 请勿修改此文件中的代码!
 */
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Native.Core.Domain;
using Native.Sdk.Cqp;
using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;
using Unity;

namespace Native.App.Export
{
	/// <summary>	
	/// 表示酷Q菜单导出的类	
	/// </summary>	
	public class CQMenuExport	
	{	
		#region --构造函数--	
		/// <summary>	
		/// 由托管环境初始化的 <see cref="CQMenuExport"/> 的新实例	
		/// </summary>	
		static CQMenuExport ()	
		{	
			
			// 调用方法进行实例化	
			ResolveBackcall ();	
		}	
		#endregion	
		
		#region --私有方法--	
		/// <summary>	
		/// 读取容器中的注册项, 进行事件分发	
		/// </summary>	
		private static void ResolveBackcall ()	
		{	
			/*	
			 * Name: 醋Q聊天窗	
			 * Function: _menuA	
			 */	
			if (AppData.UnityContainer.IsRegistered<IMenuCall> ("醋Q聊天窗"))	
			{	
				Menu_menuAHandler += AppData.UnityContainer.Resolve<IMenuCall> ("醋Q聊天窗").MenuCall;	
			}	
			
			/*	
			 * Name: WinForm	
			 * Function: _menuB	
			 */	
			if (AppData.UnityContainer.IsRegistered<IMenuCall> ("WinForm"))	
			{	
				Menu_menuBHandler += AppData.UnityContainer.Resolve<IMenuCall> ("WinForm").MenuCall;	
			}	
			
		}	
		#endregion	
		
		#region --导出方法--	
		/*	
		 * Name: 醋Q聊天窗	
		 * Function: _menuA	
		 */	
		public static event EventHandler<CQMenuCallEventArgs> Menu_menuAHandler;	
		[DllExport (ExportName = "_menuA", CallingConvention = CallingConvention.StdCall)]	
		public static int Menu_menuA ()	
		{	
			if (Menu_menuAHandler != null)	
			{	
				CQMenuCallEventArgs args = new CQMenuCallEventArgs (AppData.CQApi, AppData.CQLog, "醋Q聊天窗", "_menuA");	
				Menu_menuAHandler (typeof (CQMenuExport), args);	
			}	
			return 0;	
		}	
		
		/*	
		 * Name: WinForm	
		 * Function: _menuB	
		 */	
		public static event EventHandler<CQMenuCallEventArgs> Menu_menuBHandler;	
		[DllExport (ExportName = "_menuB", CallingConvention = CallingConvention.StdCall)]	
		public static int Menu_menuB ()	
		{	
			if (Menu_menuBHandler != null)	
			{	
				CQMenuCallEventArgs args = new CQMenuCallEventArgs (AppData.CQApi, AppData.CQLog, "WinForm", "_menuB");	
				Menu_menuBHandler (typeof (CQMenuExport), args);	
			}	
			return 0;	
		}	
		
		#endregion	
	}	
}
