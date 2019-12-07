using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    
    public enum ObjectGenCodeErrorCode
    {
        ConfigAlreadyExisted = 1,
        ConfigNotFound,
        EmptyConfig,
        AllowOnlyOneConfigActivedAtTheSameTime,
        NoActivedConfigWasFound,
        NotSupportedYet
    }
}
