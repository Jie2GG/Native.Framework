using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Enum
{
	/// <summary>
	/// 消息的处理方式
	/// </summary>
	public enum MessageHanding
	{
		/// <summary>
		/// 忽略消息
		/// </summary>
		Ignored = 0,
		/// <summary>
		/// 拦截消息
		/// </summary>
		Intercept = 1
	}
}
