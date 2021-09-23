using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Verp.Resources.Enums.ErrorCodes.Voucher;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("VOU")]
    [LocalizedDescription(ResourceType = typeof(VoucherErrorCodeDescription))]

    public enum VoucherErrorCode
    {
        
        VoucherTypeNotFound = 1,
        
        VoucherCodeAlreadyExisted = 2,
        
        VoucherTitleAlreadyExisted = 3,
        
        VoucherAreaNotFound = 4,
        
        VoucherAreaCodeAlreadyExisted = 5,
        
        VoucherAreaTitleAlreadyExisted = 6,
        
        VoucherAreaFieldAlreadyExisted = 7,
        
        VoucherValueBillNotFound = 8,
        
        SourceCategoryFieldNotFound = 9,
        
        VoucherAreaFieldOverLoad = 10,
        
        VoucherAreaFieldNotFound = 11,
        
        DataTypeNotFound = 12,
        
        FormTypeNotFound = 13,
        
        VoucherReadOnly = 14,
        
        RequiredFieldIsEmpty = 15,
        
        UniqueValueAlreadyExisted = 16,
        
        ReferValueNotFound = 17,
        
        ReferValueNotValidFilter = 117,
        
        VoucherRowNotFound = 18,
        
        IsVoucherArea = 19,
        
        VoucherValueNotFound = 23,
        
        VoucherValueInValid = 25,
        
        VoucherRowAlreadyExisted = 26,
        
        VoucherIsNotModule = 27,
        
        FormatFileInvalid = 28,
        
        VoucherFieldNotFound = 29,
        
        VoucherFieldAlreadyExisted = 30,
        
        VoucherFieldIsUsed = 31,
        
        MapGenCodeConfigFail = 32,
        
        SourceVoucherTypeNotFound = 33,
        
        MultiRowAreaAlreadyExisted = 34,
        
        MultiRowAreaEmpty = 35,

        
        PrintConfigNotFound = 36,
        
        PrintConfigNameAlreadyExisted = 37,
        
        VoucherActionNotFound = 39,
        
        VoucherActionCodeAlreadyExisted = 40
    }
}
