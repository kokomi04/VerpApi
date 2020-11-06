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
        Task<PageData<RequestOutsourcePartModel>> GetListRequest(string keyWord, int page, int size);
        Task<RequestOutsourcePartModel> GetRequestById(int outsourceId);
        Task<int> CreateRequest(RequestOutsourcePartModel req);
        Task<bool> UpdateRequest(int requestId, RequestOutsourcePartModel req);
        Task<bool> DeleteRequest(int requestId);

    }
}
