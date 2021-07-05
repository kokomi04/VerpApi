using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Service;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IManufacturingHelperService
    {
        Task<IList<StepSimpleInfo>> GetStepByArrayId(int[] arrayId);
        Task<IList<StepSimpleInfo>> GetSteps();
        Task<bool> CopyProductionProcess(EnumContainerType containerTypeId, long fromContainerId, long toContainerId);
    }
    public class ManufacturingHelperService: IManufacturingHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;

        public ManufacturingHelperService(IHttpCrossService httpCrossService,
            IOptions<AppSetting> appSetting,
            ILogger<ManufacturingHelperService> logger)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
        }

        public async Task<bool> CopyProductionProcess(EnumContainerType containerTypeId, long fromContainerId, long toContainerId)
        {
            return await _httpCrossService.Post<bool>($"api/internal/InternalManufacturing/productionProcess/copy?containerTypeId={containerTypeId}&fromContainerId={fromContainerId}&toContainerId={toContainerId}", new {});
        }

        public async Task<IList<StepSimpleInfo>> GetStepByArrayId(int[] arrayId)
        {
            return await _httpCrossService.Post<IList<StepSimpleInfo>>($"api/internal/InternalManufacturing/steps/array", arrayId);
        }

        public async Task<IList<StepSimpleInfo>> GetSteps()
        {
            return await _httpCrossService.Get<IList<StepSimpleInfo>>($"api/internal/InternalManufacturing/steps");
        }
    }
}
