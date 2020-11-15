using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class RequestOutsourceStepDetail
    {
        public int RequestOutsourceStepDetailId { get; set; }
        public int RequestOutsourceStepId { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public int Quanity { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public string DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual ProductionStepLinkData ProductionStepLinkData { get; set; }
        public virtual RequestOutsourceStep RequestOutsourceStep { get; set; }
    }
}
