using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;
using Native.Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code
{
    public class Event_Status : IStatusUpdate
    {
        public CQFloatWindow StatusUpdate(object sender, CQStatusUpdateEventArgs e)
        {
            Common.UpTime += 1;

            //运行报告(只有在酷Q选取当前悬浮窗才触发)
            if (Common.UpTime % 3600 == 0)
            {
                e.CQLog.Info("运行报告", $"当前已运行{Common.UpTime / 3600}小时了，共加入了{Common.NewFriends}位新朋友!啪唧啪唧~");
            }

            return new CQFloatWindow()
            {
                TextColor = Native.Sdk.Cqp.Enum.CQFloatWindowColors.Green,
                Unit = "人",
                Value = Common.NewFriends
            };
        }
    }
}
