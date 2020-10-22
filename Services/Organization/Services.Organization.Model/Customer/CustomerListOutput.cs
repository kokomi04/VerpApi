using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Organization.Model.Customer
{
    public class CustomerListOutput
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public EnumCustomerType CustomerTypeId { get; set; }
        public string Address { get; set; }
        public string TaxIdNo { get; set; }
        public string PhoneNumber { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Identify { get; set; }        
        public EnumCustomerStatus CustomerStatusId { get; set; }

        public int? DebtDays { get; set; }
        public decimal? DebtLimitation { get; set; }
        public EnumBeginingType DebtBeginingTypeId { get; set; }
        public int? DebtManagerUserId { get; set; }

        public int? LoanDays { get; set; }
        public decimal? LoanLimitation { get; set; }
        public EnumBeginingType LoanBeginingTypeId { get; set; }
        public int? LoanManagerUserId { get; set; }

    }
}
