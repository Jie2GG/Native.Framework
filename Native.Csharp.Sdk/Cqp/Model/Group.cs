using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// 描述 QQ群 的类
	/// </summary>
	public class Group
	{
		#region --属性--
		/// <summary>
		/// 获取当前实例用于获取信息的 <see cref="Native.Csharp.Sdk.Cqp.CQApi"/> 实例对象
		/// </summary>
		public CQApi CQApi { get; private set; }

		/// <summary>
		/// 获取当前实例的唯一ID (QQ群号)
		/// </summary>
		public long Id { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化 <see cref="Group"/> 类的新实例
		/// </summary>
		/// <param name="api">用于获取信息的实例</param>
		/// <param name="groupId">用于初始化实例的QQ群号</param>
		/// /// <exception cref="ArgumentNullException">参数: api 是 null</exception>
		/// <exception cref="ArgumentOutOfRangeException">群号超出范围</exception>
		public Group (CQApi api, long groupId)
		{
			if (api == null)
			{
				throw new ArgumentNullException ("api");
			}

			if (groupId < 10000)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}

			this.CQApi = api;
			this.Id = groupId;
		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// 返回表示当前对象的字符串
		/// </summary>
		/// <returns>表示当前对象的字符串</returns>
		public override string ToString ()
		{
			return string.Format ("QQ群号: {0}", this.Id);
		}
		#endregion
	}
}
