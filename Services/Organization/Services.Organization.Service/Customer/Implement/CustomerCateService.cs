using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.Organization.Customer;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Customer;

namespace VErp.Services.Organization.Service.Customer.Implement
{
    public class CustomerCateService : ICustomerCateService
    {
        private readonly OrganizationDBContext organizationDBContext;
        private readonly IMapper mapper;
        private readonly ObjectActivityLogFacade _customerActivityLog;

        public CustomerCateService(OrganizationDBContext organizationDBContext, IMapper mapper, IActivityLogService activityLogService)
        {
            this.organizationDBContext = organizationDBContext;
            this.mapper = mapper;
            _customerActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.CustomerCate);
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

            await _customerActivityLog.LogBuilder(() => CustomerCateActivityLogMessageLog.Delete)
               .MessageResourceFormatDatas(info.CustomerCateCode)
               .ObjectId(info.CustomerCateId)
               .JsonData(info.JsonSerialize())
               .CreateLog();

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

            await _customerActivityLog.LogBuilder(() => CustomerCateActivityLogMessageLog.Update)
              .MessageResourceFormatDatas(info.CustomerCateCode)
              .ObjectId(info.CustomerCateId)
              .JsonData(customerCate.JsonSerialize())
              .CreateLog();

            return true;
        }

        public async Task<int> CreateCustomerCate(CustomerCateModel customerCate)
        {
            var info = mapper.Map<CustomerCate>(customerCate);
            await organizationDBContext.CustomerCate.AddAsync(info);
            await organizationDBContext.SaveChangesAsync();

            await _customerActivityLog.LogBuilder(() => CustomerCateActivityLogMessageLog.Create)
              .MessageResourceFormatDatas(info.CustomerCateCode)
              .ObjectId(info.CustomerCateId)
              .JsonData(customerCate.JsonSerialize())
              .CreateLog();

            return info.CustomerCateId;
        }
    }
}
