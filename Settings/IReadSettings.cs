namespace DataComparer.Settings
{
    public interface IReadSettings
    {
        DataFlowSettings DataFlowSettings { get; }
        string GetConnectionString(DataFlow f);
    }
}