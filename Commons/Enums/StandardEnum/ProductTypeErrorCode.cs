using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("PRT")]
    public enum ProductTypeErrorCode
    {
        [Description("Tên loại mã không được bỏ trống")]
        EmptyProductTypeName = 1,

        [Description("Loại cha không tồn tại")]
        ParentProductTypeNotfound = 2,

        [Description("Loại mã mặt hàng không tồn tại")]
        ProductTypeNotfound = 3,

        [Description("Để xóa loại cha, vui lòng xóa hoặc di chuyển loại con")]
        CanNotDeletedParentProductType = 4
    }
}
