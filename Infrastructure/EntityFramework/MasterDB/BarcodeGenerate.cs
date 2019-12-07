using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class BarcodeGenerate
    {
        public int BarcodeGenerateId { get; set; }
        public DateTime GeneratedDatetimeUtc { get; set; }
        public bool IsUsed { get; set; }
    }
}
