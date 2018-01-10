using SkunkLab.Security.Authentication;
using System.Configuration;
using System.Web.Http;
using WebGateway.Formatters;

namespace WebGateway
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.Formatters.Insert(0, new TextMediaTypeFormatter());
            config.Formatters.Insert(1, new BinaryMediaTypeFormatter());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: null,
                handler: new JwtValidationHandler(ConfigurationManager.AppSettings["symmetricKey"], ConfigurationManager.AppSettings["issuer"], ConfigurationManager.AppSettings["audience"])
            );

            config.Routes.MapHttpRoute(
                    name: "ExplicitApi",
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
