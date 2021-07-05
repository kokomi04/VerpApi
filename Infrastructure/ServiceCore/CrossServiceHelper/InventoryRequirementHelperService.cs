using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IInventoryRequirementHelperService
    {
        Task<long> AddInventoryRequirement(EnumInventoryType inventoryType, InventoryRequirementSimpleModel req);
    }
    public class InventoryRequirementHelperService: IInventoryRequirementHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        public InventoryRequirementHelperService(IHttpCrossService httpCrossService, IOptions<AppSetting> appSetting, ILogger<ProductHelperService> logger)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
        }

        public async Task<long> AddInventoryRequirement(EnumInventoryType inventoryType, InventoryRequirementSimpleModel req)
        {
            return await _httpCrossService.Post<long>($"api/internal/InternalInventoryRequirement/{inventoryType}", req);
        }
    }
}
