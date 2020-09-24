using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class BackupStorage
    {
        public int DatabaseId { get; set; }
        public long BackupPoint { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public DateTime BackupDate { get; set; }
        public DateTime? RestoreDate { get; set; }
        public string CreatedByUserId { get; set; }
        public string UpdatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
