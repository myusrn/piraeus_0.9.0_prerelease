namespace Piraeus.Configuration.Settings
{
    public class ProtocolSettings
    {
        public ProtocolSettings()
        {

        }

        public ProtocolSettings(MqttSettings mqttSettings, CoapSettings coapSettings)
        {
            Mqtt = mqttSettings;
            Coap = coapSettings;
        }
        public MqttSettings Mqtt { get; set; }

        public CoapSettings Coap { get; set; }
    }
}
