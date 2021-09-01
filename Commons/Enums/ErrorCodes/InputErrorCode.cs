using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("INP")]
    public enum InputErrorCode
    {
        [Description("Không tìm thấy chứng từ")]
        InputTypeNotFound = 1,
        [Description("Mã chứng từ đã tồn tại")]
        InputCodeAlreadyExisted = 2,
        [Description("Tên chứng từ đã tồn tại")]
        InputTitleAlreadyExisted = 3,
        [Description("Không tìm thấy vùng dữ liệu")]
        InputAreaNotFound = 4,
        [Description("Mã vùng dữ liệu đã tồn tại")]
        InputAreaCodeAlreadyExisted = 5,
        [Description("Tiêu đề vùng dữ liệu đã tồn tại")]
        InputAreaTitleAlreadyExisted = 6,
        [Description("Trường dữ liệu đã tồn tại")]
        InputAreaFieldAlreadyExisted = 7,
        [Description("Chứng từ không tồn tại")]
        InputValueBillNotFound = 8,
        [Description("Trường tham chiếu không tồn tại")]
        SourceCategoryFieldNotFound = 9,
        [Description("Không còn trường dữ liệu trống")]
        InputAreaFieldOverLoad = 10,
        [Description("Trường dữ liệu không tồn tại")]
        InputAreaFieldNotFound = 11,
        [Description("Kiểu dữ liệu không tồn tại")]
        DataTypeNotFound = 12,
        [Description("Kiểu nhập liệu không tồn tại")]
        FormTypeNotFound = 13,
        [Description("Chứng từ không được phép thay đổi")]
        InputReadOnly = 14,
        [Description("Không được phép để trống dòng {0} trường thông tin {1}")]
        RequiredFieldIsEmpty = 15,
        [Description("Trường thông tin {0} có giá trị đã tồn tại")]
        UniqueValueAlreadyExisted = 16,
        [Description("Thông tin giá trị dòng {0} của trường {1} không tồn tại")]
        ReferValueNotFound = 17,
        [Description("Thông tin giá trị dòng {0} của trường {1} không thỏa mãn điều kiện lọc")]
        ReferValueNotValidFilter = 117,
        [Description("Dòng thông tin không tồn tại")]
        InputRowNotFound = 18,
        [Description("vùng dữ liệu đang trực thuộc chứng từ")]
        IsInputArea = 19,
        [Description("Giá trị không tồn tại")]
        InputValueNotFound = 23,
        [Description("Giá trị {0} dòng {1} trường dữ liệu {2} không hợp lệ")]
        InputValueInValid = 25,
        [Description("Đang có dòng dữ liệu sử dụng giá trị này")]
        InputRowAlreadyExisted = 26,
        [Description("Không phải là chứng từ")]
        InputIsNotModule = 27,
        [Description("File không hợp lệ")]
        FormatFileInvalid = 28,
        [Description("Trường dữ liệu dùng chung không tồn tại")]
        InputFieldNotFound = 29,
        [Description("Trường dữ liệu dùng chung đã tồn tại")]
        InputFieldAlreadyExisted = 30,
        [Description("Trường dữ liệu dùng chung đang được sử dụng")]
        InputFieldIsUsed = 31,
        [Description("Cấu hình mã tự sinh thất bại")]
        MapGenCodeConfigFail = 32,
        [Description("Cấu hình chứng từ nguồn không tồn tại")]
        SourceInputTypeNotFound = 33,
        [Description("Vùng dữ liệu dạng bảng đã tồn tại")]
        MultiRowAreaAlreadyExisted = 34,
        [Description("Thông tin dữ liệu dạng bảng không được để trống")]
        MultiRowAreaEmpty = 35,

        [Description("Không tìm thấy cấu hình phiếu in")]
        PrintConfigNotFound = 36,
        [Description("Tên cấu hình phiếu in đã tồn tại")]
        PrintConfigNameAlreadyExisted = 37,
        DoNotGeneratePrintTemplate = 38,

        [Description("Không tìm thấy chức năng")]
        InputActionNotFound = 39,
        [Description("Mã chức năng đã tồn tại")]
        InputActionCodeAlreadyExisted = 40,
    }
}
