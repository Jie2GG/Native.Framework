using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.App.Model
{
	public class GroupMemberAlterEventArgs : EventArgsBase
	{
		/// <summary>
		/// 发送时间
		/// </summary>
		public DateTime SendTime { get; set; }
		/// <summary>
		/// 来源群号
		/// </summary>
		public long FromGroup { get; set; }
		/// <summary>
		/// 被操作QQ
		/// </summary>
		public long BeingOperateQQ { get; set; }
	}
}
