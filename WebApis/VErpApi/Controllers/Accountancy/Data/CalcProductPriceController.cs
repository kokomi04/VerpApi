using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Accountancy.Service.Input;
using System.IO;

namespace VErpApi.Controllers.Accountancy.Data
{

    [Route("api/accountancy/data/CalcProductPrice")]

    public class CalcProductPriceController : VErpBaseController
    {
        private readonly ICalcProductPriceService _calcProductPriceService;
        public CalcProductPriceController(ICalcProductPriceService calcProductPriceService)
        {
            _calcProductPriceService = calcProductPriceService;
        }


       
        [HttpPost]
        [VErpAction(EnumAction.View)]
        [GlobalApi]
        [Route("GetCalcProductPriceTable")]
        public async Task<IList<NonCamelCaseDictionary>> GetCalcProductPriceTable([FromBody] CalcProductPriceGetTableInput req)
        {
            return await _calcProductPriceService.GetCalcProductPriceTable(req).ConfigureAwait(true);
        }       
    }
}
