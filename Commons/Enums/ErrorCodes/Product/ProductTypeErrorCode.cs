using Verp.Resources.Enums.ErrorCodes.Product;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("PRT")]
    [LocalizedDescription(ResourceType = typeof(ProductTypeErrorCodeDescription))]

    public enum ProductTypeErrorCode
    {
        EmptyProductTypeName = 1,
        ParentProductTypeNotfound = 2,
        ProductTypeNotfound = 3,
        CanNotDeletedParentProductType = 4,
        ProductTypeNameAlreadyExisted = 5,
        ProductTypeInUsed = 6,
    }
}
