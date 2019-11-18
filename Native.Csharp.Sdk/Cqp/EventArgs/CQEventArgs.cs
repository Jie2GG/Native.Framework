using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.EventArgs
{
	/// <summary>
	/// 提供用于描述酷Q事件参数的基础类, 该类是抽象的
	/// </summary>
	public abstract class CQEventArgs
	{
		#region --属性--
		/// <summary>
		/// 获取当前事件的回调函数名称. 是 function 字段
		/// </summary>
		public string Function { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化 <see cref="CQEventArgs"/> 类的新实例
		/// </summary>
		/// <param name="function">触发此事件的函数名称</param>
		public CQEventArgs (string function)
		{
			this.Function = function;
		}
		#endregion
	}
}
