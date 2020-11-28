using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace VErp.Commons.Library.Model
{
    public class ImportExcelMapping
    {
        public string SheetName { get; set; }
        public int FromRow { get; set; }
        public int ToRow { get; set; }

        public IList<ImportExcelMappingField> MappingFields { get; set; }
    }

    public class ImportExcelMappingField
    {
        public string FieldName { get; set; }
        public string RefFieldName { get; set; }
        public string Column { get; set; }
        public bool IsRequire { get; set; }
    }

    public class ImportExcelRowData
    {
        public ImportExcelMappingField FieldMapping { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public string CellValue { get; set; }
    }
}
