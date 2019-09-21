using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("PRT")]
    public enum ProductTypeErrorCode
    {
        [Description("Tên loại không được bỏ trống")]
        EmptyProductTypeName = 1,

        [Description("Loại cha không tồn tại")]
        ParentProductTypeNotfound = 2,

        [Description("Loại SP/Vật tư không tồn tại")]
        ProductTypeNotfound = 3
    }
}
