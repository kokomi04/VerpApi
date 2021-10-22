using Verp.Resources.Enums.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.ErrorCodes.PO
{
    [ErrorCodePrefix("POEVM")]
    [LocalizedDescription(ResourceType = typeof(ElectronicInvoiceMappingErrorCodeDescription))]

    public enum ElectronicInvoiceMappingErrorCode
    {
        ExistsElectronicInvoiceMapping = 1,
        NotFoundElectronicInvoiceMapping = 2,
        EInvoiceCreateProcessFailed = 3,
    }
}