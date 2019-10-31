using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// 群信息
	/// </summary>
	public class GroupInfo
	{
		/// <summary>
		/// 群号码
		/// </summary>
		public long Id { get; set; }
		/// <summary>
		/// 群名字
		/// </summary>
		public string Name { get; set; }
        /// <summary>
        /// 当前成员数量, 由 <see cref="CqApi.GetGroupInfo"/> 方法获取
        /// </summary>
        public int CurrentNumber { get; set; }
        /// <summary>
        /// 获取最大成员数量, 由 <see cref="CqApi.GetGroupInfo"/> 方法获取
        /// </summary>
        public int MaximumNumber { get; set; }
    }
}
