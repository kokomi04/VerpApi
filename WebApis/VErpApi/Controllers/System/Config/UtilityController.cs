﻿using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErpApi.Models;

namespace VErpApi.Controllers.System.Config
{
    [Route("api/[controller]")]
    public class UtilityController : VErpBaseController
    {
        private readonly HttpClient client;

        public UtilityController(HttpClient client)
        {
            this.client = client;
        }

        [HttpPost("Eval")]
        [GlobalApi]
        public decimal Eval([FromBody] string expression)
        {
            return EvalUtils.Eval(expression);
        }

        [HttpPost("HttpRequest")]
        [GlobalApi]
        public async Task<HttpResponseModel> HttpRequest([FromBody] HttpRequestProxyModel req)
        {
            if (req?.Url?.StartsWith("http") != true) throw new BadRequestException(GeneralCode.InvalidParams, "url must start with 'http'");

            var reqMessage = new HttpRequestMessage(new HttpMethod(req.Method), req.Url);
            if (req.Headers != null)
            {
                foreach (var h in req.Headers)
                {
                    reqMessage.Headers.Add(h.Key, h.Value);
                }
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            if (!string.IsNullOrWhiteSpace(req.Body))
                reqMessage.Content = new StringContent(req.Body, Encoding.UTF8);
            var response = await client.SendAsync(reqMessage);
            return new HttpResponseModel()
            {
                IsSuccessStatusCode = response.IsSuccessStatusCode,
                StatusCode = response.StatusCode,
                Headers = response.Headers.ToDictionary(h => h.Key, h =>
                {
                    return string.Join(", ", h.Value);
                }).ToNonCamelCaseDictionaryData(v => v.Key, v => v.Value),
                Body = await response.Content.ReadAsStringAsync()
            };
        }
    }
}