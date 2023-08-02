using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class ProductionStep
{
    public long ProductionStepId { get; set; }

    public string ProductionStepCode { get; set; }

    /// <summary>
    /// NULL nếu là quy trình con
    /// </summary>
    public int? StepId { get; set; }

    public string Title { get; set; }

    public string ParentCode { get; set; }

    public long? ParentId { get; set; }

    /// <summary>
    /// 1: Sản phẩm
    /// 2: Lệnh SX
    /// </summary>
    public int ContainerTypeId { get; set; }

    /// <summary>
    /// ID của Product hoặc lệnh SX
    /// </summary>
    public long ContainerId { get; set; }

    /// <summary>
    /// khoi luong cong viec
    /// </summary>
    public decimal? Workload { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int CreatedByUserId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public int SortOrder { get; set; }

    public bool? IsGroup { get; set; }

    public decimal? CoordinateX { get; set; }

    public decimal? CoordinateY { get; set; }

    public int SubsidiaryId { get; set; }

    public bool IsFinish { get; set; }

    public long? OutsourceStepRequestId { get; set; }

    public string Comment { get; set; }

    public virtual OutsourceStepRequest OutsourceStepRequest { get; set; }

    public virtual ICollection<OutsourceStepRequestData> OutsourceStepRequestData { get; set; } = new List<OutsourceStepRequestData>();

    public virtual ICollection<ProductionConsumMaterial> ProductionConsumMaterial { get; set; } = new List<ProductionConsumMaterial>();

    public virtual ICollection<ProductionHandover> ProductionHandoverFromProductionStep { get; set; } = new List<ProductionHandover>();

    public virtual ICollection<ProductionHandover> ProductionHandoverToProductionStep { get; set; } = new List<ProductionHandover>();

    public virtual ICollection<ProductionHistory> ProductionHistory { get; set; } = new List<ProductionHistory>();

    public virtual ICollection<ProductionHumanResource> ProductionHumanResource { get; set; } = new List<ProductionHumanResource>();

    public virtual ICollection<ProductionMaterialsRequirementDetail> ProductionMaterialsRequirementDetail { get; set; } = new List<ProductionMaterialsRequirementDetail>();

    public virtual ICollection<ProductionStepLinkDataRole> ProductionStepLinkDataRole { get; set; } = new List<ProductionStepLinkDataRole>();

    public virtual ProductionStepWorkInfo ProductionStepWorkInfo { get; set; }

    public virtual Step Step { get; set; }
}
