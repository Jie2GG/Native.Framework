using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.App.Model
{
	public class FriendAddRequestEventArgs : EventArgsBase
	{
		/// <summary>
		/// 发送时间
		/// </summary>
		public DateTime SendTime { get; set; }
		/// <summary>
		/// 附加消息
		/// </summary>
		public string AppendMsg { get; set; }
		/// <summary>
		/// 反馈标识(处理请求时使用)
		/// </summary>
		public string Tag { get; set; }
	}
}
