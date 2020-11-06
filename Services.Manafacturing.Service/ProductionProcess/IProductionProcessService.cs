using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductionStep;

namespace VErp.Services.Manafacturing.Service.ProductionProcess
{
    public interface IProductionProcessService
    {
        Task<ProductionProcessInfo> GetProductionProcessByProductId(int productId);
        Task<ProductionStepInfo> GetProductionStepById(int productId, int productionStepId);
        Task<bool> UpdateProductionStagesById(int productId, int productionStepId, ProductionStepInfo req);
        Task<int> CreateProductionStep(int productId, ProductionStepInfo req);
        Task<bool> DeleteProductionStepById(int productId, int productionStepId);
        Task<bool> GenerateProductionStepMapping(int productId, List<ProductionStepLinkModel> req);
    }
}
