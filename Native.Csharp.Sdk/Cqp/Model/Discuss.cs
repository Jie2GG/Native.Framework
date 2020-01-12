using Native.Csharp.Sdk.Cqp.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// 描述 讨论组 的类
	/// </summary>
	public class Discuss : IToSendString
	{
		#region --常量--
		/// <summary>
		/// 表示 <see cref="Discuss"/> 的最小值, 此字段为常数.
		/// </summary>
		public const long MinValue = 10000;
		#endregion

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
			if (discussId < MinValue)
			{
				throw new ArgumentOutOfRangeException ("discussId");
			}
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
		/// 返回用于发送的字符串
		/// </summary>
		/// <returns>用于发送的字符串</returns>
		public string ToSendString ()
		{
			return this.Id.ToString ();
		}

		/// <summary>
		/// 确定指定的对象是否等于当前对象
		/// </summary>
		/// <param name="obj">要与当前对象进行比较的对象</param>
		/// <returns>如果指定的对象等于当前对象，则为 <code>true</code>，否则为 <code>false</code></returns>
		public override bool Equals (object obj)
		{
			Discuss discuss = obj as Discuss;
			if (discuss != null)
			{
				return this.Id == discuss.Id;
			}
			return base.Equals (obj);
		}

		/// <summary>
		/// 返回该字符串的哈希代码
		/// </summary>
		/// <returns> 32 位有符号整数哈希代码</returns>
		public override int GetHashCode ()
		{
			return base.GetHashCode () & this.Id.GetHashCode ();
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

		#region --私有方法--
		/// <summary>
		/// 比较 <see cref="Discuss"/> 中的内容和 string 是否相等
		/// </summary>
		/// <param name="discuss">相比较的 <see cref="Discuss"/> 对象</param>
		/// <param name="discussId">相比较的讨论组号</param>
		/// <returns>如果相同返回 <code>true</code>, 不同返回 <code>false</code></returns>
		private static bool Equals (Discuss discuss, long discussId)
		{
			if (object.ReferenceEquals (discuss, null) || discussId < 10000)
			{
				return false;
			}
			return discuss.Id == discussId;
		}
		#endregion

		#region --运算符方法--
		/// <summary>
		/// 确定<see cref="Discuss"/> 中是否是指定的讨论组号
		/// </summary>
		/// <param name="discuss">要比较的<see cref="Discuss"/>对象，或 null</param>
		/// <param name="discussId">要比较的讨论组号</param>
		/// <returns>如果 discuss 中的值与 discussId 相同, 则为 <code>true</code>, 否则为 <code>false</code></returns>
		[TargetedPatchingOptOut ("性能至关重要的内联跨NGen图像边界")]
		public static bool operator == (Discuss discuss, long discussId)
		{
			return Equals (discuss, discussId);
		}

		/// <summary>
		/// 确定<see cref="Discuss"/> 中是否是指定的讨论组号
		/// </summary>
		/// <param name="discussId">要比较的讨论组号</param>
		/// <param name="discuss">要比较的<see cref="Discuss"/>对象，或 null</param>
		/// <returns>如果 discussId 与 discuss 中的值相同, 则为 <code>true</code>, 否则为 <code>false</code></returns>
		[TargetedPatchingOptOut ("性能至关重要的内联跨NGen图像边界")]
		public static bool operator == (long discussId, Discuss discuss)
		{
			return Equals (discuss, discussId);
		}

		/// <summary>
		/// 确定<see cref="Discuss"/> 中是否不是指定的讨论组号
		/// </summary>
		/// <param name="discuss">要比较的<see cref="Discuss"/>对象，或 null</param>
		/// <param name="discussId">要比较的讨论组号</param>
		/// <returns>如果 discuss 中的值与 discussId 不同, 则为 <code>true</code>, 否则为 <code>false</code></returns>
		[TargetedPatchingOptOut ("性能至关重要的内联跨NGen图像边界")]
		public static bool operator != (Discuss discuss, long discussId)
		{
			return !Equals (discuss, discussId);
		}

		/// <summary>
		/// 确定<see cref="Discuss"/> 中是否不是指定的讨论组号
		/// </summary>
		/// <param name="discussId">要比较的讨论组号</param>
		/// <param name="discuss">要比较的<see cref="Discuss"/>对象，或 null</param>
		/// <returns>如果 discussId 与 discuss 中的值不同, 则为 <code>true</code>, 否则为 <code>false</code></returns>
		[TargetedPatchingOptOut ("性能至关重要的内联跨NGen图像边界")]
		public static bool operator != (long discussId, Discuss discuss)
		{
			return !Equals (discuss, discussId);
		}
		#endregion
	}
}
