using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.PurchaseOrder
{
    public class CuttingWorkSheetDestModel : IMapFrom<CuttingWorkSheetDest>
    {
        public long CuttingWorkSheetId { get; set; }
        public int ProductId { get; set; }
        public decimal ProductQuantity { get; set; }
    }
}