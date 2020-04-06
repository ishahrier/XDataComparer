using DataComparer.Extensions;
using DataComparer.Settings;
using Microsoft.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataComparer.DatabaseConnectors
{
    public class SqlServerConnector : ABaseConnector<SqlConnection>
    {
        public SqlServerConnector(IReadSettings settings) : base(settings) { }

        public override SqlConnection GetConnection(DataFlow setting)
        {
            if (setting.DataBaseType != GetDatabaseType()) throw new Exception("Connection 'DataBaseType' must be 'SqlServer'.");
            var conString = settings.GetConnectionString(setting);            
            return new SqlConnection(conString);
        }

        protected override DataBaseType GetDatabaseType() => DataBaseType.SqlServer;


    }
}
