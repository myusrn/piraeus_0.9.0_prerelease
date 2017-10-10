using Capl.Authorization;

namespace Piraeus.Grains
{
    public class AccessControlState
    {
        public AuthorizationPolicy Policy { get; set; }
    }
}
