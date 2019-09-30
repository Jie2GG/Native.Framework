using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.EventArgs
{
	/// <summary>
	/// 表示应用启动事件参数的类
	/// </summary>
	public class CqAppEnableEventArgs : CqEventArgsBase
	{
		/// <summary>
		/// 获取一个值, 该值表示当前事件的类型
		/// </summary>
		public override int Type { get { return 1003; } }
        
		/// <summary>
		/// 初始化 <see cref="CqAppEnableEventArgs"/> 类的一个新实例
		/// </summary>
		/// <param name="id">事件ID</param>
        /// <param name="name">事件名称</param>
		public CqAppEnableEventArgs (int id, string name)
		{
			base.Id = id;
            base.Name = name;
		}
	}
}
