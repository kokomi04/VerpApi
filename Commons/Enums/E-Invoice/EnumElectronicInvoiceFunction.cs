﻿using System.ComponentModel;

namespace VErp.Commons.Enums.E_Invoice
{
    public enum EnumElectronicInvoiceFunction
    {
        [Description("Phát hành hóa đơn")]
        Issue = 1,
        [Description("Điều chỉnh hóa đơn")]
        Adjust = 2,
        // [Description("Xem hóa đơn (html)")]
        // GetHtml = 3,
        [Description("Xem hóa đơn (pdf)")]
        GetPdf = 4,
        // [Description("Phát hành hóa đơn tạm")]
        // IssueTemp = 6,
        [Description("Thay thế hóa đơn")]
        Replace = 7,
        [Description("Hủy bỏ hóa đơn")]
        Cancel = 8,
    }
}
