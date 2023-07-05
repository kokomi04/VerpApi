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


        InputAreaFieldNotFound = 11,

        RequireValueNotValidFilter = 14,

        RequiredFieldIsEmpty = 15,

        UniqueValueAlreadyExisted = 16,

        ReferValueNotFound = 17,

        ReferValueNotValidFilter = 117,



        InputValueInValid = 25,



        InputFieldNotFound = 29,

        InputFieldAlreadyExisted = 30,

        InputFieldIsUse = 31,//in use vs in used

        MapGenCodeConfigFail = 32,

        SourceInputTypeNotFound = 33,

        MultiRowAreaAlreadyExisted = 34,

        MultiRowAreaEmpty = 35,


        PrintConfigNotFound = 36,



        InputActionNotFound = 39,

        InputActionCodeAlreadyExisted = 40,
        InputFieldDataSizeInValid = 48,
    }
}
