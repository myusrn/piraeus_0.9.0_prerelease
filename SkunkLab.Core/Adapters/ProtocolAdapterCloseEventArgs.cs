using System;

namespace SkunkLab.Core.Adapters
{
    public class ProtocolAdapterCloseEventArgs : EventArgs
    {
        public ProtocolAdapterCloseEventArgs(string channelId)
        {
            ChannelId = channelId;
        }

        public string ChannelId { get; set; }
    }
}
