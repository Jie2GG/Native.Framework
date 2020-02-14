using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Code.Require
{
    public class TraceMoe
    {
        static readonly RestClient restClient = new RestClient("https://trace.moe/");
        static readonly string userAgentPC = "native.csharp.demo";

        public TraceMoe()
        {
            restClient.UserAgent = userAgentPC;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        public string Get(string imagePath)
        {
            var request = new RestSharp.RestRequest($"/api/search", Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded; charset=UTF-8");
            request.RequestFormat = DataFormat.Json;

            using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (Image img = Image.FromStream(fs))
                {
                    if (ImageFormat.Gif.Equals(img.RawFormat))
                    {
                        FrameDimension dimension = new FrameDimension(img.FrameDimensionsList[0]);
                        img.SelectActiveFrame(dimension, img.GetFrameCount(dimension) > 2 ? img.GetFrameCount(dimension) / 2 : 0);
                    }
                    using (var ms = new MemoryStream())
                    {
                        img.Save(ms, img.RawFormat);

                        request.AddParameter("application/x-www-form-urlencoded",
                            $"image={RestSharp.Extensions.StringExtensions.UrlEncode($"data:image/jpeg;base64,{ms.ToArray()}")}", ParameterType.RequestBody);
                    }
                }
            }
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

                return obj?.SelectToken("docs")?["title_chinese"]?.ToObject<string>();
            }

            return null;
        }
    }
}
