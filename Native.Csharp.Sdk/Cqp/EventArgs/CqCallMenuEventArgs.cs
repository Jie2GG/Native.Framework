using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.EventArgs
{
	/// <summary>
	/// 表示菜单被调用的参数的类
	/// </summary>
	public class CqCallMenuEventArgs : System.EventArgs
	{
		/// <summary>
		/// 获取调用菜单的菜单名称
		/// </summary>
		public string Menu { get; private set; }

		/// <summary>
		/// 初始化 <see cref="CqCallMenuEventArgs"/> 类的一个新实例
		/// </summary>
		/// <param name="menu">菜单名称</param>
		public CqCallMenuEventArgs (string menu)
		{
			this.Menu = menu;
		}
	}
}
