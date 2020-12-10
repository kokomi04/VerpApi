using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Commons.GlobalObject;
using VErp.Commons.Enums.Manafacturing;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IProductionScheduleHelperService
    {
        Task<bool> UpdateProductionScheduleStatus(long scheduleTurnId, EnumScheduleStatus status);
    }
    public class ProductionScheduleHelperService : IProductionScheduleHelperService
    {
        private readonly IHttpCrossService _httpCrossService;

        public ProductionScheduleHelperService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }

        public async Task<bool> UpdateProductionScheduleStatus(long scheduleTurnId, EnumScheduleStatus status)
        {
            return await _httpCrossService.Put<bool>($"api/internal/InternalProductionSchedule/{scheduleTurnId}/status/{status}?isManual=true", null);
        }
    }
}
