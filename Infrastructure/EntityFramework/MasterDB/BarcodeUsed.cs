using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class BarcodeUsed
    {
        public int BarcodeUsedId { get; set; }
        public int BarcodeConfigId { get; set; }
        public int ProductCode { get; set; }
        public bool IsActived { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
    }
}
