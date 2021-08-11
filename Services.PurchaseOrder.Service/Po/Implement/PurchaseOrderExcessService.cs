using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VErp.Commons.Enums.ErrorCodes.PO;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model;

namespace VErp.Services.PurchaseOrder.Service.Implement
{
    public class PurchaseOrderExcessService : IPurchaseOrderExcessService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;

        public PurchaseOrderExcessService(
            PurchaseOrderDBContext purchaseOrderDBContext,
            IOptions<AppSetting> appSetting,
            ILogger<PurchaseOrderExcessService> logger,
            IActivityLogService activityLogService,
            IMapper mapper)
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        public async Task<bool> UpdatePurchaseOrderExcess(long purchaseOrderExcessId, PurchaseOrderExcessModel model)
        {
            var excess = await _purchaseOrderDBContext.PurchaseOrderExcess.FirstOrDefaultAsync(x => x.PurchaseOrderExcessId == purchaseOrderExcessId);
            if (excess == null)
                throw new BadRequestException(PurchaseOrderErrorCode.ExcessNotFound);

            _mapper.Map(model, excess);
            _purchaseOrderDBContext.SaveChanges();
            return true;
        }
    }
}