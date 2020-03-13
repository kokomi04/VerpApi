using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace VErp.Commons.Enums.ErrorCodes
{
    public enum PurchasingSuggestErrorCode
    {
        [Description("Không tìm thấy phiếu đề nghị mua hàng tương ứng")]
        NotFound = 1,
        
        [Description("Mã phiếu đề nghị mua hàng không có")]
        PurchasingRequestCodeEmpty = 2,

        [Description("Phiếu đề nghị mua hàng đã tồn tại")]
        AlreadyExisted = 3,

        [Description("Phiếu đề nghị mua hàng đã được duyệt")]
        AlreadyApproved = 3,
    }
}
