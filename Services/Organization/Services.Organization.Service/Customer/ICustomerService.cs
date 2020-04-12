using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Customer;

namespace VErp.Services.Organization.Service.Customer
{
    public interface ICustomerService
    {
        Task<ServiceResult<int>> AddCustomer(int updatedUserId, CustomerModel data);
        Task<PageData<CustomerListOutput>> GetList(string keyword, EnumCustomerStatus? customerStatusId, int page, int size);
        Task<IList<CustomerListOutput>> GetListByIds(IList<int> customerIds);
        Task<ServiceResult<CustomerModel>> GetCustomerInfo(int customerId);
        Task<Enum> UpdateCustomer(int updatedUserId, int customerId, CustomerModel data);
        Task<Enum> DeleteCustomer(int customerId);
    }
}
