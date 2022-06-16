using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class ProductionHandoverByDateModel
    {
        public int RowNumber { get; set; }
        public long ProductionOrderId { get; set; }
        public int FromDepartmentId { get; set; }
        public long FromProductionStepId { get; set; }
        public long FromStepId { get; set; }

        public EnumProductionStepLinkDataObjectType LinkDataObjectTypeId { get; set; }
        public long LinkDataObjectId { get; set; }

        public int ToDepartmentId { get; set; }
        public long ToProductionStepId { get; set; }
        public long ToStepId { get; set; }
        public decimal AssignmentQuantity { get; set; }
        public decimal Quantity { get; set; }
        public long Date { get; set; }
        public string Note { get; set; }
    }
}
