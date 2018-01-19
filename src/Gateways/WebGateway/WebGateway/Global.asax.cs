using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebGateway.Security;

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

            // Create a trace listener for Web forms.
            WebPageTraceListener gbTraceListener = new WebPageTraceListener();
            // Add the event log trace listener to the collection.
            System.Diagnostics.Trace.Listeners.Add(gbTraceListener);

            if (!Orleans.GrainClient.IsInitialized)
            {
                bool dockerized = Convert.ToBoolean(ConfigurationManager.AppSettings["dockerize"]);
                if (!dockerized)
                {
                    OrleansClientConfig.TryStart("global.asax");
                }
                else
                {
                    string hostname = ConfigurationManager.AppSettings["dnsHostEntry"];
                    OrleansClientConfig.TryStart("global.asax", hostname);
                }
            }


        }
    }
}
