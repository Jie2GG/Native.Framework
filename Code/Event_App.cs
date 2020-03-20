using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;
using Native.Tool.IniConfig.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Model;
using Code.Service;
using Nancy.Hosting.Wcf;
using System.ServiceModel.Web;
using Code.Helper;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace Code
{
    public class Event_App : ICQStartup, ICQExit, IAppEnable, IAppDisable
    {
        public void AppDisable(object sender, CQAppDisableEventArgs e)
        {
            Common.IsRunning = false;
            if (Common.WebServiceHost.State == System.ServiceModel.CommunicationState.Opening || Common.WebServiceHost.State == System.ServiceModel.CommunicationState.Opened)
            {
                Common.WebServiceHost.Close();
            }
        }

        public void AppEnable(object sender, CQAppEnableEventArgs e)
        {
            if (Common.IsLoaded == false)
            {
                Common.CoolQDatabase = new CoolQDataBase(e.CQApi.GetLoginQQ().Id);

                Common.WebServiceHostPort = TCPHelper.GetAvailablePort(80);
                Common.WebSocketPort = TCPHelper.GetAvailablePort(10000);
                Common.ZeroMqPort = TCPHelper.GetAvailablePort(20000);

                Common.WebServiceHost = new WebServiceHost(new NancyWcfGenericService(new Startup.NancyBootstrapper()), new Uri($"http://localhost:{Common.WebServiceHostPort}"));
                Common.WebServiceHost.AddServiceEndpoint(typeof(NancyWcfGenericService), new Startup.NancyWebHttpBinding(), "");
                Common.CoolQWebSocket = new CoolQWebSocket(e.CQApi, e.CQLog);
                Common.CoolQZeroMQ = new CoolQZeroMQ(e.CQApi, e.CQLog);

                e.CQLog.Info("WCF服务端口", $"{Common.WebServiceHostPort}");
                e.CQLog.Info("WebSocket服务端口", $"{Common.WebSocketPort}");
                e.CQLog.Info("ZeroMQ服务端口", $"{Common.ZeroMqPort}");
            }

            Common.IsLoaded = true;
            Common.IsRunning = true;

            try
            {
                Common.WebServiceHost.Open();
            }
            catch (System.ServiceModel.AddressAccessDeniedException)
            {
                e.CQLog.Warning("WCF服务失效", "需要以管理员权限运行酷Q方可启用WCF服务");
            }
        }

        public void CQExit(object sender, CQExitEventArgs e)
        {
            Common.WebServiceHost.Abort();
        }

        public void CQStartup(object sender, CQStartupEventArgs e)
        {
            Common.Api = e.CQApi;
            Common.Log = e.CQLog;
            Common.Friends = new Request.FriendRequest(e.CQApi, e.CQLog);
            Common.VipInfo = new Request.VipInfo(e.CQApi, e.CQLog);
            Common.Icon = new Request.Icon(e.CQApi, e.CQLog);
            Common.Group = new Request.GroupRequest(e.CQApi, e.CQLog);

            ViewModel.MainInstance.Api = e.CQApi;
            ViewModel.MainInstance.Log = e.CQLog;
            ViewModel.MainInstance.UISettingPath = Path.Combine(e.CQApi.AppDirectory, "UISetting.ini");


            //提供每秒一次事件通知
            Common.Pub = Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1)).Timestamp().Publish();

            //订阅初始化
            Common.Pub.Connect();

            var runtime = DateTime.Now;

            //订阅事件
            Common.Pub.Subscribe(sub =>
            {
                if (sub.Value == 1)
                {
                    e.CQLog.Debug("订阅事件執行", sub.Value);
                }

                if (sub.Value % 60 == 0)//每分钟一次
                {
                    e.CQLog.Debug("每分钟一次", sub.Value);
                }

                if (DateTime.Now == runtime.AddMinutes(1))//在运行后一分钟
                {
                    e.CQLog.Debug("应用运行执行了一分钟", sub.Value);

                    //停用订阅
                    Common.Pub.Connect().Dispose();

                    Task.Delay(TimeSpan.FromSeconds(10)).Wait();

                    //再次订阅
                    Common.Pub.Connect();

                    Task.Delay(TimeSpan.FromSeconds(10)).Wait();
                }
            });

        }
    }
}
