using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductSemi
    {
        public ProductSemi()
        {
            ProductSemiConversion = new HashSet<ProductSemiConversion>();
        }

        public long ProductSemiId { get; set; }
        public long ContainerId { get; set; }
        public int ContainerTypeId { get; set; }
        public string Title { get; set; }
        public string Specification { get; set; }
        public int UnitId { get; set; }
        public string Note { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }
        public int? DecimalPlace { get; set; }

        public virtual ICollection<ProductSemiConversion> ProductSemiConversion { get; set; }
    }
}
