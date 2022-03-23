using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Services.Organization.Model.TimeKeeping;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Organization.Service.TimeKeeping
{
    public interface IWorkScheduleMarkService
    {
        Task<int> AddWorkScheduleMark(WorkScheduleMarkModel model);
        Task<bool> DeleteWorkScheduleMark(int workScheduleId);
        Task<IList<WorkScheduleMarkModel>> GetListWorkScheduleMark(int? employeeId);
        Task<WorkScheduleMarkModel> GetWorkScheduleMark(int workScheduleId);
        Task<bool> UpdateWorkScheduleMark(int workScheduleId, WorkScheduleMarkModel model);
    }

    public class WorkScheduleMarkService : IWorkScheduleMarkService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _workScheduleActivityLog;

        public WorkScheduleMarkService(OrganizationDBContext organizationDBContext, IMapper mapper, IActivityLogService activityLogService)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
            // _workScheduleActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.WorkScheduleMark);
        }

        public async Task<int> AddWorkScheduleMark(WorkScheduleMarkModel model)
        {
            var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {

                var lastMark = await _organizationDBContext.WorkScheduleMark.Where(x => x.EmployeeId == model.EmployeeId)
                                                                            .OrderBy(x => x.WorkScheduleMarkId)
                                                                            .LastOrDefaultAsync();

                var entity = _mapper.Map<WorkScheduleMark>(model);

                await _organizationDBContext.WorkScheduleMark.AddAsync(entity);

                if(lastMark != null)
                {
                    lastMark.ExpiryDate = entity.BeginDate.AddDays(-1);
                }

                await _organizationDBContext.SaveChangesAsync();
                await trans.CommitAsync();

                // await _workScheduleActivityLog.LogBuilder(() => WorkScheduleMarkActivityLogMessage.UpdateWorkScheduleMark)
                //         .MessageResourceFormatDatas(workScheduleMark.WorkScheduleMarkTitle)
                //         .ObjectId(workScheduleMark.WorkScheduleMarkId)
                //         .JsonData(model.JsonSerialize())
                //         .CreateLog();

                return entity.WorkScheduleMarkId;

            }
            catch (System.Exception ex)
            {
                await trans.RollbackAsync();
                throw ex;
            }
        }

        public async Task<bool> UpdateWorkScheduleMark(int workScheduleId, WorkScheduleMarkModel model)
        {
            var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                var workScheduleMark = await _organizationDBContext.WorkScheduleMark.FirstOrDefaultAsync(x => x.WorkScheduleMarkId == workScheduleId);
                if (workScheduleMark == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                model.WorkScheduleMarkId = workScheduleId;

                _mapper.Map(model, workScheduleMark);

                await _organizationDBContext.SaveChangesAsync();

                // await _workScheduleActivityLog.LogBuilder(() => WorkScheduleMarkActivityLogMessage.UpdateWorkScheduleMark)
                //          .MessageResourceFormatDatas(workScheduleMark.WorkScheduleMarkTitle)
                //          .ObjectId(workScheduleMark.WorkScheduleMarkId)
                //          .JsonData(model.JsonSerialize())
                //          .CreateLog();

                await trans.CommitAsync();
                return true;
            }
            catch (System.Exception ex)
            {
                await trans.RollbackAsync();
                throw ex;
            }
        }

        public async Task<bool> DeleteWorkScheduleMark(int workScheduleId)
        {
            var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                var workSchedule = await _organizationDBContext.WorkScheduleMark.FirstOrDefaultAsync(x => x.WorkScheduleMarkId == workScheduleId);
                if (workSchedule == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                workSchedule.IsDeleted = true;

                await _organizationDBContext.SaveChangesAsync();

                // await _workScheduleActivityLog.LogBuilder(() => WorkScheduleMarkActivityLogMessage.DeleteWorkScheduleMark)
                //          .MessageResourceFormatDatas(workSchedule.WorkScheduleMarkTitle)
                //          .ObjectId(workSchedule.WorkScheduleMarkId)
                //          .CreateLog();

                await trans.CommitAsync();
                return true;
            }
            catch (System.Exception ex)
            {
                await trans.RollbackAsync();
                throw ex;
            }

        }

        public async Task<WorkScheduleMarkModel> GetWorkScheduleMark(int workScheduleId)
        {
            var workSchedule = await _organizationDBContext.WorkScheduleMark.FirstOrDefaultAsync(x => x.WorkScheduleMarkId == workScheduleId);
            if (workSchedule == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            var result = _mapper.Map<WorkScheduleMarkModel>(workSchedule);
            return result;
        }

        public async Task<IList<WorkScheduleMarkModel>> GetListWorkScheduleMark(int? employeeId)
        {
            var query = _organizationDBContext.WorkScheduleMark.AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(x => x.EmployeeId == employeeId);

            return await query
            .AsNoTracking()
            .ProjectTo<WorkScheduleMarkModel>(_mapper.ConfigurationProvider)
            .ToListAsync();
        }
    }
}