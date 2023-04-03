using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class CustomGenCode
    {
        public int CustomGenCodeId { get; set; }
        public int SubsidiaryId { get; set; }
        public int? ParentId { get; set; }
        public string CustomGenCodeName { get; set; }
        public int CodeLength { get; set; }
        public string LastCode { get; set; }
        public bool IsActived { get; set; }
        public bool IsDeleted { get; set; }
        public int? UpdatedUserId { get; set; }
        public DateTime? ResetDate { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsDefault { get; set; }
        public string BaseFormat { get; set; }
        public string CodeFormat { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
    }
}
