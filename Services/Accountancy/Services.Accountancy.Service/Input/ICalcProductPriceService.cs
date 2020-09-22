using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Accountancy.Service.Input
{
    public interface ICalcProductPriceService
    {
        Task<CalcProductPriceGetTableOutput> GetCalcProductPriceTable(CalcProductPriceGetTableInput req);

        Task<IList<NonCamelCaseDictionary>> GetWeightedAverageProductPrice(CalcProductPriceWeightedAverageInput req);
    }
}
