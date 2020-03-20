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
                    IniObject iObject = IniObject.Load(ViewModel.MainInstance.UISettingPath);
                    IniSection LastPreSendSection;
                    IniValue MessageValue;
                    IniValue GroupIdValue;

                    if (iObject.Any(a => a.ContainsKey("LastPreSend")))
                    {
                        LastPreSendSection = iObject["LastPreSend"];
                        if (LastPreSendSection.TryGetValue("Message", out MessageValue))
                        {
                            ViewModel.MainInstance.ReadyToSend = MessageValue.Value;

                            if (LastPreSendSection.TryGetValue("GroupId", out GroupIdValue))
                            {
                                Group LastSelectedGroup = ViewModel.MainInstance.Groups.FirstOrDefault(w => w.GroupId == GroupIdValue.ToInt64());
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
            IniObject iObject = IniObject.Load(ViewModel.MainInstance.UISettingPath);

            IniSection LastPreSendSection = new IniSection("LastPreSend");
            IniValue MessageValue = new IniValue(ViewModel.MainInstance.ReadyToSend);
            IniValue GroupIdValue = new IniValue(ViewModel.MainInstance.SelectedGroup.GroupId);
            LastPreSendSection.Add("Message", MessageValue);
            LastPreSendSection.Add("GroupId", GroupIdValue);

            if (iObject.Any(a=>a.Keys.Contains("LastPreSend")))
            {
                if (iObject["LastPreSend"].ContainsKey("Message"))
                {
                    iObject["LastPreSend"]["Message"] = LastPreSendSection["Message"];
                }
                else
                {
                    iObject["LastPreSend"].Add("Message", LastPreSendSection["Message"]);
                }

                if (iObject["LastPreSend"].ContainsKey("GroupId"))
                {
                    iObject["LastPreSend"]["GroupId"] = LastPreSendSection["GroupId"];
                }
                else
                {
                    iObject["LastPreSend"].Add("GroupId", LastPreSendSection["GroupId"]);
                }
            }
            else
            {
                iObject.Add(LastPreSendSection);
            }
            iObject.Save(ViewModel.MainInstance.UISettingPath);
        }
    }
}
