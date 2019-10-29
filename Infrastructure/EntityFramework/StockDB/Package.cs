using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class Package
    {
        public long PackageId { get; set; }
        public string PackageCode { get; set; }
        public int? LocationId { get; set; }
        public int StockId { get; set; }
    }
}
