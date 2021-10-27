using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.E_Invoice
{
    public enum EnumElectronicInvoiceFunction
    {
        [Description("Tạo hóa đơn")]
        Create = 1,
        [Description("Điều chỉnh hóa đơn")]
        Modify = 2,
        [Description("Xem hóa đơn (html)")]
        GetHtml = 3,
        [Description("Xem hóa đơn (pdf)")]
        GetPdf = 4,
        [Description("Phát hành hóa đơn")]
        Publish = 5,
        [Description("Phát hành hóa đơn tạm")]
        PublishTemp = 6,
    }
}
