using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.EventArgs
{
	/// <summary>
	/// 提供用于描述酷Q菜单调用事件参数的类
	/// </summary>
	public class CQMenuCallEventArgs : CQMenuEventArgs
	{
		#region --构造函数--
		/// <summary>
		/// 初始化 <see cref="CQMenuCallEventArgs"/> 类的新实例
		/// </summary>
		/// <param name="name"></param>
		/// <param name="function"></param>
		public CQMenuCallEventArgs (string name, string function)
			: base (name, function)
		{
		} 
		#endregion
	}
}
