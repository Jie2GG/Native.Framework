using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// 描述 QQ 的类
	/// </summary>
	public class QQ
	{
		#region --属性--
		/// <summary>
		/// 获取当前实例用于获取信息的 <see cref="Native.Csharp.Sdk.Cqp.CQApi"/> 实例对象
		/// </summary>
		public CQApi CQApi { get; private set; }

		/// <summary>
		/// 获取当前实例的唯一ID (QQ号)
		/// </summary>
		public long Id { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化 <see cref="QQ"/> 类的新实例
		/// </summary>
		/// <param name="api">用于获取信息的实例</param>
		/// <param name="qqId">用于初始化实例的QQ号</param>
		/// <exception cref="ArgumentNullException">参数: api 是 null</exception>
		/// <exception cref="ArgumentOutOfRangeException">QQ号超出范围</exception>
		public QQ (CQApi api, long qqId)
		{
			if (api == null)
			{
				throw new ArgumentNullException ("api");
			}

			if (qqId < 10000)
			{
				throw new ArgumentOutOfRangeException ("qqId");
			}

			this.CQApi = api;
			this.Id = qqId;
		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// 返回表示当前对象的字符串
		/// </summary>
		/// <returns>表示当前对象的字符串</returns>
		public override string ToString ()
		{
			return string.Format ("QQ号: {0}", this.Id);
		} 
		#endregion
	}
}
