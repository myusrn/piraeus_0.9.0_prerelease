using Piraeus.Grains;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace WebGatewayDocker.Audit
{
    public static class AuditorConfiguration
    {
        private static bool calledOnce;

        public static void Configure()
        {
            if (calledOnce)
            {
                return;
            }

            try
            {
                string connectionString = ConfigurationManager.AppSettings["auditTableStorageConnectionString"];
                string tableName = ConfigurationManager.AppSettings["auditTableName"];

                if (!(String.IsNullOrEmpty(connectionString) || String.IsNullOrEmpty(tableName)))
                {
                    Task task = GraphManager.SetAuditConfigAsync(connectionString, tableName);
                    Task.WhenAll(task);
                }

                calledOnce = true;

            }
            catch (Exception ex)
            {
                //faulted 
            }
        }

        public static void ChangeConfig(string connectionString, string tableName)
        {
            Task task = GraphManager.SetAuditConfigAsync(connectionString, tableName);
            Task.WhenAll(task);
        }
    }
}