using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Accountancy.Service.Input.Implement
{
    public class CalcProductPriceService : ICalcProductPriceService
    {

        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly AccountancyDBContext _accountancyDBContext;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IOutsideImportMappingService _outsideImportMappingService;

        public CalcProductPriceService(AccountancyDBContext accountancyDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<InputConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , ICurrentContextService currentContextService
            , IOutsideImportMappingService outsideImportMappingService
            )
        {
            _accountancyDBContext = accountancyDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _currentContextService = currentContextService;
            _outsideImportMappingService = outsideImportMappingService;
        }

        public async Task<IList<NonCamelCaseDictionary>> GetCalcProductPriceTable(CalcProductPriceGetTableInput req)
        {
            var fDate = req.FromDate.UnixToDateTime();
            var tDate = req.ToDate.UnixToDateTime();
            return (await _accountancyDBContext.QueryDataTable(
                "sp_CalcProductPrice",
                    new[] {
                    new SqlParameter("@FromDate", SqlDbType.DateTime2){ Value = fDate},
                    new SqlParameter("@ToDate", SqlDbType.DateTime2){ Value = tDate},
                    req.GroupColumns.ToNValueSqlParameter("@GroupColumns"),
                    req.OtherFee.ToDecimalKeyValueSqlParameter("@OtherFee"),
                    new SqlParameter("@CP_NVL_GT_TCPB_ID", SqlDbType.Int){ Value = req.CP_NVL_GT_TCPB_ID},
                    new SqlParameter("@CP_NHANC_GT_TCPB_ID", SqlDbType.Int){ Value = req.CP_NHANC_GT_TCPB_ID},
                    new SqlParameter("@CP_SXCHUNG_TCPB_ID", SqlDbType.Int){ Value = req.CP_SXCHUNG_TCPB_ID},
                }, CommandType.StoredProcedure)
                ).ConvertData();
        }
    }
}
