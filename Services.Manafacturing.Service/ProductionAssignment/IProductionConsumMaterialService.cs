using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductionAssignment;

namespace VErp.Services.Manafacturing.Service.ProductionAssignment
{
    public interface IProductionConsumMaterialService
    {
        Task<IList<ProductionConsumMaterialModel>> GetConsumMaterials(int departmentId, long productionOrderId, long productionStepId);
        Task<long> CreateConsumMaterial(int departmentId, long productionOrderId, long productionStepId, ProductionConsumMaterialModel model);
        Task<bool> UpdateConsumMaterial(int departmentId, long productionOrderId, long productionStepId, long productionConsumMaterialId, ProductionConsumMaterialModel model);
        Task<bool> DeleteConsumMaterial(int departmentId, long productionOrderId, long productionStepId, long productionConsumMaterialId);
        Task<bool> DeleteMaterial(int departmentId, long productionOrderId, long productionStepId, int objectTypeId, long objectId);
    }
}
