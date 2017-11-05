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

        public static byte[] ConvertToCoap(CoapSession session, EventMessage message, byte[] observableToken = null)
        {            
            CoapToken token = CoapToken.Create();

            ushort id = observableToken == null ? session.CoapSender.NewId(token.TokenBytes) : session.CoapSender.NewId(observableToken);

            string uriString = CoapUri.Create(session.Config.Authority, message.ResourceUri, IsEncryptedChannel);
            CoapRequest request = null;
            CoapResponse response = null;

            if (message.Protocol == ProtocolType.MQTT)
            {
                MqttMessage msg = MqttMessage.DecodeMessage(message.Message);
                PublishMessage pub = msg as PublishMessage;
                MqttUri uri = new MqttUri(pub.Topic);
                if (observableToken == null)
                {
                    RequestMessageType messageType = msg.QualityOfService == QualityOfServiceLevelType.AtMostOnce ? RequestMessageType.NonConfirmable : RequestMessageType.Confirmable;
                    request = new CoapRequest(id, messageType, MethodType.POST, new Uri(uriString), MediaTypeConverter.ConvertToMediaType(message.ContentType));
                }
                else
                {
                    response = new CoapResponse(id, ResponseMessageType.NonConfirmable, ResponseCodeType.Content, observableToken, MediaTypeConverter.ConvertToMediaType(uri.ContentType), msg.Payload);
                }
            }
            else if(message.Protocol == ProtocolType.COAP)
            {
                CoapMessage msg = CoapMessage.DecodeMessage(message.Message);
                if (observableToken == null)
                {
                    request = new CoapRequest(id, msg.MessageType == CoapMessageType.Confirmable ? RequestMessageType.Confirmable : RequestMessageType.NonConfirmable, MethodType.POST, new Uri(uriString), MediaTypeConverter.ConvertToMediaType(message.ContentType));
                }
                else
                {
                    response = new CoapResponse(id, ResponseMessageType.NonConfirmable, ResponseCodeType.Content, observableToken, MediaTypeConverter.ConvertToMediaType(message.ContentType), msg.Payload);
                }
            }
            else
            {
                if (observableToken == null)
                {
                    request = new CoapRequest(id, RequestMessageType.NonConfirmable, MethodType.POST, new Uri(uriString), MediaTypeConverter.ConvertToMediaType(message.ContentType), message.Message);
                }
                else
                {
                    response = new CoapResponse(id, ResponseMessageType.NonConfirmable, ResponseCodeType.Content, observableToken, MediaTypeConverter.ConvertToMediaType(message.ContentType), message.Message);
                }
            }

            return request.Encode();
        }

        public static byte[] ConvertToHttp(EventMessage message)
        {
            if(message.Protocol == ProtocolType.MQTT)
            {
                MqttMessage mqtt = MqttMessage.DecodeMessage(message.Message);
                return mqtt.Payload;
            }
            else if(message.Protocol == ProtocolType.COAP)
            {
                CoapMessage coap = CoapMessage.DecodeMessage(message.Message);
                return coap.Payload;
            }
            else
            {
                return message.Message;
            }
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
