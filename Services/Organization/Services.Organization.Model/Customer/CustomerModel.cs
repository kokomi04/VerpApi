﻿using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.Organization;

namespace VErp.Services.Organization.Model.Customer
{
    public class CustomerModel : BaseCustomerModel, IMapFrom<BaseCustomerImportModel>
    {
        public int CustomerId { get; set; }
        public long? LogoFileId { get; set; }

        public CustomerModel()
        {
            Contacts = new List<CustomerContactModel>();
            BankAccounts = new List<CustomerBankAccountModel>();
            CustomerAttachments = new List<CustomerAttachmentModel>();
        }
        public IList<CustomerContactModel> Contacts { get; set; }
        public IList<CustomerBankAccountModel> BankAccounts { get; set; }
        public IList<CustomerAttachmentModel> CustomerAttachments { get; set; }

    }
}
