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
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Dictionary;
using VErp.Services.Master.Service.Resources.ReuseContent;

namespace VErp.Services.Master.Service.Dictionay.Implement
{
    public class ReuseContentService : IReuseContentService
    {
        private readonly MasterDBContext _masterContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _reuseContentActivityLog;

        public ReuseContentService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<UnitService> logger
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService
            , IMapper mapper
            )
        {
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _currentContextService = currentContextService;
            _mapper = mapper;
            _reuseContentActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ReuseContent);
        }

        public async Task<IList<ReuseContentModel>> GetList(string key)
        {
            return await _masterContext.ReuseContent.Where(c => c.Key == key).ProjectTo<ReuseContentModel>(_mapper.ConfigurationProvider).ToListAsync();
        }


        public async Task<long> Create(ReuseContentModel model)
        {
            var entity = _mapper.Map<ReuseContent>(model);
            await _masterContext.ReuseContent.AddAsync(entity);
            await _masterContext.SaveChangesAsync();

            await _reuseContentActivityLog.LogBuilder(() => ReuseContentActivityMessage.Create)
                .MessageResourceFormatDatas(entity.Key, entity.Content)
                .ObjectId(entity.ReuseContentId)
                .JsonData(model.JsonSerialize())
                .CreateLog();

            return entity.ReuseContentId;
        }


        public async Task<ReuseContentModel> Info(long reuseContentId)
        {
            var model = await _masterContext.ReuseContent.ProjectTo<ReuseContentModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync(c => c.ReuseContentId == reuseContentId);
            if (model == null) throw GeneralCode.ItemNotFound.BadRequest();

            return model;
        }


        public async Task<bool> Update(long reuseContentId, ReuseContentModel model)
        {
            var entity = await _masterContext.ReuseContent.FirstOrDefaultAsync(c => c.ReuseContentId == reuseContentId);
            if (entity == null) throw GeneralCode.ItemNotFound.BadRequest();

            _mapper.Map(model, entity);

            await _masterContext.SaveChangesAsync();

            await _reuseContentActivityLog.LogBuilder(() => ReuseContentActivityMessage.Update)
                .MessageResourceFormatDatas(entity.Key, entity.Content)
                .ObjectId(entity.ReuseContentId)
                .JsonData(model.JsonSerialize())
                .CreateLog();

            return true;
        }

        public async Task<bool> Delete(long reuseContentId)
        {
            var entity = await _masterContext.ReuseContent.FirstOrDefaultAsync(c => c.ReuseContentId == reuseContentId);
            if (entity == null) throw GeneralCode.ItemNotFound.BadRequest();

            entity.IsDeleted = true;
            await _masterContext.SaveChangesAsync();

            await _reuseContentActivityLog.LogBuilder(() => ReuseContentActivityMessage.Delete)
                .MessageResourceFormatDatas(entity.Key, entity.Content)
                .ObjectId(entity.ReuseContentId)
                .JsonData(entity.JsonSerialize())
                .CreateLog();

            return true;
        }
    }
}
