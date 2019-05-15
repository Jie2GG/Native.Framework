using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Native.Csharp.Tool.SQLite
{
	/// <summary>
	/// 代表 SQLite3 数据源引发异常的基类
	/// </summary>
	public class ArgumentException : DbException
	{
		#region --构造函数--
		/// <summary>
		/// 初始化 <see cref="DbException"/> 类的新实例。
		/// </summary>
		public ArgumentException ()
		{
		}

		/// <summary>
		/// 使用指定的错误消息初始化 <see cref="DbException"/> 类的新实例。
		/// </summary>
		/// <param name="message">要显示此异常的消息。</param>
		public ArgumentException (string message) : base (message)
		{
		}

		/// <summary>
		/// 新实例初始化 <see cref="DbException"/> 使用指定的错误消息和对导致此异常的内部异常的引用。
		/// </summary>
		/// <param name="message">错误消息字符串。</param>
		/// <param name="innerException">内部异常引用。</param>
		public ArgumentException (string message, Exception innerException) : base (message, innerException)
		{
		}

		/// <summary>
		/// 新实例初始化 <see cref="DbException"/> 使用指定的错误消息和错误代码的类。
		/// </summary>
		/// <param name="message">解释异常原因的错误消息。</param>
		/// <param name="errorCode">异常的错误代码。</param>
		public ArgumentException (string message, int errorCode) : base (message, errorCode)
		{
		}

		/// <summary>
		/// 用指定的序列化信息和上下文初始化 <see cref="DbException"/> 类的新实例。
		/// </summary>
		/// <param name="info">包含有关所引发异常的序列化对象数据的 <see cref="SerializationInfo"/>。</param>
		/// <param name="context"><see cref="StreamingContext"/>，它包含关于源或目标的上下文信息。</param>
		public ArgumentException (SerializationInfo info, StreamingContext context) : base (info, context)
		{
		} 
		#endregion
	}
}
