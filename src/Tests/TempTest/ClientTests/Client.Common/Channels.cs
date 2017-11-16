using SkunkLab.Channels;
using SkunkLab.Channels.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Common
{
    public class Channels
    {

        public static IChannel GetChannel(string endpoint, string securityToken, string subprotocol, WebSocketConfig config, CancellationToken token)
        {
            IChannel channel = CreateWebSocketChannel(endpoint, securityToken, subprotocol, config, token);
            OpenChannel(channel);
            Receive(channel);

            return channel;
        }

        public static IChannel GetChannel(string endpoint, string resourceUriString, string contentType, string securityToken)
        {
            IChannel channel = CreateRestChannel(endpoint, resourceUriString, contentType, securityToken);
            OpenChannel(channel);
            return channel;
        }

        public static IChannel GetChannel(string endpoint, string securityToken, IEnumerable<Observer> observers, CancellationToken token)
        {
            IChannel channel = CreateRestChannel(endpoint, securityToken, observers, token);
            OpenChannel(channel);
            Receive(channel);
            return channel;
        }

        /// <summary>
        /// Web Socket 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="endpoint"></param>
        /// <param name="securityToken"></param>
        /// <param name="subprotocol"></param>
        /// <param name="config"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static IChannel CreateWebSocketChannel(string endpoint, string securityToken, string subprotocol, WebSocketConfig config, CancellationToken token)
        {
            return ChannelFactory.Create(new Uri(endpoint), securityToken, subprotocol, config, token);
        }

        /// <summary>
        /// Http POST Request
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="resourceUriString"></param>
        /// <param name="contentType"></param>
        /// <param name="securityToken"></param>
        /// <returns></returns>
        private static IChannel CreateRestChannel(string endpoint, string resourceUriString, string contentType, string securityToken)
        {
            return ChannelFactory.Create(endpoint, resourceUriString, contentType, securityToken);
        }

       

        /// <summary>
        /// Long Polling
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="securityToken"></param>
        /// <param name="observers"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static IChannel CreateRestChannel(string endpoint, string securityToken, IEnumerable<Observer> observers, CancellationToken token)
        {
            return ChannelFactory.Create(endpoint, securityToken, observers, token);
        }

        private static void Receive(IChannel channel)
        {
            Task task = channel.ReceiveAsync();
            Task.WhenAll(task);
        }

        private static void OpenChannel(IChannel channel)
        {
            channel.OnOpen += Channel_OnOpen;
            channel.OnError += Channel_OnError;
            channel.OnClose += Channel_OnClose;
            Task task = channel.OpenAsync();
            Task.WaitAll(task);
        }

        private static void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Channel {0} is closed.", e.ChannelId);
            Console.ResetColor();
        }

        private static void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0} - {1}", e.ChannelId, e.Error.Message);
            Console.ResetColor();
        }

        private static void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Channel {0} is open", e.ChannelId);
            Console.ResetColor();
        }

        
    }
}
