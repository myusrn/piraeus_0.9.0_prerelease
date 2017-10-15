using System;
using SkunkLab.Core.Adapters;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Mqtt;

namespace SkunkLab.Core.Messaging
{
    public sealed class EventMessage
    {
        public EventMessage(ProtocolType protocol, string resourceUriString, string contentType, bool audit, byte[] message)
        {
            Protocol = protocol;
            ContentType = contentType;
            Message = message;
            Timestamp = DateTime.UtcNow;
            Payload = message;
        }

        public EventMessage(ProtocolType protocol, string resourceUriString, string contentType, bool audit, MqttMessage message)
        {
            Protocol = protocol;
            ContentType = contentType;
            Message = message.Encode();
            Timestamp = DateTime.UtcNow;
            Payload = message.Payload;
        }

        public EventMessage(ProtocolType protocol, string resourceUriString, string contentType, bool audit, CoapMessage message)
        {
            Protocol = protocol;
            ContentType = contentType;
            Message = message.Encode();
            Timestamp = DateTime.UtcNow;
            Payload = message.Payload;
        }

        private byte[] _payload;

        public ProtocolType Protocol { get; internal set; }

        public string ResourceUriString { get; internal set; }

        public byte[] Message { get; internal set; }

        public string ContentType { get; internal set; }

        public DateTime Timestamp { get; internal set; }

        public bool Audit { get; internal set; }
        public byte[] Payload { get; internal set; }


        
    }
}
