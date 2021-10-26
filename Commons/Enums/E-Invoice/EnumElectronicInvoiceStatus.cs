namespace VErp.Commons.Enums.E_Invoice
{
    public enum EnumElectronicInvoiceStatus
    {
        EInvoiceNotExists = -1,
        EInvoiceWithoutDigitalSignature = 0,
        EInvoiceWithDigitalSignature = 1,
        EInvoiceWitDigitalSignature = 2,
        EInvoiceDeclaredTax = 3,
        EInvoiceReplaced = 4,
        EInvoiceAdjusted = 5,
        EInvoiceCanceled = 6,
        EInvoiceApproved = 7,
    }
}