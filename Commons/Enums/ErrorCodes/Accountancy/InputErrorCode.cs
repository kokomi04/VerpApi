using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Verp.Resources.Enums.ErrorCodes.Accountancy;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("INP")]
    [LocalizedDescription(ResourceType = typeof(InputErrorCodeDescription))]

    public enum InputErrorCode
    {
        
        InputTypeNotFound = 1,
        
        InputCodeAlreadyExisted = 2,
        
        InputTitleAlreadyExisted = 3,
        
        InputAreaNotFound = 4,
        
        InputAreaCodeAlreadyExisted = 5,
        
        InputAreaTitleAlreadyExisted = 6,
        
        InputAreaFieldAlreadyExisted = 7,
        
        InputValueBillNotFound = 8,
        
        SourceCategoryFieldNotFound = 9,
        
        InputAreaFieldOverLoad = 10,
        
        InputAreaFieldNotFound = 11,
        
        DataTypeNotFound = 12,
        
        FormTypeNotFound = 13,
        
        InputReadOnly = 14,
        
        RequiredFieldIsEmpty = 15,
        
        UniqueValueAlreadyExisted = 16,
        
        ReferValueNotFound = 17,
        
        ReferValueNotValidFilter = 117,
        
        InputRowNotFound = 18,
        
        IsInputArea = 19,
        
        InputValueNotFound = 23,
        
        InputValueInValid = 25,
        
        InputRowAlreadyExisted = 26,
        
        InputIsNotModule = 27,
        
        FormatFileInvalid = 28,
        
        InputFieldNotFound = 29,
        
        InputFieldAlreadyExisted = 30,
        
        InputFieldIsUsed = 31,
        
        MapGenCodeConfigFail = 32,
        
        SourceInputTypeNotFound = 33,
        
        MultiRowAreaAlreadyExisted = 34,
        
        MultiRowAreaEmpty = 35,

        
        PrintConfigNotFound = 36,
        
        PrintConfigNameAlreadyExisted = 37,

        
        InputActionNotFound = 39,
        
        InputActionCodeAlreadyExisted = 40,
    }
}
