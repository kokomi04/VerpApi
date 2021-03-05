using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.ErrorCodes
{
    public enum ProductionMaterialsRequirementErrorCode: int
    {
        [Description("Mã yêu cầu vật tư thêm đã tồn tại")]
        OutsoureOrderCodeAlreadyExisted = 1,
        [Description("Không tìm thấy yêu cầu vật tư thêm")]
        NotFoundRequirement = 2,
        [Description("Không tìm thấy chi tiết các mặt hàng")]
        NotFoundDetailMaterials = 3
    }
}
