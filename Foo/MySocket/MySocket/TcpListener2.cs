using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySocket
{
    public class TcpListener2
    {
       
        public TcpListener2(IPEndPoint localEP)
        {

            socket = new Socket(localEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Connect(localEP);

        }

        private readonly int connectionTimeout = 10000;
        private Socket socket;

        public void Listen(EndPoint localEP)
        {
            Socket listener = new Socket(localEP.AddressFamily,SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEP);           
            listener.Listen(5000);
            listener.LingerState = new LingerOption(true, 2);
            listener.DontFragment = true;
            listener.ExclusiveAddressUse = false;
            listener.NoDelay = true;
            
        }


        public void Connect(EndPoint localEP)
        {           
            var signal = new AutoResetEvent(false);
            var e = new SocketAsyncEventArgs();
            e.RemoteEndPoint = localEP;
            e.Completed += (sender, eventArgs) => signal.Set();
            socket.ConnectAsync(e);
            
            if (!signal.WaitOne(connectionTimeout))
                throw new TimeoutException($"Connection to {localEP} could not be established in {connectionTimeout}");

            if (e.SocketError != SocketError.Success || !socket.Connected)
                throw new InvalidOperationException($"Could not connect to {localEP}: {e.SocketError}");

        }

        public void Read()
        {
            
            socket.Accept();
        }
    }
}
