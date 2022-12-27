using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Accountancy.Service.Input
{
    public interface ICalcPeriodServiceBase
    {
        Task<PageData<CalcPeriodListModel>> GetList(EnumCalcPeriodType calcPeriodTypeId, string keyword, long? fromDate, long? toDate, int page, int? size);

        Task<CalcPeriodDetailModel> GetInfo(EnumCalcPeriodType calcPeriodTypeId, long calcPeriodId);

        Task<CalcPeriodView<TFilter, TOutput>> CalcPeriodInfo<TFilter, TOutput>(EnumCalcPeriodType calcPeriodTypeId, long calcPeriodId);

        Task<bool> Delete(EnumCalcPeriodType calcPeriodTypeId, long calcPeriodId);


        Task<long> Create(EnumCalcPeriodType calcPeriodTypeId, string title, string description, long? fromDate, long? toDate, IFilterHashData filterData, object data);
    }

    public interface ICalcPeriodPrivateService: ICalcPeriodServiceBase
    {

    }

    public interface ICalcPeriodPublicService : ICalcPeriodServiceBase
    {

    }
}
