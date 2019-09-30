using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native.Csharp.Sdk.Cqp.Model;

namespace Native.Csharp.Sdk.Cqp.EventArgs
{
	/// <summary>
	/// 表示群组消息事件参数的类
	/// </summary>
	public class CqGroupMessageEventArgs : CqEventArgsBase
	{
		/// <summary>
		/// 获取一个值, 该值表示当前事件的类型
		/// </summary>
		public override int Type { get { return 2; } }

		/// <summary>
		/// 获取或设置一个值, 表示当前事件所产生消息的唯一编号, 可用于撤回消息
		/// </summary>
		public int MsgId { get; set; }

		/// <summary>
		/// 获取当前消息的来源群组号
		/// </summary>
		public long FromGroup { get; private set; }

		/// <summary>
		/// 获取当前消息的来源QQ号
		/// </summary>
		public long FromQQ { get; private set; }

		/// <summary>
		/// 获取一个值, 该值指示本条消息是否为匿名消息
		/// </summary>
		public bool IsAnonymous { get { return FromAnonymous != null; } }

		/// <summary>
		/// 获取若当前消息为匿名消息时的匿名对象参数
		/// </summary>
		public GroupAnonymous FromAnonymous { get; private set; }

		/// <summary>
		/// 获取当前消息的消息内容
		/// </summary>
		public string Message { get; private set; }

		/// <summary>
		/// 获取或设置一个值, 指示当前是否处理过此事件. 若此值为 True 将停止处理后续事件
		/// </summary>
		public bool Handler { get; set; }

		/// <summary>
		/// 初始化 <see cref="CqGroupMessageEventArgs"/> 类的一个新实例
		/// </summary>
		/// <param name="id">事件ID</param>
		/// <param name="name">事件名称</param>
		/// <param name="msgId">消息ID</param>
		/// <param name="fromGroup">来源群组</param>
		/// <param name="fromQQ">来源QQ</param>
		/// <param name="anonymous">来源匿名</param>
		/// <param name="msg">消息</param>
		public CqGroupMessageEventArgs (int id, string name, int msgId, long fromGroup, long fromQQ, GroupAnonymous anonymous, string msg)
		{
			base.Id = id;
			base.Name = name;
			this.MsgId = msgId;
			this.FromGroup = fromGroup;
			this.FromQQ = fromQQ;
			this.FromAnonymous = anonymous;
			this.Message = msg;
		}
	}
}
