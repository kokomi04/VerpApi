using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Library;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErpApi.Controllers.System
{
    [Route("api/[controller]")]
    public class UtilityController : VErpBaseController
    {
        [HttpPost]
        public decimal Eval([FromBody] string expression)
        {
            return Utils.Eval(expression);
        }
    }
}