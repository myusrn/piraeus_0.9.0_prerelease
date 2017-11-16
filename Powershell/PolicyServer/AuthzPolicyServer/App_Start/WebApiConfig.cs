using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using AuthzPolicyServer.Models;
using System.Configuration;
using System.Web.Http.Dispatcher;




namespace AuthzPolicyServer
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api2/Acct/{id}",
                defaults: new { 
                    controller = "Acct",
                    action = "Post",
                    id = RouteParameter.Optional 
                }
            );

            config.Routes.MapHttpRoute(
                name: "TokenApi",
                routeTemplate: "api/SecurityToken/{id}",
                defaults: new
                {
                    controller = "SecurityToken",
                    action = "Get",
                    id = RouteParameter.Optional
                }
            );

            

            DelegatingHandler[] handler = new DelegatingHandler[] { new JwtBrokerValidationHandler(ConfigurationManager.AppSettings["SymmetricKey"], ConfigurationManager.AppSettings["Audience"], ConfigurationManager.AppSettings["Issuer"]) };
            

            // Create a message handler chain with an end-point.
            var handlers = HttpClientFactory.CreatePipeline(
                new HttpControllerDispatcher(config), handler);

            

            config.Routes.MapHttpRoute(
                name: "SecureRoute",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: null,
                handler: handlers
            );
        }
    }
}
