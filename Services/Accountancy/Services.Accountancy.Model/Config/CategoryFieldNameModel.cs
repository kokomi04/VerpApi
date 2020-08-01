using System;
using System.Collections.Generic;
using System.Text;
using VErp.Infrastructure.EF.AccountancyDB;
using CategoryEntity = VErp.Infrastructure.EF.AccountancyDB.Category;

namespace VErp.Services.Accountancy.Model.Category
{
    public class CategoryNameModel
    {
        public int CategoryId { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryTitle { get; set; }
        public bool IsTreeView { get; set; }
        public IList<CategoryFieldNameModel> Fields { get; set; }
    }

    public class CategoryFieldNameModel
    {
        public int CategoryFieldId { get; set; }
        public string FieldName { get; set; }
        public string FieldTitle { get; set; }
        public CategoryNameModel RefCategory { get; set; }
    }

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
