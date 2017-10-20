using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Channels
{
    //public delegate void ChannelReceivedEventHandler(object sender, ChannelReceivedEventArgs args);
    //public delegate void ChannelCloseEventHandler(object sender, ChannelCloseEventArgs args);
    //public delegate void ChannelOpenEventHandler(object sender, ChannelOpenEventArgs args);
    //public delegate void ChannelErrorEventHandler(object sender, ChannelErrorEventArgs args);
    //public delegate void ChannelStateEventHandler(object sender, ChannelStateEventArgs args);
    //public delegate void ChannelRetryEventHandler(object sender, ChannelRetryEventArgs args);
    //public delegate void ChannelSentEventHandler(object sender, ChannelSentEventArgs args);

    public interface IChannel : IDisposable
    {
        event EventHandler<ChannelReceivedEventArgs> OnReceive;
        event EventHandler<ChannelCloseEventArgs> OnClose;
        event EventHandler<ChannelOpenEventArgs> OnOpen;
        event EventHandler<ChannelErrorEventArgs> OnError;
        event EventHandler<ChannelStateEventArgs> OnStateChange;
        event EventHandler<ChannelRetryEventArgs> OnRetry;
        event EventHandler<ChannelSentEventArgs> OnSent;
        event EventHandler<ChannelObserverEventArgs> OnObserve;
        

        bool IsConnected { get; }
        string Id { get; }

        int Port { get; }

        ChannelState State { get; }

        bool IsEncrypted { get; }

        bool IsAuthenticated { get; }
        
        Task OpenAsync();

        Task SendAsync(byte[] message);
        
        Task CloseAsync();

        Task ReceiveAsync();
        Task AddMessageAsync(byte[] message);
    }
}
