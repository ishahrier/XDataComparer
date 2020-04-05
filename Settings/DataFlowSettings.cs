namespace DataComparer.Settings
{
    public enum DbType
    {
        Oracle,
        SqlServer
    }
    public class DataFlowSettings
    {
        public DataFlow Source { get; set; }
        public DataFlow Target { get; set; }
    }

    public class DataFlow
    { 
        public DbType DbType { get; set; }
        public string ConnectionName { get; set; } 
    }
}
