namespace Piraeus.Configuration.Settings
{
    public class MqttSettings
    {
        public MqttSettings(double keepAliveSeconds = 180.0, double ackTimeoutSeconds = 2.0, double ackRandomFactor = 1.5, int maxRetransmit = 4, double maxLatencySeconds = 100.0)
        {
            KeepAliveSeconds = keepAliveSeconds;
            AckTimeoutSeconds = ackTimeoutSeconds;
            AckRandomFactor = ackRandomFactor;
            MaxRetransmit = maxRetransmit;
            MaxLatencySeconds = maxLatencySeconds;
        }
        public double KeepAliveSeconds { get; set; }

     
        public double AckTimeoutSeconds { get; set; }

        public double AckRandomFactor { get; set; }
        
        public int MaxRetransmit { get; set; }

        public double MaxLatencySeconds { get; set; }
    }
}
