namespace DataComparer
{
    public interface IReadSettings
    {
        DataFlowSettings DataFlow { get; }
        string GetConnectionString(DataFlow f);
    }
}