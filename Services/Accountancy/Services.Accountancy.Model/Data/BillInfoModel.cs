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
        public string SheetName { get; set; }
        public int FromRow { get; set; }
        public int ToRow { get; set; }
        public string Key { get; set; }
        public IList<ImportBillExcelMappingField> MappingFields { get; set; }
    }

    public class ImportBillExcelMappingField
    {
        public string FieldName { get; set; }
        public string Column { get; set; }
        public bool IsRequire { get; set; }
        public string RefTableField { get; set; }
    }
}
