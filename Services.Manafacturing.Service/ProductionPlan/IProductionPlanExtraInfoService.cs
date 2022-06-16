using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductionPlan;

namespace VErp.Services.Manafacturing.Service.ProductionPlan
{
    public interface IProductionPlanExtraInfoService
    {

        Task<IList<ProductionPlanExtraInfoModel>> UpdateProductionPlanExtraInfo(int monthPlanId, IList<ProductionPlanExtraInfoModel> data);
        Task<IList<ProductionPlanExtraInfoModel>> GetProductionPlanExtraInfo(int monthPlanId);
    }
}
