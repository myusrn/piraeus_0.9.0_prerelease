using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using SkunkLab.Channels;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Coap.Handlers;

namespace Piraeus.Adapters
{
    public class CoapRequestDispatcher : ICoapRequestDispatch
    {
        public CoapRequestDispatcher(CoapSession session, IChannel channel)
        {
            this.channel = channel;
            this.session = session;
            coapObserved = new Dictionary<string, byte[]>();
            coapUnobserved = new HashSet<string>();
            adapter = new OrleansAdapter();
            adapter.OnObserve += Adapter_OnObserve;
            Task task = LoadDurables();
            Task.WhenAll(task);
        }

        private OrleansAdapter adapter;
        private IChannel channel;
        private CoapSession session;
        private HashSet<string> coapUnobserved;
        private Dictionary<string, byte[]> coapObserved;
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Unsubscribes an ephemeral subscription from a resource.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public CoapMessage Delete(CoapMessage message)
        {
            Exception error = null;

            CoapUri uri = new CoapUri(message.ResourceUri.ToString());
            try
            {
                Task task = UnsubscribeAsync(uri.Resource);
                Task.WhenAll(task);
            }
            catch (AggregateException ae)
            {
                error = ae.Flatten().InnerException;
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error == null)
            {
                ResponseMessageType rmt = message.MessageType == CoapMessageType.Confirmable ? ResponseMessageType.Acknowledgement : ResponseMessageType.NonConfirmable;
                return new CoapResponse(message.MessageId, rmt, ResponseCodeType.Deleted, message.Token);
            }
            else
            {
                return new CoapResponse(message.MessageId, ResponseMessageType.Reset, ResponseCodeType.EmptyMessage);
            }
        }

        private async Task UnsubscribeAsync(string resourceUriString)
        {
            await adapter.UnsubscribeAsync(resourceUriString);
            coapObserved.Remove(resourceUriString);
        }

        /// <summary>
        /// Not implemented in Piraeus
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <remarks>GET is associated with CoAP observe.  If the client does not support this, then
        /// should use PUT to create a subscription.  Therefore, a GET which is not CoAP observe returns RST.</remarks>
        public CoapMessage Get(CoapMessage message)
        {
            return new CoapResponse(message.MessageId, ResponseMessageType.Reset, ResponseCodeType.EmptyMessage, message.Token);
        }

        /// <summary>
        /// CoAP observe, observes a resource for a subscription.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public CoapMessage Observe(CoapMessage message)
        {
            Task<CoapMessage> task = ObserveAsync(message);
            Task.WhenAll<CoapMessage>(task);
            return task.Result;
        }


        public async Task<CoapMessage> ObserveAsync(CoapMessage message)
        {
            if (!message.Observe.HasValue)
            {
                //RST because GET needs to be observe/unobserve
                return new CoapResponse(message.MessageId, ResponseMessageType.Reset, ResponseCodeType.EmptyMessage);
            }

            CoapUri uri = new CoapUri(message.ResourceUri.ToString());
            ResponseMessageType rmt = message.MessageType == CoapMessageType.Confirmable ? ResponseMessageType.Acknowledgement : ResponseMessageType.NonConfirmable;

            if (!await adapter.CanSubscribeAsync(uri.Resource, channel.IsEncrypted))
            {
                //not authorized
                return new CoapResponse(message.MessageId, rmt, ResponseCodeType.Unauthorized, message.Token);
            }

            if (!message.Observe.Value)
            {
                //unsubscribe
                await adapter.UnsubscribeAsync(uri.Resource);
                coapObserved.Remove(uri.Resource);
            }
            else
            {
                //subscribe
                SubscriptionMetadata metadata = new SubscriptionMetadata()
                {
                    IsEphemeral = true,
                    Identity = session.Identity,
                    Indexes = session.Indexes
                };

                string subscriptionUriString = await adapter.SubscribeAsync(uri.Resource, metadata);

                if (!coapObserved.ContainsKey(uri.Resource)) //add resource to observed list
                {
                    coapObserved.Add(uri.Resource, message.Token);
                }
            }

            return new CoapResponse(message.MessageId, rmt, ResponseCodeType.Valid, message.Token);
        }

