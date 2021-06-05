using GrpcProto.Protos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IBarcodeConfigHelperService
    {

        Task<PageData<BarcodeConfigListOutput>> GetList(string keyword, int page = 1, int size = 0);
    }


    public class BarcodeConfigHelperService : IBarcodeConfigHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        public BarcodeConfigHelperService(IHttpCrossService httpCrossService,
            IOptions<AppSetting> appSetting,
            ILogger<ProductHelperService> logger)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
        }
       
        public async Task<PageData<BarcodeConfigListOutput>> GetList(string keyword, int page = 1, int size = 0)
        {
            var queries = new
            {
                keyword,
                page,
                size
            };

            return await _httpCrossService.Get<PageData<BarcodeConfigListOutput>>($"api/internal/InternalBarcodeConfig", queries);
        }
    }
}
