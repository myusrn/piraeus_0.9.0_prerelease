using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace Piraeus.Actors
{
    public class Resource :  IResource, Grain<ResourceState>
    {
        public override Task OnActivateAsync()
        {
            
        }

        public override Task OnDeactivateAsync()
        {
            WriteStateAsync();
        }

    }
}
