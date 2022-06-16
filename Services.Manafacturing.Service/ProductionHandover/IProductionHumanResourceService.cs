using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductionHandover;

namespace VErp.Services.Manafacturing.Service.ProductionHandover
{
    public interface IProductionHumanResourceService
    {

        Task<IList<ProductionHumanResourceModel>> GetByProductionOrder(long productionOrderId);

        Task<ProductionHumanResourceModel> Create(long productionOrderId, ProductionHumanResourceInputModel data);

        Task<ProductionHumanResourceModel> Update(long productionOrderId, long productionHumanResourceId, ProductionHumanResourceInputModel data);

        Task<IList<ProductionHumanResourceModel>> CreateMultiple(long productionOrderId, IList<ProductionHumanResourceInputModel> data);
        Task<bool> Delete(long productionHumanResourceId);

        Task<IList<ProductionHumanResourceModel>> GetByDepartment(int departmentId, long startDate, long endDate);
        Task<IList<ProductionHumanResourceModel>> CreateMultipleByDepartment(int departmentId, long startDate, long endDate, IList<ProductionHumanResourceInputModel> data);

        Task<IList<UnFinishProductionInfo>> GetUnFinishProductionInfo(int departmentId, long startDate, long endDate);
    }
}
