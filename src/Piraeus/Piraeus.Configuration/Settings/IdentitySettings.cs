namespace Piraeus.Configuration.Settings
{
    public class IdentitySettings
    {
        public IdentitySettings()
        {

        }

        public IdentitySettings(ClientIdentity client, ServiceIdentity service)
        {
            Client = client;
            Service = service;
        }
        public ClientIdentity Client { get; set; }

        public ServiceIdentity Service { get; set; }
    }
}
