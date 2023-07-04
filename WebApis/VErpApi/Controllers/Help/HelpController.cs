using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json;
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
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model.Guides;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionHandover;

namespace VErpApi.Controllers.Help
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelpController : VErpBaseController
    {
        private readonly IGuidesHelperService _guidesHelper;

        public HelpController(IGuidesHelperService guidesHelper)
        {
            _guidesHelper = guidesHelper;
        }

        [HttpGet]
        [Route("Token")]
        public async Task<GuideTokenResponse> GetTokenViaHelpApi()
        {
            try
            {
                return await _guidesHelper.GetToken();
            }
            catch
            {
                throw new BadRequestException(GeneralCode.InternalError, "Can't get token from Help API!");
            }
        }
    }
}
