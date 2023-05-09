using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using VErp.Infrastructure.AppSettings;
using VErp.Infrastructure.AppSettings.Model;
using System.Text.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace MigrateAndMappingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _appConfig;
        private readonly IConfigurationBuilder _builder;
        public LoginController(IConfiguration appConfig)
        {
            _appConfig = appConfig;
        }
        [Route("GetConfigs")]
        [HttpGet]
        public async Task<JObject> GetConfigs()
        {
            string json = System.IO.File.ReadAllText("AppServiceCustom.json");
            JObject data = JObject.Parse(json);
            return data;
        }
    }
}
