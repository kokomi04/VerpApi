using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class MaterialCalc
    {
        public MaterialCalc()
        {
            MaterialCalcConsumptionGroup = new HashSet<MaterialCalcConsumptionGroup>();
            MaterialCalcProduct = new HashSet<MaterialCalcProduct>();
            MaterialCalcSummary = new HashSet<MaterialCalcSummary>();
            PurchasingRequest = new HashSet<PurchasingRequest>();
        }

        public long MaterialCalcId { get; set; }
        public int SubsidiaryId { get; set; }
        public string MaterialCalcCode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public long? PurchasingSuggestId { get; set; }

        public virtual ICollection<MaterialCalcConsumptionGroup> MaterialCalcConsumptionGroup { get; set; }
        public virtual ICollection<MaterialCalcProduct> MaterialCalcProduct { get; set; }
        public virtual ICollection<MaterialCalcSummary> MaterialCalcSummary { get; set; }
        public virtual ICollection<PurchasingRequest> PurchasingRequest { get; set; }
    }
}
