using DataComparer.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataComparer.Validator
{
    public interface IValidateData2
    {
        string GetRedsSql();
        string GetCobaltSql();
        ValidatorGroup GetGroup();
        string GetValidatorName();
        string GetValidatorDescription();
        bool Run();
    }

    public interface IDataValidator
    {
        DataBaseType SourceDbType();
        DataBaseType TargetDbType();
        string GetSourceSql();
        string GetTargetSql();
        string GetValidationGroup();
        string GetName();
        string GetCommandCode();
        string GetDescription();
        bool Run();
    }

}
