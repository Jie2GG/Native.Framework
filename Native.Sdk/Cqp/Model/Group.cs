using Native.Sdk.Cqp.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Native.Sdk.Cqp.Model
{
	/// <summary>
	/// 描述 QQ群 的类
	/// </summary>
	public class Group : IToSendString
	{
		#region --常量--
		/// <summary>
		/// 表示 <see cref="Group"/> 的最小值, 此字段为常数.
		/// </summary>
		public const long MinValue = 10000;
		#endregion

		#region --属性--
		/// <summary>
		/// 获取当前实例用于获取信息的 <see cref="Native.Sdk.Cqp.CQApi"/> 实例对象
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

			if (groupId < MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}

			this.CQApi = api;
			this.Id = groupId;
		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// 发送群消息
		/// </summary>
		/// <param name="message">消息内容, 获取内容时将调用<see cref="object.ToString"/>进行消息转换</param>
		/// <returns>发送成功将返回 <see cref="QQMessage"/> 对象</returns>
		public QQMessage SendGroupMessage (params object[] message)
		{
			return this.CQApi.SendGroupMessage (this, message);
		}

		/// <summary>
		/// 获取群信息
		/// </summary>
		/// <param name="notCache">不使用缓存, 通常为 <code>false</code>, 仅在必要时使用</param>
		/// <returns>获取成功返回 <see cref="GroupInfo"/> 对象</returns>
		public GroupInfo GetGroupInfo (bool notCache = false)
		{
			return this.CQApi.GetGroupInfo (this, notCache);
		}

		/// <summary>
		/// 获取群成员列表
		/// </summary>
		/// <returns>获取成功返回 <see cref="List{GroupMemberInfo}"/></returns>
		public List<GroupMemberInfo> GetGroupMemberList ()
		{
			return this.CQApi.GetGroupMemberList (this);
		}

		/// <summary>
		/// 获取群成员信息
		/// </summary>
		/// <param name="qq">目标帐号</param>
		/// <param name="notCache">不使用缓存, 默认为 <code>false</code>, 通常忽略本参数, 仅在必要时使用</param>
		/// <returns>获取成功返回 <see cref="GroupMemberInfo"/></returns>
		public GroupMemberInfo GetGroupMemberInfo (QQ qq, bool notCache = false)
		{
			return this.CQApi.GetGroupMemberInfo (this, qq, notCache);
		}

		/// <summary>
		/// 获取群成员信息
		/// </summary>
		/// <param name="qqId">目标帐号</param>
		/// <param name="notCache">不使用缓存, 默认为 <code>false</code>, 通常忽略本参数, 仅在必要时使用</param>
		/// <returns>获取成功返回 <see cref="GroupMemberInfo"/></returns>
		public GroupMemberInfo GetGroupMemberInfo (long qqId, bool notCache = false)
		{
			return this.CQApi.GetGroupMemberInfo (this.Id, qqId, notCache);
		}

		/// <summary>
		/// 设置群匿名成员禁言
		/// </summary>
		/// <param name="anonymous">目标群成员匿名信息</param>
		/// <param name="time">禁言的时长 (范围: 1秒 ~ 30天)</param>
		public bool SetGroupAnonymousMemberBanSpeak (GroupMemberAnonymousInfo anonymous, TimeSpan time)
		{
			return this.CQApi.SetGroupAnonymousMemberBanSpeak (this, anonymous, time);
		}

		/// <summary>
		/// 设置群成员禁言
		/// </summary>
		/// <param name="qqId">目标QQ</param>
		/// <param name="time">禁言时长 (范围: 1秒 ~ 30天)</param>
		/// <returns>禁言成功返回 <code>true</code>, 否则返回 <code>false</code></returns>
		public bool SetGroupMemberBanSpeak (long qqId, TimeSpan time)
		{
			return this.CQApi.SetGroupMemberBanSpeak (this.Id, qqId, time);
		}

		/// <summary>
		/// 设置群成员禁言
		/// </summary>
		/// <param name="qq">目标QQ</param>
		/// <param name="time">禁言时长 (范围: 1秒 ~ 30天)</param>
		/// <returns>禁言成功返回 <code>true</code>, 否则返回 <code>false</code></returns>
		public bool SetGroupMemberBanSpeak (QQ qq, TimeSpan time)
		{
			return this.CQApi.SetGroupMemberBanSpeak (this, qq, time);
		}

		/// <summary>
		/// 解除群成员禁言
		/// </summary>
		/// <param name="qqId">目标QQ</param>
		/// <returns>禁言成功返回 <code>true</code>, 否则返回 <code>false</code></returns>
		public bool RemoveGroupMemberBanSpeak (long qqId)
		{
			return this.CQApi.RemoveGroupMemberBanSpeak (this.Id, qqId);
		}

		/// <summary>
		/// 解除群成员禁言
		/// </summary>
		/// <param name="qq">目标QQ</param>
		/// <returns>禁言成功返回 <code>true</code>, 否则返回 <code>false</code></returns>
		public bool RemoveGroupMemberBanSpeak (QQ qq)
		{
			return this.CQApi.RemoveGroupMemberBanSpeak (this, qq);
		}

		/// <summary>
		/// 设置群全体禁言
		/// </summary>
		/// <returns>操作成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupBanSpeak ()
		{
			return this.CQApi.SetGroupBanSpeak (this);
		}

		/// <summary>
		/// 解除群全体禁言
		/// </summary>
		/// <returns>操作成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveGroupBanSpeak ()
		{
			return this.CQApi.RemoveGroupBanSpeak (this);
		}

		/// <summary>
		/// 设置群成员名片
		/// </summary>
		/// <param name="qqId">目标QQ</param>
		/// <param name="newName">新名称</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupMemberVisitingCard (long qqId, string newName)
		{
			return this.CQApi.SetGroupMemberVisitingCard (this.Id, qqId, newName);
		}

		/// <summary>
		/// 设置群成员名片
		/// </summary>
		/// <param name="qq">目标QQ</param>
		/// <param name="newName">新名称</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupMemberVisitingCard (QQ qq, string newName)
		{
			return this.CQApi.SetGroupMemberVisitingCard (this, qq, newName);
		}

		/// <summary>
		/// 设置群成员专属头衔, 并指定其过期的时间
		/// </summary>
		/// <param name="qqId">目标QQ</param>
		/// <param name="newTitle">新头衔</param>
		/// <param name="time">过期时间</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupMemberExclusiveTitle (long qqId, string newTitle, TimeSpan time)
		{
			return this.CQApi.SetGroupMemberExclusiveTitle (this.Id, qqId, newTitle, time);
		}

		/// <summary>
		/// 设置群成员专属头衔, 并指定其过期的时间
		/// </summary>
		/// <param name="qq">目标QQ</param>
		/// <param name="newTitle">新头衔</param>
		/// <param name="time">过期时间 (范围: 1秒 ~ 30天)</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupMemberExclusiveTitle (QQ qq, string newTitle, TimeSpan time)
		{
			return this.CQApi.SetGroupMemberExclusiveTitle (this, qq, newTitle, time);
		}

		/// <summary>
		/// 设置群成员永久专属头衔
		/// </summary>
		/// <param name="qqId">目标QQ</param>
		/// <param name="newTitle">新头衔</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupMemberForeverExclusiveTitle (long qqId, string newTitle)
		{
			return this.CQApi.SetGroupMemberForeverExclusiveTitle (this.Id, qqId, newTitle);
		}

		/// <summary>
		/// 设置群成员永久专属头衔
		/// </summary>
		/// <param name="qq">目标QQ</param>
		/// <param name="newTitle">新头衔</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupMemberForeverExclusiveTitle (QQ qq, string newTitle)
		{
			return this.CQApi.SetGroupMemberForeverExclusiveTitle (this, qq, newTitle);
		}

		/// <summary>
		/// 设置群管理员
		/// </summary>
		/// <param name="qqId">目标QQ</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupManage (long qqId)
		{
			return this.CQApi.SetGroupManage (this.Id, qqId);
		}

		/// <summary>
		/// 设置群管理员
		/// </summary>
		/// <param name="qq">目标QQ</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupManage (QQ qq)
		{
			return this.CQApi.SetGroupManage (this, qq);
		}

		/// <summary>
		/// 解除群管理员
		/// </summary>
		/// <param name="qqId">目标QQ</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveGroupManage (long qqId)
		{
			return this.CQApi.RemoveGroupManage (this.Id, qqId);
		}

		/// <summary>
		/// 解除群管理员
		/// </summary>
		/// <param name="qq">目标QQ</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveGroupManage (QQ qq)
		{
			return this.CQApi.RemoveGroupManage (this, qq);
		}

		/// <summary>
		/// 移除群成员
		/// </summary>
		/// <param name="qqId">目标QQ</param>
		/// <param name="notRequest">不再接收加群申请. 请慎用, 默认: False</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveGroupMember (long qqId, bool notRequest = false)
		{
			return this.CQApi.RemoveGroupMember (this.Id, qqId, notRequest);
		}

		/// <summary>
		/// 移除群成员
		/// </summary>
		/// <param name="qq">目标QQ</param>
		/// <param name="notRequest">不再接收加群申请. 请慎用, 默认: False</param>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveGroupMember (QQ qq, bool notRequest = false)
		{
			return this.CQApi.RemoveGroupMember (this, qq, notRequest);
		}

		/// <summary>
		/// 开启群匿名
		/// </summary>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool OpenGroupAnonymous ()
		{
			return this.CQApi.OpenGroupAnonymous (this);
		}

		/// <summary>
		/// 关闭群匿名
		/// </summary>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool StopGroupAnonymous ()
		{
			return this.CQApi.StopGroupAnonymous (this);
		}

		/// <summary>
		/// 退出群. 慎用, 此接口需要严格授权
		/// </summary>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool ExitGroup ()
		{
			return this.CQApi.ExitGroup (this);
		}

		/// <summary>
		/// 解散群. 慎用, 此接口需要严格授权
		/// </summary>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool DissolutionGroup ()
		{
			return this.CQApi.DissolutionGroup (this);
		}

		/// <summary>
		/// 确定指定的对象是否等于当前对象
		/// </summary>
		/// <param name="obj">要与当前对象进行比较的对象</param>
		/// <returns>如果指定的对象等于当前对象，则为 <code>true</code>，否则为 <code>false</code></returns>
		public override bool Equals (object obj)
		{
			if (obj is Group)
			{
				return this.Id == ((Group)obj).Id;
			}
			else if (obj is long)
			{
				return this.Id == (long)obj;
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
			return string.Format ("群号: {0}", this.Id);
		}

		/// <summary>
		/// 返回用于发送的字符串
		/// </summary>
		/// <returns>用于发送的字符串</returns>
		public string ToSendString ()
		{
			return this.Id.ToString ();
		}
		#endregion

		#region --私有方法--
		/// <summary>
		/// 比较 <see cref="Group"/> 中的内容和 string 是否相等
		/// </summary>
		/// <param name="group">相比较的 <see cref="Group"/> 对象</param>
		/// <param name="groupId">相比较的群号</param>
		/// <returns>如果相同返回 <code>true</code>, 不同返回 <code>false</code></returns>
		public static bool Equals (Group group, long groupId)
		{
			if (object.ReferenceEquals (group, null) || groupId < 10000)
			{
				return false;
			}
			return group.Id == groupId;
		}
		#endregion

		#region --运算符方法--
		/// <summary>
		/// 确定<see cref="Group"/> 中是否是指定的讨论组号
		/// </summary>
		/// <param name="group">要比较的<see cref="Group"/>对象，或 null</param>
		/// <param name="groupId">要比较的讨论组号</param>
		/// <returns>如果 group 中的值与 groupId 相同, 则为 <code>true</code>, 否则为 <code>false</code></returns>
		[TargetedPatchingOptOut ("性能至关重要的内联跨NGen图像边界")]
		public static bool operator == (Group group, long groupId)
		{
			return Equals (group, groupId);
		}

		/// <summary>
		/// 确定<see cref="Group"/> 中是否是指定的讨论组号
		/// </summary>
		/// <param name="groupId">要比较的讨论组号</param>
		/// <param name="group">要比较的<see cref="Group"/>对象，或 null</param>
		/// <returns>如果 groupId 与 group 中的值相同, 则为 <code>true</code>, 否则为 <code>false</code></returns>
		[TargetedPatchingOptOut ("性能至关重要的内联跨NGen图像边界")]
		public static bool operator == (long groupId, Group group)
		{
			return Equals (group, groupId);
		}

		/// <summary>
		/// 确定<see cref="Group"/> 中是否不是指定的讨论组号
		/// </summary>
		/// <param name="group">要比较的<see cref="Group"/>对象，或 null</param>
		/// <param name="groupId">要比较的讨论组号</param>
		/// <returns>如果 group 中的值与 groupId 不同, 则为 <code>true</code>, 否则为 <code>false</code></returns>
		[TargetedPatchingOptOut ("性能至关重要的内联跨NGen图像边界")]
		public static bool operator != (Group group, long groupId)
		{
			return !Equals (group, groupId);
		}

		/// <summary>
		/// 确定<see cref="Group"/> 中是否不是指定的讨论组号
		/// </summary>
		/// <param name="groupId">要比较的讨论组号</param>
		/// <param name="group">要比较的<see cref="Group"/>对象，或 null</param>
		/// <returns>如果 groupId 与 group 中的值不同, 则为 <code>true</code>, 否则为 <code>false</code></returns>
		[TargetedPatchingOptOut ("性能至关重要的内联跨NGen图像边界")]
		public static bool operator != (long groupId, Group group)
		{
			return !Equals (group, groupId);
		}
		#endregion
	}
}
