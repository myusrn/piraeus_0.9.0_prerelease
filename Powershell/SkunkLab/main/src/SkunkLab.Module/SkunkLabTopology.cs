using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using SkunkLab.Common;

namespace SkunkLab.Module
{
    [Cmdlet(VerbsCommon.Add, "Topology")]
    public class AddTopology : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        [Parameter(HelpMessage = "Topology of topic and subscriptions,", Mandatory = true)]
        public Topology Topology;

        protected override void ProcessRecord()
        {

        }

    }

    [Cmdlet(VerbsCommon.Get, "Topology")]
    public class GetTopology : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        [Parameter(HelpMessage = "Topic URI to access topology.", Mandatory = true)]
        public string Topic;

        protected override void ProcessRecord()
        {

        }

    }
    


}
