using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionOrder;

namespace VErp.Services.Manafacturing.Service.ProductionOrder
{
    public interface IProductionScheduleService
    {
        Task<IList<ProductionPlaningOrderModel>> GetProductionPlaningOrders();
        Task<IList<ProductionPlaningOrderDetailModel>> GetProductionPlaningOrderDetail(int productionOrderId);
        Task<PageData<ProductionScheduleModel>> GetProductionSchedule(string keyword, int page, int size, string orderByFieldName, bool asc, Clause filters = null);
        Task<List<ProductionScheduleInputModel>> UpdateProductionSchedule(List<ProductionScheduleInputModel> data);
        Task<List<ProductionScheduleInputModel>> CreateProductionSchedule(List<ProductionScheduleInputModel> data);
        Task<bool> DeleteProductionSchedule(int[] productionScheduleIds);
    }
}
