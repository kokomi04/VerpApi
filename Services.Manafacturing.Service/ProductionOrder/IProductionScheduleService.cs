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
        Task<PageData<ProductionScheduleModel>> GetProductionSchedule(string keyword, int page, int size, Clause filters = null);
        Task<ProductionScheduleModel> UpdateProductionSchedule(int productionOrderDetailId, ProductionScheduleModel data);
        Task<ProductionScheduleModel> CreateProductionSchedule(ProductionScheduleModel data);
    }
}
