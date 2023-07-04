using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Customer;
using CustomerEntity = VErp.Infrastructure.EF.OrganizationDB.Customer;

namespace VErp.Services.Organization.Service.Customer
{
    public interface ICustomerService
    {
        Task<int> AddCustomer(int updatedUserId, CustomerModel data);
        Task<PageData<CustomerListOutput>> GetList(string keyword, int? customerCateId, IList<int> customerIds, EnumCustomerStatus? customerStatusId, int page, int size, Clause filters = null);
        Task<(Stream stream, string fileName, string contentType)> ExportList(IList<string> fieldNames, string keyword, int? customerCateId, IList<int> customerIds, EnumCustomerStatus? customerStatusId, int page, int size, Clause filters = null);
        Task<IList<CustomerListOutput>> GetListByIds(IList<int> customerIds);
        Task<CustomerModel> GetCustomerInfo(int customerId);
        Task<bool> UpdateCustomer(int customerId, CustomerModel data);
        Task<bool> DeleteCustomer(int customerId);
        CategoryNameModel GetCustomerFieldDataForMapping();
        Task<bool> ImportCustomerFromMapping(ImportExcelMapping mapping, Stream stream);
        Task<Dictionary<CustomerEntity, CustomerModel>> AddBatchCustomers(IList<CustomerModel> customers);
        Task<CustomerEntity> UpdateCustomerBase(int customerId, CustomerModel data, bool igDeleteRef = false);
        Task<Dictionary<CustomerEntity, CustomerModel>> AddBatchCustomersBase(IList<CustomerModel> customers);

        Task<IList<ObjectBillInUsedInfo>> GetCustomerTopInUsed(IList<int> customerIds, bool isCheckExistOnly);
    }
}
