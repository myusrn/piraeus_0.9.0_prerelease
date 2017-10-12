using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Channels;

namespace Piraeus.Gateway.Adapters
{
    public abstract class ProtocolAdapter
    {
        public abstract void Init();
        public abstract IChannel Channel { get; set; }

        //adapter events


    }
}
