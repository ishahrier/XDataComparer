using DataComparer.Extensions;
using DataComparer.Settings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace DataComparer.DatabaseConnectors
{
    public abstract class ABaseConnector<T> : IDbConnector<T> where T : DbConnection
    {
        protected readonly IReadSettings settings;
        protected ABaseConnector(IReadSettings settings) => this.settings = settings;

        public abstract DataBaseType GetDataBaseType();
        public T GetConnection()
        {
            if (settings.DataFlowSettings.Source.DataBaseType == GetDataBaseType()) return GetConnection(settings.DataFlowSettings.Source);
            else if (settings.DataFlowSettings.Target.DataBaseType == GetDataBaseType()) return GetConnection(settings.DataFlowSettings.Target);
            else throw new Exception($"Neither 'Source' or 'Target' DbType is '{GetDataBaseType()}'");
        }
        public abstract T GetConnection(DataFlow flowSetting);

        public virtual void TestConnection(T con)
        {
            try
            {
                Console.WriteLine($"Testing connection to '{GetDataBaseType()}' Database...".InfoWithBg());
                Console.Write("Connection String: ");
                Console.WriteLine("\"" + con.ConnectionString.HotPink() + "\"");
                Console.Write("Opening connection...");
                Console.WriteLine("SUCCESS!".SuccessWithBg());
                Console.Write("Closing connection...");
                Console.WriteLine("DONE".SuccessWithBg());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Connecting to '{GetDataBaseType()}' Database.".ErrorWithBg());
                Console.WriteLine(ex);
            }
        }

        public abstract DataSet Fill(string sql,T con);
         
    }
}
