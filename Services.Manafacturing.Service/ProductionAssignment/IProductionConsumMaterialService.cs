using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductionAssignment;

namespace VErp.Services.Manafacturing.Service.ProductionAssignment
{
    public interface IProductionConsumMaterialService
    {
        Task<IList<ProductionConsumMaterialModel>> GetConsumMaterials(int departmentId, long scheduleTurnId, long productionStepId);
        Task<long> CreateConsumMaterial(int departmentId, long scheduleTurnId, long productionStepId, ProductionConsumMaterialModel model);
        Task<bool> UpdateConsumMaterial(int departmentId, long scheduleTurnId, long productionStepId, long productionConsumMaterialId, ProductionConsumMaterialModel model);
        Task<bool> DeleteConsumMaterial(int departmentId, long scheduleTurnId, long productionStepId, long productionConsumMaterialId);
    }
}
