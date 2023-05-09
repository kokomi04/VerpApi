using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace MigrateAndMappingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [Route("GetConfigs")]
        [HttpGet]
        public async Task<JObject> GetConfig()
        {
            JObject data = JObject.Parse(System.IO.File.ReadAllText("AppServiceCustom.json"));
            return data;
        }
    }
}