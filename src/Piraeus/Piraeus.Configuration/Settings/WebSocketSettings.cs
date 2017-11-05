namespace Piraeus.Configuration.Settings
{
    public class WebSocketSettings
    {
       
        public WebSocketSettings(int maxIncomingMessageSize = 0x400000, int receiveLoopBufferSize = 0x2000, int sendBufferSize = 0x2000, double closeTimeoutMilliseconds = 250.0)
        {
            MaxIncomingMessageSize = maxIncomingMessageSize;
            ReceiveLoopBufferSize = receiveLoopBufferSize;
            SendBufferSize = sendBufferSize;
            CloseTimeoutMilliseconds = closeTimeoutMilliseconds;
        }

        public int MaxIncomingMessageSize { get; set; }

        public int ReceiveLoopBufferSize { get; set; }

        public int SendBufferSize { get; set; }

        public double CloseTimeoutMilliseconds { get; set; }


    }
}
