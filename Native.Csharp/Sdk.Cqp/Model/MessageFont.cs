using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// 消息字体
	/// </summary>
	[Obsolete("由于酷Q废弃 Font 参数, 本模型弃用")]
	public class MessageFont
	{
		/// <summary>
		/// 字体名称
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// 字体大小
		/// </summary>
		public int Size { get; set; }
		/// <summary>
		/// 字体颜色
		/// </summary>
		public int Color { get; set; }
		/// <summary>
		/// 字体风格
		/// </summary>
		public int Style { get; set; }
		/// <summary>
		/// 气泡样式
		/// </summary>
		public int Bubble { get; set; }
	}
}
