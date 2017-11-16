using System;
using SkunkLab.Channels;

namespace Piraeus.Adapters
{
    public abstract class ProtocolAdapter : IDisposable
    {
        //public static CoapConfig CoapConfig { get; set; }
        //public static MqttConfig MqttConfig { get; set; }

        public abstract IChannel Channel { get; set; }

        public abstract event System.EventHandler<ProtocolAdapterErrorEventArgs> OnError;
        public abstract event System.EventHandler<ProtocolAdapterCloseEventArgs> OnClose;
        public abstract event System.EventHandler<ChannelObserverEventArgs> OnObserve;

        public abstract void Init();
        public abstract void Dispose();



    }
}

