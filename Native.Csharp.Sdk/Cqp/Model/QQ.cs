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

		/// <summary>
		/// 判断是否是登录QQ (机器人QQ)
		/// </summary>
		public bool IsLoginQQ { get { return this.CQApi.GetLoginQQLong () == this.Id; } }
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
		/// 发送私聊消息
		/// </summary>
		/// <param name="message">消息内容, 获取内容时将调用<see cref="object.ToString"/>进行消息转换</param>
		/// <returns>发送成功将返回 <see cref="QQMessage"/> 对象</returns>
		public QQMessage SendPrivateMessage (params object[] message)
		{
			return this.CQApi.SendPrivateMessage (this, message);
		}

		/// <summary>
		/// 发送赞
		/// </summary>
		/// <param name="count">发送赞的次数, 范围: 1~10 (留空为1次)</param>
		/// <returns>执行成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SendPraise (int count = 1)
		{
			return this.CQApi.SendPraise (this, count);
		}

		/// <summary>
		/// 获取陌生人信息
		/// </summary>
		/// <param name="notCache">不使用缓存, 默认为 <code>false</code>, 通常忽略本参数, 仅在必要时使用</param>
		/// <returns>获取成功返回 <see cref="StrangerInfo"/></returns>
		public StrangerInfo GetStrangerInfo (bool notCache = false)
		{
			return this.CQApi.GetStrangerInfo (this, notCache);
		}

		/// <summary>
		/// 获取群成员信息
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="notCache">不使用缓存, 默认为 <code>false</code>, 通常忽略本参数, 仅在必要时使用</param>
		/// <returns>获取成功返回 <see cref="GroupMemberInfo"/></returns>
		public GroupMemberInfo GetGroupMemberInfo (long groupId, bool notCache = false)
		{
			return this.CQApi.GetGroupMemberInfo (groupId, this.Id, notCache);
		}

		/// <summary>
		/// 获取群成员信息
		/// </summary>
		/// <param name="group">目标群</param>
		/// <param name="notCache">不使用缓存, 默认为 <code>false</code>, 通常忽略本参数, 仅在必要时使用</param>
		/// <returns>获取成功返回 <see cref="GroupMemberInfo"/></returns>
		public GroupMemberInfo GetGroupMemberInfo (Group group, bool notCache = false)
		{
			return this.CQApi.GetGroupMemberInfo (group, this, notCache);
		}

		/// <summary>
		/// 设置群成员禁言
		/// </summary>
		/// <param name="groupId">目标群号</param>
		/// <param name="time">禁言时长 (范围: 1秒 ~ 30天)</param>
		/// <returns>禁言成功返回 <code>true</code>, 否则返回 <code>false</code></returns>
		public bool SetGroupMemberBanSpeak (long groupId, TimeSpan time)
		{
			return this.CQApi.SetGroupMemberBanSpeak (groupId, this.Id, time);
		}

		/// <summary>
		/// 设置群成员禁言
		/// </summary>
		/// <param name="group">目标群号</param>
		/// <param name="time">禁言时长 (范围: 1秒 ~ 30天)</param>
		/// <returns>禁言成功返回 <code>true</code>, 否则返回 <code>false</code></returns>
		public bool SetGroupMemberBanSpeak (Group group, TimeSpan time)
		{
			return this.CQApi.SetGroupMemberBanSpeak (group, this, time);
		}

		/// <summary>
		/// 解除群成员禁言
		/// </summary>
		/// <param name="groupId">目标群号</param>
		/// <returns>禁言成功返回 <code>true</code>, 否则返回 <code>false</code></returns>
		public bool RemoveGroupMemberBanSpeak (long groupId)
		{
			return this.CQApi.RemoveGroupMemberBanSpeak (groupId, this.Id);
		}

		/// <summary>
		/// 解除群成员禁言
		/// </summary>
		/// <param name="group">目标群号</param>
		/// <returns>禁言成功返回 <code>true</code>, 否则返回 <code>false</code></returns>
		public bool RemoveGroupMemberBanSpeak (Group group)
		{
			return this.CQApi.RemoveGroupMemberBanSpeak (group, this);
		}

		/// <summary>
		/// 设置群成员名片
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="newName">新名称</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupMemberVisitingCard (long groupId, string newName)
		{
			return this.CQApi.SetGroupMemberVisitingCard (groupId, this.Id, newName);
		}

		/// <summary>
		/// 设置群成员名片
		/// </summary>
		/// <param name="group">目标群</param>
		/// <param name="newName">新名称</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupMemberVisitingCard (Group group, string newName)
		{
			return this.CQApi.SetGroupMemberVisitingCard (group, this, newName);
		}

		/// <summary>
		/// 设置群成员专属头衔, 并指定其过期的时间
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="newTitle">新头衔</param>
		/// <param name="time">过期时间</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupMemberExclusiveTitle (long groupId, string newTitle, TimeSpan time)
		{
			return this.SetGroupMemberExclusiveTitle (groupId, newTitle, time);
		}

		/// <summary>
		/// 设置群成员专属头衔, 并指定其过期的时间
		/// </summary>
		/// <param name="group">目标群</param>
		/// <param name="newTitle">新头衔</param>
		/// <param name="time">过期时间 (范围: 1秒 ~ 30天)</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupMemberExclusiveTitle (Group group, string newTitle, TimeSpan time)
		{
			return this.CQApi.SetGroupMemberExclusiveTitle (group, this, newTitle, time);
		}

		/// <summary>
		/// 设置群成员永久专属头衔
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="newTitle">新头衔</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupMemberForeverExclusiveTitle (long groupId, string newTitle)
		{
			return this.CQApi.SetGroupMemberForeverExclusiveTitle (groupId, this.Id, newTitle);
		}

		/// <summary>
		/// 设置群成员永久专属头衔
		/// </summary>
		/// <param name="group">目标群</param>
		/// <param name="newTitle">新头衔</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupMemberForeverExclusiveTitle (Group group, string newTitle)
		{
			return this.CQApi.SetGroupMemberForeverExclusiveTitle (group, this, newTitle);
		}

		/// <summary>
		/// 设置群管理员
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupManage (long groupId)
		{
			return this.CQApi.SetGroupManage (groupId, this.Id);
		}

		/// <summary>
		/// 设置群管理员
		/// </summary>
		/// <param name="group">目标群</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupManage (Group group)
		{
			return this.CQApi.SetGroupManage (group, this);
		}

		/// <summary>
		/// 解除群管理员
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveGroupManage (long groupId)
		{
			return this.CQApi.RemoveGroupManage (groupId, this.Id);
		}

		/// <summary>
		/// 解除群管理员
		/// </summary>
		/// <param name="group">目标群</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveGroupManage (Group group)
		{
			return this.CQApi.RemoveGroupManage (group, this);
		}

		/// <summary>
		/// 移除群成员
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="notRequest">不再接收加群申请. 请慎用, 默认: False</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveGroupMember (long groupId, bool notRequest = false)
		{
			return this.CQApi.RemoveGroupMember (groupId, this.Id, notRequest);
		}

		/// <summary>
		/// 移除群成员
		/// </summary>
		/// <param name="group">目标群</param>
		/// <param name="notRequest">不再接收加群申请. 请慎用, 默认: False</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveGroupMember (Group group, bool notRequest = false)
		{
			return this.CQApi.RemoveGroupMember (group, this, notRequest);
		}

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
