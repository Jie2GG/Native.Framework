using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.App.Model
{
	public class FriendAddRequestEventArgs : EventArgs
	{
		/// <summary>
		/// 发送时间
		/// </summary>
		public DateTime SendTime { get; set; }
		/// <summary>
		/// 来源QQ
		/// </summary>
		public long FromQQ { get; set; }
		/// <summary>
		/// 附加消息
		/// </summary>
		public string AppendMsg { get; set; }
		/// <summary>
		/// 反馈标识(处理请求时使用)
		/// </summary>
		public string Tag { get; set; }

		/// <summary>
		/// 获取或设置一个值，该值指示是否处理过此事件
		/// </summary>
		public bool Handled { get; set; }
	}
}
