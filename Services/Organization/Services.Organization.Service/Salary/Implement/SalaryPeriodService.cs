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
        private readonly ObjectActivityLogFacade _salaryRefTableActivityLog;

        public SalaryPeriodService(OrganizationDBContext organizationDBContext, ICurrentContextService currentContextService, IMapper mapper, IActivityLogService activityLogService)
        {
            _organizationDBContext = organizationDBContext;
            _currentContextService = currentContextService;
            _mapper = mapper;
            _salaryRefTableActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.SalaryPeriod);
        }

        public async Task<bool> Censor(int salaryPeriodId, bool isSuccess)
        {
            var info = await _organizationDBContext.SalaryPeriod.FirstOrDefaultAsync(s => s.SalaryPeriodId == salaryPeriodId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            info.SalaryPeriodCensorStatusId = (int)(isSuccess ? EnumSalaryPeriodCensorStatus.CensorApproved : EnumSalaryPeriodCensorStatus.CensorRejected);
            info.CensorByUserId = _currentContextService.UserId;
            info.CensorDatetimeUtc= DateTime.UtcNow;

            await _organizationDBContext.SaveChangesAsync();

            ObjectActivityLogModelBuilder<string> logBuilder;
            if (isSuccess)
                logBuilder = _salaryRefTableActivityLog.LogBuilder(() => SalaryPeriodActivityLogMessage.CensorApproved);
            else
                logBuilder = _salaryRefTableActivityLog.LogBuilder(() => SalaryPeriodActivityLogMessage.CensorRejected);

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

            info.SalaryPeriodCensorStatusId = (int)(isSuccess ? EnumSalaryPeriodCensorStatus.CheckedAccepted : EnumSalaryPeriodCensorStatus.CheckedRejected);
            info.CheckedByUserId = _currentContextService.UserId;
            info.CheckedDatetimeUtc = DateTime.UtcNow;

            await _organizationDBContext.SaveChangesAsync();

            ObjectActivityLogModelBuilder<string> logBuilder;
            if (isSuccess)
                logBuilder = _salaryRefTableActivityLog.LogBuilder(() => SalaryPeriodActivityLogMessage.CheckAccepted);
            else
                logBuilder = _salaryRefTableActivityLog.LogBuilder(() => SalaryPeriodActivityLogMessage.CensorRejected);

            await logBuilder
                .MessageResourceFormatDatas(info.Month, info.Year)
                .ObjectId(info.SalaryPeriodId)
                .JsonData(info.JsonSerialize())
                .CreateLog();

            return true;
        }

        public async Task<int> Create(SalaryPeriodModel model)
        {
            var info = _mapper.Map<SalaryPeriod>(model);
            info.SalaryPeriodCensorStatusId = (int)EnumSalaryPeriodCensorStatus.New;
            await _organizationDBContext.SalaryPeriod.AddAsync(info);
            await _organizationDBContext.SaveChangesAsync();
            await _salaryRefTableActivityLog.LogBuilder(() => SalaryPeriodActivityLogMessage.Create)
                .MessageResourceFormatDatas(model.Month, model.Year)
                .ObjectId(info.SalaryPeriodId)
                .JsonData(model.JsonSerialize())
                .CreateLog();
            return info.SalaryPeriodId;
        }

        public async Task<bool> Delete(int salaryPeriodId)
        {
            var info = await _organizationDBContext.SalaryPeriod.FirstOrDefaultAsync(s => s.SalaryPeriodId == salaryPeriodId);
            if (info == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            info.IsDeleted = true;

            await _organizationDBContext.SaveChangesAsync();
            await _salaryRefTableActivityLog.LogBuilder(() => SalaryPeriodActivityLogMessage.Delete)
                .MessageResourceFormatDatas(info.Month, info.Year)
                .ObjectId(info.SalaryPeriodId)
                .JsonData(info.JsonSerialize())
                .CreateLog();

            return true;
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

            await _salaryRefTableActivityLog.LogBuilder(() => SalaryPeriodActivityLogMessage.Update)
                .MessageResourceFormatDatas(info.Month, info.Year)
                .ObjectId(info.SalaryPeriodId)
                .JsonData(info.JsonSerialize())
                .CreateLog();

            return true;
        }
    }
}
