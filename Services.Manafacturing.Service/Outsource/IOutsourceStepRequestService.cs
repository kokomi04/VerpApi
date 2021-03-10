using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.RequestStep;
using VErp.Services.Manafacturing.Model.ProductionStep;

namespace VErp.Services.Manafacturing.Service.Outsource
{
    public interface IOutsourceStepRequestService
    {
        Task<PageData<OutsourceStepRequestSearch>> SearchOutsourceStepRequest(string keyword, int page, int size, string orderByFieldName, bool asc, Clause filters = null);
        Task<IList<OutsourceStepRequestDataInfo>> GetOutsourceStepRequestData(long outsourceStepRequestId);
        Task<IList<OutsourceStepRequestModel>> GetAllOutsourceStepRequest();
        Task<IList<ProductionStepInOutsourceStepRequest>> GetProductionStepHadOutsourceStepRequest(long productionOrderId);
        Task<IList<ProductionStepInfo>> GeneralOutsourceStepOfProductionOrder(long productionOrderId);
        Task<bool> UpdateOutsourceStepRequestStatus(long[] outsourceStepRequestId);
        Task<IList<OutsourceStepRequestDetailOutput>> GetOutsourceStepRequestDatasByProductionOrderId(long productionOrderId);

        //refactor

        Task<OutsourceStepRequestPrivateKey> AddOutsourceStepRequest(OutsourceStepRequestInput requestModel);
        Task<OutsourceStepRequestOutput> GetOutsourceStepRequestOutput(long outsourceStepRequestId);
        Task<bool> UpdateOutsourceStepRequest(long outsourceStepRequestId, OutsourceStepRequestInput req);
        Task<bool> DeleteOutsourceStepRequest(long outsourceStepRequestId);

    }
}
