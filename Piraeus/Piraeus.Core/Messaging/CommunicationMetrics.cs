using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Core.Messaging
{
    public class CommunicationMetrics
    {
        public CommunicationMetrics()
        {
        }

        public CommunicationMetrics(string id, long messageCount, long byteCount, long errorCount, DateTime lastMessageTimestamp, DateTime? lastErrorTimestamp, Exception lastError = null)
        {
            Id = id;
            MessageCount = messageCount;
            ByteCount = byteCount;
            ErrorCount = errorCount;
            LastMessageTimestamp = lastMessageTimestamp;
            LastErrorTimestamp = lastErrorTimestamp;
            LastError = lastError;
        }

        public string Id { get; set; }

        public long ByteCount { get; set; }

        public long MessageCount { get; set; }

        public long ErrorCount { get; set; }

        public DateTime? LastErrorTimestamp { get; set; }
        public DateTime LastMessageTimestamp { get; set; }

        public Exception LastError { get; set; }
    }
}
