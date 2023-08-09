using System.Data;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject.InternalDataInterface.Manufacturing;
using VErp.Commons.Library;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper.Manufacture
{
    //public interface IProductionOrderHelperService
    //{
    //    Task<bool> UpdateProductionOrderStatus(string productionOrderCode, DataTable inventories, EnumProductionStatus status);
    //}
    //public class ProductionOrderHelperService : IProductionOrderHelperService
    //{
    //    private readonly IHttpCrossService _httpCrossService;

    //    public ProductionOrderHelperService(IHttpCrossService httpCrossService)
    //    {
    //        _httpCrossService = httpCrossService;
    //    }

    //    public async Task<bool> UpdateProductionOrderStatus(string productionOrderCode, DataTable inventories, EnumProductionStatus status)
    //    {
    //        return await _httpCrossService.Put<bool>($"api/internal/InternalProductionOrder/status", new
    //        {
    //            ProductionOrderCode = productionOrderCode,
    //            ProductionOrderStatus = status,
    //            Inventories = inventories.ConvertData<InternalProductionInventoryRequirementModel>()
    //        });
    //    }
    //}
}
