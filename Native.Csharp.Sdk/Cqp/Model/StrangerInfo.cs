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
        /// 获取一个值, 指示当前的QQ帐号
        /// </summary>
        public long Account { get; private set; }

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
		/// <exception cref="ArgumentNullException">参数: cipherBytes 为 null 时引发此异常</exception>
		/// <param name="cipherBytes">原始数据</param>
		public StrangerInfo (byte[] cipherBytes)
        {
			if (cipherBytes == null)
			{
				throw new ArgumentNullException ("cipherBytes");
			}

			using (BinaryReader reader = new BinaryReader (new MemoryStream (cipherBytes)))
			{
				this.Account = reader.ReadInt64_Ex ();
				this.Nick = reader.ReadString_Ex ();
				this.Sex = (QQSex)reader.ReadInt32_Ex ();
				this.Age = reader.ReadInt32_Ex ();
			}
		}
        #endregion

        #region --公开方法--
        /// <summary>
        /// 将当前实例的字符串
        /// </summary>
        /// <returns>返回当前实例的字符串</returns>
        public override string ToString ()
        {
            StringBuilder builder = new StringBuilder ();
            builder.AppendLine (string.Format ("帐号: {0}", this.Account));
            builder.AppendLine (string.Format ("昵称: {0}", this.Nick));
            builder.AppendLine (string.Format ("性别: {0}", this.Sex));
            builder.AppendFormat ("年龄: {0}", this.Age);
            return builder.ToString ();
        }
        #endregion
    }
}
