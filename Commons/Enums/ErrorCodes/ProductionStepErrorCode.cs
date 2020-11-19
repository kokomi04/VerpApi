using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.ErrorCodes
{
    public enum ProductionStepErrorCode
    {
        [Description("Không tìm thấy công đoạn")]
        NotFoundProductionStep = 1,
        [Description("Không tìm thấy các chi tiết trong công đoạn")]
        NotFoundInOutStep = 2,
        [Description("Không thể xóa công đoạn")]
        InvalidDeleteProductionStep = 3,
    }
}
