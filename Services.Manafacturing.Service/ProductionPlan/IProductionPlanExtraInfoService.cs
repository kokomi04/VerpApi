using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionPlan;

namespace VErp.Services.Manafacturing.Service.ProductionPlan
{
    public interface IProductionPlanExtraInfoService
    {
      
        Task<IList<ProductionPlanExtraInfoModel>> UpdateProductionPlanExtraInfo(int monthPlanId, IList<ProductionPlanExtraInfoModel> data);
        Task<IList<ProductionPlanExtraInfoModel>> GetProductionPlanExtraInfo(int monthPlanId);
    }
}
