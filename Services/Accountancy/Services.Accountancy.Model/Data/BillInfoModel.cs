using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Accountancy.Model.Data
{
    public class BillInfoModel
    {
        public NonCamelCaseDictionary Info { get; set; }
        public IList<NonCamelCaseDictionary> Rows { get; set; }
        public OutsideImportMappingData OutsideImportMappingData { get; set; }
    }

    public class OutsideImportMappingData
    {
        public string MappingFunctionKey { get; set; }
        public string ObjectId { get; set; }
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
