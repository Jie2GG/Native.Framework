using Native.Sdk.Cqp.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Sdk.Cqp.Model
{
	/// <summary>
	/// 描述 QQ请求 的类
	/// </summary>
	public class QQRequest
	{
		#region --属性--
		/// <summary>
		/// 获取当前实例用于获取信息的 <see cref="Native.Sdk.Cqp.CQApi"/> 实例对象
		/// </summary>
		public CQApi CQApi { get; private set; }

		/// <summary>
		/// 获取当前实例的请求反馈标识
		/// </summary>
		public string ResponseFlag { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化 <see cref="QQRequest"/> 类的新实例
		/// </summary>
		/// <param name="api">用于获取信息的实例</param>
		/// <param name="responseFlag">请求反馈标识</param>
		public QQRequest (CQApi api, string responseFlag)
		{
			if (api == null)
			{
				throw new ArgumentNullException ("api");
			}

			if (responseFlag == null)
			{
				throw new ArgumentNullException ("discussId");
			}

			this.CQApi = api;
			this.ResponseFlag = responseFlag;
		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// 置好友添加请求
		/// </summary>
		/// <param name="response">反馈类型</param>
		/// <param name="appendMsg">备注</param>
		/// <returns>操作成功返回 <code>true</code>, 否则返回 <code>false</code></returns>
		public bool SetFriendAddRequest (CQResponseType response, string appendMsg = null)
		{
			return this.CQApi.SetFriendAddRequest (this.ResponseFlag, response, appendMsg);
		}

		/// <summary>
		/// 置群添加请求
		/// </summary>
		/// <param name="request">请求类型</param>
		/// <param name="response">反馈类型</param>
		/// <param name="appendMsg">备注</param>
		/// <returns>操作成功返回 <code>true</code>, 否则返回 <code>false</code></returns>
		public bool SetGroupAddRequest (CQGroupAddRequestType request, CQResponseType response, string appendMsg = null)
		{
			return CQApi.SetGroupAddRequest (this.ResponseFlag, request, response, appendMsg);
		}
		#endregion
	}
}
