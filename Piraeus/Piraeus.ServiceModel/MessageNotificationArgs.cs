using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Piraeus.Core.Messaging;

namespace Piraeus.ServiceModel
{
    public class MessageNotificationArgs : EventArgs
    {
        public MessageNotificationArgs(EventMessage message)
        {
            Message = message;
            Timestamp = DateTime.UtcNow;
        }

        public EventMessage Message { get; internal set; }

        public DateTime? Timestamp { get; internal set; }
    }
}
