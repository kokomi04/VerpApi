using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.Stages;

namespace VErp.Services.Manafacturing.Service.Stages
{
    public interface IProductionProcessService
    {
        Task<ProductionProcessModel> GetProductionProcessByProductId(int productId);
        Task<ProductionStagesModel> GetProductionStagesById(int productId, int stagesId);
        Task<bool> UpdateProductionStagesById(int productId, int stagesId, ProductionStagesModel req);
        Task<int> CreateProductionStages(int productId, ProductionStagesModel req);
        Task<bool> DeleteProductionStagesById(int productId, int stagesId);
        Task<bool> GenerateStagesMapping(int productId, List<ProductionStagesMappingModel> req);
    }
}
