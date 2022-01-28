using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchaseOrderMaterialsModel : IMapFrom<PurchaseOrderMaterials>
    {
        public long PurchaseOrderMaterialsId { get; set; }
        public long PurchaseOrderId { get; set; }
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
        public long? OutsourceRequestId { get; set; }
        public long? ProductionStepLinkDataId { get; set; }
        public int? SortOrder { get; set; }
    }
}