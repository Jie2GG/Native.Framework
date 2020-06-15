using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Native.Sdk.Cqp.Enum
{
	/// <summary>
	/// 表示QQ性别的枚举
	/// </summary>
	public enum QQSex
	{
		/// <summary>
		/// 男性
		/// </summary>
		[Description ("男")]
		Man = 0,
		/// <summary>
		/// 女性 
		/// </summary>
		[Description ("女")]
		Woman = 1,
		/// <summary>
		/// 未知
		/// </summary>
		[Description ("未知")]
		Unknown = 255
	}
}
