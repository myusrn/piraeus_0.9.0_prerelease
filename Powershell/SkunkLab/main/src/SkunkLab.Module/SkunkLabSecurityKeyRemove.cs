using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Module
{
    [Cmdlet(VerbsCommon.Remove, "SkunkLabSecurityKey")]
    public class SkunkLabSecurityKeyRemove : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key to remove.", Mandatory = true)]
        public string KeyToRemove;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
        }
    }
}
