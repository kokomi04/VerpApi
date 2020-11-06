using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductInStep
    {
        public ProductInStep()
        {
            InOutStepLink = new HashSet<InOutStepLink>();
            RequestOutsourcePart = new HashSet<RequestOutsourcePart>();
        }

        public int ProductInStepId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SortOrder { get; set; }

        public virtual ICollection<InOutStepLink> InOutStepLink { get; set; }
        public virtual ICollection<RequestOutsourcePart> RequestOutsourcePart { get; set; }
    }
}
