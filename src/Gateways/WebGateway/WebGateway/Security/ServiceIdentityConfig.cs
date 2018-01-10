//using Piraeus.Grains;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Security.Claims;
//using System.Security.Cryptography.X509Certificates;
//using System.Threading.Tasks;

//namespace WebGateway.Security
//{
//    public class ServiceIdentityConfig
//    {
//        private static bool configured;
//        public static bool TryConfig()
//        {
//            bool result = false;

//            if(configured)
//            {
//                return true;
//            }

//            try
//            {
//                List<Claim> claims = null;
//                X509Certificate2 certificate = Piraeus.Configuration.PiraeusConfigManager.Settings.Security.Service.Certificate;
//                IEnumerable<Claim> claimSet = Piraeus.Configuration.PiraeusConfigManager.Settings.Identity.Service.Claims;

//                if (claimSet != null)
//                {
//                    claims = new List<Claim>(claimSet);
//                }

//                Task task = SetVariables(claims, certificate);
//                Task.WaitAll(task);

//                result = true;
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceWarning("Service identity not set with error {0}", ex.Message);
//            }

//            return result;
//        }

//        private static async Task SetVariables(List<Claim> claims = null, X509Certificate2 certificate = null)
//        {
//            Task task = GraphManager.SetServiceIdentityAsync(claims, certificate);
//            await Task.WhenAll(task);
//        }
//    }
//}