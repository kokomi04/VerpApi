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
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Service.Category.Implement
{
    public class CategoryActionConfigService : ActionButtonConfigHelperServiceAbstract, ICategoryActionConfigService
    {
        private readonly MasterDBContext _masterDBContext;

        public CategoryActionConfigService(MasterDBContext masterDBContext,
            IMapper mapper,
             IActionButtonConfigHelperService actionButtonConfigHelperService
            ) : base(mapper, actionButtonConfigHelperService, EnumObjectType.Category, "Danh mục")
        {
            _masterDBContext = masterDBContext;

        }

        protected override async Task<string> GetObjectTitle(int objectId)
        {
            var info = await _masterDBContext.Category.FirstOrDefaultAsync(v => v.CategoryId == objectId);
            if (info == null) throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            return info.Title;
        }
    }
}

