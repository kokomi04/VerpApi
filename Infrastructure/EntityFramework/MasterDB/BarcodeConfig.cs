using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class BarcodeConfig
    {
        public int BarcodeConfigId { get; set; }
        public int SubsidiaryId { get; set; }
        public string Name { get; set; }
        public int BarcodeStandardId { get; set; }
        public string ConfigurationJson { get; set; }
        public bool IsActived { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
    }
}
