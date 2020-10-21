using System;
using System.Collections.Generic;
using System.Text;
using VErp.Infrastructure.EF.MasterDB;
using CategoryEntity = VErp.Infrastructure.EF.MasterDB.Category;

namespace VErp.Services.Master.Model.Category
{
   
    public class CategoryImportExelMapping
    {
        public string SheetName { get; set; }
        public int FromRow { get; set; }
        public int ToRow { get; set; }
        public IList<CategoryImportExcelMappingField> MappingFields { get; set; }
    }

    public class CategoryImportExcelMappingField
    {
        public string FieldName { get; set; }
        public string Column { get; set; }
        public bool IsRequire { get; set; }
        public int? RefFieldId { get; set; }
        public string RefFieldName { get; set; }
    }

    public class CategoryImportExcelRowData
    {
        public CategoryImportExcelMappingField FieldMapping { get; set; }
        public CategoryField FieldConfig { get; set; }
        public string CellValue { get; set; }
    }    


}
