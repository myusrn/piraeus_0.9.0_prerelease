using Orleans;

namespace Piraeus.Grains
{
    public interface IAccessControlState : IGrainState
    {
        string PolicyId { get; set; }
    }
}
