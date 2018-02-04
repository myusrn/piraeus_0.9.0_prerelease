

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

        public Auditor(string connectionstring, string tablename)
        {
            Task task = SetupAsync(connectionstring, tablename);
            Task.WhenAll(task);
        }

        private TableStorage storage;
        private string tablename;

        public bool CanAudit { get; internal set; }

        private Task SetupAsync(string connectionstring = null, string tablename = null)
        {          
            //try to get the audit config from environment variables (best method)
            if (connectionstring == null)
            {
                connectionstring = System.Environment.GetEnvironmentVariable("ORLEANS_AUDIT_DATACONNECTIONSTRING");
                this.tablename = System.Environment.GetEnvironmentVariable("ORLEANS_AUDIT_TABLENAME");                
            }

            //audit config is not set...exit
            if (connectionstring == null)
            {
                Trace.TraceWarning("Auditor not configurable.");
                return Task.CompletedTask;
            }
            try
            {
                storage = TableStorage.New(connectionstring);
                CanAudit = true;
                Trace.TraceInformation("Auditor initialized.");
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Auditor failed to initialize with error {0}", ex.Message);
            }

            return Task.CompletedTask;
            
        }

        public async Task WriteAuditRecordAsync(AuditRecord record)
        {
            if (storage != null && record != null)
            {
                try
                {
                    await storage.WriteAsync(tablename, record);
                }
                catch(Exception ex)
                {
                    Trace.TraceWarning("Auditor failed to write record.");
                    Trace.TraceError("Auditor error {0}", ex.Message);
                    Trace.TraceError("Auditor stacktrace {0}", ex.StackTrace);
                }
            }
        }

        public void WriteAuditRecord(AuditRecord record)
        {
            if (storage != null && record != null)
            {
                try
                {
                    storage.Write(tablename, record);
                }
                catch(Exception ex)
                {
                    Trace.TraceWarning("Auditor failed to write record.");
                    Trace.TraceError("Auditor error {0}", ex.Message);
                    Trace.TraceError("Auditor stacktrace {0}", ex.StackTrace);
                }
            }
        }
    }


}
