using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.RequestPart;

namespace VErp.Services.Manafacturing.Service.Outsource
{
    public interface IRequestOutsourcePartService
    {
        Task<PageData<RequestOutsourcePartInfo>> GetListRequestOutsourcePart(string keyWord, int page, int size);
        Task<IList<RequestOutsourcePartInfo>> GetRequestOutsourcePartExtraInfo(int productionOrderDetailId = 0);
        Task<bool> CreateRequestOutsourcePart(List<RequestOutsourcePartModel> req);
        Task<bool> UpdateRequestOutsourcePart(int productionOrderDetailId, List<RequestOutsourcePartModel> req);

    }
}
