using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Protocols.Utilities;

namespace SkunkLab.Protocols.Coap.Handlers
{
    public class CoapObserveHandler : CoapMessageHandler
    {
        public CoapObserveHandler(CoapSession session, CoapMessage message, ICoapRequestDispatch dispatcher = null)
            : base(session, message, dispatcher)
        {
        }

        public override async Task<CoapMessage> ProcessAsync()
        {
            if (!Session.CoapReceiver.IsDup(Message.MessageId))
            {
                Session.CoapReceiver.CacheId(Message.MessageId);               
            }

            return await Task.FromResult<CoapMessage>(Dispatcher.Observe(Message));
        }
    }
}
