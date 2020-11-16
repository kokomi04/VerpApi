using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class RequestOutsourceStep
    {
        public RequestOutsourceStep()
        {
            RequestOutsourceStepDetail = new HashSet<RequestOutsourceStepDetail>();
        }

        public long RequestOutsourceStepId { get; set; }
        public string RequestOutsourceStepCode { get; set; }
        public int ProductionOrderId { get; set; }
        public DateTime DateRequiredComplete { get; set; }
        public string PathStepId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public string DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual ICollection<RequestOutsourceStepDetail> RequestOutsourceStepDetail { get; set; }
    }
}
