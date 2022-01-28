using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Organization.Model.Customer;

namespace VErp.Services.Organization.Service.Customer
{
    public interface ICustomerCateService
    {
        public Task<IList<CustomerCateModel>> GetList();
        public Task<CustomerCateModel> GetInfo(int customerCateId);
        public Task<int> CreateCustomerCate(CustomerCateModel customerCate);
        public Task<bool> UpdateCustomerCate(int customerCateId, CustomerCateModel customerCate);
        public Task<bool> DeleteCustomerCate(int customerCateId);

    }
}
