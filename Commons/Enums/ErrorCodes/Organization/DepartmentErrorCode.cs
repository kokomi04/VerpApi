using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Verp.Resources.Enums.ErrorCodes.Organization;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("DEPT")]
    [LocalizedDescription(ResourceType = typeof(DepartmentErrorCodeDescription))]

    public enum DepartmentErrorCode
    {
        DepartmentNotFound = 1,
        DepartmentParentNotFound = 2,
        DepartmentCodeAlreadyExisted = 3,
        DepartmentNameAlreadyExisted = 4,
        DepartmentChildAlreadyExisted = 5,
        DepartmentChildActivedAlreadyExisted = 6,
        DepartmentUserActivedAlreadyExisted = 7,
        DepartmentActived = 8,
        DepartmentParentInActived = 9,
        DepartmentInActived = 10,
    }
}
