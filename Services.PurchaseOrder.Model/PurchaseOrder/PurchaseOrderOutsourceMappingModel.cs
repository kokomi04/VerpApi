using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchaseOrderOutsourceMappingModel : IMapFrom<PurchaseOrderOutsourceMapping>
    {
        public long PurchaseOrderOutsourceMappingId { get; set; }
        public long PurchaseOrderDetailId { get; set; }
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
        public string ProductionOrderCode { get; set; }
        public string OrderCode { get; set; }
        public long OutsourcePartRequestId { get; set; }
        public long? ProductionStepLinkDataId { get; set; }
    }
}