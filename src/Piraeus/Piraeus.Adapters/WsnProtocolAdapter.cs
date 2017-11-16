using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Piraeus.Configuration.Settings;
using SkunkLab.Channels;

namespace Piraeus.Adapters
{
    public class WsnProtocolAdapter : ProtocolAdapter
    {
        public WsnProtocolAdapter(PiraeusConfig config, IChannel channel)
        {
        }

        public override IChannel Channel { get; set; }

        public override event EventHandler<ProtocolAdapterErrorEventArgs> OnError;
        public override event EventHandler<ProtocolAdapterCloseEventArgs> OnClose;
        public override event EventHandler<ChannelObserverEventArgs> OnObserve;

        public override void Init()
        {
            OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(null, null));
            OnClose?.Invoke(this, new ProtocolAdapterCloseEventArgs(null));
            OnObserve?.Invoke(this, new ChannelObserverEventArgs(null, null, null));
        }

        public override void Dispose()
        {
            
        }

        
    }
}
