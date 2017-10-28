using System.Threading.Tasks;
using Capl.Authorization;
using Orleans;

namespace Piraeus.GrainInterfaces
{
    public interface IAccessControl : IGrainWithStringKey
    {
        Task UpsertPolicyAsync(AuthorizationPolicy policy);

        Task ClearAsync();

        Task<AuthorizationPolicy> GetPolicyAsync();
    }
}
