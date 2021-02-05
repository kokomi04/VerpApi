using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Accountancy.Service.Input
{
    public interface ICalcProductPriceService
    {
        Task<CalcProductPriceGetTableOutput> CalcProductPriceTable(CalcProductPriceGetTableInput req);
        Task<CalcProductOutputPriceModel> CalcProductOutputPrice(CalcProductOutputPriceInput req);
        Task<IList<NonCamelCaseDictionary>> GetWeightedAverageProductPrice(CalcProductPriceWeightedAverageInput req);

        Task<CalcProfitAndLossTableOutput> CalcProfitAndLoss(CalcProfitAndLossInput req);

        Task<PageData<CalcPeriodListModel>> CalcProfitAndLossPeriods(string keyword, long? fromDate, long? toDate, int page, int? size);

        Task<CalcProfitAndLossView> CalcProfitAndLossPeriodInfo(long calcPeriodId);
    }
}
