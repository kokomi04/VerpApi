using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Service.Config;

namespace VErpApi.Controllers.System
{

    [Route("api/dataConfig")]
    public class DataConfigController : VErpBaseController
    {
        private readonly IDataConfigService _dataConfigService;

        public DataConfigController(IDataConfigService dataConfigService)
        {
            _dataConfigService = dataConfigService;
        }

        [Route("")]
        [HttpGet]
        public Task<DataConfigModel> GetConfig()
        {
            return _dataConfigService.GetConfig();
        }


        [Route("")]
        [HttpPut]
        public Task<bool> UpdateConfig([FromBody] DataConfigModel req)
        {
            return _dataConfigService.UpdateConfig(req);
        }
    }
}