using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Services.Organization.Model.Customer;

namespace VErp.Services.Organization.Service.Customer.Implement
{
    public class CustomerCateService : ICustomerCateService
    {
        private readonly OrganizationDBContext organizationDBContext;
        private readonly IMapper mapper;
        public CustomerCateService(OrganizationDBContext organizationDBContext, IMapper mapper)
        {
            this.organizationDBContext = organizationDBContext;
            this.mapper = mapper;
        }

        public async Task<bool> DeleteCustomerCate(int customerCateId)
        {
            var info = await organizationDBContext.CustomerCate.FirstOrDefaultAsync(c => c.CustomerCateId == customerCateId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }
            info.IsDeleted = true;
            await organizationDBContext.SaveChangesAsync();
            return true;
        }

        public async Task<CustomerCateModel> GetInfo(int customerCateId)
        {
            var info = await organizationDBContext.CustomerCate.FirstOrDefaultAsync(c => c.CustomerCateId == customerCateId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }
            return mapper.Map<CustomerCateModel>(info);
        }

        public async Task<IList<CustomerCateModel>> GetList()
        {
            var lst = await organizationDBContext.CustomerCate.ToListAsync();
            return mapper.Map<List<CustomerCateModel>>(lst);
        }

        public async Task<bool> UpdateCustomerCate(int customerCateId, CustomerCateModel customerCate)
        {
            customerCate.CustomerCateId = customerCateId;
            var info = await organizationDBContext.CustomerCate.FirstOrDefaultAsync(c => c.CustomerCateId == customerCateId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }
            mapper.Map(customerCate, info);
            await organizationDBContext.SaveChangesAsync();
            return true;
        }

        public async Task<int> CreateCustomerCate(CustomerCateModel customerCate)
        {
            var info = mapper.Map<CustomerCate>(customerCate);
            await organizationDBContext.CustomerCate.AddAsync(info);
            await organizationDBContext.SaveChangesAsync();
            return info.CustomerCateId;
        }
    }
}
