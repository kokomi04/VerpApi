using System.Collections.Generic;

namespace VErp.Services.Stock.Model.Package
{
    public class PackageJoinInput : INewPackageBase
    {
        public string PackageCode { get; set; }
        public int? LocationId { get; set; }
        public IList<long> FromPackageIds { get; set; }
    }
}
