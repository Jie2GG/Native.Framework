using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native.Csharp.Sdk.Cqp.EventArgs;

namespace Native.Csharp.Sdk.Cqp.Interface
{
	/// <summary>
	/// Type=21 在线状态消息, 事件接口
	/// </summary>
	public interface IReceiveOnlineStatusMessage
	{
		/// <summary>
		/// 当在派生类中重写时, 处理收到的在线状态消息
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void ReceiveOnlineStatusMessage (object sender, CqPrivateMessageEventArgs e);
	}
}
