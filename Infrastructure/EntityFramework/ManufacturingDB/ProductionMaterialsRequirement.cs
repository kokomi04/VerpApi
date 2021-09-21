using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionMaterialsRequirement
    {
        public ProductionMaterialsRequirement()
        {
            ProductionMaterialsRequirementDetail = new HashSet<ProductionMaterialsRequirementDetail>();
        }

        public long ProductionMaterialsRequirementId { get; set; }
        public string RequirementCode { get; set; }
        public DateTime RequirementDate { get; set; }
        public string RequirementContent { get; set; }
        public long ProductionOrderId { get; set; }
        public int? CensorByUserId { get; set; }
        public DateTime? CensorDatetimeUtc { get; set; }
        public int CensorStatus { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual ProductionOrder ProductionOrder { get; set; }
        public virtual ICollection<ProductionMaterialsRequirementDetail> ProductionMaterialsRequirementDetail { get; set; }
    }
}
