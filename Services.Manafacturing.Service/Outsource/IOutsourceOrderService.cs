using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.Order;

namespace VErp.Services.Manafacturing.Service.Outsource
{
    public interface IOutsourceOrderService
    {
        Task<PageData<OutsoureOrderInfo>> GetListOutsourceOrder(int requestContainerTypeId, string keyWord, int page, int size);
        Task<int> CreateOutsourceOrder(OutsoureOrderInfo req);
        Task<bool> UpdateOutsourceOrder(int outsourceOrderId, OutsoureOrderInfo req);
        Task<bool> DeleteOutsourceOrder(int outsourceOrderId);
    }
}
