namespace DataComparer.Settings
{
    public enum DataBaseType
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
        public DataBaseType DataBaseType { get; set; }
        public string ConnectionName { get; set; } 
    }
}
