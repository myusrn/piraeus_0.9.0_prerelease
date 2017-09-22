using System;

namespace SkunkLab.Protocols.Coap
{
    [Flags]
    public enum CoapConfigOptions
    {
        Observe = 1,
        NoResponse = 2
    }
}
