using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.EventArgs
{
	/// <summary>
	/// 提供用于描述酷Q菜单类事件参数的基础类, 该类是抽象的
	/// </summary>
	public abstract class CQMenuEventArgs : CQEventArgs
	{
		#region --属性--
		/// <summary>
		/// 获取来源事件的事件名称. 是 name 字段
		/// </summary>
		public string Name { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化 <see cref="CQMenuEventArgs"/> 类的新实例
		/// </summary>
		/// <param name="name">菜单名称</param>
		/// <param name="function">函数名</param>
		public CQMenuEventArgs (string name, string function)
			: base (function)
		{
			this.Name = name;
		}
		#endregion
	}
}
