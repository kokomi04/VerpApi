using Verp.Resources.Enums.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.ErrorCodes.PO
{
    [ErrorCodePrefix("POEVC")]
    [LocalizedDescription(ResourceType = typeof(ElectronicInvoiceConfigErrorCodeDescription))]

    public enum ElectronicInvoiceConfigErrorCode
    {
        NotFoundElectronicInvoiceConfig = 1,
        NotFoundElectronicInvoiceFunction = 2,
    }
}