using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB;

public partial class InventoryRequirement
{
    public long InventoryRequirementId { get; set; }

    public int SubsidiaryId { get; set; }

    public string InventoryRequirementCode { get; set; }

    public int InventoryRequirementTypeId { get; set; }

    public int InventoryOutsideMappingTypeId { get; set; }

    public int InventoryTypeId { get; set; }

    public string Content { get; set; }

    public DateTime Date { get; set; }

    public int? DepartmentId { get; set; }

    public int CreatedByUserId { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public int? CensorByUserId { get; set; }

    public DateTime? CensorDatetimeUtc { get; set; }

    public int CensorStatus { get; set; }

    public int? CustomerId { get; set; }

    public int ModuleTypeId { get; set; }

    public int? ProductMaterialsConsumptionGroupId { get; set; }

    public long? ProductionOrderMaterialSetId { get; set; }

    public virtual ICollection<InventoryRequirementDetail> InventoryRequirementDetail { get; set; } = new List<InventoryRequirementDetail>();

    public virtual ICollection<InventoryRequirementFile> InventoryRequirementFile { get; set; } = new List<InventoryRequirementFile>();
}
