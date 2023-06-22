using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.GlobalObject.Attributes
{
    public class DynamicCategoryMappingAttribute : Attribute
    {
        public string CategoryCode { get; set; }
    }
    public class ValidateDuplicateByKeyCodeAttribute : Attribute
    {
        public ValidateDuplicateByKeyCodeAttribute()
        {
        }
    }
    public class DynamicObjectCategoryMappingAttribute : Attribute
    {
        public DynamicObjectCategoryMappingAttribute()
        {
        }
    }

}
