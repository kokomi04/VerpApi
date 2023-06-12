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

        HrAreaFieldNotFound = 11,




        RequiredFieldIsEmpty = 15,

        UniqueValueAlreadyExisted = 16,

        ReferValueNotFound = 17,




        HrValueInValid = 25,



        HrFieldNotFound = 29,

        HrFieldAlreadyExisted = 30,

        HrFieldIsUsed = 31,

        MapGenCodeConfigFail = 32,

        SourceHrTypeNotFound = 33,
        // [Description("Vùng dữ liệu dạng bảng đã tồn tại")]
        // MultiRowAreaAlreadyExisted = 34,




        HrActionNotFound = 39,



        ReferValueNotValidFilter = 42,
        HrFieldDataSizeInValid = 48,

        HrBillInUsed = 49,
    }
}
