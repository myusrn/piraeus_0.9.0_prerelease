using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace FakeClient
{
    public class FakeHttpWorkerRequest : SimpleWorkerRequest
    {
        public FakeHttpWorkerRequest()
            : base("/", "c:\\temp", "api/connect", "", null)
        {
                
        }
        public override int GetLocalPort()
        {
            return 80;
        }

        public override int GetRemotePort()
        {
            return 80;
        }

        public override bool IsClientConnected()
        {
            return true;
        }

        
    }
}
