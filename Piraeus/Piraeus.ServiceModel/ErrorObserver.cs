using System;
using Piraeus.Grains;

namespace Piraeus.ServiceModel
{
    public class ErrorObserver : IErrorObserver
    {
        public ErrorObserver()
        {
        }

        public event EventHandler<ErrorNotificationEventArgs> OnNotify;
        public void NotifyError(string grainId, Exception error)
        {
            OnNotify?.Invoke(this, new ErrorNotificationEventArgs(grainId, error));
        }
    }
}
