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
        Task<PageData<RequestOutsourcePartModel>> GetListRequestOutsourcePart(string keyWord, int page, int size);
        Task<RequestOutsourcePartModel> GetRequestById(int requestOutsourceId);
        Task<int> CreateRequestOutsourcePart(RequestOutsourcePartModel req);
        Task<bool> UpdateRequestOutsourcePart(int requestOutsourceId, RequestOutsourcePartModel req);
        Task<bool> DeleteRequestOutsourcePart(int requestOutsourceId);

    }
}
