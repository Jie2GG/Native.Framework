using Native.Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Action
{
    public static class ReceiveRecord
    {
        public static string ReceiveRecordAsAMR(QQMessage message)
        {
            if (message.CQCodes.Any(a => a.Function == Native.Sdk.Cqp.Enum.CQFunction.Record)) 
            {
                return message.ReceiveRecord(Native.Sdk.Cqp.Enum.CQAudioFormat.AMR_NB);
            }
            return string.Empty;
        }
    }
}
