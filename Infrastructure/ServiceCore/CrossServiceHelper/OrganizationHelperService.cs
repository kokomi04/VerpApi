using GrpcProto.Protos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IOrganizationHelperService
    {
        Task<BaseCustomerModel> CustomerInfo(int customerId);

        Task<BusinessInfoModel> BusinessInfo();
        Task<IList<DepartmentSimpleModel>> GetDepartmentSimples(int[] departmentId);
    }


    public class OrganizationHelperService : IOrganizationHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly OrganizationProvider.OrganizationProviderClient _organizationClient;
        public OrganizationHelperService(IHttpCrossService httpCrossService,
            IOptions<AppSetting> appSetting,
            ILogger<ProductHelperService> logger,
            OrganizationProvider.OrganizationProviderClient organizationClient)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
            _organizationClient = organizationClient;
        }

        public async Task<BaseCustomerModel> CustomerInfo(int customerId)
        {
            if (_appSetting.GrpcInternal?.Address?.Contains("https") == true)
            {
                var result = await _organizationClient.CustomerInfoAsync(new CustomerInfoRequest{ CustomerId = customerId });
                return new BaseCustomerModel
                {
                    Address = result.Address,
                    CustomerCode = result.CustomerCode,
                    CustomerName = result.CustomerName,
                    CustomerStatusId = (Commons.Enums.MasterEnum.EnumCustomerStatus)result.CustomerStatusId,
                    CustomerTypeId = (Commons.Enums.MasterEnum.EnumCustomerType)result.CustomerTypeId,
                    DebtDays = result.DebtDays,
                    Description = result.Description,
                    Email = result.Email,
                    Identify = result.Identify,
                    IsActived = result.IsActived,
                    LegalRepresentative = result.LegalRepresentative,
                    PhoneNumber = result.PhoneNumber,
                    TaxIdNo = result.TaxIdNo,
                    Website = result.Website
                    
                };
            }
            return await _httpCrossService.Get<BaseCustomerModel>($"api/internal/InternalCustomer/{customerId}");
        }

        public async Task<BusinessInfoModel> BusinessInfo()
        {
            if (_appSetting.GrpcInternal?.Address?.Contains("https") == true)
            {
                var result = await _organizationClient.BusinessInfoAsync(new Google.Protobuf.WellKnownTypes.Empty());
                return new BusinessInfoModel
                {
                    Website = result.Website,
                    Address = result.Address,
                    CompanyName = result.CompanyName,
                    Email = result.Email,
                    LegalRepresentative = result.LegalRepresentative,
                    LogoFileId = result.LogoFileId,
                    PhoneNumber = result.PhoneNumber,
                    TaxIdNo = result.TaxIdNo
                };
            }
            return await _httpCrossService.Get<BusinessInfoModel>($"api/internal/InternalBussiness/businessInfo");
        }

        public async Task<IList<DepartmentSimpleModel>> GetDepartmentSimples(int[] departmentIds)
        {
            return await _httpCrossService.Post<IList<DepartmentSimpleModel>>($"api/internal/InternalDepartment/GetByIds", departmentIds);
        }
    }
}
