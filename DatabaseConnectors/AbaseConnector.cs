using DataComparer.Settings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataComparer.DatabaseConnectors
{
    public abstract class ABaseConnector<T> where T:class
    {
        protected readonly IReadSettings settings;
        protected ABaseConnector(IReadSettings settings) => this.settings = settings;

        protected abstract DataBaseType GetConnectorDbType();
        public T GetDefaultConnection()
        {
            if (settings.DataFlowSettings.Source.DataBaseType == GetConnectorDbType()) return GetConnection(settings.DataFlowSettings.Source);
            else if (settings.DataFlowSettings.Target.DataBaseType == GetConnectorDbType()) return GetConnection(settings.DataFlowSettings.Target);
            else throw new Exception($"Neither 'Source' or 'Target' DbType is '{GetConnectorDbType()}'");
        }
        public abstract T GetConnection(DataFlow flowSetting);

        
    }
}
