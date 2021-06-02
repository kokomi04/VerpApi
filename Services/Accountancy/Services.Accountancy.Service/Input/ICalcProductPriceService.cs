﻿using System;
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
        Task<IList<NonCamelCaseDictionary>> GetWeightedAverageProductPrice(CalcProductPriceInput req);
        Task<IList<NonCamelCaseDictionary>> GetProductPriceBuyLastest(CalcProductPriceInput req);
        Task<CalcProfitAndLossTableOutput> CalcProfitAndLoss(CalcProfitAndLossInput req);
       
    }
}
