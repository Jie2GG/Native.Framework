using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Native.Csharp.UI.ViewModels
{
	public class MainViewModel : NotificationObject
	{
		#region --字段--
		private string _windowTitle = "酷Q样例应用控制台";
		#endregion

		#region --数据属性--
		public string WindowTitle
		{
			get { return _windowTitle; }
			set
			{
				_windowTitle = value;
				RaisePropertyChanged(nameof(WindowTitle));  //触发更新
			}
		}
		#endregion

		#region --命令属性--
		public ICommand ButtonCommandDemo
		{
			get
			{
				return new DelegateCommand(() =>
				{
					System.Windows.MessageBox.Show("按钮被按下了");
				});
			}
		}
		#endregion

		#region --构造函数--
		public MainViewModel()
		{

		}
		#endregion
	}
}
