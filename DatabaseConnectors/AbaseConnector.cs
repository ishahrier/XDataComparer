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

        protected abstract DataBaseType GetDatabaseType();
        public T GetDefaultConnection()
        {
            if (settings.DataFlowSettings.Source.DataBaseType == GetDatabaseType()) return GetConnection(settings.DataFlowSettings.Source);
            else if (settings.DataFlowSettings.Target.DataBaseType == GetDatabaseType()) return GetConnection(settings.DataFlowSettings.Target);
            else throw new Exception($"Neither 'Source' or 'Target' DbType is '{GetDatabaseType()}'");
        }
        public abstract T GetConnection(DataFlow flowSetting);

        public virtual void TestConnection(T con)
        {
            try
            {
                Console.WriteLine($"Testing connection to '{GetDatabaseType()}' Database...".InfoWithBg());
                Console.Write("Connection String: ");
                Console.WriteLine("\"" + con.ConnectionString.HotPink() + "\"");
                Console.Write("Opening connection...");
                Console.WriteLine("SUCCESS!".SuccessWithBg());
                Console.Write("Closing connection...");
                Console.WriteLine("DONE".SuccessWithBg());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Connecting to '{GetDatabaseType()}' Database.".ErrorWithBg());
                Console.WriteLine(ex);
            }
        }
    }
}
