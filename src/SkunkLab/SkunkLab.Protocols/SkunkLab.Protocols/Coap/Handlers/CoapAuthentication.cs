using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Protocols.Utilities;

namespace SkunkLab.Protocols.Coap.Handlers
{
    public class CoapAuthentication
    {
        public static void EnsureAuthentication(CoapSession session, CoapMessage message, bool force = false)
        {            
            if(!session.IsAuthenticated || force)
            {
                CoapUri coapUri = new CoapUri(message.ResourceUri.ToString());

                Trace.WriteLine(String.Format("Coap URI token type = {0}", coapUri.TokenType));
                Trace.WriteLine(String.Format("Coap URI token = {0}", coapUri.SecurityToken));
                if (!session.Authenticate(coapUri.TokenType, coapUri.SecurityToken))
                {
                    Trace.WriteLine("Coap URI authentication failed.");
                    throw new SecurityException("CoAP session not authenticated.");
                }
                else
                {
                    Trace.WriteLine("Coap URI authentication is successful");
                }

            }
        }
    }
}
