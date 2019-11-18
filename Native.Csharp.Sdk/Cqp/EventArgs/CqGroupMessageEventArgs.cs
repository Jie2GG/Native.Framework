using Native.Csharp.Sdk.Cqp.Enum;
using Native.Csharp.Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.EventArgs
{
	/// <summary>
	/// 提供用于描述酷Q群聊消息事件参数的类
	/// <para/>
	/// Type: 2
	/// </summary>
	public class CQGroupMessageEventArgs : CQEventEventArgs
	{
		#region --属性--
		/// <summary>
		/// 获取当前事件的消息子类型
		/// </summary>
		public CQGroupMessageType SubType { get; private set; }

		/// <summary>
		/// 获取当前事件的来源群
		/// </summary>
		public Group FromGroup { get; private set; }

		/// <summary>
		/// 获取当前事件的来源QQ
		/// </summary>
		public QQ FromQQ { get; private set; }

		/// <summary>
		/// 获取当前事件的匿名对象
		/// </summary>
		public GroupMemberAnonymousInfo FromAnonymous { get; private set; }

		/// <summary>
		/// 获取当前事件的消息内容
		/// </summary>
		public QQMessage Message { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化 <see cref="CQGroupMessageEventArgs"/> 类的新实例
		/// </summary>
		/// <param name="id">事件Id</param>
		/// <param name="type">事件类型</param>
		/// <param name="name">事件名称</param>
		/// <param name="function">函数名称</param>
		/// <param name="priority">默认优先级</param>
		/// <param name="subType">子类型</param>
		/// <param name="msgId">消息Id</param>
		/// <param name="fromGroup">来源群号</param>
		/// <param name="fromQQ">来源QQ</param>
		/// <param name="fromAnonymous">来源匿名</param>
		/// <param name="msg">消息内容</param>
		/// <param name="api">接口Api实例</param>
		public CQGroupMessageEventArgs (int id, int type, string name, string function, uint priority, int subType, int msgId, long fromGroup, long fromQQ, string fromAnonymous, string msg, CQApi api)
			: base (id, type, name, function, priority)
		{
			this.SubType = (CQGroupMessageType)subType;
			this.Message = new QQMessage (api, msgId, msg);
			this.FromGroup = new Group (api, fromGroup);
			this.FromQQ = new QQ (api, fromQQ);
			this.FromAnonymous = new GroupMemberAnonymousInfo (fromAnonymous);
		}
		#endregion
	}
}
