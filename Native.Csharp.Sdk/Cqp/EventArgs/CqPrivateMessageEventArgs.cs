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
	/// 提供用于描述酷Q私聊消息事件参数的类
	/// <para/>
	/// Type: 21
	/// </summary>
	public class CQPrivateMessageEventArgs : CQEventEventArgs
	{
		#region --属性--
		/// <summary>
		/// 获取当前事件的消息子类型
		/// </summary>
		public CQPrviateMessageType SubType { get; private set; }

		/// <summary>
		/// 获取当前事件的来源QQ
		/// </summary>
		public QQ FromQQ { get; private set; }

		/// <summary>
		/// 获取当前事件的消息内容
		/// </summary>
		public QQMessage Message { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化 <see cref="CQPrivateMessageEventArgs"/> 类的新实例
		/// </summary>
		/// <param name="id">事件id</param>
		/// <param name="type">事件类型</param>
		/// <param name="name">事件名称</param>
		/// <param name="function">事件函数名</param>
		/// <param name="priority">事件优先级</param>
		/// <param name="subType">消息子类型</param>
		/// <param name="msgId">消息ID</param>
		/// <param name="fromQQ">来源QQ</param>
		/// <param name="msg">消息内容</param>
		/// <param name="api">接口API实例</param>
		public CQPrivateMessageEventArgs (int id, int type, string name, string function, uint priority, int subType, int msgId, long fromQQ, string msg, CQApi api)
			: base (id, type, name, function, priority)
		{
			this.SubType = (CQPrviateMessageType)subType;
			this.FromQQ = new QQ (api, fromQQ);
			this.Message = new QQMessage (api, msgId, msg);
		}
		#endregion
	}
}
