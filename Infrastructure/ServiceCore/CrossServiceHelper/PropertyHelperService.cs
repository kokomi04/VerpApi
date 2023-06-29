using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface.Stock;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IPropertyHelperService
    {
        Task<PropertyInfoModel> GetInfo(int propertyId);
        Task<IList<PropertyInfoModel>> GetByIds(IList<int> propertyIds);
    }


    public class PropertyHelperService : IPropertyHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly VErp.Grpc.Protos.Product.ProductClient _productClient;

        public PropertyHelperService(IHttpCrossService httpCrossService, IOptions<AppSetting> appSetting, ILogger<ProductHelperService> logger, VErp.Grpc.Protos.Product.ProductClient productClient)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
            _productClient = productClient;
        }

        public async Task<PropertyInfoModel> GetInfo(int propertyId)
        {
            return await _httpCrossService.Get<PropertyInfoModel>($"api/internal/InternalProperty/{propertyId}");
        }

        public async Task<IList<PropertyInfoModel>> GetByIds(IList<int> propertyIds)
        {
            return await _httpCrossService.Post<IList<PropertyInfoModel>>($"api/internal/InternalProperty/GetByIds", propertyIds);
        }
    }
}
