using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.Order;

namespace VErp.Services.Manafacturing.Service.Outsource
{
    public interface IOutsourcePropertyService
    {
        Task<long> Create(OutsourcePropertyOrderInput req);
        Task<bool> Delete(long outsourceOrderId);
        Task<PageData<OutsourcePropertyOrderList>> GetList(string keyword, int page, int size, string orderByFieldName, bool asc, long fromDate, long toDate, Clause filters = null);
        Task<OutsourcePropertyOrderInput> Info(long outsourceOrderId);
        Task<OutsourcePropertyOrderInput> GetInfoByPropertyCalcId(long propertyCalcId);

        Task<bool> Update(long outsourceOrderId, OutsourcePropertyOrderInput req);
    }
}