using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Library.Model
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
        //optional
        public string GroupName { get; set; }

        public int CategoryFieldId { get; set; }
        public string FieldName { get; set; }
        public string FieldTitle { get; set; }
        public CategoryNameModel RefCategory { get; set; }
    }
}
