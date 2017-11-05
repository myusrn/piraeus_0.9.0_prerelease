namespace Piraeus.Configuration.Settings
{
    public class ChannelSettings
    {
        public ChannelSettings()
        {

        }

        public ChannelSettings(WebSocketSettings websocket, TcpSettings tcp)
        {
            WebSocket = websocket;
            Tcp = tcp;
        }

        public WebSocketSettings WebSocket { get; set; }
        public TcpSettings Tcp { get; set; }
    }
}
