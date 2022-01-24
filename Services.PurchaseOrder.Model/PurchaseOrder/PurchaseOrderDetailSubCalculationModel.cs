using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchaseOrderDetailSubCalculationModel : IMapFrom<PurchaseOrderDetailSubCalculation>
    {
        public int PurchaseOrderDetailSubCalculationId { get; set; }
        public long PurchaseOrderDetailId { get; set; }
        public long ProductBomId { get; set; }
        public decimal PrimaryUnitPrice { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public int? UnitConversionId { get; set; }
    }
}