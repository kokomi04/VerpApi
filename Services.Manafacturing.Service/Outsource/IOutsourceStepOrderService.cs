using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.Order;

namespace VErp.Services.Manafacturing.Service.Outsource
{
    public interface IOutsourceStepOrderService
    {
        Task<long> CreateOutsourceStepOrderPart(OutsourceStepOrderModel req);
        Task<OutsourceStepOrderModel> GetOutsourceStepOrder(long outsourceStepOrderId);
        Task<PageData<OutsourceStepOrderSeach>> SearchOutsourceStepOrder(string keyword, int page, int size);
        Task<bool> UpdateOutsourceStepOrder(long outsourceStepOrderId, OutsourceStepOrderModel req);
        Task<bool> DeleteOutsouceStepOrder(long outsourceStepOrderId);
    }
}
