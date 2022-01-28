using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.RequestStep;
using VErp.Services.Manafacturing.Model.ProductionStep;

namespace VErp.Services.Manafacturing.Service.Outsource
{
    public interface IOutsourceStepRequestService
    {
        Task<IList<OutsourceStepRequestModel>> GetAllOutsourceStepRequest();
        Task<bool> UpdateOutsourceStepRequestStatus(long[] outsourceStepRequestId);
        Task<IList<OutsourceStepRequestDetailOutput>> GetOutsourceStepRequestDatasByProductionOrderId(long productionOrderId);

        //refactor

        Task<OutsourceStepRequestPrivateKey> AddOutsourceStepRequest(OutsourceStepRequestInput requestModel);
        Task<OutsourceStepRequestOutput> GetOutsourceStepRequestOutput(long outsourceStepRequestId);
        Task<bool> UpdateOutsourceStepRequest(long outsourceStepRequestId, OutsourceStepRequestInput req);
        Task<bool> DeleteOutsourceStepRequest(long outsourceStepRequestId);
        Task<PageData<OutsourceStepRequestSearch>> SearchOutsourceStepRequest(string keyword, int page, int size, string orderByFieldName, bool asc, long fromDate, long toDate, Clause filters = null);
        Task<IList<OutsourceStepRequestDataExtraInfo>> GetOutsourceStepRequestData(long outsourceStepRequestId);
        Task<IList<OutsourceStepRequestDataExtraInfo>> GetOutsourceStepRequestData(long[] productionStepLinkDataId);

        Task<IList<OutsourceStepRequestMaterialsConsumption>> GetOutsourceStepMaterialsConsumption(long outsourceStepRequestId);
    }
}
