using Native.Csharp.Sdk.Cqp.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// 描述 QQ 的类
	/// </summary>
	public class QQInfo
	{
		/// <summary>
		/// 获取唯一标识符, 即QQ号码
		/// </summary>
		public long Id { get; set; }
		/// <summary>
		/// 获取当前实例性别 <see cref="Enum.Sex"/>
		/// </summary>
		public Sex Sex { get; set; }
		/// <summary>
		/// 获取当前实例年龄
		/// </summary>
		public int Age { get; set; }
		/// <summary>
		/// 获取当前实例昵称
		/// </summary>
		public string Nick { get; set; }
	}
}
