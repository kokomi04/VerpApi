using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;

namespace VErp.Services.Accountancy.Service.Input.Implement
{
    public class InputActionConfigService : ActionButtonConfigHelperServiceAbstract, IInputActionConfigService
    {
        private readonly AccountancyDBContext _accountancyDBContext;

        public InputActionConfigService(AccountancyDBContext accountancyDBContext
            , IMapper mapper
            , IActionButtonConfigHelperService actionButtonConfigHelperService
            ) : base(mapper, actionButtonConfigHelperService, EnumObjectType.InputType, "Chứng từ kế toán")
        {
            _accountancyDBContext = accountancyDBContext;
        }

        protected override async Task<string> GetObjectTitle(int objectId)
        {
            var info = await _accountancyDBContext.InputType.FirstOrDefaultAsync(v => v.InputTypeId == objectId);
            if (info == null) throw new BadRequestException(InputErrorCode.InputTypeNotFound);
            return info.Title;
        }
    }
}

