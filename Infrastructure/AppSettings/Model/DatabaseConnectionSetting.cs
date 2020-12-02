using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Infrastructure.AppSettings.Model
{
    public class DatabaseConnectionSetting
    {
        public string MasterDatabase { get; set; }
        public string StockDatabase { get; set; }
        public string PurchaseOrderDatabase { get; set; }
        public string IdentityDatabase { get; set; }
        public string OrganizationDatabase { get; set; }
        //public string AccountingDatabase { get; set; }
        public string AccountancyDatabase { get; set; }
        public string ReportConfigDatabase { get; set; }
        public string ActivityLogDatabase { get; set; }
        public string ManufacturingDatabase { get; set; }
    }
}
