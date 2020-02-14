using Native.Sdk.Cqp;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Request
{
    public class Icon
    {
        static readonly RestClient restClientQq = new RestClient("http://q1.qlogo.cn/");
        static readonly RestClient restClientGroup = new RestClient("http://p.qlogo.cn/gh");
        static readonly string userAgentPC = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_3) AppleWebKit/604.5.6 (KHTML, like Gecko) Version/11.0.3 Safari/604.5.6";

        private CQLog Log { get; set; }
        private string AppDirectory { get; set; }

        public Icon(CQApi api, CQLog log)
        {
            AppDirectory = api.AppDirectory;
            Log = log;
            restClientQq.UserAgent = userAgentPC;
            restClientGroup.UserAgent = userAgentPC;
        }

        public enum IconSize : int
        {
            Raw = 0,
            Small = 40,
            Normal = 100
        }

        public string SaveQqIcon(long qqId, IconSize size = IconSize.Small, string folderPath = "", string prefix = "")
        {
            string url = $"/g?b=qq&nk={qqId}&s={(int)size}";
            string iconPath = Path.Combine(folderPath, $"{prefix}{(int)size}_{qqId}.jpg");
            string fullPath = Path.Combine(Environment.CurrentDirectory, "data/image", iconPath);

            if (File.Exists(fullPath)) { return iconPath; }
            if (DownloadAndSave(restClientQq, url, iconPath, fullPath)) { return iconPath; }
            return null;
        }

        public string SaveGroupIcon(long groupId, IconSize size = IconSize.Small, string folderPath = "", string prefix = "")
        {
            string url = $"/gh/{groupId}/{groupId}/{(int)size}";
            string iconPath = Path.Combine(folderPath, $"{prefix}{(int)size}_{groupId}.jpg");
            string fullPath = Path.Combine(Environment.CurrentDirectory, "data/image", iconPath);

            if (File.Exists(fullPath)) { return iconPath; }
            if (DownloadAndSave(restClientGroup, url, iconPath, fullPath)) { return iconPath; }
            return null;
        }

        private bool DownloadAndSave(RestClient client, string url, string iconPath, string fullPath)
        {
            var response = client.Execute(new RestRequest(url));
            if (response?.StatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    using (FileStream fs = new FileStream(fullPath, FileMode.Create))
                    {
                        fs.Write(response.RawBytes, 0, response.RawBytes.Count());
                        return true;
                    }
                }
                catch (Exception ex) { Log.Debug("新增文件错误", ex); }
            }
            return false;
        }
    }
}
