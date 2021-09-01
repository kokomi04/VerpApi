﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("PRO")]
    public enum ProductErrorCode
    {
        [Description("Không tìm thấy mặt hàng")]
        ProductNotFound = 1,
        [Description("Mã mặt hàng không được để trống")]
        ProductCodeEmpty = 2,
        [Description("Mã mặt hàng đã tồn tại")]
        ProductCodeAlreadyExisted = 3,
        [Description("Danh mục mặt hàng không đúng")]
        ProductCateInvalid = 4,
        [Description("Loại sinh mã mặt hàng không đúng")]
        ProductTypeInvalid = 5,
        [Description("Đơn vị chuyển đổi đang được sử dụng")]
        SomeProductUnitConversionInUsed = 6,
        [Description("Tên mặt hàng đã tồn tại")]
        ProductNameAlreadyExisted = 7,
        [Description("Lỗi không thể tính toán biểu thức đơn vị chuyển đổi")]
        InvalidUnitConversionExpression = 8,
        [Description("Mặt hàng đang được sử dụng")]
        ProductInUsed = 9,

        [Description("Tên mặt hàng không được để rỗng")]
        ProductNameEmpty = 10,

        [Description("Số lượng sử dụng của NVL tiêu hao bằng 0")]
        QuantityOfMaterialsConsumptionIsZero = 11,
    }
}