using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// 群信息
	/// </summary>
	public class Group
	{
		/// <summary>
		/// 群号码
		/// </summary>
		public long Id { get; set; }
		/// <summary>
		/// 群名字
		/// </summary>
		public string Name { get; set; }
	}
}
