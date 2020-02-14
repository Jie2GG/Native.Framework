using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code
{
    public class Event_Friend : IFriendAdd, IFriendAddRequest
    {
        public void FriendAdd(object sender, CQFriendAddEventArgs e)
        {
            Common.NewFriends += 1;
        }

        public void FriendAddRequest(object sender, CQFriendAddRequestEventArgs e)
        {
            if (Common.IsRunning == false) { return; }
            e.CQApi.SetFriendAddRequest(e.Request, Native.Sdk.Cqp.Enum.CQResponseType.PASS);
        }
    }
}
