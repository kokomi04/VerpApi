using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.FileResources
{
    public class FileToDownloadInfo
    {
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        public long FileLength { get; set; }
    }
}
