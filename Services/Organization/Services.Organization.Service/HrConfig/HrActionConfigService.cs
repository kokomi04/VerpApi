using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Organization.Service.HrConfig
{
    public interface IHrActionConfigService : IActionButtonConfigHelper
    {

    }

    public class HrActionConfigService : ActionButtonConfigHelperServiceAbstract, IHrActionConfigService
    {
        private readonly OrganizationDBContext _organizationDBContext;

        public HrActionConfigService(
            IMapper mapper,
            IActionButtonConfigHelperService actionButtonConfigHelperService,
            OrganizationDBContext organizationDBContext)
            : base(mapper, actionButtonConfigHelperService, EnumObjectType.HrType,"Hành chính nhân sự")
        {
            _organizationDBContext = organizationDBContext;
        }

        protected override async Task<string> GetObjectTitle(int objectId)
        {
            var info = await _organizationDBContext.HrType.FirstOrDefaultAsync(v => v.HrTypeId == objectId);
            if (info == null) throw new BadRequestException(HrErrorCode.HrTypeNotFound);
            return info.Title;
        }    
    }
}
