using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.EventArgs
{
	/// <summary>
	/// 表示酷Q事件参数基类
	/// </summary>
	public abstract class CqEventArgsBase : System.EventArgs
	{
		/// <summary>
		/// 获取一个值, 表示当前事件在 Json 中定义的 ID
		/// </summary>
		public int Id { get; protected set; }

        /// <summary>
		/// 获取一个值, 表示当前事件在 Json 中定义的 Name
		/// </summary>
		public string Name { get; protected set; }

        /// <summary>
        /// 当在派生类中重写时, 表示当前事件的类型
        /// </summary>
        public abstract int Type { get; }
	}
}
