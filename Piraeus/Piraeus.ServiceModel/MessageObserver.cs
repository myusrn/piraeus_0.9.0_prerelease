using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Piraeus.Core.Messaging;
using Piraeus.Grains;

namespace Piraeus.ServiceModel
{
    public class MessageObserver : IMessageObserver
    {
        public MessageObserver()
        {
        }

        public event EventHandler<MessageNotificationArgs> OnNotify;

        public void Notify(EventMessage message)
        {
            OnNotify?.Invoke(this, new MessageNotificationArgs(message));
        }
    }
}
