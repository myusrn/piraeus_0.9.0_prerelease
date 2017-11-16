using SkunkLab.Channels.WebSocket;
using SkunkLab.Protocols.Coap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Common
{
    public class Configuration
    {
        public static CoapConfig GetCoapConfig(string hostname)
        {
            return new CoapConfig(null, hostname, CoapConfigOptions.NoResponse | CoapConfigOptions.Observe);

        }

        public static WebSocketConfig GetWebSocketConfig()
        {
            return new WebSocketConfig();
        }
    }
}
