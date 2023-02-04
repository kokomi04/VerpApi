﻿using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
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
    public class CalcProductPricePrivateService : CalcProductPriceServiceBase, ICalcProductPricePrivateService
    {
        public CalcProductPricePrivateService(AccountancyDBPrivateContext accountancyDBContext, ICalcPeriodPrivateService calcPeriodService, IInputDataPrivateService inputDataPrivateService) : base(accountancyDBContext, calcPeriodService, inputDataPrivateService) { }

    }

    public class CalcProductPricePublicService : CalcProductPriceServiceBase, ICalcProductPricePublicService
    {
        public CalcProductPricePublicService(AccountancyDBPublicContext accountancyDBContext, ICalcPeriodPublicService calcPeriodService, IInputDataPublicService inputDataPublicService) : base(accountancyDBContext, calcPeriodService, inputDataPublicService)
        {
        }
    }

    public abstract class CalcProductPriceServiceBase : ICalcProductPriceServiceBase
    {

        private readonly AccountancyDBContext _accountancyDBContext;
        private readonly ICalcPeriodServiceBase _calcPeriodService;
        private readonly IInputDataServiceBase _inputDataService;

        public CalcProductPriceServiceBase(AccountancyDBContext accountancyDBContext, ICalcPeriodServiceBase calcPeriodService, IInputDataServiceBase inputDataService)
        {
            _accountancyDBContext = accountancyDBContext;
            _calcPeriodService = calcPeriodService;
            _inputDataService = inputDataService;
        }

        public async Task<CalcProductPriceGetTableOutput> CalcProductPriceTable(CalcProductPriceGetTableInput req)
        {
            var fDate = req.FromDate.UnixToDateTime();
            var tDate = req.ToDate.UnixToDateTime();

            await _inputDataService.ValidateAccountantConfig(fDate, null);

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

            var dataSet = await _accountancyDBContext.QueryMultiDataTable(
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

                }, CommandType.StoredProcedure, new TimeSpan(0, 30, 0));

            IList<NonCamelCaseDictionary> data = null;
            IList<NonCamelCaseDictionary> resultData = null;

            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                data = dataSet.Tables[0].ConvertData();
                if (dataSet.Tables.Count > 1)
                {
                    resultData = dataSet.Tables[1].ConvertData();
                }
            }

            var result = new CalcProductPriceGetTableOutput()
            {
                Data = data,
                Result = resultData,
                IndirectMaterialFeeSum = indirectMaterialFeeSum.Value as decimal?,
                IndirectLaborFeeSum = indirectLaborFeeSum.Value as decimal?,
                GeneralManufacturingSum = generalManufacturingSum.Value as decimal?
            };

            if (req.IsSave)
            {
                var calcPeriodId = await _calcPeriodService.Create(EnumCalcPeriodType.CalcProductPrice, req.Title, req.Descirption, req.FromDate, req.ToDate, req, result);
                result.CalcPeriodId = calcPeriodId;
            }
            return result;
        }


        public async Task<CalcProductOutputPriceModel> CalcProductOutputPrice(CalcProductOutputPriceInput req)
        {
            var fDate = req.FromDate.UnixToDateTime();
            var tDate = req.ToDate.UnixToDateTime();

            await _inputDataService.ValidateAccountantConfig(fDate, null);


            var isInvalid = new SqlParameter("@IsInvalid", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var isError = new SqlParameter("@IsError", SqlDbType.Bit) { Direction = ParameterDirection.Output };

            var data = (await _accountancyDBContext.QueryDataTable(
                "asp_CalcProduct_OutputPrice",
                    new[] {

                    new SqlParameter("@FromDate", SqlDbType.DateTime2){ Value = fDate},
                    new SqlParameter("@ToDate", SqlDbType.DateTime2){ Value = tDate},
                    new SqlParameter("@ProductId", SqlDbType.Int){ Value = req.ProductId.HasValue?req.ProductId.Value: (object)DBNull.Value},
                    new SqlParameter("@Tk", SqlDbType.NVarChar){ Value = req.Tk},
                    new SqlParameter("@IsIgnoreZeroPrice", SqlDbType.Bit){ Value = req.IsIgnoreZeroPrice},
                    new SqlParameter("@IsUpdate", SqlDbType.Bit){ Value = req.IsUpdate},
                    isInvalid,
                    isError

                }, CommandType.StoredProcedure, new TimeSpan(0, 30, 0))
                ).ConvertData();

            return new CalcProductOutputPriceModel
            {
                Data = data,
                IsInvalid = (isInvalid.Value as bool?).GetValueOrDefault(),
                IsError = (isError.Value as bool?).GetValueOrDefault(),
            };
        }

        public async Task<IList<NonCamelCaseDictionary>> GetWeightedAverageProductPrice(CalcProductPriceInput req)
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

        public async Task<IList<NonCamelCaseDictionary>> GetProductPriceBuyLastest(CalcProductPriceInput req)
        {
            return (
                await _accountancyDBContext.QueryDataTable(
                "usp_CalcProductPrice_BuyLastest",
                 new[] {
                    new SqlParameter("@Date", SqlDbType.DateTime2){ Value = req.Date.UnixToDateTime()},
                    req.ProductIds.ToSqlParameter("@ProductIds")

                }, CommandType.StoredProcedure, new TimeSpan(0, 30, 0))
                ).ConvertData();

        }


        public async Task<CalcProfitAndLossTableOutput> CalcProfitAndLoss(CalcProfitAndLossInput req)
        {
            var fDate = req.FromDate.UnixToDateTime();
            var tDate = req.ToDate.UnixToDateTime();

            
            var priceSellInDirectlySum = new SqlParameter("@PriceSellInDirectlySum", SqlDbType.Decimal) { Direction = ParameterDirection.Output };
            var costAccountingSum = new SqlParameter("@CostAccountingSum", SqlDbType.Decimal) { Direction = ParameterDirection.Output };
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

                    new SqlParameter("@CostAccountingAllocationTypeId", SqlDbType.Int){ Value = req.CostAccountingAllocationTypeId},
                    req.CostAccountingSumCustom.ToSqlParameterValue("@CostAccountingSumCustom"),
                    costAccountingSum,

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
                CostAccountingSum = costAccountingSum.Value as decimal?,
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
