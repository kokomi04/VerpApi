using System.ComponentModel;

namespace VErp.Commons.Enums.E_Invoice
{
    public enum EnumElectronicInvoiceStatus : int
    {
        [Description("Hóa đơn không tồn tại")]
        EInvoiceNotExists = 0,

        [Description("Hóa đơn chưa có chữ ký số")]
        EInvoiceWithoutDigitalSignature = 1,

        [Description("Hóa đơn có chữ ký số")]
        EInvoiceWithDigitalSignature = 2,

        [Description("Hóa đơn đã khai báo thuế")]
        EInvoiceDeclaredTax = 3,

        [Description("Hóa đơn bị thay thế")]
        EInvoiceReplaced = 4,

        [Description("Hóa đơn bị điều chỉnh")]
        EInvoiceAdjusted = 5,

        [Description("Hóa đơn bị hủy")]
        EInvoiceCanceled = 6,

        [Description("Hóa đơn đã duyệt")]
        EInvoiceApproved = 7,
    }
}