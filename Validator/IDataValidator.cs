namespace DataComparer.Validator
{

    public interface IDataValidator
    { 

        string GetSourceSql();
        string GetTargetSql();
        string GetValidationGroup();
        string GetName();
        string GetCommandCode();
        string GetDescription();
        bool Run();
    }

}
