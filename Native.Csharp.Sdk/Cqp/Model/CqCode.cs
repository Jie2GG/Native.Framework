using Native.Csharp.Sdk.Cqp.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Model
{
	public class CqCode
	{
		#region --属性--
		/// <summary>
		/// 获取一个值, 该值指示当前实例所表示的 CQ码 的类型
		/// </summary>
		public CqCodeType Type { get; set; }

		/// <summary>
		/// 获取一个值, 指示该 CQ码 中所有的键值对
		/// </summary>
		public Dictionary<string, string> Dictionary { get; set; }
		#endregion
	}
}
