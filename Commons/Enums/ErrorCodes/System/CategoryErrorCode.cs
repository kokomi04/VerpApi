using Verp.Resources.Enums.System;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("CTGY")]
    [LocalizedDescription(ResourceType = typeof(CategoryErrorCodeDescription))]

    public enum CategoryErrorCode
    {

        CategoryNotFound = 1,

        CategoryCodeAlreadyExisted = 2,

        CategoryTitleAlreadyExisted = 3,

        SubCategoryNotFound = 4,

        SubCategoryIsModule = 5,

        SubCategoryHasParent = 6,


        CategoryFieldNameAlreadyExisted = 8,

        SourceCategoryFieldNotFound = 9,

        DestCategoryFieldAlreadyExisted = 10,

        CategoryFieldNotFound = 11,

        DataTypeNotFound = 12,

        FormTypeNotFound = 13,

        CategoryReadOnly = 14,

        RequiredFieldIsEmpty = 15,

        UniqueValueAlreadyExisted = 16,

        ReferValueNotFound = 17,

        CategoryRowNotFound = 18,


        SubCategoryCodeAlreadyExisted = 20,

        SubCategoryTitleAlreadyExisted = 21,


        CategoryValueNotFound = 23,


        CategoryValueInValid = 25,




        ParentCategoryRowNotExisted = 30,

        CategoryIsOutSideData = 31,


        ParentCategoryFromItSelf = 33,

        CategoryFieldReadOnly = 44,

        HadSomeDataRelatedToThisValue = 45,

        InvalidSubsidiary = 46,

        CatRelationshipAlreadyExisted = 47,
        DataSizeInValid = 48,
    }
}
