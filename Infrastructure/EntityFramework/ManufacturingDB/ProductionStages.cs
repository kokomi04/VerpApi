using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStages
    {
        public int ProductionStagesId { get; set; }
        public int ProductionStagesType { get; set; }
        public string ProductionStagesTitle { get; set; }
        public int? ProductionStagesParent { get; set; }
        public int ProductId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SortOrder { get; set; }
    }
}
