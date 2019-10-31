using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native.Csharp.Sdk.Cqp.EventArgs;

namespace Native.Csharp.Sdk.Cqp.Interface
{
	/// <summary>
	/// Type=302 群事件 - 群请求 - 申请入群, 事件接口
	/// </summary>
	public interface IReceiveAddGroupRequest
	{
		/// <summary>
		/// 当在派生类中重写时, 处理收到的群请求 (申请入群) 事件
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void ReceiveAddGroupRequest (object sender, CqAddGroupRequestEventArgs e);
	}
}
