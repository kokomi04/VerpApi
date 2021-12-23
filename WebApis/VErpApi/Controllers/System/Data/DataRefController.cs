using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Master.Model.Data;
using VErp.Services.Master.Service.Data;

namespace VErpApi.Controllers.System.Data
{
    [Route("api/DataRef")]
    public class DataRefController : VErpBaseController
    {
        private readonly IDataRefService _dataRefService;
        public DataRefController(IDataRefService dataRefService)
        {
            _dataRefService = dataRefService;
        }

        [HttpGet("")]
        [GlobalApi()]
        public Task<IList<DataRefModel>> GetDataRef([FromQuery] EnumObjectType objectTypeId, [FromQuery] long? id, [FromQuery] string code)
        {
            return _dataRefService.GetDataRef(objectTypeId, id, code);
        }

    }
}
