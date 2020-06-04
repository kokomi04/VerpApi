﻿using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Organization.Model.Customer
{
    public class CustomerBankAccountModel
    {
        public int CustomerBankAccountId { get; set; }
        [MaxLength(128, ErrorMessage = "Tên ngân hàng quá dài")]
        public string BankName { get; set; }
        [MaxLength(32, ErrorMessage = "Số tài khoản quá dài")]
        public string AccountNumber { get; set; }
        [MaxLength(64, ErrorMessage = "Swiff code quá dài")]
        public string SwiffCode { get; set; }
    }
}