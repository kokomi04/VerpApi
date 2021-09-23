using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Verp.Resources.Enums.ErrorCodes.Product;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("PRC")]
    [LocalizedDescription(ResourceType = typeof(ProductCateErrorCodeDescription))]

    public enum ProductCateErrorCode
    {
        EmptyProductCateName = 1,
        ParentProductCateNotfound = 2,
        ProductCateNotfound = 3,
        CanNotDeletedParentProductCate = 4,
        ProductCateNameAlreadyExisted = 5,
        ProductCateInUsed = 6,
    }
}
