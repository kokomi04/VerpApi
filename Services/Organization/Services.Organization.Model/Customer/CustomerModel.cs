using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.Organization.Model.Customer
{
    public class CustomerModel: BaseCustomerModel
    {
        public IList<CustomerContactModel> Contacts { get; set; }
        public IList<CustomerBankAccountModel> BankAccounts { get; set; }
    }
}
