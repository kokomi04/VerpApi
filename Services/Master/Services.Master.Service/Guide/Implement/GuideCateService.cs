using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
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

namespace VErp.Services.Master.Service.Guide.Implement
{


    public class GuideCateService : IGuideCateService
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly ObjectActivityLogFacade _guideActivityLog;

        private readonly IMapper _mapper;

        public GuideCateService(MasterDBContext masterDBContext, IMapper mapper, IActivityLogService activityLogService)
        {
            _masterDBContext = masterDBContext;
            _mapper = mapper;
            _guideActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.GuideCate);
        }

        public async Task<int> Create(GuideCateModel model)
        {
            var entity = _mapper.Map<GuideCate>(model);

            _masterDBContext.GuideCate.Add(entity);
            await _masterDBContext.SaveChangesAsync();

            await _guideActivityLog.LogBuilder(() => GuideCateActivityLogMessage.Create)
             .MessageResourceFormatDatas(model.Title)
             .ObjectId(entity.GuideCateId)
             .JsonData(model.JsonSerialize())
             .CreateLog();

            return entity.GuideCateId;
        }

        public async Task<bool> Delete(int guideCateId)
        {
            var g = await _masterDBContext.GuideCate.FirstOrDefaultAsync(g => g.GuideCateId == guideCateId);
            if (g == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            g.IsDeleted = true;
            await _masterDBContext.SaveChangesAsync();

            await _guideActivityLog.LogBuilder(() => GuideCateActivityLogMessage.Delete)
            .MessageResourceFormatDatas(g.Title)
            .ObjectId(g.GuideCateId)
            .JsonData(g.JsonSerialize())
            .CreateLog();
            return true;
        }


        public async Task<GuideCateModel> Info(int guideCateId)
        {
            return await _masterDBContext.GuideCate.AsNoTracking()
                .ProjectTo<GuideCateModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(g => g.GuideCateId == guideCateId);
        }

        public async Task<IList<GuideCateModel>> GetList()
        {

            return await _masterDBContext.GuideCate.OrderBy(q => q.SortOrder)
                .ProjectTo<GuideCateModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

        }

        public async Task<bool> Update(int guideCateId, GuideCateModel model)
        {
            model.GuideCateId = guideCateId;
            if (model.ParentId == guideCateId)
            {
                throw GeneralCode.InvalidParams.BadRequest();
            }
            var g = await _masterDBContext.GuideCate.FirstOrDefaultAsync(g => g.GuideCateId == guideCateId);
            if (g == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            _mapper.Map(model, g);

            await _masterDBContext.SaveChangesAsync();

            await _guideActivityLog.LogBuilder(() => GuideCateActivityLogMessage.Update)
            .MessageResourceFormatDatas(model.Title)
            .ObjectId(guideCateId)
            .JsonData(model.JsonSerialize())
            .CreateLog();

            return true;
        }
    }
}
