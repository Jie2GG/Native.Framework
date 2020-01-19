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
	/// 表示描述 QQ群成员 的类
	/// </summary>
	public class GroupMemberInfo
	{
		#region --属性--
		/// <summary>
		/// 获取一个值, 指示成员所在群的 <see cref="Model.Group"/> 实例
		/// </summary>
		public Group Group { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前成员的QQ号的 <see cref="Model.QQ"/> 实例
		/// </summary>
		public QQ QQ { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前成员的QQ昵称
		/// </summary>
		public string Nick { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前成员在此群的群名片
		/// </summary>
		public string Card { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前群成员的性别
		/// </summary>
		public QQSex Sex { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前群成员年龄
		/// </summary>
		public int Age { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前成员所在地区
		/// </summary>
		public string Area { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前成员加入群的日期和时间
		/// </summary>
		public DateTime JoinGroupDateTime { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前群成员最后一次发言的日期和时间
		/// </summary>
		public DateTime LastSpeakDateTime { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前群成员的等级
		/// </summary>
		public string Level { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前的群成员类型
		/// </summary>
		public QQGroupMemberType MemberType { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前群成员在此群获得的专属头衔
		/// </summary>
		public string ExclusiveTitle { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前群成员在此群的专属头衔过期时间, 若本属性为 null 则表示无期限
		/// </summary>
		public DateTime? ExclusiveTitleExpirationTime { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前群成员是否为不良记录群成员
		/// </summary>
		public bool IsBadRecord { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前群成员是否允许修改群名片
		/// </summary>
		public bool IsAllowEditorCard { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 使用原始数据初始化 <see cref="GroupMemberInfo"/> 类的新实例
		/// </summary>
		/// <param name="api">用于获取信息的实例</param>
		/// <param name="cipherBytes">原始数据</param>
		/// <exception cref="ArgumentNullException">当参数 cipherBytes 为 null 时引发此异常</exception>
		/// <exception cref="InvalidDataException">原始数据格式错误</exception>
		public GroupMemberInfo (CQApi api, byte[] cipherBytes)
		{
			if (cipherBytes == null)
			{
				throw new ArgumentNullException ("cipherBytes");
			}

			using (BinaryReader reader = new BinaryReader (new MemoryStream (cipherBytes)))
			{
				if (reader.Length () < 40)
				{
					throw new InvalidDataException ("读取失败, 原始数据格式错误");
				}
				this.Group = new Group (api, reader.ReadInt64_Ex ());
				this.QQ = new QQ (api, reader.ReadInt64_Ex ());
				this.Nick = reader.ReadString_Ex (CQApi.DefaultEncoding);
				this.Card = reader.ReadString_Ex (CQApi.DefaultEncoding);
				this.Sex = (QQSex)reader.ReadInt32_Ex ();
				this.Age = reader.ReadInt32_Ex ();
				this.Area = reader.ReadString_Ex (CQApi.DefaultEncoding);
				this.JoinGroupDateTime = reader.ReadInt32_Ex ().ToDateTime ();
				this.LastSpeakDateTime = reader.ReadInt32_Ex ().ToDateTime ();
				this.Level = reader.ReadString_Ex (CQApi.DefaultEncoding);
				this.MemberType = (QQGroupMemberType)reader.ReadInt32_Ex ();
				this.IsBadRecord = reader.ReadInt32_Ex () == 1;
				this.ExclusiveTitle = reader.ReadString_Ex (CQApi.DefaultEncoding);
				this.LastSpeakDateTime = reader.ReadInt32_Ex ().ToDateTime ();
				this.IsAllowEditorCard = reader.ReadInt32_Ex () == 1;
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
			GroupMemberInfo info = obj as GroupMemberInfo;
			if (info != null)
			{
				return this.Group == info.Group && this.QQ == info.QQ && string.Equals (this.Nick, info.Nick) && string.Equals (this.Card, info.Card) && this.JoinGroupDateTime == info.JoinGroupDateTime && this.LastSpeakDateTime == info.LastSpeakDateTime && string.Equals (this.Level, info.Level) && this.MemberType == info.MemberType && string.Equals (this.ExclusiveTitle, info.ExclusiveTitle) && this.ExclusiveTitleExpirationTime == info.ExclusiveTitleExpirationTime && this.IsAllowEditorCard == info.IsAllowEditorCard && this.IsBadRecord == info.IsBadRecord;
			}
			return base.Equals (obj);
		}

		/// <summary>
		/// 返回该字符串的哈希代码
		/// </summary>
		/// <returns> 32 位有符号整数哈希代码</returns>
		public override int GetHashCode ()
		{
			return base.GetHashCode () & this.Group.GetHashCode () & this.QQ.GetHashCode () & this.Nick.GetHashCode () & this.Card.GetHashCode () & this.JoinGroupDateTime.GetHashCode () & this.LastSpeakDateTime.GetHashCode () & this.Level.GetHashCode () & this.MemberType.GetHashCode () & this.ExclusiveTitle.GetHashCode () & this.ExclusiveTitleExpirationTime.GetHashCode () & this.IsAllowEditorCard.GetHashCode () & this.IsBadRecord.GetHashCode ();
		}

		/// <summary>
		/// 将当前实例的字符串
		/// </summary>
		/// <returns>返回当前实例的字符串</returns>
		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			builder.AppendLine (string.Format ("群号: {0}", this.Group != null ? this.Group.Id.ToString () : string.Empty));
			builder.AppendLine (string.Format ("帐号: {0}", this.QQ != null ? this.QQ.Id.ToString () : string.Empty));
			builder.AppendLine (string.Format ("昵称: {0}", this.Nick != null ? this.Nick : string.Empty));
			builder.AppendLine (string.Format ("群名片: {0}", this.Card != null ? this.Card : string.Empty));
			builder.AppendLine (string.Format ("性别: {0}", this.Sex));
			builder.AppendLine (string.Format ("年龄: {0}", this.Age));
			builder.AppendLine (string.Format ("所在地区: {0}", this.Area != null ? this.Area : string.Empty));
			builder.AppendLine (string.Format ("加群时间: {0}", this.JoinGroupDateTime.ToString ("yyyy-MM-dd HH:mm:ss")));
			builder.AppendLine (string.Format ("最后发言时间: {0}", this.LastSpeakDateTime.ToString ("yyyy-MM-dd HH:mm:ss")));
			builder.AppendLine (string.Format ("等级: {0}", this.Level != null ? this.Level : string.Empty));
			builder.AppendLine (string.Format ("成员类型: {0}", this.MemberType.GetDescription ()));
			builder.AppendLine (string.Format ("专属头衔: {0}", this.ExclusiveTitle != null ? this.ExclusiveTitle : string.Empty));
			builder.AppendLine (string.Format ("专属头衔过期时间: {0}", this.ExclusiveTitleExpirationTime.Value.ToString ("yyyy-MM-dd HH:mm:ss")));
			builder.AppendLine (string.Format ("是否为不良记录成员: {0}", this.IsBadRecord ? "是" : "否"));
			builder.AppendFormat ("是否允许修改名片: {0}", this.IsAllowEditorCard ? "是" : "否");
			return builder.ToString ();
		}
		#endregion
	}
}
