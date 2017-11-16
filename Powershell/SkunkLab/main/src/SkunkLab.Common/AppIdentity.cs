using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Common
{
    /// <summary>
    /// The application identity allows the association of claims for use in a security token
    /// where the users acquiring the security have the same set of claims.
    /// </summary>
    [JsonObject("appIdentity")]
    public class AppIdentity
    {
        public AppIdentity()
        {

        }


        public AppIdentity(string id, string description, ClaimLiteral[] claims)
        {
            this.Id = id;
            this.Description = description;
            this.Claims = claims;
        }

        [JsonProperty("id")]
        /// <summary>
        /// Gets or sets the unique identifier for the application.
        /// </summary>
        public string Id { get; set; }



        [JsonProperty("description")]
        /// <summary>
        /// Gets or sets a description of the application.
        /// </summary>
        public string Description { get; set; }


        [JsonProperty("claims")]
        /// <summary>
        /// Gets or sets claims associated with the application.
        /// </summary>
        public ClaimLiteral[] Claims { get; set; }


    }
}
