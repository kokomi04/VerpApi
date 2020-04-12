
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Accountant.Model.Category
{
    public class AccountingAccountInputModel
    {
        public AccountingAccountInputModel()
        {
        }

        public int AccountingAccountId { get; set; }
        public int? ParentAccountingAccountId { get; set; }

        [Required(ErrorMessage = "Vui lòng số tài khoản")]
        [MaxLength(64, ErrorMessage = "Số tài khoản quá dài")]
        public string AccountNumber { get; set; }
        public int AccountLevel { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên tài khoản")]
        [MaxLength(128, ErrorMessage = "Tên tài khoản quá dài")]
        public string AccountNameVi { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên tài khoản")]
        [MaxLength(128, ErrorMessage = "Tên tài khoản quá dài")]
        public string AccountNameEn { get; set; }

        public bool IsStock { get; set; }
        public bool IsLiability { get; set; }
        public bool IsForeignCurrency { get; set; }
        public bool IsBranch { get; set; }
        public bool IsCorp { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ngoại tệ")]
        public string Currency { get; set; }
        public string Description { get; set; }
    }

    public class AccountingAccountOutputModel : AccountingAccountInputModel
    {
        public AccountingAccountOutputModel()
        {
        }
        public AccountingAccountOutputModel ParentAccountingAccount { get; set; }
    }
}
