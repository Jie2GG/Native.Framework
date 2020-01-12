using Native.Csharp.Sdk.Cqp.Enum;
using Native.Csharp.Sdk.Cqp.Expand;
using Native.Csharp.Sdk.Cqp.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.Sdk.Cqp.Model
{
	/// <summary>
	/// 描述 QQ消息 的类
	/// </summary>
	public class QQMessage : IToSendString
	{
		#region --字段--
		private List<CQCode> _cqCodes = null;
		#endregion

		#region --属性--
		/// <summary>
		/// 获取当前实例用于获取信息的 <see cref="Native.Csharp.Sdk.Cqp.CQApi"/> 实例对象
		/// </summary>
		public CQApi CQApi { get; private set; }

		/// <summary>
		/// 获取当前实例的消息ID
		/// </summary>
		public int Id { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前消息是否发送成功.
		/// </summary>
		public bool IsSuccess { get { return this.Id >= 0; } }

		/// <summary>
		/// 获取当前实例的原始消息
		/// </summary>
		public string Text { get; private set; }

		/// <summary>
		/// 获取一个值, 指示当前消息是否为正则消息
		/// </summary>
		public bool IsRegexMessage { get; private set; }

		/// <summary>
		/// 获取当前实例解析出的正则消息键值对
		/// </summary>
		public Dictionary<string, string> RegexKeyValuePairs { get; private set; }

		/// <summary>
		/// 获取当前消息的所有 [CQ:...] 的对象集合
		/// </summary>
		public List<CQCode> CQCodes
		{
			get
			{
				if (this.IsRegexMessage)
				{
					return null;
				}

				if (this._cqCodes == null)
				{
					_cqCodes = CQCode.Parse (this.Text);
				}

				return this._cqCodes;
			}
		}
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化 <see cref="QQMessage"/> 类的新实例
		/// </summary>
		/// <param name="api">用于获取信息的实例</param>
		/// <param name="id">消息ID</param>
		/// <param name="msg">消息内容</param>
		public QQMessage (CQApi api, int id, string msg)
			: this (api, id, msg, false)
		{

		}

		/// <summary>
		/// 初始化 <see cref="QQMessage"/> 类的新实例
		/// </summary>
		/// <param name="api">用于获取信息的实例</param>
		/// <param name="id">消息ID</param>
		/// <param name="msg">消息内容</param>
		/// <param name="isRegex">是否正则</param>
		public QQMessage (CQApi api, int id, string msg, bool isRegex)
		{
			if (api == null)
			{
				throw new ArgumentNullException ("api");
			}

			if (msg == null)
			{
				throw new ArgumentNullException ("msg");
			}

			this.CQApi = api;
			this.Id = id;
			this.Text = msg;
			this.IsRegexMessage = isRegex;
			this.RegexKeyValuePairs = null;

			#region --正则事件解析--
			if (isRegex)
			{
				// 进行正则事件消息解析
				using (BinaryReader reader = new BinaryReader (new MemoryStream (Convert.FromBase64String (msg))))
				{
					if (reader.Length () < 4)
					{
						throw new InvalidDataException ("读取失败, 原始数据格式错误");
					}

					this.RegexKeyValuePairs = new Dictionary<string, string> (reader.ReadInt32_Ex ());

					for (int i = 0; i < RegexKeyValuePairs.Count; i++)
					{
						using (BinaryReader tempReader = new BinaryReader (new MemoryStream (reader.ReadToken_Ex ())))
						{
							if (tempReader.Length () < 4)
							{
								throw new InvalidDataException ("读取失败, 原始数据格式错误");
							}

							// 读取结果
							string key = tempReader.ReadString_Ex ();
							string content = tempReader.ReadString_Ex ();

							this.RegexKeyValuePairs.Add (key, content);
						}
					}
				}
			}
			#endregion
		}
		#endregion

		#region --公开方法--
		/// <summary>
		/// 撤回消息
		/// </summary>
		/// <returns>撤回成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveMessage ()
		{
			return this.CQApi.RemoveMessage (this.Id);
		}

		/// <summary>
		/// 接收消息中的语音 (消息含有CQ码 "record" 的消息)
		/// </summary>
		/// <param name="format">所需的目标语音的音频格式</param>
		/// <returns>返回语音文件位于本地服务器的绝对路径</returns>
		public string ReceiveRecord (CQAudioFormat format)
		{
			if (this.IsRegexMessage)
			{
				return null;
			}

			CQCode record = (from code in this.CQCodes where code.Function == CQFunction.Record select code).First ();
			return this.CQApi.ReceiveRecord (record.Items["file"], format);
		}

		/// <summary>
		/// 接收消息中的图片 (消息含有CQ码 "image" 的消息)
		/// </summary>
		/// <param name="index">要接收的图片索引, 该索引从 0 开始</param>
		/// <returns>返回图片文件位于本地服务器的绝对路径</returns>
		public string ReceiveImage (int index)
		{
			if (this.IsRegexMessage)
			{
				return null;
			}

			CQCode image = (from code in this.CQCodes where code.Function == CQFunction.Image select code).Skip (index).First ();
			return this.CQApi.ReceiveImage (image.Items["file"]);
		}

		/// <summary>
		/// 返回用于发送的字符串
		/// </summary>
		/// <returns>用于发送的字符串</returns>
		public string ToSendString ()
		{
			if (!this.IsRegexMessage)
			{
				return this.Text;
			}
			return string.Empty;
		}

		/// <summary>
		/// 确定指定的对象是否等于当前对象
		/// </summary>
		/// <param name="obj">要与当前对象进行比较的对象</param>
		/// <returns>如果指定的对象等于当前对象，则为 <code>true</code>，否则为 <code>false</code></returns>
		public override bool Equals (object obj)
		{
			QQMessage msg = obj as QQMessage;
			if (msg != null)
			{
				return string.Compare (this.Text, msg.Text) == 0;
			}
			return base.Equals (obj);
		}

		/// <summary>
		/// 返回该字符串的哈希代码
		/// </summary>
		/// <returns> 32 位有符号整数哈希代码</returns>
		public override int GetHashCode ()
		{
			return this.Text.GetHashCode () & base.GetHashCode ();
		}

		/// <summary>
		/// 返回表示当前对象的字符串
		/// </summary>
		/// <returns>表示当前对象的字符串</returns>
		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			builder.AppendLine (string.Format ("ID: {0}", this.Id));
			builder.AppendLine (string.Format ("正则消息: {0}", this.IsRegexMessage));
			if (!this.IsRegexMessage)
			{
				builder.AppendFormat ("消息内容: {0}", this.Text);
			}
			else
			{
				builder.Append ("解析结果: ");
				foreach (KeyValuePair<string, string> keyValue in this.RegexKeyValuePairs)
				{
					builder.AppendFormat ("{0}-{1}, ", keyValue.Key, keyValue.Value);
				}
				builder.Remove (builder.Length - 2, 2); // 删除最后的符号和空格
			}
			return builder.ToString ();
		}
		#endregion

		#region --私有方法--
		/// <summary>
		/// 比较 <see cref="QQMessage"/> 中的内容和 string 是否相等
		/// </summary>
		/// <param name="msg">相比较的 <see cref="QQMessage"/> 对象</param>
		/// <param name="str">相比较的字符串</param>
		/// <returns>如果相同返回 <code>true</code>, 不同返回 <code>false</code></returns>
		private static bool Equals (QQMessage msg, string str)
		{
			if (object.ReferenceEquals (msg, null) || object.ReferenceEquals (str, null))
			{
				return false;
			}
			return string.Compare (msg.Text, str) == 0;
		}
		#endregion

		#region --运算符方法--
		/// <summary>
		/// 确定<see cref="QQMessage"/> 和字符串是否具有相同的值
		/// </summary>
		/// <param name="msg">要比较的第一个<see cref="QQMessage"/>对象，或 null</param>
		/// <param name="str">要比较的第二个字符串，或 null</param>
		/// <returns>如果 msg 中的值与 str 相同, 则为 <code>true</code>, 否则为 <code>false</code></returns>
		[TargetedPatchingOptOut ("性能至关重要的内联跨NGen图像边界")]
		public static bool operator == (QQMessage msg, string str)
		{
			return Equals (msg, str);
		}

		/// <summary>
		/// 确定<see cref="QQMessage"/> 和字符串是否具有相同的值
		/// </summary>
		/// <param name="str">要比较的第一个字符串，或 null</param>
		/// <param name="msg">要比较的第二个<see cref="QQMessage"/>对象，或 null</param>
		/// <returns>如果 str与 msg 中的值相同, 则为 <code>true</code>, 否则为 <code>false</code></returns>
		[TargetedPatchingOptOut ("性能至关重要的内联跨NGen图像边界")]
		public static bool operator == (string str, QQMessage msg)
		{
			return Equals (msg, str);
		}

		/// <summary>
		/// 确定<see cref="QQMessage"/> 和字符串是否具有相同的值
		/// </summary>
		/// <param name="msg">要比较的第一个<see cref="QQMessage"/>对象，或 null</param>
		/// <param name="str">要比较的第二个字符串，或 null</param>
		/// <returns>如果 msg 中的值与 str 不同, 则为 <code>true</code>, 否则为 <code>false</code></returns>
		[TargetedPatchingOptOut ("性能至关重要的内联跨NGen图像边界")]
		public static bool operator != (QQMessage msg, string str)
		{
			return !Equals (msg, str);
		}

		/// <summary>
		/// 确定<see cref="QQMessage"/> 和字符串是否具有相同的值
		/// </summary>
		/// <param name="str">要比较的第一个字符串，或 null</param>
		/// <param name="msg">要比较的第二个<see cref="QQMessage"/>对象，或 null</param>
		/// <returns>如果 str与 msg 中的值不同, 则为 <code>true</code>, 否则为 <code>false</code></returns>
		[TargetedPatchingOptOut ("性能至关重要的内联跨NGen图像边界")]
		public static bool operator != (string str, QQMessage msg)
		{
			return !Equals (msg, str);
		}
		#endregion
	}
}
