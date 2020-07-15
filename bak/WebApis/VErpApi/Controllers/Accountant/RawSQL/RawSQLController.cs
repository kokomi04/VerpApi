using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Config;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Accountant.Model.Input;
using System.Collections.Generic;
using VErp.Commons.Library;
using System;
using Newtonsoft.Json;
using VErp.Services.Accountant.Service.RawSQLQuery;

namespace VErpApi.Controllers.Accountant
{
    [Route("api/rawsql")]

    public class RawSQLController : VErpBaseController
    {
        private readonly IRawSQLQueryService _rawSQLQueryService;
        private readonly IFileService _fileService;
        public RawSQLController(IRawSQLQueryService rawSQLQueryService
            , IFileService fileService
            )
        {
            _fileService = fileService;
            _rawSQLQueryService = rawSQLQueryService;
        }

        [HttpGet]
        [Route("fromsqlraw")]
        public async Task<ServiceResult<List<List<Dictionary<string, string>>>>> FromSQLRaw([FromQuery] string query)
        {
            return await _rawSQLQueryService.FromSQLRaw(query);
        }
    }
}