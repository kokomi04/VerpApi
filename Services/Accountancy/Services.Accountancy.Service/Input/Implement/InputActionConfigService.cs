using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Accountancy.Model.Data;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Facade;
using Verp.Resources.Accountancy.InputData;

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

