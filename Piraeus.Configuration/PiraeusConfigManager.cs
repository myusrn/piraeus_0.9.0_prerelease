using System;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Piraeus.Configuration.Settings;

namespace Piraeus.Configuration
{
    public class PiraeusConfigManager
    {
        static PiraeusConfigManager()
        {
            ChannelSettings channelSettings = null;
            ProtocolSettings protocolSettings = null;
            IdentitySettings identitySettings = null;
            SecuritySettings securitySettings = null;


            PiraeusSection section = ConfigurationManager.GetSection("piraeus") as PiraeusSection;
            if (section == null)
                throw new ConfigurationErrorsException("Piraeus configuration section not found.");

            if (section.Channels != null)
            {
                WebSocketSettings websocket = null;
                TcpSettings tcp = null;
                if(section.Channels.WebSocket != null)
                {
                    websocket = new WebSocketSettings(section.Channels.WebSocket.MaxIncomingMessageSize,
                                                                section.Channels.WebSocket.ReceiveLoopBufferSize,
                                                                section.Channels.WebSocket.SendBufferSize,
                                                                section.Channels.WebSocket.CloseTimeoutMilliseconds);
                }

                if (section.Channels.TCP != null)
                {
                    X509Certificate2 tcpCertificate = null;
                    string pskIdentity = null;
                    byte[] pskKey = null;
                    bool prefix = section.Channels.TCP.UseLengthPrefix;
                    bool authn = false;
                    int? blockSize = section.Channels.TCP.BlockSize;
                    int? maxBufferSize = section.Channels.TCP.MaxBufferSize;

                    if (section.Channels.TCP.PSK != null)
                    {
                        pskIdentity = section.Channels.TCP.PSK.Identity;
                        pskKey = Convert.FromBase64String(section.Channels.TCP.PSK.Key);
                    }
                    if (section.Channels.TCP.Certificate != null)
                    {
                        //get the certificate
                        authn = section.Channels.TCP.Certificate.AuthenticateServer;
                        tcpCertificate = GetCertificate(section.Channels.TCP.Certificate.Store, section.Channels.TCP.Certificate.Location, section.Channels.TCP.Certificate.Thumbprint);
                    }

                    tcp = new TcpSettings(prefix, authn, tcpCertificate, pskIdentity, pskKey, blockSize, maxBufferSize);
                }

                channelSettings = new ChannelSettings(websocket, tcp);
            }

            CoapSettings coapSettings = null;
            if (section.Protocols != null && section.Protocols.Coap != null)
            {

                coapSettings = new CoapSettings(section.Protocols.Coap.HostName,
                                                section.Protocols.Coap.AutoRetry,
                                                section.Protocols.Coap.ObserveOption,
                                                section.Protocols.Coap.NoResponseOption,
                                                section.Protocols.Coap.KeepAliveSeconds,
                                                section.Protocols.Coap.AckTimeoutSeconds,
                                                section.Protocols.Coap.AckRandomFactor,
                                                section.Protocols.Coap.MaxRetransmit,
                                                section.Protocols.Coap.MaxLatencySeconds,
                                                section.Protocols.Coap.NStart,
                                                section.Protocols.Coap.DefaultLeisure,
                                                section.Protocols.Coap.ProbingRate);
            }

            MqttSettings mqttSettings = null;
            if (section.Protocols != null && section.Protocols.Mqtt != null)
            {
                mqttSettings = new MqttSettings(section.Protocols.Mqtt.KeepAliveSeconds,
                                        section.Protocols.Mqtt.AckTimeoutSeconds,
                                        section.Protocols.Mqtt.AckRandomFactor,
                                        section.Protocols.Mqtt.MaxRetransmit, section.Protocols.Mqtt.MaxLatencySeconds);
            }

            protocolSettings = new ProtocolSettings(mqttSettings, coapSettings);

            ClientIdentity clientIdentity = null;
            if (section.Identity != null && section.Identity.Client != null)
            {
                if (section.Identity.Client.Indexes != null)
                {
                    clientIdentity = new ClientIdentity(section.Identity.Client.IdentityClaimType, section.Identity.Client.Indexes.GetIndexes());
                }
                else
                {
                    clientIdentity = new ClientIdentity(section.Identity.Client.IdentityClaimType, null);
                }
            }

            ServiceIdentity serviceIdentity = null;

            if (section.Identity != null && section.Identity.Service != null && section.Identity.Service.Claims != null)
            {
                serviceIdentity = new ServiceIdentity(section.Identity.Service.Claims.GetServiceClaims());
            }

            identitySettings = new IdentitySettings(clientIdentity, serviceIdentity);

            ClientSecurity clientSecurity = null;
            if (section.Security != null && section.Security.Client != null)
            {
                if (section.Security.Client.SymmetricKey != null)
                {
                    clientSecurity = new ClientSecurity(section.Security.Client.SymmetricKey.SecurityTokenType,
                                            section.Security.Client.SymmetricKey.SharedKey,
                                            section.Security.Client.SymmetricKey.Issuer, section.Security.Client.SymmetricKey.Audience);
                }
            }

            ServiceSecurity serviceSecurity = null;
            if (section.Security.Service != null && section.Security.Service != null)
            {
                if (section.Security.Service.AsymmetricKey != null)
                {
                    X509Certificate2 serviceCertificate = GetCertificate(section.Security.Service.AsymmetricKey.Store, section.Security.Service.AsymmetricKey.Location, section.Security.Service.AsymmetricKey.Thumbprint);
                    serviceSecurity = new ServiceSecurity(serviceCertificate);
                }
            }

            securitySettings = new SecuritySettings(clientSecurity, serviceSecurity);

            config = new PiraeusConfig(channelSettings, protocolSettings, identitySettings, securitySettings);
        }

        private static PiraeusConfig config;

        public static PiraeusConfig Settings
        {
            get { return config; }
        }
        

        private static X509Certificate2 GetCertificate(string store, string location, string thumbprint)
        {
            thumbprint = Regex.Replace(thumbprint, @"[^\da-fA-F]", string.Empty).ToUpper();


            StoreName storeName = (StoreName)Enum.Parse(typeof(StoreName), store, true);
            StoreLocation storeLocation= (StoreLocation)Enum.Parse(typeof(StoreLocation), location, true);
            

            X509Store certStore = new X509Store(storeName, storeLocation);
            certStore.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection coll = certStore.Certificates;

            X509Certificate2Collection certCollection =
              certStore.Certificates.Find(X509FindType.FindByThumbprint,
                                      thumbprint.ToUpper(), false);
            X509Certificate2Enumerator enumerator = certCollection.GetEnumerator();
            X509Certificate2 cert = null;
            while (enumerator.MoveNext())
            {
                cert = enumerator.Current;
            }
            return cert;

        }

    }
}
