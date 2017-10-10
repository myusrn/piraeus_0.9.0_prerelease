using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.ServiceModel
{
    public class ErrorNotificationEventArgs : EventArgs
    {
        public ErrorNotificationEventArgs()
        {
        }

        public ErrorNotificationEventArgs(string id, Exception error)
        {
            Id = id;
            Error = error;
        }

        public string Id { get; internal set; }

        public Exception Error { get; internal set; }
    }
}
