using System;

namespace Piraeus.Core.Metadata
{
    [Serializable]
    public enum SecurityTokenType
    {
        None,
        Jwt,
        Swt,
        X509
    }
}
