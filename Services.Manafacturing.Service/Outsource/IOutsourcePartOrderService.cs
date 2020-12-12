using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.Order;

namespace VErp.Services.Manafacturing.Service.Outsource
{
    public interface IOutsourcePartOrderService
    {
        Task<long> CreateOutsourceOrderPart(OutsourceOrderInfo req);
        Task<PageData<OutsourceOrderPartDetailOutput>> GetListOutsourceOrderPart(string keyword, int page, int size, Clause filters = null);
        Task<bool> UpdateOutsourceOrderPart(long outsourceOrderId,OutsourceOrderInfo req);
        Task<bool> DeleteOutsourceOrderPart(long outsourceOrderId);
        Task<OutsourceOrderInfo> GetOutsourceOrderPart(long outsourceOrderId);
    }
}
