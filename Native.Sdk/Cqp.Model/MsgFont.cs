using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Native.Sdk.Cqp.Model
{
	/// <summary>
	/// 消息字体
	/// </summary>
	public class MsgFont
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
