using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountancyDB
{
    public partial class AccountancyOutsiteMapping
    {
        public int AccountancyOutsiteMappingId { get; set; }
        public int? AccountancyOutsiteMappingFunctionId { get; set; }
        public string SourceFieldName { get; set; }
        public string DestinationFieldName { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
    }
}
