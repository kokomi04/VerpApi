using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.RequestPart;

namespace VErp.Services.Manafacturing.Service.Outsource
{
    public interface IOutsourcePartRequestService
    {
        Task<PageData<OutsourcePartRequestSearchModel>> Search(string keyWord, int page, int size, long fromDate, long toDate, long? productionOrderId, Clause filters = null);
        Task<OutsourcePartRequestModel> GetOutsourcePartRequest(long outsourcePartRequestId = 0);
        Task<long> CreateOutsourcePartRequest(OutsourcePartRequestModel req, bool isValidate = true);
        Task<bool> UpdateOutsourcePartRequest(long outsourcePartRequestId, OutsourcePartRequestModel req);
        Task<bool> DeletedOutsourcePartRequest(long outsourcePartRequestId);
        Task<IList<OutsourcePartRequestDetailInfo>> GetOutsourcePartRequestDetailByProductionOrderId(long productionOrderId);
        Task<bool> UpdateOutsourcePartRequestStatus(long[] outsourcePartRequestId);
        Task<IList<OutsourcePartRequestOutput>> GetOutsourcePartRequestByProductionOrderId(long productionOrderId);
        Task<IList<MaterialsForProductOutsource>> GetMaterialsForProductOutsource(long outsourcePartRequestId, long[] productId);
        Task<bool> CheckHasPurchaseOrder(long outsourcePartRequestId);

    }
}
