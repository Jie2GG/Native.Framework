using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// 群匿名
	/// </summary>
	public class GroupAnonymous
	{
		/// <summary>
		/// 匿名用户标识
		/// </summary>
		public long Id { get; set; }
		/// <summary>
		/// 代号
		/// </summary>
		public string CodeName { get; set; }
		/// <summary>
		/// 令牌
		/// </summary>
		public byte[] Token { get; set; }
	}
}
