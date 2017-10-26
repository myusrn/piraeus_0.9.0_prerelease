using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Orleans.Storage;

namespace WebGateway
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

             var config = Orleans.Runtime.Configuration.ClientConfiguration.LocalhostSilo();
            //config.Gateways.Clear();
            //config.Gateways.Add(new System.Net.IPEndPoint(IPAddress.Parse("127.0.0.1"), 30000));
            // foreach(var item in config.Gateways)
            //{
            //    Console.WriteLine("Address {0}  Port {1}", item.Address.ToString(), item.Port);
            //}
            

            Orleans.GrainClient.Initialize(config);
        }
    }
}
