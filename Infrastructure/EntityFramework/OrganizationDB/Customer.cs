using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class Customer
{
    public int CustomerId { get; set; }

    public int SubsidiaryId { get; set; }

    public string CustomerCode { get; set; }

    public string CustomerName { get; set; }

    public int CustomerTypeId { get; set; }

    public string Address { get; set; }

    public string TaxIdNo { get; set; }

    public string PhoneNumber { get; set; }

    public string Website { get; set; }

    public string Email { get; set; }

    public string Description { get; set; }

    public bool IsActived { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public DateTime? UpdatedDatetimeUtc { get; set; }

    public int CustomerStatusId { get; set; }

    public string LegalRepresentative { get; set; }

    public string Identify { get; set; }

    public int? DebtDays { get; set; }

    public decimal? DebtLimitation { get; set; }

    public int DebtBeginningTypeId { get; set; }

    public int? DebtManagerUserId { get; set; }

    public int? LoanDays { get; set; }

    public decimal? LoanLimitation { get; set; }

    public int LoanBeginningTypeId { get; set; }

    public int? LoanManagerUserId { get; set; }

    public string PartnerId { get; set; }

    public int? CustomerCateId { get; set; }

    public long? LogoFileId { get; set; }

    public int? PayConditionsId { get; set; }

    public int? DeliveryConditionsId { get; set; }

    public virtual ICollection<CustomerAttachment> CustomerAttachment { get; set; } = new List<CustomerAttachment>();

    public virtual ICollection<CustomerBankAccount> CustomerBankAccount { get; set; } = new List<CustomerBankAccount>();

    public virtual CustomerCate CustomerCate { get; set; }

    public virtual ICollection<CustomerContact> CustomerContact { get; set; } = new List<CustomerContact>();
}
