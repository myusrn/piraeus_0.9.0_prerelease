using SkunkLab.Channels;
using System;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Mqtt;

namespace Piraeus.Adapters
{
    //public delegate void ProtocolAdapterErrorHandler (object sender, ProtocolAdapterErrorEventArgs args);
    //public delegate void ProtocolAdapterCloseHandler (object sender, ProtocolAdapterCloseEventArgs args);
    public abstract class ProtocolAdapter : IDisposable
    {
        public static CoapConfig CoapConfig { get; set; }
        public static MqttConfig MqttConfig { get; set; }

        public abstract IChannel Channel { get; set; }
        //public abstract event ProtocolAdapterErrorHandler OnError;
        //public abstract event ProtocolAdapterCloseHandler OnClose;

        public abstract event System.EventHandler<ProtocolAdapterErrorEventArgs> OnError;
        public abstract event System.EventHandler<ProtocolAdapterCloseEventArgs> OnClose;
        public abstract event System.EventHandler<ChannelObserverEventArgs> OnObserve;

        public abstract void Init();
        public abstract void Dispose();



    }
}
