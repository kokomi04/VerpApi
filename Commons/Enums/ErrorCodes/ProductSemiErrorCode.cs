using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.ErrorCodes
{
    public enum ProductSemiErrorCode
    {
        [Description("Không tìm thấy bán thành phẩm")]
        NotFoundProductSemi = 1,
        [Description("Không tìm thấy bán thành phẩm chuyển đổi")]
        NotFoundProductSemiConversion = 2
    }
}
