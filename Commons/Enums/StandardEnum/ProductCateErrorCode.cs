using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("PRC")]
    public enum ProductCateErrorCode
    {
        [Description("Tên danh mục không được bỏ trống")]
        EmptyProductCateName = 1,

        [Description("Danh mục cha không tồn tại")]
        ParentProductCateNotfound = 2,

        [Description("Danh mục không tồn tại")]
        ProductCateNotfound = 3
    }
}
