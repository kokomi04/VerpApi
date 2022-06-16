using System;

#nullable disable

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class GuideCate
    {
        public int GuideCateId { get; set; }
        public int? ParentId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
    }
}
