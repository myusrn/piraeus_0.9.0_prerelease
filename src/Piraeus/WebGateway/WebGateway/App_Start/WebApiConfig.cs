using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using SkunkLab.Security.Authentication;

namespace WebGateway
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { id = RouteParameter.Optional },
                constraints: null,
                handler: new JwtValidationHandler(ConfigurationManager.AppSettings["symmetricKey"], ConfigurationManager.AppSettings["issuer"], ConfigurationManager.AppSettings["audience"])
            );

            config.Routes.MapHttpRoute(
                    name: "ManagementApi",
                    routeTemplate: "api2/{controller}/{action}",
                    defaults: new { id = RouteParameter.Optional },
                    constraints: null,
                    handler: new JwtValidationHandler(ConfigurationManager.AppSettings["symmetricKey"], ConfigurationManager.AppSettings["issuer"], ConfigurationManager.AppSettings["audience"])
                );
            
            config.Routes.MapHttpRoute(
                name: "TokenApi",
                routeTemplate: "api3/Manage/{id}",
                defaults: new
                {
                    controller = "Manage",
                    action = "Get",
                    id = RouteParameter.Optional
                },
                constraints: null,
                handler: null
            );
        }
    }
}
