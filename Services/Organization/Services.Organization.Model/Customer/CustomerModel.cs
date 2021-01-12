using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.Organization.Model.Customer
{
    public class CustomerModel : BaseCustomerModel, IMapFrom<BaseCustomerImportModel>
    {
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
