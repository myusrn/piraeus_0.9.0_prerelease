using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
namespace Piraeus.Grains
{
    public interface ISubscription : IGrainWithStringKey
    {
    }
}
