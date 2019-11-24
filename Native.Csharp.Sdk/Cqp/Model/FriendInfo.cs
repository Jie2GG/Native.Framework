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
	/// 描述 QQ好友信息 的类
	/// </summary>
	public class FriendInfo
	{
		#region --属性--
		/// <summary>
		/// 获取一个值, 指示当前的QQ帐号
		/// </summary>
		public long Account { get; private set; }

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
		/// <exception cref="ArgumentNullException">参数: cipherBytes 为 null 时引发此异常</exception>
		/// <param name="cipherBytes">原始数据</param>
		public FriendInfo (byte[] cipherBytes)
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

				this.Account = reader.ReadInt32_Ex ();
				this.Nick = reader.ReadString_Ex (CQApi.DefaultEncoding);
				this.Postscript = reader.ReadString_Ex (CQApi.DefaultEncoding);
			}
		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// 获取与当前实例等效的字符串
		/// </summary>
		/// <returns>返回与当前实例等效的字符串</returns>
		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			builder.AppendLine (string.Format ("账号: {0}", this.Account));
			builder.AppendLine (string.Format ("昵称: {0}", this.Nick));
			builder.AppendFormat ("备注: {0}", this.Postscript);
			return builder.ToString ();
		}
		#endregion
	}
}
