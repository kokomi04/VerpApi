using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("EMP")]
    public enum EmployeeErrorCode
    {
        EmptyEmployeeCode = 1,
        EmployeeCodeExisted = 2,
        EmployeeNotFound = 3,
        EmployeeCodeAlreadyExisted = 4
    }
}
