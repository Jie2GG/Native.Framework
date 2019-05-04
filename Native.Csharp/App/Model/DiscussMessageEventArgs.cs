using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.App.Model
{
	public class DiscussMessageEventArgs : EventArgsBase
	{
		/// <summary>
		/// 消息Id
		/// </summary>
		public int MsgId { get; set; }
		/// <summary>
		/// 来源讨论组
		/// </summary>
		public long FromDiscuss { get; set; }
		/// <summary>
		/// 消息内容
		/// </summary>
		public string Msg { get; set; }
	}
}
