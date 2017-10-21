namespace Piraeus.Configuration.Settings
{
    public class ChannelSettings
    {
        public ChannelSettings()
        {

        }

        public ChannelSettings(TcpSettings tcp)
        {
            Tcp = tcp;
        }
        public TcpSettings Tcp { get; set; }
    }
}
