using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionHandover;

namespace VErp.Services.Manafacturing.Service.ProductionHandover
{
    public interface IProductionHumanResourceService
    {

        Task<IList<ProductionHumanResourceModel>> GetProductionHumanResources(long productionOrderId);
   
        Task<ProductionHumanResourceModel> CreateProductionHumanResource(long productionOrderId, ProductionHumanResourceInputModel data);
        Task<IList<ProductionHumanResourceModel>> CreateMultipleProductionHumanResource(long productionOrderId, IList<ProductionHumanResourceInputModel> data);
        Task<bool> DeleteProductionHumanResource(long productionHumanResourceId);

    }
}
