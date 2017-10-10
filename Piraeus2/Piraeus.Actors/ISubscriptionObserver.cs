using Orleans;

namespace Piraeus.Actors
{
    public interface ISubscriptionObserver : IGrainObserver
    {
        void Notify(EventMessage message);
    }
}
