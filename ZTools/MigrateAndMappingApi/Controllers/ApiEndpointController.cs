using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MigrateAndMappingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiEndpointController : ControllerBase
    {
        public async Task<bool> SyncData()
        {

        }
    }
}