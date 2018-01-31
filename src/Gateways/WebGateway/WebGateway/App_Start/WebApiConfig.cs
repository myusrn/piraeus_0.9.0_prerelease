using SkunkLab.Security.Authentication;
using System;
using System.Configuration;
using System.Web.Http;
using WebGateway.Formatters;

namespace WebGateway
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            string symmetricKey = null;
            string issuer = null;
            string audience = null;

            bool dockerized = Convert.ToBoolean(ConfigurationManager.AppSettings["dockerize"]);

            if(dockerized)
            {
                symmetricKey = System.Environment.GetEnvironmentVariable("MGMT_API_SYMMETRICKEY");
                issuer = System.Environment.GetEnvironmentVariable("MGMT_API_ISSUER");
                audience = System.Environment.GetEnvironmentVariable("MGMT_API_AUDIENCE");
            }
            else
            {
                symmetricKey = ConfigurationManager.AppSettings["symmetricKey"];
                issuer = ConfigurationManager.AppSettings["issuer"];
                audience = ConfigurationManager.AppSettings["audience"];
            }


            config.MapHttpAttributeRoutes();
            config.Formatters.Insert(0, new TextMediaTypeFormatter());
            config.Formatters.Insert(1, new BinaryMediaTypeFormatter());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: null,
                handler: new JwtValidationHandler(symmetricKey, issuer, audience)
            );

            config.Routes.MapHttpRoute(
                    name: "ExplicitApi",
                    routeTemplate: "api2/{controller}/{action}",
                    defaults: new { id = RouteParameter.Optional },
                    constraints: null,
                    handler: new JwtValidationHandler(symmetricKey, issuer, audience)
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
