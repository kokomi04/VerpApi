using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IProductHelperService
    {
        Task<bool> ValidateProductUnitConversions(Dictionary<int, int> productUnitConvertsionProduct);
        Task<IList<ProductModel>> GetListByCodeAndInternalNames(IList<string> productCodes, IList<string> productInternalNames);
        Task<IList<ProductModel>> GetListProducts(IList<int> productIds);
    }


    public class ProductHelperService : IProductHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        public ProductHelperService(IHttpCrossService httpCrossService, IOptions<AppSetting> appSetting, ILogger<ProductHelperService> logger)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
        }
        public async Task<bool> ValidateProductUnitConversions(Dictionary<int, int> productUnitConvertsionProduct)
        {
            return await _httpCrossService.Post<bool>("api/internal/InternalProduct/ValidateProductUnitConversion", productUnitConvertsionProduct);
        }

        public async Task<IList<ProductModel>> GetListByCodeAndInternalNames(IList<string> productCodes, IList<string> productInternalNames)
        {
            return await _httpCrossService.Post<IList<ProductModel>>("api/internal/InternalProduct/GetListByCodeAndInternalNames", new
            {
                productCodes,
                productInternalNames,
            });
        }

        public async Task<IList<ProductModel>> GetListProducts(IList<int> productIds)
        {
            return await _httpCrossService.Post<IList<ProductModel>>("api/internal/InternalProduct/GetListProductsByIds", productIds);
        }
    }
}
