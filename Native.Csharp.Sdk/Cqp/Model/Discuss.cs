using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// 描述 讨论组 的类
	/// </summary>
	public class Discuss
	{
		#region --属性--
		/// <summary>
		/// 获取当前实例用于获取信息的 <see cref="Native.Csharp.Sdk.Cqp.CQApi"/> 实例对象
		/// </summary>
		public CQApi CQApi { get; private set; }

		/// <summary>
		/// 获取当前实例的唯一ID (讨论组号)
		/// </summary>
		public long Id { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化 <see cref="Discuss"/> 类的新实例
		/// </summary>
		/// <param name="api">用于获取信息的实例</param>
		/// <param name="discussId">用于初始化实例的QQ号</param>
		public Discuss (CQApi api, long discussId)
		{
			this.CQApi = api;
			this.Id = discussId;
		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// 发送讨论组消息
		/// </summary>
		/// <param name="message">消息内容, 获取内容时将调用<see cref="object.ToString"/>进行消息转换</param>
		/// <returns>发送成功将返回消息 Id, 发送失败将返回负值</returns>
		public QQMessage SendDiscussMessage (params object[] message)
		{
			return this.CQApi.SendDiscussMessage (this, message);
		}

		/// <summary>
		/// 退出讨论组.
		/// </summary>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool ExitDiscuss ()
		{
			return this.CQApi.ExitDiscuss (this);
		}

		/// <summary>
		/// 返回表示当前对象的字符串
		/// </summary>
		/// <returns>表示当前对象的字符串</returns>
		public override string ToString ()
		{
			return string.Format ("讨论组号: {0}", this.Id);
		}
		#endregion
	}
}
