using System.Net;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebGatewayDocker
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            try
            {

                var hostEntry = Dns.GetHostEntry("orleans-silo");
                var ip = hostEntry.AddressList[0];
                var config = new Orleans.Runtime.Configuration.ClientConfiguration();
                config.Gateways.Add(new IPEndPoint(ip, 10400));

                //var config = Orleans.Runtime.Configuration.ClientConfiguration.LocalhostSilo();
                Orleans.GrainClient.Initialize(config);
            }
            catch
            { }
        }
    }
}
