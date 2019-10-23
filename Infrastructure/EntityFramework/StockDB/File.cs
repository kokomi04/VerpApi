using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class File
    {
        public long FileId { get; set; }
        public int FileTypeId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int ObjectTypeId { get; set; }
        public long? ObjectId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public int FileStatusId { get; set; }
        public string ContentType { get; set; }
        public long? FileLength { get; set; }
    }
}
