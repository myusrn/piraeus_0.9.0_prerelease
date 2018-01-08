using Orleans;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Piraeus.GrainInterfaces
{
    public interface IServiceIdentity : IGrainWithStringKey
    {
        Task<X509Certificate2> GetCertificateAsync();

        Task<List<Claim>> GetClaimsAsync();

        Task AddCertificateAsync(X509Certificate2 certificate);

        Task AddClaimsAsync(List<Claim> claims);
    }
}
