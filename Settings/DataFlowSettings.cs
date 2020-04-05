namespace DataComparer.Settings
{
    public class DataFlowSettings
    {
        public DataFlow Source { get; set; }
        public DataFlow Target { get; set; }
    }

    public class DataFlow
    {
        public DataFlow()
        {
        }
        public string DbType { get; set; }
        public string ConnectionName { get; set; }

 
    }
}
