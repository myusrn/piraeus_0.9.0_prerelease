using Piraeus.Adapters;
using SkunkLab.Channels;
using SkunkLab.Protocols.Coap;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FakeClient
{
    class Program
    {
        private static string audience = "http://www.skunklab.io/";
        private static string issuer = "http://www.skunklab.io/";
        private static string symmetricKey = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
        private static string nameClaimType = "http://www.skunklab.io/name";
        private static string roleClaimType = "http://www.skunklab.io/role";
        private static string nameClaimValue = Guid.NewGuid().ToString();
        private static string roleClaimVlaue = "pub";



        static ushort id;
        static void Main(string[] args)
        {
            string b64 = "MDwALmh0dHA6Ly93d3cuc2t1bmtsYWIuaW8vcmVzb3VyY2UxP2N0PXBsYWluL3RleHQAAWhlbGxvIG1xdHQ=";
            SkunkLab.Protocols.Mqtt.MqttMessage mqttM = SkunkLab.Protocols.Mqtt.MqttMessage.DecodeMessage(Convert.FromBase64String(b64));



            CreateIdentity();
            StartOrleansClient();
            FakeChannel channel = new FakeChannel();

            Console.Write("(1) CoAP (2) MQTT ? ");
            int protocolNo = Convert.ToInt32(Console.ReadLine());
            channel.ProtocolNo = protocolNo;
                       
            
            //CoapProtocolAdapter adapter = new CoapProtocolAdapter(new CoapConfig(null, "www.skunklab.io", CoapConfigOptions.NoResponse | CoapConfigOptions.Observe), channel);
            ProtocolAdapter adapter = GetAdapter(channel, protocolNo);
            adapter.Init();

            Console.WriteLine("Adapter created");
            //create a coap message to observe
            
            if(protocolNo == 1)
            {
                RunCoap(channel);
            }
            else
            {                
                RunMqtt(channel);
            }


            Console.WriteLine("Done");
            Console.ReadKey();
        }


        static void RunCoap(IChannel channel)
        {
            Uri observeUri = new Uri("coap://www.skunklab.io?r=http://www.skunklab.io/resource1");
            byte[] coapToken1 = SkunkLab.Protocols.Coap.CoapToken.Create().TokenBytes;
            id++;
            CoapRequest observeRequest = new CoapRequest(id, RequestMessageType.NonConfirmable, MethodType.GET, coapToken1, observeUri, MediaType.TextPlain);
            observeRequest.Observe = true;
            Task observeTask = channel.AddMessageAsync(observeRequest.Encode());
            Task.WhenAll(observeTask);

            Console.WriteLine("Observing");

            Thread.Sleep(500);
            //create a coap message to send to the resource
            byte[] coapToken2 = SkunkLab.Protocols.Coap.CoapToken.Create().TokenBytes;
            id++;
            CoapRequest pubRequest = new CoapRequest(id, RequestMessageType.NonConfirmable, MethodType.POST, coapToken2, observeUri, MediaType.TextPlain, Encoding.UTF8.GetBytes("hello"));

            Task pubTask = channel.AddMessageAsync(pubRequest.Encode());
            Task.WhenAll(pubTask);

            Console.WriteLine("Published");
        }

        public static void RunMqtt(IChannel channel)
        {
            string jwt = CreateJwt(symmetricKey, issuer, audience, 20.0, GetClaims());
            SkunkLab.Protocols.Mqtt.ConnectMessage con = new SkunkLab.Protocols.Mqtt.ConnectMessage("myclientId", "JWT", jwt, 40, true);

            Task t = channel.OpenAsync();
            Task.WhenAll(t);

            Thread.Sleep(1000);

            //send connect
            Task t1 = channel.AddMessageAsync(con.Encode());
            Task.WhenAll(t1);

            Thread.Sleep(1000);
            id++;

            //send subscribe
            Dictionary<string, SkunkLab.Protocols.Mqtt.QualityOfServiceLevelType> dict = new Dictionary<string, SkunkLab.Protocols.Mqtt.QualityOfServiceLevelType>();
            dict.Add("http://www.skunklab.io/resource1", SkunkLab.Protocols.Mqtt.QualityOfServiceLevelType.AtLeastOnce);
            SkunkLab.Protocols.Mqtt.SubscribeMessage sub = new SkunkLab.Protocols.Mqtt.SubscribeMessage(id, dict);
            Task t2 = channel.AddMessageAsync(sub.Encode());
            Task.WhenAll(t2);

            Thread.Sleep(1000);

            id++;

            int index = 0;

            while (index < 5)
            {
                id++;
                //send publish
                SkunkLab.Protocols.Mqtt.PublishMessage pub = new SkunkLab.Protocols.Mqtt.PublishMessage(false, SkunkLab.Protocols.Mqtt.QualityOfServiceLevelType.ExactlyOnce, false, id, "http://www.skunklab.io/resource1?ct=plain/text", Encoding.UTF8.GetBytes("hello mqtt"));
                byte[] encoded = pub.Encode();
                SkunkLab.Protocols.Mqtt.MqttMessage m = SkunkLab.Protocols.Mqtt.MqttMessage.DecodeMessage(encoded);
                Task t3 = channel.AddMessageAsync(pub.Encode());
                Task.WhenAll(t3);
                Console.WriteLine("Sent {0}", id);

                SkunkLab.Protocols.Mqtt.PublishAckMessage p = new SkunkLab.Protocols.Mqtt.PublishAckMessage(SkunkLab.Protocols.Mqtt.PublishAckType.PUBREL, id);
                Task tx = channel.AddMessageAsync(p.Encode());
                Task.WhenAll(tx);

                id++;
                if(index > 5)
                {
                    SkunkLab.Protocols.Mqtt.UnsubscribeMessage unsub = new SkunkLab.Protocols.Mqtt.UnsubscribeMessage(2323, new List<string>() { "http://www.skunklab.io/resource1" });
                    Task t4 = channel.AddMessageAsync(unsub.Encode());
                    Task.WhenAll(t4);
                }
                index++;
                Thread.Sleep(2000);
            }

        }

        static ProtocolAdapter GetAdapter(IChannel channel, int protocolNo)
        {
            if (protocolNo == 1)
            {
                return new CoapProtocolAdapter(new CoapConfig(null, "www.skunklab.io", CoapConfigOptions.NoResponse | CoapConfigOptions.Observe), channel);
            }
            else
            {
                SkunkLab.Protocols.Mqtt.MqttConfig mqttConfig = new SkunkLab.Protocols.Mqtt.MqttConfig(null);

                mqttConfig.IdentityClaimType = nameClaimType;
                mqttConfig.Indexes = new List<KeyValuePair<string, string>>();
                mqttConfig.Indexes.Add(new KeyValuePair<string, string>("http://wwww.skunklab.io/key1", "value1"));
                return new MqttProtocolAdapter(mqttConfig, channel);
            }
            
        }

        public static string CreateJwt(string key, string issuer, string audience, double lifetimeMinutes, List<Claim> claims)
        {
            JsonWebToken token = new JsonWebToken(new Uri(audience), key, issuer, claims, lifetimeMinutes);
            return token.ToString();
        }




        static void CreateIdentity()
        {
            List<Claim> claims = GetClaims();
            ClaimsIdentity identity = new ClaimsIdentity(claims);
            Thread.CurrentPrincipal = new ClaimsPrincipal(identity);
        }

        static void StartOrleansClient()
        {
            while (!Orleans.GrainClient.IsInitialized)
            {
                var config = Orleans.Runtime.Configuration.ClientConfiguration.LocalhostSilo();
                Orleans.GrainClient.Initialize(config);
            }

            Console.WriteLine("Orleans client started");
        }

        static List<Claim> GetClaims()
        {
            return new List<Claim>()
            {
                new Claim(nameClaimType, nameClaimValue),
                new Claim(roleClaimType, roleClaimVlaue)
            };
        }
    }

    
}
