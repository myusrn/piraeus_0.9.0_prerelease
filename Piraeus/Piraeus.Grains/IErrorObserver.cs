using System;
using Orleans;

namespace Piraeus.Grains
{
    public interface IErrorObserver : IGrainObserver
    {
        void NotifyError(string grainId, Exception error); 
    }
}
