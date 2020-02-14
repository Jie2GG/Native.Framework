using Native.Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Action
{
    public static class RemoveMessage
    {
        public static void RemoveByGroupIdWithIndex(long groupId, int index)
        {
            Common.Api.RemoveMessage(Common.CoolQDatabase.GetGroupEvent(groupId, index + 1).FirstOrDefault().Id);
        }
    }
}
