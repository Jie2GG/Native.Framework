using Native.Sdk.Cqp.Expand;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Sdk.Cqp.Model
{
	/// <summary>
	/// 描述 QQ好友信息 的类
	/// </summary>
	public class FriendInfo
	{
		#region --属性--
		/// <summary>
		/// 获取一个值, 指示当前账号的 <see cref="Model.QQ"/> 实例
		/// </summary>
		public QQ QQ { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前的QQ昵称
		/// </summary>
		public string Nick { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前的备注信息
		/// </summary>
		public string Postscript { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 使用原始数据初始化 <see cref="FriendInfo"/> 类的新实例
		/// </summary>
		/// <param name="api">用于获取信息的实例</param>
		/// <param name="cipherBytes">原始数据</param>
		/// <exception cref="ArgumentNullException">参数: cipherBytes 为 null 时引发此异常</exception>
		public FriendInfo (CQApi api, byte[] cipherBytes)
		{
			if (cipherBytes == null)
			{
				throw new ArgumentNullException ("cipherBytes");
			}

			using (BinaryReader reader = new BinaryReader (new MemoryStream (cipherBytes)))
			{
				if (reader.Length () < 12)
				{
					throw new InvalidDataException ("读取失败, 原始数据格式错误");
				}

				this.QQ = new QQ (api, reader.ReadInt64_Ex ());
				this.Nick = reader.ReadString_Ex (CQApi.DefaultEncoding);
				this.Postscript = reader.ReadString_Ex (CQApi.DefaultEncoding);
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
			FriendInfo info = obj as FriendInfo;
			if (info != null)
			{
				return this.QQ == info.QQ && string.Equals (this.Nick, info.Nick) && string.Equals (this.Postscript, info.Postscript);
			}
			return base.Equals (obj);
		}

		/// <summary>
		/// 返回此实例的哈希代码
		/// </summary>
		/// <returns>32 位有符号整数哈希代码</returns>
		public override int GetHashCode ()
		{
			return base.GetHashCode () & this.QQ.GetHashCode () & this.Nick.GetHashCode () & this.Postscript.GetHashCode ();
		}

		/// <summary>
		/// 获取与当前实例等效的字符串
		/// </summary>
		/// <returns>返回与当前实例等效的字符串</returns>
		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			builder.AppendLine (string.Format ("账号: {0}", this.QQ != null ? this.QQ.Id.ToString () : string.Empty));
			builder.AppendLine (string.Format ("昵称: {0}", this.Nick != null ? this.Nick : string.Empty));
			builder.AppendFormat ("备注: {0}", this.Postscript != null ? this.Postscript : string.Empty);
			return builder.ToString ();
		}
		#endregion
	}
}
