using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Services.ReportConfig.Model;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
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
using VErp.Infrastructure.ServiceCore.Service;

namespace Verp.Services.ReportConfig.Service.Implement
{
    public interface IDataDashboardService
    {
        Task<IList<NonCamelCaseDictionary>> Dashboard(int dashboardTypeId, ReportFilterDataModel model);
    }

    public class DataDashboardService : IDataDashboardService
    {
        private readonly ReportConfigDBContext _reportConfigDBContext;
        private readonly IDashboardConfigService _dashboardConfigService;
        private readonly IDocOpenXmlService _docOpenXmlService;
        private readonly AppSetting _appSetting;
        private readonly IPhysicalFileService _physicalFileService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICurrentContextService _currentContextService;

        private readonly Dictionary<EnumModuleType, Type> ModuleDbContextTypes = new Dictionary<EnumModuleType, Type>()
        {
            { EnumModuleType.Accountant,typeof(AccountancyDBContext) },
            { EnumModuleType.Master,typeof(MasterDBContext) },
            { EnumModuleType.PurchaseOrder,typeof(PurchaseOrderDBContext) },
            { EnumModuleType.Stock,typeof(StockDBContext) },
            { EnumModuleType.Organization,typeof(OrganizationDBContext) },
            { EnumModuleType.Manufacturing,typeof(ManufacturingDBContext) }
        };

        private readonly Dictionary<EnumModuleType, DbContext> ModuleDbContexts = new Dictionary<EnumModuleType, DbContext>();

        public DataDashboardService(
            ReportConfigDBContext reportConfigDBContext,
            IDashboardConfigService dashboardConfigService,
            IDocOpenXmlService docOpenXmlService,
            IOptions<AppSetting> appSetting,
            IPhysicalFileService physicalFileService,
            IServiceProvider serviceProvider,
            ICurrentContextService currentContextService
            )
        {
            _reportConfigDBContext = reportConfigDBContext;
            _dashboardConfigService = dashboardConfigService;
            _docOpenXmlService = docOpenXmlService;
            _appSetting = appSetting.Value;
            _physicalFileService = physicalFileService;
            _serviceProvider = serviceProvider;
            _currentContextService = currentContextService;
        }

        private DbContext GetDbContext(EnumModuleType moduleType)
        {
            if (ModuleDbContexts.ContainsKey(moduleType)) return ModuleDbContexts[moduleType];

            if (!ModuleDbContextTypes.ContainsKey(moduleType))
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy DBContext cho phân hệ {moduleType.GetEnumDescription()}");
            }
            var dbContext = _serviceProvider.GetService(ModuleDbContextTypes[moduleType]) as DbContext;
            ModuleDbContexts.TryAdd(moduleType, dbContext);
            return dbContext;
        }

        public async Task<IList<NonCamelCaseDictionary>> Dashboard(int dashboardTypeId, ReportFilterDataModel model)
        {
            var filters = model.Filters.GroupBy(f => f.Key.Trim().ToLower()).ToDictionary(f => f.Key, f => f.Last().Value);

            var dashboardTypeInfo = await _reportConfigDBContext.DashboardType.Include(x => x.DashboardTypeGroup).AsNoTracking().FirstOrDefaultAsync(r => r.DashboardTypeId == dashboardTypeId);

            if (dashboardTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại biểu đồ báo cáo");


            var _dbContext = GetDbContext((EnumModuleType)dashboardTypeInfo.DashboardTypeGroup.ModuleTypeId);

            var reportViewInfo = await _dashboardConfigService.DashboardTypeViewGetInfo(dashboardTypeInfo.DashboardTypeId, isConfig: true);

            var sqlParams = new List<SqlParameter>();

            foreach (var filterFiled in reportViewInfo.Fields)
            {
                object value = null;
                foreach (var param in filterFiled.ParamerterName.Split(','))
                {
                    if (string.IsNullOrWhiteSpace(param)) continue;

                    var paramName = param.Trim().ToLower();
                    if (filterFiled.FormTypeId == EnumFormType.MultiSelect)
                    {
                        if (filters.ContainsKey(paramName))
                        {
                            value = filters[paramName];

                        }
                        switch (filterFiled.DataTypeId)
                        {
                            case EnumDataType.Int:
                                sqlParams.Add((!value.IsNullObject() ? ((JArray)value).ToObject<IList<int>>() : Array.Empty<int>()).ToSqlParameter($"@{paramName}"));
                                break;
                            case EnumDataType.BigInt:
                                sqlParams.Add((!value.IsNullObject() ? ((JArray)value).ToObject<IList<long>>() : Array.Empty<long>()).ToSqlParameter($"@{paramName}"));
                                break;
                            case EnumDataType.Text:
                                sqlParams.Add((!value.IsNullObject() ? ((JArray)value).ToObject<IList<string>>() : Array.Empty<string>()).ToSqlParameter($"@{paramName}"));
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        if (filters.ContainsKey(paramName))
                        {
                            value = filters[paramName];
                            if (!value.IsNullObject())
                            {
                                if (filterFiled.DataTypeId.IsTimeType())
                                {
                                    value = Convert.ToInt64(value);
                                }
                            }
                        }
                        sqlParams.Add(new SqlParameter($"@{paramName}", filterFiled.DataTypeId.GetSqlValue(value)));
                    }
                }
            }

            var suffix = 0;
            var filterCondition = new StringBuilder();
            if (model.ColumnsFilters != null)
            {
                var viewAlias = string.Empty;
                model.ColumnsFilters.FilterClauseProcess(string.Empty, viewAlias, ref filterCondition, ref sqlParams, ref suffix);
            }

            return await GetRowsByQuery(dashboardTypeInfo, filterCondition.ToString(), sqlParams.Select(p => p.CloneSqlParam()).ToList());
        }

        private async Task<IList<NonCamelCaseDictionary>> GetRowsByQuery(DashboardType reportInfo, string filterCondition, IList<SqlParameter> sqlParams)
        {
            var _dbContext = GetDbContext((EnumModuleType)reportInfo.DashboardTypeGroup.ModuleTypeId);

            var sql = reportInfo.BodySql;

            if (reportInfo.BodySql.Contains("$FILTER"))
            {
                sql = sql.Replace("$FILTER", string.IsNullOrWhiteSpace(filterCondition) ? " 1 = 1 " : filterCondition);
            }
            else
            {
                if (!string.IsNullOrEmpty(filterCondition))
                {
                    sql = sql.TSqlAppendCondition(filterCondition);
                }
            }


            var table = await _dbContext.QueryDataTable(sql, sqlParams.Select(p => p.CloneSqlParam()).ToArray(), timeout: AccountantConstants.REPORT_QUERY_TIMEOUT);

            var totals = new NonCamelCaseDictionary<decimal>();

            var data = table.ConvertData();

            return data;
        }
    }
}