using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Enum
{
	/// <summary>
	/// 权限类型
	/// </summary>
	public enum PermitType
	{
		/// <summary>
		/// 成员
		/// </summary>
		None = 1,
		/// <summary>
		/// 管理
		/// </summary>
		Manage = 2,
		/// <summary>
		/// 群主
		/// </summary>
		Holder = 3
	}
}
