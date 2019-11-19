using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    public enum ProductErrorCode
    {
        ProductNotFound = 1,
        ProductCodeEmpty = 2,
        ProductCodeAlreadyExisted = 3,
        ProductCateInvalid = 4,
        ProductTypeInvalid = 5,
        SomeProductUnitConversionInUsed = 6
    }
}
