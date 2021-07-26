using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PropertyCalc
    {
        public PropertyCalc()
        {
            CuttingWorkSheet = new HashSet<CuttingWorkSheet>();
            PropertyCalcProduct = new HashSet<PropertyCalcProduct>();
            PropertyCalcProperty = new HashSet<PropertyCalcProperty>();
            PropertyCalcSummary = new HashSet<PropertyCalcSummary>();
            PurchasingRequest = new HashSet<PurchasingRequest>();
        }

        public long PropertyCalcId { get; set; }
        public int SubsidiaryId { get; set; }
        public string PropertyCalcCode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<CuttingWorkSheet> CuttingWorkSheet { get; set; }
        public virtual ICollection<PropertyCalcProduct> PropertyCalcProduct { get; set; }
        public virtual ICollection<PropertyCalcProperty> PropertyCalcProperty { get; set; }
        public virtual ICollection<PropertyCalcSummary> PropertyCalcSummary { get; set; }
        public virtual ICollection<PurchasingRequest> PurchasingRequest { get; set; }
    }
}
