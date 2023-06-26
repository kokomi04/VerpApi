using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NPOI.POIFS.Crypt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionHandover;

namespace VErpApi.Controllers.Help
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelpController : VErpBaseController
    {
        private readonly HttpClient _httpClient;
        private readonly AppSetting _appSetting;

        public HelpController(HttpClient httpClient, IOptionsSnapshot<AppSetting> appSetting) 
        {
            _httpClient = httpClient;
            _appSetting = appSetting?.Value;
        }

        [HttpGet]
        public async Task<IActionResult> GetTokenViaHelpApi()
        {
            var secretKey = _appSetting.Configuration.ExternalHelpApiKey;

            _httpClient.BaseAddress = new Uri("http://localhost:5232");
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/verp");

            _httpClient.DefaultRequestHeaders.Add("SecretKey", secretKey);

            try
            {
                var response = await _httpClient.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.Unauthorized || !response.IsSuccessStatusCode)
                {
                    return Unauthorized();
                }

                IEnumerable<string> headerValues;
                if (response.Headers.TryGetValues("Token", out headerValues))
                {
                    throw new BadRequestException(GeneralCode.ItemNotFound, "Can't try to get token!");
                }

                return Ok(headerValues.FirstOrDefault());
            }
            catch
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Can't connect to Help API!");
            }
        }
    }
}
