using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchaseOrderOrderMappingModel : IMapFrom<PurchaseOrderOrderMapping>
    {
        public long PurchaseOrderOrderMappingId { get; set; }

        public long PurchaseOrderDetailId { get; set; }

        public string OrderCode { get; set; }

        public decimal? PrimaryQuantity { get; set; }

        public decimal? PuQuantity { get; set; }

        public string? Note { get; set; }
    }
}