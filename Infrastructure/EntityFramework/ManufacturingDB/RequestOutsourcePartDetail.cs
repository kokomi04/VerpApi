using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class RequestOutsourcePartDetail
    {
        public int RequestOutsourcePartDetailId { get; set; }
        public int RequestOutsourcePartId { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public int Quanity { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ProductionStepLinkData ProductionStepLinkData { get; set; }
        public virtual RequestOutsourcePart RequestOutsourcePart { get; set; }
    }
}
