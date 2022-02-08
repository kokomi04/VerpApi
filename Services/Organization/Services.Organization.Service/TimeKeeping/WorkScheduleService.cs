using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Services.Organization.Model.TimeKeeping;
using Verp.Resources.Organization.TimeKeeping;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Organization.Service.TimeKeeping
{
    public interface IWorkScheduleService
    {
        Task<int> AddWorkSchedule(WorkScheduleModel model);
        Task<bool> DeleteWorkSchedule(int workScheduleId);
        Task<IList<WorkScheduleModel>> GetListWorkSchedule();
        Task<WorkScheduleModel> GetWorkSchedule(int workScheduleId);
        Task<bool> UpdateWorkSchedule(int workScheduleId, WorkScheduleModel model);
    }

    public class WorkScheduleService : IWorkScheduleService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _workScheduleActivityLog;

        public WorkScheduleService(OrganizationDBContext organizationDBContext, IMapper mapper, IActivityLogService activityLogService)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
            _workScheduleActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.WorkSchedule);
        }

        public async Task<int> AddWorkSchedule(WorkScheduleModel model)
        {
            var entity = _mapper.Map<WorkSchedule>(model);

            await _organizationDBContext.WorkSchedule.AddAsync(entity);
            await _organizationDBContext.SaveChangesAsync();

            await _workScheduleActivityLog.LogBuilder(() => WorkScheduleActivityLogMessage.CreateWorkSchedule)
                      .MessageResourceFormatDatas(entity.WorkScheduleTitle)
                      .ObjectId(entity.WorkScheduleId)
                      .JsonData(model.JsonSerialize())
                      .CreateLog();

            return entity.WorkScheduleId;
        }

        public async Task<bool> UpdateWorkSchedule(int workScheduleId, WorkScheduleModel model)
        {
            var workSchedule = await _organizationDBContext.WorkSchedule.FirstOrDefaultAsync(x => x.WorkScheduleId == workScheduleId);
            if (workSchedule == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            model.WorkScheduleId = workScheduleId;

            _mapper.Map(model, workSchedule);

            await _organizationDBContext.SaveChangesAsync();

            await _workScheduleActivityLog.LogBuilder(() => WorkScheduleActivityLogMessage.UpdateWorkSchedule)
                     .MessageResourceFormatDatas(workSchedule.WorkScheduleTitle)
                     .ObjectId(workSchedule.WorkScheduleId)
                     .JsonData(model.JsonSerialize())
                     .CreateLog();

            return true;
        }

        public async Task<bool> DeleteWorkSchedule(int workScheduleId)
        {
            var workSchedule = await _organizationDBContext.WorkSchedule.FirstOrDefaultAsync(x => x.WorkScheduleId == workScheduleId);
            if (workSchedule == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            workSchedule.IsDeleted = true;

            await _organizationDBContext.SaveChangesAsync();

            await _workScheduleActivityLog.LogBuilder(() => WorkScheduleActivityLogMessage.DeleteWorkSchedule)
                     .MessageResourceFormatDatas(workSchedule.WorkScheduleTitle)
                     .ObjectId(workSchedule.WorkScheduleId)
                     .CreateLog();

            return true;
        }

        public async Task<WorkScheduleModel> GetWorkSchedule(int workScheduleId)
        {
            var workSchedule = await _organizationDBContext.WorkSchedule.FirstOrDefaultAsync(x => x.WorkScheduleId == workScheduleId);
            if (workSchedule == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            return _mapper.Map<WorkScheduleModel>(workSchedule);
        }

        public async Task<IList<WorkScheduleModel>> GetListWorkSchedule()
        {
            var query = _organizationDBContext.WorkSchedule.AsNoTracking();

            return await query
            .ProjectTo<WorkScheduleModel>(_mapper.ConfigurationProvider)
            .ToListAsync();
        }
    }
}