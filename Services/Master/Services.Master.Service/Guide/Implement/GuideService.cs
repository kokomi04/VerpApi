using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.Master.Guide;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Guide;
using GuideEntity = VErp.Infrastructure.EF.MasterDB.Guide;

namespace VErp.Services.Master.Service.Guide.Implement
{
    public class GuideService : IGuideService
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly ObjectActivityLogFacade _guideActivityLog;

        private readonly IMapper _mapper;

        public GuideService(MasterDBContext masterDBContext, IMapper mapper, IActivityLogService activityLogService)
        {
            _masterDBContext = masterDBContext;           
            _mapper = mapper;
            _guideActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Guide);
        }

        public async Task<int> Create(GuideModel model)
        {
            var entity = _mapper.Map<GuideEntity>(model);

            _masterDBContext.Guide.Add(entity);
            await _masterDBContext.SaveChangesAsync();

            await _guideActivityLog.LogBuilder(() => GuideActivityLogMessage.Create)
             .MessageResourceFormatDatas(model.Title)
             .ObjectId(entity.GuideId)
             .JsonData(model.JsonSerialize())
             .CreateLog();

            return entity.GuideId;
        }

        public async Task<bool> Deleted(int guideId)
        {
            var g = await _masterDBContext.Guide.FirstOrDefaultAsync(g => g.GuideId == guideId);
            if (g == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            g.IsDeleted = true;
            await _masterDBContext.SaveChangesAsync();

            await _guideActivityLog.LogBuilder(() => GuideActivityLogMessage.Delete)
            .MessageResourceFormatDatas(g.Title)
            .ObjectId(g.GuideId)
            .JsonData(g.JsonSerialize())
            .CreateLog();
            return true;
        }

        public async Task<IList<GuideModel>> GetGuidesByCode(string guideCode)
        {
            return await _masterDBContext.Guide.AsNoTracking()
                .Where(g => g.GuideCode.Equals(guideCode))
                .OrderBy(x => x.SortOrder)
                .ProjectTo<GuideModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<GuideModel> GetGuideById(int guideId)
        {
            return await _masterDBContext.Guide.AsNoTracking()
                .ProjectTo<GuideModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(g => g.GuideId == guideId);
        }

        public async Task<PageData<GuideModelOutput>> GetList(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            
            var query = _masterDBContext.Guide.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.GuideCode.Contains(keyword) || x.Title.Contains(keyword));
            }
            var total = await query.CountAsync();
            IList<GuideModelOutput> pagedData = null;

            if (size > 0)
            {
                pagedData = await query.OrderBy(q => q.GuideCode).ThenBy(q=>q.SortOrder)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ProjectTo<GuideModelOutput>(_mapper.ConfigurationProvider).ToListAsync();
            }
            else
            {
                pagedData = await query.OrderBy(q => q.GuideCode).ThenBy(q => q.SortOrder)
                    .ProjectTo<GuideModelOutput>(_mapper.ConfigurationProvider)
                    .ToListAsync();
            }

            return (pagedData, total);
        }

        public async Task<bool> Update(int guideId, GuideModel model)
        {
            var g = await _masterDBContext.Guide.FirstOrDefaultAsync(g => g.GuideId == guideId);
            if (g == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            _mapper.Map(model, g);

            await _masterDBContext.SaveChangesAsync();

            await _guideActivityLog.LogBuilder(() => GuideActivityLogMessage.Update)
            .MessageResourceFormatDatas(model.Title)
            .ObjectId(guideId)
            .JsonData(model.JsonSerialize())
            .CreateLog();

            return true;
        }
    }
}
