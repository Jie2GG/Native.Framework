using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI;
using System.Windows;
using System.Reflection;

namespace Code
{
    public class Event_Menu : IMenuCall
    {

        public void MenuCall(object sender, CQMenuCallEventArgs e)
        {
            e.CQLog.Debug("菜单点击事件", $"打开介面-{e.Name}");
            if(e.Name== "WinForm")
            {
                OpenWinForm();
            }
            else
            {
                OpenWpf();
            }
        }

        public void OpenWinForm()
        {
            if (Common.MainForm != null)
            {
                if (Common.MainForm.IsDisposed == false)
                {
                    if (Common.MainForm.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                    {
                        Common.MainForm.WindowState = System.Windows.Forms.FormWindowState.Normal;
                    }
                    Common.MainForm.Focus();
                    return;
                }
            }
            Common.MainForm = new MainForm();
            Common.MainForm.Show();
        }

        public void OpenWpf()
        {
            if (Common.MainWindow != null)
            {
                var propertyInfo = typeof(MainWindow).GetProperty("IsDisposed", BindingFlags.NonPublic | BindingFlags.Instance);
                if (!(bool)propertyInfo.GetValue(Common.MainWindow, null))
                {
                    if(Common.MainWindow.WindowState == WindowState.Minimized)
                    {
                        Common.MainWindow.WindowState = WindowState.Normal;
                    }
                    Common.MainWindow.Focus();
                    return;
                }
            }
            Common.MainWindow = new MainWindow();
            Common.MainWindow.Show();
        }
    }
}
