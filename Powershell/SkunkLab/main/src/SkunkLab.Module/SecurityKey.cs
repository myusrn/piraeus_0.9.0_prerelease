using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Module
{
    [Cmdlet(VerbsCommon.Add, "SecurityKey")]
    public class AddSecurityKey : Cmdlet
    {
    }


    [Cmdlet(VerbsCommon.Remove, "SecurityKey")]
    public class RemoveSecurityKey : Cmdlet
    {
    }

    [Cmdlet(VerbsCommon.Get, "SecurityKey")]
    public class GetSecurityKey : Cmdlet
    {
    }

    [Cmdlet(VerbsData.Update, "SecurityKey")]
    public class UpdateSecurityKey : Cmdlet
    {
    }

    [Cmdlet(VerbsCommon.Get, "SecurityKeyList")]
    public class SecurityKeyList : Cmdlet
    {
    }


}
