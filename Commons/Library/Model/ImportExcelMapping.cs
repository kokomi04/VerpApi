using System.Collections.Generic;
using System.Reflection;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;

namespace VErp.Commons.Library.Model
{
    public class ImportExcelMapping
    {
        public string FileName { get; set; }
        public string SheetName { get; set; }
        public int FromRow { get; set; }
        public int? ToRow { get; set; }
        public EnumImportDuplicateOption? ImportDuplicateOptionId { get; set; }
        public IList<ImportExcelMappingField> MappingFields { get; set; }
        public bool? ConfirmFlag { get; set; }
        public EnumHandleFilterOption? HandleFilterOptionId { get; set;}
    }

    public class ImportExcelMappingExtra<T>
    {
        public ImportExcelMapping Mapping { get; set; }
        public T Extra { get; set; }
    }

    public class ImportExcelMappingField
    {
        public string FieldName { get; set; }
        public string RefFieldName { get; set; }
        public string Column { get; set; }
        public bool IsIgnoredIfEmpty { get; set; }
        public bool IsIdentityDetail { get; set; }
    }

    public class ImportExcelRowData
    {
        public ImportExcelMappingField FieldMapping { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public string CellValue { get; set; }
    }

    public class BillParseMapping
    {
        public BillInfoModel Bill { get; set; }
        public int AreaId { get; set; }
        public ImportExcelMapping Mapping { get; set; }
    }
}
