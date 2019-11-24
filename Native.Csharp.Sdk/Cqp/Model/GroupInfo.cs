using Native.Csharp.Sdk.Cqp.Expand;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// 表示描述 QQ群信息 的类
	/// </summary>
	public class GroupInfo
	{
		#region --属性--
		/// <summary>
		/// 获取一个值, 指示当前QQ群号码
		/// </summary>
		public long GroupId { get; private set; }

		/// <summary>
		/// 获取当前QQ群的名称
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// 获取一个值, 指示QQ群的当前人数
		/// </summary>
		public int CurrentMemberCount { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前QQ群最大可容纳的人数
		/// </summary>
		public int MaxMemberCount { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 使用指定的原始数据初始化 <see cref="GroupInfo"/> 类的新实例
		/// </summary>
		/// <param name="cipherBytes">原始数据</param>
		/// <param name="oldApi">是否兼容老久API</param>
		/// <exception cref="ArgumentNullException">当参数 cipherBytes 为 null 时引发此异常</exception>
		/// <exception cref="InvalidDataException">原始数据格式错误</exception>
		public GroupInfo (byte[] cipherBytes, bool oldApi = false)
		{
			if (cipherBytes == null)
			{
				throw new ArgumentNullException (nameof (cipherBytes));
			}

			using (BinaryReader reader = new BinaryReader (new MemoryStream (cipherBytes)))
			{
				if (reader.Length () < 10)
				{
					throw new InvalidDataException ("读取失败, 原始数据格式错误");
				}

				this.GroupId = reader.ReadInt64_Ex ();
				this.Name = reader.ReadString_Ex (CQApi.DefaultEncoding);
				if (oldApi == false)
				{
					this.CurrentMemberCount = reader.ReadInt32_Ex ();
					this.MaxMemberCount = reader.ReadInt32_Ex ();
				}
			}
		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// 获取于当前实例等效的字符串副本
		/// </summary>
		/// <returns>返回于当前实例等效的字符串</returns>
		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			builder.AppendLine (string.Format ("群号: {0}", this.GroupId));
			builder.AppendLine (string.Format ("群名称: {0}", this.Name));
			builder.AppendFormat ("当前人数: {0}/{1}", this.CurrentMemberCount, this.MaxMemberCount);
			return builder.ToString ();
		}
		#endregion
	}
}
