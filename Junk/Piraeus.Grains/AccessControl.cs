using System.Threading.Tasks;
using Capl.Authorization;
using Orleans;
using Piraeus.GrainInterfaces;

namespace Piraeus.Grains
{
    public class AccessControl : Grain<AccessControlState>, IAccessControl
    {
        public async Task ClearAsync()
        {
            await WriteStateAsync();
        }

        public async Task<AuthorizationPolicy> GetPolicyAsync()
        {
            return await Task.FromResult<AuthorizationPolicy>(State.Policy);
        }

        public async Task UpsertPolicyAsync(AuthorizationPolicy policy)
        {
            State.Policy = policy;
            await Task.CompletedTask;
        }
    }
}
