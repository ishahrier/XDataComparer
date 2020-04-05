using DataComparer.Extensions;
using DataComparer.Settings;
using Microsoft.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataComparer.DatabaseConnectors
{
    public class SqlServerConnector : ABaseConnector<SqlConnection>, ISqlServerConnector
    {
        public SqlServerConnector(IReadSettings settings) : base(settings) { }

        public override SqlConnection GetConnection(DataFlow setting)
        {
            if (setting.DataBaseType != GetConnectorDbType()) throw new Exception("Connection 'DataBaseType' must be 'SqlServer'.");
            var conString = settings.GetConnectionString(setting);            
            return new SqlConnection(conString);
        }

        protected override DataBaseType GetConnectorDbType() => DataBaseType.SqlServer;

        public void TestConnection(SqlConnection con)
        {

            try
            {
                Console.WriteLine("Testing connection to SQL Server Database...".InfoWithBg());
                Console.Write("Connection String: ");
                Console.WriteLine("\"" + con.ConnectionString.HotPink() + "\"");
                Console.Write("Opening connection...");
                Console.WriteLine("SUCCESS!".SuccessWithBg());
                Console.Write("Closing connection...");
                Console.WriteLine("DONE".SuccessWithBg());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Connecting to SQL Server Database.".ErrorWithBg());
                Console.WriteLine(ex);
            }
        }


    }
}
