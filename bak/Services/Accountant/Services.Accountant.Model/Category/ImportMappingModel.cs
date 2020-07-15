using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Accountant.Model.Category
{
    public class ImportMappingModel
    {
        public int StartRow { get; set; }
        public IList<ImportMappingFieldModel> Mappings { get; set; }
    }
    public class ImportMappingFieldModel
    {
        public string Column { get; set; }
        public string Header { get; set; }
        public string ParentColumn { get; set; }
        public int CategoryFieldId { get; set; }
        public int CategoryFieldName { get; set; }
        public int CategoryFieldTitle { get; set; }
        public int? RefCategoryId { get; set; }
        public int? RefCategoryFieldId { get; set; }
    }
}
