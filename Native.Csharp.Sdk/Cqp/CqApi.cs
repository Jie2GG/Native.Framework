using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Native.Csharp.Sdk.Cqp.Core;
using Native.Csharp.Sdk.Cqp.Enum;
using Native.Csharp.Sdk.Cqp.Expand;
using Native.Csharp.Sdk.Cqp.Model;

namespace Native.Csharp.Sdk.Cqp
{
	/// <summary>
	/// 表示 酷Q接口 的封装类
	/// </summary>
	public class CQApi
	{
		#region --字段--
		private readonly int _authCode = 0;
		private static readonly Encoding _defaultEncoding = null;
		private string _appDirectoryCache = null;
		#endregion

		#region --属性--
		/// <summary>
		/// 获取接口默认编码
		/// </summary>
		public static Encoding DefaultEncoding { get { return _defaultEncoding; } }

		/// <summary>
		/// 获取当前实例的验证码
		/// </summary>
		public int AuthCode { get { return _authCode; } }

		/// <summary>
		/// 获取酷Q分配给本应用的数据路径 (所用应用数据应存放于此路径下)
		/// </summary>
		public string AppDirectory
		{
			get
			{
				if (string.IsNullOrEmpty (this._appDirectoryCache))
				{
					this._appDirectoryCache = CQP.CQ_getAppDirectory (AuthCode).ToString (DefaultEncoding);
				}
				return this._appDirectoryCache;
			}
		}

		/// <summary>
		/// 获取一个值, 指示正在运行的酷Q版本是否支持发送语音
		/// </summary>
		public bool IsAllowSendRecord { get { return CQP.CQ_canSendRecord (AuthCode) > 0; } }

		/// <summary>
		/// 获取一个值, 指示正在运行的酷Q版本是否支持发送图片
		/// </summary>
		public bool IsAllowSendImage { get { return CQP.CQ_canSendImage (AuthCode) > 0; } }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 静态初始化 <see cref="CQApi"/> 类的静态实例, 该实例只会构造一次, 初始化一些公共资源
		/// </summary>
		static CQApi ()
		{
			_defaultEncoding = Encoding.GetEncoding ("GB18030");
		}

		/// <summary>
		/// 初始化 <see cref="CQApi"/> 类的新实例, 该实例由 <code>Initialize</code> 函数进行授权
		/// </summary>
		/// <param name="authCode">授权码</param>
		public CQApi (int authCode)
		{
			this._authCode = authCode;
		}
		#endregion

		#region --CQ码类方法--
		/// <summary>
		/// 获取酷Q "At某人" 代码
		/// </summary>
		/// <param name="qqId">QQ号</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: qqId 超出范围</exception>
		/// <returns>返回 <see cref="CQCode"/> 对象</returns>
		public static CQCode CQCode_At (long qqId)
		{
			if (qqId < QQ.MinValue)
			{
				throw new ArgumentOutOfRangeException ("qqId");
			}
			return new CQCode (
				CQFunction.At,
				new KeyValuePair<string, string> ("qq", Convert.ToString (qqId)));
		}

		/// <summary>
		/// 获取酷Q "At某人" 代码
		/// </summary>
		/// <param name="qq">QQ对象</param>
		/// <exception cref="ArgumentNullException">参数: qq 为 null</exception>
		/// <returns>返回 <see cref="CQCode"/> 对象</returns>
		public static CQCode CQCode_At (QQ qq)
		{
			if (qq == null)
			{
				throw new ArgumentNullException ("qq");
			}

			return CQCode_At (qq.Id);
		}

		/// <summary>
		/// 获取酷Q "At全体成员" 代码
		/// </summary>
		/// <returns>返回 <see cref="CQCode"/> 对象</returns>
		public static CQCode CQCode_AtAll ()
		{
			return new CQCode (
				CQFunction.At,
				new KeyValuePair<string, string> ("qq", "all"));
		}

		/// <summary>
		/// 获取酷Q "Emoji" 代码
		/// </summary>
		/// <param name="id">Emoji的Id</param>
		/// <returns>返回 <see cref="CQCode"/> 对象</returns>
		public static CQCode CQCode_Emoji (int id)
		{
			return new CQCode (
				CQFunction.Emoji,
				new KeyValuePair<string, string> ("id", Convert.ToString (id)));
		}

		/// <summary>
		/// 获取酷Q "表情" 代码
		/// </summary>
		/// <param name="face">表情枚举</param>
		/// <returns>返回 <see cref="CQCode"/> 对象</returns>
		public static CQCode CQCode_Face (CQFace face)
		{
			return new CQCode (
				CQFunction.Face,
				new KeyValuePair<string, string> ("id", Convert.ToString ((int)face)));
		}

		/// <summary>
		/// 获取酷Q "戳一戳" 代码
		/// </summary>
		/// <returns>返回 <see cref="CQCode"/> 对象</returns>
		public static CQCode CQCode_Shake ()
		{
			return new CQCode (CQFunction.Shake);
		}

		/// <summary>
		/// 获取字符串副本的转义形式
		/// </summary>
		/// <param name="source">欲转义的原始字符串</param>
		/// <param name="enCodeComma">是否转义逗号, 默认 <code>false</code></param>
		/// <exception cref="ArgumentNullException">参数: source 为 null</exception>
		/// <returns>返回转义后的字符串副本</returns>
		public static string CQEnCode (string source, bool enCodeComma)
		{
			if (source == null)
			{
				throw new ArgumentNullException ("source");
			}
			StringBuilder builder = new StringBuilder (source);
			builder = builder.Replace ("&", "&amp;");
			builder = builder.Replace ("[", "&#91;");
			builder = builder.Replace ("]", "&#93;");
			if (enCodeComma)
			{
				builder = builder.Replace (",", "&#44;");
			}
			return builder.ToString ();
		}

		/// <summary>
		/// 获取字符串副本的非转义形式
		/// </summary>
		/// <param name="source">欲反转义的原始字符串</param>
		/// <exception cref="ArgumentNullException">参数: source 为 null</exception>
		/// <returns>返回反转义的字符串副本</returns>
		public static string CQDeCode (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException ("source");
			}
			StringBuilder builder = new StringBuilder (source);
			builder = builder.Replace ("&#91;", "[");
			builder = builder.Replace ("&#93;", "]");
			builder = builder.Replace ("&#44;", ",");
			builder = builder.Replace ("&amp;", "&");
			return builder.ToString ();
		}

		/// <summary>
		/// 获取酷Q "链接分享" 代码
		/// </summary>
		/// <param name="url">分享的链接</param>
		/// <param name="title">显示的标题, 建议12字以内</param>
		/// <param name="content">简介信息, 建议30字以内</param>
		/// <param name="imageUrl">分享的图片链接, 留空则为默认图片</param>
		/// <exception cref="ArgumentException">参数: url 是空字符串或为 null</exception>
		/// <returns>返回 <see cref="CQCode"/> 对象</returns>
		public static CQCode CQCode_ShareLink (string url, string title, string content, string imageUrl = null)
		{
			if (string.IsNullOrEmpty (url))
			{
				throw new ArgumentException ("分享的链接为空", "url");
			}

			CQCode code = new CQCode (
				CQFunction.Share,
				new KeyValuePair<string, string> ("url", url));

			if (!string.IsNullOrEmpty (title))
			{
				code.Items.Add ("title", title);
			}
			if (!string.IsNullOrEmpty (content))
			{
				code.Items.Add ("content", content);
			}
			if (!string.IsNullOrEmpty (imageUrl))
			{
				code.Items.Add ("image", imageUrl);
			}

			return code;
		}

