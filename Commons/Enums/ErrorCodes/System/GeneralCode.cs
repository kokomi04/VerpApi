using Verp.Resources.Enums.System;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("GEN")]
    [LocalizedDescription(ResourceType = typeof(GeneralCodeDescription))]

    public enum GeneralCode
    {

        Success = 0,


        InvalidParams = 2,


        InternalError = 3,


        X_ModuleMissing = 4,


        Forbidden = 5,


        NotYetSupported = 6,


        DistributedLockExeption = 7,


        ItemNotFound = 8,




        UserInActived = 9,




        ItemCodeExisted = 11,
        DuplicateProductionStep = 12,
    }
}
