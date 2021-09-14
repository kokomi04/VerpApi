using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("VOU")]
    public enum VoucherErrorCode
    {
        [Description("Không tìm thấy chứng từ")]
        VoucherTypeNotFound = 1,
        [Description("Mã chứng từ đã tồn tại")]
        VoucherCodeAlreadyExisted = 2,
        [Description("Tên chứng từ đã tồn tại")]
        VoucherTitleAlreadyExisted = 3,
        [Description("Không tìm thấy vùng dữ liệu")]
        VoucherAreaNotFound = 4,
        [Description("Mã vùng dữ liệu đã tồn tại")]
        VoucherAreaCodeAlreadyExisted = 5,
        [Description("Tiêu đề vùng dữ liệu đã tồn tại")]
        VoucherAreaTitleAlreadyExisted = 6,
        [Description("Trường dữ liệu đã tồn tại")]
        VoucherAreaFieldAlreadyExisted = 7,
        [Description("Chứng từ không tồn tại")]
        VoucherValueBillNotFound = 8,
        [Description("Trường tham chiếu không tồn tại")]
        SourceCategoryFieldNotFound = 9,
        [Description("Không còn trường dữ liệu trống")]
        VoucherAreaFieldOverLoad = 10,
        [Description("Trường dữ liệu không tồn tại")]
        VoucherAreaFieldNotFound = 11,
        [Description("Kiểu dữ liệu không tồn tại")]
        DataTypeNotFound = 12,
        [Description("Kiểu nhập liệu không tồn tại")]
        FormTypeNotFound = 13,
        [Description("Chứng từ không được phép thay đổi")]
        VoucherReadOnly = 14,
        [Description("Không được phép để trống dòng {0} trường thông tin {1}")]
        RequiredFieldIsEmpty = 15,
        [Description("Trường thông tin {0} có giá trị đã tồn tại")]
        UniqueValueAlreadyExisted = 16,
        [Description("Thông tin giá trị dòng {0} của trường {1} không tồn tại")]
        ReferValueNotFound = 17,
        [Description("Thông tin giá trị dòng {0} của trường {1} không thỏa mãn điều kiện lọc")]
        ReferValueNotValidFilter = 117,
        [Description("Dòng thông tin không tồn tại")]
        VoucherRowNotFound = 18,
        [Description("vùng dữ liệu đang trực thuộc chứng từ")]
        IsVoucherArea = 19,
        [Description("Giá trị không tồn tại")]
        VoucherValueNotFound = 23,
        [Description("Giá trị dòng {0} trường dữ liệu {1} không hợp lệ")]
        VoucherValueInValid = 25,
        [Description("Đang có dòng dữ liệu sử dụng giá trị này")]
        VoucherRowAlreadyExisted = 26,
        [Description("Không phải là chứng từ")]
        VoucherIsNotModule = 27,
        [Description("File không hợp lệ")]
        FormatFileInvalid = 28,
        [Description("Trường dữ liệu dùng chung không tồn tại")]
        VoucherFieldNotFound = 29,
        [Description("Trường dữ liệu dùng chung đã tồn tại")]
        VoucherFieldAlreadyExisted = 30,
        [Description("Trường dữ liệu dùng chung đang được sử dụng")]
        VoucherFieldIsUsed = 31,
        [Description("Cấu hình mã tự sinh thất bại")]
        MapGenCodeConfigFail = 32,
        [Description("Cấu hình chứng từ nguồn không tồn tại")]
        SourceVoucherTypeNotFound = 33,
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
        VoucherActionNotFound = 39,
        [Description("Mã chức năng đã tồn tại")]
        VoucherActionCodeAlreadyExisted = 40
    }
}
