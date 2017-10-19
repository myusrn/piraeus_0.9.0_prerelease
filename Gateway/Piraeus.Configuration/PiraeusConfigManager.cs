using System.Collections.Generic;
using System.Configuration;
using Piraeus.Configuration.Security;

namespace Piraeus.Configuration
{
    public class PiraeusConfigManager
    {
        static PiraeusConfigManager()
        {
            PiraeusSection section = ConfigurationManager.GetSection("piraeus") as PiraeusSection;
            if (section == null)
                throw new ConfigurationErrorsException("Piraeus configuration section not found.");

            if (section.Protocols != null && section.Protocols.Coap != null)
            {
                coapAckRandomFactor = section.Protocols.Coap.AckRandomFactor;
                coapAckTimeout = section.Protocols.Coap.AckTimeoutSeconds;
                coapAutoRetry = section.Protocols.Coap.AutoRetry;
                coapDefaultLeisure = section.Protocols.Coap.DefaultLeisure;
                coapHostName = section.Protocols.Coap.HostName;
                coapKeepAlive = section.Protocols.Coap.KeepAliveSeconds;
                coapMaxLatency = section.Protocols.Coap.MaxLatencySeconds;
                coapMaxRetransmit = section.Protocols.Coap.MaxRetransmit;
                coapNStart = section.Protocols.Coap.NStart;
                coapProbingRate = section.Protocols.Coap.ProbingRate;
            }

            if (section.Protocols != null && section.Protocols.Mqtt != null)
            {
                mqttAckRandomFactor = section.Protocols.Mqtt.AckRandomFactor;
                mqttAckTimeout = section.Protocols.Mqtt.AckTimeoutSeconds;
                mqttKeepAlive = section.Protocols.Mqtt.KeepAliveSeconds;
                mqttMaxLatency = section.Protocols.Mqtt.MaxLatencySeconds;
                mqttMaxRetransmit = section.Protocols.Mqtt.MaxRetransmit;
            }

            if(section.Identity != null && section.Identity.Client != null)
            {
                clientIdentityClaimType = section.Identity.Client.IdentityClaimType;

                if(section.Identity.Client.Indexes != null)
                {
                    clientIndexes = section.Identity.Client.Indexes.GetIndexes();
                }
            }

            if(section.Identity != null && section.Identity.Service != null && section.Identity.Service.Claims != null)
            {                
                serviceClaims = section.Identity.Service.Claims.GetServiceClaims();
            }

            if(section.Security != null && section.Security.Client != null)
            {
                if(section.Security.Client.SymmetricKey != null)
                {
                    clientSymmetricKeyParams = new SymmetricKeyParameters(section.Security.Client.SymmetricKey.SecurityTokenType,
                        section.Security.Client.SymmetricKey.SharedKey,
                        section.Security.Client.SymmetricKey.Issuer,
                        section.Security.Client.SymmetricKey.Audience);                    
                }
            }

            if(section.Security.Service != null && section.Security.Service != null)
            {
                
                if (section.Security.Service.AsymmetricKey != null)
                {
                    serviceAsymmetricKeyParams = new AsymmetricKeyParameters(section.Security.Service.AsymmetricKey.Store,
                        section.Security.Service.AsymmetricKey.Location,
                        section.Security.Service.AsymmetricKey.Thumbprint);
                }
            }
        }



        #region CoAP private variables
        private static string coapHostName;
        private static double coapAckRandomFactor;
        private static double coapAckTimeout;
        private static bool coapAutoRetry;
        private static double coapDefaultLeisure;
        private static double coapKeepAlive;
        private static double coapMaxLatency;
        private static int coapMaxRetransmit;
        private static double coapProbingRate;
        private static int coapNStart;
        #endregion

        #region MQTT private variables

        private static double mqttAckRandomFactor;
        private static double mqttAckTimeout;
        private static double mqttKeepAlive;
        private static double mqttMaxLatency;
        private static int mqttMaxRetransmit;

        #endregion

        #region Identity private variable
        private static string clientIdentityClaimType;
        private static List<KeyValuePair<string, string>> clientIndexes;
        private static List<KeyValuePair<string, string>> serviceClaims;
        #endregion

        private static SymmetricKeyParameters clientSymmetricKeyParams;
        private static AsymmetricKeyParameters serviceAsymmetricKeyParams;

        #region CoAP parameters
        public static string CoapHostName
        {
            get { return coapHostName; }
        }

        public double CoapAckTimeout
        { get { return coapAckTimeout; } }

        public static double CoapAckRandomFactor
        {
            get { return coapAckRandomFactor; }
        }

        public static bool CoapAutoRetry
        {
            get { return coapAutoRetry; }
        }

        public static double CoapDefaultLeisure
        {
            get { return coapDefaultLeisure; }
        }

        public static double CoapKeepAlive
        {
            get { return coapKeepAlive; }
        }

        private static double CoapMaxLatency
        {
            get { return coapMaxLatency; }
        }

        public static int CoapMaxRetransmit
        {
            get { return coapMaxRetransmit; }
        }

        public static double CoapProbingRate
        {
            get { return coapProbingRate; }
        }

        public static int CoapNStart
        {
            get { return coapNStart; }
        }

        #endregion

        #region MQTT parameters
        public static double MqttAckRandomFactor
        { get { return mqttAckRandomFactor; } }

        public static double MqttAckTimeout
        { get { return mqttAckTimeout; } }

        public static double MqttKeepAlive
        { get { return mqttKeepAlive; } }

        public static double MqttMaxLatency
        { get { return mqttMaxLatency; } }

        public static double MqttMaxRetransmit
        { get { return mqttMaxRetransmit; } }

        #endregion

        #region Identity parameters
        public static string ClientIdentityClaimType
        { get { return clientIdentityClaimType; } }

        public static List<KeyValuePair<string,string>> ClientIndexes
        { get { return clientIndexes; } }

        public static List<KeyValuePair<string,string>> ServiceClaims
        { get { return serviceClaims; } }
        #endregion

        public static SymmetricKeyParameters ClientSymmetricKeyParams
        {
            get { return clientSymmetricKeyParams; }
        }        

        public static AsymmetricKeyParameters ServiceAsymmetricKeyParams
        { get { return serviceAsymmetricKeyParams; } }

    }
}
