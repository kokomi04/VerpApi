using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.RequestStep;

namespace VErp.Services.Manafacturing.Service.Outsource
{
    public interface IOutsourceStepRequestService
    {
        Task<OutsourceStepRequestInfo> GetOutsourceStepRequest(long outsourceStepRequestId);
        Task<long> CreateOutsourceStepRequest(OutsourceStepRequestModel req);
        Task<bool> UpdateOutsourceStepRequest(long outsourceStepRequestId, OutsourceStepRequestModel req);
        Task<bool> DeleteOutsourceStepRequest(long outsourceStepRequestId);
        Task<PageData<OutsourceStepRequestSearch>> GetListOutsourceStepRequest(string keyword, int page, int size, string orderByFieldName, bool asc, Clause filters = null);
        Task<IList<OutsourceStepRequestDataInfo>> GetProductionStepInOutsourceStepRequest(long outsourceStepRequestId);
        Task<IList<OutsourceStepRequestModel>> GetAllOutsourceStepRequest();
    }
}
