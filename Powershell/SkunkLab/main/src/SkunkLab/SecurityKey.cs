using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Common;
namespace SkunkLab
{
    [Cmdlet(VerbsCommon.Add, "SecurityKey")]
    public class AddSecurityKey : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        public bool Admin;

        public string Description;

        public int LifetimeMinutes;

        public ClaimLiteral[] Claims;

        protected override void ProcessRecord()
        { 
        }
    }
    
    [Cmdlet(VerbsCommon.Remove, "SecurityKey")]
    public class RemoveSecurityKey : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;
    }

    [Cmdlet(VerbsCommon.Get, "SecurityKey")]
    public class GetSecurityKey : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        public string Id;

        protected override void ProcessRecord()
        {
        }
    }

    [Cmdlet(VerbsData.Update, "SecurityKey")]
    public class UpdateSecurityKey : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        public string Id;

        public bool Admin;

        public string Description;

        public int LifetimeMinutes;

        public ClaimLiteral[] Claims;

        protected override void ProcessRecord()
        {            
        }
    }

    [Cmdlet(VerbsCommon.Get, "SecurityKeyList")]
    public class SecurityKeyList : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security key for access.", Mandatory = true)]
        public string Key;

        protected override void ProcessRecord()
        {
        }
    }


}
