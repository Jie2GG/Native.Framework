using Native.Sdk.Cqp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace Code.Service
{
    public class CoolQZeroMQ
    {
        private CQApi Api { get; set; }
        private CQLog Log { get; set; }
        private ResponseSocket Socket { get; set; }
        public EventHandler<String> OnMessage;

        public CoolQZeroMQ(CQApi api, CQLog log)
        {
            this.Api = api;
            this.Log = log;
            Task.Factory.StartNew(() =>
            {
                using (this.Socket = new ResponseSocket($"@tcp://localhost:{Common.ZeroMqPort}"))
                {
                    while (true)
                    {
                        string msg = this.Socket.ReceiveFrameString();
                        OnMessage(this.Socket, msg);
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        ~CoolQZeroMQ() 
        {
            this.Socket.Dispose();
        }
    }
}
