using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class PackageOperation
    {
        public long PackageOperationId { get; set; }
        public int PackageOperationTypeId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
    }
}
