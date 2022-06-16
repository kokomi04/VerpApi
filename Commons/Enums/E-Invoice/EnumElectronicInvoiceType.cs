using System.ComponentModel;

namespace VErp.Commons.Enums.E_Invoice
{
    public enum EnumElectronicInvoiceType
    {
        [Description("Hóa đơn thông thường")]
        ElectronicInvoiceNormal = 1,

        [Description("Hóa đơn điều chỉnh")]
        ElectronicInvoiceModify = 2,

        [Description("Hóa đơn thay thế")]
        ElectronicInvoiceReplace = 3,
    }
}