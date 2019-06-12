using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.EventArgs
{
	/// <summary>
	/// 表示酷Q启动事件参数的类
	/// </summary>
	public class CqStartupEventArgs : CqEventArgsBase
	{
		/// <summary>
		/// 获取一个值, 该值表示当前事件的类型
		/// </summary>
		public override int Type { get { return 1001; } }

		/// <summary>
		/// 获取一个值, 该值表示当前事件的名称
		/// </summary>
		public override string Name { get { return "酷Q启动"; } }

		/// <summary>
		/// 初始化 <see cref="CqStartupEventArgs"/> 类的一个新实例
		/// </summary>
		/// <param name="id">事件ID</param>
		public CqStartupEventArgs (int id)
		{
			this.Id = id;
		}
	}
}
