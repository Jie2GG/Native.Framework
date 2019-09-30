using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native.Csharp.Sdk.Cqp.EventArgs;
using Native.Csharp.Sdk.Cqp.Interface;

namespace Native.Csharp.App.Event
{
    /// <summary>
	/// Type=1001 酷Q启动, 事件实现
	/// </summary>
    public class Event_CqStartup : ICqStartup
    {
        /// <summary>
		/// 处理 酷Q 的启动事件回调
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
        public void CqStartup (object sender, CqStartupEventArgs e)
        {
            // 本方法会在酷Q【主线程】中被调用。
            // 每次启动酷Q后，应用启用并被加载时，本方法将被调用，请在这里执行插件初始化代码。
            // 请务必尽快缩短本方法的执行时间，否则会卡住其他插件以及主程序的加载。

            Common.AppDirectory = Common.CqApi.GetAppDirectory ();  // 获取应用数据目录(无需储存数据时，请将此行注释)

            // 返回如：D:\CoolQ\data\app\com.example.demo\
            // 应用的所有数据、配置【必须】存放于此目录，避免给用户带来困扰。
        }
    }
}
