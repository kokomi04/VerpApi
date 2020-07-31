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
    }


    public class OrganizationHelperService : IOrganizationHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        public OrganizationHelperService(IHttpCrossService httpCrossService, IOptions<AppSetting> appSetting, ILogger<ProductHelperService> logger)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
        }

        public async Task<BaseCustomerModel> CustomerInfo(int customerId)
        {
            return await _httpCrossService.Get<BaseCustomerModel>($"api/internal/InternalCustomer/{customerId}");
        }

        public async Task<BusinessInfoModel> BusinessInfo()
        {
            return await _httpCrossService.Get<BusinessInfoModel>($"api/internal/InternalBussiness/businessInfo");
        }

    }
}
