using System.Security;
using System.Web.Http;
using Piraeus.Configuration;
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
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //string signingKey = PiraeusConfigManager.Settings.Security.Client.SymmetricKey;
            //string issuer = PiraeusConfigManager.Settings.Security.Client.Issuer;
            //string audience = PiraeusConfigManager.Settings.Security.Client.Audience;
            //string tokenType = PiraeusConfigManager.Settings.Security.Client.TokenType;

            string signingKey = "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=";
            string issuer = "http://www.example.org/";
            string audience = "http://www.example.org/";
            string tokenType = "JWT";

            if (tokenType.ToUpper() == "JWT")
            {
                config.MessageHandlers.Add(new JwtValidationHandler(signingKey, issuer, audience));
            }
            else if(tokenType.ToUpper() == "SWT")
            {
                config.MessageHandlers.Add(new SwtValidationHandler(signingKey, issuer, audience));
            }
            else if(tokenType.ToUpper() == "X509")
            {
                config.MessageHandlers.Add(new X509ValidationHandler());
            }
            else
            {
                throw new SecurityException("Not authenticated.");
            }
        }
    }
}
