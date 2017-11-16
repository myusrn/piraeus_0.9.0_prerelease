using Capl.Authorization;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace SkunkLab.Common
{
    /// <summary>
    /// 
    /// </summary>
    [JsonObject("accessControlPolicy")]
    public class AccessControlPolicy
    {
        /// <summary>
        /// Creates a new instance of the access control policy.
        /// </summary>
        public AccessControlPolicy()
        {
        }

        /// <summary>
        /// Creates a new instance of the access control policy.
        /// </summary>
        /// <param name="policy">AuthorizationPolicy object.</param>
        public AccessControlPolicy(AuthorizationPolicy policy)
            : this(policy, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the access control policy.
        /// </summary>
        /// <param name="policy">AuthorizationPolicy object.</param>
        /// <param name="description">Description of the access control policy.</param>
        public AccessControlPolicy(AuthorizationPolicy policy, string description)
        {
            if(policy == null)
            {
                throw new ArgumentNullException("policy");
            }

            this.Description = description;

            this.PolicyId = policy.PolicyId.ToString().ToLower(CultureInfo.InvariantCulture);

            StringBuilder builder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings() { OmitXmlDeclaration = true };
            using(XmlWriter writer = XmlWriter.Create(builder, settings))
            {
                policy.WriteXml(writer);
                writer.Flush();
                writer.Close();
            }

            this.AuthorizationPolicyXml = builder.ToString();
        }

        /// <summary>
        /// Gets or sets the URI string identifier of the access control policy.
        /// </summary>
        [JsonProperty("policyId")]
        public string PolicyId { get; set; }

        /// <summary>
        /// Gets or sets a description of the access control policy.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the Xml string of the access control policy.
        /// </summary>
        [JsonProperty("authorizationPolicyXml")]
        public string AuthorizationPolicyXml { get; set; }

        /// <summary>
        /// Gets an AuthorizationPolicy object.
        /// </summary>
        /// <returns></returns>
        public AuthorizationPolicy GetPolicy()
        {
            AuthorizationPolicy policy = null;
            byte[] bytes = Encoding.UTF8.GetBytes(this.AuthorizationPolicyXml);
            using(MemoryStream stream = new MemoryStream(bytes))
            {
                stream.Position = 0;
                using(XmlReader reader = XmlReader.Create(stream))
                {
                    policy = AuthorizationPolicy.Load(reader);
                    reader.Close();
                }

                stream.Close();
            }

            return policy;
        }
    }
}
