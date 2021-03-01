using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;

namespace VErp.Services.Manafacturing.Service.ProductionOrder
{
    public interface IProductionOrderMaterialsService
    {
        Task<ProductionOrderMaterialsModel> GetProductionOrderMaterialsCalc(long productionOrderId);
        Task<bool> UpdateProductionOrderMaterials(long productionOrderId, IList<ProductionOrderMaterialsInput> materials);
        Task<IList<ProductionOrderMaterialsOutput>> GetProductionOrderMaterials(long productionOrderId);
    }
}
