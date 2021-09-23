using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
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
        
        ParentCategoryAlreadyExisted = 7,
        
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
        
        IsSubCategory = 19,
        
        SubCategoryCodeAlreadyExisted = 20,
        
        SubCategoryTitleAlreadyExisted = 21,
        
        ReferenceFromItSelf = 22,
        
        CategoryValueNotFound = 23,
        
        CategoryFieldNotDefaultValue = 24,
        
        CategoryValueInValid = 25,
        
        CategoryRowAlreadyExisted = 26,
        
        CategoryIsNotModule = 27,
        
        FormatFileInvalid = 28,
        
        FormTypeNotSwitch = 29,
        
        ParentCategoryRowNotExisted = 30,
        
        CategoryIsOutSideData =31,
        
        CategoryIsOutSideDataError = 32,
        
        ParentCategoryFromItSelf = 33,
        
        CategoryFieldReadOnly = 44,
        
        RelationshipAlreadyExisted = 45,
        
        InvalidSubsidiary = 46,
        
        CatRelationshipAlreadyExisted = 47,
    }
}
