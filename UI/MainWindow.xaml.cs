using Native.Sdk.Cqp;
using Native.Sdk.Cqp.EventArgs;
using Native.Tool.IniConfig.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using UI.Data;
using UI.Model;

namespace UI
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            BindingOperations.EnableCollectionSynchronization(ViewModel.MainInstance.GroupMessages, ViewModel.MainInstance.SyncLock);
            BindingOperations.EnableCollectionSynchronization(ViewModel.MainInstance.Groups, ViewModel.MainInstance.SyncLock);

            ViewModel.MainInstance.Api.GetGroupList().ForEach(g =>
            {
                if (ViewModel.MainInstance.Groups.Any(a => a.GroupId == g.Group.Id) == false)
                {
                    ViewModel.MainInstance.Groups.Add(new Group { GroupId = g.Group.Id, GroupName = g.Name });
                }
            });

            this.Loaded += (s, e) =>
            {
                AppSetting.Load();
            };

            this.Closing += (s, e) =>
            {
                AppSetting.Save();
            };
        }


    }
}
