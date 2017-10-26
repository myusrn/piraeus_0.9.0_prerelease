using System;
using Capl.Authorization;

namespace Piraeus.Grains
{
    [Serializable]
    public class AccessControlState
    {
        public AuthorizationPolicy Policy { get; set; }
    }
}
