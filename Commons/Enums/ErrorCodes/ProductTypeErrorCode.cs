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
        CanNotDeletedParentProductType = 4,

        [Description("Tên loại mã mặt hàng đã tồn tại, vui lòng chọn tên khác")]
        ProductTypeNameAlreadyExisted = 5,

        [Description("Không thể xóa loại mã hàng đang được sử dụng")]
        ProductTypeInUsed = 6,

    }
}
