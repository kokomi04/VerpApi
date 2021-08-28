using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.FileResources;
using PackageEntity = VErp.Infrastructure.EF.StockDB.Package;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Services.Stock.Service.Products;
using VErp.Commons.Library.Model;
using System.Data;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.ServiceCore.Facade;
using static VErp.Services.Stock.Service.Resources.InventoryProcess.InventoryBillInputMessage;
using VErp.Services.Stock.Service.Resources.InventoryProcess;
using InventoryEntity = VErp.Infrastructure.EF.StockDB.Inventory;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public partial class InventoryBillInputService : InventoryServiceAbstract, IInventoryBillInputService
    {
        //const decimal MINIMUM_JS_NUMBER = Numbers.MINIMUM_ACCEPT_DECIMAL_NUMBER;

        
        private readonly IAsyncRunnerService _asyncRunner;
        private readonly ICurrentContextService _currentContextService;
        private readonly IProductService _productService;
        private readonly ObjectActivityLogFacade _invInputActivityLog;
        private readonly ObjectActivityLogFacade _packageActivityLog;

        public InventoryBillInputService(
            StockDBContext stockContext
            , ILogger<InventoryService> logger
            , IActivityLogService activityLogService
            , IAsyncRunnerService asyncRunner
            , ICurrentContextService currentContextService
            , IProductService productService
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IProductionOrderHelperService productionOrderHelperService
            , IProductionHandoverService productionHandoverService
            ) : base(stockContext, logger, customGenCodeHelperService, productionOrderHelperService, productionHandoverService, currentContextService)
        {
         
            _asyncRunner = asyncRunner;
            _currentContextService = currentContextService;
            _productService = productService;
            _invInputActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.InventoryInput);

            _packageActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Package); ;
        }


        public ObjectActivityLogModelBuilder<string> ImportedLogBuilder()
        {
            return _invInputActivityLog.LogBuilder(() => InventoryBillInputActivityLogMessage.Import);
        }

        /// <summary>
        /// Thêm mới phiếu nhập kho
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<long> AddInventoryInput(InventoryInModel req)
        {
            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(req.StockId)))
            {
                var ctx = await GenerateInventoryCode(EnumInventoryType.Input, req);

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    var inventoryId = await AddInventoryInputDB(req);
                    await trans.CommitAsync();

                    await ctx.ConfirmCode();

                    await _invInputActivityLog.LogBuilder(() => InventoryBillInputActivityLogMessage.Create)
                       .MessageResourceFormatDatas(req.InventoryCode)
                       .ObjectId(inventoryId)
                       .JsonData(req.JsonSerialize())
                       .CreateLog();


                    return inventoryId;
                }
            }

        }

        public async Task<long> AddInventoryInputDB(InventoryInModel req)
        {
            if (req == null || req.InProducts.Count == 0)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            await ValidateInventoryConfig(req.Date.UnixToDateTime(), null);

            req.InventoryCode = req.InventoryCode.Trim();

            var stockInfo = await _stockDbContext.Stock.AsNoTracking().FirstOrDefaultAsync(s => s.StockId == req.StockId);
            if (stockInfo == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest(StockInfoNotFound);
            }

            //using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(req.StockId)))
            {

                await ValidateInventoryCode(null, req.InventoryCode);

                var issuedDate = req.Date.UnixToDateTime().Value;

                var validInventoryDetails = await ValidateInventoryIn(false, req);

                if (!validInventoryDetails.Code.IsSuccess())
                {
                    throw new BadRequestException(validInventoryDetails.Code);
                }

                var totalMoney = InputCalTotalMoney(validInventoryDetails.Data);

                var inventoryObj = new InventoryEntity
                {
                    StockId = req.StockId,
                    InventoryCode = req.InventoryCode,
                    InventoryTypeId = (int)EnumInventoryType.Input,
                    Shipper = req.Shipper,
                    Content = req.Content,
                    Date = issuedDate,
                    CustomerId = req.CustomerId,
                    Department = req.Department,
                    StockKeeperUserId = req.StockKeeperUserId,
                    BillForm = req.BillForm,
                    BillCode = req.BillCode,
                    BillSerial = req.BillSerial,
                    BillDate = req.BillDate?.UnixToDateTime(),
                    TotalMoney = totalMoney,
                    AccountancyAccountNumber = req.AccountancyAccountNumber,
                    CreatedByUserId = _currentContextService.UserId,
                    UpdatedByUserId = _currentContextService.UserId,
                    IsApproved = false,
                    DepartmentId = req.DepartmentId
                };
                await _stockDbContext.AddAsync(inventoryObj);
                await _stockDbContext.SaveChangesAsync();

                // Thêm danh sách file đính kèm vào phiếu nhập | xuất
                if (req.FileIdList != null && req.FileIdList.Count > 0)
                {
                    var attachedFiles = new List<InventoryFile>(req.FileIdList.Count);
                    attachedFiles.AddRange(req.FileIdList.Select(fileId => new InventoryFile() { FileId = fileId, InventoryId = inventoryObj.InventoryId }));
                    await _stockDbContext.AddRangeAsync(attachedFiles);
                    await _stockDbContext.SaveChangesAsync();
                }

                foreach (var item in validInventoryDetails.Data)
                {
                    item.InventoryId = inventoryObj.InventoryId;
                }
                inventoryObj.TotalMoney = totalMoney;

                await _stockDbContext.InventoryDetail.AddRangeAsync(validInventoryDetails.Data);
                await _stockDbContext.SaveChangesAsync();

                //Move file from tmp folder
                if (req.FileIdList != null)
                {
                    foreach (var fileId in req.FileIdList)
                    {
                        _asyncRunner.RunAsync<IFileService>(f => f.FileAssignToObject(EnumObjectType.InventoryInput, inventoryObj.InventoryId, fileId));
                    }
                }
                return inventoryObj.InventoryId;
            }
        }





        /// <summary>
        /// Cập nhật phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> UpdateInventoryInput(long inventoryId, InventoryInModel req)
        {
            if (inventoryId <= 0)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }

            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(req.StockId)))
            {

                var issuedDate = req.Date.UnixToDateTime().Value;

                var validate = await ValidateInventoryIn(false, req);

                await ValidateInventoryCode(inventoryId, req.InventoryCode);

                if (!validate.Code.IsSuccess())
                {
                    throw new BadRequestException(validate.Code);
                }

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        #region Update Inventory - Phiếu nhập kho
                        var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);
                        if (inventoryObj == null)
                        {
                            trans.Rollback();
                            throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
                        }

                        if (inventoryObj.StockId != req.StockId)
                        {
                            trans.Rollback();
                            throw new BadRequestException(InventoryErrorCode.CanNotChangeStock);
                        }

                        if (inventoryObj.IsApproved)
                        {
                            trans.Rollback();
                            throw new BadRequestException(GeneralCode.NotYetSupported);
                        }

                        if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Input)
                        {
                            throw new BadRequestException(GeneralCode.InvalidParams);
                        }

                        await ValidateInventoryConfig(req.Date.UnixToDateTime(), inventoryObj.Date);

                        #endregion

                        var inventoryDetails = await _stockDbContext.InventoryDetail.Where(d => d.InventoryId == inventoryId).ToListAsync();
                        foreach (var d in inventoryDetails)
                        {
                            d.IsDeleted = true;
                            d.UpdatedDatetimeUtc = DateTime.UtcNow;
                        }

                        foreach (var item in validate.Data)
                        {
                            item.InventoryId = inventoryObj.InventoryId;
                        }

                        InventoryInputUpdateData(inventoryObj, req, InputCalTotalMoney(validate.Data));

                        await _stockDbContext.InventoryDetail.AddRangeAsync(validate.Data);

                        var files = await _stockDbContext.InventoryFile.Where(f => f.InventoryId == inventoryId).ToListAsync();

                        if (req.FileIdList != null && req.FileIdList.Count > 0)
                        {
                            foreach (var f in files)
                            {
                                if (!req.FileIdList.Contains(f.FileId))
                                    f.IsDeleted = true;
                            }

                            foreach (var newFileId in req.FileIdList)
                            {
                                if (!files.Select(q => q.FileId).ToList().Contains(newFileId))
                                    _stockDbContext.InventoryFile.Add(new InventoryFile()
                                    {
                                        InventoryId = inventoryId,
                                        FileId = newFileId,
                                        IsDeleted = false
                                    });
                            }
                        }

                        await _stockDbContext.SaveChangesAsync();
                        trans.Commit();

                        await _invInputActivityLog.LogBuilder(() => InventoryBillInputActivityLogMessage.Update)
                            .MessageResourceFormatDatas(req.InventoryCode)
                            .ObjectId(inventoryId)
                            .JsonData(req.JsonSerialize())
                            .CreateLog();
                    }
                    catch (Exception ex)
                    {
                        trans.TryRollbackTransaction();
                        _logger.LogError(ex, "UpdateInventoryInput");
                        throw;
                    }
                }

                //Move file from tmp folder
                if (req.FileIdList != null)
                {
                    foreach (var fileId in req.FileIdList)
                    {
                        _asyncRunner.RunAsync<IFileService>(f => f.FileAssignToObject(EnumObjectType.InventoryInput, inventoryId, fileId));
                    }
                }

                return true;
            }
        }

        protected void InventoryInputUpdateData(InventoryEntity inventoryObj, InventoryInModel req, decimal totalMoney)
        {
            var issuedDate = req.Date.UnixToDateTime().Value;

            //inventoryObj.StockId = req.StockId; Khong cho phep sua kho
            inventoryObj.InventoryCode = req.InventoryCode;
            inventoryObj.Date = issuedDate;
            inventoryObj.Shipper = req.Shipper;
            inventoryObj.Content = req.Content;
            inventoryObj.CustomerId = req.CustomerId;
            inventoryObj.Department = req.Department;
            inventoryObj.StockKeeperUserId = req.StockKeeperUserId;
            inventoryObj.BillForm = req.BillForm;
            inventoryObj.BillCode = req.BillCode;
            inventoryObj.BillSerial = req.BillSerial;
            inventoryObj.BillDate = req.BillDate?.UnixToDateTime();
            inventoryObj.AccountancyAccountNumber = req.AccountancyAccountNumber;
            inventoryObj.UpdatedByUserId = _currentContextService.UserId;
            inventoryObj.TotalMoney = totalMoney;
            inventoryObj.DepartmentId = req.DepartmentId;
        }


        /// <summary>
        /// Xoá phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteInventoryInput(long inventoryId)
        {
            var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(p => p.InventoryId == inventoryId);
            if (inventoryObj == null)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }

            if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Input)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(inventoryObj.StockId)))
            {
                //reload inventory after lock
                inventoryObj = _stockDbContext.Inventory.FirstOrDefault(p => p.InventoryId == inventoryId);

                if (inventoryObj.IsApproved)
                {
                    /*Khong duoc phep xoa phieu nhap da duyet (Cần xóa theo lưu đồ, flow)*/
                    throw new BadRequestException(InventoryErrorCode.NotSupportedYet);

                    //var processResult = await RollBackInventoryInput(inventoryObj);
                    //if (!Equals(processResult, GeneralCode.Success))
                    //{
                    //    trans.Rollback();
                    //    return GeneralCode.InvalidParams;
                    //}
                }

                await ValidateInventoryConfig(null, inventoryObj.Date);

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        inventoryObj.IsDeleted = true;
                        //inventoryObj.IsApproved = false;

                        var inventoryDetails = await _stockDbContext.InventoryDetail.Where(iv => iv.InventoryId == inventoryId).ToListAsync();
                        foreach (var item in inventoryDetails)
                        {
                            item.IsDeleted = true;
                        }

                        await _stockDbContext.SaveChangesAsync();
                        trans.Commit();


                        await _invInputActivityLog.LogBuilder(() => InventoryBillInputActivityLogMessage.Delete)
                          .MessageResourceFormatDatas(inventoryObj.InventoryCode)
                          .ObjectId(inventoryId)
                          .JsonData(inventoryObj.JsonSerialize())
                          .CreateLog();

                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.TryRollbackTransaction();
                        _logger.LogError(ex, "DeleteInventoryInput");
                        throw;
                    }
                }
            }
        }



        /// <summary>
        /// Duyệt phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> ApproveInventoryInput(long inventoryId)
        {
            if (inventoryId < 0)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }

            var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);
            if (inventoryObj == null)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }
            if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Input)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }

            await ValidateInventoryConfig(inventoryObj.Date, inventoryObj.Date);

            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(inventoryObj.StockId)))
            {
                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        //reload after lock
                        inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);

                        if (inventoryObj.IsApproved)
                        {
                            trans.Rollback();
                            throw new BadRequestException(InventoryErrorCode.InventoryAlreadyApproved);
                        }

                        inventoryObj.IsApproved = true;
                        //inventoryObj.UpdatedByUserId = currentUserId;
                        //inventoryObj.UpdatedDatetimeUtc = DateTime.UtcNow;
                        inventoryObj.CensorByUserId = _currentContextService.UserId;
                        inventoryObj.CensorDatetimeUtc = DateTime.UtcNow;

                        await _stockDbContext.SaveChangesAsync();

                        var inventoryDetails = _stockDbContext.InventoryDetail.Where(q => q.InventoryId == inventoryId).ToList();

                        var r = await ProcessInventoryInputApprove(inventoryObj.StockId, inventoryObj.Date, inventoryDetails, inventoryObj.InventoryCode);

                        if (!r.IsSuccess())
                        {
                            trans.Rollback();
                            throw new BadRequestException(r);
                        }

                        await ReCalculateRemainingAfterUpdate(inventoryId);


                        await UpdateProductionOrderStatus(inventoryDetails, EnumProductionStatus.Finished);


                        trans.Commit();


                        await _invInputActivityLog.LogBuilder(() => InventoryBillInputActivityLogMessage.Approve)
                        .MessageResourceFormatDatas(inventoryObj.InventoryCode)
                        .ObjectId(inventoryId)
                        .JsonData(inventoryObj.JsonSerialize())
                        .CreateLog();


                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.TryRollbackTransaction();
                        _logger.LogError(ex, "ApproveInventoryInput");
                        throw;
                    }
                }
            }
        }


        /// <summary>
        /// Lấy danh sách sản phẩm để nhập kho
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="stockIdList"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public async Task<PageData<ProductListOutput>> GetProductListForImport(string keyword, IList<int> stockIdList, int page = 1, int size = 20)
        {
            var productList = await _productService.GetList(keyword, new int[0], "", new int[0], new int[0], page, size, null, null, null);

            var pagedData = productList.List;

            var productIdList = pagedData.Select(p => p.ProductId).ToList();

            var stockProductData = await _stockDbContext.StockProduct.AsNoTracking().Where(q => stockIdList.Contains(q.StockId)).Where(q => productIdList.Contains(q.ProductId)).ToListAsync();

            foreach (var item in pagedData)
            {
                item.StockProductModelList =
                    stockProductData.Where(q => q.ProductId == item.ProductId).Select(q => new StockProductOutput
                    {
                        StockId = q.StockId,
                        ProductId = q.ProductId,
                        PrimaryUnitId = item.UnitId,
                        PrimaryQuantityRemaining = q.PrimaryQuantityRemaining.RoundBy(),
                        ProductUnitConversionId = q.ProductUnitConversionId,
                        ProductUnitConversionRemaining = q.ProductUnitConversionRemaining.RoundBy()
                    }).ToList();
            }

            return productList;

        }


        #region Private helper method

        private async Task<Enum> ProcessInventoryInputApprove(int stockId, DateTime date, IList<InventoryDetail> inventoryDetails, string inventoryCode)
        {
            var inputTransfer = new List<InventoryDetailToPackage>();
            var billPackages = new List<PackageEntity>();

            foreach (var item in inventoryDetails.OrderBy(d => d.InventoryDetailId))
            {
                await UpdateStockProduct(stockId, item);

                if (item.PackageOptionId != null)
                    switch ((EnumPackageOption)item.PackageOptionId)
                    {
                        case EnumPackageOption.Append:
                            var appendResult = await AppendToCustomPackage(item);
                            if (!appendResult.IsSuccess())
                            {
                                return appendResult;
                            }

                            break;

                        case EnumPackageOption.NoPackageManager:
                            var defaultPackge = await AppendToDefaultPackage(stockId, date, item);
                            item.ToPackageId = defaultPackge.PackageId;

                            break;

                        case EnumPackageOption.Create:

                            var newPackage = await CreateNewPackage(stockId, date, item, inventoryCode);
                            item.ToPackageId = newPackage.PackageId;
                            break;


                        case EnumPackageOption.CreateMerge:

                            var packageInfo = billPackages
                                         .FirstOrDefault(p =>
                                             p.StockId == stockId
                                             && p.ProductId == item.ProductId
                                             && p.ProductUnitConversionId == item.ProductUnitConversionId
                                             && p.PackageTypeId == (int)EnumPackageType.Custom
                                             );
                            if (packageInfo == null)
                            {
                                var createPackage = await CreateNewPackage(stockId, date, item, inventoryCode);
                                item.ToPackageId = createPackage.PackageId;
                                billPackages.Add(createPackage);
                            }
                            else
                            {
                                item.ToPackageId = packageInfo.PackageId;
                                var mergeResult = await AppendToCustomPackage(item);
                                if (!mergeResult.IsSuccess())
                                {
                                    return mergeResult;
                                }
                            }
                            break;
                        default:
                            return GeneralCode.NotYetSupported;
                    }
                else
                {
                    var newPackage = await CreateNewPackage(stockId, date, item, inventoryCode);

                    item.ToPackageId = newPackage.PackageId;
                }

                inputTransfer.Add(new InventoryDetailToPackage()
                {
                    InventoryDetailId = item.InventoryDetailId,
                    ToPackageId = item.ToPackageId.Value,
                    IsDeleted = false
                });

            }

            await _stockDbContext.InventoryDetailToPackage.AddRangeAsync(inputTransfer);
            await _stockDbContext.SaveChangesAsync();

            return GeneralCode.Success;
        }


        private async Task<ServiceResult<IList<InventoryDetail>>> ValidateInventoryIn(bool isApproved, InventoryInModel req)
        {
            if (req.InProducts == null)
                req.InProducts = new List<InventoryInProductModel>();

            var productIds = req.InProducts.Select(p => p.ProductId).Distinct().ToList();

            var productInfos = (await _stockDbContext.Product.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToListAsync()).ToDictionary(p => p.ProductId, p => p);

            var productUnitConversions = await _stockDbContext.ProductUnitConversion.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToListAsync();

            var toPackageIds = req.InProducts.Select(p => p.ToPackageId).ToList();
            var toPackages = await _stockDbContext.Package.Where(p => toPackageIds.Contains(p.PackageId) && p.PackageTypeId == (int)EnumPackageType.Custom).ToListAsync();

            var inventoryDetailList = new List<InventoryDetail>(req.InProducts.Count);
            foreach (var details in req.InProducts)
            {
                productInfos.TryGetValue(details.ProductId, out var productInfo);
                if (productInfo == null)
                {
                    return ProductErrorCode.ProductNotFound;
                }
                var puDefault = productUnitConversions.FirstOrDefault(c => c.ProductId == details.ProductId && c.IsDefault);

                var puInfo = productUnitConversions.FirstOrDefault(c => c.ProductUnitConversionId == details.ProductUnitConversionId);
                if (puInfo == null)
                {
                    return ProductUnitConversionErrorCode.ProductUnitConversionNotFound;
                }
                if (puInfo.ProductId != details.ProductId)
                {
                    return ProductUnitConversionErrorCode.ProductUnitConversionNotBelongToProduct;
                }

                if ((puInfo.IsFreeStyle ?? false) == false)
                {


                    var calcModel = new QuantityPairInputModel()
                    {
                        PrimaryQuantity = details.PrimaryQuantity,
                        PrimaryDecimalPlace = puDefault?.DecimalPlace ?? 12,

                        PuQuantity = details.ProductUnitConversionQuantity,
                        PuDecimalPlace = puInfo.DecimalPlace,

                        FactorExpression = puInfo.FactorExpression,

                        FactorExpressionRate = null
                    };


                    //  var (isSuccess, pucQuantity) = Utils.GetProductUnitConversionQuantityFromPrimaryQuantity(details.PrimaryQuantity, puInfo.FactorExpression, details.ProductUnitConversionQuantity, puInfo.DecimalPlace);

                    var (isSuccess, primaryQuantity, pucQuantity) = Utils.GetProductUnitConversionQuantityFromPrimaryQuantity(calcModel);

                    if (isSuccess)
                    {
                        details.ProductUnitConversionQuantity = pucQuantity;
                    }
                    else
                    {
                        _logger.LogWarning($"Wrong pucQuantity input data: PrimaryQuantity={details.PrimaryQuantity}, FactorExpression={puInfo.FactorExpression}, ProductUnitConversionQuantity={details.ProductUnitConversionQuantity}, evalData={pucQuantity}");
                        //return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                        throw PuConversionError.BadRequestFormat(puInfo.ProductUnitConversionName, productInfo.ProductCode);
                    }
                }

                if (!isApproved && details.ProductUnitConversionQuantity <= 0)
                {
                    //return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                    throw PuConversionError.BadRequestFormat(puInfo.ProductUnitConversionName, productInfo.ProductCode);
                }

                // }

                if (!isApproved)
                {
                    if (details.ProductUnitConversionQuantity <= 0 || details.PrimaryQuantity <= 0)
                    {
                        return GeneralCode.InvalidParams;
                    }
                }

                switch (details.PackageOptionId)
                {
                    case EnumPackageOption.Append:

                        var toPackageInfo = toPackages.FirstOrDefault(p => p.PackageId == details.ToPackageId);
                        if (toPackageInfo == null) return PackageErrorCode.PackageNotFound;

                        if (toPackageInfo.ProductId != details.ProductId
                            || toPackageInfo.ProductUnitConversionId != details.ProductUnitConversionId
                            || toPackageInfo.StockId != req.StockId)
                        {
                            return InventoryErrorCode.InvalidPackage;
                        }
                        break;
                    case EnumPackageOption.Create:
                    case EnumPackageOption.CreateMerge:
                    case EnumPackageOption.NoPackageManager:

                        if (!isApproved && details.ToPackageId.HasValue)
                        {
                            return GeneralCode.InvalidParams;
                        }
                        break;
                }

                inventoryDetailList.Add(new InventoryDetail
                {
                    InventoryDetailId = isApproved ? details.InventoryDetailId ?? 0 : 0,
                    ProductId = details.ProductId,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    RequestPrimaryQuantity = details.RequestPrimaryQuantity?.RoundBy(puDefault.DecimalPlace),
                    PrimaryQuantity = details.PrimaryQuantity.RoundBy(puDefault.DecimalPlace),
                    UnitPrice = details.UnitPrice.RoundBy(puDefault.DecimalPlace),
                    ProductUnitConversionId = details.ProductUnitConversionId,
                    RequestProductUnitConversionQuantity = details.RequestProductUnitConversionQuantity?.RoundBy(puInfo.DecimalPlace),
                    ProductUnitConversionQuantity = details.ProductUnitConversionQuantity.RoundBy(puInfo.DecimalPlace),
                    ProductUnitConversionPrice = details.ProductUnitConversionPrice.RoundBy(puInfo.DecimalPlace),
                    RefObjectTypeId = details.RefObjectTypeId,
                    RefObjectId = details.RefObjectId,
                    RefObjectCode = details.RefObjectCode,
                    OrderCode = details.OrderCode,
                    Pocode = details.POCode,
                    ProductionOrderCode = details.ProductionOrderCode,
                    FromPackageId = null,
                    ToPackageId = details.ToPackageId,
                    PackageOptionId = (int)details.PackageOptionId,
                    SortOrder = details.SortOrder,
                    Description = details.Description,
                    AccountancyAccountNumberDu = details.AccountancyAccountNumberDu,
                    InventoryRequirementCode = details.InventoryRequirementCode
                });
            }
            return inventoryDetailList;
        }



        private async Task UpdateStockProduct(int stockId, InventoryDetail detail, EnumInventoryType type = EnumInventoryType.Input)
        {
            var stockProductInfo = await EnsureStockProduct(stockId, detail.ProductId, detail.ProductUnitConversionId);
            switch (type)
            {
                case EnumInventoryType.Input:
                    {
                        stockProductInfo.PrimaryQuantityRemaining = stockProductInfo.PrimaryQuantityRemaining.AddDecimal(detail.PrimaryQuantity);
                        stockProductInfo.ProductUnitConversionRemaining = stockProductInfo.ProductUnitConversionRemaining.AddDecimal(detail.ProductUnitConversionQuantity);
                        break;
                    }
                case EnumInventoryType.Output:
                    {
                        stockProductInfo.PrimaryQuantityRemaining = stockProductInfo.PrimaryQuantityRemaining.SubDecimal(detail.PrimaryQuantity);
                        stockProductInfo.ProductUnitConversionRemaining = stockProductInfo.ProductUnitConversionRemaining.SubDecimal(detail.ProductUnitConversionQuantity);
                        break;
                    }
                default:
                    break;
            }
        }

        private async Task<Enum> AppendToCustomPackage(InventoryDetail detail)
        {
            var packageInfo = await _stockDbContext.Package.FirstOrDefaultAsync(p => p.PackageId == detail.ToPackageId && p.PackageTypeId == (int)EnumPackageType.Custom);
            if (packageInfo == null) return PackageErrorCode.PackageNotFound;

            //packageInfo.PrimaryQuantity += detail.PrimaryQuantity;
            packageInfo.PrimaryQuantityRemaining = packageInfo.PrimaryQuantityRemaining.AddDecimal(detail.PrimaryQuantity);
            //packageInfo.ProductUnitConversionQuantity += detail.ProductUnitConversionQuantity;
            packageInfo.ProductUnitConversionRemaining = packageInfo.ProductUnitConversionRemaining.AddDecimal(detail.ProductUnitConversionQuantity);
            return GeneralCode.Success;
        }


        private async Task<PackageEntity> AppendToDefaultPackage(int stockId, DateTime billDate, InventoryDetail detail)
        {
            var ensureDefaultPackage = await _stockDbContext.Package
                                          .FirstOrDefaultAsync(p =>
                                              p.StockId == stockId
                                              && p.ProductId == detail.ProductId
                                              && p.ProductUnitConversionId == detail.ProductUnitConversionId
                                              && p.PackageTypeId == (int)EnumPackageType.Default
                                              );

            if (ensureDefaultPackage == null)
            {
                ensureDefaultPackage = new Package()
                {

                    PackageTypeId = (int)EnumPackageType.Default,
                    PackageCode = "",
                    LocationId = null,
                    StockId = stockId,
                    ProductId = detail.ProductId,
                    //PrimaryQuantity = 0,
                    ProductUnitConversionId = detail.ProductUnitConversionId,
                    //ProductUnitConversionQuantity = 0,
                    PrimaryQuantityWaiting = 0,
                    PrimaryQuantityRemaining = 0,
                    ProductUnitConversionWaitting = 0,
                    ProductUnitConversionRemaining = 0,
                    Date = billDate,
                    ExpiryTime = null,
                };

                await _stockDbContext.Package.AddAsync(ensureDefaultPackage);
            }

            //ensureDefaultPackage.PrimaryQuantity += detail.PrimaryQuantity;
            ensureDefaultPackage.PrimaryQuantityRemaining = ensureDefaultPackage.PrimaryQuantityRemaining.AddDecimal(detail.PrimaryQuantity);
            //ensureDefaultPackage.ProductUnitConversionQuantity += detail.ProductUnitConversionQuantity;
            ensureDefaultPackage.ProductUnitConversionRemaining = ensureDefaultPackage.ProductUnitConversionRemaining.AddDecimal(detail.ProductUnitConversionQuantity);

            await _stockDbContext.SaveChangesAsync();

            return ensureDefaultPackage;
        }

        private async Task<PackageEntity> CreateNewPackage(int stockId, DateTime date, InventoryDetail detail, string inventoryCode)
        {
            var config = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.Package, EnumObjectType.Package, 0, null, null, date.GetUnix());

            var newPackageCodeResult = await _customGenCodeHelperService.GenerateCode(config.CustomGenCodeId, config.CurrentLastValue.LastValue, null, null, date.GetUnix());

            var newPackage = new Package()
            {
                PackageTypeId = (int)EnumPackageType.Custom,
                PackageCode = newPackageCodeResult?.CustomCode,
                LocationId = null,
                StockId = stockId,
                ProductId = detail.ProductId,
                //PrimaryQuantity = detail.PrimaryQuantity,
                ProductUnitConversionId = detail.ProductUnitConversionId,
                //ProductUnitConversionQuantity = detail.ProductUnitConversionQuantity,
                PrimaryQuantityWaiting = 0,
                PrimaryQuantityRemaining = detail.PrimaryQuantity,
                ProductUnitConversionWaitting = 0,
                ProductUnitConversionRemaining = detail.ProductUnitConversionQuantity,
                Date = date,
                ExpiryTime = null,
                Description = inventoryCode
            };
            await _stockDbContext.Package.AddAsync(newPackage);
            await _stockDbContext.SaveChangesAsync();

            await _customGenCodeHelperService.ConfirmCode(config.CurrentLastValue);

            await _packageActivityLog.LogBuilder(() => InventoryBillInputActivityLogMessage.CreatePackage)
               .MessageResourceFormatDatas(newPackage.PackageCode, inventoryCode)
               .ObjectId(newPackage.PackageId)
               .JsonData(newPackage.JsonSerialize())
               .CreateLog();


            return newPackage;
        }


        //private async Task<ServiceResult> ValidateBalanceForOutput(int stockId, int productId, long currentInventoryId, int productUnitConversionId, DateTime endDate, decimal outPrimary, decimal outSecondary)
        //{
        //    var sums = await (
        //        from id in _stockDbContext.InventoryDetail
        //        join iv in _stockDbContext.Inventory on id.InventoryId equals iv.InventoryId
        //        where iv.StockId == stockId
        //        && id.ProductId == productId
        //        && id.ProductUnitConversionId == productUnitConversionId
        //        && iv.Date <= endDate
        //        && iv.IsApproved
        //        && iv.InventoryId != currentInventoryId
        //        select new
        //        {
        //            iv.InventoryTypeId,
        //            id.PrimaryQuantity,
        //            id.ProductUnitConversionQuantity
        //        }).GroupBy(g => true)
        //           .Select(g => new
        //           {
        //               TotalPrimary = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity),
        //               TotalSecondary = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.ProductUnitConversionQuantity : -d.ProductUnitConversionQuantity)
        //           }).FirstAsync();


        //    if (sums.TotalPrimary.SubDecimal(outPrimary) < 0 || sums.TotalSecondary.SubDecimal(outSecondary) < 0)
        //    {
        //        var productCode = await _stockDbContext
        //                            .Product
        //                            .Where(p => p.ProductId == productId)
        //                            .Select(p => p.ProductCode)
        //                            .FirstOrDefaultAsync();

        //        var total = sums.TotalSecondary;
        //        var output = outSecondary;

        //        if (sums.TotalPrimary - outPrimary < MINIMUM_JS_NUMBER)
        //        {
        //            total = sums.TotalPrimary;
        //            output = outPrimary;
        //        }


        //        var message = $"Số lượng \"{productCode}\" trong kho tại thời điểm {endDate:dd-MM-yyyy} là " +
        //           $"{total.Format()} không đủ để xuất ({output.Format()})";

        //        return (InventoryErrorCode.NotEnoughQuantity, message);
        //    }

        //    return GeneralCode.Success;

        //}


        #endregion
    }
}
