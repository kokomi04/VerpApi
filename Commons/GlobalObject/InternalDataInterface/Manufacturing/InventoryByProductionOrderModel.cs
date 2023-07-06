using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.InternalDataInterface.Manufacturing
{
    public class InventoryByProductionOrderModel
    {
        public required long? InventoryRequirementDetailId { get; set; }
        public required long? InventoryRequirementId { get; set; }
        public required string? InventoryRequirementCode { get; set; }
        public required long? ProductionStepId { get; set; }
        public required int? DepartmentId { get; set; }
        public required EnumInventoryType InventoryTypeId { get; set; }
        public required int ProductId { get; set; }
        public required decimal? RequireQuantity { get; set; }

        public required DateTime InventoryDate {  get; set; }        
        public required long InventoryDetailId { get; set; }
        public required long InventoryId { get; set; }
        public required string InventoryCode { get; set; }
        
        public required decimal InventoryQuantity { get; set; }

        public required string Content { get; set; }
    }
}
