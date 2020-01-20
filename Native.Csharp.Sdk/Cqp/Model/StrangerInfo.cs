using Native.Csharp.Sdk.Cqp.Enum;
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
	/// 描述 QQ陌生人信息 的类
	/// </summary>
	public class StrangerInfo
	{
		#region --属性--
		/// <summary>
		/// 获取一个值, 指示当前的账号的 <see cref="Model.QQ"/> 实例
		/// </summary>
		public QQ QQ { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前的QQ昵称
		/// </summary>
		public string Nick { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前QQ的性别
		/// </summary>
		public QQSex Sex { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前的年龄
		/// </summary>
		public int Age { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 使用原始数据初始化 <see cref="StrangerInfo"/> 类的新实例
		/// </summary>
		/// <param name="api">用于获取信息的实例</param>
		/// <param name="cipherBytes">原始数据</param>
		/// <exception cref="ArgumentNullException">参数: cipherBytes 为 null 时引发此异常</exception>
		public StrangerInfo (CQApi api, byte[] cipherBytes)
		{
			if (cipherBytes == null)
			{
				throw new ArgumentNullException ("cipherBytes");
			}

			using (BinaryReader reader = new BinaryReader (new MemoryStream (cipherBytes)))
			{
				this.QQ = new QQ (api, reader.ReadInt64_Ex ());
				this.Nick = reader.ReadString_Ex (CQApi.DefaultEncoding);
				this.Sex = (QQSex)reader.ReadInt32_Ex ();
				this.Age = reader.ReadInt32_Ex ();
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
			StrangerInfo info = obj as StrangerInfo;
			if (info != null)
			{
				return this.QQ == info.QQ && string.Equals (this.Nick, info.Nick) && this.Age == info.Age && this.Sex == info.Sex;
			}
			return base.Equals (obj);
		}

		/// <summary>
		/// 返回该字符串的哈希代码
		/// </summary>
		/// <returns> 32 位有符号整数哈希代码</returns>
		public override int GetHashCode ()
		{
			return base.GetHashCode () & this.QQ.GetHashCode () & this.Nick.GetHashCode () & this.Sex.GetHashCode () & this.Age.GetHashCode ();
		}

		/// <summary>
		/// 将当前实例的字符串
		/// </summary>
		/// <returns>返回当前实例的字符串</returns>
		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			builder.AppendLine (string.Format ("帐号: {0}", this.QQ != null ? this.QQ.Id.ToString () : string.Empty));
			builder.AppendLine (string.Format ("昵称: {0}", this.Nick != null ? this.Nick : string.Empty));
			builder.AppendLine (string.Format ("性别: {0}", this.Sex));
			builder.AppendFormat ("年龄: {0}", this.Age);
			return builder.ToString ();
		}
		#endregion
	}
}
