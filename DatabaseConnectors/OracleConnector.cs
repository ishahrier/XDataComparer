using DataComparer.Extensions;
using DataComparer.Settings;
using Oracle.ManagedDataAccess.Client;
using System;

namespace DataComparer.DatabaseConnectors
{
    public class OracleConnector : ABaseConnector<OracleConnection>, IOracleConnector
    {
        public OracleConnector(IReadSettings settings) : base(settings) { }

        protected override DataBaseType GetConnectorDbType() => DataBaseType.Oracle;

        public override OracleConnection GetConnection(DataFlow setting)
        {
            if (setting.DataBaseType != DataBaseType.Oracle) throw new Exception("Connection 'DataBaseType' must be 'Oracle'.");
            var conString = settings.GetConnectionString(setting);
            if (OracleConfiguration.OracleDataSources[setting.ConnectionName] == "")
                OracleConfiguration.OracleDataSources.Add(setting.ConnectionName, conString);
            return new OracleConnection(conString);
        }

        public void TestConnection(OracleConnection con)
        {
            try
            {
                Console.WriteLine("Testing connection to Oracle Database...".InfoWithBg());
                Console.Write("Connection String: ");
                Console.WriteLine("\"" + con.ConnectionString.HotPink() + "\"");
                Console.Write("Opening connection...");
                Console.WriteLine("SUCCESS!".SuccessWithBg());
                Console.Write("Closing connection...");
                Console.WriteLine("DONE".SuccessWithBg());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Connecting to Oracle Database.".ErrorWithBg());
                Console.WriteLine(ex);
            }
        }
    }
}
