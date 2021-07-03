using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class ProductionInventoryRequirementModel
    {
        public int Status { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int InventoryTypeId { get; set; }
        public long InventoryRequirementDetailId { get; set; }
        public int ProductId { get; set; }
        public int CreatedByUserId { get; set; }
        public decimal RequirementQuantity { get; set; }
        public decimal? ActualQuantity { get; set; }
        public int? AssignStockId { get; set; }
        public string StockName { get; set; }
        public long ProductionStepId { get; set; }
        public int? DepartmentId { get; set; }
        public string Content { get; set; }
        public string InventoryCode { get; set; }
        public long InventoryId { get; set; }
        public long? OutsourceStepRequestId { get; set; }
        public string InventoryRequirementCode { get; set; }
        public long InventoryRequirementId { get; set; }
    }
}
