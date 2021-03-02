using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStepCollection
    {
        public long ProductionStepCollectionId { get; set; }
        public string Title { get; set; }
        public int Frequence { get; set; }
        public string Collections { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
