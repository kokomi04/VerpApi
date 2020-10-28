using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.PackingList
{
    public class PackingListDetailModel : IMapFrom<PackingListDetail>
    {
        public int PackingListDetailId { get; set; }
        public int PackingListId { get; set; }
        public int VoucherValueRowId { get; set; }
        public int ActualNumber { get; set; }
        public decimal NetWeight { get; set; }
        public decimal GrossWeight { get; set; }
        public decimal CubicMeter { get; set; }
    }
}
