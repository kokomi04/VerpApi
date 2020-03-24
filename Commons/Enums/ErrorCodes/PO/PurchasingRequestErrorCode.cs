using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using VErp.Commons.Enums.StandardEnum;

namespace VErp.Commons.Enums.ErrorCodes
{
    public enum PurchasingRequestErrorCode
    {
        [Description("Không tìm thấy phiếu yêu cầu VTHH tương ứng")]
        RequestNotFound = 1,
        [Description("Mã phiếu yêu cầu VTHH không có")]
        RequestCodeEmpty = 2,
        [Description("Phiếu yêu cầu VTHH đã tồn tại")]
        RequestCodeAlreadyExisted = 3,
        [Description("Không thế xóa phiếu yêu cầu VTHH đã được duyệt")]
        RequestAlreadyApproved = 4,
    }
}
