using System.Threading.Tasks;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.Order;

namespace VErp.Services.Manafacturing.Service.Outsource
{
    public interface IOutsourceMaterialService
    {
        Task<long> Create(OutsourceStepOrderInput req);
        Task<bool> Delete(long outsourceOrderId);
        Task<PageData<OutsourceMaterialOrderList>> GetList(string keyword, int page, int size, string orderByFieldName, bool asc, long fromDate, long toDate, Clause filters = null);
        Task<OutsourceStepOrderOutput> Info(long outsourceOrderId);
        Task<bool> Update(long outsourceOrderId, OutsourceStepOrderOutput req);
    }
}