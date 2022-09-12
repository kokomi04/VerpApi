using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;

namespace VErp.Services.Manafacturing.Service.ProductionOrder
{
    public interface IProductionOrderMaterialSetService
    {
        Task<ProductionOrderMaterialInfo> GetProductionOrderMaterialsCalc(long productionOrderId);

        Task<bool> UpdateAll(long productionOrderId, IList<ProductionOrderMaterialSetModel> model);


    }
}
