using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Commons.GlobalObject;
using VErp.Commons.Enums.Manafacturing;
using System.Data;
using VErp.Commons.Library;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IProductionHandoverHelperService
    {
        Task<bool> ChangeAssignedProgressStatus(string productionOrderCode, int departmentId, DataTable inventories);
    }
    public class ProductionHandoverHelperService : IProductionHandoverHelperService
    {
        private readonly IHttpCrossService _httpCrossService;

        public ProductionHandoverHelperService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }

        public async Task<bool> ChangeAssignedProgressStatus(string productionOrderCode, int departmentId, DataTable inventories)
        {
            return await _httpCrossService.Put<bool>($"api/internal/InternalProductionHandover/productionOrder/{productionOrderCode}/department/{departmentId}/status", inventories.ConvertData<ProductionInventoryRequirementModel>());
        }
    }
}
