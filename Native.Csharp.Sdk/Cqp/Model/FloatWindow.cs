using Native.Csharp.Sdk.Cqp.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// 浮悬窗
	/// </summary>
	public class FloatWindow
	{
		/// <summary>
		/// 数据
		/// </summary>
		public string Data { get; set; }
		/// <summary>
		/// 单位
		/// </summary>
		public string Unit { get; set; }
		/// <summary>
		/// 颜色
		/// </summary>
		public FloatWindowColors Color { get; set; }
	}
}
