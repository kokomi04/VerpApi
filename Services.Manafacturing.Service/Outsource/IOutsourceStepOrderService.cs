using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.Outsource.Order;

namespace VErp.Services.Manafacturing.Service.Outsource
{
    public interface IOutsourceStepOrderService
    {
        Task<long> CreateOutsourceStepOrderPart(OutsourceStepOrderModel req);
    }
}
