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
	/// 提供用于描述酷Q讨论组事件参数的类
	/// <para/>
	/// Type: 4 
	/// </summary>
	public class CQDiscussMessageEventArgs : CQEventEventArgs
	{
		#region --属性--
		/// <summary>
		/// 获取当前事件的消息子类型
		/// </summary>
		public CQDiscussMessageType SubType { get; private set; }

		/// <summary>
		/// 获取当前事件的来源讨论组
		/// </summary>
		public Discuss FromDiscuss { get; private set; }

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
		/// 初始化 <see cref="CQDiscussMessageEventArgs"/> 类的新实例
		/// </summary>
		/// <param name="id">事件Id</param>
		/// <param name="type">事件类型</param>
		/// <param name="name">事件名称</param>
		/// <param name="function">函数名称</param>
		/// <param name="priority">默认优先级</param>
		/// <param name="subType">子类型</param>
		/// <param name="msgId">消息Id</param>
		/// <param name="fromDiscuss">来源讨论组</param>
		/// <param name="fromQQ">来源QQ</param>
		/// <param name="msg">消息内容</param>
		/// <param name="isRegex">是否为正则消息</param>
		/// <param name="api">接口Api实例</param>
		public CQDiscussMessageEventArgs (int id, int type, string name, string function, uint priority, int subType, int msgId, long fromDiscuss, long fromQQ, string msg, bool isRegex, CQApi api)
			: base (id, type, name, function, priority)
		{
			this.SubType = (CQDiscussMessageType)subType;
			this.Message = new QQMessage (api, msgId, msg, isRegex);
			this.FromDiscuss = new Discuss (api, fromDiscuss);
			this.FromQQ = new QQ (api, fromQQ);
		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// 返回表示当前对象的字符串
		/// </summary>
		/// <returns>表示当前对象的字符串</returns>
		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			builder.AppendLine (string.Format ("ID: {0}", this.Id));
			builder.AppendLine (string.Format ("类型: {0}({1})", this.Type, (int)this.Type));
			builder.AppendLine (string.Format ("名称: {0}", this.Name));
			builder.AppendLine (string.Format ("函数: {0}", this.Function));
			builder.AppendLine (string.Format ("优先级: {0}", this.Priority));
			builder.AppendLine (string.Format ("子类型: {0}({1})", this.SubType, (int)this.SubType));
			builder.AppendLine (this.FromDiscuss.ToString ());
			builder.AppendLine (this.FromQQ.ToString ());
			builder.Append (this.Message.ToString ());
			return builder.ToString ();
		}
		#endregion
	}
}
