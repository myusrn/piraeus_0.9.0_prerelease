namespace Piraeus.Configuration.Settings
{
    public class SecuritySettings
    {
        public SecuritySettings()
        {

        }

        public SecuritySettings(ClientSecurity client, ServiceSecurity service = null)
        {
            Client = client;
            Service = service;
        }
        public ClientSecurity Client { get; set; }

        public ServiceSecurity Service { get; set; }
    }
}
