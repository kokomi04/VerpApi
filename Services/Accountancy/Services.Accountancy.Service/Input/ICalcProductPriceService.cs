using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Accountancy.Service.Input
{
    public interface ICalcProductPriceServiceBase
    {
        Task<CalcProductPriceGetTableOutput> CalcProductPriceTable(CalcProductPriceGetTableInput req);
        Task<CalcProductOutputPriceModel> CalcProductOutputPrice(CalcProductOutputPriceInput req);
        Task<IList<NonCamelCaseDictionary>> GetWeightedAverageProductPrice(CalcProductPriceInput req);
        Task<IList<NonCamelCaseDictionary>> GetProductPriceBuyLastest(CalcProductPriceInput req);
        Task<CalcProfitAndLossTableOutput> CalcProfitAndLoss(CalcProfitAndLossInput req);

    }

    public interface ICalcProductPricePrivateService: ICalcProductPriceServiceBase
    {

    }

    public interface ICalcProductPricePublicService : ICalcProductPriceServiceBase
    {

    }
}
