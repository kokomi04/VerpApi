using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("PRO")]
    public enum ProductErrorCode
    {
        [EnumStatusCode(HttpStatusCode.NotFound)]
        [Description("Không tìm thấy mặt hàng")]
        ProductNotFound = 1,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Mã mặt hàng không được để trống")]
        ProductCodeEmpty = 2,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Mã mặt hàng đã tồn tại")]
        ProductCodeAlreadyExisted = 3,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Danh mục mặt hàng không đúng")]
        ProductCateInvalid = 4,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Loại sinh mã mặt hàng không đún")]
        ProductTypeInvalid = 5,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Đơn vị chuyển đổi đang được sử dụng")]
        SomeProductUnitConversionInUsed = 6,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Tên mặt hàng đã tồn tại")]
        ProductNameAlreadyExisted = 7,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Lỗi không thể tính toán biểu thức đơn vị chuyển đổi")]
        InvalidUnitConversionExpression = 7
    }
}
