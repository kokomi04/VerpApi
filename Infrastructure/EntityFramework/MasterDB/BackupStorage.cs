using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class BackupStorage
    {
        public int DatabaseId { get; set; }
        public long BackupPoint { get; set; }
        public string Title { get; set; }
        public DateTime BackupDate { get; set; }
        public DateTime? RestoreDate { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public long FileId { get; set; }
    }
}
