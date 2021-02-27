using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Commons.GlobalObject;
using VErp.Commons.Enums.Manafacturing;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IProductionOrderHelperService
    {
        Task<bool> UpdateProductionOrderStatus(long productionOrderId, EnumProductionStatus status);
    }
    public class ProductionOrderHelperService : IProductionOrderHelperService
    {
        private readonly IHttpCrossService _httpCrossService;

        public ProductionOrderHelperService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }

        public async Task<bool> UpdateProductionOrderStatus(long productionOrderId, EnumProductionStatus status)
        {
            return await _httpCrossService.Put<bool>($"api/internal/InternalProductionOrder/{productionOrderId}/status", new { ProductionOrderStatus = status });
        }
    }
}
