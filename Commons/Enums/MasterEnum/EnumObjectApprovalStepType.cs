using System.ComponentModel;

namespace VErp.Commons.Enums.MasterEnum
{
    public enum EnumObjectApprovalStepType
    {
        [Description("Bước duyệt")]
        ApprovalStep = 1,
        [Description("Bước kiểm tra")]
        CheckingStep = 2,
    }
}
