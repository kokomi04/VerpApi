using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.PurchaseOrder
{
    public class CuttingWorkSheetFileModel : IMapFrom<CuttingWorkSheetFile>
    {
        public long FileId { get; set; }
    }
}