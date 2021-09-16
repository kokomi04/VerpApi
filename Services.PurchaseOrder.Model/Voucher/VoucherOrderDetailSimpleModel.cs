using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model.Voucher
{
    public class VoucherOrderDetailSimpleModel
    {
        public long OrderId { get; set; }
        public string OrderCode { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
    }
}
