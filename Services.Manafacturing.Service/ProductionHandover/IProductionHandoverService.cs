using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using VErp.Services.Manafacturing.Service.StatusProcess;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.ProductionHandover
{
    public interface IProductionHandoverService : IStatusProcessService
    {
        Task<PageData<DepartmentHandoverModel>> GetDepartmentHandovers(long departmentId, string keyword, int page, int size, long fromDate, long toDate, int? stepId, int? productId, bool? isInFinish, bool? isOutFinish, EnumProductionStepLinkDataRoleType? productionStepLinkDataRoleTypeId);
        Task<IList<ProductionHandoverModel>> GetProductionHandovers(long productionOrderId);
        Task<ProductionHandoverModel> CreateProductionHandover(long productionOrderId, ProductionHandoverInputModel data);
        Task<ProductionHandoverModel> CreateStatictic(long productionOrderId, ProductionHandoverInputModel data);
        Task<IList<ProductionHandoverModel>> CreateMultipleStatictic(long productionOrderId, IList<ProductionHandoverInputModel> data);
        Task<ProductionHandoverModel> ConfirmProductionHandover(long productionOrderId, long productionHandoverId, EnumHandoverStatus status);
        Task<bool> AcceptProductionHandoverBatch(IList<ProductionHandoverAcceptBatchInput> req);

        Task<bool> DeleteProductionHandover(long productionHandoverId);
        Task<bool> CreateProductionHandoverPatch(IList<ProductionHandoverInputModel> datas);
    }
}
