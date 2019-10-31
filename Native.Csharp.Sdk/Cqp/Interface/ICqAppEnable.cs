using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native.Csharp.Sdk.Cqp.EventArgs;

namespace Native.Csharp.Sdk.Cqp.Interface
{
	/// <summary>
	/// Type=1003 应用被启用, 事件接口
	/// </summary>
	public interface ICqAppEnable
	{
		/// <summary>
		/// 当在派生类中重写时, 处理 酷Q 的插件启动事件回调
		/// </summary>
		/// <param name="sender">事件的触发对象</param>
		/// <param name="e">事件的附加参数</param>
		void CqAppEnable (object sender, CqAppEnableEventArgs e);
	}
}
