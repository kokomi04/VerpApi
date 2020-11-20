using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.Order;

namespace VErp.Services.Manafacturing.Service.Outsource
{
    public interface IOutsourceOrderService
    {
        Task<long> CreateOutsourceOrderPart(OutsourceOrderInfo req);
        Task<PageData<OutsourceOrderPartDetailOutput>> GetListOutsourceOrderPart(string keyword, int page, int size);


    }
}
