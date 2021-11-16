using Verp.Resources.Enums.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.ErrorCodes.PO
{
    [ErrorCodePrefix("POEVP")]
    [LocalizedDescription(ResourceType = typeof(ElectronicInvoiceProviderErrorCodeDescription))]

    public enum ElectronicInvoiceProviderErrorCode
    {
        EInvoiceProcessFailed = 1,
        ThisFunctionIsInDevelopment = 2,
        NotFoundXmlData = 3,
        NotFoundPatternOrSerialOfEInvoice = 4,
    }
}