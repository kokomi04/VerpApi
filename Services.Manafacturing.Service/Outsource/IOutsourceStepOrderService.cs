using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.Order;

namespace VErp.Services.Manafacturing.Service.Outsource
{
    public interface IOutsourceStepOrderService
    {
        Task<long> CreateOutsourceStepOrder(OutsourceStepOrderInput req);
        Task<OutsourceStepOrderOutput> GetOutsourceStepOrder(long outsourceStepOrderId);
        Task<PageData<OutsourceStepOrderSeach>> SearchOutsourceStepOrder(string keyword, int page, int size, string orderByFieldName, bool asc, long fromDate, long toDate, Clause filters = null);
        Task<bool> UpdateOutsourceStepOrder(long outsourceStepOrderId, OutsourceStepOrderOutput req);
        Task<bool> DeleteOutsouceStepOrder(long outsourceStepOrderId);

        Task<bool> UpdateOutsourceStepOrderStatus(long outsourceStepOrderId);
    }
}
