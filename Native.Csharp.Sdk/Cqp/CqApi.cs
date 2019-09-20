using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Native.Csharp.Sdk.Cqp.Core;
using Native.Csharp.Sdk.Cqp.Enum;
using Native.Csharp.Sdk.Cqp.Model;
using Native.Csharp.Sdk.Cqp.Other;

namespace Native.Csharp.Sdk.Cqp
{
    /// <summary>
    /// 酷Q Api封装类
    /// </summary>
    public class CqApi
    {
        #region --字段--
        private int _authCode = 0;
        private string _appDirCache = null;
        private Encoding _defaultEncoding = null;
        private Regex _cookieRegex = null;
        #endregion

        #region --属性--
        /// <summary>
        /// 获取或设置该实例的验证码
        /// </summary>
        public int AuthCode { get { return _authCode; } set { _authCode = value; } }
        #endregion

        #region --构造函数--
        /// <summary>
        /// 初始化一个 <see cref="CqApi"/> 类的新实例, 该实例将由 <code>Initialize (int)</code> 函数授权
        /// </summary>
        /// <param name="authCode">插件验证码</param>
        public CqApi (int authCode)
        {
            this._authCode = authCode;
            this._defaultEncoding = Encoding.GetEncoding ("GB18030");
            this._cookieRegex = new Regex ("(.*?)=(.*?)(?:;|$)", RegexOptions.Compiled);
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
        /// <exception cref="ArgumentException">当参数无效时引发异常</exception>
        /// <returns></returns>
        [Obsolete ("本方法已经过时, 且不再使用. 请使用 CqCode_Music 的第二个重载方法")]
        public string CqCode_Music (long id, string type = "qq", bool newStyle = false)
        {
            MusicType musicType;
            if (type.CompareTo (MusicType.Tencent.GetDescription ()) == 0)
            {
                musicType = MusicType.Tencent;
            }
            else if (type.CompareTo (MusicType.Netease.GetDescription ()) == 0)
            {
                musicType = MusicType.Netease;
            }
            else if (type.CompareTo (MusicType.XiaMi.GetDescription ()) == 0)
            {
                musicType = MusicType.XiaMi;
            }
            else
            {
                throw new ArgumentException ("参数: type 无效");
            }
            return CqCode_Music (id, musicType, newStyle ? MusicStyle.BigCard : MusicStyle.Old);
        }
        /// <summary>
        /// 获取酷Q "音乐" 代码
        /// </summary>
        /// <param name="id">歌曲ID</param>
        /// <param name="type">歌曲来源</param>
        /// <param name="style">分享样式</param>
        /// <returns>返回可发送的 CQ码</returns>
        public string CqCode_Music (long id, MusicType type = MusicType.Tencent, MusicStyle style = MusicStyle.Old)
        {
            return string.Format ("[CQ:music,id={0},type=]", id, type.GetDescription (), (int)style);
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
        /// <returns>失败返回负值, 成功返回消息 Id</returns>
        public int SendGroupMessage (long groupId, string message)
        {
            GCHandle handle = message.GetStringGCHandle (_defaultEncoding);
            int msgId = CQP.CQ_sendGroupMsg (_authCode, groupId, handle.AddrOfPinnedObject ());
            handle.Free ();
            return msgId;
        }

        /// <summary>
        /// 发送私聊消息
        /// </summary>
        /// <param name="qqId">目标QQ</param>
        /// <param name="message">消息内容</param>
        /// <returns>失败返回负值, 成功返回消息 Id</returns>
        public int SendPrivateMessage (long qqId, string message)
        {
            GCHandle handle = message.GetStringGCHandle (_defaultEncoding);
            int msgId = CQP.CQ_sendPrivateMsg (_authCode, qqId, handle.AddrOfPinnedObject ());
            handle.Free ();
            return msgId;
        }

        /// <summary>
        /// 发送讨论组消息
        /// </summary>
        /// <param name="discussId">目标讨论组</param>
        /// <param name="message">消息内容</param>
        /// <returns>失败返回负值, 成功返回消息 Id</returns>
        public int SendDiscussMessage (long discussId, string message)
        {
            GCHandle handle = message.GetStringGCHandle (_defaultEncoding);
            int msgid = CQP.CQ_sendDiscussMsg (_authCode, discussId, handle.AddrOfPinnedObject ());
            handle.Free ();
            return msgid;
        }

        /// <summary>
        /// 发送赞
        /// </summary>
        /// <param name="qqId">目标QQ</param>
        /// <param name="count">赞的次数，最多10次（留空为1次）</param>
        /// <returns></returns>
        public int SendPraise (long qqId, int count = 1)
        {
            if (count < 1)
            {
                count = 1;
            }
            if (count > 10)
            {
                count = 10;
            }
            return CQP.CQ_sendLikeV2 (_authCode, qqId, count);
        }

        /// <summary>
        /// 接收消息中的语音 (含有CQ码 "record" 的消息)
        /// </summary>
        /// <param name="fileName">文件名, 收到消息中的语音文件名(file)</param>
        /// <param name="formatType">应用所需的语音文件格式</param>
        /// <returns>返回语音文件绝对路径</returns>
        public string ReceiveRecord (string fileName, AudioOutFormat formatType)
        {
            return CQP.CQ_getRecordV2 (_authCode, fileName, formatType.GetDescription ());
        }

        /// <summary>
        /// 接收消息中的图片 (含有CQ码 "image" 的消息)
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>返回图片文件绝对路径</returns>
        public string ReceiveImage (string fileName)
        {
            return CQP.CQ_getImage (_authCode, fileName);
        }

        /// <summary>
        /// 撤回消息
        /// </summary>
        /// <param name="id">消息ID</param>
        /// <returns></returns>
        public int RepealMessage (int id)
        {
            return CQP.CQ_deleteMsg (_authCode, id);
        }
        #endregion

        #region --框架--
        /// <summary>
        /// 取登录QQ
        /// </summary>
        /// <returns>返回整数</returns>
        public long GetLoginQQ ()
        {
            return CQP.CQ_getLoginQQ (_authCode);
        }

        /// <summary>
        /// 获取当前登录QQ的昵称
        /// </summary>
        /// <returns>返回 GB108030 编码的字符串</returns>
        public string GetLoginNick ()
        {
            return CQP.CQ_getLoginNick (_authCode).ToString (_defaultEncoding);
        }

        /// <summary>
        /// 取应用目录
        /// </summary>
        /// <returns>返回本地路径字符串</returns>
        public string GetAppDirectory ()
        {
            if (_appDirCache == null)
            {
                _appDirCache = CQP.CQ_getAppDirectory (_authCode);
            }
            return _appDirCache;
        }

        /// <summary>
        /// 获取 Cookies 慎用,此接口需要严格授权
        /// </summary>
        /// <returns>返回 Cookies 字符串</returns>
        [Obsolete ("此方法已失效, 请使用 GetCookies 的第二个重载. 此方法将永远抛出异常")]
        public string GetCookies ()
        {
            return CQP.CQ_getCookies (_authCode);
        }

        /// <summary>
        /// 获取 Cookies 慎用,此接口需要严格授权
        /// </summary>
        /// <param name="domain">目标域名, 如 api.example.com</param>
        /// <returns>返回 Cookies 字符串</returns>
        public string GetCookies (string domain)
        {
            return CQP.CQ_getCookiesV2 (_authCode, domain);
        }

        /// <summary>
        /// 获取 Cookies 慎用, 此接口需要严格授权
        /// </summary>
        /// <param name="domain">目标域名, 如 api.example.com</param>
        /// <returns>返回 <see cref="CookieCollection"/> 对象</returns>
        public CookieCollection GetCookieCollection (string domain)
        {
            /*
             * uin=o2184656498;
               skey= MVPSrhTvmh;
               vkey=GC%2FgRGC875U%2Boa09uLp6xdkLMlH0wfbz82373.6720201%3D%3D
             */
            CookieCollection collection = new CookieCollection ();
            MatchCollection matchCollection = _cookieRegex.Matches (GetCookies (domain));    // 根据 Cookies 规则匹配键值
            foreach (Match item in matchCollection)
            {
                collection.Add (new Cookie (item.Groups[1].Value, item.Groups[2].Value));   // 转换为 Cookie 对象
            }
            return collection;
        }

        /// <summary>
        /// 即QQ网页用到的bkn/g_tk等 慎用,此接口需要严格授权
        /// </summary>
        /// <returns>返回 bkn/g_tk 字符串</returns>
        public int GetCsrfToken ()
        {
            return CQP.CQ_getCsrfToken (_authCode);
        }

        /// <summary>
        /// 获取QQ信息
        /// </summary>
        /// <param name="qqId">目标QQ</param>
        /// <param name="notCache">不使用缓存, 默认为"False"，通常忽略本参数，仅在必要时使用</param>
        /// <returns>获取成功返回 <see cref="QQInfo"/>, 失败返回 null</returns>
        public QQInfo GetQQInfo (long qqId, bool notCache = false)
        {
            string result = CQP.CQ_getStrangerInfo (_authCode, qqId, notCache).ToString (Encoding.ASCII);
            if (string.IsNullOrEmpty (result))
            {
                return null;
            }
            using (BinaryReader binary = new BinaryReader (new MemoryStream (Convert.FromBase64String (result))))
            {
                QQInfo qqInfo = new QQInfo ();
                qqInfo.Id = binary.ReadInt64_Ex ();
                qqInfo.Nick = binary.ReadString_Ex (_defaultEncoding);
                qqInfo.Sex = (Sex)binary.ReadInt32_Ex ();
                qqInfo.Age = binary.ReadInt32_Ex ();
                return qqInfo;
            }
        }

        /// <summary>
        /// 获取群成员信息
        /// </summary>
        /// <param name="groupId">目标群</param>
        /// <param name="qqId">目标QQ</param>
        /// <param name="notCache">默认为 "Flase", 通常忽略本参数, 仅在必要的是否使用</param>
        /// <returns>获取成功返回 <see cref="GroupMember"/>, 失败返回 null</returns>
        public GroupMember GetMemberInfo (long groupId, long qqId, bool notCache = false)
        {
            string result = CQP.CQ_getGroupMemberInfoV2 (_authCode, groupId, qqId, notCache).ToString (Encoding.ASCII);
            if (string.IsNullOrEmpty (result))
            {
                return null;
            }
            #region --其它_转换_文本到群成员信息--
            using (BinaryReader binary = new BinaryReader (new MemoryStream (Convert.FromBase64String (result))))
            {
                GroupMember member = new GroupMember ();
                member.GroupId = binary.ReadInt64_Ex ();
                member.QQId = binary.ReadInt64_Ex ();
                member.Nick = binary.ReadString_Ex (_defaultEncoding);
                member.Card = binary.ReadString_Ex (_defaultEncoding);
                member.Sex = (Sex)binary.ReadInt32_Ex ();
                member.Age = binary.ReadInt32_Ex ();
                member.Area = binary.ReadString_Ex (_defaultEncoding);
                member.JoiningTime = binary.ReadInt32_Ex ().ToDateTime ();
                member.LastDateTime = binary.ReadInt32_Ex ().ToDateTime ();
                member.Level = binary.ReadString_Ex (_defaultEncoding);
                member.PermitType = (PermitType)binary.ReadInt32_Ex ();
                member.BadRecord = binary.ReadInt32_Ex () == 1;
                member.SpecialTitle = binary.ReadString_Ex (_defaultEncoding);
                member.SpecialTitleDurationTime = binary.ReadInt32_Ex ().ToDateTime ();
                member.CanModifiedCard = binary.ReadInt32_Ex () == 1;
                return member;
            }
            #endregion
        }

        /// <summary>
        /// 获取群成员列表
        /// </summary>
        /// <param name="groupId">目标群</param>
        /// <returns>获取成功返回 <see cref="List{GroupMember}"/>, 失败返回 null</returns>
        public List<GroupMember> GetMemberList (long groupId)
        {
            string result = CQP.CQ_getGroupMemberList (_authCode, groupId).ToString (Encoding.ASCII);
            if (string.IsNullOrEmpty (result))
            {
                return null;
            }
            #region --其他_转换_文本到群成员列表信息a--
            using (BinaryReader binary = new BinaryReader (new MemoryStream (Convert.FromBase64String (result))))
            {
                List<GroupMember> memberInfos = new List<GroupMember> ();
                for (int i = 0, len = binary.ReadInt32_Ex (); i < len; i++)
                {
                    if (binary.Length () <= 0)
                    {
                        return null;
                    }
                    #region --其它_转换_ansihex到群成员信息--
                    using (BinaryReader tempBinary = new BinaryReader (new MemoryStream (binary.ReadToken_Ex ()))) //解析群成员信息
                    {
                        GroupMember member = new GroupMember ();
                        member.GroupId = tempBinary.ReadInt64_Ex ();
                        member.QQId = tempBinary.ReadInt64_Ex ();
                        member.Nick = tempBinary.ReadString_Ex (_defaultEncoding);
                        member.Card = tempBinary.ReadString_Ex (_defaultEncoding);
                        member.Sex = (Sex)tempBinary.ReadInt32_Ex ();
                        member.Age = tempBinary.ReadInt32_Ex ();
                        member.Area = tempBinary.ReadString_Ex (_defaultEncoding);
                        member.JoiningTime = tempBinary.ReadInt32_Ex ().ToDateTime ();
                        member.LastDateTime = tempBinary.ReadInt32_Ex ().ToDateTime ();
                        member.Level = tempBinary.ReadString_Ex (_defaultEncoding);
                        member.PermitType = (PermitType)tempBinary.ReadInt32_Ex ();
                        member.BadRecord = tempBinary.ReadInt32_Ex () == 1;
                        member.SpecialTitle = tempBinary.ReadString_Ex (_defaultEncoding);
                        member.SpecialTitleDurationTime = tempBinary.ReadInt32_Ex ().ToDateTime ();
                        member.CanModifiedCard = tempBinary.ReadInt32_Ex () == 1;
                        memberInfos.Add (member);
                    }
                    #endregion
                }
                return memberInfos;
            }
            #endregion
        }

        /// <summary>
        /// 获取群列表
        /// </summary>
        /// <returns>获取成功返回 <see cref="List{Group}"/>, 失败返回 null</returns>
        public List<Model.Group> GetGroupList ()
        {
            string result = CQP.CQ_getGroupList (_authCode).ToString (Encoding.ASCII);
            if (string.IsNullOrEmpty (result))
            {
                return null;
            }
            List<Model.Group> groups = new List<Model.Group> ();
            #region --其他_转换_文本到群列表信息a--
            using (BinaryReader binary = new BinaryReader (new MemoryStream (Convert.FromBase64String (result))))
            {
                for (int i = 0, len = binary.ReadInt32_Ex (); i < len; i++)
                {
                    if (binary.Length () <= 0)
                    {
                        return null;
                    }
                    #region --其他_转换_ansihex到群信息--
                    using (BinaryReader tempBinary = new BinaryReader (new MemoryStream (binary.ReadToken_Ex ())))
                    {
                        Model.Group group = new Model.Group ();
                        group.Id = tempBinary.ReadInt64_Ex ();
                        group.Name = tempBinary.ReadString_Ex (_defaultEncoding);
                        groups.Add (group);
                    }
                    #endregion
                }
                return groups;
            }
            #endregion
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
            GCHandle handle = content.GetStringGCHandle (_defaultEncoding);
            int result = CQP.CQ_addLog (_authCode, (int)level, type, handle.AddrOfPinnedObject ());
            handle.Free ();
            return result;

        }

        /// <summary>
        /// 添加致命错误提示
        /// </summary>
        /// <param name="message">错误信息</param>
        /// <returns></returns>
        public int AddFatalError (string message)
        {
            return CQP.CQ_setFatal (_authCode, message);
        }
        #endregion

        #region --请求--
        /// <summary>
        /// 置好友添加请求
        /// </summary>
        /// <param name="tag">请求反馈标识</param>
        /// <param name="response">反馈类型</param>
        /// <param name="notes">备注</param>
        /// <returns></returns>
        public int SetFriendAddRequest (string tag, ResponseType response, string notes = null)
        {
            if (notes == null)
            {
                notes = string.Empty;
            }
            GCHandle handle = notes.GetStringGCHandle (_defaultEncoding);
            int result = CQP.CQ_setFriendAddRequest (_authCode, tag, (int)response, handle.AddrOfPinnedObject ());
            handle.Free ();
            return result;
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
            GCHandle handle = appendMsg.GetStringGCHandle (_defaultEncoding);
            int result = CQP.CQ_setGroupAddRequestV2 (_authCode, tag, (int)request, (int)response, handle.AddrOfPinnedObject ());
            handle.Free ();
            return result;
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
            GCHandle handle = anonymous.GetStringGCHandle (_defaultEncoding);
            int result = CQP.CQ_setGroupAnonymousBan (_authCode, groupId, handle.AddrOfPinnedObject (), (long)time.TotalSeconds);
            handle.Free ();
            return result;
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
            GCHandle handle = newNick.GetStringGCHandle (_defaultEncoding);
            int result = CQP.CQ_setGroupCard (_authCode, groupId, qqId, handle.AddrOfPinnedObject ());
            handle.Free ();
            return result;
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
            GCHandle handle = specialTitle.GetStringGCHandle (_defaultEncoding);
            int result = CQP.CQ_setGroupSpecialTitle (_authCode, groupId, qqId, handle.AddrOfPinnedObject (), (long)time.TotalSeconds);
            handle.Free ();
            return result;
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
            BinaryReader binary = new BinaryReader (new MemoryStream (Convert.FromBase64String (source)));
            GroupAnonymous anonymous = new GroupAnonymous ();
            anonymous.Id = binary.ReadInt64_Ex ();
            anonymous.CodeName = binary.ReadString_Ex ();
            anonymous.Token = binary.ReadToken_Ex ();
            return anonymous;
        }

        /// <summary>
        /// 获取群文件
        /// </summary>
        /// <param name="source">群文件参数</param>
        /// <returns></returns>
        public GroupFile GetFile (string source)
        {
            BinaryReader binary = new BinaryReader (new MemoryStream (Convert.FromBase64String (source)));
            GroupFile file = new GroupFile ();
            file.Id = binary.ReadString_Ex (_defaultEncoding);      // 参照官方SDK, 编码为 ASCII
            file.Name = binary.ReadString_Ex (_defaultEncoding);    // 参照官方SDK, 编码为 ASCII
            file.Size = binary.ReadInt64_Ex ();
            file.Busid = Convert.ToInt32 (binary.ReadInt64_Ex ());
            return file;
        }

        /// <summary>
        /// 编码悬浮窗数据置文本
        /// </summary>
        /// <param name="floatWindow"></param>
        /// <returns></returns>
        public string FormatStringFloatWindow (FloatWindow floatWindow)
        {
            BinaryWriter binary = new BinaryWriter (new MemoryStream ());
            binary.Write_Ex (floatWindow.Data);
            binary.Write_Ex (floatWindow.Unit);
            binary.Write_Ex ((int)floatWindow.Color);

            return Convert.ToBase64String (binary.ToArray ());
        }
        #endregion
    }
}
