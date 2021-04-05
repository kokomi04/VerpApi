using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductSemiConversion
    {
        public long ProductSemiConversionId { get; set; }
        public long ProductSemiId { get; set; }
        public int ConversionGroup { get; set; }
        public int ConversionTypeId { get; set; }
        public long ConversionId { get; set; }
        public decimal ConversionRate { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ProductSemi ProductSemi { get; set; }
    }
}
