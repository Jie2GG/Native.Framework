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
	/// 表示描述 群文件信息 的类
	/// </summary>
	public class GroupFileInfo
	{
		#region --属性--
		/// <summary>
		/// 获取一个值, 指示当前文件的 Busid (唯一标识符)
		/// </summary>
		public int Id { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前文件的名称
		/// </summary>
		public string FileName { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前文件的Id
		/// </summary>
		public string FileId { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前文件的大小
		/// </summary>
		public long FileSize { get; private set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 使用原始数据初始化 <see cref="GroupFileInfo"/> 类的新实例
		/// </summary>
		/// <exception cref="ArgumentNullException">当参数 cipherBytes 为 null 时引发此异常</exception>
		/// <exception cref="InvalidDataException">原始数据格式错误</exception>
		/// <param name="cipherText">原始数据</param>
		public GroupFileInfo (string cipherText)
		{
			if (cipherText == null)
			{
				throw new ArgumentNullException ("cipherText");
			}
			using (BinaryReader reader = new BinaryReader (new MemoryStream (Convert.FromBase64String (cipherText))))
			{
				if (reader.Length () < 20)
				{
					throw new InvalidDataException ("数据格式错误");
				}
				this.FileId = reader.ReadString_Ex (CQApi.DefaultEncoding);
				this.FileName = reader.ReadString_Ex (CQApi.DefaultEncoding);
				this.FileSize = reader.ReadInt64_Ex ();
				this.Id = (int)reader.ReadInt64_Ex ();
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
			GroupFileInfo info = obj as GroupFileInfo;
			if (info != null)
			{
				return this.Id == info.Id && string.Equals (this.FileId, info.FileId) && string.Equals (this.FileName, info.FileName) && this.FileSize == info.FileSize;
			}
			return base.Equals (obj);
		}

		/// <summary>
		/// 返回该字符串的哈希代码
		/// </summary>
		/// <returns> 32 位有符号整数哈希代码</returns>
		public override int GetHashCode ()
		{
			return base.GetHashCode () & this.Id.GetHashCode () & this.FileId.GetHashCode () & this.FileName.GetHashCode () & this.FileSize.GetHashCode ();
		}

		/// <summary>
		/// 返回表示当前对象的字符串
		/// </summary>
		/// <returns>表示当前对象的字符串</returns>
		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			builder.AppendLine ("文件信息: ");
			builder.AppendLine (string.Format ("    BusId: {0}", this.Id));
			builder.AppendLine (string.Format ("    ID: {0}", this.FileId != null ? this.FileId : string.Empty));
			builder.AppendLine (string.Format ("    名称: {0}", this.FileName != null ? this.FileName : string.Empty));
			builder.AppendFormat ("    大小: {0}", this.FileSize);
			return builder.ToString ();
		}
		#endregion
	}
}
