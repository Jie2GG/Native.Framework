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
	public class QQRequest : BasisModel, IEquatable<QQRequest>
	{
		#region --属性--
		/// <summary>
		/// 获取当前实例的请求反馈标识
		/// </summary>
		public string ResponseFlag { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 使用指定的 <see cref="CQApi"/> 和响应标识初始化 <see cref="QQRequest"/> 类的新实例
		/// </summary>
		/// <param name="api">用于获取信息的实例</param>
		/// <param name="responseFlag">请求反馈标识</param>
		/// <exception cref="ArgumentNullException">参数: api 为 null</exception>
		public QQRequest (CQApi api, string responseFlag)
			: base (api)
		{
			if (object.ReferenceEquals (responseFlag, null))
			{
				throw new ArgumentNullException ("responseFlag");
			}

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
			return this.CQApi.SetFriendAddRequest (this, response, appendMsg);
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
			return CQApi.SetGroupAddRequest (this, request, response, appendMsg);
		}
		/// <summary>
		/// 指示当前对象是否等于同一类型的另一个对象
		/// </summary>
		/// <param name="other">一个与此对象进行比较的对象</param>
		/// <returns>如果当前对象等于 other 参数，则为 <see langword="true"/>；否则为 <see langword="false"/></returns>
		public bool Equals (QQRequest other)
		{
			if (object.ReferenceEquals (other, null))
			{
				return false;
			}

			return this.ResponseFlag.Equals (other.ResponseFlag);
		}
		/// <summary>
		/// 指示当前对象是否等于同一类型的另一个对象
		/// </summary>
		/// <param name="obj">一个与此对象进行比较的对象</param>
		/// <returns>如果当前对象等于 other 参数，则为 <see langword="true"/>；否则为 <see langword="false"/></returns>
		public override bool Equals (object obj)
		{
			return this.Equals (obj as QQRequest);
		}
		/// <summary>
		/// 返回此实例的哈希代码
		/// </summary>
		/// <returns>32 位有符号整数哈希代码</returns>
		public override int GetHashCode ()
		{
			return this.ResponseFlag.GetHashCode ();
		}
		/// <summary>
		/// 返回表示当前对象的字符串
		/// </summary>
		/// <returns>表示当前对象的字符串</returns>
		public override string ToString ()
		{
			return this;
		}
		/// <summary>
		/// 当在派生类中重写时, 处理返回用于发送的字符串
		/// </summary>
		/// <returns>用于发送的字符串</returns>
		public override string ToSendString ()
		{
			return this;
		}
		#endregion

		#region --转换方法--
		/// <summary>
		/// 定义将 <see cref="QQRequest"/> 对象转换为 <see cref="string"/>
		/// </summary>
		/// <param name="value">转换的 <see cref="QQRequest"/> 对象</param>
		public static implicit operator string (QQRequest value)
		{
			return value.ResponseFlag;
		}
		/// <summary>
		/// 确定两个指定的 <see cref="QQRequest"/> 实例是否具有相同的值
		/// </summary>
		/// <param name="a">要比较的第一个对象</param>
		/// <param name="b">要比较的第二个对象</param>
		/// <returns>如果 a 是与 b 相同的值，或两者均为 <see langword="null"/>，则为 <see langword="true"/>；否则为 <see langword="false"/></returns>
		public static bool operator == (QQRequest a, QQRequest b)
		{
			if (object.ReferenceEquals (a, null) && object.ReferenceEquals (b, null))
			{
				return true;
			}
			return a.Equals (b);
		}
		/// <summary>
		/// 确定两个指定的 <see cref="QQRequest"/> 实例是否具有不同的值
		/// </summary>
		/// <param name="a">要比较的第一个对象</param>
		/// <param name="b">要比较的第二个对象</param>
		/// <returns>如果 a 是与 b 相同的值，或两者均为 <see langword="null"/>，则为 <see langword="false"/>；否则为 <see langword="true"/></returns>
		public static bool operator != (QQRequest a, QQRequest b)
		{
			return !(a == b);
		}
		/// <summary>
		/// 确定指定的 <see cref="QQRequest"/> 和 <see cref="string"/> 实例是否具有相同的值
		/// </summary>
		/// <param name="a">要比较的 <see cref="QQRequest"/> 对象</param>
		/// <param name="b">要比较的 <see cref="string"/> 对象</param>
		/// <returns>如果 a.ResponseFlag 是与 b 相同的值，或两者均为 <see langword="null"/>，则为 <see langword="true"/>；否则为 <see langword="false"/></returns>
		public static bool operator == (QQRequest a, string b)
		{
			if (object.ReferenceEquals (a, null) && object.ReferenceEquals (b, null))
			{
				return true;
			}

			return ((string)a).Equals (b);
		}
		/// <summary>
		/// 确定指定的 <see cref="QQRequest"/> 和 <see cref="string"/> 实例是否具有不同的值
		/// </summary>
		/// <param name="a">要比较的 <see cref="QQRequest"/> 对象</param>
		/// <param name="b">要比较的 <see cref="string"/> 对象</param>
		/// <returns>如果 a.ResponseFlag 是与 b 相同的值，或两者均为 <see langword="null"/>，则为 <see langword="false"/>；否则为 <see langword="true"/></returns>
		public static bool operator != (QQRequest a, string b)
		{
			return !(a == b);
		}
		/// <summary>
		/// 确定指定的 <see cref="QQRequest"/> 和 <see cref="string"/> 实例是否具有相同的值
		/// </summary>
		/// <param name="a">要比较的 <see cref="string"/> 对象</param>
		/// <param name="b">要比较的 <see cref="QQRequest"/> 对象</param>
		/// <returns>如果 a 是与 b.ResponseFlag 相同的值，或两者均为 <see langword="null"/>，则为 <see langword="true"/>；否则为 <see langword="false"/></returns>
		public static bool operator == (string a, QQRequest b)
		{
			return b == a;
		}
		/// <summary>
		/// 确定指定的 <see cref="QQRequest"/> 和 <see cref="string"/> 实例是否具有不同的值
		/// </summary>
		/// <param name="a">要比较的 <see cref="string"/> 对象</param>
		/// <param name="b">要比较的 <see cref="QQRequest"/> 对象</param>
		/// <returns>如果 a 是与 b.ResponseFlag 相同的值，或两者均为 <see langword="null"/>，则为 <see langword="false"/>；否则为 <see langword="true"/></returns>
		public static bool operator != (string a, QQRequest b)
		{
			return b != a;
		}
		#endregion
	}
}
