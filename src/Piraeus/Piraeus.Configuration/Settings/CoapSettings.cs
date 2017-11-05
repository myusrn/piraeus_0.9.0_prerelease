namespace Piraeus.Configuration.Settings
{
    public class CoapSettings : MqttSettings
    {
        public CoapSettings(string hostname, bool observeOption, bool noresponseOption, bool autoRetry = false, double keepAliveSeconds = 180.0, double ackTimeoutSeconds = 2.0, 
            double ackRandomFactor = 1.5, int maxRetransmit = 4, double maxLatencySeconds = 100.0, 
            int nstart = 1, double defaultLeisure = 4.0, double probingRate = 1.0)
        {
            HostName = hostname;
            ObserveOption = observeOption;
            NoResponseOption = noresponseOption;
            AutoRetry = autoRetry;
            KeepAliveSeconds = keepAliveSeconds;
            AckTimeoutSeconds = ackTimeoutSeconds;
            AckRandomFactor = ackRandomFactor;
            MaxRetransmit = maxRetransmit;
            MaxLatencySeconds = maxLatencySeconds;
            NStart = nstart;
            DefaultLeisure = defaultLeisure;
            ProbingRate = probingRate;
        }

        public string HostName { get; set; }
        
        public bool AutoRetry { get; set; }

        public bool ObserveOption { get; set; }
        public bool NoResponseOption { get; set; }
        
        public int NStart { get; set; }

        public double DefaultLeisure { get; set; }

        public double ProbingRate { get; set; }
    }
}
