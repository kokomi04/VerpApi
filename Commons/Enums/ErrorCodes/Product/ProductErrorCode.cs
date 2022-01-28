using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Verp.Resources.Enums.ErrorCodes.Product;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("PRO")]
    [LocalizedDescription(ResourceType = typeof(ProductErrorCodeDescription))]

    public enum ProductErrorCode
    {
        ProductNotFound = 1,
        ProductCodeEmpty = 2,
        ProductCodeAlreadyExisted = 3,
        ProductCateInvalid = 4,
        ProductTypeInvalid = 5,
        SomeProductUnitConversionInUsed = 6,
        ProductNameAlreadyExisted = 7,
        InvalidUnitConversionExpression = 8,
        ProductInUsed = 9,

        ProductNameEmpty = 10,

        QuantityOfMaterialsConsumptionIsZero = 11,
    }
}
