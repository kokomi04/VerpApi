using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class PackageRef
    {
        public long PackageId { get; set; }
        public long RefPackageId { get; set; }
    }
}
