using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.General;

namespace VErp.Services.PurchaseOrder.Service.Voucher.Implement
{
    public class VoucherActionConfigService : ActionButtonConfigHelperServiceAbstract, IVoucherActionConfigService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;

        public VoucherActionConfigService(PurchaseOrderDBContext purchaseOrderDBContext
            , IMapper mapper
            , IActionButtonConfigHelperService actionButtonConfigHelperService
            ) : base(mapper, actionButtonConfigHelperService, EnumObjectType.VoucherType, "Chứng từ bán hàng")
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
        }

        protected override async Task<string> GetObjectTitle(int objectId)
        {
            var info = await _purchaseOrderDBContext.VoucherType.FirstOrDefaultAsync(v => v.VoucherTypeId == objectId);
            if (info == null) throw new BadRequestException(InputErrorCode.InputTypeNotFound);
            return info.Title;
        }
    }
}