        private void Adapter_OnObserve(object sender, ObserveMessageEventArgs e)
        {

            byte[] message = null;

            if (coapObserved.ContainsKey(e.Message.ResourceUri))
            {
                message = ProtocolTransition.ConvertToCoap(session, e.Message, coapObserved[e.Message.ResourceUri]);
            }
            else
            {
                message = ProtocolTransition.ConvertToCoap(session, e.Message);
            }

            //byte[] message = coapObserved.ContainsKey(e.Message.ResourceUri) ? ProtocolTransition.ConvertToCoap(session, e.Message, coapObserved[e.Message.ResourceUri]) : ProtocolTransition.ConvertToCoap(session, e.Message);

            Task task = channel.SendAsync(message);
            Task.WhenAll(task);
        }

        /// <summary>
        /// Publishing a message to a resource.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public CoapMessage Post(CoapMessage message)
        {
            Task<CoapMessage> task = PostAsync(message);
            Task.WhenAll<CoapMessage>(task);
            return task.Result;
        }

        private async Task<CoapMessage> PostAsync(CoapMessage message)
        {
            CoapUri uri = new CoapUri(message.ResourceUri.ToString());
            ResponseMessageType rmt = message.MessageType == CoapMessageType.Confirmable ? ResponseMessageType.Acknowledgement : ResponseMessageType.NonConfirmable;

            if (!await adapter.CanPublishAsync(uri.Resource, channel.IsEncrypted))
            {
                return new CoapResponse(message.MessageId, rmt, ResponseCodeType.Unauthorized, message.Token);
            }

            string contentType = message.ContentType.HasValue ? message.ContentType.Value.ConvertToContentType() : "application/octet-stream";
            EventMessage msg = new EventMessage(contentType, uri.Resource, ProtocolType.COAP, message.Encode());

            

            if(uri.Indexes == null)
            {
                await adapter.PublishAsync(msg);
            }
           else
            {
                List<KeyValuePair<string, string>> indexes = new List<KeyValuePair<string, string>>(uri.Indexes);
                await adapter.PublishAsync(msg, indexes);
            }
            

            return new CoapResponse(message.MessageId, rmt, ResponseCodeType.Created, message.Token);
        }

        /// <summary>
        /// Subscribe message for ephemeral subscription
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public CoapMessage Put(CoapMessage message)
        {
            Task<CoapMessage> task = PutAsync(message);
            Task.WhenAll<CoapMessage>(task);
            return task.Result;
        }

        private async Task<CoapMessage> PutAsync(CoapMessage message)
        {
            CoapUri uri = new CoapUri(message.ResourceUri.ToString());
            ResponseMessageType rmt = message.MessageType == CoapMessageType.Confirmable ? ResponseMessageType.Acknowledgement : ResponseMessageType.NonConfirmable;

            if (!await adapter.CanSubscribeAsync(uri.Resource, channel.IsEncrypted))
            {
                return new CoapResponse(message.MessageId, rmt, ResponseCodeType.Unauthorized, message.Token);
            }

            if (coapObserved.ContainsKey(uri.Resource) || coapUnobserved.Contains(uri.Resource))
            {
                //resource previously subscribed 
                return new CoapResponse(message.MessageId, rmt, ResponseCodeType.NotAcceptable, message.Token);
            }

            //this point the resource is not being observed, so we can
            // #1 subscribe to it
            // #2 add to unobserved resources (means not coap observed)

            SubscriptionMetadata metadata = new SubscriptionMetadata()
            {
                IsEphemeral = true,
                Identity = session.Identity,
                Indexes = session.Indexes
            };

            string subscriptionUriString = await adapter.SubscribeAsync(uri.Resource, metadata);

            coapUnobserved.Add(uri.Resource);

            return new CoapResponse(message.MessageId, rmt, ResponseCodeType.Created, message.Token);
        }


        private async Task LoadDurables()
        {
            List<string> list = await adapter.LoadDurableSubscriptionsAsync(session.Identity);

            if (list != null)
            {
                coapUnobserved = new HashSet<string>(list);
            }
        }

        #region IDisposable Support


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    adapter.OnObserve -= Adapter_OnObserve;
                    adapter.Dispose();
                    coapObserved.Clear();
                    coapUnobserved.Clear();
                    coapObserved = null;
                    coapUnobserved = null;
                }

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CoapRequestDispatcher() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion






    }
}
