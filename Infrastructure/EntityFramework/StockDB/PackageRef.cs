using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class PackageRef
    {
        public long PackageId { get; set; }
        public long RefPackageId { get; set; }
        public int? PrimaryUnitId { get; set; }
        public decimal? PrimaryQuantity { get; set; }
        public int? ProductUnitConversionId { get; set; }
        public decimal? ProductUnitConversionQuantity { get; set; }
        public DateTime? CreatedDatetimeUtc { get; set; }
        public int? PackageOperationTypeId { get; set; }

        public virtual Package Package { get; set; }
        public virtual ProductUnitConversion ProductUnitConversion { get; set; }
        public virtual Package RefPackage { get; set; }
    }
}
