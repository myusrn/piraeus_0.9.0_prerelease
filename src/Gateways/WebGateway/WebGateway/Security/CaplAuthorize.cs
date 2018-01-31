using System;
using System.Configuration;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Capl.Authorization;

namespace WebGateway.Security
{
    public class CaplAuthorize : AuthorizeAttribute
    {
        public string PolicyId { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            ClaimsIdentity identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;

            AuthorizationPolicy policy = GetPolicyFromCache();

            if (policy == null)
            {
                policy = CreatePolicy();
                CachePolicy(policy);
            }

            return policy.Evaluate(identity);
        }

        private void CachePolicy(AuthorizationPolicy policy)
        {
            HttpContext.Current.Application.Set(policy.PolicyId.ToString(), policy);
        }

        private AuthorizationPolicy GetPolicyFromCache()
        {
            return HttpContext.Current.Application.Get(PolicyId) as AuthorizationPolicy;
        }

        private AuthorizationPolicy CreatePolicy()
        {
            string claimType = null;
            string value = null;
            bool dockerized = Convert.ToBoolean(ConfigurationManager.AppSettings["dockerize"]);

            if (!dockerized)
            {
                claimType = ConfigurationManager.AppSettings["roleClaimType"];
                value = ConfigurationManager.AppSettings["roleClaimValue"];
            }
            else
            {
                claimType = System.Environment.GetEnvironmentVariable("MGMT_API_ROLE_CLAIM_TYPE");
                value = System.Environment.GetEnvironmentVariable("MGMT_API_ROLE_CLAIM_VALUE");
            }

            Rule rule = new Rule();
            rule.MatchExpression = new Capl.Authorization.Match(Capl.Authorization.Matching.LiteralMatchExpression.MatchUri, claimType, true);
            rule.Operation = new EvaluationOperation(Capl.Authorization.Operations.EqualOperation.OperationUri, value);
            return new AuthorizationPolicy(rule, new Uri(this.PolicyId));
        }



    }
}