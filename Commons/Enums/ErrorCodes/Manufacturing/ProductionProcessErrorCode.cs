using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.ErrorCodes
{
    public enum ProductionProcessErrorCode
    {
        [Description("Không tìm thấy công đoạn")]
        NotFoundProductionStep = 1,
        [Description("Không tìm thấy các chi tiết trong công đoạn")]
        NotFoundInOutStep = 2,
        [Description("Không thể xóa công đoạn")]
        InvalidDeleteProductionStep = 3,
        ValidateProductionStepLinkData = 4,
        [Description("Đã tồn tại quy trình sản xuất")]
        ExistsProductionProcess = 5,
        [Description("Không tìm thấy quy trình sản xuất")]
        NotFoundProductionProcess = 6,
        [Description("Các công đoạn không nằm trong cùng 1 quy trình sản xuất")]
        ListProductionStepNotInContainerId = 7,
        ValidateProductionStep = 8,
        ValidateOutsourcePartRequest = 9,
        ValidateProductionStepLinkDataRole = 10,
    }
}
