using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.RequestPart;

namespace VErp.Services.Manafacturing.Service.Outsource
{
    public interface IOutsourcePartRequestService
    {
        Task<PageData<OutsourcePartRequestDetailInfo>> GetListOutsourcePartRequest(string keyWord, int page, int size, Clause filters = null);
        Task<OutsourcePartRequestInfo> GetOutsourcePartRequestExtraInfo(long outsourcePartRequestId = 0);
        Task<long> CreateOutsourcePartRequest(OutsourcePartRequestInfo req);
        Task<bool> UpdateOutsourcePartRequest(long outsourcePartRequestId, OutsourcePartRequestInfo req);
        Task<bool> DeletedOutsourcePartRequest(long outsourcePartRequestId);
        Task<IList<OutsourcePartRequestOutput>> GetOutsourcePartRequestByProductionOrderId(long productionOrderId);
        Task<IList<OutsourcePartRequestDetailInfo>> GetRequestDetailByArrayRequestId(long[] outsourcePartRequestIds);
        Task<bool> UpdateOutsourcePartRequestStatus(long[] outsourcePartRequestId);

    }
}
