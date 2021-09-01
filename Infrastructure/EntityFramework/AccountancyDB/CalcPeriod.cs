using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.AccountancyDB
{
    public partial class CalcPeriod
    {
        public long CalcPeriodId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CalcPeriodTypeId { get; set; }
        public string FilterHash { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string FilterData { get; set; }
        public string Data { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }
    }
}
