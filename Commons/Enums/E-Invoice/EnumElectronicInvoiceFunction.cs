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
        Adjust = 2,
        [Description("Xem hóa đơn (html)")]
        GetHtml = 3,
        [Description("Xem hóa đơn (pdf)")]
        GetPdf = 4,
        [Description("Phát hành hóa đơn")]
        Issue = 5,
        [Description("Phát hành hóa đơn tạm")]
        IssueTemp = 6,
        [Description("Thay thế hóa đơn")]
        Replace = 7,
        [Description("Hủy bỏ hóa đơn")]
        Cancel = 8,
    }
}
