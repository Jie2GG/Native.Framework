using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Enum
{
	/// <summary>
	/// 日志等级
	/// </summary>
	public enum LogerLevel
	{
		/// <summary>
		/// 调试
		/// </summary>
		Debug = 0,
		/// <summary>
		/// 信息
		/// </summary>
		Info = 10,
		/// <summary>
		/// 信息_成功
		/// </summary>
		Info_Success = 11,
		/// <summary>
		/// 信息_接收
		/// </summary>
		Info_Receive = 12,
		/// <summary>
		/// 信息_发送
		/// </summary>
		Info_Send = 13,
		/// <summary>
		/// 警告
		/// </summary>
		Warning = 20,
		/// <summary>
		/// 错误
		/// </summary>
		Error = 30,
		/// <summary>
		/// 严重错误
		/// </summary>
		Fatal = 40
	}
}
