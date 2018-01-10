//using Piraeus.Grains;
//using System;
//using System.Configuration;
//using System.Diagnostics;
//using System.Threading.Tasks;

//namespace WebGateway.Audit
//{
//    public class AuditorConfiguration
//    {
//        private static bool configured;

//        public static bool TryConfigure()
//        {
//            if (configured)
//            {
//                return true;
//            }

//            try
//            {
//                string connectionString = ConfigurationManager.AppSettings["auditTableStorageConnectionString"];
//                string tableName = ConfigurationManager.AppSettings["auditTableName"];

//                if (!(String.IsNullOrEmpty(connectionString) || String.IsNullOrEmpty(tableName)))
//                {
//                    Task task = GraphManager.SetAuditConfigAsync(connectionString, tableName);
//                    Task.WhenAll(task);
//                }

//                configured = true;

//            }
//            catch (Exception ex)
//            {
//                Trace.TraceWarning("Audit failed to configured with error {0}", ex.Message);
//            }

//            return configured;
//        }

//        public static void ChangeConfig(string connectionString, string tableName)
//        {
//            Task task = GraphManager.SetAuditConfigAsync(connectionString, tableName);
//            Task.WhenAll(task);
//        }
//    }
//}