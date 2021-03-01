using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Data;

namespace VErp.Services.Accountancy.Service.Input
{
    public interface ICalcPeriodService
    {
        Task<PageData<CalcPeriodListModel>> GetList(EnumCalcPeriodType calcPeriodTypeId, string keyword, long? fromDate, long? toDate, int page, int? size);

        Task<CalcPeriodDetailModel> GetInfo(EnumCalcPeriodType calcPeriodTypeId, long calcPeriodId);

        Task<bool> Delete(EnumCalcPeriodType calcPeriodTypeId, long calcPeriodId);


        Task<long> Create(EnumCalcPeriodType calcPeriodTypeId, string title, string description, long? fromDate, long? toDate, IFilterHashData filterData, object data);
    }
}
