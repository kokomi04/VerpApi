using System.ComponentModel;

namespace VErp.Commons.Enums.Manafacturing
{
    public enum EnumProductionMaterialsRequirementStatus : int
    {
        [Description("Đang chờ duyệt")]
        Waiting = 1,
        [Description("Đã duyệt")]
        Accepted = 2,
        [Description("Từ chối")]
        Rejected = 3,
    }
}
