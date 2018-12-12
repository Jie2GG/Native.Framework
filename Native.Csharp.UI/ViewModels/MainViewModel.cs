using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;

namespace Native.Csharp.UI.ViewModels
{
	public class MainViewModel : NotificationObject
	{
		#region --字段--
		private string _windowTitle = "酷Q样例应用控制台";
		#endregion

		#region --属性--
		/// <summary>
		/// 获取 MainView 窗口标题
		/// </summary>
		public string WindowTitle { get { return _windowTitle; } }
		#endregion

		/// <summary>
		/// 初始化 MainViewModel 实例对象
		/// </summary>
		public MainViewModel()
		{

		}
	}
}
