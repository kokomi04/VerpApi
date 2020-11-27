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
        Task<PageData<RequestOutsourcePartDetailInfo>> GetListOutsourcePartRequest(string keyWord, int page, int size, Clause filters = null);
        Task<RequestOutsourcePartInfo> GetOutsourcePartRequestExtraInfo(int requestOutsourcePartId = 0);
        Task<long> CreateOutsourcePartRequest(RequestOutsourcePartInfo req);
        Task<bool> UpdateOutsourcePartRequest(int requestOutsourcePartId, RequestOutsourcePartInfo req);
        Task<bool> DeletedOutsourcePartRequest(int requestOutsourcePart);

    }
}
