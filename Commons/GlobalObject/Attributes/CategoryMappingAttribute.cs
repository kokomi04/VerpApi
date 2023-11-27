using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

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
    public class RequireWhenHandleFilterAttribute : Attribute
    {

        public RequireWhenHandleFilterAttribute(string errorMessage, EnumHandleFilterOption enumHandleFilterOption, bool isNotNull)
        {
            ErrorMessage = errorMessage;
            EnumHandleFilterOption = enumHandleFilterOption;
            IsNotNull = isNotNull;
        }
        public string ErrorMessage { get; set; }
        public EnumHandleFilterOption EnumHandleFilterOption { get; set; }
        public bool IsNotNull { get; set; }
    }
}
