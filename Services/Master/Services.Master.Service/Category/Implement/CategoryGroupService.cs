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
using Verp.Resources.Master.Category;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Category;
using static Verp.Resources.Master.Category.CategoryGroupValidationMessage;

namespace VErp.Services.Master.Service.Category.Implement
{
    public class CategoryGroupService : ICategoryGroupService
    {
        private readonly ILogger _logger;
        private readonly AppSetting _appSetting;
        private readonly IMapper _mapper;
        private readonly MasterDBContext _masterContext;
        private readonly ObjectActivityLogFacade _categoryGroupActivityLog;

        public CategoryGroupService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            )
        {
            _logger = logger;
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _mapper = mapper;
            _categoryGroupActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.CategoryGroup);
        }

        public async Task<int> Add(CategoryGroupModel model)
        {
            var info = _mapper.Map<CategoryGroup>(model);
            _masterContext.CategoryGroup.Add(info);
            await _masterContext.SaveChangesAsync();

            await _categoryGroupActivityLog.LogBuilder(() => CategoryGroupActivityLogMessage.Create)
              .MessageResourceFormatDatas(info.CategoryGroupName)
              .ObjectId(info.CategoryGroupId)
              .JsonData(model.JsonSerialize())
              .CreateLog();

            return info.CategoryGroupId;
        }

        public async Task<bool> Delete(int categoryGroupId)
        {
            if (await _masterContext.Category.AnyAsync(c => c.CategoryGroupId == categoryGroupId))
            {
                throw CannotDeleteGroupWhichExistedChildren.BadRequest();
            }

            var info = await _masterContext.CategoryGroup.FirstOrDefaultAsync(c => c.CategoryGroupId == categoryGroupId);

            if (info == null)
            {
                throw GroupNotFound.BadRequest();
            }

            info.IsDeleted = true;

            await _masterContext.SaveChangesAsync();

            await _categoryGroupActivityLog.LogBuilder(() => CategoryGroupActivityLogMessage.Delete)
             .MessageResourceFormatDatas(info.CategoryGroupName)
             .ObjectId(info.CategoryGroupId)
             .JsonData(info.JsonSerialize())
             .CreateLog();

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
                throw GroupNotFound.BadRequest();
            }

            _mapper.Map(model, info);

            await _masterContext.SaveChangesAsync();


            await _categoryGroupActivityLog.LogBuilder(() => CategoryGroupActivityLogMessage.Update)
             .MessageResourceFormatDatas(info.CategoryGroupName)
             .ObjectId(info.CategoryGroupId)
             .JsonData(info.JsonSerialize())
             .CreateLog();

            return true;

        }
    }
}
