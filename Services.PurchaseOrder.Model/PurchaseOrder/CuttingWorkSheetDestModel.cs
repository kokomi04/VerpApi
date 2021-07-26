using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.PurchaseOrder
{
    public class CuttingWorkSheetDestModel : IMapFrom<CuttingWorkSheetDest>
    {
        public int ProductId { get; set; }
        public decimal ProductQuantity { get; set; }
        public decimal WorkpieceQuantity { get; set; }
        public string Note { get; set; }
    }
}