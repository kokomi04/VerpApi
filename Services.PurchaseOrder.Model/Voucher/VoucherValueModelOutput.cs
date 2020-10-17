using System;
using System.Collections.Generic;

namespace VErp.Services.PurchaseOrder.Model.Voucher
{

    public class VoucherValueOuputModel
    {
        public VoucherValueOuputModel()
        {
            Rows = new List<VoucherRowOutputModel>();
        }
        public ICollection<VoucherRowOutputModel> Rows { get; set; }
    }

    public class VoucherRowOutputModel
    {
        public VoucherRowOutputModel()
        {
            FieldValues = new Dictionary<int, string>();
        }
        public int VoucherAreaId { get; set; }
        public long VoucherValueRowId { get; set; }
        public IDictionary<int, string> FieldValues { get; set; }
    }
}
