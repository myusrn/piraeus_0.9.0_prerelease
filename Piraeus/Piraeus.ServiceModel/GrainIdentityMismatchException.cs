using System;
using System.Runtime.Serialization;

namespace Piraeus.ServiceModel
{
    public class GrainIdentityMismatchException : Exception
    {
        public GrainIdentityMismatchException()
        {

        }

        public GrainIdentityMismatchException(string message) 
            : base(message)
        {

        }

        public GrainIdentityMismatchException(string message, Exception innerException) 
            : base(message, innerException)
        {

        }

        protected GrainIdentityMismatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
