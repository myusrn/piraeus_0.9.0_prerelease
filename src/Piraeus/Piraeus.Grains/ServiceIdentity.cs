using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Piraeus.GrainInterfaces;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace Piraeus.Grains
{
    [StorageProvider(ProviderName = "store")]
    public class ServiceIdentity : Grain<ServiceIdentityState>, IServiceIdentity
    {       

        public override async Task OnDeactivateAsync()
        {
            await WriteStateAsync();
        }


        public Task<X509Certificate2> GetCertificateAsync()
        {
            return Task.FromResult<X509Certificate2>(State.Certificate);
        }

        public Task<List<Claim>> GetClaimsAsync()
        {
            return Task.FromResult<List<Claim>>(State.Claims);
        }

        public Task AddCertificateAsync(X509Certificate2 certificate)
        {
            State.Certificate = certificate;
            return Task.CompletedTask;
        }

        public Task AddClaimsAsync(List<Claim> claims)
        {
            TaskCompletionSource<Task> tcs = new TaskCompletionSource<Task>();
            State.Claims = claims;
            tcs.SetResult(null);
            return tcs.Task;
        }
    }
}
