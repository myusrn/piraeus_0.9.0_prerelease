using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace AdapterTests.Settings
{
    public class SimpleHttpWorkerRequest : SimpleWorkerRequest
    {
        public SimpleHttpWorkerRequest(string page, string query, TextWriter output) : base(page, query, output)
        {
        }

        public override bool IsSecure()
        {
            
            return true;
        }

        
    }
}
