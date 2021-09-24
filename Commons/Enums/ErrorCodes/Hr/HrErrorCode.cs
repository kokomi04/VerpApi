using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Verp.Resources.Enums.ErrorCodes.Hr;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("HREC")]
    [LocalizedDescription(ResourceType = typeof(HrErrorCodeDescription))]

    public enum HrErrorCode
    {
        
        HrTypeNotFound = 1,
        
        HrCodeAlreadyExisted = 2,
        
        HrTitleAlreadyExisted = 3,
        
        HrAreaNotFound = 4,
        
        HrAreaCodeAlreadyExisted = 5,
        
        HrAreaTitleAlreadyExisted = 6,
        
        HrAreaFieldAlreadyExisted = 7,
        
        HrValueBillNotFound = 8,
        
        SourceCategoryFieldNotFound = 9,
        
        HrAreaFieldOverLoad = 10,
        
        HrAreaFieldNotFound = 11,
        
        DataTypeNotFound = 12,
        
        FormTypeNotFound = 13,
        
        HrReadOnly = 14,
        
        RequiredFieldIsEmpty = 15,
        
        UniqueValueAlreadyExisted = 16,
        
        ReferValueNotFound = 17,
        
        HrRowNotFound = 18,
        
        IsHrArea = 19,
        
        HrValueNotFound = 23,
        
        HrValueInValid = 25,
        
        HrRowAlreadyExisted = 26,
        
        HrIsNotModule = 27,
        
        FormatFileInvalid = 28,
        
        HrFieldNotFound = 29,
        
        HrFieldAlreadyExisted = 30,
        
        HrFieldIsUsed = 31,
        
        MapGenCodeConfigFail = 32,
        
        SourceHrTypeNotFound = 33,
        // [Description("Vùng dữ liệu dạng bảng đã tồn tại")]
        // MultiRowAreaAlreadyExisted = 34,
        
        MultiRowAreaEmpty = 35,

        
        PrintConfigNotFound = 36,
        
        PrintConfigNameAlreadyExisted = 37,

        
        HrActionNotFound = 39,
        
        HrActionCodeAlreadyExisted = 40,

        CanNotInsertHrData = 41,
        
        ReferValueNotValidFilter = 42,
    }
}
