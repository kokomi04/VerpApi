using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.InternalDataInterface.Manufacturing
{
   

    public sealed class InventoryRequireDetailByProductionOrderModel
    {
        public int? DepartmentId { get; set; }
        public long InventoryRequirementDetailId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public long? ProductionStepId { get; set; }
        public EnumInventoryType InventoryTypeId { get; set; }
        public long InventoryRequirementId { get; set; }
        public string InventoryRequirementCode { get; set; }
        public DateTime Date { get; set; }
        public int ProductId { get; set; }
    }

    public sealed class InventoryDetailByProductionOrderModel
    {
        public DateTime Date { get; set; }
        public long InventoryId { get; set; }
        public string InventoryCode { get; set; }
        public long InventoryDetailId { get; set; }
        public int? DepartmentId { get; set; }
        public EnumInventoryType InventoryTypeId { get; set; }
        public long? InventoryRequirementDetailId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public int ProductId { get; set; }
        public string Description { get; set; }
    }
}
