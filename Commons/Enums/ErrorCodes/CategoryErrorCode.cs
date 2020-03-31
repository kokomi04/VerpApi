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
        [Description("Danh mục tham chiếu không phải là module")]
        SourceCategoryNotModule = 11,
        [Description("Đang có trường dữ liệu tham chiếu tới")]
        DestCategoryFieldAlreadyExisted = 12,
    }
}
