using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using NPOI.HSSF.Record.Chart;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verp.Resources.Report;
using Verp.Services.ReportConfig.Model;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Report;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.EF.ReportConfigDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace Verp.Services.ReportConfig.Service.Implement
{
    public class FilterSettingReportService : IFilterSettingReportService
    {
        private readonly ReportConfigDBContext _reportConfigDBContext;
        private readonly ICurrentContextService _currentContextService;

        private readonly ObjectActivityLogFacade _reportTypeViewActivityLog;

        public FilterSettingReportService(
            ReportConfigDBContext reportConfigDBContext,
            ICurrentContextService currentContextService,
            IActivityLogService activityLogService
            )
        {
            _reportConfigDBContext = reportConfigDBContext;
            _currentContextService = currentContextService;

            _reportTypeViewActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ReportType);
        }

        public async Task<Dictionary<int, object>> Get(int reportTypeId)
        {
            var reportTypeInfo = await _reportConfigDBContext.ReportType.FirstOrDefaultAsync(t => t.ReportTypeId == reportTypeId);
            if (reportTypeInfo == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var values = await (from v in _reportConfigDBContext.ReportTypeView
                                join f in _reportConfigDBContext.ReportTypeViewField.AsNoTracking() on v.ReportTypeViewId equals f.ReportTypeViewId
                                join value in _reportConfigDBContext.ReportTypeViewFieldValue on f.ReportTypeViewFieldId equals value.ReportTypeViewFieldId
                                where v.ReportTypeId == reportTypeId && v.ReportViewFilterTypeId == (int)EmumReportViewFilterType.Setting
                                select value).ToListAsync();
            return values.ToDictionary(v => v.ReportTypeViewFieldId, v => v.JsonValue.JsonDeserialize());
        }

        public async Task<bool> Update(int reportTypeId, Dictionary<int, object> fieldValues)
        {
            var reportTypeInfo = await _reportConfigDBContext.ReportType.FirstOrDefaultAsync(t => t.ReportTypeId == reportTypeId);
            if (reportTypeInfo == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var fields = await (from v in _reportConfigDBContext.ReportTypeView
                                join f in _reportConfigDBContext.ReportTypeViewField.AsNoTracking() on v.ReportTypeViewId equals f.ReportTypeViewId
                                where v.ReportTypeId == reportTypeId && v.ReportViewFilterTypeId == (int)EmumReportViewFilterType.Setting
                                select f).ToListAsync();
            var fieldIds = fields.Select(f => f.ReportTypeViewFieldId).ToHashSet();

            foreach (var (fieldId, value) in fieldValues)
            {
                if (!fieldIds.Contains(fieldId))
                {
                    throw GeneralCode.InvalidParams.BadRequest($"Tham số {fieldId} {value} không tồn tại trong báo cáo");
                }

                var fieldInfo = fields.FirstOrDefault(f => f.ReportTypeViewFieldId == fieldId);
                if (fieldInfo.FormTypeId == (int)EnumFormType.MultiSelect)
                {
                    if (!value.IsNullOrEmptyObject())
                    {
                        var type = value.GetType();
                        if (!type.IsArray && !typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                        {
                            throw GeneralCode.InvalidParams.BadRequest($"Tham số {fieldInfo.Title} {value} phải là dạng mảng");
                        }
                    }
                }
            }

            using (var trans = await _reportConfigDBContext.Database.BeginTransactionAsync())
            {
                var oldValues = await _reportConfigDBContext.ReportTypeViewFieldValue.Where(v => fieldIds.Contains(v.ReportTypeViewFieldId)).ToListAsync();
                _reportConfigDBContext.ReportTypeViewFieldValue.RemoveRange(oldValues);
                await _reportConfigDBContext.SaveChangesAsync();
                await _reportConfigDBContext.ReportTypeViewFieldValue.AddRangeAsync(fieldValues.Select(v => new ReportTypeViewFieldValue()
                {
                    ReportTypeViewFieldId = v.Key,
                    JsonValue = v.Value.JsonSerialize(),
                }));

                await _reportConfigDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _reportTypeViewActivityLog.LogBuilder(() => ReportTypeViewActivityLogMessage.UpdateSetting)
                  .MessageResourceFormatDatas(reportTypeInfo.ReportTypeName)
                  .ObjectId(reportTypeId)
                  .JsonData(fieldValues)
                  .CreateLog();

                return true;

            }

        }



    }



}
