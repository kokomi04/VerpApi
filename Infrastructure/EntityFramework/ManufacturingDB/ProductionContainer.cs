using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionContainer
    {
        public int ContainerTypeId { get; set; }
        public long ContainerId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
    }
}
