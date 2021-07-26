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
        Task<IList<ProductionOrderListModel>> GetProductionOrdersByCodes(IList<string> productionOrderCodes);

        Task<PageData<ProductionOrderListModel>> GetProductionOrders(string keyword, int page, int size, string orderByFieldName, bool asc, long fromDate, long toDate, Clause filters = null);
        Task<IList<ProductionOrderExtraInfo>> GetProductionOrderExtraInfo(long orderId);
        Task<ProductionOrderOutputModel> GetProductionOrder(long productionOrderId);
        Task<ProductionOrderInputModel> UpdateProductionOrder(long productionOrderId, ProductionOrderInputModel data);
        Task<ProductionOrderInputModel> CreateProductionOrder(ProductionOrderInputModel data);
        Task<bool> DeleteProductionOrder(long productionOrderId);
        Task<ProductionOrderDetailOutputModel> GetProductionOrderDetail(long? productionOrderDetailId);
        Task<IList<ProductOrderModel>> GetProductionOrders();

        Task<bool> UpdateProductionOrderStatus(long productionOrderId, ProductionOrderStatusDataModel data);
        Task<bool> UpdateManualProductionOrderStatus(long productionOrderId, ProductionOrderStatusDataModel status);
    }
}
