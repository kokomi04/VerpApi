using System;
using System.Collections.Generic;
using System.Text;
using VErp.Infrastructure.EF.AccountingDB;
using CategoryEntity = VErp.Infrastructure.EF.AccountingDB.Category;

namespace VErp.Services.Accountant.Model.Category
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

    public class ImportExelMapping
    {
        public string SheetName { get; set; }
        public int FromRow { get; set; }
        public int ToRow { get; set; }
        public IList<ImportExcelMappingField> MappingFields { get; set; }
    }

    public class ImportExcelMappingField
    {
        public int? FieldId { get; set; }
        public string Column { get; set; }
        public bool IsRequire { get; set; }
        public int? RefCategoryFieldId { get; set; }        
    }

    public class ImportExcelRowData
    {
        public ImportExcelMappingField FieldMapping { get; set; }
        public CategoryField FieldConfig { get; set; }
        public string CellValue { get; set; }
    }    

    public class CategoryQueryData
    {
        public int CategoryId { get; set; }
        public int CategoryFieldId { get; set; }
        public string Value { get; set; }
        public CategoryRowListOutputModel RowData { get; set; }
    }

}
