using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionOrder;

namespace VErp.Services.Manafacturing.Service.ProductionOrder
{
    public interface IProductionOrderService
    {
        Task<PageData<ProductionOrderListModel>> GetProductionOrders(string keyword, int page, int size, string orderByFieldName, bool asc, Clause filters = null);
        Task<IList<ProductionOrderExtraInfo>> GetProductionOrderExtraInfo(long orderId);
        Task<ProductionOrderOutputModel> GetProductionOrder(int productionOrderId);
        Task<ProductionOrderInputModel> UpdateProductionOrder(int productionOrderId, ProductionOrderInputModel data);
        Task<ProductionOrderInputModel> CreateProductionOrder(ProductionOrderInputModel data);
        Task<bool> DeleteProductionOrder(int productionOrderId);
        Task<ProductionOrderDetailOutputModel> GetProductionOrderDetail(long? productionOrderDetailId);
    }
}
