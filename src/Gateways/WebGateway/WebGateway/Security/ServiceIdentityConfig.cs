using Piraeus.Grains;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace WebGateway.Security
{
    public class ServiceIdentityConfig
    {
        public static bool IsConfigured;

        public static async Task Configure()
        {
            if(IsConfigured)
            {
                return;
            }

            if (Orleans.GrainClient.IsInitialized)
            {
                List<Claim> claimSet = await GraphManager.GetServiceIdentityClaimsAsync();
                X509Certificate2 cert = await GraphManager.GetServiceIdentityCertificateAsync();

                IsConfigured = cert != null || claimSet != null;

                if (!IsConfigured)
                {
                    IEnumerable<Claim> claimArray = Piraeus.Configuration.PiraeusConfigManager.Settings.Identity.Service.Claims;
                    cert = Piraeus.Configuration.PiraeusConfigManager.Settings.Security.Service.Certificate;
                    List<Claim> claimList = claimArray != null ? new List<Claim>(claimArray) : null;
                    await GraphManager.SetServiceIdentityAsync(claimList, cert);

                    IsConfigured = true;
                }
            }

            
        }
        
    }
}