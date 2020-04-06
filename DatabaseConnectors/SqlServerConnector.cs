using DataComparer.Extensions;
using DataComparer.Settings;
using Microsoft.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace DataComparer.DatabaseConnectors
{
    public class SqlServerConnector : ABaseConnector<SqlConnection> 
    {
        public SqlServerConnector(IReadSettings settings) : base(settings) { }

        public override SqlConnection GetConnection(DataFlow setting)
        {
            if (setting.DataBaseType != GetDataBaseType()) throw new Exception("Connection 'DataBaseType' must be 'SqlServer'.");
            var conString = settings.GetConnectionString(setting);            
            return new SqlConnection(conString);
        }

        public override DataSet Fill(string sql,SqlConnection con)
        {
            DataSet ds = new DataSet();
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sql;
                var adapter = new SqlDataAdapter(cmd);
             
                adapter.Fill(ds);
            }
            return ds;
        }
        
        public override DataBaseType GetDataBaseType() => DataBaseType.SqlServer;

     
    }
}
