using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.FileResources
{
    public class VisualFile
    {
        public string file { get; set; }
        public string path { get; set; }
        public string ext { get; set; }
        public long size { get; set; }
        public long time { get; set; }
    }
}
