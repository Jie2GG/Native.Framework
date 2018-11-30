using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.UI.ViewModels
{
	public class MainViewModel
	{
		#region --属性--
		/// <summary>
		/// 获取一个值, 指示窗口的标题
		/// </summary>
		public string WindowTitle
		{
			get { return "酷Q样例应用(WPF版)"; }
		}
		#endregion

		#region --构造函数--
		public MainViewModel()
		{

		}
		#endregion
	}
}
