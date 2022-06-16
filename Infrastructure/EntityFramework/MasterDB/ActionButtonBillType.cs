using System;

#nullable disable

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class ActionButtonBillType
    {
        public int ActionButtonId { get; set; }
        public long BillTypeObjectId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ActionButton ActionButton { get; set; }
    }
}
