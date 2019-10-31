using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// 描述 QQ 好友信息的类
	/// </summary>
	public class FriendInfo
	{
		/// <summary>
		/// QQ帐号
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		/// QQ昵称
		/// </summary>
		public string Nick { get; set; }

		/// <summary>
		/// 备注信息
		/// </summary>
		public string Note { get; set; }
	}
}
