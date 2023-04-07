using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.Organization.Salary;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Organization.Salary;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary
{
    public class SalaryPeriodGroupService : ISalaryPeriodGroupService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _salaryPeriodActivityLog;
        private readonly ISalaryPeriodService _salaryPeriodService;
        private readonly ISalaryGroupService _salaryGroupService;

        public SalaryPeriodGroupService(OrganizationDBContext organizationDBContext, ICurrentContextService currentContextService, IMapper mapper, IActivityLogService activityLogService, ISalaryPeriodService salaryPeriodService, ISalaryGroupService salaryGroupService)
        {
            _organizationDBContext = organizationDBContext;
            _currentContextService = currentContextService;
            _mapper = mapper;
            _salaryPeriodActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.SalaryPeriod);
            _salaryPeriodService = salaryPeriodService;
            _salaryGroupService = salaryGroupService;
        }

        public async Task<bool> Censor(long salaryPeriodGroupId, bool isSuccess)
        {
            var info = await _organizationDBContext.SalaryPeriodGroup.FirstOrDefaultAsync(s => s.SalaryPeriodGroupId == salaryPeriodGroupId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var groupInfo = await _salaryGroupService.GetInfo(info.SalaryGroupId);

            var period = await _salaryPeriodService.GetInfo(info.SalaryPeriodId);

            info.SalaryPeriodCensorStatusId = (int)(isSuccess ? EnumSalaryPeriodCensorStatus.CensorApproved : EnumSalaryPeriodCensorStatus.CensorRejected);
            info.CensorByUserId = _currentContextService.UserId;
            info.CensorDatetimeUtc = DateTime.UtcNow;

            await _organizationDBContext.SaveChangesAsync();

            ObjectActivityLogModelBuilder<string> logBuilder;
            if (isSuccess)
                logBuilder = _salaryPeriodActivityLog.LogBuilder(() => SalaryPeriodGroupActivityLogMessage.CensorApproved);
            else
                logBuilder = _salaryPeriodActivityLog.LogBuilder(() => SalaryPeriodGroupActivityLogMessage.CensorRejected);

            await logBuilder
                .MessageResourceFormatDatas(period.Month, period.Year, groupInfo.Title)
                .ObjectId(info.SalaryPeriodGroupId)
                .JsonData(info.JsonSerialize())
                .CreateLog();

            return true;
        }

        public async Task<bool> Check(long salaryPeriodGroupId, bool isSuccess)
        {
            var info = await _organizationDBContext.SalaryPeriodGroup.FirstOrDefaultAsync(s => s.SalaryPeriodGroupId == salaryPeriodGroupId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var groupInfo = await _salaryGroupService.GetInfo(info.SalaryGroupId);

            var period = await _salaryPeriodService.GetInfo(info.SalaryPeriodId);

            info.SalaryPeriodCensorStatusId = (int)(isSuccess ? EnumSalaryPeriodCensorStatus.CheckedAccepted : EnumSalaryPeriodCensorStatus.CheckedRejected);
            info.CheckedByUserId = _currentContextService.UserId;
            info.CheckedDatetimeUtc = DateTime.UtcNow;

            await _organizationDBContext.SaveChangesAsync();

            ObjectActivityLogModelBuilder<string> logBuilder;
            if (isSuccess)
                logBuilder = _salaryPeriodActivityLog.LogBuilder(() => SalaryPeriodGroupActivityLogMessage.CheckAccepted);
            else
                logBuilder = _salaryPeriodActivityLog.LogBuilder(() => SalaryPeriodGroupActivityLogMessage.CensorRejected);

            await logBuilder
                .MessageResourceFormatDatas(period.Month, period.Year, groupInfo.Title)
                .ObjectId(info.SalaryPeriodGroupId)
                .JsonData(info.JsonSerialize())
                .CreateLog();

            return true;
        }

        public async Task<IList<SalaryPeriodGroupModel>> GetList(int salaryPeriodId)
        {
            return await _organizationDBContext.SalaryPeriodGroup.ProjectTo<SalaryPeriodGroupModel>(_mapper.ConfigurationProvider)
                .Where(s => s.SalaryPeriodId == salaryPeriodId)
                .ToListAsync();
        }

        public async Task<SalaryPeriodGroupModel> GetInfo(int salaryPeriodId, int salaryGroupId)
        {
            return await _organizationDBContext.SalaryPeriodGroup.ProjectTo<SalaryPeriodGroupModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(s => s.SalaryPeriodId == salaryPeriodId && s.SalaryGroupId == salaryGroupId);
        }

        public async Task<SalaryPeriodGroupModel> GetInfo(long salaryPeriodGroupId)
        {
            return await _organizationDBContext.SalaryPeriodGroup.ProjectTo<SalaryPeriodGroupModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(s => s.SalaryPeriodGroupId == salaryPeriodGroupId);
        }
        

        public async Task<int> Create(SalaryPeriodGroupModel model)
        {
            if (await _organizationDBContext.SalaryPeriodGroup.AnyAsync(s => s.SalaryPeriodId == model.SalaryPeriodId && s.SalaryGroupId == model.SalaryGroupId))
            {
                throw GeneralCode.InvalidParams.BadRequest("Bảng tính lương cho kỳ lương đã tồn tại!");
            }

            var groupInfo = await _salaryGroupService.GetInfo(model.SalaryGroupId);
            if (groupInfo == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest("Loại bảng lương không tồn tại");
            }
            var period = await _salaryPeriodService.GetInfo(model.SalaryPeriodId);
            if (period == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest("Kỳ tính lương không tồn tại");
            }
            var info = _mapper.Map<SalaryPeriodGroup>(model);
            info.SalaryPeriodCensorStatusId = (int)EnumSalaryPeriodCensorStatus.New;
            await _organizationDBContext.SalaryPeriodGroup.AddAsync(info);
            await _organizationDBContext.SaveChangesAsync();
            await _salaryPeriodActivityLog.LogBuilder(() => SalaryPeriodGroupActivityLogMessage.Create)
                .MessageResourceFormatDatas(period.Month, period.Year, groupInfo.Title)
                .ObjectId(info.SalaryPeriodGroupId)
                .JsonData(model.JsonSerialize())
                .CreateLog();
            return info.SalaryPeriodId;
        }

        public async Task<bool> Delete(long salaryPeriodGroupId)
        {
            var info = await _organizationDBContext.SalaryPeriodGroup.FirstOrDefaultAsync(s => s.SalaryPeriodGroupId == salaryPeriodGroupId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }
            var groupInfo = await _salaryGroupService.GetInfo(info.SalaryGroupId);

            var period = await _salaryPeriodService.GetInfo(info.SalaryPeriodId);

            info.IsDeleted = true;

            await _organizationDBContext.SaveChangesAsync();
            await _salaryPeriodActivityLog.LogBuilder(() => SalaryPeriodGroupActivityLogMessage.Delete)
                .MessageResourceFormatDatas(period.Month, period.Year, groupInfo.Title)
                .ObjectId(info.SalaryPeriodGroupId)
                .JsonData(info.JsonSerialize())
                .CreateLog();

            return true;
        }

        public async Task<bool> Update(long salaryPeriodGroupId, SalaryPeriodGroupModel model)
        {
            var info = await _organizationDBContext.SalaryPeriodGroup.FirstOrDefaultAsync(s => s.SalaryPeriodGroupId == salaryPeriodGroupId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }
            var groupInfo = await _salaryGroupService.GetInfo(info.SalaryGroupId);

            var period = await _salaryPeriodService.GetInfo(info.SalaryPeriodId);

            _mapper.Map(model, info);
            info.SalaryPeriodCensorStatusId = (int)EnumSalaryPeriodCensorStatus.New;
            info.SalaryPeriodGroupId = salaryPeriodGroupId;
            await _organizationDBContext.SaveChangesAsync();


            await _salaryPeriodActivityLog.LogBuilder(() => SalaryPeriodGroupActivityLogMessage.Update)
               .MessageResourceFormatDatas(period.Month, period.Year, groupInfo.Title)
               .ObjectId(info.SalaryPeriodGroupId)
               .JsonData(info.JsonSerialize())
               .CreateLog();

            return true;
        }
    }


}
