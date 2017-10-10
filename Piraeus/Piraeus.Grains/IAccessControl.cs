using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Capl.Authorization;
using Orleans;
namespace Piraeus.Grains
{
    public interface IAccessControl : IGrainWithStringKey
    {
        Task UpsertPolicyAsync(AuthorizationPolicy policy);

        Task ClearAsync();

        Task<bool> IsAuthorizedAsync(ClaimsIdentity identity);
    }
}
