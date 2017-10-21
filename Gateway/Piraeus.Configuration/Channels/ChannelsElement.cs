using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Configuration.Channels
{
    public class ChannelsElement : ConfigurationElement
    {
        [ConfigurationProperty("tcp")]
        public TcpChannelElement TCP
        {
            get { return (TcpChannelElement)base["tcp"]; }
            set { base["tcp"] = value; }
        }
    }
}
