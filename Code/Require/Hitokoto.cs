using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Code.Require
{
    public class Hitokoto
    {
        static readonly RestClient restClient = new RestClient("https://v1.hitokoto.cn/");
        static readonly string userAgentPC = "native.csharp.demo";

        public Hitokoto()
        {
            restClient.UserAgent = userAgentPC;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        public string New()
        {
            RestRequest request = new RestRequest();
            var response = restClient.Execute(request);

            if (response?.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JObject obj = null;
                try
                {
                    obj = JObject.Parse(response.Content, new JsonLoadSettings()
                    {
                        DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Ignore,
                        LineInfoHandling = LineInfoHandling.Load
                    });
                }
                catch { }

                return obj["hitokoto"]?.ToObject<string>();
            }
            return null;
        }
    }

}
