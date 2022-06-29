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


        VoucherAreaFieldNotFound = 11,




        RequiredFieldIsEmpty = 15,

        UniqueValueAlreadyExisted = 16,

        ReferValueNotFound = 17,

        ReferValueNotValidFilter = 117,


        VoucherValueInValid = 25,



        VoucherFieldNotFound = 29,

        VoucherFieldAlreadyExisted = 30,

        VoucherFieldIsUsed = 31,

        MapGenCodeConfigFail = 32,

        SourceVoucherTypeNotFound = 33,

        MultiRowAreaAlreadyExisted = 34,


        VoucherActionNotFound = 39,

    }
}
