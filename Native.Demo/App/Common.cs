using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Demo.App
{
	public static class Common
	{
		/// <summary>
		/// 应用目录
		/// </summary>
		public static string AppDirectory { get; set; }

		/// <summary>
		/// 应用是否正在运行
		/// </summary>
		public static bool IsRunning { get; set; }
	}
}
