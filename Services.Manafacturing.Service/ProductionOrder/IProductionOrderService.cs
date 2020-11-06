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
        Task<PageData<ProductionOrderListModel>> GetProductionOrders(string keyword, int page, int size, Clause filters = null);
        Task<ProductionOrderModel> GetProductionOrder(int productionOrderId);
        Task<ProductionOrderModel> UpdateProductionOrder(int productionOrderId, ProductionOrderModel data);
        Task<ProductionOrderModel> CreateProductionOrder(ProductionOrderModel data);
        Task<bool> DeleteProductionOrder(int productionOrderId);
    }
}
