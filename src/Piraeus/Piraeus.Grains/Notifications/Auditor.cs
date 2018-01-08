

using SkunkLab.Storage;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Piraeus.Grains.Notifications
{
    public class Auditor
    {
        public Auditor()
        {
            Task task = SetupAsync();
            Task.WhenAll(task);
        }

        private TableStorage storage;
        private string tablename;

        private async Task SetupAsync()
        {
            string connectionstring = await GraphManager.GetAuditConfigConnectionstringAsync();
            tablename = await GraphManager.GetAuditConfigTablenameAsync();
            try
            {
                storage = TableStorage.New(connectionstring);
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Auditor failed to initialize with error {0}", ex.Message);
            }
        }

        public async Task WriteAuditRecordAsync(AuditRecord record)
        {
            if (storage != null && record != null)
            {
                await storage.WriteAsync(tablename, record);
            }
        }

        public void WriteAuditRecord(AuditRecord record)
        {
            if (storage != null && record != null)
            {
                storage.Write(tablename, record);
            }
        }
    }


}
