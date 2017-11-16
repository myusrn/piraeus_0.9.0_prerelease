using Capl.Authorization;
using System.Management.Automation;

namespace SkunkLab.Powershell
{
    [Cmdlet(VerbsCommon.New, "CaplLogicalOr")]
    public class CaplLogicalOr : Cmdlet
    {
        [Parameter(HelpMessage = "Truthful evaluation of the logical OR", Mandatory = true)]
        public bool Evaluates;

        [Parameter(HelpMessage = "Array of Terms (Rules, Logical OR, Logical AND) or any combinations", Mandatory = true)]
        public Term[] Terms;

        protected override void ProcessRecord()
        {
            LogicalOrCollection loc = new LogicalOrCollection();
            loc.Evaluates = this.Evaluates;
            foreach (Term term in this.Terms)
            {
                loc.Add(term);
            }

            WriteObject(loc);
        }
    }
}
