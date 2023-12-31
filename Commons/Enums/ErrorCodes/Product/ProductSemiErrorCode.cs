﻿using Verp.Resources.Enums.ErrorCodes.Product;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.ErrorCodes
{
    [LocalizedDescription(ResourceType = typeof(ProductSemiErrorCodeDescription))]

    public enum ProductSemiErrorCode
    {
        NotFoundProductSemi = 1,
        NotFoundProductSemiConversion = 2
    }
}
