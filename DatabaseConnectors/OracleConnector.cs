using DataComparer.Extensions;
using DataComparer.Settings;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace DataComparer.DatabaseConnectors
{
    public class OracleConnector : ABaseConnector<OracleConnection> 
    {
        public OracleConnector(IReadSettings settings) : base(settings) { }

        public override DataBaseType GetDataBaseType() => DataBaseType.Oracle;

        public override OracleConnection GetConnection(DataFlow setting)
        {
            
            if (setting.DataBaseType != DataBaseType.Oracle) throw new Exception("Connection 'DataBaseType' must be 'Oracle'.");
            var conString = settings.GetConnectionString(setting);
            if (OracleConfiguration.OracleDataSources[setting.ConnectionName] == "")
                OracleConfiguration.OracleDataSources.Add(setting.ConnectionName, conString);
            return new OracleConnection(conString);
        }

        public override DataSet Fill(string sql, OracleConnection con)
        {
            DataSet ds = new DataSet();
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sql;
                var adapter = new OracleDataAdapter(cmd);
                adapter.Fill(ds);
            }
            return ds;
        }
    }
}