		/// <summary>
		/// 获取酷Q "好友名片分享" 代码
		/// </summary>
		/// <param name="qqId">QQ号码</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: qqId 超出范围</exception>
		/// <returns>返回 <see cref="CQCode"/> 对象</returns>
		public static CQCode CQCode_ShareFriendCard (long qqId)
		{
			if (qqId < QQ.MinValue)
			{
				throw new ArgumentOutOfRangeException ("qqId");
			}
			return new CQCode (CQFunction.Contact,
				new KeyValuePair<string, string> ("type", "qq"),
				new KeyValuePair<string, string> ("id", Convert.ToString (qqId)));
		}

		/// <summary>
		/// 获取酷Q "好友名片分享" 代码
		/// </summary>
		/// <param name="qq">QQ号码</param>
		/// <exception cref="ArgumentNullException">参数: qq 为 null</exception>
		/// <returns>返回 <see cref="CQCode"/> 对象</returns>
		public static CQCode CQCode_ShareFriendCard (QQ qq)
		{
			if (qq == null)
			{
				throw new ArgumentNullException ("qq");
			}

			return CQCode_ShareFriendCard (qq.Id);
		}

		/// <summary>
		/// 获取酷Q "群名片分享" 代码
		/// </summary>
		/// <param name="groupId">群组</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 超出范围</exception>
		/// <returns>返回 <see cref="CQCode"/> 对象</returns>
		public static CQCode CQCode_ShareGroupCard (long groupId)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}
			return new CQCode (CQFunction.Contact,
				new KeyValuePair<string, string> ("type", "group"),
				new KeyValuePair<string, string> ("group", Convert.ToString (groupId)));
		}

		/// <summary>
		/// 获取酷Q "群名片分享" 代码
		/// </summary>
		/// <param name="group">群组</param>
		/// <exception cref="ArgumentNullException">参数: group 为 null</exception>
		/// <returns>返回 <see cref="CQCode"/> 对象</returns>
		public static CQCode CQCode_ShareGroupCard (Group group)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			return CQCode_ShareGroupCard (group.Id);
		}

		/// <summary>
		/// 获取酷Q "位置分享" 代码
		/// </summary>
		/// <param name="site">地点, 建议12字以内</param>
		/// <param name="detail">详细地址, 建议20字以内</param>
		/// <param name="lat">维度</param>
		/// <param name="lon">经度</param>
		/// <param name="zoom">放大倍数, 默认: 15倍</param>
		/// <exception cref="ArgumentException">参数: site 或 detail 是空字符串或为 null</exception>
		/// <returns>返回 <see cref="CQCode"/> 对象</returns>
		public static CQCode CQCode_ShareGPS (string site, string detail, double lat, double lon, int zoom = 15)
		{
			if (string.IsNullOrEmpty (site))
			{
				throw new ArgumentException ("分享的地点不能为空", "site");
			}

			if (string.IsNullOrEmpty (detail))
			{
				throw new ArgumentException ("详细地址不能为空", "detail");
			}

			return new CQCode (
				CQFunction.Location,
				new KeyValuePair<string, string> ("lat", Convert.ToString (lat)),
				new KeyValuePair<string, string> ("lon", Convert.ToString (lon)),
				new KeyValuePair<string, string> ("zoom", Convert.ToString (zoom)),
				new KeyValuePair<string, string> ("title", site),
				new KeyValuePair<string, string> ("content", detail));
		}

		/// <summary>
		/// 获取酷Q "匿名" 代码
		/// </summary>
		/// <param name="forced">强制发送, 若本参数为 <code>true</code> 发送失败时将转换为普通消息</param>
		/// <returns>返回 <see cref="CQCode"/> 对象</returns>
		public static CQCode CQCode_Anonymous (bool forced = false)
		{
			CQCode code = new CQCode (CQFunction.Anonymous);
			if (forced)
			{
				code.Items.Add ("ignore", "true");
			}
			return code;
		}

		/// <summary>
		/// 获取酷Q "音乐" 代码
		/// </summary>
		/// <param name="id"></param>
		/// <param name="type"></param>
		/// <param name="style"></param>
		/// <returns>返回 <see cref="CQCode"/> 对象</returns>
		public static CQCode CQCode_Music (long id, CQMusicType type = CQMusicType.Tencent, CQMusicStyle style = CQMusicStyle.Old)
		{
			return new CQCode (
				CQFunction.Music,
				new KeyValuePair<string, string> ("id", Convert.ToString (id)),
				new KeyValuePair<string, string> ("type", type.GetDescription ()),
				new KeyValuePair<string, string> ("style", Convert.ToString ((int)style)));
		}

		/// <summary>
		/// 获取酷Q "音乐自定义" 代码
		/// </summary>
		/// <param name="url">分享链接, 点击后进入的页面 (歌曲介绍)</param>
		/// <param name="musicUrl">歌曲链接, 音频链接 (mp3链接)</param>
		/// <param name="title">标题, 建议12字以内</param>
		/// <param name="content">简介, 建议30字以内</param>
		/// <param name="imageUrl">封面图片链接, 留空为默认</param>
		/// <exception cref="ArgumentException">参数: url 或 musicUrl 是空字符串或为 null</exception>
		/// <returns>返回 <see cref="CQCode"/> 对象</returns>
		public static CQCode CQCode_DIYMusic (string url, string musicUrl, string title = null, string content = null, string imageUrl = null)
		{
			if (string.IsNullOrEmpty (url))
			{
				throw new ArgumentException ("分享链接不能为空", "url");
			}

			if (string.IsNullOrEmpty (musicUrl))
			{
				throw new ArgumentException ("歌曲链接不能为空", "musicUrl");
			}

			CQCode code = new CQCode (
				CQFunction.Music,
				new KeyValuePair<string, string> ("type", "custom"),
				new KeyValuePair<string, string> ("url", url),
				new KeyValuePair<string, string> ("audio", musicUrl));
			if (!string.IsNullOrEmpty (title))
			{
				code.Items.Add ("title", title);
			}

			if (!string.IsNullOrEmpty (content))
			{
				code.Items.Add ("content", content);
			}

			if (!string.IsNullOrEmpty (imageUrl))
			{
				code.Items.Add ("imageUrl", imageUrl);
			}
			return code;
		}

		/// <summary>
		/// 获取酷Q "图片" 代码
		/// </summary>
		/// <param name="path">图片的路径, 将图片放在 酷Q\data\image 下, 并填写相对路径. 如 酷Q\data\image\1.jpg 则填写 1.jpg</param>
		/// <exception cref="ArgumentException">参数: path 是空字符串或为 null</exception>
		/// <returns>返回 <see cref="CQCode"/> 对象</returns>
		public static CQCode CQCode_Image (string path)
		{
			if (string.IsNullOrEmpty (path))
			{
				throw new ArgumentException ("路径不能为空", "path");
			}

			return new CQCode (
				CQFunction.Image,
				new KeyValuePair<string, string> ("file", path));
		}

		/// <summary>
		/// 获取酷Q "语音" 代码
		/// </summary>
		/// <param name="path">语音的路径, 将音频放在 酷Q\data\record 下, 并填写相对路径. 如 酷Q\data\record\1.amr 则填写 1.amr</param>
		/// <exception cref="ArgumentException">参数: path 是空字符串或为 null</exception>
		/// <returns>返回 <see cref="CQCode"/> 对象</returns>
		public static CQCode CQCode_Record (string path)
		{
			if (string.IsNullOrEmpty (path))
			{
				throw new ArgumentException ("语音路径不允许为空", "path");
			}

			return new CQCode (
				CQFunction.Record,
				new KeyValuePair<string, string> ("file", path));
		}
		#endregion

		#region --消息类方法--
		/// <summary>
		/// 发送群消息
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="message">消息内容, 获取内容时将调用<see cref="object.ToString"/>进行消息转换</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 超出范围</exception>
		/// <excpetion cref="ArgumentNullException">参数: message 中含有 null</excpetion>
		/// <returns>发送成功将返回消息ID, 发送失败则返回负数</returns>
		public int SendGroupMessage (long groupId, params object[] message)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}

			GCHandle messageHandle = message.ToSendString ().GetStringGCHandle (CQApi.DefaultEncoding);
			try
			{
				return CQP.CQ_sendGroupMsg (this.AuthCode, groupId, messageHandle.AddrOfPinnedObject ());
			}
			finally
			{
				messageHandle.Free ();
			}
		}

		/// <summary>
		/// 发送群消息
		/// </summary>
		/// <param name="group">目标群</param>
		/// <param name="message">消息内容, 获取内容时将调用<see cref="object.ToString"/>进行消息转换</param>
		/// <excpetion cref="ArgumentNullException">参数: group 是 null 或 message 中含有 null</excpetion>
		/// <returns>发送成功将返回 <see cref="QQMessage"/> 对象</returns>
		public QQMessage SendGroupMessage (Group group, params object[] message)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			string msg = message.ToSendString ();
			int msgId = this.SendGroupMessage (group.Id, msg);
			return new QQMessage (this, msgId, msg);
		}

		/// <summary>
		/// 发送私聊消息
		/// </summary>
		/// <param name="qqId">目标QQ</param>
		/// <param name="message">消息内容, 获取内容时将调用<see cref="object.ToString"/>进行消息转换</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: qqId 超出范围</exception>
		/// <excpetion cref="ArgumentNullException">当参数: message 中含有 null</excpetion>
		/// <returns>发送成功将返回消息 Id, 发送失败将返回负值</returns>
		public int SendPrivateMessage (long qqId, params object[] message)
		{
			if (qqId < QQ.MinValue)
			{
				throw new ArgumentOutOfRangeException ("qqId");
			}

			GCHandle messageHandle = message.ToSendString ().GetStringGCHandle (CQApi.DefaultEncoding);
			try
			{
				return CQP.CQ_sendPrivateMsg (this.AuthCode, qqId, messageHandle.AddrOfPinnedObject ());
			}
			finally
			{
				messageHandle.Free ();
			}
		}

		/// <summary>
		/// 发送私聊消息
		/// </summary>
		/// <param name="qq">目标QQ</param>
		/// <param name="message">消息内容, 获取内容时将调用<see cref="object.ToString"/>进行消息转换</param>
		/// <excpetion cref="ArgumentNullException">参数: qq 是 null 或 message 中含有 null</excpetion>
		/// <returns>发送成功将返回 <see cref="QQMessage"/> 对象</returns>
		public QQMessage SendPrivateMessage (QQ qq, params object[] message)
		{
			if (qq == null)
			{
				throw new ArgumentNullException ("qq");
			}

			string msg = message.ToSendString ();
			int msgId = this.SendPrivateMessage (qq.Id, msg);
			return new QQMessage (this, msgId, msg);
		}

		/// <summary>
		/// 发送讨论组消息
		/// </summary>
		/// <param name="discussId">目标讨论组</param>
		/// <param name="message">消息内容, 获取内容时将调用<see cref="object.ToString"/>进行消息转换</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: discussId 超出范围</exception>
		/// <exception cref="ArgumentNullException">当参数: message 中含有 null</exception>
		/// <returns>发送成功将返回消息 Id, 发送失败将返回负值</returns>
		public int SendDiscussMessage (long discussId, params object[] message)
		{
			if (discussId < Discuss.MinValue)
			{
				throw new ArgumentOutOfRangeException ("discussId");
			}

			GCHandle messageHandle = message.ToSendString ().GetStringGCHandle (CQApi.DefaultEncoding);
			try
			{
				return CQP.CQ_sendDiscussMsg (this.AuthCode, discussId, messageHandle.AddrOfPinnedObject ());
			}
			finally
			{
				messageHandle.Free ();
			}
		}

		/// <summary>
		/// 发送讨论组消息
		/// </summary>
		/// <param name="discuss">目标讨论组</param>
		/// <param name="message">消息内容, 获取内容时将调用<see cref="object.ToString"/>进行消息转换</param>
		/// <exception cref="ArgumentNullException">参数: discuss 是 null 或 message 中含有 null</exception>
		/// <returns>发送成功将返回消息 Id, 发送失败将返回负值</returns>
		public QQMessage SendDiscussMessage (Discuss discuss, params object[] message)
		{
			if (discuss == null)
			{
				throw new ArgumentNullException ("discuss");
			}

			string msg = message.ToSendString ();
			int msgId = this.SendDiscussMessage (discuss.Id, msg);
			return new QQMessage (this, msgId, msg);
		}

		/// <summary>
		/// 发送赞
		/// </summary>
		/// <param name="qqId">目标QQ</param>
		/// <param name="count">发送赞的次数, 范围: 1~10 (留空为1次)</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: qqId 或 count 超出范围</exception>
		/// <returns>执行成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SendPraise (long qqId, int count = 1)
		{
			if (qqId < QQ.MinValue)
			{
				throw new ArgumentOutOfRangeException ("qqId");
			}

			if (count < 1 || count > 10)
			{
				throw new ArgumentOutOfRangeException ("count", count, "点赞次数超出可处理范围, 其次数最少为 1, 最多为 10");
			}

			return CQP.CQ_sendLikeV2 (this.AuthCode, qqId, count) == 0;
		}

		/// <summary>
		/// 发送赞
		/// </summary>
		/// <param name="qq">目标QQ</param>
		/// <param name="count">发送赞的次数, 范围: 1~10 (留空为1次)</param>
		/// <exception cref="ArgumentNullException">参数: qq 为 null</exception>
		/// <exception cref="ArgumentOutOfRangeException">参数: count 超出范围</exception>
		/// <returns>执行成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SendPraise (QQ qq, int count = 1)
		{
			if (qq == null)
			{
				throw new ArgumentNullException ("qq");
			}

			return this.SendPraise (qq.Id, count);
		}

		/// <summary>
		/// 撤回消息
		/// </summary>
		/// <param name="msgId">消息Id</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: msgId 超出范围</exception>
		/// <returns>执行成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveMessage (int msgId)
		{
			if (msgId < 0)
			{
				throw new ArgumentOutOfRangeException ("msgId");
			}

			return CQP.CQ_deleteMsg (this.AuthCode, msgId) == 0;
		}

		/// <summary>
		/// 撤回消息
		/// </summary>
		/// <param name="message">消息</param>
		/// <exception cref="ArgumentNullException">参数: message 为 null</exception>
		/// <returns>执行成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveMessage (QQMessage message)
		{
			if (message == null)
			{
				throw new ArgumentNullException ("message");
			}

			return this.RemoveMessage (message.Id);
		}

		/// <summary>
		/// 接收消息中的语音 (消息含有CQ码 "record" 的消息)
		/// </summary>
		/// <param name="fileName">文件名, [CQ:record...] 中的文件名部分</param>
		/// <param name="format">所需的目标语音的音频格式</param>
		/// <exception cref="ArgumentException">参数: fileName 是空字符串或为 null</exception>
		/// <returns>返回语音文件位于本地服务器的绝对路径</returns>
		public string ReceiveRecord (string fileName, CQAudioFormat format)
		{
			if (string.IsNullOrEmpty (fileName))
			{
				throw new ArgumentException ("文件名不可为空", "fileName");
			}

			GCHandle fileNameHandler = fileName.GetStringGCHandle (CQApi.DefaultEncoding);
			GCHandle formatHandler = format.GetDescription ().GetStringGCHandle (CQApi.DefaultEncoding);

			try
			{
				return CQP.CQ_getRecordV2 (AuthCode, fileNameHandler.AddrOfPinnedObject (), formatHandler.AddrOfPinnedObject ()).ToString (CQApi.DefaultEncoding);
			}
			finally
			{
				fileNameHandler.Free ();
				formatHandler.Free ();
			}
		}

		/// <summary>
		/// 接收消息中的语音 (消息含有CQ码 "record" 的消息)
		/// </summary>
		/// <param name="recordCode">CQ码对象 <see cref="CQCode"/>, 必须是 <see cref="CQFunction.Record"/> 类型</param>
		/// <param name="format">所需的目标语音的音频格式</param>
		/// <exception cref="ArgumentNullException">参数: recordCode 是 null</exception>
		/// <exception cref="ArgumentException">参数: recordCode 不是 Record 类型</exception>
		/// <returns>返回语音文件位于本地服务器的绝对路径</returns>
		public string ReceiveRecord (CQCode recordCode, CQAudioFormat format)
		{
			if (recordCode == null)
			{
				throw new ArgumentNullException ("recordCode");
			}

			if (recordCode.Function != CQFunction.Record)
			{
				throw new ArgumentException ("传入的CQ码不是 \"Record\" 类型", "recordCode");
			}

			return this.ReceiveRecord (recordCode.Items["file"], format);
		}

		/// <summary>
		/// 接收消息中的图片 (消息含有CQ码 "image" 的消息)
		/// </summary>
		/// <param name="fileName">文件名, [CQ:image...] 中的文件名部分</param>
		/// <exception cref="ArgumentException">文件名为空时发生</exception>
		/// <returns>返回图片文件位于本地服务器的绝对路径</returns>
		public string ReceiveImage (string fileName)
		{
			if (string.IsNullOrEmpty (fileName))
			{
				throw new ArgumentException ("文件名不可为空", "fileName");
			}

			GCHandle handle = fileName.GetStringGCHandle (CQApi.DefaultEncoding);
			try
			{
				return CQP.CQ_getImage (AuthCode, handle.AddrOfPinnedObject ()).ToString (CQApi.DefaultEncoding);
			}
			finally
			{
				handle.Free ();
			}
		}

		/// <summary>
		/// 接收消息中的图片 (消息含有CQ码 "image" 的消息)
		/// </summary>
		/// <param name="imageCode">CQ码对象 <see cref="CQCode"/>, 必须是 <see cref="CQFunction.Image"/> 类型</param>
		/// <exception cref="ArgumentNullException">参数: imageCode 是 null</exception>
		/// <exception cref="ArgumentException">参数: imageCode 不是 Image 类型</exception>
		/// <returns>返回图片文件位于本地服务器的绝对路径</returns>
		public string ReceiveImage (CQCode imageCode)
		{
			if (imageCode == null)
			{
				throw new ArgumentNullException ("imageCode");
			}

			if (imageCode.Function != CQFunction.Image)
			{
				throw new ArgumentException ("传入的CQ码不是 \"Image\" 类型", "imageCode");
			}

			return this.ReceiveImage (imageCode.Items["file"]);
		}
		#endregion

		#region --框架类方法--
		/// <summary>
		/// 获取登录帐号
		/// </summary>
		/// <returns>返回当前酷Q框架登录的帐号</returns>
		public long GetLoginQQId ()
		{
			return CQP.CQ_getLoginQQ (this.AuthCode);
		}

		/// <summary>
		/// 获取登录帐号
		/// </summary>
		/// <returns>当前酷Q框架登录的帐号对象</returns>
		public QQ GetLoginQQ ()
		{
			return new QQ (this, this.GetLoginQQId ());
		}

		/// <summary>
		/// 获取登录帐号的昵称
		/// </summary>
		/// <returns>返回当前登录帐号的昵称字符串</returns>
		public string GetLoginNick ()
		{
			return CQP.CQ_getLoginNick (this.AuthCode).ToString (CQApi.DefaultEncoding);
		}

		/// <summary>
		/// 获取 Cookies. 慎用, 此接口需要严格授权
		/// </summary>
		/// <param name="domain">要获取的目标域名的 Cookies, 如 api.example.com</param>
		/// <exception cref="ArgumentNullException">参数: domain 是 null</exception>
		/// <returns>返回 Cookies 字符串</returns>
		public string GetCookies (string domain)
		{
			if (domain == null)
			{
				throw new ArgumentNullException ("domain");
			}

			GCHandle handle = domain.GetStringGCHandle (CQApi.DefaultEncoding);
			try
			{
				return CQP.CQ_getCookiesV2 (AuthCode, handle.AddrOfPinnedObject ()).ToString (CQApi.DefaultEncoding);
			}
			finally
			{
				handle.Free ();
			}
		}

		/// <summary>
		/// 获取 Cookies. 慎用, 此接口需要严格授权
		/// </summary>
		/// <param name="domain">要获取的目标域名的 Cookies, 如 api.example.com</param>
		/// <exception cref="ArgumentNullException">参数: domain 是 null</exception>
		/// <exception cref="ArgumentException">参数: domain 不是正确的 Uri</exception>
		/// <exception cref="CookieException">解析 Cookie 时发生错误</exception>
		/// <returns>返回 Cookies 的 <see cref="CookieCollection"/></returns>
		public CookieCollection GetCookieCollection (string domain)
		{
			try
			{
				// 初始化域名对象
				Uri uri = new Uri (domain);

				// 使用 Container 对象解析Cookies
				CookieContainer container = new CookieContainer ();
				container.SetCookies (uri, this.GetCookies (domain));

				// 返回实例
				return container.GetCookies (uri);
			}
			catch (UriFormatException ex)
			{
				throw new ArgumentException ("domain", ex);
			}
			catch (CookieException)
			{
				throw;
			}
			catch (ArgumentNullException)
			{
				throw;
			}
		}

		/// <summary>
		/// 获取登录QQ网页需要用到的 bkn/g_tk 等. 慎用, 此接口需要严格收取
		/// </summary>
		/// <returns>返回 bkn/g_tk 值</returns>
		public int GetCsrfToken ()
		{
			return CQP.CQ_getCsrfToken (this.AuthCode);
		}

		/// <summary>
		/// 获取陌生人信息
		/// </summary>
		/// <param name="qqId">目标QQ</param>
		/// <param name="notCache">不使用缓存, 默认为 <code>false</code>, 通常忽略本参数, 仅在必要时使用</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: qqId 超出范围</exception>
		/// <exception cref="InvalidDataException">获取信息时得到的数据流错误</exception>
		/// <returns>获取成功返回 <see cref="StrangerInfo"/></returns>
		public StrangerInfo GetStrangerInfo (long qqId, bool notCache = false)
		{
			if (qqId < QQ.MinValue)
			{
				throw new ArgumentOutOfRangeException ("qqId");
			}

			byte[] data = Convert.FromBase64String (CQP.CQ_getStrangerInfo (this.AuthCode, qqId, notCache).ToString (CQApi.DefaultEncoding));
			try
			{
				return new StrangerInfo (this, data);
			}
			catch (ArgumentNullException ex)
			{
				throw new InvalidDataException ("数据流格式错误", ex);
			}
		}

		/// <summary>
		/// 获取陌生人信息
		/// </summary>
		/// <param name="qq">目标QQ</param>
		/// <param name="notCache">不使用缓存, 默认为 <code>false</code>, 通常忽略本参数, 仅在必要时使用</param>
		/// <exception cref="ArgumentNullException">参数: qq 是 null</exception>
		/// <exception cref="InvalidDataException">获取信息时得到的数据流错误</exception>
		/// <returns>获取成功返回 <see cref="StrangerInfo"/></returns>
		public StrangerInfo GetStrangerInfo (QQ qq, bool notCache = false)
		{
			if (qq == null)
			{
				throw new ArgumentNullException ("qq");
			}

			try
			{
				return this.GetStrangerInfo (qq.Id, notCache);
			}
			catch (InvalidDataException)
			{
				throw;
			}
		}

		/// <summary>
		/// 获取好友列表
		/// </summary>
		/// <exception cref="InvalidDataException">数据流格式错误或为 null</exception>
		/// <exception cref="EndOfStreamException">无法读取数据, 因为已经读取到数据流末尾</exception>
		/// <returns>获取成功返回 <see cref="List{FriendInfo}"/></returns>
		public List<FriendInfo> GetFriendList ()
		{
			byte[] data = Convert.FromBase64String (CQP.CQ_getFriendList (AuthCode, false).ToString (_defaultEncoding));
			if (data == null)
			{
				throw new InvalidDataException ("获取的数据为 null");
			}
			using (BinaryReader reader = new BinaryReader (new MemoryStream (data)))
			{
				List<FriendInfo> tempFriends = new List<FriendInfo> (reader.ReadInt32_Ex ());
				for (int i = 0; i < tempFriends.Capacity; i++)
				{
					if (reader.Length () <= 0)
					{
						throw new EndOfStreamException ("无法读取数据, 因为已读取到数据流末尾");
					}

					try
					{
						tempFriends.Add (new FriendInfo (this, reader.ReadToken_Ex ()));
					}
					catch (ArgumentNullException ex)
					{
						throw new InvalidDataException ("数据流格式错误", ex);
					}
					catch (InvalidDataException ex)
					{
						throw new EndOfStreamException ("无法读取数据, 因为已读取到数据流末尾", ex);
					}
				}
				return tempFriends;
			}
		}

		/// <summary>
		/// 获取群成员信息
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="qqId">目标帐号</param>
		/// <param name="notCache">不使用缓存, 默认为 <code>false</code>, 通常忽略本参数, 仅在必要时使用</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 或 qqId 超出范围</exception>
		/// <exception cref="InvalidDataException">数据流格式错误 或为 null</exception>
		/// <exception cref="EndOfStreamException">无法读取数据, 因为已读取到数据流末尾</exception>
		/// <returns>获取成功返回 <see cref="GroupMemberInfo"/></returns>
		public GroupMemberInfo GetGroupMemberInfo (long groupId, long qqId, bool notCache = false)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}

			if (qqId < QQ.MinValue)
			{
				throw new ArgumentOutOfRangeException ("qqId");
			}

			byte[] data = Convert.FromBase64String (CQP.CQ_getGroupMemberInfoV2 (this.AuthCode, groupId, qqId, notCache).ToString (CQApi.DefaultEncoding));
			try
			{
				return new GroupMemberInfo (this, data);
			}
			catch (ArgumentNullException ex)
			{
				throw new InvalidDataException ("数据流格式错误或数据流为 null", ex);
			}
			catch (InvalidDataException ex)
			{
				throw new EndOfStreamException ("无法读取数据, 因为已读取到数据流末尾", ex);
			}
		}

		/// <summary>
		/// 获取群成员信息
		/// </summary>
		/// <param name="group">目标群</param>
		/// <param name="qq">目标帐号</param>
		/// <param name="notCache">不使用缓存, 默认为 <code>false</code>, 通常忽略本参数, 仅在必要时使用</param>
		/// <exception cref="ArgumentNullException">参数: group 为 null 或 qq 为 null</exception>
		/// <exception cref="InvalidDataException">数据流格式错误</exception>
		/// <exception cref="EndOfStreamException">无法读取数据, 因为已读取到数据流末尾</exception>
		/// <returns>获取成功返回 <see cref="GroupMemberInfo"/></returns>
		public GroupMemberInfo GetGroupMemberInfo (Group group, QQ qq, bool notCache = false)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			if (qq == null)
			{
				throw new ArgumentNullException ("qq");
			}

			try
			{
				return this.GetGroupMemberInfo (group.Id, qq.Id, notCache);
			}
			catch (InvalidDataException)
			{
				throw;
			}
			catch (EndOfStreamException)
			{
				throw;
			}
		}

		/// <summary>
		/// 获取群成员列表
		/// </summary>
		/// <param name="groupId">目标群号</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 超出范围</exception>
		/// <exception cref="InvalidDataException">数据流格式错误或为 null</exception>
		/// <exception cref="EndOfStreamException">无法读取数据, 因为已读取到数据流末尾</exception>
		/// <returns>获取成功返回 <see cref="List{GroupMemberInfo}"/></returns>
		public List<GroupMemberInfo> GetGroupMemberList (long groupId)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}

			byte[] data = Convert.FromBase64String (CQP.CQ_getGroupMemberList (this.AuthCode, groupId).ToString (CQApi.DefaultEncoding));
			if (data == null)
			{
				throw new InvalidDataException ("获取的数据为 null");
			}

			using (BinaryReader reader = new BinaryReader (new MemoryStream (data)))
			{
				List<GroupMemberInfo> members = new List<GroupMemberInfo> (reader.ReadInt32_Ex ());
				for (int i = 0; i < members.Capacity; i++)
				{
					if (reader.Length () <= 0)
					{
						throw new EndOfStreamException ("无法读取数据, 因为已读取到数据流末尾");
					}

					try
					{
						members.Add (new GroupMemberInfo (this, reader.ReadToken_Ex ()));
					}
					catch (ArgumentNullException ex)
					{
						throw new InvalidDataException ("数据流格式错误", ex);
					}
					catch (InvalidDataException ex)
					{
						throw new EndOfStreamException ("无法读取数据, 因为已读取到数据流末尾", ex);
					}
				}
			}

			return this.GetGroupMemberList (new Group (this, groupId));
		}

		/// <summary>
		/// 获取群成员列表
		/// </summary>
		/// <param name="group">目标群号</param>
		/// <exception cref="ArgumentNullException">参数: group 为 null</exception>
		/// <exception cref="InvalidDataException">数据流格式错误或为 null</exception>
		/// <exception cref="EndOfStreamException">无法读取数据, 因为已读取到数据流末尾</exception>
		/// <returns>获取成功返回 <see cref="List{GroupMemberInfo}"/></returns>
		public List<GroupMemberInfo> GetGroupMemberList (Group group)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			try
			{
				return this.GetGroupMemberList (group.Id);
			}
			catch (InvalidDataException)
			{
				throw;
			}
			catch (EndOfStreamException)
			{
				throw;
			}
		}

		/// <summary>
		/// 获取群信息
		/// </summary>
		/// <param name="groupId">目标群号</param>
		/// <param name="notCache">不使用缓存, 通常为 <code>false</code>, 仅在必要时使用</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 超出范围</exception>
		/// <exception cref="InvalidDataException">数据流格式错误或为 null</exception>
		/// <exception cref="EndOfStreamException">无法读取数据, 因为已读取到数据流末尾</exception>
		/// <returns>获取成功返回 <see cref="GroupInfo"/> 对象</returns>
		public GroupInfo GetGroupInfo (long groupId, bool notCache = false)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}

			byte[] data = Convert.FromBase64String (CQP.CQ_getGroupInfo (this.AuthCode, groupId, notCache).ToString (CQApi.DefaultEncoding));

			try
			{
				return new GroupInfo (this, data);
			}
			catch (ArgumentNullException ex)
			{
				throw new InvalidDataException ("数据流格式错误或为 null", ex);
			}
			catch (InvalidDataException ex)
			{
				throw new EndOfStreamException ("无法读取数据, 因为已读取到数据流末尾", ex);
			}
		}

		/// <summary>
		/// 获取群信息
		/// </summary>
		/// <param name="group">目标群号</param>
		/// <param name="notCache">不使用缓存, 通常为 <code>false</code>, 仅在必要时使用</param>
		/// <exception cref="ArgumentNullException">参数: group 为 null</exception>
		/// <exception cref="InvalidDataException">数据流格式错误或为 null</exception>
		/// <exception cref="EndOfStreamException">无法读取数据, 因为已读取到数据流末尾</exception>
		/// <returns>获取成功返回 <see cref="GroupInfo"/> 对象</returns>
		public GroupInfo GetGroupInfo (Group group, bool notCache = false)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			try
			{
				return this.GetGroupInfo (group.Id, notCache);
			}
			catch (InvalidDataException)
			{
				throw;
			}
			catch (EndOfStreamException)
			{
				throw;
			}
		}

		/// <summary>
		/// 获取群列表
		/// </summary>
		/// <exception cref="InvalidDataException">数据流格式错误或为 null</exception>
		/// <exception cref="EndOfStreamException">无法读取数据, 因为已读取到数据流末尾</exception>
		/// <returns>获取成功返回 <see cref="List{GroupInfo}"/> 对象</returns>
		public List<GroupInfo> GetGroupList ()
		{
			byte[] data = Convert.FromBase64String (CQP.CQ_getGroupList (AuthCode).ToString (Encoding.ASCII));
			if (data == null)
			{
				throw new InvalidDataException ("获取的数据为 null");
			}

			using (BinaryReader reader = new BinaryReader (new MemoryStream (data)))
			{
				List<GroupInfo> tempGroups = new List<GroupInfo> (reader.ReadInt32_Ex ());
				for (int i = 0; i < tempGroups.Capacity; i++)
				{
					if (reader.Length () <= 0)
					{
						throw new EndOfStreamException ("无法读取数据, 因为已读取到数据流末尾");
					}

					try
					{
						tempGroups.Add (new GroupInfo (this, reader.ReadToken_Ex (), true));
					}
					catch (ArgumentNullException ex)
					{
						throw new InvalidDataException ("数据流格式错误", ex);
					}
					catch (InvalidDataException ex)
					{
						throw new EndOfStreamException ("无法读取数据, 因为已读取到数据流末尾", ex);
					}
				}
				return tempGroups;
			}
		}
		#endregion

		#region --群管理方法--
		/// <summary>
		/// 设置群匿名成员禁言
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="anonymous">目标群成员匿名信息</param>
		/// <param name="time">禁言的时长 (范围: 1秒 ~ 30天)</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 或 time 超出范围</exception>
		/// <exception cref="ArgumentNullException">参数: anonymous 是 null</exception>
		/// <returns>禁言成功返回 <code>true</code>, 否则返回 <code>false</code></returns>
		public bool SetGroupAnonymousMemberBanSpeak (long groupId, GroupMemberAnonymousInfo anonymous, TimeSpan time)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}

			if (anonymous == null)
			{
				throw new ArgumentNullException ("anonymous");
			}

			if (time.TotalSeconds <= 0 || time.TotalSeconds > 2592000)  //要在 1秒 ~ 30天 的范围内
			{
				throw new ArgumentOutOfRangeException ("time");
			}

			GCHandle anonymousHandle = anonymous.OriginalString.GetStringGCHandle (CQApi.DefaultEncoding);
			try
			{
				return CQP.CQ_setGroupAnonymousBan (this.AuthCode, groupId, anonymousHandle.AddrOfPinnedObject (), (long)time.TotalSeconds) == 0;
			}
			finally
			{
				anonymousHandle.Free ();
			}
		}

		/// <summary>
		/// 设置群匿名成员禁言
		/// </summary>
		/// <param name="group">目标群</param>
		/// <param name="anonymous">目标群成员匿名信息</param>
		/// <param name="time">禁言的时长 (范围: 1秒 ~ 30天)</param>
		/// <exception cref="ArgumentNullException">参数: group 或 anonymous 是 null</exception>
		/// <exception cref="ArgumentOutOfRangeException">参数: time 超出范围</exception>
		/// <returns>禁言成功返回 <code>true</code>, 否则返回 <code>false</code></returns>
		public bool SetGroupAnonymousMemberBanSpeak (Group group, GroupMemberAnonymousInfo anonymous, TimeSpan time)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			try
			{
				return this.SetGroupAnonymousMemberBanSpeak (group.Id, anonymous, time);
			}
			catch (ArgumentOutOfRangeException)
			{
				throw;
			}
			catch (ArgumentNullException)
			{
				throw;
			}
		}

		/// <summary>
		/// 设置群成员禁言
		/// </summary>
		/// <param name="groupId">目标群号</param>
		/// <param name="qqId">目标QQ</param>
		/// <param name="time">禁言时长 (范围: 1秒 ~ 30天)</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 、qqId 或 time 超出范围</exception>
		/// <returns>禁言成功返回 <code>true</code>, 否则返回 <code>false</code></returns>
		public bool SetGroupMemberBanSpeak (long groupId, long qqId, TimeSpan time)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}

			if (qqId < QQ.MinValue)
			{
				throw new ArgumentOutOfRangeException ("qqId");
			}

			if (time.TotalSeconds <= 0 || time.TotalSeconds > 2592000)  //要在 1秒 ~ 30天 的范围内
			{
				throw new ArgumentOutOfRangeException ("time");
			}

			return CQP.CQ_setGroupBan (this.AuthCode, groupId, qqId, (long)time.TotalSeconds) == 0;
		}

		/// <summary>
		/// 设置群成员禁言
		/// </summary>
		/// <param name="group">目标群号</param>
		/// <param name="qq">目标QQ</param>
		/// <param name="time">禁言时长 (范围: 1秒 ~ 30天)</param>
		/// <exception cref="ArgumentNullException">参数: group 或 qq 为 null</exception>
		/// <exception cref="ArgumentOutOfRangeException">参数: time 超出范围</exception>
		/// <returns>禁言成功返回 <code>true</code>, 否则返回 <code>false</code></returns>
		public bool SetGroupMemberBanSpeak (Group group, QQ qq, TimeSpan time)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			if (qq == null)
			{
				throw new ArgumentNullException ("qq");
			}

			try
			{
				return this.SetGroupMemberBanSpeak (group.Id, qq.Id, time);
			}
			catch (ArgumentOutOfRangeException)
			{
				throw;
			}
		}

		/// <summary>
		/// 解除群成员禁言
		/// </summary>
		/// <param name="groupId">目标群号</param>
		/// <param name="qqId">目标QQ</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 或 qqId 超出范围</exception>
		/// <returns>禁言成功返回 <code>true</code>, 否则返回 <code>false</code></returns>
		public bool RemoveGroupMemberBanSpeak (long groupId, long qqId)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}

			if (qqId < QQ.MinValue)
			{
				throw new ArgumentOutOfRangeException ("qqId");
			}

			return CQP.CQ_setGroupBan (this.AuthCode, groupId, qqId, 0) == 0;
		}

		/// <summary>
		/// 解除群成员禁言
		/// </summary>
		/// <param name="group">目标群号</param>
		/// <param name="qq">目标QQ</param>
		/// <exception cref="ArgumentNullException">参数: group 或 qq 为 null</exception>
		/// <returns>禁言成功返回 <code>true</code>, 否则返回 <code>false</code></returns>
		public bool RemoveGroupMemberBanSpeak (Group group, QQ qq)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			if (qq == null)
			{
				throw new ArgumentNullException ("qq");
			}

			return this.RemoveGroupMemberBanSpeak (group.Id, qq.Id);
		}

		/// <summary>
		/// 设置群全体禁言
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 超出范围</exception>
		/// <returns>操作成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupBanSpeak (long groupId)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}

			return CQP.CQ_setGroupWholeBan (this.AuthCode, groupId, true) == 0;
		}

		/// <summary>
		/// 设置群全体禁言
		/// </summary>
		/// <param name="group">目标群</param>
		/// <exception cref="ArgumentNullException">参数: group 为 null</exception>
		/// <returns>操作成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupBanSpeak (Group group)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			return this.SetGroupBanSpeak (group.Id);
		}

		/// <summary>
		/// 解除群全体禁言
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 超出范围</exception>
		/// <returns>操作成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveGroupBanSpeak (long groupId)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}

			return CQP.CQ_setGroupWholeBan (this.AuthCode, groupId, false) == 0;
		}

		/// <summary>
		/// 解除群全体禁言
		/// </summary>
		/// <param name="group">目标群</param>
		/// <exception cref="ArgumentNullException">参数: group 为 null</exception>
		/// <returns>操作成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveGroupBanSpeak (Group group)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			return this.RemoveGroupBanSpeak (group.Id);
		}

		/// <summary>
		/// 设置群成员名片
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="qqId">目标QQ</param>
		/// <param name="newName">新名称</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 或 qqId 超出范围</exception>
		/// <exception cref="ArgumentNullException">参数: newName 为 null</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupMemberVisitingCard (long groupId, long qqId, string newName)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}

			if (qqId < QQ.MinValue)
			{
				throw new ArgumentOutOfRangeException ("qqId");
			}

			if (newName == null)
			{
				throw new ArgumentNullException ("newName");
			}

			GCHandle newNameHandle = newName.GetStringGCHandle (CQApi.DefaultEncoding);
			try
			{
				return CQP.CQ_setGroupCard (this.AuthCode, groupId, qqId, newNameHandle.AddrOfPinnedObject ()) == 0;
			}
			finally
			{
				newNameHandle.Free ();
			}
		}

		/// <summary>
		/// 设置群成员名片
		/// </summary>
		/// <param name="group">目标群</param>
		/// <param name="qq">目标QQ</param>
		/// <param name="newName">新名称</param>
		/// <exception cref="ArgumentNullException">参数: group、qq 或 newName 为 null</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupMemberVisitingCard (Group group, QQ qq, string newName)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			if (qq == null)
			{
				throw new ArgumentNullException ("qq");
			}

			try
			{
				return this.SetGroupMemberVisitingCard (group.Id, qq.Id, newName);
			}
			catch (ArgumentNullException)
			{
				throw;
			}
		}

		/// <summary>
		/// 设置群成员专属头衔, 并指定其过期的时间
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="qqId">目标QQ</param>
		/// <param name="newTitle">新头衔</param>
		/// <param name="time">过期时间</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId、qqId 或 time 超出范围</exception>
		/// <exception cref="ArgumentNullException">参数: newTtitle 是 null</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupMemberExclusiveTitle (long groupId, long qqId, string newTitle, TimeSpan time)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}

			if (qqId < QQ.MinValue)
			{
				throw new ArgumentOutOfRangeException ("qqId");
			}

			if (newTitle == null)
			{
				throw new ArgumentNullException ("newTitle");
			}

			if (time.TotalSeconds <= 0 || time.TotalSeconds > 2592000)  //要在 1秒 ~ 30天 的范围内
			{
				throw new ArgumentOutOfRangeException ("time");
			}

			GCHandle newTitleHandle = newTitle.GetStringGCHandle (CQApi.DefaultEncoding);
			try
			{
				return CQP.CQ_setGroupSpecialTitle (this.AuthCode, groupId, qqId, newTitleHandle.AddrOfPinnedObject (), (long)time.TotalSeconds) == 0;
			}
			finally
			{
				newTitleHandle.Free ();
			}
		}

		/// <summary>
		/// 设置群成员专属头衔, 并指定其过期的时间
		/// </summary>
		/// <param name="group">目标群</param>
		/// <param name="qq">目标QQ</param>
		/// <param name="newTitle">新头衔</param>
		/// <param name="time">过期时间 (范围: 1秒 ~ 30天)</param>
		/// <exception cref="ArgumentNullException">参数: group、qq 或 newTitle 为 null</exception>
		/// <exception cref="ArgumentOutOfRangeException">参数: time 超出范围</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupMemberExclusiveTitle (Group group, QQ qq, string newTitle, TimeSpan time)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			if (qq == null)
			{
				throw new ArgumentNullException ("qq");
			}

			try
			{
				return this.SetGroupMemberExclusiveTitle (group.Id, qq.Id, newTitle, time);
			}
			catch (ArgumentNullException)
			{
				throw;
			}
			catch (ArgumentOutOfRangeException)
			{
				throw;
			}
		}

		/// <summary>
		/// 设置群成员永久专属头衔
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="qqId">目标QQ</param>
		/// <param name="newTitle">新头衔</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 或 qqId 超出范围</exception>
		/// <exception cref="ArgumentNullException">参数: newTitle 是 null</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupMemberForeverExclusiveTitle (long groupId, long qqId, string newTitle)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}

			if (qqId < QQ.MinValue)
			{
				throw new ArgumentOutOfRangeException ("qqId");
			}

			if (newTitle == null)
			{
				throw new ArgumentNullException ("newTitle");
			}

			GCHandle newTitleHandle = newTitle.GetStringGCHandle (CQApi.DefaultEncoding);
			try
			{
				return CQP.CQ_setGroupSpecialTitle (this.AuthCode, groupId, qqId, newTitleHandle.AddrOfPinnedObject (), -1) == 0;
			}
			finally
			{
				newTitleHandle.Free ();
			}
		}

		/// <summary>
		/// 设置群成员永久专属头衔
		/// </summary>
		/// <param name="group">目标群</param>
		/// <param name="qq">目标QQ</param>
		/// <param name="newTitle">新头衔</param>
		/// <exception cref="ArgumentNullException">参数: group、qq 或 newTitle 为 null</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupMemberForeverExclusiveTitle (Group group, QQ qq, string newTitle)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			if (qq == null)
			{
				throw new ArgumentNullException ("qq");
			}

			try
			{
				return this.SetGroupMemberForeverExclusiveTitle (group.Id, qq.Id, newTitle);
			}
			catch (ArgumentNullException)
			{
				throw;
			}
		}

		/// <summary>
		/// 设置群管理员
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="qqId">目标QQ</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 或 qqId 超出范围</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupManage (long groupId, long qqId)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}

			if (qqId < QQ.MinValue)
			{
				throw new ArgumentOutOfRangeException ("qqId");
			}

			return CQP.CQ_setGroupAdmin (this.AuthCode, groupId, qqId, true) == 0;
		}

		/// <summary>
		/// 设置群管理员
		/// </summary>
		/// <param name="group">目标群</param>
		/// <param name="qq">目标QQ</param>
		/// <exception cref="ArgumentNullException">参数: group 或 qq 为 null</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool SetGroupManage (Group group, QQ qq)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			if (qq == null)
			{
				throw new ArgumentNullException ("qq");
			}

			return this.SetGroupManage (group.Id, qq.Id);
		}

		/// <summary>
		/// 解除群管理员
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="qqId">目标QQ</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 或 qqId 超出范围</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveGroupManage (long groupId, long qqId)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}

			if (qqId < QQ.MinValue)
			{
				throw new ArgumentOutOfRangeException ("qqId");
			}

			return CQP.CQ_setGroupAdmin (this.AuthCode, groupId, qqId, false) == 0;
		}

		/// <summary>
		/// 解除群管理员
		/// </summary>
		/// <param name="group">目标群</param>
		/// <param name="qq">目标QQ</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: group 或 qq 为 null</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveGroupManage (Group group, QQ qq)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			if (qq == null)
			{
				throw new ArgumentNullException ("qq");
			}

			return this.RemoveGroupManage (group.Id, qq.Id);
		}

		/// <summary>
		/// 开启群匿名
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 超出范围</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool OpenGroupAnonymous (long groupId)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}
			return CQP.CQ_setGroupAnonymous (this.AuthCode, groupId, true) == 0;
		}

		/// <summary>
		/// 开启群匿名
		/// </summary>
		/// <param name="group">目标群</param>
		/// <exception cref="ArgumentNullException">参数: group 为 null</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool OpenGroupAnonymous (Group group)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			return this.OpenGroupAnonymous (group.Id);
		}

		/// <summary>
		/// 关闭群匿名
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 超出范围</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool StopGroupAnonymous (long groupId)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}
			return CQP.CQ_setGroupAnonymous (this.AuthCode, groupId, false) == 0;
		}

		/// <summary>
		/// 关闭群匿名
		/// </summary>
		/// <param name="group">目标群</param>
		/// <exception cref="ArgumentNullException">参数: group 为 null</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool StopGroupAnonymous (Group group)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			return this.StopGroupAnonymous (group.Id);
		}

		/// <summary>
		/// 退出群. 慎用, 此接口需要严格授权
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 超出范围</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool ExitGroup (long groupId)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}
			return CQP.CQ_setGroupLeave (this.AuthCode, groupId, false) == 0;
		}

		/// <summary>
		/// 退出群. 慎用, 此接口需要严格授权
		/// </summary>
		/// <param name="group">目标群</param>
		/// <exception cref="ArgumentNullException">参数: group 为 null</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool ExitGroup (Group group)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			return this.ExitGroup (group.Id);
		}

		/// <summary>
		/// 解散群. 慎用, 此接口需要严格授权
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 超出范围</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool DissolutionGroup (long groupId)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}

			return CQP.CQ_setGroupLeave (this.AuthCode, groupId, true) == 0;
		}

		/// <summary>
		/// 解散群. 慎用, 此接口需要严格授权
		/// </summary>
		/// <param name="group">目标群</param>
		/// <exception cref="ArgumentNullException">参数: group 为 null</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool DissolutionGroup (Group group)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			return this.DissolutionGroup (group.Id);
		}

		/// <summary>
		/// 退出讨论组.
		/// </summary>
		/// <param name="discussId">目标讨论组</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: discussId 超出范围</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool ExitDiscuss (long discussId)
		{
			if (discussId < Discuss.MinValue)
			{
				throw new ArgumentOutOfRangeException ("discussId");
			}

			return CQP.CQ_setDiscussLeave (this.AuthCode, discussId) == 0;
		}

		/// <summary>
		/// 退出讨论组.
		/// </summary>
		/// <param name="discuss">目标讨论组</param>
		/// <exception cref="ArgumentNullException">参数: discuss 为 null</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool ExitDiscuss (Discuss discuss)
		{
			if (discuss == null)
			{
				throw new ArgumentNullException ("discuss");
			}

			return this.ExitDiscuss (discuss.Id);
		}

		/// <summary>
		/// 移除群成员
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="qqId">目标QQ</param>
		/// <param name="notRequest">不再接收加群申请. 请慎用, 默认: False</param>
		/// <exception cref="ArgumentOutOfRangeException">参数: groupId 或 qqId 超出范围</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveGroupMember (long groupId, long qqId, bool notRequest = false)
		{
			if (groupId < Group.MinValue)
			{
				throw new ArgumentOutOfRangeException ("groupId");
			}

			if (qqId < QQ.MinValue)
			{
				throw new ArgumentOutOfRangeException ("qqId");
			}

			return CQP.CQ_setGroupKick (this.AuthCode, groupId, qqId, notRequest) == 0;
		}

		/// <summary>
		/// 移除群成员
		/// </summary>
		/// <param name="group">目标群</param>
		/// <param name="qq">目标QQ</param>
		/// <param name="notRequest">不再接收加群申请. 请慎用, 默认: False</param>
		/// <exception cref="ArgumentNullException">参数: group 或 qq 为 null</exception>
		/// <returns>修改成功返回 <code>true</code>, 失败返回 <code>false</code></returns>
		public bool RemoveGroupMember (Group group, QQ qq, bool notRequest = false)
		{
			if (group == null)
			{
				throw new ArgumentNullException ("group");
			}

			if (qq == null)
			{
				throw new ArgumentNullException ("qq");
			}

			return this.RemoveGroupMember (group.Id, qq.Id, notRequest);
		}
		#endregion

		#region --请求类方法--
		/// <summary>
		/// 置好友添加请求
		/// </summary>
		/// <param name="tag">请求反馈标识</param>
		/// <param name="response">反馈类型</param>
		/// <param name="notes">备注</param>
		/// <returns>禁言成功返回 <code>true</code>, 否则返回 <code>false</code></returns>
		public bool SetFriendAddRequest (string tag, CQResponseType response, string notes = null)
		{
			if (notes == null)
			{
				notes = string.Empty;
			}
			GCHandle notesHandle = notes.GetStringGCHandle (CQApi.DefaultEncoding);
			GCHandle tagHandler = tag.GetStringGCHandle (CQApi.DefaultEncoding);
			try
			{
				return CQP.CQ_setFriendAddRequest (this.AuthCode, tagHandler.AddrOfPinnedObject (), (int)response, notesHandle.AddrOfPinnedObject ()) == 0;
			}
			finally
			{
				notesHandle.Free ();
				tagHandler.Free ();
			}
		}

		/// <summary>
		/// 置群添加请求
		/// </summary>
		/// <param name="tag">请求反馈标识</param>
		/// <param name="request">请求类型</param>
		/// <param name="response">反馈类型</param>
		/// <param name="appendMsg">备注</param>
		/// <returns>禁言成功返回 <code>true</code>, 否则返回 <code>false</code></returns>
		public bool SetGroupAddRequest (string tag, CQGroupAddRequestType request, CQResponseType response, string appendMsg)
		{
			if (appendMsg == null)
			{
				appendMsg = string.Empty;
			}
			GCHandle appendMsgHandle = appendMsg.GetStringGCHandle (CQApi.DefaultEncoding);
			GCHandle tagHandle = tag.GetStringGCHandle (CQApi.DefaultEncoding);
			try
			{
				return CQP.CQ_setGroupAddRequestV2 (this.AuthCode, tagHandle.AddrOfPinnedObject (), (int)request, (int)response, appendMsgHandle.AddrOfPinnedObject ()) == 0;
			}
			finally
			{
				appendMsgHandle.Free ();
				tagHandle.Free ();
			}
		}
		#endregion
	}
}