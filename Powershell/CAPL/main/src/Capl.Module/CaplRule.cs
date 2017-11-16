using Capl.Authorization;
using System.Management.Automation;

namespace Capl.Module
{
    [Cmdlet(VerbsCommon.New, "CaplRule")]
    public class CaplRule : Cmdlet
    {
        [Parameter(HelpMessage = "Truthful evaluation of the rule", Mandatory = true)]
        public bool Evaluates;

        [Parameter(HelpMessage = "Name of issuer (optional)", Mandatory = false)]
        public string Issuer;

        [Parameter(HelpMessage = "CAPL Match Expression", Mandatory = true)]
        public Match MatchExpression;

        [Parameter(HelpMessage = "CAPL Operation", Mandatory = true)]
        public EvaluationOperation Operation;

        protected override void ProcessRecord()
        {
            Rule rule = new Rule(this.MatchExpression, this.Operation, this.Evaluates);
            if (!string.IsNullOrEmpty(this.Issuer))
            {
                rule.Issuer = this.Issuer;
            }

            WriteObject(rule);
        }
    }
}
