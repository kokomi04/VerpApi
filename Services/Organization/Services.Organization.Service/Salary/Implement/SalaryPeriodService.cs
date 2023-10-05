using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Resources.Organization.Salary;
using Verp.Resources.Organization.Salary.Validation;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Organization.Salary;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Abstract;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Salary;
using VErp.Services.Organization.Service.Salary.Implement.Abstract;

namespace VErp.Services.Organization.Service.Salary
{
    public class SalaryPeriodService : SalaryPeriodGroupEmployeeAbstract, ISalaryPeriodService
    {
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _salaryPeriodActivityLog;

        public SalaryPeriodService(OrganizationDBContext organizationDBContext, ICurrentContextService currentContextService, ILogger<SalaryPeriodService> logger, IMapper mapper, IActivityLogService activityLogService)
            : base(organizationDBContext, currentContextService, logger)
        {
            _mapper = mapper;
            _salaryPeriodActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.SalaryPeriod);
        }

        public async Task<bool> Censor(int salaryPeriodId, bool isSuccess)
        {
            var info = await _organizationDBContext.SalaryPeriod.FirstOrDefaultAsync(s => s.SalaryPeriodId == salaryPeriodId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            await ValidateDateOfBill(new DateTime(info.Year, info.Month, 1).ToUniversalTime(), null);

            await ValidateCensor(salaryPeriodId);

            if (isSuccess)
            {

                if (info.SalaryPeriodCensorStatusId != (int)EnumSalaryPeriodCensorStatus.CensorRejected && info.SalaryPeriodCensorStatusId != (int)EnumSalaryPeriodCensorStatus.CheckedAccepted)
                {
                    throw SalaryPeriodValidationMessage.InvalidStatus.BadRequest();
                }

            }
            else
            {
                if (info.SalaryPeriodCensorStatusId != (int)EnumSalaryPeriodCensorStatus.CheckedAccepted)
                {
                    throw SalaryPeriodValidationMessage.InvalidStatus.BadRequest();
                }
            }

            info.SalaryPeriodCensorStatusId = (int)(isSuccess ? EnumSalaryPeriodCensorStatus.CensorApproved : EnumSalaryPeriodCensorStatus.CensorRejected);
            info.CensorByUserId = _currentContextService.UserId;
            info.CensorDatetimeUtc = DateTime.UtcNow;

            await _organizationDBContext.SaveChangesAsync();

            ObjectActivityLogModelBuilder<string> logBuilder;
            if (isSuccess)
                logBuilder = _salaryPeriodActivityLog.LogBuilder(() => SalaryPeriodActivityLogMessage.CensorApproved);
            else
                logBuilder = _salaryPeriodActivityLog.LogBuilder(() => SalaryPeriodActivityLogMessage.CensorRejected);

            await logBuilder
                .MessageResourceFormatDatas(info.Month, info.Year)
                .ObjectId(info.SalaryPeriodId)
                .JsonData(info)
                .CreateLog();

            return true;
        }

        public async Task<bool> Check(int salaryPeriodId, bool isSuccess)
        {
            var info = await _organizationDBContext.SalaryPeriod.FirstOrDefaultAsync(s => s.SalaryPeriodId == salaryPeriodId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }
            
            await ValidateDateOfBill(new DateTime(info.Year, info.Month, 1).ToUniversalTime(), null);

            await ValidateCensor(salaryPeriodId);

            if (isSuccess)
            {

                if (info.SalaryPeriodCensorStatusId != (int)EnumSalaryPeriodCensorStatus.CheckedRejected && info.SalaryPeriodCensorStatusId != (int)EnumSalaryPeriodCensorStatus.New)
                {
                    throw SalaryPeriodValidationMessage.InvalidStatus.BadRequest();
                }

            }
            else
            {
                if (info.SalaryPeriodCensorStatusId != (int)EnumSalaryPeriodCensorStatus.New)
                {
                    throw SalaryPeriodValidationMessage.InvalidStatus.BadRequest();
                }
            }

            info.SalaryPeriodCensorStatusId = (int)(isSuccess ? EnumSalaryPeriodCensorStatus.CheckedAccepted : EnumSalaryPeriodCensorStatus.CheckedRejected);
            info.CheckedByUserId = _currentContextService.UserId;
            info.CheckedDatetimeUtc = DateTime.UtcNow;

            await _organizationDBContext.SaveChangesAsync();

            ObjectActivityLogModelBuilder<string> logBuilder;
            if (isSuccess)
                logBuilder = _salaryPeriodActivityLog.LogBuilder(() => SalaryPeriodActivityLogMessage.CheckAccepted);
            else
                logBuilder = _salaryPeriodActivityLog.LogBuilder(() => SalaryPeriodActivityLogMessage.CensorRejected);

            await logBuilder
                .MessageResourceFormatDatas(info.Month, info.Year)
                .ObjectId(info.SalaryPeriodId)
                .JsonData(info)
                .CreateLog();

            return true;
        }

        private async Task ValidateCensor(int salaryPeriodId)
        {
            var childNotApproved = await _organizationDBContext.SalaryPeriodGroup.FirstOrDefaultAsync(g => g.SalaryPeriodId == salaryPeriodId && g.SalaryPeriodCensorStatusId != (int)EnumSalaryPeriodCensorStatus.CensorApproved);
            if (childNotApproved != null)
            {
                throw SalaryPeriodValidationMessage.PeriodGroupHasNotApprovedYet.BadRequest();
            }

            var periodGroupIds = await _organizationDBContext.SalaryPeriodGroup.Where(p => p.SalaryPeriodId == salaryPeriodId).Select(p => p.SalaryGroupId).ToListAsync();

            var groupIds = await _organizationDBContext.SalaryGroup
                .Where(g => g.IsActived || periodGroupIds.Contains(g.SalaryGroupId))
                .Select(g => g.SalaryGroupId).ToListAsync();

            var notCreatedYet = groupIds.Except(periodGroupIds).ToList();
            if (notCreatedYet.Count() > 0)
            {
                var groupInfo = await _organizationDBContext.SalaryGroup.FirstOrDefaultAsync(s => s.SalaryGroupId == notCreatedYet[0]);
                throw SalaryPeriodValidationMessage.PeriodGroupHasNotCreatedYet.BadRequestFormat(groupInfo?.Title);
            }
        }

        public async Task<int> Create(SalaryPeriodModel model)
        {
            if (await _organizationDBContext.SalaryPeriod.AnyAsync(p => p.Year == model.Year && p.Month == model.Month))
            {
                throw SalaryPeriodValidationMessage.PeriodHasBeenCreated.BadRequestFormat(model.Month, model.Year);
            }

            var info = _mapper.Map<SalaryPeriod>(model);

            await ValidateDateOfBill(new DateTime(info.Year, info.Month, 1).ToUniversalTime(), null);

            info.SalaryPeriodCensorStatusId = (int)EnumSalaryPeriodCensorStatus.New;
            await _organizationDBContext.SalaryPeriod.AddAsync(info);
            await _organizationDBContext.SaveChangesAsync();
            await _salaryPeriodActivityLog.LogBuilder(() => SalaryPeriodActivityLogMessage.Create)
                .MessageResourceFormatDatas(model.Month, model.Year)
                .ObjectId(info.SalaryPeriodId)
                .JsonData(model)
                .CreateLog();
            return info.SalaryPeriodId;
        }

        public async Task<bool> Delete(int salaryPeriodId)
        {
            using (var trans = await _organizationDBContext.Database.BeginTransactionAsync())
            {
                var info = await _organizationDBContext.SalaryPeriod.FirstOrDefaultAsync(s => s.SalaryPeriodId == salaryPeriodId);
                if (info == null)
                {
                    throw GeneralCode.ItemNotFound.BadRequest();
                }
                await ValidateDateOfBill(new DateTime(info.Year, info.Month, 1).ToUniversalTime(), null);

                info.IsDeleted = true;

                await _organizationDBContext.SaveChangesAsync();

                await DeletePeriodGroupEmployee(salaryPeriodId);

                await DeletePeriodGroup(salaryPeriodId);

                await trans.CommitAsync();

                await _salaryPeriodActivityLog.LogBuilder(() => SalaryPeriodActivityLogMessage.Delete)
                    .MessageResourceFormatDatas(info.Month, info.Year)
                    .ObjectId(info.SalaryPeriodId)
                    .JsonData(info)
                    .CreateLog();

                return true;
            }
        }


        private async Task DeletePeriodGroupEmployee(int salaryPeriodId)
        {
            await DeleteSalaryEmployeeByPeriodGroup(salaryPeriodId, null);
        }

        private async Task DeletePeriodGroup(int salaryPeriodId)
        {
            await _organizationDBContext.SalaryPeriodGroup.Where(e => e.SalaryPeriodId == salaryPeriodId)
               .UpdateByBatch(e => new SalaryPeriodGroup
               {
                   IsDeleted = true,
                   DeletedDatetimeUtc = DateTime.UtcNow,
                   UpdatedByUserId = _currentContextService.UserId
               });
        }


        public async Task<SalaryPeriodInfo> GetInfo(int year, int month)
        {
            return await _organizationDBContext.SalaryPeriod.Where(s => s.Year == year && s.Month == month)
                .ProjectTo<SalaryPeriodInfo>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<SalaryPeriodInfo> GetInfo(int salaryPeriodId)
        {
            return await _organizationDBContext.SalaryPeriod.Where(s => s.SalaryPeriodId == salaryPeriodId)
                .ProjectTo<SalaryPeriodInfo>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<PageData<SalaryPeriodInfo>> GetList(int page, int size)
        {
            var lst = _organizationDBContext.SalaryPeriod.ProjectTo<SalaryPeriodInfo>(_mapper.ConfigurationProvider);
            var total = await lst.CountAsync();
            var lstPaged = await lst.Skip((page - 1) * size).Take(size).ToListAsync();
            return (lstPaged, total);
        }

        public async Task<bool> Update(int salaryPeriodId, SalaryPeriodModel model)
        {
            if (await _organizationDBContext.SalaryPeriod.AnyAsync(p => p.Year == model.Year && p.Month == model.Month && p.SalaryPeriodId != salaryPeriodId))
            {
                throw SalaryPeriodValidationMessage.PeriodHasBeenCreated.BadRequestFormat(model.Month, model.Year);
            }

            var info = await _organizationDBContext.SalaryPeriod.FirstOrDefaultAsync(s => s.SalaryPeriodId == salaryPeriodId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }
            
            await ValidateDateOfBill(new DateTime(info.Year, info.Month, 1).ToUniversalTime(), new DateTime(model.Year, model.Month, 1).ToUniversalTime());

            _mapper.Map(model, info);
            info.SalaryPeriodCensorStatusId = (int)EnumSalaryPeriodCensorStatus.New;
            info.SalaryPeriodId = salaryPeriodId;
            await _organizationDBContext.SaveChangesAsync();

            await _salaryPeriodActivityLog.LogBuilder(() => SalaryPeriodActivityLogMessage.Update)
                .MessageResourceFormatDatas(info.Month, info.Year)
                .ObjectId(info.SalaryPeriodId)
                .JsonData(info)
                .CreateLog();

            return true;
        }

        public async Task<IList<SalaryPeriodInfo>> GetAllList()
        {
            return await _organizationDBContext.SalaryPeriod.ProjectTo<SalaryPeriodInfo>(_mapper.ConfigurationProvider).OrderByDescending(x=> x.Year).ThenByDescending(x=>x.Month).ToListAsync();
        }
    }
}
