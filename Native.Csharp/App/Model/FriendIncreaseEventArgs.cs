using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.App.Model
{
	public class FriendIncreaseEventArgs : EventArgsBase
	{
		/// <summary>
		/// 发送时间
		/// </summary>
		public DateTime SendTime { get; set; }
	}
}
