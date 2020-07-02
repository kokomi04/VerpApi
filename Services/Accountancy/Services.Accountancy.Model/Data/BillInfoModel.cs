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

    public class ImportBillExelMapping
    {
        public string SheetInfo { get; set; }
        public int FromInfo { get; set; }
        public int ToInfo { get; set; }
        public string SheetRow { get; set; }
        public int FromRow { get; set; }
        public int ToRow { get; set; }
        public IList<ImportBillExcelMappingField> MappingInfoFields { get; set; }
        public IList<ImportBillExcelMappingField> MappingRowFields { get; set; }
    }

    public class ImportBillExcelMappingField
    {
        public string FieldName { get; set; }
        public string Column { get; set; }
        public bool IsRequire { get; set; }
        public string RefTableField { get; set; }
    }
}
