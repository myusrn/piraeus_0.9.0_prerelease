using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Piraeus.Core.Messaging;
using Piraeus.Grains;

namespace Piraeus.ServiceModel
{
    public class MetricObserver : IMetricObserver
    {
        public MetricObserver()
        {
        }

        public event EventHandler<MetricNotificationEventArgs> OnNotify;

        public void NotifyMetrics(CommunicationMetrics metrics)
        {
            OnNotify?.Invoke(this, new MetricNotificationEventArgs(metrics));
        }
    }
}
