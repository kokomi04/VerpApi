using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;

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

