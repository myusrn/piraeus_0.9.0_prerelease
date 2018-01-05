using SkunkLab.Diagnostics.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Channels.Udp
{
    public class UdpServerChannel : UdpChannel
    {

        public UdpServerChannel(UdpClient listener, IPEndPoint remoteEP, CancellationToken token)
        {
            Id = "udp-" + Guid.NewGuid().ToString();
            this.client = listener;
            this.remoteEP = remoteEP;
            this.token = token;
            
        }

        
        private IPEndPoint remoteEP;
        //private IPEndPoint localEP;
        private UdpClient client;
        private ChannelState _state;
        private CancellationToken token;
        private bool disposedValue;

        public override event EventHandler<ChannelReceivedEventArgs> OnReceive;
        public override event EventHandler<ChannelCloseEventArgs> OnClose;
        public override event EventHandler<ChannelOpenEventArgs> OnOpen;
        public override event EventHandler<ChannelErrorEventArgs> OnError;
        public override event EventHandler<ChannelStateEventArgs> OnStateChange;
        public override event EventHandler<ChannelRetryEventArgs> OnRetry;
        public override event EventHandler<ChannelSentEventArgs> OnSent;
        public override event EventHandler<ChannelObserverEventArgs> OnObserve;


        public override string Id { get; internal set; }

        public override int Port { get; internal set; }

        public override bool IsEncrypted { get; internal set; }

        public override bool IsAuthenticated { get; internal set; }

        public override bool IsConnected
        {
            get
            {
                return ChannelState.Open == State;
            }
        }

        public override ChannelState State
        {
            get
            {
                return _state;
            }
            internal set
            {
                if(value != _state)
                {
                    OnStateChange?.Invoke(this, new ChannelStateEventArgs(Id, value));
                }

                _state = value;
            }
        }

        public override async Task OpenAsync()
        {
            try
            {                
                State = ChannelState.Open; //the channel is already open by the listener

                OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, null));
            }
            catch(Exception ex)
            {
                await Log.LogErrorAsync("UDP server channel open error {0}", ex.Message);
                State = ChannelState.Aborted;
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
            }
            
        }

        public override async Task ReceiveAsync()
        {

            //nothing implemented here because the listener will call AddMessageAsync and raise OnReceive
            //We do bind the remote endpoint to call SendAsync to the connected UDP client.

            await TaskDone.Done;

            //while (!token.IsCancellationRequested)
            //{
            //    try
            //    {
            //        UdpReceiveResult result = await client.ReceiveAsync();

            //        if (remoteEP == null)
            //        {
            //            remoteEP = result.RemoteEndPoint;
            //        }

            //        if (result.Buffer.Length > 0)
            //        {
            //            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, result.Buffer));
            //            OnObserve?.Invoke(this, new ChannelObserverEventArgs(null, null, result.Buffer));
            //        }
            //    }
            //    catch(Exception ex)
            //    {
            //        await Log.LogErrorAsync("UDP server channel receive error {0}", ex.Message);
            //        OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
            //    }
            //}
        }

        public override async Task AddMessageAsync(byte[] message)
        {
            //Raise the event received from the Protocol Adapter on the gateway
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, message));
            OnObserve?.Invoke(this, new ChannelObserverEventArgs(null, null, message));
            await TaskDone.Done;
        }

        public override async Task CloseAsync()
        {
            //nothing to do here because closing the client is closing the listener to all channels 
            //connected to the listener.

            //client.Close();            
        }

        protected void Disposing(bool dispose)
        {
            if (dispose & !disposedValue)
            {
                //if (client != null && IsConnected)
                //{
                //    client.Close();
                //}

                //client = null;
                disposedValue = true;
            }
        }

        public override void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }

        public override async Task SendAsync(byte[] message)
        {
            try
            {               
                await client.SendAsync(message, message.Length, remoteEP);
            }
            catch(Exception ex)
            {               
                await Log.LogErrorAsync("UDP server channel send error {0}", ex.Message);
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
            }
        }
    }
}
