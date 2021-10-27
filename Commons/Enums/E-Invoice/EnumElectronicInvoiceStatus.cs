namespace VErp.Commons.Enums.E_Invoice
{
    public enum EnumElectronicInvoiceStatus: int
    {
        EInvoiceNotExists = 0,
        EInvoiceWithoutDigitalSignature = 1,
        EInvoiceWithDigitalSignature = 2,
        EInvoiceDeclaredTax = 3,
        EInvoiceReplaced = 4,
        EInvoiceAdjusted = 5,
        EInvoiceCanceled = 6,
        EInvoiceApproved = 7,
    }
}