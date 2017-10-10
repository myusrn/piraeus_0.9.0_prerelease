using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Capl.Authorization;

namespace Piraeus.Actors
{
    public interface IAccessControl
    {
        Task<AuthorizationPolicy> GetPolicyAsync();

        Task SetPolicyAsnyc(AuthorizationPolicy policy);

        Task<bool> IsAuthorizedAsync(System.Security.Claims.ClaimsIdentity identity);
    }
}
