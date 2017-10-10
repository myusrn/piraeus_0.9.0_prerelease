using System.Security.Claims;
using System.Threading.Tasks;
using Capl.Authorization;
using Orleans;
using Piraeus.Grains;

namespace Piraeus.ServiceModel
{
    public class AccessControl : Grain<AccessControlState>, IAccessControl
    {
        public async Task ClearAsync()
        {
            await ClearStateAsync();
            await Task.CompletedTask;
        }

        public async Task<bool> IsAuthorizedAsync(ClaimsIdentity identity)
        {
            if(State.Policy != null)
            {
                return await Task.FromResult<bool>(State.Policy.Evaluate(identity));
            }
            else
            {
                return await Task.FromResult<bool>(false);
            }
        }

        public async Task UpsertPolicyAsync(AuthorizationPolicy policy)
        {
            State.Policy = policy;
            await Task.CompletedTask;
        }
    }
}
