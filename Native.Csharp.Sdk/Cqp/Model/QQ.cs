using Native.Csharp.Sdk.Cqp.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// QQ信息
	/// </summary>
	public class QQInfo
	{
		/// <summary>
		/// QQ号
		/// </summary>
		public long Id { get; set; }
		/// <summary>
		/// 性别
		/// </summary>
		public Sex Sex { get; set; }
		/// <summary>
		/// 年龄
		/// </summary>
		public int Age { get; set; }
		/// <summary>
		/// 昵称
		/// </summary>
		public string Nick { get; set; }
	}
}
