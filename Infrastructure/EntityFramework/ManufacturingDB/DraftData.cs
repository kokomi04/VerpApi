using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class DraftData
    {
        public int ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
        public int SubsidiaryId { get; set; }
        public string Data { get; set; }
    }
}
