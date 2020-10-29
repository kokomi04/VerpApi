using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PackingList
    {
        public PackingList()
        {
            PackingListDetail = new HashSet<PackingListDetail>();
        }

        public int PackingListId { get; set; }
        public int VoucherBillId { get; set; }
        public string ContSealNo { get; set; }
        public string PackingNote { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual ICollection<PackingListDetail> PackingListDetail { get; set; }
    }
}
