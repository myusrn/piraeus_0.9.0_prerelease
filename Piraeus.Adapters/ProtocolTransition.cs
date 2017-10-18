using System;
using Piraeus.Core.Messaging;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Coap.Handlers;
using SkunkLab.Protocols.Mqtt;

namespace Piraeus.Adapters
{
    public class ProtocolTransition
    {
        public static bool IsEncryptedChannel { get; set; }

        public static byte[] ConvertToMqtt(MqttSession session, EventMessage message)
        {
            if(message.Protocol == ProtocolType.MQTT)
            {
                return MqttConversion(session, message.Message);
            }
            else if(message.Protocol == ProtocolType.COAP)
            {
                CoapMessage msg = CoapMessage.DecodeMessage(message.Message);
                return MqttConversion(session, msg.Payload, message.ContentType);
            }
            else
            {
                return MqttConversion(session, message.Message, message.ContentType);
            }
        }

        public static byte[] ConvertToCoap(CoapSession session, EventMessage message)
        {            
            CoapToken token = CoapToken.Create();
            ushort id = session.CoapSender.NewId(token.TokenBytes);
            string uriString = CoapUri.Create(session.Config.Authority, message.ResourceUri, IsEncryptedChannel);
            CoapRequest request = null;

            if (message.Protocol == ProtocolType.MQTT)
            {
                MqttMessage msg = MqttMessage.DecodeMessage(message.Message);                
                RequestMessageType messageType = msg.QualityOfService == QualityOfServiceLevelType.AtMostOnce ? RequestMessageType.NonConfirmable : RequestMessageType.Confirmable;
                request = new CoapRequest(id, messageType, MethodType.POST, new Uri(uriString), MediaTypeConverter.ConvertToMediaType(message.ContentType));                
            }
            else if(message.Protocol == ProtocolType.COAP)
            {
                CoapMessage msg = CoapMessage.DecodeMessage(message.Message);                
                request = new CoapRequest(id, msg.MessageType == CoapMessageType.Confirmable ? RequestMessageType.Confirmable : RequestMessageType.NonConfirmable, MethodType.POST, new Uri(uriString), MediaTypeConverter.ConvertToMediaType(message.ContentType));
            }
            else
            {   
                request = new CoapRequest(id, RequestMessageType.NonConfirmable, MethodType.POST, new Uri(""), MediaTypeConverter.ConvertToMediaType(message.ContentType), message.Message);
            }

            return request.Encode();
        }


        

        

        private static byte[] MqttConversion(MqttSession session, byte[] message, string contentType = null)
        {
            PublishMessage msg = MqttMessage.DecodeMessage(message) as PublishMessage;
            MqttUri uri = new MqttUri(msg.Topic);
            QualityOfServiceLevelType? qos = session.GetQoS(uri.Resource);

            msg.QualityOfService = qos.HasValue ? qos.Value : QualityOfServiceLevelType.AtMostOnce;
            msg.MessageId = session.NewId();

            if (msg.QualityOfService != QualityOfServiceLevelType.AtMostOnce)
            {
                session.Quarantine(msg);
            }

            return msg.Encode();
        }

        

        
    }
}
