using System;
using Capl.Authorization;

namespace Piraeus.GrainInterfaces
{
    [Serializable]
    public class AccessControlState
    {
        public AuthorizationPolicy Policy { get; set; }
    }
}
