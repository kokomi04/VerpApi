using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace VErp.Commons.Enums.ErrorCodes
{
    public enum PurchasingRequestErrorCode
    {
        [Description("Không tìm thấy phiếu yêu cầu mua hàng tương ứng")]
        NotFound = 1,
        
        [Description("Mã phiếu yêu cầu mua hàng không có")]
        PurchasingRequestCodeEmpty = 2,

        [Description("Phiếu yêu cầu mua hàng đã tồn tại")]
        AlreadyExisted = 3,

        [Description("Phiếu yêu cầu mua hàng đã được duyệt")]
        AlreadyApproved = 3,
    }
}
