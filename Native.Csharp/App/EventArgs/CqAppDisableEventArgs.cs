using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.EventArgs
{
	/// <summary>
	/// 表示应用停用事件参数的类
	/// </summary>
	public class CqAppDisableEventArgs : CqEventArgsBase
	{
		/// <summary>
		/// 获取一个值, 该值表示当前事件的类型
		/// </summary>
		public override int Type { get { return 1004; } }

		/// <summary>
		/// 获取一个值, 该值表示当前事件的名称
		/// </summary>
		public override string Name { get { return "应用停用"; } }

		/// <summary>
		/// 初始化 <see cref="CqAppDisableEventArgs"/> 类的一个新实例
		/// </summary>
		/// <param name="id">事件ID</param>
		public CqAppDisableEventArgs (int id)
		{
			this.Id = id;
		}
	}
}
