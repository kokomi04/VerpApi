using System.Data;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IProductionHandoverHelperService
    {
        Task<bool> ChangeAssignedProgressStatus(string productionOrderCode, string inventoryCode, DataTable inventories);

        Task<bool> UpdateIgnoreAllocation(string[] productionOrderCodes);
    }
    public class ProductionHandoverHelperService : IProductionHandoverHelperService
    {
        private readonly IHttpCrossService _httpCrossService;

        public ProductionHandoverHelperService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }

        public async Task<bool> ChangeAssignedProgressStatus(string productionOrderCode, string inventoryCode, DataTable inventories)
        {
            return await _httpCrossService.Put<bool>($"api/internal/InternalProductionHandover/status", new
            {
                ProductionOrderCode = productionOrderCode,
                InventoryCode = inventoryCode,
                Inventories = inventories.ConvertData<InternalProductionInventoryRequirementModel>()
            });
        }


        public async Task<bool> UpdateIgnoreAllocation(string[] productionOrderCodes)
        {
            return await _httpCrossService.Put<bool>($"api/internal/InternalProductionHandover/ignore-allocation", productionOrderCodes);
        }
    }
}
