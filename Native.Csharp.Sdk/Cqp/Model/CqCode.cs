using Native.Csharp.Sdk.Cqp.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// 描述 酷Q 消息中一串 [CQ:...] 的对象
	/// </summary>
	public class CqCode
	{
		#region --属性--
		/// <summary>
		/// 获取或设置一个值, 指示当前实例的原始字符串
		/// </summary>
		public string OriginalString { get; set; }

		/// <summary>
		/// 获取或设置一个值, 指示当前实例位于消息上下文的起始索引
		/// </summary>
		public int Index { get; set; }

		/// <summary>
		/// 获取一个值, 该值指示当前实例所表示的 CQ码 的类型
		/// </summary>
		public CqCodeType Type { get; set; }

		/// <summary>
		/// 获取一个值, 指示该 CQ码 中所有的键值对
		/// </summary>
		public Dictionary<string, string> Dictionary { get; set; }
		#endregion

		/// <summary>
		/// 初始化 <see cref="CqCode"/> 类的新实例
		/// </summary>
		public CqCode ()
		{
			this.OriginalString = string.Empty;
			this.Index = 0;
			this.Type = CqCodeType.Unknown;
			this.Dictionary = new Dictionary<string, string> ();
		}
	}
}
