using Code.Request;
using Code.Require;
using Code.Service;
using Native.Sdk.Cqp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using UI;

namespace Code
{
    public static class Common
    {
        /// <summary>
        /// 获取或设置当前 App 是否处于运行状态
        /// </summary>
        public static bool IsRunning { get; set; } = false;

        /// <summary>
        /// 是否处于服务运行状态
        /// </summary>
        public static bool IsLoaded { get; set; } = false;

        /// <summary>
        /// 酷Q接口的封装类
        /// </summary>
        public static CQApi Api { get; set; }

        /// <summary>
        ///  酷Q日志的封装类
        /// </summary>
        public static CQLog Log { get; set; }

        /// <summary>
        /// 好友列表扩展
        /// </summary>
        public static FriendRequest Friends { get; set; }

        /// <summary>
        /// QQ会员资料
        /// </summary>
        public static VipInfo VipInfo { get; set; }

        /// <summary>
        /// QQ图标
        /// </summary>
        public static Icon Icon { get; set; }

        /// <summary>
        /// 群公告
        /// </summary>
        public static GroupRequest Group { get; set; }

        /// <summary>
        /// 新加入的朋友数
        /// </summary>
        public static long NewFriends { get; set; } = 0;

        /// <summary>
        /// UI窗口(WPF)
        /// </summary>
        public static MainWindow MainWindow = null;

        /// <summary>
        /// UI窗口(WinForm)
        /// </summary>
        public static MainForm MainForm = null;

        /// <summary>
        /// 酷Q内部的数据库
        /// </summary>
        public static CoolQDataBase CoolQDatabase { get; set; }

        /// <summary>
        /// WCF服务
        /// </summary>
        public static WebServiceHost WebServiceHost { get; set; }

        /// <summary>
        /// WebSocket服务
        /// </summary>
        public static CoolQWebSocket CoolQWebSocket { get; set; }

        /// <summary>
        /// WCF服务端口
        /// </summary>
        public static int WebServiceHostPort { get; set; }

        /// <summary>
        /// WebSocket服务端口
        /// </summary>
        public static int WebSocketPort { get; set; }

        /// <summary>
        /// WebSocket访问密钥
        /// </summary>
        public static string WebSocketSecret = "kSLuTF2GC2Q4q4ugm3";

        /// <summary>
        /// ZeroMQ服务端口
        /// </summary>
        public static int ZeroMqPort { get; set; }

        /// <summary>
        /// ZeroMQ服务
        /// </summary>
        public static CoolQZeroMQ CoolQZeroMQ { get; set; }
        /// <summary>
        /// 悬浮窗使用时长
        /// </summary>
        public static long UpTime { get; set; }

        /// <summary>
        /// Hitokoto
        /// </summary>
        public static Hitokoto Hitokoto { get; set; }

        /// <summary>
        /// TraceMoe
        /// </summary>
        public static TraceMoe TraceMoe { get; set; }

    }

}
