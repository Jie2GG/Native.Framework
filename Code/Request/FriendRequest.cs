using Native.Sdk.Cqp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using RestSharp;
using System.Net;
using Newtonsoft.Json.Linq;
using Code.Model;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;

namespace Code.Request
{
    public class FriendRequest
    {
        static readonly RestClient restClientMobile = new RestClient("http://m.qzone.com/");
        static readonly RestClient restClientPC = new RestClient("https://h5.qzone.qq.com/");
        static readonly string userAgentMobile = "Mozilla/5.0 (iPhone; CPU iPhone OS 12_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/16A366 QQ/7.8.8.420 V1_IPH_SQ_7.8.8_1_APP_A Pixel/1125 Core/WKWebView Device/Apple(iPhone X) NetType/WIFI QBWebViewType/1 WKType/1";
        static readonly string userAgentPC = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_3) AppleWebKit/604.5.6 (KHTML, like Gecko) Version/11.0.3 Safari/604.5.6";

        private int Gtk { get; set; }
        private long Login_Qq { get; set; }
        private CQLog Log { get; set; }

        public FriendRequest(CQApi api, CQLog log)
        {
            Log = log;
            Gtk = api.GetCsrfToken();
            Login_Qq = api.GetLoginQQId();
            var CookieContainer = new System.Net.CookieContainer();

            CookieContainer.SetCookies(restClientMobile.BaseUrl, api.GetCookies(restClientMobile.BaseUrl.AbsoluteUri).Replace(";", ","));
            CookieContainer.SetCookies(restClientPC.BaseUrl, api.GetCookies(restClientPC.BaseUrl.AbsoluteUri).Replace(";", ","));

            restClientMobile.UserAgent = userAgentMobile;
            restClientMobile.CookieContainer = CookieContainer;
            restClientMobile.Encoding = Encoding.GetEncoding("gb2312");
            restClientMobile.AddDefaultHeader("Content-Type", "text/html;charset=gb2312");
            restClientPC.UserAgent = userAgentPC;
            restClientPC.CookieContainer = CookieContainer;
            restClientPC.Encoding = Encoding.GetEncoding("gb2312");
            restClientPC.AddDefaultHeader("Content-Type", "text/html;charset=gb2312");
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        /// <summary>
        /// 需要开通QQ空间
        /// </summary>
        /// <returns></returns>
        public List<Friend> GetFrientListH5()
        {
            List<Friend> friends = new List<Friend>();
            JObject obj = null;

            string url = $"/proxy/domain/base.qzone.qq.com/cgi-bin/user/friend_show_qqfriends?g_tk={Gtk}&uin={Login_Qq}";
            RestRequest request = new RestRequest(url);
            var response = restClientPC.Execute(request);

            if (response?.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string Content = Encoding.GetEncoding("gb2312").GetString(response.RawBytes);

                Match rgxMatch = new Regex(@"\{(.|\s)*\}").Match(Content);

                if (rgxMatch.Success)
                {
                    obj = JObject.Parse(rgxMatch.Value, new JsonLoadSettings()
                    {
                        DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace,
                        LineInfoHandling = LineInfoHandling.Ignore
                    });

                    if (obj.ContainsKey("error"))
                    {
                        if (obj.ContainsKey("msg"))
                        {
                            Log.Warning("取好友列表", obj["msg"].ToObject<string>());
                        }
                        return null;
                    }
                    else
                    {
                        ///TODO
                    }
                }
            }
            return null;
        }

        public List<Friend> GetFrientList()
        {
            string url = $"/friend/mfriend_list?g_tk={Gtk}&res_uin={Login_Qq}&res_type=normal&format=json";

            RestRequest request = new RestRequest(url);
            var response = restClientMobile.Execute(request);

            if (response?.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JObject obj = JObject.Parse(response.Content, new JsonLoadSettings()
                {
                    DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace,
                    LineInfoHandling = LineInfoHandling.Ignore
                });

                List<Friend> friends = new List<Friend>();

                if (obj["code"].ToObject<int>() == 0)
                {
                    var data = obj["data"];
                    var gname = data["gpnames"];
                    var list = data["list"];

                    Dictionary<int, string> groupName = new Dictionary<int, string>();

                    gname.AsEnumerable().ToList().ForEach(g =>
                    {
                        groupName.Add(g["gpid"].ToObject<int>(), g["gpname"].ToObject<string>());
                    });

                    foreach (var friend in list.AsEnumerable())
                    {
                        friends.Add(new Friend
                        {
                            NickName = friend["nick"].ToObject<string>(),
                            QqId = friend["uin"].ToObject<long>(),
                            GroupIndex = friend["groupid"].ToObject<int>(),
                            Remark = friend["remark"].ToObject<string>(),
                            GroupName = groupName[friend["groupid"].ToObject<int>()],
                            IsVip = friend["isvip"].ToObject<bool>(),
                            VipLevel = friend["viplevel"].ToObject<int>(),
                        });
                    }
                    return friends;
                }
                else { Debug.WriteLine(response.Content); return null; }
            }
            else
            {
                throw new HttpListenerException();
            }
        }
    }
}
