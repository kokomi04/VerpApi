using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.PurchaseOrder
{
    public class CuttingExcessMaterialModel : IMapFrom<CuttingExcessMaterial>
    {
        public string ExcessMaterial { get; set; }
        public decimal ProductQuantity { get; set; }
        public decimal WorkpieceQuantity { get; set; }
        public string Note { get; set; }
        public string Specification { get; set; }
    }
}