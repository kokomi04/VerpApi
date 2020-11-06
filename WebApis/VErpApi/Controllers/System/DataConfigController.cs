using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Model.Dictionary;
using VErp.Services.Master.Service.Config;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Service.Dictionary;

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