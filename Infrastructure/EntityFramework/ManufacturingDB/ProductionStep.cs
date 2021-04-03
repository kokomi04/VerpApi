using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStep
    {
        public ProductionStep()
        {
            OutsourceStepRequestData = new HashSet<OutsourceStepRequestData>();
            ProductionMaterialsRequirementDetail = new HashSet<ProductionMaterialsRequirementDetail>();
            ProductionStepLinkDataRole = new HashSet<ProductionStepLinkDataRole>();
        }

        public long ProductionStepId { get; set; }
        public string ProductionStepCode { get; set; }
        public int? StepId { get; set; }
        public string Title { get; set; }
        public string ParentCode { get; set; }
        public long? ParentId { get; set; }
        public int ContainerTypeId { get; set; }
        public long ContainerId { get; set; }
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

        public virtual OutsourceStepRequest OutsourceStepRequest { get; set; }
        public virtual Step Step { get; set; }
        public virtual ProductionStepWorkInfo ProductionStepWorkInfo { get; set; }
        public virtual ICollection<OutsourceStepRequestData> OutsourceStepRequestData { get; set; }
        public virtual ICollection<ProductionMaterialsRequirementDetail> ProductionMaterialsRequirementDetail { get; set; }
        public virtual ICollection<ProductionStepLinkDataRole> ProductionStepLinkDataRole { get; set; }
    }
}
