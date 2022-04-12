﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Services.Stock.Model.Inventory;
using Verp.Resources.Stock.Inventory;
using VErp.Commons.Library.Formaters;
using static Verp.Resources.Stock.Inventory.Abstract.InventoryAbstractMessage;
using VErp.Services.Stock.Service.Inventory.Implement.Abstract;
using static VErp.Commons.GlobalObject.QueueName.ManufacturingQueueNameConstants;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public abstract class InventoryServiceAbstract : InventoryBillDateAbstract
    {
        protected readonly ILogger _logger;
        protected readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IProductionOrderHelperService _productionOrderHelperService;
        private readonly IProductionHandoverHelperService _productionHandoverHelperService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IQueueProcessHelperService _queueProcessHelperService;

        internal InventoryServiceAbstract(StockDBContext stockContext
            , ILogger logger
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IProductionOrderHelperService productionOrderHelperService
            , IProductionHandoverHelperService productionHandoveHelperService
            , ICurrentContextService currentContextService
            , IQueueProcessHelperService queueProcessHelperService
            ) : base(stockContext)
        {
            _logger = logger;
            _customGenCodeHelperService = customGenCodeHelperService;
            _productionOrderHelperService = productionOrderHelperService;
            _productionHandoverHelperService = productionHandoveHelperService;
            _currentContextService = currentContextService;
            _queueProcessHelperService = queueProcessHelperService;
        }


        protected async Task<GenerateCodeContext> GenerateInventoryCode(EnumInventoryType inventoryTypeId, InventoryModelBase req, Dictionary<string, int> baseValueChains = null)
        {
            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext(baseValueChains);

            var objectTypeId = inventoryTypeId == EnumInventoryType.Input ? EnumObjectType.InventoryInput : EnumObjectType.InventoryOutput;
            var code = await ctx
                .SetConfig(objectTypeId, EnumObjectType.Stock, req.StockId)
                .SetConfigData(0, req.Date)
                .TryValidateAndGenerateCode(_stockDbContext.Inventory, req.InventoryCode, (s, code) => s.InventoryTypeId == (int)inventoryTypeId && s.InventoryCode == code);

            req.InventoryCode = code;
            return ctx;
        }


        protected decimal InputCalTotalMoney(IList<InventoryDetail> data)
        {
            var totalMoney = (decimal)0;
            foreach (var item in data)
            {
                totalMoney += (item.UnitPrice * item.PrimaryQuantity);
            }
            return totalMoney;
        }
        protected async Task<StockProduct> EnsureStockProduct(int stockId, int productId, int? productUnitConversionId)
        {
            var stockProductInfo = await _stockDbContext.StockProduct
                                .FirstOrDefaultAsync(s =>
                                                s.StockId == stockId
                                                && s.ProductId == productId
                                                && s.ProductUnitConversionId == productUnitConversionId
                                                );

            if (stockProductInfo == null)
            {
                stockProductInfo = new StockProduct()
                {
                    StockId = stockId,
                    ProductId = productId,
                    ProductUnitConversionId = productUnitConversionId,
                    PrimaryQuantityWaiting = 0,
                    PrimaryQuantityRemaining = 0,
                    ProductUnitConversionWaitting = 0,
                    ProductUnitConversionRemaining = 0,
                };
                await _stockDbContext.StockProduct.AddAsync(stockProductInfo);
                await _stockDbContext.SaveChangesAsync();
            }
            return stockProductInfo;
        }


        protected async Task ValidateInventoryCode(long? inventoryId, string inventoryCode)
        {
            inventoryId = inventoryId ?? 0;
            if (await _stockDbContext.Inventory.AnyAsync(q => q.InventoryId != inventoryId && q.InventoryCode == inventoryCode))
            {
                throw InventoryErrorCode.InventoryCodeAlreadyExisted.BadRequest();
            }
        }


        protected async Task ValidateInventoryConfig(DateTime? billDate, DateTime? oldDate)
        {
            await ValidateBill(billDate, oldDate);
        }

        /// <summary>
        /// Tính toán lại vết khi update phiếu nhập/xuất
        /// </summary>
        /// <param name="inventoryId"></param>
        protected async Task ReCalculateRemainingAfterUpdate(long inventoryId, long? effecttedFromInventoryId = null)
        {
            //var errorInventoryId = new SqlParameter("@ErrorInventoryId", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
            var errorIventoryDetailId = new SqlParameter("@ErrorIventoryDetailId", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

            // await _stockDbContext.Database.ExecuteSqlRawAsync("EXEC usp_InventoryDetail_UpdatePrimaryQuantityRemanings_Event @UpdatedInventoryId = @UpdatedInventoryId", new SqlParameter("@UpdatedInventoryId", inventoryId), errorInventoryId);

            await _stockDbContext.ExecuteNoneQueryProcedure("usp_InventoryDetail_UpdatePrimaryQuantityRemanings_Event",
                new[] {
                    new SqlParameter("@UpdatedInventoryId", inventoryId),
                    new SqlParameter("@EffecttedFromInventoryId", (object)effecttedFromInventoryId??DBNull.Value),
                    errorIventoryDetailId }
                );
            //var inventoryTrackingFacade = await InventoryTrackingFacadeFactory.Create(_stockDbContext, inventoryId);
            //await inventoryTrackingFacade.Execute();

            var inventoryDetailId = (errorIventoryDetailId.Value as long?).GetValueOrDefault();
            if (inventoryDetailId > 0)
            {
                var errorInfo = await (
                    from iv in _stockDbContext.Inventory
                    join id in _stockDbContext.InventoryDetail on iv.InventoryId equals id.InventoryId
                    join p in _stockDbContext.Product on id.ProductId equals p.ProductId into ps
                    from p in ps.DefaultIfEmpty()
                    where id.InventoryDetailId == inventoryDetailId
                    select new
                    {
                        iv.Date,
                        iv.InventoryId,
                        iv.InventoryCode,
                        iv.InventoryTypeId,
                        ProductCode = p == null ? null : p.ProductCode,
                        ProductId = p == null ? (int?)null : p.ProductId,
                        ProductName = p == null ? null : p.ProductName,

                        id.InventoryDetailId,
                        id.PrimaryQuantityRemaning
                    }).FirstOrDefaultAsync();
                if (errorInfo == null)
                {
                    throw BillDetailErrorUnknown.BadRequest();
                }
                else
                {

                    //var message = $"Số lượng \"{errorInfo.ProductCode}\" trong kho tại thời điểm {errorInfo.Date:dd-MM-yyyy} phiếu " +
                    //    $"{(errorInfo.InventoryTypeId == (int)EnumInventoryType.Input ? "Nhập" : "Xuất")} {errorInfo.InventoryCode} không đủ. Số tồn là " +
                    //   $"{errorInfo.PrimaryQuantityRemaning.Value.Format()} không hợp lệ";

                    var date = errorInfo.Date.AddMinutes(_currentContextService.TimeZoneOffset ?? 0);
                    var billInfo = errorInfo.InventoryTypeId == (int)EnumInventoryType.Input ? InventoryInput : InventoryOuput;
                    billInfo += " " + errorInfo.InventoryCode;

                    throw BillDetailError.BadRequestFormat(errorInfo.ProductCode, date.Format(), billInfo, errorInfo.PrimaryQuantityRemaning.Value.Format());

                }
            }

        }

        //protected async Task UpdateIgnoreAllocation(IList<InventoryDetail> inventoryDetails)
        //{
        //    try
        //    {
        //        var productionOrderCodes = inventoryDetails.Where(d => !string.IsNullOrEmpty(d.ProductionOrderCode)).Select(d => d.ProductionOrderCode).Distinct().ToArray();
        //        await _productionHandoverHelperService.UpdateIgnoreAllocation(productionOrderCodes);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "AutoIgnoreAllocation");
        //    }

        //}

        protected async Task UpdateProductionOrderStatus(IList<InventoryDetail> inventoryDetails, EnumProductionStatus status, string inventoryCode)
        {
            var productionOrderCodes = inventoryDetails.Where(d => !string.IsNullOrEmpty(d.ProductionOrderCode)).Select(d => d.ProductionOrderCode).Distinct().ToList();

            await ProductionOrderInventory(productionOrderCodes, status, inventoryCode);

            /*
            var errorProductionOrderCode = "";
            try
            {
                // update trạng thái cho lệnh sản xuất
                var productionOrderCodes = inventoryDetails.Where(d => !string.IsNullOrEmpty(d.ProductionOrderCode)).Select(d => d.ProductionOrderCode).Distinct().ToList();

                Dictionary<string, DataTable> inventoryMap = new Dictionary<string, DataTable>();
                foreach (var productionOrderCode in productionOrderCodes)
                {
                    errorProductionOrderCode = productionOrderCode;
                    var parammeters = new SqlParameter[]
                    {
                        new SqlParameter("@ProductionOrderCode", productionOrderCode)
                    };
                    var resultData = await _stockDbContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByProductionOrder", parammeters);
                    inventoryMap.Add(productionOrderCode, resultData);
                    await _productionOrderHelperService.UpdateProductionOrderStatus(productionOrderCode, resultData, status);
                    await _productionHandoverHelperService.ChangeAssignedProgressStatus(productionOrderCode, inventoryCode, inventoryMap[productionOrderCode]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, UpdateProductionOrderStatusError);
                throw new Exception(string.Format(UpdateProductionOrderStatusError, errorProductionOrderCode) + ": " + ex.Message, ex);
            }
            */

        }

        public async Task ProductionOrderInventory(IList<string> productionOrderCodes, EnumProductionStatus status, string inventoryCode)
        {
            var errorProductionOrderCode = "";
            try
            {
                Dictionary<string, DataTable> inventoryMap = new Dictionary<string, DataTable>();
                foreach (var productionOrderCode in productionOrderCodes)
                {
                    errorProductionOrderCode = productionOrderCode;
                    var parammeters = new SqlParameter[]
                    {
                        new SqlParameter("@ProductionOrderCode", productionOrderCode)
                    };
                    var resultData = await _stockDbContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByProductionOrder", parammeters);

                    var inventories = resultData.ConvertData<InternalProductionInventoryRequirementModel>();

                    var data = new
                    {
                        ProductionOrderCode = productionOrderCode,
                        InventoryCode = inventoryCode,
                        ProductionOrderStatus = status,
                        Inventories = inventories
                    };

                    await _queueProcessHelperService.EnqueueAsync(PRODUCTION_INVENTORY_APPROVED, data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, UpdateProductionOrderStatusError);
                throw new Exception(string.Format(UpdateProductionOrderStatusError, errorProductionOrderCode) + ": " + ex.Message, ex);
            }
        }

        protected void ValidatePackage(Package package)
        {

            if (package.PrimaryQuantityWaiting < 0) throw new Exception("Package Negative PrimaryQuantityWaiting! " + package.PackageId);

            if (package.PrimaryQuantityRemaining < 0) throw new Exception("Package Negative PrimaryQuantityRemaining! " + package.PackageId);

            if (package.ProductUnitConversionWaitting < 0) throw new Exception("Package Negative ProductUnitConversionWaitting! " + package.PackageId);

            if (package.ProductUnitConversionRemaining < 0)
            {
                throw new Exception("Package Negative ProductUnitConversionRemaining! " + package.PackageId);
            }


        }

        protected void ValidateStockProduct(StockProduct stockProduct)
        {

            if (stockProduct.PrimaryQuantityWaiting < 0) throw new Exception("Stock Negative PrimaryQuantityWaiting! " + stockProduct.StockProductId);

            if (stockProduct.PrimaryQuantityRemaining < 0) throw new Exception("Stock Negative PrimaryQuantityRemaining! " + stockProduct.StockProductId);

            if (stockProduct.ProductUnitConversionWaitting < 0) throw new Exception("Stock Negative ProductUnitConversionWaitting! " + stockProduct.StockProductId);

            if (stockProduct.ProductUnitConversionRemaining < 0) throw new Exception("Stock Negative ProductUnitConversionRemaining! " + stockProduct.StockProductId);

        }



    }
}
