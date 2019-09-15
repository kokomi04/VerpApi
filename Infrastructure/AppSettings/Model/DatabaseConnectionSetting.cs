using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Infrastructure.AppSettings.Model
{
    public class DatabaseConnectionSetting
    {
        public string MasterDatabase { get; set; }
        public string StockDatabase { get; set; }
        public string IdentityDatabase { get; set; }
    }
}
