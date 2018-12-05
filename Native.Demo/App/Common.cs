using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Demo.App
{
	public class Common
	{
		#region --字段--
		private static readonly Lazy<Common> _instance = new Lazy<Common>(() => new Common());
		#endregion

		#region --属性--
		/// <summary>
		/// 获取 Common 的实例对象
		/// </summary>
		public static Common Instance
		{
			get { return _instance.Value; }
		}
		/// <summary>
		/// 应用目录
		/// </summary>
		public string AppDirectory { get; set; }
		/// <summary>
		/// 应用是否正在运行
		/// </summary>
		public bool IsRunning { get; set; }
		#endregion

		#region --构造函数--
		/// <summary>
		/// 隐藏构造函数
		/// </summary>
		private Common()
		{

		}
		#endregion
	}
}
