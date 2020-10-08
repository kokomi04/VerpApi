using System;
using System.Collections.Generic;
using System.Text;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Services.PurchaseOrder.Model.Voucher
{
    public class VoucherTypeBillsRequestModel
    {
        public string Keyword { get; set; }
        public Dictionary<int, object> Filters { get; set; }
        public string OrderBy { get; set; }
        public bool Asc { get; set; } = true;
        public int Page { get; set; }
        public int Size { get; set; }

        public Clause ColumnsFilters { get; set; }
    }
}
