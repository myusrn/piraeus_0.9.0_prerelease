using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Module
{
    [Cmdlet(VerbsCommon.New, "SkunkLabClaim")]
    public class SkunkLabClaim : Cmdlet
    {
        [Parameter(HelpMessage = "Claim Type", Mandatory = true)]
        public string ClaimType;


        [Parameter(HelpMessage = "Claim Value", Mandatory = true)]
        public string Value;

        protected override void ProcessRecord()
        {
            Claim claim = new Claim(this.ClaimType, this.Value);
            WriteObject(claim);
        }
    }
}
