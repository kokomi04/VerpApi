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
        [Description("Tên trường đã tồn tại")]
        InputAreaFieldNameAlreadyExisted = 7,
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
        [Description("Không được phép để trống trường thông tin {0}")]
        RequiredFieldIsEmpty = 15,
        [Description("Trường thông tin {0} có giá trị đã tồn tại")]
        UniqueValueAlreadyExisted = 16,
        [Description("Thông tin trường {0} không tồn tại")]
        ReferValueNotFound = 17,
        [Description("Dòng thông tin không tồn tại")]
        InputRowNotFound = 18,
        [Description("vùng dữ liệu đang trực thuộc chứng từ")]
        IsInputArea = 19,
        [Description("Giá trị không tồn tại")]
        InputValueNotFound = 23,
        [Description("Giá trị {0} không hợp lệ")]
        InputValueInValid = 25,
        [Description("Đang có dòng dữ liệu sử dụng giá trị này")]
        InputRowAlreadyExisted = 26,
        [Description("Không phải là chứng từ")]
        InputIsNotModule = 27,
        [Description("File không hợp lệ")]
        FormatFileInvalid = 28
    }
}
