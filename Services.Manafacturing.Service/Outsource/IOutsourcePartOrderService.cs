using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.Order;
using VErp.Services.Manafacturing.Model.Outsource.Order.Part;
using VErp.Services.Manafacturing.Model.ProductionStep;

namespace VErp.Services.Manafacturing.Service.Outsource
{
    public interface IOutsourcePartOrderService
    {
        Task<long> CreateOutsourceOrderPart(OutsourcePartOrderInput req);
        Task<PageData<OutsourcePartOrderDetailInfo>> GetListOutsourceOrderPart(string keyword, int page, int size, long fromDate, long toDate, Clause filters = null);
        Task<bool> UpdateOutsourceOrderPart(long outsourceOrderId, OutsourcePartOrderInput req);
        Task<bool> DeleteOutsourceOrderPart(long outsourceOrderId);
        Task<OutsourcePartOrderOutput> GetOutsourceOrderPart(long outsourceOrderId);

        Task<IList<Model.Outsource.Order.OutsourceOrderMaterialsLSX>> GetMaterials(long outsourceOrderId);

        Task<bool> UpdateOutsourcePartOrderStatus(long outsourceStepOrderId);
    }
}
