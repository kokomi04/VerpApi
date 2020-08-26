﻿using AutoMapper;
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

        public async Task<CalcProductPriceGetTableOutput> GetCalcProductPriceTable(CalcProductPriceGetTableInput req)
        {
            var fDate = req.FromDate.UnixToDateTime();
            var tDate = req.ToDate.UnixToDateTime();

            var indirectMaterialFeeSum = new SqlParameter("@IndirectMaterialFeeSum", SqlDbType.Decimal) { Direction = ParameterDirection.Output };
            var indirectLaborFeeSum = new SqlParameter("@IndirectLaborFeeSum", SqlDbType.Decimal) { Direction = ParameterDirection.Output };
            var generalManufacturingSum = new SqlParameter("@GeneralManufacturingSum", SqlDbType.Decimal) { Direction = ParameterDirection.Output };

            var data = (await _accountancyDBContext.QueryDataTable(
                "sp_CalcProductPrice",
                    new[] {
                    new SqlParameter("@FromDate", SqlDbType.DateTime2){ Value = fDate},
                    new SqlParameter("@ToDate", SqlDbType.DateTime2){ Value = tDate},

                    req.GroupColumns.ToNValueSqlParameter("@GroupColumns"),                    
                    req.AllocationRate.ToDecimalKeyValueSqlParameter("@AllocationRate"),
                    req.DirectMaterialFee.ToDecimalKeyValueSqlParameter("@DirectMaterialFee"),
                    req.DirectLaborFee.ToDecimalKeyValueSqlParameter("@DirectLaborFee"),
                    req.OtherFee.ToDecimalKeyValueSqlParameter("@OtherFee"),
                    req.CustomPrice.ToDecimalKeyValueSqlParameter("@CustomPrice"),

                    new SqlParameter("@IndirectMaterialFeeAllocationTypeId", SqlDbType.Int){ Value = req.IndirectMaterialFeeAllocationTypeId},
                    new SqlParameter("@IndirectMaterialFeeSumCustom", SqlDbType.Decimal){ Value = req.IndirectMaterialFeeSumCustom},
                    indirectMaterialFeeSum,

                    new SqlParameter("@IndirectLaborFeeAllocationTypeId", SqlDbType.Int){ Value = req.IndirectLaborFeeAllocationTypeId},
                    new SqlParameter("@IndirectLaborFeeSumCustom", SqlDbType.Decimal){ Value = req.IndirectLaborFeeSumCustom},
                    indirectLaborFeeSum,

                    new SqlParameter("@GeneralManufacturingAllocationTypeId", SqlDbType.Int){ Value = req.GeneralManufacturingAllocationTypeId},
                    new SqlParameter("@GeneralManufacturingSumCustom", SqlDbType.Decimal){ Value = req.GeneralManufacturingSumCustom},
                    generalManufacturingSum,

                    new SqlParameter("@IsReviewUpdate", SqlDbType.Decimal){ Value = req.IsReviewUpdate},
                    new SqlParameter("@IsUpdate", SqlDbType.Decimal){ Value = req.IsUpdate}

                }, CommandType.StoredProcedure)
                ).ConvertData();

            return new CalcProductPriceGetTableOutput()
            {
                Data = data,
                IndirectMaterialFeeSum = indirectMaterialFeeSum.Value as decimal?,
                IndirectLaborFeeSum = indirectLaborFeeSum.Value as decimal?,
                GeneralManufacturingSum = generalManufacturingSum.Value as decimal?
            };
        }
    }
}
