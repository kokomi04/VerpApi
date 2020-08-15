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
        Task<IList<NonCamelCaseDictionary>> GetCalcProductPriceTable(CalcProductPriceGetTableInput req);
    }
}
