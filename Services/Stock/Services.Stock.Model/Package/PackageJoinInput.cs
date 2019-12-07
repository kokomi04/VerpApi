using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Package
{
    public class PackageJoinInput
    {
        public string PackageCode { get; set; }
        public int? LocationId { get; set; }
        public IList<long> FromPackageIds { get; set; }
    }
}
