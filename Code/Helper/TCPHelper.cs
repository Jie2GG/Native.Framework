using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Code.Helper
{
    public static class TCPHelper
    {
        public static int GetAvailablePort(int startingPort)
        {
            var portArray = new List<int>();
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var connections = properties.GetActiveTcpConnections();
            var endPoints = properties.GetActiveTcpListeners();

            portArray.AddRange(connections.Where(w => w.LocalEndPoint.Port >= startingPort).Select(s=>s.LocalEndPoint.Port));
            portArray.AddRange(endPoints.Where(w => w.Port >= startingPort).Select(s => s.Port));
            portArray.Sort();

            for (var i = startingPort; i < UInt16.MaxValue; i++)
            {
                if (!portArray.Contains(i))
                {
                    return i;
                }
            }

            return 0;
        }
    }
}
