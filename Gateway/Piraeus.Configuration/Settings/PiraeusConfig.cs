namespace Piraeus.Configuration.Settings
{
    public class PiraeusConfig
    {
        public PiraeusConfig()
        {

        }

        public PiraeusConfig(ChannelSettings channels, ProtocolSettings protocols, IdentitySettings identity, SecuritySettings security)
        {
            Channels = channels;
            Protocols = protocols;
            Identity = identity;
            Security = security;
        }

        public ChannelSettings Channels { get; set; }

        public ProtocolSettings Protocols { get; set; }

        public IdentitySettings Identity { get; set; }

        public SecuritySettings Security { get; set; }
    }
}
