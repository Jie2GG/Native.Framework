using Native.Csharp.Sdk.Cqp.Expand;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// 表示描述 QQ群成员匿名信息 的类
	/// </summary>
	public class GroupMemberAnonymousInfo
	{
		#region --属性--
		/// <summary>
		/// 获取一个值, 指示该成员的群匿名标识
		/// </summary>
		public long Id { get; private set; }

		/// <summary>
		/// 获取一个值, 作为该成员在群中的匿名称号
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// 获取当前匿名成员的执行令牌, 用于对该成员做出一些操作. 如: 禁言
		/// </summary>
		public byte[] Token { get; private set; }

		/// <summary>
		/// 获取当前匿名成员的原始字符串, 这个字符串是构造函数中传入的
		/// </summary>
		public string OriginalString { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 使用原始数据初始化 <see cref="GroupMemberInfo"/> 类的新实例
		/// </summary>
		/// <exception cref="ArgumentNullException">当参数 cipherBytes 为 null 时引发此异常</exception>
		/// <exception cref="InvalidDataException">原始数据格式错误</exception>
		/// <param name="cipherText">原始数据</param>
		public GroupMemberAnonymousInfo (string cipherText)
		{
			if (cipherText == null)
			{
				throw new ArgumentNullException ("cipherText");
			}

			this.OriginalString = cipherText;

			using (BinaryReader reader = new BinaryReader (new MemoryStream (Convert.FromBase64String (cipherText))))
			{
				if (reader.Length () < 12)
				{
					throw new InvalidDataException ("数据格式错误");
				}

				this.Id = reader.ReadInt64_Ex ();
				this.Name = reader.ReadString_Ex (CQApi.DefaultEncoding);
				this.Token = reader.ReadToken_Ex ();
			}
		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// 确定指定的对象是否等于当前对象
		/// </summary>
		/// <param name="obj">要与当前对象进行比较的对象</param>
		/// <returns>如果指定的对象等于当前对象，则为 <code>true</code>，否则为 <code>false</code></returns>
		public override bool Equals (object obj)
		{
			GroupMemberAnonymousInfo info = obj as GroupMemberAnonymousInfo;
			if (info != null)
			{
				return string.Equals (this.OriginalString, info.OriginalString);
			}
			return base.Equals (obj);
		}

		/// <summary>
		/// 返回该字符串的哈希代码
		/// </summary>
		/// <returns> 32 位有符号整数哈希代码</returns>
		public override int GetHashCode ()
		{
			return base.GetHashCode () & this.OriginalString.GetHashCode ();
		}

		/// <summary>
		/// 获取与当前实例等效的字符串
		/// </summary>
		/// <returns>返回与当前实例等效的字符串</returns>
		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			builder.AppendLine (string.Format ("Id: {0}", this.Id));
			builder.AppendFormat ("代号: {0}", this.Name != null ? this.Name : string.Empty);
			return builder.ToString ();
		}
		#endregion
	}
}
