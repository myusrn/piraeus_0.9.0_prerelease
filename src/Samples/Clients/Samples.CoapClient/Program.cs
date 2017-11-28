using Piraeus.Clients.Coap;
using SkunkLab.Channels;
using SkunkLab.Channels.WebSocket;
using SkunkLab.Protocols.Coap;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samples.CoapClient
{
    class Program
    {

        static string audience = "http://www.skunklab.io/";
        static string issuer = "http://www.skunklab.io/";
        static string symmetricKey = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
        static string nameClaimType = "http://www.skunklab.io/name";
        static string roleClaimType = "http://www.skunklab.io/role";

        static string endpoint = "ws://localhost:4163/api/connect";
        //static string endpoint = "ws://localhost:3111/api/connect";
        static IChannel channel;
        static CancellationTokenSource source;
        static bool abSwitch;
        static string resourceA = "http://www.skunklab.io/resourcea";
        static string resourceB = "http://www.skunklab.io/resourceb";

        static void Main(string[] args)
        {
            source = new CancellationTokenSource();

            Console.Write("Select a role (A/B) ? ");
            abSwitch = Console.ReadLine().ToUpperInvariant() == "A";

            string token = GetSecurityToken(abSwitch ? "A" : "B");
            channel = ChannelFactory.Create(new Uri(endpoint), token, "coapv1", new WebSocketConfig(), source.Token);
           


            channel.OnOpen += Channel_OnOpen;
            channel.OnReceive += Channel_OnReceive;
            channel.OnError += Channel_OnError;
            channel.OnStateChange += Channel_OnStateChange;
            Task openTask = channel.OpenAsync();
            Task.WaitAll(openTask);

            Console.WriteLine("Waiting the channel state");
            Console.ReadKey();



           

            //create channel and open it
            CreateWebSocketChannel();

            //create the Piraeus client the uses the channel
            CoapConfig config = new CoapConfig(null, "www.skunklab.io", CoapConfigOptions.NoResponse | CoapConfigOptions.Observe);
            PiraeusCoapClient coapClient = new PiraeusCoapClient(config, channel, null);

            //create an observer for a resource
            string subResource = abSwitch ? resourceB : resourceA;
            Task observeTask = coapClient.ObserveAsync(subResource, new Action<CodeType, string, byte[]>(ObserveResource));
            Task.WhenAll(observeTask);

            Thread.Sleep(3000);

            Console.WriteLine("Press any key to send");
            Console.ReadKey();

            //send messages over coap to a resource
            string pubResource = abSwitch ? resourceA : resourceB;
            string message = "Message 1";
            Task pubTask = coapClient.PublishAsync(pubResource, "text/plain", Encoding.UTF8.GetBytes(message), false, new Action<CodeType, string, byte[]>(Response));
            Task.WhenAll(pubTask);
            Console.WriteLine("Message is sent");

            Thread.Sleep(3000);

            Console.WriteLine("waiting...");
            Console.ReadKey();

            source.Cancel();
            Console.ReadKey();

        }

        private static void ObserveResource(CodeType code, string contentType, byte[] message)
        {
            Console.WriteLine("Code = {0} Message = {1}", code, Encoding.UTF8.GetString(message));
        }

        private static void Response(CodeType code, string contentType, byte[] message)
        {
            Console.WriteLine("Coap Response Code = {0}", code);
        }

        private static void CreateWebSocketChannel()
        {
            string token = GetSecurityToken(abSwitch ? "A" : "B");
            channel = ChannelFactory.Create(new Uri(endpoint), token, "coapv1", new WebSocketConfig(), source.Token);
            channel.OnOpen += Channel_OnOpen;
            channel.OnReceive += Channel_OnReceive;
            channel.OnError += Channel_OnError;
            channel.OnStateChange += Channel_OnStateChange;
            Task task = OpenChannel(channel);
            Task.WaitAll(task);
            
        }

        private static void Channel_OnStateChange(object sender, ChannelStateEventArgs e)
        {
            Console.WriteLine("State = {0}", e.State);
            if(e.State == ChannelState.Open)
            {
                Task task = channel.ReceiveAsync();
                Task.WhenAll(task);
            }
        }

        private static void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            Console.WriteLine("Error {0}", e.Error.Message);
        }

        private static Task OpenChannel(IChannel channel)
        {
            TaskCompletionSource<Task> tcs = new TaskCompletionSource<Task>();
            Task task = channel.OpenAsync();
            tcs.SetResult(null);
            return tcs.Task;        

        }

        private static void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            Console.WriteLine("Message = {0}", Encoding.UTF8.GetString(e.Message));
        }

        private static void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            //Console.WriteLine("Channel is {0}", "Open");
           
        }

        static string GetSecurityToken(string role)
        {
            string roleClaimValue = abSwitch ? "A" : "B";
            List<Claim> claims = new List<Claim>()
            {
                new Claim(nameClaimType, Guid.NewGuid().ToString()),
                new Claim(roleClaimType, roleClaimValue)
            };

            JsonWebToken token = new JsonWebToken(new Uri(audience), symmetricKey, issuer, claims, 20.0);
            return token.ToString();
        }

        



    }
}
