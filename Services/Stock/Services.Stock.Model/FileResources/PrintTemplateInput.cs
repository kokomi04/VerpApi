using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.FileResources
{
    public class PrintTemplateInput
    {
        public string Extension { get; set; }
        public Dictionary<string, string> dataReplace { get; set; }
        public IList<string[][]> dataTable { get; set; }
    }
}
