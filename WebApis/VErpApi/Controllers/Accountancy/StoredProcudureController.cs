using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Accountancy.Service.StoredProcedure;

namespace VErpApi.Controllers.Accountancy
{
    [Route("api/Accountancy/[controller]")]
    public class StoredProcudureController : VErpBaseController
    {
        private readonly IStoredProcedureService _storedProcedureService;

        public StoredProcudureController(IStoredProcedureService storedProcedureService)
        {
            _storedProcedureService = storedProcedureService;
        }

        [HttpGet("List")]
        [AllowAnonymous]
        public async Task<NonCamelCaseDictionary<IList<NonCamelCaseDictionary>>> GetList()
        {
            return await _storedProcedureService.GetList();
        }
    }
}
