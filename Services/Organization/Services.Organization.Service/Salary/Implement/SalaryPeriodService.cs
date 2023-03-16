using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
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
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary
{
    public class SalaryPeriodService : ISalaryPeriodService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _salaryPeriodActivityLog;

        public SalaryPeriodService(OrganizationDBContext organizationDBContext, ICurrentContextService currentContextService, IMapper mapper, IActivityLogService activityLogService)
        {
            _organizationDBContext = organizationDBContext;
            _currentContextService = currentContextService;
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
                .JsonData(info.JsonSerialize())
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
                .JsonData(info.JsonSerialize())
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


            var groupIds = await _organizationDBContext.SalaryGroup.Select(g => g.SalaryGroupId).ToListAsync();
            var periodGroupIds = await _organizationDBContext.SalaryPeriodGroup.Where(p => p.SalaryPeriodId == salaryPeriodId).Select(p => p.SalaryGroupId).ToListAsync();
            if (groupIds.Count != periodGroupIds.Count)
            {
                throw SalaryPeriodValidationMessage.PeriodGroupHasNotCreatedYet.BadRequest();
            }
        }

        public async Task<int> Create(SalaryPeriodModel model)
        {
            var info = _mapper.Map<SalaryPeriod>(model);
            info.SalaryPeriodCensorStatusId = (int)EnumSalaryPeriodCensorStatus.New;
            await _organizationDBContext.SalaryPeriod.AddAsync(info);
            await _organizationDBContext.SaveChangesAsync();
            await _salaryPeriodActivityLog.LogBuilder(() => SalaryPeriodActivityLogMessage.Create)
                .MessageResourceFormatDatas(model.Month, model.Year)
                .ObjectId(info.SalaryPeriodId)
                .JsonData(model.JsonSerialize())
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

                info.IsDeleted = true;

                await _organizationDBContext.SaveChangesAsync();

                await DeletePeriodGroupEmployee(salaryPeriodId);

                await DeletePeriodGroup(salaryPeriodId);

                await trans.CommitAsync();

                await _salaryPeriodActivityLog.LogBuilder(() => SalaryPeriodActivityLogMessage.Delete)
                    .MessageResourceFormatDatas(info.Month, info.Year)
                    .ObjectId(info.SalaryPeriodId)
                    .JsonData(info.JsonSerialize())
                    .CreateLog();

                return true;
            }
        }


        private async Task DeletePeriodGroupEmployee(int salaryPeriodId)
        {
            await _organizationDBContext.SalaryEmployee.Where(e => e.SalaryPeriodId == salaryPeriodId)
                .UpdateByBatch(e => new SalaryEmployee
                {
                    IsDeleted = true,
                    DeletedDatetimeUtc = DateTime.UtcNow,
                    UpdatedByUserId = _currentContextService.UserId
                });
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


        public async Task<SalaryPeriodModel> GetInfo(int year, int month)
        {
            return await _organizationDBContext.SalaryPeriod.Where(s => s.Year == year && s.Month == month)
                .ProjectTo<SalaryPeriodModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<SalaryPeriodModel> GetInfo(int salaryPeriodId)
        {
            return await _organizationDBContext.SalaryPeriod.Where(s => s.SalaryPeriodId == salaryPeriodId)
                .ProjectTo<SalaryPeriodModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<PageData<SalaryPeriodModel>> GetList(int page, int size)
        {
            var lst = _organizationDBContext.SalaryPeriod.ProjectTo<SalaryPeriodModel>(_mapper.ConfigurationProvider);
            var total = await lst.CountAsync();
            var lstPaged = await lst.Skip((page - 1) * size).Take(size).ToListAsync();
            return (lstPaged, total);
        }

        public async Task<bool> Update(int salaryPeriodId, SalaryPeriodModel model)
        {
            var info = await _organizationDBContext.SalaryPeriod.FirstOrDefaultAsync(s => s.SalaryPeriodId == salaryPeriodId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }
            _mapper.Map(model, info);
            info.SalaryPeriodCensorStatusId = (int)EnumSalaryPeriodCensorStatus.New;
            info.SalaryPeriodId = salaryPeriodId;
            await _organizationDBContext.SaveChangesAsync();

            await _salaryPeriodActivityLog.LogBuilder(() => SalaryPeriodActivityLogMessage.Update)
                .MessageResourceFormatDatas(info.Month, info.Year)
                .ObjectId(info.SalaryPeriodId)
                .JsonData(info.JsonSerialize())
                .CreateLog();

            return true;
        }
    }
}
