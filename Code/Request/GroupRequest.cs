using Code.Model;
using Native.Sdk.Cqp;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Request
{
    public class GroupRequest
    {
        static readonly RestClient restClient = new RestClient("https://web.qun.qq.com");
        static readonly string userAgentPC = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_3) AppleWebKit/604.5.6 (KHTML, like Gecko) Version/11.0.3 Safari/604.5.6";

        private CQApi Api { get; set; }
        private CQLog Log { get; set; }

        public GroupRequest(CQApi api, CQLog log)
        {
            Api = api;
            Log = log;
            var CookieContainer = new System.Net.CookieContainer();
            CookieContainer.SetCookies(restClient.BaseUrl, api.GetCookies(restClient.BaseUrl.AbsoluteUri).Replace(";", ","));
            restClient.CookieContainer = CookieContainer;
            restClient.UserAgent = userAgentPC;
        }

        public List<Notice> GetGroupNotice(long groupId)
        {
            JObject obj = null;
            List<Notice> notice = new List<Notice>();
            string urlParams = $"bkn={Api.GetCsrfToken()}&qid={groupId}";
            string url = $"/cgi-bin/announce/get_t_list?{urlParams}&ft=23&s=-1&n=10&ni=1&i=1";
            RestRequest request = new RestRequest(url);
            var response = restClient.Execute(request);

            if (response?.StatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    obj = JObject.Parse(response.Content, new JsonLoadSettings()
                    {
                        DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Ignore,
                        LineInfoHandling = LineInfoHandling.Load
                    });

                    if (obj.ContainsKey("ec") && obj["ec"].ToObject<int>() == 0)
                    {
                        if (obj.ContainsKey("inst"))
                        {
                            foreach (var d in obj["inst"].AsEnumerable())
                            {
                                var n = new Notice()
                                {
                                    IsForNewMember = true,
                                    PublicQqId = d["u"].ToObject<long>(),
                                    Text = d["msg"]["text"].ToObject<string>(),
                                    Content = d["msg"]["text_face"].ToObject<string>(),
                                    NoticId = d["fid"].ToObject<string>(),
                                    Type = d["type"].ToObject<int>(),
                                    PublicDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(d["pubt"].ToObject<long>()).ToLocalTime(),
                                    ReadNum = d["read_num"].ToObject<long>()
                                };

                                if (d["msg"].SelectToken("pics") != null)
                                {
                                    n.PictureId = new List<string>();
                                    foreach (var i in d["msg"]["pics"].AsEnumerable())
                                    {
                                        n.PictureId.Add(i.SelectToken("id")?.ToObject<string>());
                                    }
                                }
                                notice.Add(n);
                            }
                        }
                        if (obj.ContainsKey("feeds"))
                        {
                            foreach (var d in obj["feeds"].AsEnumerable())
                            {
                                var n = new Notice()
                                {
                                    IsForNewMember = false,
                                    PublicQqId = d["u"].ToObject<long>(),
                                    Text = d["msg"]["text"].ToObject<string>(),
                                    Content = d["msg"]["text_face"].ToObject<string>(),
                                    NoticId = d["fid"].ToObject<string>(),
                                    Type = d["type"].ToObject<int>(),
                                    PublicDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(d["pubt"].ToObject<long>()).ToLocalTime(),
                                    ReadNum = d["read_num"].ToObject<long>()
                                };

                                if (d["msg"].SelectToken("pics")!= null)
                                {
                                    n.PictureId = new List<string>();
                                    foreach (var i in d["msg"]["pics"].AsEnumerable())
                                    {
                                        n.PictureId.Add(i.SelectToken("id")?.ToObject<string>());
                                    }
                                }
                                notice.Add(n);
                            }
                        }
                        return notice;
                    }
                }
                catch { }
            }

            return null;
        }
    }
}
