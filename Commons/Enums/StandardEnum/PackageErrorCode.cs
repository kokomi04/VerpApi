using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    /// <summary>
    /// 
    /// </summary>
    public enum PackageErrorCode
    {
        PackageNotFound = 1,
        PackageCodeEmpty = 2,
        PackageAlreadyExisted = 3,
        PackageNotAllowUpdate = 4,
        QualtityOfProductInPackageNotEnough = 5
    }
}
