using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Demo.App.Model
{
	public class AppInfoEventArgs : EventArgs
	{
		/// <summary>
		/// 获取当前Api版本
		/// </summary>
		public int ApiVer { get; set; }
		/// <summary>
		/// 应用ID
		/// </summary>
		public string AppId { get; set; }
	}
}
