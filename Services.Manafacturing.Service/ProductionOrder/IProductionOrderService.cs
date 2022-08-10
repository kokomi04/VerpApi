using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Service.ProductionOrder.Implement;
using ProductionOrderEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionOrder;

namespace VErp.Services.Manafacturing.Service.ProductionOrder
{
    public interface IProductionOrderService
    {
        Task<IList<ProductionOrderListModel>> GetProductionOrdersByCodes(IList<string> productionOrderCodes);
        Task<IList<ProductionOrderListModel>> GetProductionOrdersByIds(IList<long> productionOrderIds);

        Task<PageData<ProductionOrderListModel>> GetProductionOrders(int? monthPlanId, string keyword, int page, int size, string orderByFieldName, bool asc, long fromDate, long toDate, bool? hasNewProductionProcessVersion = null, Clause filters = null);
        Task<PageData<ProductOrderModelExtra>> GetProductionOrderList(string keyword, int page, int size, string orderByFieldName, bool asc, long fromDate, long toDate, Clause filters = null);
        Task<IList<ProductionOrderExtraInfo>> GetProductionOrderExtraInfo(long orderId);
        Task<ProductionOrderOutputModel> GetProductionOrder(long productionOrderId);
        Task<IList<ProductionOrderDetailByOrder>> GetProductionHistoryByOrder(IList<string> orderCodes, IList<int> productIds);

        Task<IList<OrderProductInfo>> GetOrderProductInfo(IList<long> productionOderIds);

        Task<ProductionOrderInputModel> UpdateProductionOrder(long productionOrderId, ProductionOrderInputModel data);
        Task<ProductionOrderInputModel> CreateProductionOrder(ProductionOrderInputModel data);
        Task<int> CreateMultipleProductionOrder(int monthPlanId, ProductionOrderInputModel[] data);
        Task<bool> DeleteProductionOrder(long productionOrderId);
        Task<ProductionOrderDetailOutputModel> GetProductionOrderDetail(long? productionOrderDetailId);
        Task<IList<ProductOrderModel>> GetProductionOrders();

        Task<bool> UpdateProductionOrderStatus(ProductionOrderStatusDataModel data);
        Task<bool> UpdateManualProductionOrderStatus(long productionOrderId, ProductionOrderStatusDataModel status);
        Task<bool> EditNote(long productionOrderDetailId, string note);
        Task<bool> EditDate(long[] productionOrderDetailId, long startDate, long planEndDate, long endDate);

        Task<ProductionCapacityModel> GetProductionCapacity(int? monthPlanId, long fromDate, long toDate, int? assignDepartmentId);

        Task<IList<ProductionStepWorkloadModel>> ListWorkLoads(long productionOrderId);

        Task<CapacityStepByProduction> GetProductionWorkLoads(IList<ProductionOrderEntity> productionOrders, long? assignDepartmentId);

        Task<ProductionOrderConfigurationModel> GetProductionOrderConfiguration();
        Task<bool> UpdateProductionOrderConfiguration(ProductionOrderConfigurationModel model);
        Task<bool> UpdateProductionProcessVersion(long productionOrderId, int productId);
    }
}
