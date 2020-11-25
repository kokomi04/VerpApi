using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.Outsource.RequestStep;

namespace VErp.Services.Manafacturing.Service.Outsource
{
    public interface IOutsourceStepRequestService
    {
        Task<OutsourceStepRequestInfo> GetOutsourceStepRequest(long outsourceStepRequestId);
        Task<long> CreateOutsourceStepRequest(OutsourceStepRequestInfo req);
        Task<bool> UpdateOutsourceStepRequest(long outsourceStepRequestId, OutsourceStepRequestInfo req);
        Task<bool> DeleteOutsourceStepRequest(long outsourceStepRequestId);
    }
}
