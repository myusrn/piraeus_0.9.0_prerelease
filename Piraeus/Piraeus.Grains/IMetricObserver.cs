using Orleans;
using Piraeus.Core.Messaging;

namespace Piraeus.Grains
{
    public interface IMetricObserver : IGrainObserver
    {
        void NotifyMetrics(CommunicationMetrics metrics);
    }
}
