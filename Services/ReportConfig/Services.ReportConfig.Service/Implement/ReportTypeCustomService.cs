using System;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ReportConfigDB;
using Verp.Services.ReportConfig.Model;
using AutoMapper;
using Microsoft.AspNetCore.DataProtection;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.System;
using VErp.Infrastructure.ServiceCore.Facade;
using Microsoft.Extensions.Logging;
using Verp.Resources.Report.ReportConfig;
using VErp.Infrastructure.EF.EFExtensions;
using Microsoft.EntityFrameworkCore;
using VErp.Infrastructure.ServiceCore.Service;

namespace Verp.Services.ReportConfig.Service.Implement
{
    internal class ReportTypeCustomService : IReportTypeCustomService
    {
        private readonly ReportConfigDBContext _reportConfigContext;
        private readonly AppSetting _appSetting;
        private readonly ObjectActivityLogFacade _objLogActivityReportTypeCustom;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IRoleHelperService _roleHelperService;
        public ReportTypeCustomService(ReportConfigDBContext reportConfigContext
            , ILogger<ReportConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper, IDataProtectionProvider protectionProvider, IRoleHelperService roleHelperService)
        {
            _reportConfigContext = reportConfigContext;
            _objLogActivityReportTypeCustom = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ReportType);
            _mapper = mapper;
            _logger = logger;
            _roleHelperService = roleHelperService;
        }

        #region Report Type Custom
        public async Task<int> AddReportTypeCustom(ReportTypeCustomImportModel data)
        {
            ReportTypeCustom report = _mapper.Map<ReportTypeCustom>(data);

            using var trans = await _reportConfigContext.Database.BeginTransactionAsync();
            try
            {

                await _reportConfigContext.ReportTypeCustom.AddAsync(report);
                await _reportConfigContext.SaveChangesAsync();
                trans.Commit();


                await _objLogActivityReportTypeCustom.LogBuilder(() => ReportConfigActivityLogMessage.CreateReport)
                             .MessageResourceFormatDatas(report.ReportTypeId)
                             .ObjectId(report.ReportTypeId)
                             .JsonData(data)
                             .CreateLog();

                await _roleHelperService.GrantPermissionForAllRoles(EnumModule.ReportView, EnumObjectType.ReportType, report.ReportTypeId);

            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Create");
                throw new BadRequestException(GeneralCode.InternalError);
            }

            data.ReportTypeId = report.ReportTypeId;
            return report.ReportTypeId;
        }

        public async Task<int> UpdateReportTypeCustom(int reportTypeId, ReportTypeCustomImportModel data)
        {
            var report = await _reportConfigContext.ReportTypeCustom
               .Where(r => r.ReportTypeId == reportTypeId)
               .FirstOrDefaultAsync();
            if (report == null)
            {
               return await AddReportTypeCustom(data);
            }
            using var trans = await _reportConfigContext.Database.BeginTransactionAsync();
            try
            {
                _mapper.Map(data, report);
                report.ReportTypeId = reportTypeId;

                await _reportConfigContext.SaveChangesAsync();
                trans.Commit();
                await _objLogActivityReportTypeCustom.LogBuilder(() => ReportConfigActivityLogMessage.UpdateReport)
                             .MessageResourceFormatDatas(report.ReportTypeId)
                             .ObjectId(report.ReportTypeId)
                             .JsonData(data)
                             .CreateLog();

            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Update");
                throw new BadRequestException(GeneralCode.InternalError);
            }

            data.ReportTypeId = reportTypeId;
            return report.ReportTypeId;
        }

        public async Task<bool> DeleteReportTypeCustom(int reportTypeId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockReportKey(reportTypeId));
            var report = await _reportConfigContext.ReportTypeCustom
                .Where(r => r.ReportTypeId == reportTypeId)
                .FirstOrDefaultAsync();
            if (report == null)
            {
                throw new BadRequestException(ReportErrorCode.ReportNotFound);
            }
            using var trans = await _reportConfigContext.Database.BeginTransactionAsync();
            try
            {
                report.IsDeleted = true;
                await _reportConfigContext.SaveChangesAsync();
                trans.Commit();

                await _objLogActivityReportTypeCustom.LogBuilder(() => ReportConfigActivityLogMessage.DeleteReport)
                             .MessageResourceFormatDatas(report.ReportTypeId)
                             .ObjectId(report.ReportTypeId)
                             .JsonData(report)
                             .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Delete");
                throw new BadRequestException(GeneralCode.InternalError);
            }
        }

        public async Task<ReportTypeCustomModel> InfoReportTypeCustom(int reportTypeId)
        {
            var report = await _reportConfigContext.ReportTypeCustom
               .Where(r => r.ReportTypeId == reportTypeId)
               .FirstOrDefaultAsync();
            var reportType = _mapper.Map<ReportTypeCustomModel>(report);
            if (report == null)
            {
                return new ReportTypeCustomModel()
                {
                    ReportTypeId = reportTypeId
                };
            }
            return reportType;
        }
        #endregion
    }
}
