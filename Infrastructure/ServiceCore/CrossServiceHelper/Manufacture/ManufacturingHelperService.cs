﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface.Manufacturing;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Service;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper.Manufacture
{
    public interface IManufacturingHelperService
    {
        Task<IList<StepSimpleInfo>> GetStepByArrayId(int[] arrayId);
        Task<IList<StepSimpleInfo>> GetSteps();
        Task<bool> CopyProductionProcess(EnumContainerType containerTypeId, long fromContainerId, long toContainerId);
        Task<bool> UpdateOutsourcePartRequestStatus(long[] outsourcePartRequestId);
        Task<bool> UpdateOutsourceStepRequestStatus(long[] outsourceStepRequestId);
    }
    public class ManufacturingHelperService : IManufacturingHelperService
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
            return await _httpCrossService.Post<bool>($"api/internal/InternalManufacturing/productionProcess/copy?containerTypeId={containerTypeId}&fromContainerId={fromContainerId}&toContainerId={toContainerId}", new { });
        }

        public async Task<IList<StepSimpleInfo>> GetStepByArrayId(int[] arrayId)
        {
            return await _httpCrossService.Post<IList<StepSimpleInfo>>($"api/internal/InternalManufacturing/steps/array", arrayId);
        }

        public async Task<IList<StepSimpleInfo>> GetSteps()
        {
            return await _httpCrossService.Get<IList<StepSimpleInfo>>($"api/internal/InternalManufacturing/steps");
        }

        public async Task<bool> UpdateOutsourcePartRequestStatus(long[] outsourcePartRequestId)
        {
            return await _httpCrossService.Put<bool>($"api/internal/InternalManufacturing/outsourceRequest/Part/Status", outsourcePartRequestId);
        }

        public async Task<bool> UpdateOutsourceStepRequestStatus(long[] outsourceStepRequestId)
        {
            return await _httpCrossService.Put<bool>($"api/internal/InternalManufacturing/outsourceRequest/Step/Status", outsourceStepRequestId);
        }
    }
}
