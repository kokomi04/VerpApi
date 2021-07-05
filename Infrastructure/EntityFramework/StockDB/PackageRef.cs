using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class PackageRef
    {
        public long PackageId { get; set; }
        public long RefPackageId { get; set; }
        public decimal? PrimaryQuantity { get; set; }
        public int? ProductUnitConversionId { get; set; }
        public decimal? ProductUnitConversionQuantity { get; set; }
        public int? PackageOperationTypeId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual Package Package { get; set; }
        public virtual ProductUnitConversion ProductUnitConversion { get; set; }
        public virtual Package RefPackage { get; set; }
    }
}
