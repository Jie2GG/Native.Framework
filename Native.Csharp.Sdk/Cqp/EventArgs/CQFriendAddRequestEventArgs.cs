using Native.Csharp.Sdk.Cqp.Enum;
using Native.Csharp.Sdk.Cqp.Expand;
using Native.Csharp.Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.EventArgs
{
	/// <summary>
	/// 提供用于描述酷Q好友添加请求事件参数的类
	/// <para/>
	/// Type: 301
	/// </summary>
	public class CQFriendAddRequestEventArgs : CQEventEventArgs
	{
		#region --属性--
		/// <summary>
		/// 获取当前事件的子类型
		/// </summary>
		public CQFriendAddRequestType SubType { get; private set; }

		/// <summary>
		/// 获取当前事件的发送时间
		/// </summary>
		public DateTime SendTime { get; private set; }

		/// <summary>
		/// 获取当前事件的来源QQ
		/// </summary>
		public QQ FromQQ { get; private set; }

		/// <summary>
		/// 获取当前事件的附加消息
		/// </summary>
		public string AppendMessage { get; private set; }

		/// <summary>
		/// 获取当前事件用于处理请求所使用的响应标识
		/// </summary>
		public string ResponseFlag { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化 <see cref="CQFriendAddRequestEventArgs"/> 类的新实例
		/// </summary>
		/// <param name="id">事件Id</param>
		/// <param name="type">事件类型</param>
		/// <param name="name">事件名称</param>
		/// <param name="function">函数名称</param>
		/// <param name="priority">默认优先级</param>
		/// <param name="subType">子类型</param>
		/// <param name="sendTime">发送时间</param>
		/// <param name="fromQQ">来源QQ</param>
		/// <param name="msg">附言</param>
		/// <param name="responseFlag">反馈标识</param>
		/// <param name="api">接口Api实例</param>
		public CQFriendAddRequestEventArgs (int id, int type, string name, string function, uint priority, int subType, int sendTime, long fromQQ, string msg, string responseFlag, CQApi api)
			: base (id, type, name, function, priority)
		{
			this.SubType = (CQFriendAddRequestType)subType;
			this.SendTime = sendTime.ToDateTime ();
			this.FromQQ = new QQ (api, fromQQ);
			this.AppendMessage = msg;
			this.ResponseFlag = responseFlag;
		}
		#endregion
	}
}
