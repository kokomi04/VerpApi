using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Customer;

namespace VErp.Services.Master.Service.Customer
{
    public interface ICustomerService
    {
        Task<ServiceResult<int>> AddCustomer(CustomerModel data);
        Task<PageData<CustomerListOutput>> GetList(string keyword, int page, int size);
        Task<IList<CustomerListOutput>> GetListByIds(IList<int> customerIds);
        Task<ServiceResult<CustomerModel>> GetCustomerInfo(int customerId);
        Task<Enum> UpdateCustomer(int customerId, CustomerModel data);
        Task<Enum> DeleteCustomer(int customerId);
    }
}
