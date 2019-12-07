using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.FileResources
{
    public class FileThumbnailInfo
    {     
        public string ThumbnailUrl { get; set; }
        public string FileName { get; set; }
    }
    public class FileToDownloadInfo: FileThumbnailInfo
    {
        public string FileUrl { get; set; }     
        public long FileLength { get; set; }

        public long? FileId { set; get; }
    }
}
