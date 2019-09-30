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
	/// Type=1002 酷Q退出, 事件实现
	/// </summary>
    public class Event_CqExit : ICqExit
    {
        /// <summary>
		/// 处理 酷Q 的退出事件回调
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
        public void CqExit (object sender, CqExitEventArgs e)
        {
            // 本方法会在酷Q【主线程】中被调用。
            // 无论本应用是否被启用，本方法都会在酷Q退出前执行一次，请在这里执行插件关闭代码。

            // 本方法将固定给酷Q返回 0, 返回后酷Q将很快关闭，请不要再通过线程等方式执行其他代码。
        }
    }
}
