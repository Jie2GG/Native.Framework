using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native.Csharp.Sdk.Cqp.EventArgs;

namespace Native.Csharp.Sdk.Cqp.Interface
{
	/// <summary>
	/// Type=102 群事件 - 群成员减少 - 成员移除, 事件接口
	/// </summary>
	public interface IReceiveGroupMemberRemove
	{
		/// <summary>
		/// 当在派生类中重写时, 处理收到的群成员减少 (成员移除) 事件
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void ReceiveGroupMemberRemove (object sender, CqGroupMemberDecreaseEventArgs e);
	}
}
