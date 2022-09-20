using AutoMapper;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class ProductionHistoryModel : ProductionHistoryInputModel
    {
        
        public long ProductionHandoverReceiptId { get; set; }
        public string ProductionHandoverReceiptCode { get; set; }

        public int CreatedByUserId { get; set; }

      
    }

    public class ProductionHistoryInputModel : IMapFrom<ProductionHistory>
    {
        public long? ProductionHistoryId { get; set; }
        public decimal ProductionQuantity { get; set; }
        public decimal? OvertimeProductionQuantity { get; set; }
        public long ObjectId { get; set; }
        public EnumProductionProcess.EnumProductionStepLinkDataObjectType ObjectTypeId { get; set; }
        public int DepartmentId { get; set; }
        public long ProductionStepId { get; set; }
        public long? Date { get; set; }
        public string Note { get; set; }
        public long ProductionOrderId { get; set; }
       
    }
}
