using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Package
{
    public class PackageSplitInput
    {
        public IList<PackageSplitDetailModel> ToPackages { get; set; }
    }

    public class PackageSplitDetailModel
    {
        public string PackageCode { get; set; }
        public int? LocationId { get; set; }
        public decimal SecondaryUnitQualtity { get; set; }
    }
}
