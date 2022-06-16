using System;
using System.Collections.Generic;
using System.Text;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Commons.GlobalObject;

namespace VErp.Services.PurchaseOrder.Model.Voucher
{
    public class VoucherTypeBillsFilterModel
    {
        public string Keyword { get; set; }
        public Dictionary<int, object> Filters { get; set; }
        public string OrderBy { get; set; }
        public bool Asc { get; set; } = true;
        public long? FromDate { get; set; }
        public long? ToDate { get; set; }
        public Clause ColumnsFilters { get; set; }
    }

    public class VoucherTypeBillsFilterPagingModel: VoucherTypeBillsFilterModel
    {
        public bool IsMultirow { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
    }

    public class VoucherTypeBillsExportModel: VoucherTypeBillsFilterModel
    {
      public IList<string> FieldNames { get; set; }
    }
}
