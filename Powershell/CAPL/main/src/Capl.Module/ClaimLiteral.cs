using Capl.Authorization;
using System.Management.Automation;

namespace Capl.Module
{
    [Cmdlet(VerbsCommon.New, "CaplClaim")]
    public class ClaimLiteral : Cmdlet
    {
        [Parameter(HelpMessage = "Claim Type", Mandatory = true)]
        public string ClaimType;

        [Parameter(HelpMessage = "Claim Value", Mandatory = true)]
        public string Value;

        protected override void ProcessRecord()
        {
            LiteralClaim literal = new LiteralClaim(this.ClaimType, this.Value);
            WriteObject(literal);
        }
    }
}
