using Code.Model;
using Native.Sdk.Cqp;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Code.Request
{
    public class VipInfo
    {
        static readonly RestClient restClientMobile = new RestClient("https://h5.vip.qq.com");
        static readonly string userAgentMobile = "Mozilla/5.0 (iPhone; CPU iPhone OS 12_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/16A5288q QQ/6.5.5.0 TIM/2.2.5.401 V1_IPH_SQ_6.5.5_1_TIM_D Pixel/750 Core/UIWebView Device/Apple(iPhone 6s) NetType/WIFI";

        private int Gtk { get; set; }
        private long Login_Qq { get; set; }
        private CQLog Log { get; set; }

        public VipInfo(CQApi api, CQLog log)
        {
            Log = log;
            Gtk = api.GetCsrfToken();
            Login_Qq = api.GetLoginQQId();
            var CookieContainer = new System.Net.CookieContainer();

            CookieContainer.SetCookies(restClientMobile.BaseUrl, api.GetCookies(restClientMobile.BaseUrl.AbsoluteUri).Replace(";", ","));

            restClientMobile.UserAgent = userAgentMobile;
            restClientMobile.CookieContainer = CookieContainer;
            restClientMobile.Encoding = Encoding.GetEncoding("gb2312");
            restClientMobile.AddDefaultHeader("Content-Type", "text/html;charset=gb2312");

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        public Vip GetVipInfo(long QqId)
        {
            string url = $"/p/mc/cardv2/other?platform=1&qq={QqId}&adtag=geren&aid=mvip.pingtai.mobileqq.androidziliaoka.fromqita";

            RestRequest request = new RestRequest(url);
            var response = restClientMobile.Execute(request);

            if (response?.StatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    string Match1 = response.Content.Substring(response.Content.IndexOf("pk-name") + 1);
                    string Match2 = Match1.Substring(Match1.IndexOf("pk-name") + 1);
                    string Match3 = Match2.Substring(Match2.IndexOf("pk-name") + 1);
                    string Match4 = Match3.Substring(Match3.IndexOf("pk-name") + 1);
                    string Match5 = Match4.Substring(Match4.IndexOf("pk-name") + 1);

                    string QqLevel = new String(Match1.Substring(Match1.IndexOf("<small>LV</small>", Match1.IndexOf("<small>LV</small>") + 1) + "<small>LV</small>".Length, 4).Where(Char.IsNumber).ToArray());
                    string LevelSpeed = new String(Match2.Substring(Match2.IndexOf("<small>倍</small>", Match2.IndexOf("<small>倍</small>") + 1) - 4, 5).SkipWhile((s) => s != '>').Skip(1).TakeWhile((s) => s != '<').ToArray());
                    string VipLevel = new String(Match3.Substring(Match3.IndexOf("<small>", Match3.IndexOf("<small>") + 1) + "<small>".Length, 6).TakeWhile((c) => c != '<').ToArray());
                    string GrowSpeed = new String(Match4.Substring(Match4.IndexOf("<p>", Match4.IndexOf("<p>") + 1) + "<p>".Length, 6).TakeWhile((c) => c != '<').ToArray());
                    string GrowupTotal = new String(Match5.Substring(Match5.IndexOf("<p>", Match5.IndexOf("<p>") + 1) + "<p>".Length, 6).TakeWhile((c) => c != '<').Where(Char.IsNumber).ToArray());

                    return new Vip()
                    {
                        QqLevel = int.Parse(QqLevel),
                        LevelSpeed = double.Parse(QqLevel),
                        GrowSpeed = double.Parse(GrowSpeed),
                        VipLevel = VipLevel,
                        GrowupTotal = long.Parse(GrowupTotal)
                    };
                }
                catch { }
            }
            return null;
        }

    }
}
