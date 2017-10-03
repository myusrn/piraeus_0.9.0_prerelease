using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Protocols.Coap;

namespace CoapServer
{
    public class RequestDispatcher : ICoapRequestDispatch
    {
        public RequestDispatcher()
        {

        }

       

        public CoapMessage Delete(CoapMessage message)
        {
            if(message.MessageType == CoapMessageType.Confirmable)
            {
                return new CoapResponse(message.MessageId, ResponseMessageType.Acknowledgement, ResponseCodeType.Deleted, message.Token);
            }
            else
            {
                return new CoapResponse(message.MessageId, ResponseMessageType.NonConfirmable, ResponseCodeType.Deleted, message.Token);
            }

            
        }

        public CoapMessage Get(CoapMessage message)
        {
            if (message.MessageType == CoapMessageType.Confirmable)
            {
                return new CoapResponse(message.MessageId, ResponseMessageType.Acknowledgement, ResponseCodeType.Valid, message.Token);
            }
            else
            {
                return new CoapResponse(message.MessageId, ResponseMessageType.NonConfirmable, ResponseCodeType.Valid, message.Token);
            }
        }

        public CoapMessage Observe(CoapMessage message)
        {
            if (message.MessageType == CoapMessageType.Confirmable)
            {
                return new CoapResponse(message.MessageId, ResponseMessageType.Acknowledgement, ResponseCodeType.Valid, message.Token);
            }
            else
            {
                return new CoapResponse(message.MessageId, ResponseMessageType.NonConfirmable, ResponseCodeType.Valid, message.Token);
            }
        }

        public CoapMessage Post(CoapMessage message)
        {
            if (message.MessageType == CoapMessageType.Confirmable)
            {
                return new CoapResponse(message.MessageId, ResponseMessageType.Acknowledgement, ResponseCodeType.Created, message.Token);
            }
            else
            {
                return new CoapResponse(message.MessageId, ResponseMessageType.NonConfirmable, ResponseCodeType.Created, message.Token);
            }
        }

        public CoapMessage Put(CoapMessage message)
        {
            if (message.MessageType == CoapMessageType.Confirmable)
            {
                return new CoapResponse(message.MessageId, ResponseMessageType.Acknowledgement, ResponseCodeType.Created, message.Token);
            }
            else
            {
                return new CoapResponse(message.MessageId, ResponseMessageType.NonConfirmable, ResponseCodeType.Created, message.Token);
            }
        }
    }
}
