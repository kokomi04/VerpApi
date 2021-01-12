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
    public interface IProductionHandoverService
    {
        Task<IList<ProductionHandoverModel>> GetProductionHandovers(long scheduleTurnId);
        Task<IList<ProductionInventoryRequirementModel>> GetProductionInventoryRequirements(long scheduleTurnId);

        Task<ProductionHandoverModel> CreateProductionHandover(long scheduleTurnId, ProductionHandoverInputModel data);

        Task<ProductionHandoverModel> ConfirmProductionHandover(long scheduleTurnId, long productionHandoverId, EnumHandoverStatus status);
    }
}
