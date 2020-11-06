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
        CategoryTitleAlreadyExisted = 3,
        [Description("Không tìm thấy nhóm dữ liệu")]
        SubCategoryNotFound = 4,
        [Description("Nhóm dữ liệu đang là danh mục")]
        SubCategoryIsModule = 5,
        [Description("Nhóm dữ liệu đã trực thuộc một danh mục khác")]
        SubCategoryHasParent = 6,
        [Description("Đang tồn tại danh mục cha, loại bỏ liên kết trước khi xóa")]
        ParentCategoryAlreadyExisted = 7,
        [Description("Tên trường đã tồn tại")]
        CategoryFieldNameAlreadyExisted = 8,
        [Description("Trường tham chiếu không tồn tại")]
        SourceCategoryFieldNotFound = 9,
        [Description("Đang có trường dữ liệu tham chiếu tới")]
        DestCategoryFieldAlreadyExisted = 10,
        [Description("Trường dữ liệu không tồn tại")]
        CategoryFieldNotFound = 11,
        [Description("Kiểu dữ liệu không tồn tại")]
        DataTypeNotFound = 12,
        [Description("Kiểu nhập liệu không tồn tại")]
        FormTypeNotFound = 13,
        [Description("Danh mục không được phép thay đổi")]
        CategoryReadOnly = 14,
        [Description("Không được phép để trống trường thông tin {0}")]
        RequiredFieldIsEmpty = 15,
        [Description("Trường thông tin {0} có giá trị đã tồn tại")]
        UniqueValueAlreadyExisted = 16,
        [Description("Thông tin {0} không tồn tại")]
        ReferValueNotFound = 17,
        [Description("Dòng thông tin không tồn tại")]
        CategoryRowNotFound = 18,
        [Description("Nhóm dữ liệu đang trực thuộc danh mục")]
        IsSubCategory = 19,
        [Description("Mã nhóm dữ liệu đã tồn tại")]
        SubCategoryCodeAlreadyExisted = 20,
        [Description("Tên nhóm dữ liệu đã tồn tại")]
        SubCategoryTitleAlreadyExisted = 21,
        [Description("Không thể tham chiếu từ chính trường dữ liệu")]
        ReferenceFromItSelf = 22,
        [Description("Giá trị không tồn tại")]
        CategoryValueNotFound = 23,
        [Description("Trường dữ liệu không cần giá trị mặc định")]
        CategoryFieldNotDefaultValue = 24,
        [Description("Giá trị {0} không hợp lệ")]
        CategoryValueInValid = 25,
        [Description("Đang có dòng dữ liệu sử dụng giá trị này")]
        CategoryRowAlreadyExisted = 26,
        [Description("Không phải là danh mục")]
        CategoryIsNotModule = 27,
        [Description("File không hợp lệ")]
        FormatFileInvalid = 28,
        [Description("Hai kiểu nhập liệu không thể chuyển qua lại")]
        FormTypeNotSwitch = 29,
        [Description("Dữ liệu cha không tồn tại")]
        ParentCategoryRowNotExisted = 30,
        [Description("Danh mục nằm ngoài phân hệ")]
        CategoryIsOutSideData =31,
        [Description("Lấy thông tin danh mục ngoài phân hệ thất bại")]
        CategoryIsOutSideDataError = 32,
        [Description("Dữ liệu cha không được là chính nó")]
        ParentCategoryFromItSelf = 33,
        [Description("Trường dữ liệu không được phép thay đổi")]
        CategoryFieldReadOnly = 44,
        [Description("Đang tồn tại tham chiếu tới giá trị này")]
        RelationshipAlreadyExisted = 45,
        [Description("Dữ liệu không thuộc vào đơn vị")]
        InvalidSubsidiary = 46,
        [Description("Đang tồn tại tham chiếu tới danh mục này")]
        CatRelationshipAlreadyExisted = 47,
    }
}
