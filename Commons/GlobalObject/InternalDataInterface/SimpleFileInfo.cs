using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class SimpleFileInfo
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long? FileLength { get; set; }
        public int FileTypeId { get; set; }
    }
}
