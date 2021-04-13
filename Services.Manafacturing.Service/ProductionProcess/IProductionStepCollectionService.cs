using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionStep;

namespace VErp.Services.Manafacturing.Service.ProductionProcess
{
    public interface IProductionStepCollectionService
    {
        Task<long> AddProductionStepCollection(ProductionStepCollectionModel model);
        Task<bool> UpdateProductionStepCollection(long productionStepCollectionId, ProductionStepCollectionModel model);
        Task<bool> DeleteProductionStepCollection(long productionStepCollectionId);
        Task<ProductionStepCollectionModel> GetProductionStepCollection(long productionStepCollectionId);
        Task<PageData<ProductionStepCollectionSearch>> SearchProductionStepCollection(string keyword, int page, int size);
    }
}
