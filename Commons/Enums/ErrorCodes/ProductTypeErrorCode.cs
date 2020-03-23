using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("PRT")]
    public enum ProductTypeErrorCode
    {
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Tên loại mã không được bỏ trống")]
        EmptyProductTypeName = 1,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Loại cha không tồn tại")]
        ParentProductTypeNotfound = 2,
        [EnumStatusCode(HttpStatusCode.NotFound)]
        [Description("Loại mã mặt hàng không tồn tại")]
        ProductTypeNotfound = 3,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Để xóa loại cha, vui lòng xóa hoặc di chuyển loại con")]
        CanNotDeletedParentProductType = 4,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Tên loại mã mặt hàng đã tồn tại, vui lòng chọn tên khác")]
        ProductTypeNameAlreadyExisted = 5,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Không thể xóa loại mã hàng đang được sử dụng")]
        ProductTypeInUsed = 6,
    }
}
