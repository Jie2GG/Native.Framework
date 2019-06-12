using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.EventArgs
{
	/// <summary>
	/// 表示酷Q事件参数基类
	/// </summary>
	public abstract class CqEventArgsBase : System.EventArgs
	{
		/// <summary>
		/// 当在派生类中重写时, 表示当前事件的ID
		/// </summary>
		public int Id { get; internal set; }

		/// <summary>
		/// 当在派生类中重写时, 表示当前事件的类型
		/// </summary>
		public abstract int Type { get; }

		/// <summary>
		/// 当在派生类中重写时, 表示当前事件的名称
		/// </summary>
		public abstract string Name { get; }
	}
}
