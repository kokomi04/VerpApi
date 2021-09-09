using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.Library.Model
{
    public class CategoryNameModel
    {
        //public int CategoryId { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryTitle { get; set; }
        public bool IsTreeView { get; set; }
        public IList<CategoryFieldNameModel> Fields { get; set; }
    }

    public class CategoryFieldNameModel
    {
        //optional
        public string GroupName { get; set; }

        //public int CategoryFieldId { get; set; }
        public string FieldName { get; set; }
        public string FieldTitle { get; set; }
        public bool IsRequired { get; set; }
        public int? Type { get; set; }
        public CategoryNameModel RefCategory { get; set; }
    }

    public class FieldDataTypeAttribute : Attribute
    {
        public int Type { get; private set; }
        public FieldDataTypeAttribute(int type)
        {
            Type = type;
        }
    }

    public class FieldDataIgnoreAttribute : Attribute
    {
        public FieldDataIgnoreAttribute()
        {
        }
    }

    public class FieldDataIgnoreExportAttribute : Attribute
    {
        public FieldDataIgnoreExportAttribute()
        {
        }
    }

    public abstract class MappingDataRowAbstract
    {
        [FieldDataIgnore]
        public int RowNumber { get; set; }
    }
}
