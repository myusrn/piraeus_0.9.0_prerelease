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
    public class SocketManager
    {
        public static Socket GetAcceptingSocketForEndpoint(IPEndPoint address)
        {
            var s = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                // Prep the socket so it will reset on close
                s.LingerState = new LingerOption(true, 0);
                s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                // And bind it to the address
                s.Bind(address);
            }
            catch (Exception)
            {
                CloseSocket(s);
                throw;
            }
            return s;
        }

        public static void Connect(Socket s, IPEndPoint endPoint, TimeSpan connectionTimeout)
        {
            var signal = new AutoResetEvent(false);
            var e = new SocketAsyncEventArgs();
            e.RemoteEndPoint = endPoint;
            e.Completed += (sender, eventArgs) => signal.Set();
            s.ConnectAsync(e);

            if (!signal.WaitOne(connectionTimeout))
                throw new TimeoutException($"Connection to {endPoint} could not be established in {connectionTimeout}");

            if (e.SocketError != SocketError.Success || !s.Connected)
                throw new InvalidOperationException($"Could not connect to {endPoint}: {e.SocketError}");
        }

        

        internal static void CloseSocket(Socket s)
        {
            if (s == null)
            {
                return;
            }

            

            try
            {
                s.Shutdown(SocketShutdown.Both);
            }
            catch (ObjectDisposedException)
            {
                // Socket is already closed -- we're done here
                return;
            }
            catch (Exception)
            {
                // Ignore
            }
            try
            {
                s.Disconnect(false);
            }
            catch (Exception)
            {
                // Ignore
            }
            try
            {
                s.Dispose();
            }
            catch (Exception)
            {
                // Ignore
            }
        }
    }
}
