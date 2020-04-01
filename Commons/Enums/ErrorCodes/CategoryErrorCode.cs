using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("CTGY")]
    public enum CategoryErrorCode
    {
        [Description("Không tìm thấy danh mục")]
        CategoryNotFound = 1,
        [Description("Mã danh mục đã tồn tại")]
        CategoryCodeAlreadyExisted = 2,
        [Description("Tên danh mục đã tồn tại")]
        CategoryNameAlreadyExisted = 3,
        [Description("Không tìm thấy danh mục con")]
        SubCategoryNotFound = 4,
        [Description("Danh mục con đang là module")]
        SubCategoryIsModule = 5,
        [Description("Danh mục con đã trực thuộc một danh mục khác")]
        SubCategoryHasParent = 6,
        [Description("Đang tồn tại danh mục cha, loại bỏ liên kết trước khi xóa")]
        ParentCategoryAlreadyExisted = 7,
        [Description("Tên trường đã tồn tại")]
        CategoryFieldNameAlreadyExisted = 8,
        [Description("Tiêu đề trường đã tồn tại")]
        CategoryFieldTitleAlreadyExisted = 9,
        [Description("Trường tham chiếu không tồn tại")]
        SourceCategoryFieldNotFound = 10,
        [Description("Đang có trường dữ liệu tham chiếu tới")]
        DestCategoryFieldAlreadyExisted = 11,
        [Description("Trường dữ liệu không tồn tại")]
        CategoryFieldNotFound = 12,
        [Description("Kiểu dữ liệu không tồn tại")]
        DataTypeNotFound = 13,
        [Description("Kiểu nhập liệu không tồn tại")]
        FormTypeNotFound = 14,
        [Description("Danh mục không được phép thay đổi")]
        CategoryReadOnly = 15,
        [Description("Không được phép để trống trường thông tin bắt buộc")]
        RequiredFieldIsEmpty = 16,
        [Description("Trường thông tin unique có giá trị  đã tồn tại")]
        UniqueValueAlreadyExisted = 17,
    }
}
