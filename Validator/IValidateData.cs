using System;
using System.Collections.Generic;
using System.Text;

namespace DataComparer.Validator
{

    public interface IValidateData
    {
        string GetRedsSql();
        string GetCobaltSql();
        ValidatorGroup GetGroup();
        string GetValidatorName();
        string GetValidatorDescription();
        bool Run();
    }

}
