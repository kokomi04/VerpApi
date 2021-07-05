using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Category;

namespace VErp.Services.Master.Service.Category.Implement
{
    public class CategoryGroupService : ICategoryGroupService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly AppSetting _appSetting;
        private readonly IMapper _mapper;
        private readonly MasterDBContext _masterContext;

        public CategoryGroupService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            )
        {
            _logger = logger;
            _activityLogService = activityLogService;
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _mapper = mapper;
        }

        public async Task<int> Add(CategoryGroupModel model)
        {
            var info = _mapper.Map<CategoryGroup>(model);
            _masterContext.CategoryGroup.Add(info);
            await _masterContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.CategoryGroup, info.CategoryGroupId, $"Tạo nhóm danh mục  {info.CategoryGroupName}", model.JsonSerialize());

            return info.CategoryGroupId;
        }

        public async Task<bool> Delete(int categoryGroupId)
        {
            if (await _masterContext.Category.AnyAsync(c => c.CategoryGroupId == categoryGroupId))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Không thể xóa nhóm danh mục còn danh mục con!");
            }

            var info = await _masterContext.CategoryGroup.FirstOrDefaultAsync(c => c.CategoryGroupId == categoryGroupId);

            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy nhóm danh mục");
            }

            info.IsDeleted = true;

            await _masterContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.CategoryGroup, info.CategoryGroupId, $"Xóa nhóm danh mục {info.CategoryGroupName}", _mapper.Map<CategoryGroupModel>(info).JsonSerialize());

            return true;
        }

        public async Task<IList<CategoryGroupModel>> GetList()
        {
            return await _masterContext.CategoryGroup.ProjectTo<CategoryGroupModel>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<bool> Update(int categoryGroupId, CategoryGroupModel model)
        {
            var info = await _masterContext.CategoryGroup.FirstOrDefaultAsync(c => c.CategoryGroupId == categoryGroupId);

            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy nhóm danh mục");
            }

            _mapper.Map(model, info);

            await _masterContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.CategoryGroup, info.CategoryGroupId, $"Cập nhập nhóm danh mục {info.CategoryGroupName}", _mapper.Map<CategoryGroupModel>(info).JsonSerialize());

            return true;

        }
    }
}
