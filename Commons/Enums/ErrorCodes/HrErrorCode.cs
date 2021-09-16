using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("HREC")]
    public enum HrErrorCode
    {
        [Description("Không tìm thấy chứng từ hành chính nhân sự")]
        HrTypeNotFound = 1,
        [Description("Mã chứng từ hành chính nhân sự đã tồn tại")]
        HrCodeAlreadyExisted = 2,
        [Description("Tên chứng từ hành chính nhân sự đã tồn tại")]
        HrTitleAlreadyExisted = 3,
        [Description("Không tìm thấy vùng dữ liệu")]
        HrAreaNotFound = 4,
        [Description("Mã vùng dữ liệu đã tồn tại")]
        HrAreaCodeAlreadyExisted = 5,
        [Description("Tiêu đề vùng dữ liệu đã tồn tại")]
        HrAreaTitleAlreadyExisted = 6,
        [Description("Trường dữ liệu đã tồn tại")]
        HrAreaFieldAlreadyExisted = 7,
        [Description("Chứng từ không tồn tại")]
        HrValueBillNotFound = 8,
        [Description("Trường tham chiếu không tồn tại")]
        SourceCategoryFieldNotFound = 9,
        [Description("Không còn trường dữ liệu trống")]
        HrAreaFieldOverLoad = 10,
        [Description("Trường dữ liệu không tồn tại")]
        HrAreaFieldNotFound = 11,
        [Description("Kiểu dữ liệu không tồn tại")]
        DataTypeNotFound = 12,
        [Description("Kiểu nhập liệu không tồn tại")]
        FormTypeNotFound = 13,
        [Description("Chứng từ không được phép thay đổi")]
        HrReadOnly = 14,
        [Description("Không được phép để trống dòng {0} trường thông tin {1}")]
        RequiredFieldIsEmpty = 15,
        [Description("Trường thông tin {0} có giá trị đã tồn tại")]
        UniqueValueAlreadyExisted = 16,
        [Description("Thông tin giá trị dòng {0} của trường {1} không tồn tại")]
        ReferValueNotFound = 17,
        [Description("Dòng thông tin không tồn tại")]
        HrRowNotFound = 18,
        [Description("vùng dữ liệu đang trực thuộc chứng từ hành chính nhân sự")]
        IsHrArea = 19,
        [Description("Giá trị không tồn tại")]
        HrValueNotFound = 23,
        [Description("Giá trị {0} dòng {1} trường dữ liệu {2} không hợp lệ")]
        HrValueInValid = 25,
        [Description("Đang có dòng dữ liệu sử dụng giá trị này")]
        HrRowAlreadyExisted = 26,
        [Description("Không phải là chứng từ hành chính nhân sự")]
        HrIsNotModule = 27,
        [Description("File không hợp lệ")]
        FormatFileInvalid = 28,
        [Description("Trường dữ liệu dùng chung không tồn tại")]
        HrFieldNotFound = 29,
        [Description("Trường dữ liệu dùng chung đã tồn tại")]
        HrFieldAlreadyExisted = 30,
        [Description("Trường dữ liệu dùng chung đang được sử dụng")]
        HrFieldIsUsed = 31,
        [Description("Cấu hình mã tự sinh thất bại")]
        MapGenCodeConfigFail = 32,
        [Description("Cấu hình chứng từ hành chính nhân sự nguồn không tồn tại")]
        SourceHrTypeNotFound = 33,
        // [Description("Vùng dữ liệu dạng bảng đã tồn tại")]
        // MultiRowAreaAlreadyExisted = 34,
        [Description("Thông tin dữ liệu dạng bảng không được để trống")]
        MultiRowAreaEmpty = 35,

        [Description("Không tìm thấy cấu hình phiếu in")]
        PrintConfigNotFound = 36,
        [Description("Tên cấu hình phiếu in đã tồn tại")]
        PrintConfigNameAlreadyExisted = 37,
        DoNotGeneratePrintTemplate = 38,

        [Description("Không tìm thấy chức năng")]
        HrActionNotFound = 39,
        [Description("Mã chức năng đã tồn tại")]
        HrActionCodeAlreadyExisted = 40,
    }
}
