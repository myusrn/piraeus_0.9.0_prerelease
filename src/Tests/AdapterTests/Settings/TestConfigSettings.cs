using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Piraeus.Configuration.Settings;

namespace AdapterTests.Settings
{
    public class TestConfigSettings
    {
        public static PiraeusConfig GetDefaultConfig()
        {
            ChannelSettings channelSettings = new ChannelSettings(new WebSocketSettings(), new TcpSettings());
            ProtocolSettings protocolSettings = new ProtocolSettings(new MqttSettings(), new CoapSettings("www.example.org", true, true));
            ClientIdentity clientIdentity = new ClientIdentity();
            clientIdentity.IdentityClaimType = "http://www.skunklab.io/name";

            clientIdentity.Indexes = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("index1", "key1"), new KeyValuePair<string, string>("index2", "key2") };
            ServiceIdentity serviceIdentity = new ServiceIdentity();
            serviceIdentity.Claims = new List<System.Security.Claims.Claim>() { new System.Security.Claims.Claim("http://www.skunklab.io/claim1", "claim1"), new System.Security.Claims.Claim("http://www.skunklab.io/claim2", "claim2") };
            IdentitySettings identitySettings = new IdentitySettings(clientIdentity, serviceIdentity);
            ClientSecurity clientSecurity = new ClientSecurity("JWT", GenerateKey(), "http://www.skunklab.io", "http://www.skunklab.io");
            
            SecuritySettings securitySettings = new SecuritySettings(clientSecurity, null);
            return new PiraeusConfig(channelSettings, protocolSettings, identitySettings, securitySettings);                 
        }

        private static string GenerateKey()
        {
            Random ran = new Random();
            byte[] buffer = new byte[32];
            ran.NextBytes(buffer);

            return Convert.ToBase64String(buffer);
        }
    }
}
