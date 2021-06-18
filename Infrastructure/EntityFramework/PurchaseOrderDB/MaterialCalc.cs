using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class MaterialCalc
    {
        public MaterialCalc()
        {
            MaterialCalcProduct = new HashSet<MaterialCalcProduct>();
            MaterialCalcSummary = new HashSet<MaterialCalcSummary>();
        }

        public long MaterialCalcId { get; set; }
        public int SubsidiaryId { get; set; }
        public string MaterialCalcCode { get; set; }
        public string Title { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<MaterialCalcProduct> MaterialCalcProduct { get; set; }
        public virtual ICollection<MaterialCalcSummary> MaterialCalcSummary { get; set; }
    }
}
