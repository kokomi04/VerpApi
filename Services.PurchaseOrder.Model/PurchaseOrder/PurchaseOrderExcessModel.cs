using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchaseOrderExcessModel : IMapFrom<PurchaseOrderExcess>
    {
        public long PurchaseOrderExcessId { get; set; }
        public long PurchaseOrderId { get; set; }
        public string Title { get; set; }
        public int UnitId { get; set; }
        public string Specification { get; set; }
        public decimal Quantity { get; set; }
        public int? DecimalPlace { get; set; } = 12;
        public long? ProductId { get; set; }
        public int? SortOrder { get; set; }
    }
}