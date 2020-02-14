using Native.Sdk.Cqp;
using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code
{
    public class Event_Message_MenuCall : IGroupMessage
    {
        private object BoomLocker = new object();

        public void GroupMessage(object sender, CQGroupMessageEventArgs e)
        {
            if (e.Message.IsRegexMessage)
            {
                string cmd = "";
                string sid = "";
                long id = 0;
                e.Message.RegexKeyValuePairs.TryGetValue("cmd", out cmd);
                e.Message.RegexKeyValuePairs.TryGetValue("id", out sid);

                long.TryParse(sid, out id);

                if(cmd.Trim()=="爆炸" && id > 0)
                {
                    if (id > 10) { id = 10; }
                    lock (BoomLocker)
                    {
                        //爆炸n次(<11)
                        for(int i = 0; i < id; i++)
                        {
                            e.CQApi.SendGroupMessage(e.FromGroup, "BOOM!" + CQApi.CQCode_Emoji(372245));
                            Task.Delay(TimeSpan.FromSeconds(0.5)).Wait();
                        }
                        Task.Delay(TimeSpan.FromSeconds(10)).Wait();
                    }
                }

            }
        }
    }
}
