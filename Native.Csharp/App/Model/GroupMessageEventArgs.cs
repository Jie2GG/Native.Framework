using Native.Csharp.Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.App.Model
{
	public class GroupMessageEventArgs : EventArgs
	{
		/// <summary>
		/// 消息Id
		/// </summary>
		public int MsgId { get; set; }
		
		/// <summary>
		/// 来源群号
		/// </summary>
		public long FromGroup { get; set; }
		
		/// <summary>
		/// 来源QQ
		/// </summary>
		public long FromQQ { get; set; }
		
		/// <summary>
		/// 是否是匿名消息
		/// </summary>
		public bool IsAnonymousMsg { get; set; }

		/// <summary>
		/// 来源匿名
		/// </summary>
		public GroupAnonymous FromAnonymous { get; set; }
		
		/// <summary>
		/// 消息内容
		/// </summary>
		public string Msg { get; set; }

		/// <summary>
		/// 获取或设置一个值，该值指示是否处理过此事件
		/// </summary>
		public bool Handled { get; set; }
	}
}
