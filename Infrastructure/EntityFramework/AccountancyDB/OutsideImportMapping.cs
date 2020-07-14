using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountancyDB
{
    public partial class OutsideImportMapping
    {
        public long OutsideImportMappingId { get; set; }
        public int? OutsideImportMappingFunctionId { get; set; }
        public bool IsDetail { get; set; }
        public string SourceFieldName { get; set; }
        public string DestinationFieldName { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual OutsideImportMappingFunction OutsideImportMappingFunction { get; set; }
    }
}
