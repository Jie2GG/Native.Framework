using Native.Tool.IniConfig;
using Native.Tool.IniConfig.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Data;
using UI.Model;

namespace UI
{
    public static class AppSetting
    {
        public static void Load()
        {
            if (File.Exists(ViewModel.MainInstance.UISettingPath))
            {
                try
                {
                    IniConfig ini = new IniConfig(ViewModel.MainInstance.UISettingPath);
                    ini.Load();
                    if (ini.Object.Any(a => a.ContainsKey("LastPreSend")))
                    {
                        if (ini.Object["LastPreSend"].TryGetValue("Message", out IValue MessageValue))
                        {
                            ViewModel.MainInstance.ReadyToSend = MessageValue.GetValue<string>();

                            if (ini.Object["LastPreSend"].TryGetValue("GroupId", out IValue GroupIdValue))
                            {
                                Group LastSelectedGroup = ViewModel.MainInstance.Groups?.FirstOrDefault(w => w.GroupId == GroupIdValue.ToInt64());
                                if (LastSelectedGroup != null)
                                {
                                    ViewModel.MainInstance.SelectedGroup = LastSelectedGroup;
                                }
                            }
                        }
                    }
                }
                catch(Exception ex) { Debug.WriteLine(ex.Message); }
            }
        }
        public static void Save()
        {
            IniConfig ini = new IniConfig(ViewModel.MainInstance.UISettingPath);

            ini.Object["LastPreSend"] = new ISection("LastPreSend") 
            {
                {   "Message",ViewModel.MainInstance.ReadyToSend },
                {   "GroupId", ViewModel.MainInstance.SelectedGroup.GroupId }
            };

            ini.Save();
        }
    }
}
