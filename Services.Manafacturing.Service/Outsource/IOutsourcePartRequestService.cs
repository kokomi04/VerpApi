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
        Task<OutsourcePartRequestInfo> GetOutsourcePartRequestExtraInfo(int requestOutsourcePartId = 0);
        Task<long> CreateOutsourcePartRequest(OutsourcePartRequestInfo req);
        Task<bool> UpdateOutsourcePartRequest(int requestOutsourcePartId, OutsourcePartRequestInfo req);
        Task<bool> DeletedOutsourcePartRequest(int requestOutsourcePart);
        Task<IList<OutsourcePartRequestOutput>> GetOutsourcePartRequestByProductionOrderId(long productionOrderId);

    }
}
