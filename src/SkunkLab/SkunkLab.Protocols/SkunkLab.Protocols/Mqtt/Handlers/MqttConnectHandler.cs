using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SkunkLab.Security.Identity;

namespace SkunkLab.Protocols.Mqtt.Handlers
{
    public class MqttConnectHandler : MqttMessageHandler
    {
        public MqttConnectHandler(MqttSession session, MqttMessage message)
            : base(session, message)
        {
           
        }

        public override async Task<MqttMessage> ProcessAsync()
        {
            if(Session.IsConnected)
            {
                Session.Disconnect(Message);
                return null;
            }

            ConnectMessage msg = Message as ConnectMessage;
            //authenticate
            string tokenType = msg.Username;
            string token = msg.Password;


            //wrong protocol version
            if(msg.ProtocolVersion != 4)
            {
                Session.ConnectResult = ConnectAckCode.UnacceptableProtocolVersion;
                return await Task.FromResult<MqttMessage>(new ConnectAckMessage(false, ConnectAckCode.UnacceptableProtocolVersion));                
            }

            //0-byte client id and clean session = 0
            if(msg.ClientId == null && !msg.CleanSession)
            {
                Session.ConnectResult = ConnectAckCode.IdentifierRejected;
                return await Task.FromResult<MqttMessage>(new ConnectAckMessage(false, ConnectAckCode.IdentifierRejected));
            }

            if(!Session.IsAuthenticated && (msg.Username == null || msg.Password == null))
            {
                Session.ConnectResult = ConnectAckCode.BadUsernameOrPassword;
                return await Task.FromResult<MqttMessage>(new ConnectAckMessage(false, ConnectAckCode.BadUsernameOrPassword));
            }

            //if not authn'd send back not authz'd

            if (!Session.IsAuthenticated)  //check for case where authentication was not done by channel
            {
                try
                {
                    Session.IsAuthenticated = Session.Authenticate(tokenType, token);
                    if (Session.IsAuthenticated)
                    {
                        IdentityDecoder decoder = new IdentityDecoder(Session.Config.IdentityClaimType, Session.Config.Indexes);
                        Session.Identity = decoder.Id;
                        Session.Indexes = decoder.Indexes;
                        Session.IsConnected = true;
                        Session.Connect(ConnectAckCode.ConnectionAccepted);
                        return await Task.FromResult<MqttMessage>(new ConnectAckMessage(false, ConnectAckCode.ConnectionAccepted));
                    }
                    else
                    {
                        Session.ConnectResult = ConnectAckCode.NotAuthorized;
                        return await Task.FromResult<MqttMessage>(new ConnectAckMessage(false, ConnectAckCode.NotAuthorized));
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError("MQTT authentication failed {0}", ex.Message);
                    Session.ConnectResult = ConnectAckCode.BadUsernameOrPassword;
                    return await Task.FromResult<MqttMessage>(new ConnectAckMessage(false, ConnectAckCode.BadUsernameOrPassword));
                }
            }
            else
            {
                Session.IsConnected = true;
                Session.Connect(ConnectAckCode.ConnectionAccepted);      
                return await Task.FromResult<MqttMessage>(new ConnectAckMessage(false, ConnectAckCode.ConnectionAccepted));
            }
        }
    }
}
