using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountancy.Model.Data;
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
        private readonly ICalcPeriodService _calcPeriodService;

        public CalcProductPriceService(AccountancyDBContext accountancyDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<InputConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , ICurrentContextService currentContextService
            , ICalcPeriodService calcPeriodService
            )
        {
            _accountancyDBContext = accountancyDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _currentContextService = currentContextService;
            _calcPeriodService = calcPeriodService;
        }

        public async Task<CalcProductPriceGetTableOutput> CalcProductPriceTable(CalcProductPriceGetTableInput req)
        {
            var fDate = req.FromDate.UnixToDateTime();
            var tDate = req.ToDate.UnixToDateTime();

            var indirectMaterialFeeSum = new SqlParameter("@IndirectMaterialFeeSum", SqlDbType.Decimal) { Direction = ParameterDirection.Output };
            var indirectLaborFeeSum = new SqlParameter("@IndirectLaborFeeSum", SqlDbType.Decimal) { Direction = ParameterDirection.Output };
            var generalManufacturingSum = new SqlParameter("@GeneralManufacturingSum", SqlDbType.Decimal) { Direction = ParameterDirection.Output };

            if (!string.IsNullOrWhiteSpace(req.OrderCode))
            {
                req.IsByOrder = true;
            }
            if (!string.IsNullOrWhiteSpace(req.MaLsx))
            {
                req.IsByLsx = true;
            }
            if (req.StockId > 0)
            {
                req.IsByStock = true;
            }
            var data = (await _accountancyDBContext.QueryDataTable(
                "asp_CalcProductPrice",
                    new[] {
                    new SqlParameter("@ProductId", SqlDbType.Int){ Value = req.ProductId.HasValue?req.ProductId.Value: (object)DBNull.Value},
                    new SqlParameter("@OrderCode", SqlDbType.NVarChar){ Value = !string.IsNullOrWhiteSpace(req.OrderCode) ?req.OrderCode.Trim(): (object)DBNull.Value},
                    new SqlParameter("@MaLsx", SqlDbType.NVarChar){ Value = !string.IsNullOrWhiteSpace(req.MaLsx) ?req.MaLsx.Trim(): (object)DBNull.Value},
                    new SqlParameter("@StockId", SqlDbType.Int){ Value = req.StockId.HasValue ?req.StockId.Value: (object)DBNull.Value},

                    new SqlParameter("@FromDate", SqlDbType.DateTime2){ Value = fDate},
                    new SqlParameter("@ToDate", SqlDbType.DateTime2){ Value = tDate},

                    new SqlParameter("@IsByLsx", SqlDbType.Decimal){ Value = req.IsByLsx},
                    new SqlParameter("@IsByOrder", SqlDbType.Decimal){ Value = req.IsByOrder},
                    new SqlParameter("@IsByStock", SqlDbType.Decimal){ Value = req.IsByStock},

                    req.AllocationRate.ToDecimalKeyValueSqlParameter("@AllocationRate"),
                    req.DirectMaterialFee.ToDecimalKeyValueSqlParameter("@DirectMaterialFee"),
                    req.DirectLaborFee.ToDecimalKeyValueSqlParameter("@DirectLaborFee"),
                    req.DirectGeneralFee.ToDecimalKeyValueSqlParameter("@DirectGeneralFee"),
                    req.OtherFee.ToDecimalKeyValueSqlParameter("@OtherFee"),
                    req.CustomPrice.ToDecimalKeyValueSqlParameter("@CustomPrice"),

                    new SqlParameter("@IndirectMaterialFeeAllocationTypeId", SqlDbType.Int){ Value = req.IndirectMaterialFeeAllocationTypeId},
                    req.IndirectMaterialFeeSumCustom.ToSqlParameterValue("@IndirectMaterialFeeSumCustom"),
                    indirectMaterialFeeSum,

                    new SqlParameter("@IndirectLaborFeeAllocationTypeId", SqlDbType.Int){ Value = req.IndirectLaborFeeAllocationTypeId},
                    req.IndirectLaborFeeSumCustom.ToSqlParameterValue("@IndirectLaborFeeSumCustom"),

                    indirectLaborFeeSum,

                    new SqlParameter("@GeneralManufacturingAllocationTypeId", SqlDbType.Int){ Value = req.GeneralManufacturingAllocationTypeId},
                    req.GeneralManufacturingSumCustom.ToSqlParameterValue("@GeneralManufacturingSumCustom"),

                    generalManufacturingSum,

                    new SqlParameter("@IsReviewUpdate", SqlDbType.Bit){ Value = req.IsReviewUpdate},
                    new SqlParameter("@IsUpdate", SqlDbType.Bit){ Value = req.IsUpdate}

                }, CommandType.StoredProcedure, new TimeSpan(0, 30, 0))
                ).ConvertData();

            return new CalcProductPriceGetTableOutput()
            {
                Data = data,
                IndirectMaterialFeeSum = indirectMaterialFeeSum.Value as decimal?,
                IndirectLaborFeeSum = indirectLaborFeeSum.Value as decimal?,
                GeneralManufacturingSum = generalManufacturingSum.Value as decimal?
            };
        }


        public async Task<CalcProductOutputPriceModel> CalcProductOutputPrice(CalcProductOutputPriceInput req)
        {
            var fDate = req.FromDate.UnixToDateTime();
            var tDate = req.ToDate.UnixToDateTime();
            var isInvalid = new SqlParameter("@IsInvalid", SqlDbType.Bit) { Direction = ParameterDirection.Output };

            var data = (await _accountancyDBContext.QueryDataTable(
                "asp_CalcProduct_OutputPrice",
                    new[] {

                    new SqlParameter("@FromDate", SqlDbType.DateTime2){ Value = fDate},
                    new SqlParameter("@ToDate", SqlDbType.DateTime2){ Value = tDate},
                    new SqlParameter("@ProductId", SqlDbType.Int){ Value = req.ProductId.HasValue?req.ProductId.Value: (object)DBNull.Value},
                    new SqlParameter("@Tk", SqlDbType.NVarChar){ Value = req.Tk},
                    new SqlParameter("@IsUpdate", SqlDbType.Bit){ Value = req.IsUpdate},
                    isInvalid

                }, CommandType.StoredProcedure, new TimeSpan(0, 30, 0))
                ).ConvertData();

            return new CalcProductOutputPriceModel
            {
                Data = data,
                IsInvalid = (isInvalid.Value as bool?).GetValueOrDefault()
            };
        }

        public async Task<IList<NonCamelCaseDictionary>> GetWeightedAverageProductPrice(CalcProductPriceWeightedAverageInput req)
        {
            return (
                await _accountancyDBContext.QueryDataTable(
                "usp_CalcProductPrice_WeightedAverage",
                 new[] {
                    new SqlParameter("@Date", SqlDbType.DateTime2){ Value = req.Date.UnixToDateTime()},
                    req.ProductIds.ToSqlParameter("@ProductIds")

                }, CommandType.StoredProcedure, new TimeSpan(0, 30, 0))
                ).ConvertData();

        }

        public Task<PageData<CalcPeriodListModel>> CalcProfitAndLossPeriods(string keyword, long? fromDate, long? toDate, int page, int? size)
        {
            return _calcPeriodService.GetList(EnumCalcPeriodType.CalcProfitAndLoss, keyword, fromDate, toDate, page, size);
        }

        public async Task<CalcProfitAndLossView> CalcProfitAndLossPeriodInfo(long calcPeriodId)
        {
            var info =  await _calcPeriodService.GetInfo(EnumCalcPeriodType.CalcProfitAndLoss, calcPeriodId);
            return new CalcProfitAndLossView()
            {
                CalcPeriodInfo = info,
                FilterData = info.FilterData.JsonDeserialize<CalcProfitAndLossInput>(),
                OutputData = info.Data.JsonDeserialize<CalcProfitAndLossTableOutput>()
            };
        }

        public async Task<CalcProfitAndLossTableOutput> CalcProfitAndLoss(CalcProfitAndLossInput req)
        {
            var fDate = req.FromDate.UnixToDateTime();
            var tDate = req.ToDate.UnixToDateTime();

            var priceSellInDirectlySum = new SqlParameter("@PriceSellInDirectlySum", SqlDbType.Decimal) { Direction = ParameterDirection.Output };
            var costSellInDirectlySum = new SqlParameter("@CostSellInDirectlySum", SqlDbType.Decimal) { Direction = ParameterDirection.Output };
            var costManagerSum = new SqlParameter("@CostManagerSum", SqlDbType.Decimal) { Direction = ParameterDirection.Output };

            if (!string.IsNullOrWhiteSpace(req.OrderCode))
            {
                req.IsByOrder = true;
            }
            if (!string.IsNullOrWhiteSpace(req.MaLsx))
            {
                req.IsByLsx = true;
            }

            var data = (await _accountancyDBContext.QueryDataTable(
                "asp_CalcProfitAndLoss",
                    new[] {
                    new SqlParameter("@IsByLsx", SqlDbType.Decimal){ Value = req.IsByLsx},
                    new SqlParameter("@IsByOrder", SqlDbType.Decimal){ Value = req.IsByOrder},

                    new SqlParameter("@ProductId", SqlDbType.Int){ Value = req.ProductId.HasValue?req.ProductId.Value: (object)DBNull.Value},
                    new SqlParameter("@OrderCode", SqlDbType.NVarChar){ Value = !string.IsNullOrWhiteSpace(req.OrderCode) ?req.OrderCode.Trim(): (object)DBNull.Value},
                    new SqlParameter("@MaLsx", SqlDbType.NVarChar){ Value = !string.IsNullOrWhiteSpace(req.MaLsx) ?req.MaLsx.Trim(): (object)DBNull.Value},

                    new SqlParameter("@FromDate", SqlDbType.DateTime2){ Value = fDate},
                    new SqlParameter("@ToDate", SqlDbType.DateTime2){ Value = tDate},


                    req.Custom_AllocationRate.ToDecimalKeyValueSqlParameter("@Custom_AllocationRate"),
                    req.Custom_PriceSellDirectly.ToDecimalKeyValueSqlParameter("@Custom_PriceSellDirectly"),
                    req.Custom_CostSellDirectly.ToDecimalKeyValueSqlParameter("@Custom_CostSellDirectly"),
                    req.Custom_CostManagerDirectly.ToDecimalKeyValueSqlParameter("@Custom_CostManagerDirectly"),
                    req.Custom_OtherFee.ToDecimalKeyValueSqlParameter("@Custom_OtherFee"),

                    new SqlParameter("@PriceSellInDirectlyAllocationTypeId", SqlDbType.Int){ Value = req.PriceSellInDirectlyAllocationTypeId},
                    req.PriceSellInDirectlySumCustom.ToSqlParameterValue("@PriceSellInDirectlySumCustom"),
                    priceSellInDirectlySum,

                    new SqlParameter("@CostSellInDirectlyAllocationTypeId", SqlDbType.Int){ Value = req.CostSellInDirectlyAllocationTypeId},
                    req.CostSellInDirectlySumCustom.ToSqlParameterValue("@CostSellInDirectlySumCustom"),
                    costSellInDirectlySum,

                    new SqlParameter("@CostManagerAllowcationAllocationTypeId", SqlDbType.Int){ Value = req.CostManagerAllowcationAllocationTypeId},
                    req.CostManagerSumCustom.ToSqlParameterValue("@CostManagerSumCustom"),
                    costManagerSum,

                }, CommandType.StoredProcedure, new TimeSpan(0, 30, 0))
                ).ConvertData();

            var result = new CalcProfitAndLossTableOutput()
            {
                Data = data,
                PriceSellInDirectlySum = priceSellInDirectlySum.Value as decimal?,
                CostSellInDirectlySum = costSellInDirectlySum.Value as decimal?,
                CostManagerSum = costManagerSum.Value as decimal?
            };

            if (req.IsSave)
            {
                var calcPeriodId = await _calcPeriodService.Create(EnumCalcPeriodType.CalcProfitAndLoss, req.Title, req.Descirption, req.FromDate, req.ToDate, req, result);
                result.CalcPeriodId = calcPeriodId;
            }

            return result;
        }

    }
}
