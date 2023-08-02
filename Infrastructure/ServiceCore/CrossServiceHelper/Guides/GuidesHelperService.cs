using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Model.Guides;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IGuidesHelperService
    {
        Task<GuideTokenResponse> GetToken();
    }

    public class GuidesHelperService : IGuidesHelperService
    {
        private readonly IHttpGuideCrossService _httpGuideCrossService;

        public GuidesHelperService(IHttpGuideCrossService httpGuideCrossService)
        {
            _httpGuideCrossService = httpGuideCrossService;
        }
        public async Task<GuideTokenResponse> GetToken()
        {
            return await _httpGuideCrossService.Get<GuideTokenResponse>($"api/verp/token");
        }
    }
}
