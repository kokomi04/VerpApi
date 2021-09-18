using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class BaseCustomerImportModel
    {
        [Display(Name = "Mã KH", GroupName ="TT chung")]
        [Required(ErrorMessage = "Vui lòng nhập mã đối tác")]
        [MaxLength(128, ErrorMessage = "Tên đối tác quá dài")]
        public string CustomerCode { get; set; }

        [Display(Name = "Tên KH", GroupName = "TT chung")]
        [Required(ErrorMessage = "Vui lòng nhập tên đối tác")]
        [MaxLength(128, ErrorMessage = "Tên đối tác quá dài")]
        public string CustomerName { get; set; }

        [Display(Name = "Loại KH (Cá nhân, tổ chức)", GroupName = "TT chung")]
        public EnumCustomerType CustomerTypeId { get; set; }
        [MaxLength(128, ErrorMessage = "Địa chỉ đối tác quá dài")]

        [Display(Name = "Địa chỉ", GroupName = "TT chung")]
        public string Address { get; set; }

        [Display(Name = "Mã số thuế", GroupName = "TT chung")]
        [MaxLength(64, ErrorMessage = "Mã số thuế quá dài")]
        public string TaxIdNo { get; set; }

        [Display(Name = "Số điện thoại", GroupName = "TT chung")]
        [MaxLength(64, ErrorMessage = "Số điện thoại quá dài")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Website", GroupName = "TT chung")]
        [MaxLength(128, ErrorMessage = "Website quá dài")]
        public string Website { get; set; }

        [Display(Name = "Email", GroupName = "TT chung")]
        [MaxLength(128, ErrorMessage = "Email quá dài")]
        public string Email { get; set; }

        [Display(Name = "Mô tả thêm", GroupName = "TT chung")]
        [MaxLength(128, ErrorMessage = "Mô tả quá dài")]
        public string Description { get; set; }

        [Display(Name = "Tên người đại diện", GroupName = "TT đại diện")]
        [MaxLength(128, ErrorMessage = "Tên người đại diện quá dài")]
        public string LegalRepresentative { get; set; }


        [Display(Name = "Số CMND, thẻ căn cước", GroupName = "TT đại diện")]
        [MaxLength(64, ErrorMessage = "Số CMND quá dài")]
        public string Identify { get; set; }

        [Display(Name = "Số ngày nợ", GroupName = "TT bán hàng")]
        public int? DebtDays { get; set; }
        [Display(Name = "Hạn mức nợ", GroupName = "TT bán hàng")]
        public decimal? DebtLimitation { get; set; }
        [Display(Name = "Thời điểm tính nợ (0: Ngày HĐ, 1: Cuối tháng)", GroupName = "TT bán hàng")]
        public EnumBeginningType DebtBeginningTypeId { get; set; }
        [Display(Name = "NV quản lý nợ", GroupName = "TT bán hàng")]
        public int? DebtManagerUserId { get; set; }

        [Display(Name = "Số ngày vay nợ", GroupName = "TT mua hàng")]
        public int? LoanDays { get; set; }
        [Display(Name = "Hạn mức vay nợ", GroupName = "TT mua hàng")]
        public decimal? LoanLimitation { get; set; }
        [Display(Name = "Thời điểm tính vay nợ (0: Ngày HĐ, 1: Cuối tháng)", GroupName = "TT mua hàng")]
        public EnumBeginningType LoanBeginningTypeId { get; set; }
        [Display(Name = "NV quản lý vay nợ", GroupName = "TT mua hàng")]
        public int? LoanManagerUserId { get; set; }


        [Display(Name = "(1) Họ tên", GroupName = "TT liên hệ 1")]
        public string ContactName1 { get; set; }

        [Display(Name = "(1) Giới tính (Nam, Nữ, Male, Female)", GroupName = "TT liên hệ 1")]
        public EnumGender? ContactGender1 { get; set; }
        [Display(Name = "(1) Chức danh", GroupName = "TT liên hệ 1")]
        public string ContactPosition1 { get; set; }

        [Display(Name = "(1) Điện thoại", GroupName = "TT liên hệ 1")]
        public string ContactPhone1 { get; set; }
        [Display(Name = "(1) Email người", GroupName = "TT liên hệ 1")]
        public string ContactEmail1 { get; set; }


        [Display(Name = "(2) Họ tên", GroupName = "TT liên hệ 2")]
        public string ContactName2 { get; set; }

        [Display(Name = "(2) Giới tính (Nam, Nữ, Male, Female)", GroupName = "TT liên hệ 2")]
        public EnumGender? ContactGender2 { get; set; }
        [Display(Name = "(2) Chức danh", GroupName = "TT liên hệ 2")]
        public string ContactPosition2 { get; set; }

        [Display(Name = "(2) Điện thoại", GroupName = "TT liên hệ 2")]
        public string ContactPhone2 { get; set; }
        [Display(Name = "(2) Email người", GroupName = "TT liên hệ 2")]
        public string ContactEmail2 { get; set; }


        [Display(Name = "(3) Họ tên", GroupName = "TT liên hệ 3")]
        public string ContactName3 { get; set; }

        [Display(Name = "(3) Giới tính (Nam, Nữ, Male, Female)", GroupName = "TT liên hệ 3")]
        public EnumGender? ContactGender3 { get; set; }
        [Display(Name = "(3) Chức danh", GroupName = "TT liên hệ 3")]
        public string ContactPosition3 { get; set; }

        [Display(Name = "(3) Điện thoại", GroupName = "TT liên hệ 3")]
        public string ContactPhone3 { get; set; }
        [Display(Name = "(3) Email người", GroupName = "TT liên hệ 3")]
        public string ContactEmail3 { get; set; }



        [Display(Name = "(1) Tên tài khoản", GroupName = "TT ngân hàng 1")]
        public string BankAccAccountName1 { get; set; }
        [Display(Name = "(1) Tên ngân hàng", GroupName = "TT ngân hàng 1")]
        public string BankAccBankName1 { get; set; }
        [Display(Name = "(1) Số tài khoản", GroupName = "TT ngân hàng 1")]
        public string BankAccAccountNo1 { get; set; }
        [Display(Name = "(1) Swiff code", GroupName = "TT ngân hàng 1")]
        public string BankAccSwiffCode1 { get; set; }
        [Display(Name = "(1) Chi nhánh", GroupName = "TT ngân hàng 1")]
        public string BankAccBrach1 { get; set; }
        [Display(Name = "(1) Địa chỉ", GroupName = "TT ngân hàng 1")]
        public string BankAccAddress1 { get; set; }
        [Display(Name = "(1) Loại tiền", GroupName = "TT ngân hàng 1")]
        public string BankAccCurrency1 { get; set; }



        [Display(Name = "(2) Tên tài khoản", GroupName = "TT ngân hàng 2")]
        public string BankAccAccountName2 { get; set; }
        [Display(Name = "(2) Tên ngân hàng", GroupName = "TT ngân hàng 2")]
        public string BankAccBankName2 { get; set; }
        [Display(Name = "(2) Số tài khoản", GroupName = "TT ngân hàng 2")]
        public string BankAccAccountNo2 { get; set; }
        [Display(Name = "(2) Swiff code", GroupName = "TT ngân hàng 2")]
        public string BankAccSwiffCode2 { get; set; }
        [Display(Name = "(2) Chi nhánh", GroupName = "TT ngân hàng 2")]
        public string BankAccBrach2 { get; set; }
        [Display(Name = "(2) Địa chỉ", GroupName = "TT ngân hàng 2")]
        public string BankAccAddress2 { get; set; }
        [Display(Name = "(2) Loại tiền", GroupName = "TT ngân hàng 2")]
        public string BankAccCurrency2 { get; set; }



        [Display(Name = "(3) Tên tài khoản", GroupName = "TT ngân hàng 3")]
        public string BankAccAccountName3 { get; set; }
        [Display(Name = "(3) Tên ngân hàng", GroupName = "TT ngân hàng 3")]
        public string BankAccBankName3 { get; set; }
        [Display(Name = "(3) Số tài khoản", GroupName = "TT ngân hàng 3")]
        public string BankAccAccountNo3 { get; set; }
        [Display(Name = "(3) Swiff code", GroupName = "TT ngân hàng 3")]
        public string BankAccSwiffCode3 { get; set; }
        [Display(Name = "(3) Chi nhánh", GroupName = "TT ngân hàng 3")]
        public string BankAccBrach3 { get; set; }
        [Display(Name = "(3) Địa chỉ", GroupName = "TT ngân hàng 3")]
        public string BankAccAddress3 { get; set; }
        [Display(Name = "(3) Loại tiền", GroupName = "TT ngân hàng 3")]
        public string BankAccCurrency3 { get; set; }

    }

    public static class BaseCustomerImportModelExtensions
    {
        public static string ContactName = nameof(BaseCustomerImportModel.ContactName1)[..^1];
        public static string ContactGender = nameof(BaseCustomerImportModel.ContactGender1)[..^1];
        public static string ContactPosition = nameof(BaseCustomerImportModel.ContactPosition1)[..^1];
        public static string ContactPhone = nameof(BaseCustomerImportModel.ContactPhone1)[..^1];
        public static string ContactEmail = nameof(BaseCustomerImportModel.ContactEmail1)[..^1];
        public static string[] ContactFieldPrefix = new[] { 
            ContactName, 
            ContactGender, 
            ContactPosition, 
            ContactPhone,
            ContactEmail 
        };

        public static string BankAccAccountName = nameof(BaseCustomerImportModel.BankAccAccountName1)[..^1];
        public static string BankAccBankName = nameof(BaseCustomerImportModel.BankAccBankName1)[..^1];
        public static string BankAccAccountNo = nameof(BaseCustomerImportModel.BankAccAccountNo1)[..^1];
        public static string BankAccSwiffCode = nameof(BaseCustomerImportModel.BankAccSwiffCode1)[..^1];
        public static string BankAccBrach = nameof(BaseCustomerImportModel.BankAccBrach1)[..^1];
        public static string BankAccAddress = nameof(BaseCustomerImportModel.BankAccAddress1)[..^1];
        public static string BankAccCurrency = nameof(BaseCustomerImportModel.BankAccCurrency1)[..^1];

        public static string[] BankAccFieldPrefix = new[] {
            BankAccAccountName,
            BankAccBankName,
            BankAccAccountNo,
            BankAccSwiffCode,
            BankAccBrach,
            BankAccAddress,
            BankAccCurrency
        };
    }                        

    public class BaseCustomerModel: BaseCustomerImportModel
    {
        public bool IsActived { get; set; }

        public EnumCustomerStatus CustomerStatusId { get; set; }
    }

    public class BasicCustomerListModel
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
    }
}
