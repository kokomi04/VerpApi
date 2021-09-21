using System.ComponentModel;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("I18N")]
    public enum I18nLanguageErrorCode
    {
        [Description("Không tìm thấy bản ghi i18n")]
        ItemNotFound,
        [Description("Đã tồn tại mã i18n trong hệ thống")]
        AlreadyExistsKeyCode
    }
}