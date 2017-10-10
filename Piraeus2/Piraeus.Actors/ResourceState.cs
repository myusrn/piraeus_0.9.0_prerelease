using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace Piraeus.Actors
{
    public class ResourceState : IGrainState
    {
        /// <summary>
        /// Gets or sets the metadata associated with the topic.
        /// </summary>
        //ResourceMetadata Metadata { get; set; }

        Dictionary<string, ISubscription> Subscriptions { get; set; }

        long MessageCount { get; set; }

        long BytesReceived { get; set; }


    }
}
