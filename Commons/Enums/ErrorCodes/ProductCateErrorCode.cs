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
        ProductCateNotfound = 3,
        [Description("Để xóa danh mục cha, vui lòng xóa hoặc di chuyển danh mục con")]
        CanNotDeletedParentProductCate = 4,
        [Description("Tên danh mục đã tồn tại, vui lòng chọn tên khác")]
        ProductCateNameAlreadyExisted = 5,
        [Description("Không thể xóa danh mục đang được sử dụng")]
        ProductCateInUsed = 6,
    }
}
