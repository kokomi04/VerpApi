using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("PRC")]
    public enum ProductCateErrorCode
    {
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Tên danh mục không được bỏ trống")]
        EmptyProductCateName = 1,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Danh mục cha không tồn tại")]
        ParentProductCateNotfound = 2,
        [EnumStatusCode(HttpStatusCode.NotFound)]
        [Description("Danh mục không tồn tại")]
        ProductCateNotfound = 3,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Để xóa danh mục cha, vui lòng xóa hoặc di chuyển danh mục con")]
        CanNotDeletedParentProductCate = 4,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Tên danh mục đã tồn tại, vui lòng chọn tên khác")]
        ProductCateNameAlreadyExisted = 5,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Không thể xóa danh mục đang được sử dụng")]
        ProductCateInUsed = 6,
    }
}
