using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Accountancy.Model.Data
{
    public class BillInfoModel
    {
        public Dictionary<string, string> Info { get; set; }
        public Dictionary<string, string>[] Rows { get; set; }
    }
}
