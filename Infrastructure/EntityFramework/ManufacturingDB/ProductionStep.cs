using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStep
    {
        public ProductionStep()
        {
            ProductionAssignment = new HashSet<ProductionAssignment>();
            ProductionStepLinkDataRole = new HashSet<ProductionStepLinkDataRole>();
            ProductionStepOrder = new HashSet<ProductionStepOrder>();
        }

        public long ProductionStepId { get; set; }
        public string ProductionStepCode { get; set; }
        public int? StepId { get; set; }
        public string Title { get; set; }
        public string ParentCode { get; set; }
        public long? ParentId { get; set; }
        public int ContainerTypeId { get; set; }
        public long ContainerId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SortOrder { get; set; }
        public bool? IsGroup { get; set; }
        public int? CoordinateX { get; set; }
        public int? CoordinateY { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual Step Step { get; set; }
        public virtual ICollection<ProductionAssignment> ProductionAssignment { get; set; }
        public virtual ICollection<ProductionStepLinkDataRole> ProductionStepLinkDataRole { get; set; }
        public virtual ICollection<ProductionStepOrder> ProductionStepOrder { get; set; }
    }
}
