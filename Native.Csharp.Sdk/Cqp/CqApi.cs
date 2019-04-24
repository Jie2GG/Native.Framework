using Native.Csharp.Sdk.Cqp.Core;
using Native.Csharp.Sdk.Cqp.Enum;
using Native.Csharp.Sdk.Cqp.Model;
using Native.Csharp.Sdk.Cqp.Other;
using Native.Csharp.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Native.Csharp.Sdk.Cqp
{
	public class CqApi
	{
		#region --字段--
		private int _authCode = 0;
		private string _appDirCache = null;
		#endregion

		#region --属性--
		/// <summary>
		/// 获取或设置该实例的验证码
		/// </summary>
		public int AuthCode { get { return _authCode; } set { _authCode = value; } }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 初始化一个 CqApi 类的新实例, 该实例将由酷Q授权
		/// </summary>
		/// <param name="authCode">插件验证码</param>
		public CqApi (int authCode)
		{
			this._authCode = authCode;
		}
		#endregion

		#region --CQ码--
		/// <summary>
		/// 获取酷Q "At某人" 代码
		/// </summary>
		/// <param name="qqId">QQ号, 填写 -1 为At全体成员</param>
		/// <param name="addSpacing">默认为True, At后添加空格, 可使At更规范美观. 如果不需要添加空格, 请置本参数为False</param>
		/// <returns></returns>
		public string CqCode_At (long qqId = -1, bool addSpacing = true)
		{
			return string.Format ("[CQ:at,qq={0}]{1}", (qqId == -1) ? "all" : qqId.ToString (), addSpacing ? " " : string.Empty);
		}
		/// <summary>
		/// 获取酷Q "emoji表情" 代码
		/// </summary>
		/// <param name="id">表情Id</param>
		/// <returns></returns>
		public string CqCode_Emoji (int id)
		{
			return string.Format ("[CQ:emoji,id={0}]", id);
		}
		/// <summary>
		/// 获取酷Q "表情" 代码
		/// </summary>
		/// <param name="face">表情枚举</param>
		/// <returns></returns>
		public string CqCode_Face (Face face)
		{
			return string.Format ("[CQ:face,id={0}]", (int)face);
		}
		/// <summary>
		/// 获取酷Q "窗口抖动" 代码
		/// </summary>
		/// <returns></returns>
		public string CqCode_Shake ()
		{
			return "[CQ:shake]";
		}
		/// <summary>
		/// 获取字符串的转义形式
		/// </summary>
		/// <param name="str">欲转义字符串</param>
		/// <param name="commaTrope">逗号转义, 默认: False</param>
		/// <returns></returns>
		public string CqCode_Trope (string str, bool commaTrope = false)
		{
			StringBuilder @string = new StringBuilder (str);
			@string = @string.Replace ("&", "&amp;");
			@string = @string.Replace ("[", "&#91;");
			@string = @string.Replace ("]", "&#93;");
			if (commaTrope)
			{
				@string = @string.Replace (",", "&#44;");
			}
			return @string.ToString ();
		}
		/// <summary>
		/// 获取字符串的非转义形式
		/// </summary>
		/// <param name="str">欲反转义字符串</param>
		/// <returns></returns>
		public string CqCode_UnTrope (string str)
		{
			StringBuilder @string = new StringBuilder (str);
			@string = @string.Replace ("&#91;", "[");
			@string = @string.Replace ("&#93;", "]");
			@string = @string.Replace ("&#44;", ",");
			@string = @string.Replace ("&amp;", "&");
			return @string.ToString ();
		}
		/// <summary>
		/// 获取酷Q "链接分享" 代码
		/// </summary>
		/// <param name="url">分享链接</param>
		/// <param name="title">分享的标题, 建议12字以内</param>
		/// <param name="content">分享的简介, 建议30字以内</param>
		/// <param name="imgUrl">分享的图片链接, 留空则为默认图片</param>
		/// <returns></returns>
		public string CqCode_ShareLink (string url, string title, string content, string imgUrl)
		{
			StringBuilder @string = new StringBuilder ();
			@string.AppendFormat (",url={0}", CqCode_Trope (url, true));
			if (!string.IsNullOrEmpty (title))
			{
				@string.AppendFormat (",title={0}", CqCode_Trope (title, true));
			}
			if (!string.IsNullOrEmpty (content))
			{
				@string.AppendFormat (",content={0}", CqCode_Trope (content, true));
			}
			if (!string.IsNullOrEmpty (imgUrl))
			{
				@string.AppendFormat (",image={0}", CqCode_Trope (imgUrl, true));
			}
			return string.Format ("[CQ:share{0}]", @string.ToString ());
		}
		/// <summary>
		/// 获取酷Q "名片分享" 代码
		/// </summary>
		/// <param name="cardType">名片类型, qq: 好友分享, group: 群分享</param>
		/// <param name="id">类型为qq，则为qqId；类型为group，则为groupId</param>
		/// <returns></returns>
		public string CqCode_ShareCard (string cardType, long id)
		{
			return string.Format ("[CQ:contact,type={0},id={1}]", CqCode_Trope (cardType, true), id);
		}
		/// <summary>
		/// 获取酷Q "位置分享" 代码
		/// </summary>
		/// <param name="site">地点名称, 建议12字以内</param>
		/// <param name="detail">详细地址, 建议20字以内</param>
		/// <param name="lat">维度</param>
		/// <param name="lon">经度</param>
		/// <param name="zoom">放大倍数, 默认: 15</param>
		/// <returns></returns>
		public string CqCode_ShareGPS (string site, string detail, double lat, double lon, int zoom = 15)
		{
			StringBuilder @string = new StringBuilder ();
			@string.AppendFormat (",lat={0},lon={1}", lat, lon);
			@string.AppendFormat (",zoom={0}", zoom);
			@string.AppendFormat (",title={0},content={1}", CqCode_Trope (site, true), CqCode_Trope (detail, true));
			return string.Format ("[CQ:location{0}]", @string.ToString ());
		}
		/// <summary>
		/// 获取酷Q "匿名" 代码
		/// </summary>
		/// <param name="forced">强制发送
		/// <para>默认为False 如果希望匿名失败时，将消息转为普通消息发送(而不是取消发送)，请置本参数为True。</para></param>
		/// <returns></returns>
		public string CqCode_Anonymous (bool forced = false)
		{
			return string.Format ("[CQ:anonymous{0}]", forced ? ",ignore=true" : string.Empty);
		}
		/// <summary>
		/// 获取酷Q "图片" 代码
		/// </summary>
		/// <param name="filePath">图片路径
		/// <para>将图片放在 data\image 下，并填写相对路径。如 data\image\1.jpg 则填写 1.jpg</para></param>
		/// <returns></returns>
		public string CqCode_Image (string filePath)
		{
			return string.Format ("[CQ:image,file={0}]", CqCode_Trope (filePath, true));
		}
		/// <summary>
		/// 获取酷Q "音乐" 代码
		/// </summary>
		/// <param name="id">歌曲ID</param>
		/// <param name="type">歌曲来源, 目前支持 qq/QQ音乐 163/网易云音乐 xiami/虾米音乐，默认为qq</param>
		/// <param name="newStyle">启用新样式, 目前仅支持 QQ音乐 </param>
		/// <returns></returns>
		public string CqCode_Music (long id, string type = "qq", bool newStyle = false)
		{
			return string.Format ("[CQ:music,id={0},type={1}{2}]", id, CqCode_Trope (type, true), newStyle ? "style=1" : string.Empty);
		}
		/// <summary>
		/// 获取酷Q "音乐自定义" 代码
		/// </summary>
		/// <param name="url">分享链接, 点击分享后进入的音乐页面（如歌曲介绍页）</param>
		/// <param name="musicUrl">歌曲链接, 音乐的音频链接（如mp3链接）</param>
		/// <param name="title">音乐的标题，建议12字以内</param>
		/// <param name="content">音乐的简介，建议30字以内</param>
		/// <param name="imgUrl">音乐的封面图片链接，留空则为默认图片</param>
		/// <returns></returns>
		public string CqCode_MusciDIY (string url, string musicUrl, string title = null, string content = null, string imgUrl = null)
		{
			StringBuilder @string = new StringBuilder ();
			@string.AppendFormat (",url={0}", CqCode_Trope (url, true));
			@string.AppendFormat (",audio={0}", CqCode_Trope (musicUrl, true));
			if (!string.IsNullOrEmpty (title))
			{
				@string.AppendFormat (",title={0}", CqCode_Trope (title, true));
			}
			if (!string.IsNullOrEmpty (content))
			{
				@string.AppendFormat (",content={0}", CqCode_Trope (content, true));
			}
			if (!string.IsNullOrEmpty (imgUrl))
			{
				@string.AppendFormat (",image={0}", CqCode_Trope (imgUrl, true));
			}
			return string.Format ("[CQ:music,type=custom{0}]", @string.ToString ());
		}
		/// <summary>
		/// 获取酷Q "语音" 代码
		/// </summary>
		/// <param name="filePath">语音路径
		/// <para>将语音放在 data\record 下，并填写相对路径。如 data\record\1.amr 则填写 1.amr</para></param>
		/// <returns></returns>
		public string CqCode_Record (string filePath)
		{
			return string.Format ("[CQ:record,file={0}]", CqCode_Trope (filePath, true));
		}
		#endregion

		#region --消息--
		/// <summary>
		/// 发送群消息
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="message">消息内容</param>
		public int SendGroupMessage (long groupId, string message)
		{
			return CQP.CQ_sendGroupMsg (_authCode, groupId, NativeConvert.ToStringPtr (message, Encoding.GetEncoding ("GB18030")));
		}
		/// <summary>
		/// 发送私聊消息
		/// </summary>
		/// <param name="qqId">目标QQ</param>
		/// <param name="message">消息内容</param>
		/// <returns></returns>
		public int SendPrivateMessage (long qqId, string message)
		{
			return CQP.CQ_sendPrivateMsg (_authCode, qqId, NativeConvert.ToStringPtr (message, Encoding.GetEncoding ("GB18030")));
		}
		/// <summary>
		/// 发送讨论组消息
		/// </summary>
		/// <param name="discussId">目标讨论组</param>
		/// <param name="message">消息内容</param>
		/// <returns></returns>
		public int SendDiscussMessage (long discussId, string message)
		{
			return CQP.CQ_sendDiscussMsg (_authCode, discussId, NativeConvert.ToStringPtr (message, Encoding.GetEncoding ("GB18030")));
		}
		/// <summary>
		/// 发送赞
		/// </summary>
		/// <param name="qqId">目标QQ</param>
		/// <param name="count">赞的次数，最多10次（留空为1次）</param>
		/// <returns></returns>
		public int SendPraise (long qqId, int count = 1)
		{
			return CQP.CQ_sendLikeV2 (_authCode, qqId, (count <= 0 || count > 10) ? 1 : count);
		}
		/// <summary>
		/// 接收消息中的语音(record),返回语音文件绝对路径
		/// </summary>
		/// <param name="fileName">文件名, 收到消息中的语音文件名(file)</param>
		/// <param name="formatType">应用所需的语音文件格式</param>
		/// <returns></returns>
		public string ReceiveRecord (string fileName, AudioOutFormat formatType)
		{
			//return CQP.CQ_getRecord (_authCode, fileName, formatType.ToString ());
			return CQP.CQ_getRecordV2 (_authCode, fileName, formatType.ToString ());
		}
		/// <summary>
		/// 接收消息中的图片(image),返回图片文件绝对路径
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public string ReceiveImage (string fileName)
		{
			return CQP.CQ_getImage (_authCode, fileName);
		}
		/// <summary>
		/// 撤回消息
		/// </summary>
		/// <param name="id">消息ID</param>
		/// <returns></returns>
		public int RepealMessage (long id)
		{
			return CQP.CQ_deleteMsg (_authCode, id);
		}
		#endregion

		#region --框架--
		/// <summary>
		/// 取登录QQ
		/// </summary>
		/// <returns></returns>
		public long GetLoginQQ ()
		{
			return CQP.CQ_getLoginQQ (_authCode);
		}
		/// <summary>
		/// 获取当前登录QQ的昵称
		/// </summary>
		/// <returns></returns>
		public string GetLoginNick ()
		{
			return NativeConvert.ToPtrString (CQP.CQ_getLoginNick (_authCode));
		}
		/// <summary>
		/// 取应用目录
		/// </summary>
		/// <returns></returns>
		public string GetAppDirectory ()
		{
			if (_appDirCache == null)
			{
				_appDirCache = CQP.CQ_getAppDirectory (_authCode);
			}
			return _appDirCache;
		}
		/// <summary>
		/// 获取Cookies 慎用,此接口需要严格授权
		/// </summary>
		/// <returns></returns>
		public string GetCookies ()
		{
			return CQP.CQ_getCookies (_authCode);
		}
		/// <summary>
		/// 即QQ网页用到的bkn/g_tk等 慎用,此接口需要严格授权
		/// </summary>
		/// <returns></returns>
		public int GetCsrfToken ()
		{
			return CQP.CQ_getCsrfToken (_authCode);
		}
		/// <summary>
		/// 获取QQ信息
		/// </summary>
		/// <param name="qqId">目标QQ</param>
		/// <param name="qqInfo">返回QQ信息, 若获取失败, 返回null</param>
		/// <param name="notCache">不使用缓存, 默认为"False"，通常忽略本参数，仅在必要时使用</param>
		/// <returns></returns>
		public int GetQQInfo (long qqId, out QQ qqInfo, bool notCache = false)
		{
			string result = CQP.CQ_getStrangerInfo (_authCode, qqId, notCache);
			if (string.IsNullOrEmpty (result))
			{
				qqInfo = null;
				return -1000;
			}
			UnPack unpack = new UnPack (Convert.FromBase64String (result));
			qqInfo = new QQ ();
			qqInfo.Id = unpack.GetInt64 ();
			qqInfo.Nick = unpack.GetString (Encoding.GetEncoding ("GB18030"));
			qqInfo.Sex = (Sex)unpack.GetInt32 ();
			qqInfo.Age = unpack.GetInt32 ();
			return 0;
		}
		/// <summary>
		/// 获取群成员信息
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="qqId">目标QQ</param>
		/// <param name="member">如果成功, 返回群成员信息</param>
		/// <param name="notCache">默认为 "Flase", 通常忽略本参数, 仅在必要的是否使用</param>
		/// <returns>成功返回 0, 失败返回 负数</returns>
		public int GetMemberInfo (long groupId, long qqId, out GroupMember member, bool notCache = false)
		{
			string result = CQP.CQ_getGroupMemberInfoV2 (_authCode, groupId, qqId, notCache);
			if (string.IsNullOrEmpty (result))
			{
				member = null;
				return -1000;
			}
			#region --其它_转换_文本到群成员信息--
			member = new GroupMember ();
			UnPack unpack = new UnPack (Convert.FromBase64String (result));
			member.GroupId = unpack.GetInt64 ();
			member.QQId = unpack.GetInt64 ();
			member.Nick = unpack.GetString (Encoding.GetEncoding ("GB18030"));
			member.Card = unpack.GetString (Encoding.GetEncoding ("GB18030"));
			member.Sex = (Sex)unpack.GetInt32 ();
			member.Age = unpack.GetInt32 ();
			member.Area = unpack.GetString (Encoding.GetEncoding ("GB18030"));
			member.JoiningTime = NativeConvert.FotmatUnixTime (unpack.GetInt32 ().ToString ());
			member.LastDateTime = NativeConvert.FotmatUnixTime (unpack.GetInt32 ().ToString ());
			member.Level = unpack.GetString (Encoding.GetEncoding ("GB18030"));
			member.PermitType = (PermitType)unpack.GetInt32 ();
			member.BadRecord = unpack.GetInt32 () == 1;
			member.SpecialTitle = unpack.GetString (Encoding.GetEncoding ("GB18030"));
			member.SpecialTitleDurationTime = NativeConvert.FotmatUnixTime (unpack.GetInt32 ().ToString ());
			member.CanModifiedCard = unpack.GetInt32 () == 1;
			#endregion
			return 0;

		}
		/// <summary>
		/// 获取群成员列表
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="memberInfos">如果成功，返回群成员列表</param>
		/// <returns>成功返回 0, 失败返回 负数</returns>
		public int GetMemberList (long groupId, out List<GroupMember> memberInfos)
		{
			string result = CQP.CQ_getGroupMemberList (_authCode, groupId);
			if (string.IsNullOrEmpty (result))
			{
				memberInfos = null;
				return -1000;
			}
			#region --其他_转换_文本到群成员列表信息a--
			UnPack unpack = new UnPack (Convert.FromBase64String (result));
			memberInfos = new List<GroupMember> ();
			for (int i = 0, len = unpack.GetInt32 (); i < len; i++)
			{
				if (unpack.OverLength <= 0)
				{
					memberInfos = null;
					return -1000;
				}
				#region --其它_转换_ansihex到群成员信息--
				UnPack temp = new UnPack (unpack.GetToken ()); //解析群成员信息
				GroupMember member = new GroupMember ();
				member.GroupId = temp.GetInt64 ();
				member.QQId = temp.GetInt64 ();
				member.Nick = temp.GetString (Encoding.GetEncoding ("GB18030"));
				member.Card = temp.GetString (Encoding.GetEncoding ("GB18030"));
				member.Sex = (Sex)temp.GetInt32 ();
				member.Age = temp.GetInt32 ();
				member.Area = temp.GetString (Encoding.GetEncoding ("GB18030"));
				member.JoiningTime = NativeConvert.FotmatUnixTime (temp.GetInt32 ().ToString ());
				member.LastDateTime = NativeConvert.FotmatUnixTime (temp.GetInt32 ().ToString ());
				member.Level = temp.GetString (Encoding.GetEncoding ("GB18030"));
				member.PermitType = (PermitType)temp.GetInt32 ();
				member.BadRecord = temp.GetInt32 () == 1;
				member.SpecialTitle = temp.GetString (Encoding.GetEncoding ("GB18030"));
				member.SpecialTitleDurationTime = NativeConvert.FotmatUnixTime (temp.GetInt32 ().ToString ());
				member.CanModifiedCard = temp.GetInt32 () == 1;
				#endregion
				memberInfos.Add (member);
			}
			#endregion
			return 0;
		}
		/// <summary>
		/// 获取群列表
		/// </summary>
		/// <param name="groups"></param>
		/// <returns></returns>
		public int GetGroupList (out List<Group> groups)
		{
			string result = CQP.CQ_getGroupList (_authCode);
			if (string.IsNullOrEmpty (result))
			{
				groups = null;
				return -1000;
			}
			groups = new List<Group> ();
			#region --其他_转换_文本到群列表信息a--
			UnPack unpack = new UnPack (Convert.FromBase64String (result));
			for (int i = 0, len = unpack.GetInt32 (); i < len; i++)
			{
				if (unpack.OverLength <= 0)
				{
					groups = null;
					return -1000;
				}
				#region --其他_转换_ansihex到群信息--
				UnPack temp = new UnPack (unpack.GetToken ());
				Group group = new Group ();
				group.Id = temp.GetInt64 ();
				group.Name = temp.GetString (Encoding.GetEncoding ("GB18030"));
				groups.Add (group);
				#endregion
			}
			#endregion
			return 0;
		}
		/// <summary>
		/// 获取发送语音支持
		/// </summary>
		/// <returns></returns>
		public bool GetSendRecordSupport ()
		{
			return CQP.CQ_canSendRecord (_authCode) > 0;
		}
		/// <summary>
		/// 获取发送图片支持
		/// </summary>
		/// <returns></returns>
		public bool GetSendImageSupport ()
		{
			return CQP.CQ_canSendImage (_authCode) > 0;
		}
		#endregion

		#region --日志--
		/// <summary>
		/// 添加日志
		/// </summary>
		/// <param name="level">级别</param>
		/// <param name="type">类型</param>
		/// <param name="content">内容</param>
		/// <returns></returns>
		public int AddLoger (LogerLevel level, string type, string content)
		{
			return CQP.CQ_addLog (_authCode, (int)level, type, NativeConvert.ToStringPtr (content, Encoding.GetEncoding ("GB18030")));
		}
		/// <summary>
		/// 添加致命错误提示
		/// </summary>
		/// <param name="message">错误信息</param>
		/// <returns></returns>
		public int AddFatalError (string message)
		{
			return CQP.CQ_setFatal (_authCode, NativeConvert.ToStringPtr (message, Encoding.GetEncoding ("GB18030")));
		}
		#endregion

		#region --请求--
		/// <summary>
		/// 置好友添加请求
		/// </summary>
		/// <param name="tag">请求反馈标识</param>
		/// <param name="response">反馈类型</param>
		/// <param name="appendMsg">备注</param>
		/// <returns></returns>
		public int SetFriendAddRequest (string tag, ResponseType response, string appendMsg)
		{
			if (appendMsg == null)
			{
				appendMsg = string.Empty;
			}
			return CQP.CQ_setFriendAddRequest (_authCode, tag, (int)response, NativeConvert.ToStringPtr (appendMsg, Encoding.GetEncoding ("GB18030")));
		}
		/// <summary>
		/// 置群添加请求
		/// </summary>
		/// <param name="tag">请求反馈标识</param>
		/// <param name="request">请求类型</param>
		/// <param name="response">反馈类型</param>
		/// <param name="appendMsg">备注</param>
		/// <returns></returns>
		public int SetGroupAddRequest (string tag, RequestType request, ResponseType response, string appendMsg)
		{
			if (appendMsg == null)
			{
				appendMsg = string.Empty;
			}
			return CQP.CQ_setGroupAddRequestV2 (_authCode, tag, (int)request, (int)response, NativeConvert.ToStringPtr (appendMsg, Encoding.GetEncoding ("GB18030")));
		}
		#endregion

		#region --管理--
		/// <summary>
		/// 置匿名群员禁言
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="anonymous">匿名参数</param>
		/// <param name="time">禁言时间, 单位: 秒, 不支持解禁</param>
		/// <returns></returns>
		public int SetGroupAnonymousBanSpeak (long groupId, string anonymous, TimeSpan time)
		{
			if (time.TotalSeconds <= 0)
			{
				time = TimeSpan.Zero;
			}

			return CQP.CQ_setGroupAnonymousBan (_authCode, groupId, NativeConvert.ToStringPtr (anonymous, Encoding.GetEncoding ("GB18030")), (long)time.TotalSeconds);
		}
		/// <summary>
		/// 置群员禁言
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="qqId">目标QQ</param>
		/// <param name="time">禁言的时间，单位为秒。如果要解禁，请给TimeSpan.Zero</param>
		/// <returns></returns>
		public int SetGroupBanSpeak (long groupId, long qqId, TimeSpan time)
		{
			if (time.Ticks < 0)
			{
				time = TimeSpan.Zero;
			}
			return CQP.CQ_setGroupBan (_authCode, groupId, qqId, (long)time.TotalSeconds);
		}
		/// <summary>
		/// 置全群禁言
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="isOpen">是否开启</param>
		/// <returns></returns>
		public int SetGroupWholeBanSpeak (long groupId, bool isOpen)
		{
			return CQP.CQ_setGroupWholeBan (_authCode, groupId, isOpen);
		}
		/// <summary>
		/// 置群成员名片
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="qqId">目标QQ</param>
		/// <param name="newNick">新昵称</param>
		/// <returns></returns>
		public int SetGroupMemberNewCard (long groupId, long qqId, string newNick)
		{
			return CQP.CQ_setGroupCard (_authCode, groupId, qqId, NativeConvert.ToStringPtr (newNick, Encoding.GetEncoding ("GB18030")));
		}
		/// <summary>
		/// 置群成员专属头衔
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="qqId">目标QQ</param>
		/// <param name="specialTitle">如果要删除，这里填空</param>
		/// <param name="time">专属头衔有效期，单位为秒。如果永久有效，time填写负数</param>
		/// <returns></returns>
		public int SetGroupSpecialTitle (long groupId, long qqId, string specialTitle, TimeSpan time)
		{
			if (time.Ticks < 0)
			{
				time = new TimeSpan (-10000000);     //-1秒
			}
			return CQP.CQ_setGroupSpecialTitle (_authCode, groupId, qqId, NativeConvert.ToStringPtr (specialTitle, Encoding.GetEncoding ("GB18030")), (long)time.TotalSeconds);
		}
		/// <summary>
		/// 置群管理员
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="qqId">目标QQ</param>
		/// <param name="isCalcel">True: 设置管理员, False: 取消管理员</param>
		/// <returns></returns>
		public int SetGroupManager (long groupId, long qqId, bool isCalcel)
		{
			return CQP.CQ_setGroupAdmin (_authCode, groupId, qqId, isCalcel);
		}
		/// <summary>
		/// 置群匿名设置
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="isOpen">是否打开</param>
		/// <returns></returns>
		public int SetAnonymousStatus (long groupId, bool isOpen)
		{
			return CQP.CQ_setGroupAnonymous (_authCode, groupId, isOpen);
		}
		/// <summary>
		/// 置群退出 慎用,此接口需要严格授权
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="dissolve">默认为False, True: 解散本群(群主) False: 退出本群(管理、群成员)</param>
		/// <returns></returns>
		public int SetGroupExit (long groupId, bool dissolve = false)
		{
			return CQP.CQ_setGroupLeave (_authCode, groupId, dissolve);
		}
		/// <summary>
		/// 置群员移除
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="qqId">目标QQ</param>
		/// <param name="notAccept">如果为True，则“不再接收此人加群申请”，请慎用。留空为False</param>
		/// <returns></returns>
		public int SetGroupMemberRemove (long groupId, long qqId, bool notAccept = false)
		{
			return CQP.CQ_setGroupKick (_authCode, groupId, qqId, notAccept);
		}
		/// <summary>
		/// 置讨论组退出
		/// </summary>
		/// <param name="discussId">目标讨论组</param>
		/// <returns></returns>
		public int SetDiscussExit (long discussId)
		{
			return CQP.CQ_setDiscussLeave (_authCode, discussId);
		}
		#endregion

		#region --其它--
		/// <summary>
		/// 设置App验证码
		/// </summary>
		/// <param name="authCode"></param>
		[Obsolete ("更改了该类的构造方式, 此方法不建议再使用")]
		public void SetAuthCode (int authCode)
		{
			_authCode = authCode;
		}
		/// <summary>
		/// 获取App验证码
		/// </summary>
		/// <returns></returns>
		[Obsolete ("更改了该类的构造方式, 此方法不建议再使用")]
		public int GetAuthCode ()
		{
			return _authCode;
		}
		/// <summary>
		/// 获取匿名信息
		/// </summary>
		/// <param name="source">匿名参数</param>
		/// <returns></returns>
		public GroupAnonymous GetAnonymous (string source)
		{
			UnPack unPack = new UnPack (Convert.FromBase64String (source));
			GroupAnonymous anonymous = new GroupAnonymous ();
			anonymous.Id = unPack.GetInt64 ();
			anonymous.CodeName = unPack.GetString (Encoding.GetEncoding ("GB18030"));
			anonymous.Token = unPack.GetToken ();
			return anonymous;
		}
		/// <summary>
		/// 获取群文件
		/// </summary>
		/// <param name="source">群文件参数</param>
		/// <returns></returns>
		public GroupFile GetFile (string source)
		{
			UnPack unPack = new UnPack (Convert.FromBase64String (source));
			GroupFile file = new GroupFile ();
			file.Id = unPack.GetString (Encoding.GetEncoding ("GB18030"));
			file.Name = unPack.GetString (Encoding.GetEncoding ("GB18030"));
			file.Size = unPack.GetInt64 ();
			file.Busid = Convert.ToInt32 (unPack.GetInt64 ());
			return file;
		}
		/// <summary>
		/// 编码悬浮窗数据置文本
		/// </summary>
		/// <param name="floatWindow"></param>
		/// <returns></returns>
		public string FormatStringFloatWindow (FloatWindow floatWindow)
		{
			Pack pack = new Pack ();
			pack.SetLenString (floatWindow.Data);
			pack.SetLenString (floatWindow.Unit);
			pack.SetInt32 ((int)floatWindow.Color);
			return Convert.ToBase64String (pack.GetAll ());
		}
		#endregion
	}
}
