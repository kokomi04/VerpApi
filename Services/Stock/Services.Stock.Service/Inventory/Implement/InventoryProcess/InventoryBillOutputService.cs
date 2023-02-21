using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using Verp.Resources.Stock.InventoryProcess;
using VErp.Commons.Constants;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Commons.Library.Formaters;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Products;
using static Verp.Resources.Stock.InventoryProcess.InventoryBillOutputMessage;
using InventoryEntity = VErp.Infrastructure.EF.StockDB.Inventory;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public class InventoryBillOutputService : InventoryServiceAbstract, IInventoryBillOutputService
    {
        const decimal MINIMUM_JS_NUMBER = Numbers.MINIMUM_ACCEPT_DECIMAL_NUMBER;

        private readonly IAsyncRunnerService _asyncRunner;
        private readonly ObjectActivityLogFacade _invOutputActivityLog;
        private readonly INotificationFactoryService _notificationFactoryService;

        public InventoryBillOutputService(
            StockDBContext stockContext
            , ILogger<InventoryService> logger
            , IActivityLogService activityLogService
            , IAsyncRunnerService asyncRunner
            , ICurrentContextService currentContextService
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IProductionOrderHelperService productionOrderHelperService
            , IProductionHandoverHelperService productionHandoverHelperService
            , IQueueProcessHelperService queueProcessHelperService
            , INotificationFactoryService notificationFactoryService) : base(stockContext, logger, customGenCodeHelperService, productionOrderHelperService, productionHandoverHelperService, currentContextService, queueProcessHelperService)
        {
            _asyncRunner = asyncRunner;
            _invOutputActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.InventoryOutput);
            _notificationFactoryService = notificationFactoryService;
        }


        public ObjectActivityLogModelBuilder<string> ImportedLogBuilder()
        {
            return _invOutputActivityLog.LogBuilder(() => InventoryBillOutputActivityMessage.Import);
        }


        /// <summary>
        /// Thêm mới phiếu xuất kho
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<long> AddInventoryOutput(InventoryOutModel req)
        {

            if (req.InventoryActionId == EnumInventoryAction.Rotation)
            {
                throw GeneralCode.InvalidParams.BadRequestFormat(CannotUpdateInvOutputRotation);
            }


            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey()))//req.StockId
            {
                var ctx = await GenerateInventoryCode(EnumInventoryType.Output, req);
                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    var inv = await AddInventoryOutputDb(req);
                    await trans.CommitAsync();


                    await ctx.ConfirmCode();

                    await _invOutputActivityLog.LogBuilder(() => InventoryBillOutputActivityMessage.Create)
                       .MessageResourceFormatDatas(req.InventoryCode)
                       .ObjectId(inv.InventoryId)
                       .JsonData(req.JsonSerialize())
                       .CreateLog();

                    return inv.InventoryId;
                }
            }
        }

        public async Task<InventoryEntity> AddInventoryOutputDb(InventoryOutModel req)
        {
            //if (req.InventoryActionId == EnumInventoryAction.Rotation)
            //{
            //    throw GeneralCode.InvalidParams.BadRequestFormat(CannotUpdateInvOutputRotation);
            //}

            if (req == null || req.OutProducts.Count == 0)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            await ValidateInventoryConfig(req.Date.UnixToDateTime(), null);

            req.InventoryCode = req.InventoryCode.Trim();

            //using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(req.StockId)))
            {
                await ValidateInventoryCode(null, req.InventoryCode);

                var issuedDate = req.Date.UnixToDateTime().Value;

                var inventoryObj = new InventoryEntity
                {
                    StockId = req.StockId,
                    InventoryCode = req.InventoryCode,
                    InventoryTypeId = (int)EnumInventoryType.Output,
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
                    //AccountancyAccountNumber = req.AccountancyAccountNumber,
                    CreatedByUserId = _currentContextService.UserId,
                    UpdatedByUserId = _currentContextService.UserId,
                    IsApproved = false,
                    DepartmentId = req.DepartmentId,
                    InventoryActionId = (int)req.InventoryActionId,
                    InventoryStatusId = (int)EnumInventoryStatus.Draff
                };

                await _stockDbContext.AddAsync(inventoryObj);
                await _stockDbContext.SaveChangesAsync();

                if (req.FileIdList != null && req.FileIdList.Count > 0)
                {
                    var attachedFiles = new List<InventoryFile>(req.FileIdList.Count);
                    attachedFiles.AddRange(req.FileIdList.Select(fileId => new InventoryFile() { FileId = fileId, InventoryId = inventoryObj.InventoryId }));
                    await _stockDbContext.AddRangeAsync(attachedFiles);
                    await _stockDbContext.SaveChangesAsync();
                }

                var data = await ProcessInventoryOut(inventoryObj, req);

                var totalMoney = InputCalTotalMoney(data.Select(x => x.Detail).ToList());

                inventoryObj.TotalMoney = totalMoney;

                foreach (var item in data)
                {
                    var eDetail = item.Detail;
                    eDetail.InventoryId = inventoryObj.InventoryId;

                    await _stockDbContext.InventoryDetail.AddRangeAsync(eDetail);
                    await _stockDbContext.SaveChangesAsync();

                    foreach (var sub in item.Subs)
                    {
                        sub.InventoryDetailSubCalculationId = 0;
                        sub.InventoryDetailId = eDetail.InventoryDetailId;
                    }

                    await _stockDbContext.InventoryDetailSubCalculation.AddRangeAsync(item.Subs);
                    await _stockDbContext.SaveChangesAsync();

                }


                //Move file from tmp folder
                if (req.FileIdList != null)
                {
                    foreach (var fileId in req.FileIdList)
                    {
                        _asyncRunner.RunAsync<IFileService>(f => f.FileAssignToObject(EnumObjectType.InventoryOutput, inventoryObj.InventoryId, fileId));
                    }
                }
                return inventoryObj;

            }
        }

        /// <summary>
        /// Cập nhật phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> UpdateInventoryOutput(long inventoryId, InventoryOutModel req)
        {
            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey()))//req.StockId
            {
                var issuedDate = req.Date.UnixToDateTime().Value;
                await ValidateInventoryCode(inventoryId, req.InventoryCode);

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);
                        if (inventoryObj == null)
                        {
                            trans.Rollback();
                            throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
                        }

                        if (req.UpdatedDatetimeUtc != inventoryObj.UpdatedDatetimeUtc.GetUnix())
                        {
                            throw GeneralCode.DataIsOld.BadRequest();
                        }

                        if (inventoryObj.InventoryActionId == (int)EnumInventoryAction.Rotation)
                        {
                            throw GeneralCode.InvalidParams.BadRequestFormat(CannotUpdateInvOutputRotation);
                        }

                        if (inventoryObj.StockId != req.StockId)
                        {
                            trans.Rollback();
                            throw new BadRequestException(InventoryErrorCode.CanNotChangeStock);
                        }

                        await ValidateInventoryConfig(req.Date.UnixToDateTime(), inventoryObj.Date);

                        // Validate nếu thông tin xuất kho tạo từ phiếu yêu cầu => không cho phép thêm/sửa mặt hàng
                        var inventoryDetails = await _stockDbContext.InventoryDetail.Where(d => d.InventoryId == inventoryObj.InventoryId).ToListAsync();
                        if (inventoryDetails.Any(id => id.InventoryRequirementDetailId.HasValue && id.InventoryRequirementDetailId > 0))
                        {
                            if (req.OutProducts.Any(d => !inventoryDetails.Any(id => id.ProductId == d.ProductId)))
                            {
                                throw new BadRequestException(InventoryErrorCode.CanNotChangeProductInventoryHasRequirement);
                            }
                        }

                        var rollbackResult = await RollbackInventoryOutput(inventoryObj);
                        if (!rollbackResult.IsSuccess())
                        {
                            trans.Rollback();
                            throw new BadRequestException(rollbackResult);
                        }

                        if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Output)
                        {
                            throw new BadRequestException(GeneralCode.InvalidParams);
                        }

                        var data = await ProcessInventoryOut(inventoryObj, req);

                        var arrInventoryDetailId = inventoryDetails.Select(x => x.InventoryDetailId);
                        var inventoryDetailSubCalculations = await _stockDbContext.InventoryDetailSubCalculation.Where(d => arrInventoryDetailId.Contains(d.InventoryDetailId)).ToListAsync();

                        foreach (var d in inventoryDetails)
                        {
                            d.IsDeleted = true;
                            d.UpdatedDatetimeUtc = DateTime.UtcNow;
                        }

                        foreach (var s in inventoryDetailSubCalculations)
                        {
                            s.IsDeleted = true;
                            s.UpdatedDatetimeUtc = DateTime.UtcNow;
                        }

                        foreach (var item in data)
                        {
                            var eDetail = item.Detail;

                            eDetail.InventoryId = inventoryObj.InventoryId;
                            await _stockDbContext.InventoryDetail.AddRangeAsync(eDetail);
                            await _stockDbContext.SaveChangesAsync();

                            foreach (var sub in item.Subs)
                            {
                                sub.InventoryDetailSubCalculationId = 0;
                                sub.InventoryDetailId = eDetail.InventoryDetailId;
                            }

                            await _stockDbContext.InventoryDetailSubCalculation.AddRangeAsync(item.Subs);
                            await _stockDbContext.SaveChangesAsync();

                        }

                        var totalMoney = InputCalTotalMoney(data.Select(x => x.Detail).ToList());

                        inventoryObj.TotalMoney = totalMoney;

                        //note: update IsApproved after RollbackInventoryOutput
                        inventoryObj.InventoryCode = req.InventoryCode;
                        inventoryObj.Shipper = req.Shipper;
                        inventoryObj.Content = req.Content;
                        inventoryObj.Date = issuedDate;
                        inventoryObj.CustomerId = req.CustomerId;
                        inventoryObj.Department = req.Department;
                        inventoryObj.StockKeeperUserId = req.StockKeeperUserId;

                        inventoryObj.BillForm = req.BillForm;
                        inventoryObj.BillCode = req.BillCode;
                        inventoryObj.BillSerial = req.BillSerial;
                        inventoryObj.BillDate = req.BillDate?.UnixToDateTime();

                        inventoryObj.IsApproved = false;
                        //inventoryObj.AccountancyAccountNumber = req.AccountancyAccountNumber;
                        inventoryObj.UpdatedByUserId = _currentContextService.UserId;
                        inventoryObj.DepartmentId = req.DepartmentId;
                        inventoryObj.InventoryStatusId = (int)EnumInventoryStatus.Draff;
                        inventoryObj.InventoryActionId = (int)req.InventoryActionId;

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

                        if (_stockDbContext.HasChanges())
                            inventoryObj.UpdatedDatetimeUtc = DateTime.UtcNow;

                        await _stockDbContext.SaveChangesAsync();

                        await ReCalculateRemainingAfterUpdate(inventoryObj.InventoryId);

                        trans.Commit();

                        await _invOutputActivityLog.LogBuilder(() => InventoryBillOutputActivityMessage.Update)
                            .MessageResourceFormatDatas(req.InventoryCode)
                            .ObjectId(inventoryId)
                            .JsonData(req.JsonSerialize())
                            .CreateLog();


                    }
                    catch (Exception ex)
                    {
                        trans.TryRollbackTransaction();
                        _stockDbContext.RollbackEntities();
                        _logger.LogError(ex, "UpdateInventoryOutput");
                        throw;
                    }
                }

                //Move file from tmp folder
                if (req.FileIdList != null)
                {
                    foreach (var fileId in req.FileIdList)
                    {
                        _asyncRunner.RunAsync<IFileService>(f => f.FileAssignToObject(EnumObjectType.InventoryOutput, inventoryId, fileId));
                    }
                }
                return true;
            }
        }


        /// <summary>
        /// Duyệt phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> ApproveInventoryOutput(long inventoryId)
        {
            if (inventoryId <= 0)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }

            var inventoryObj = await _stockDbContext.Inventory.FirstOrDefaultAsync(q => q.InventoryId == inventoryId);
            if (inventoryObj == null)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }

            if (inventoryObj.InventoryActionId == (int)EnumInventoryAction.Rotation)
            {
                throw GeneralCode.InvalidParams.BadRequestFormat(CannotUpdateInvOutputRotation);
            }

            if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Output)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }

            await ValidateInventoryConfig(inventoryObj.Date, inventoryObj.Date);

            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey()))//inventoryObj.StockId
            {
                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {

                    try
                    {
                        inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);


                        await ApproveInventoryOutputDb(inventoryObj);

                        trans.Commit();

                        await _invOutputActivityLog.LogBuilder(() => InventoryBillOutputActivityMessage.Approve)
                            .MessageResourceFormatDatas(inventoryObj.InventoryCode)
                            .ObjectId(inventoryId)
                            .JsonData(inventoryObj.JsonSerialize())
                            .CreateLog();

                        var inventoryDetails = await _stockDbContext.InventoryDetail.Where(d => d.InventoryId == inventoryId).ToListAsync();

                        await UpdateProductionOrderStatus(inventoryDetails, EnumProductionStatus.ProcessingLessStarted, inventoryObj.InventoryCode);

                        //await UpdateIgnoreAllocation(inventoryDetails);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.TryRollbackTransaction();
                        _logger.LogError(ex, "ApproveInventoryOutput");
                        throw;
                    }
                }
            }
        }


        public async Task ApproveInventoryOutputDb(InventoryEntity inventoryObj)
        {

            if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Output)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }

            await ValidateInventoryConfig(inventoryObj.Date, inventoryObj.Date);


            if (inventoryObj.InventoryStatusId == (int)EnumInventoryStatus.Censored)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryAlreadyApproved);
            }

            inventoryObj.IsApproved = true;

            inventoryObj.InventoryStatusId = (int)EnumInventoryStatus.Censored;
            //inventoryObj.UpdatedByUserId = currentUserId;
            //inventoryObj.UpdatedDatetimeUtc = DateTime.UtcNow;
            inventoryObj.CensorByUserId = _currentContextService.UserId;
            inventoryObj.CensorDatetimeUtc = DateTime.UtcNow;

            var inventoryDetails = _stockDbContext.InventoryDetail.Where(d => d.InventoryId == inventoryObj.InventoryId).ToList();

            var fromPackageIds = inventoryDetails.Select(f => f.FromPackageId).ToList();
            var fromPackages = _stockDbContext.Package.Where(p => fromPackageIds.Contains(p.PackageId)).ToList();

            var original = fromPackages.ToDictionary(p => p.PackageId, p => p.PrimaryQuantityRemaining.RoundBy());

            //var groupByProducts = inventoryDetails
            //    .GroupBy(g => new { g.ProductId, g.ProductUnitConversionId })
            //    .Select(g => new
            //    {
            //        g.Key.ProductId,
            //        g.Key.ProductUnitConversionId,
            //        OutPrimary = g.Sum(d => d.PrimaryQuantity),
            //        OutSecondary = g.Sum(d => d.ProductUnitConversionQuantity)
            //    });
            //foreach (var product in groupByProducts)
            //{

            //    var validate = await ValidateBalanceForOutput(inventoryObj.StockId, product.ProductId, inventoryObj.InventoryId, product.ProductUnitConversionId, inventoryObj.Date, product.OutPrimary, product.OutSecondary);

            //    if (!validate.IsSuccessCode())
            //    {
            //        trans.Rollback();

            //        throw new BadRequestException(validate.Code, validate.Message);
            //    }
            //}

            foreach (var detail in inventoryDetails)
            {
                var fromPackageInfo = fromPackages.FirstOrDefault(p => p.PackageId == detail.FromPackageId);
                if (fromPackageInfo == null) throw new BadRequestException(PackageErrorCode.PackageNotFound);

                fromPackageInfo.PrimaryQuantityWaiting = fromPackageInfo.PrimaryQuantityWaiting.SubDecimal(detail.PrimaryQuantity);
                fromPackageInfo.ProductUnitConversionWaitting = fromPackageInfo.ProductUnitConversionWaitting.SubDecimal(detail.ProductUnitConversionQuantity);
                if (fromPackageInfo.PrimaryQuantityWaiting == 0)
                {
                    fromPackageInfo.ProductUnitConversionWaitting = 0;
                }
                if (fromPackageInfo.ProductUnitConversionWaitting == 0)
                {
                    fromPackageInfo.PrimaryQuantityWaiting = 0;
                }

                fromPackageInfo.PrimaryQuantityRemaining = fromPackageInfo.PrimaryQuantityRemaining.SubDecimal(detail.PrimaryQuantity);
                fromPackageInfo.ProductUnitConversionRemaining = fromPackageInfo.ProductUnitConversionRemaining.SubDecimal(detail.ProductUnitConversionQuantity);
                if (fromPackageInfo.PrimaryQuantityRemaining == 0)
                {
                    fromPackageInfo.ProductUnitConversionRemaining = 0;
                }
                if (fromPackageInfo.ProductUnitConversionRemaining == 0)
                {
                    fromPackageInfo.PrimaryQuantityRemaining = 0;
                }

                if (fromPackageInfo.PrimaryQuantityRemaining < 0)
                {
                    var productInfo = await (
                        from p in _stockDbContext.Product
                        join c in _stockDbContext.ProductUnitConversion on p.ProductId equals c.ProductId
                        where p.ProductId == detail.ProductId
                              && c.ProductUnitConversionId == detail.ProductUnitConversionId
                        select new
                        {
                            p.ProductCode,
                            p.ProductName,
                            c.ProductUnitConversionName
                        }).FirstOrDefaultAsync();

                    if (productInfo == null)
                    {
                        throw new BadRequestException(ProductErrorCode.ProductNotFound);
                    }


                    var balance = $"{original[detail.FromPackageId.Value].Format()} {productInfo.ProductUnitConversionName}";

                    var samPackages = inventoryDetails.Where(d => d.FromPackageId == detail.FromPackageId);

                    var total = samPackages.Sum(d => d.PrimaryQuantity).Format();

                    var totalOut = $" < {total} {productInfo.ProductUnitConversionName} = " + string.Join(" + ", samPackages.Select(d => d.PrimaryQuantity.Format()));


                    throw NotEnoughBalanceInStock.BadRequestFormat(productInfo.ProductCode, balance, totalOut);
                }

                ValidatePackage(fromPackageInfo);

                var stockProduct = await EnsureStockProduct(inventoryObj.StockId, detail.ProductId, detail.ProductUnitConversionId);

                stockProduct.PrimaryQuantityWaiting = stockProduct.PrimaryQuantityWaiting.SubDecimal(detail.PrimaryQuantity);
                stockProduct.ProductUnitConversionWaitting = stockProduct.ProductUnitConversionWaitting.SubDecimal(detail.ProductUnitConversionQuantity);
                if (stockProduct.PrimaryQuantityWaiting == 0)
                {
                    stockProduct.ProductUnitConversionWaitting = 0;
                }
                if (stockProduct.ProductUnitConversionWaitting == 0)
                {
                    stockProduct.PrimaryQuantityWaiting = 0;
                }

                stockProduct.PrimaryQuantityRemaining = stockProduct.PrimaryQuantityRemaining.SubDecimal(detail.PrimaryQuantity);
                stockProduct.ProductUnitConversionRemaining = stockProduct.ProductUnitConversionRemaining.SubDecimal(detail.ProductUnitConversionQuantity);
                if (stockProduct.PrimaryQuantityRemaining == 0)
                {
                    stockProduct.ProductUnitConversionRemaining = 0;
                }
                if (stockProduct.ProductUnitConversionRemaining == 0)
                {
                    stockProduct.PrimaryQuantityRemaining = 0;
                }

                ValidateStockProduct(stockProduct);
            }

            await _stockDbContext.SaveChangesAsync();

            await ReCalculateRemainingAfterUpdate(inventoryObj.InventoryId);




        }


        public async Task<bool> SentToCensor(long inventoryId)
        {
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                var info = await _stockDbContext.Inventory.FirstOrDefaultAsync(d => d.InventoryId == inventoryId);
                if (info == null) throw new BadRequestException(InventoryErrorCode.InventoryNotFound);

                //if (info.InventoryActionId == (int)EnumInventoryAction.Rotation)
                //{
                //    throw GeneralCode.InvalidParams.BadRequestFormat(CannotUpdateInvOutputRotation);
                //}

                if (info.InventoryStatusId != (int)EnumInventoryStatus.Draff && info.InventoryStatusId != (int)EnumInventoryStatus.Reject)
                {
                    throw new BadRequestException(InventoryErrorCode.InventoryNotDraffYet);
                }

                info.InventoryStatusId = (int)EnumInventoryStatus.WaitToCensor;

                await _stockDbContext.SaveChangesAsync();

                trans.Commit();

                await _notificationFactoryService.AddSubscriptionToThePermissionPerson(new SubscriptionToThePermissionPersonSimpleModel
                {
                    ObjectId = inventoryId,
                    ObjectTypeId = (int)EnumObjectType.InventoryInput,
                    ModuleId = _currentContextService.ModuleId,
                    PermissionId = (int)EnumActionType.Censor
                });

                await _invOutputActivityLog.LogBuilder(() => InventoryBillOutputActivityMessage.WaitToCensor)
                        .MessageResourceFormatDatas(info.InventoryCode)
                        .ObjectId(inventoryId)
                        .JsonData(info.JsonSerialize())
                        .CreateLog();

                await _notificationFactoryService.AddSubscription(new SubscriptionSimpleModel
                {
                    ObjectId = inventoryId,
                    UserId = _currentContextService.UserId,
                    ObjectTypeId = (int)EnumObjectType.InventoryInput
                });



                return true;
            }
        }

        public async Task<bool> Reject(long inventoryId)
        {
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                var info = await _stockDbContext.Inventory.FirstOrDefaultAsync(d => d.InventoryId == inventoryId);
                if (info == null) throw new BadRequestException(InventoryErrorCode.InventoryNotFound);

                //if (info.InventoryActionId == (int)EnumInventoryAction.Rotation)
                //{
                //    throw GeneralCode.InvalidParams.BadRequestFormat(CannotUpdateInvOutputRotation);
                //}

                if (info.InventoryStatusId != (int)EnumInventoryStatus.WaitToCensor)
                {
                    throw new BadRequestException(InventoryErrorCode.InventoryNotSentToCensorYet);
                }

                info.IsApproved = false;

                info.InventoryStatusId = (int)EnumInventoryStatus.Reject;
                info.CensorDatetimeUtc = DateTime.UtcNow;
                info.CensorByUserId = _currentContextService.UserId;

                await _stockDbContext.SaveChangesAsync();

                trans.Commit();

                await _invOutputActivityLog.LogBuilder(() => InventoryBillOutputActivityMessage.Reject)
                        .MessageResourceFormatDatas(info.InventoryCode)
                        .ObjectId(inventoryId)
                        .JsonData(info.JsonSerialize())
                        .CreateLog();



                return true;
            }
        }


        /// <summary>
        /// Xoá phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteInventoryOutput(long inventoryId)
        {
            var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(p => p.InventoryId == inventoryId);

            if (inventoryObj == null)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }

            if (inventoryObj.InventoryActionId == (int)EnumInventoryAction.Rotation)
            {
                throw GeneralCode.InvalidParams.BadRequestFormat(CannotUpdateInvOutputRotation);
            }

            if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Output)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey()))//inventoryObj.StockId
            {
                //reload from db after lock
                inventoryObj = _stockDbContext.Inventory.FirstOrDefault(p => p.InventoryId == inventoryId);

                await ValidateInventoryConfig(null, inventoryObj.Date);

                // Xử lý xoá thông tin phiếu xuất kho
                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        await DeleteInventoryOutputDb(inventoryObj);

                        trans.Commit();

                        await _invOutputActivityLog.LogBuilder(() => InventoryBillOutputActivityMessage.Delete)
                            .MessageResourceFormatDatas(inventoryObj.InventoryCode)
                            .ObjectId(inventoryId)
                            .JsonData(inventoryObj.JsonSerialize())
                            .CreateLog();


                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.TryRollbackTransaction();
                        _stockDbContext.RollbackEntities();
                        _logger.LogError(ex, "DeleteInventoryOutput");

                        throw;
                    }
                }
            }
        }


        public async Task DeleteInventoryOutputDb(InventoryEntity inventoryObj)
        {
            //Cần rollback cả 2 loại phiếu đã duyệt và chưa duyệt All approved or not need tobe rollback, bỏ if (inventoryObj.IsApproved)
            var processResult = await RollbackInventoryOutput(inventoryObj);
            if (!processResult.IsSuccess())
            {

                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            //update status after rollback
            inventoryObj.IsDeleted = true;
            //inventoryObj.IsApproved = false;

            await _stockDbContext.SaveChangesAsync();

            if (inventoryObj.IsApproved)
            {
                await ReCalculateRemainingAfterUpdate(inventoryObj.InventoryId);
            }
        }

        /*
        public async Task<PageData<PackageOutputModel>> GetPackageListForExport(int productId, IList<int> productCateIds, IList<int> stockIdList, int page = 1, int size = 20)
        {

            var productQuery = _stockDbContext.Product.AsQueryable();
            if (productCateIds?.Count > 0)
            {
                productQuery = productQuery.Where(p => productCateIds.Contains(p.ProductCateId));
            }
            var query = from pk in _stockDbContext.Package
                        join p in productQuery on pk.ProductId equals p.ProductId
                        where stockIdList.Contains(pk.StockId) && pk.ProductId == productId && pk.PrimaryQuantityRemaining > 0
                        select new
                        {
                            pk.LocationId,
                            pk.ProductUnitConversionId,
                            pk.PackageId,
                            pk.PackageCode,
                            pk.PackageTypeId,
                            pk.StockId,
                            pk.ProductId,
                            pk.Date,
                            pk.ExpiryTime,
                            pk.Description,
                            p.UnitId,
                            pk.PrimaryQuantityRemaining,
                            pk.PrimaryQuantityWaiting,
                            pk.ProductUnitConversionRemaining,
                            pk.ProductUnitConversionWaitting,
                            pk.CreatedDatetimeUtc,
                            pk.UpdatedDatetimeUtc,
                            pk.OrderCode,
                            pk.Pocode,
                            pk.ProductionOrderCode
                        };

            var total = await query.CountAsync();

            var packageData = size > 0 ? await query.AsNoTracking().Skip((page - 1) * size).Take(size).ToListAsync() : await query.AsNoTracking().ToListAsync();

            var locationIdList = packageData.Select(q => q.LocationId).ToList();
            var productUnitConversionIdList = packageData.Select(q => q.ProductUnitConversionId).ToList();
            var locationData = await _stockDbContext.Location.AsNoTracking().Where(q => locationIdList.Contains(q.LocationId)).ToListAsync();
            var productUnitConversionData = _stockDbContext.ProductUnitConversion.Where(q => productUnitConversionIdList.Contains(q.ProductUnitConversionId)).AsNoTracking().ToList();

            var packageList = new List<PackageOutputModel>(total);
            foreach (var item in packageData)
            {
                var locationObj = item.LocationId > 0 ? locationData.FirstOrDefault(q => q.LocationId == item.LocationId) : null;
                var locationOutputModel = locationObj != null ? new Model.Location.LocationOutput
                {
                    LocationId = locationObj.LocationId,
                    StockId = locationObj.StockId,
                    StockName = string.Empty,
                    Name = locationObj.Name,
                    Description = locationObj.Description,
                    Status = 0
                } : null;

                packageList.Add(new PackageOutputModel
                {
                    PackageId = item.PackageId,
                    PackageCode = item.PackageCode,
                    PackageTypeId = item.PackageTypeId,
                    LocationId = item.LocationId ?? 0,
                    StockId = item.StockId,
                    ProductId = item.ProductId,
                    Date = item.Date != null ? ((DateTime)item.Date).GetUnix() : 0,
                    ExpiryTime = item.ExpiryTime != null ? ((DateTime)item.ExpiryTime).GetUnix() : 0,
                    Description = item.Description,
                    PrimaryUnitId = item.UnitId,
                    ProductUnitConversionId = item.ProductUnitConversionId,
                    PrimaryQuantityWaiting = item.PrimaryQuantityWaiting.RoundBy(),
                    PrimaryQuantityRemaining = item.PrimaryQuantityRemaining.RoundBy(),
                    ProductUnitConversionWaitting = item.ProductUnitConversionWaitting.RoundBy(),
                    ProductUnitConversionRemaining = item.ProductUnitConversionRemaining.RoundBy(),

                    CreatedDatetimeUtc = item.CreatedDatetimeUtc.GetUnix(),
                    UpdatedDatetimeUtc = item.UpdatedDatetimeUtc.GetUnix(),
                    LocationOutputModel = locationOutputModel,
                    ProductUnitConversionModel = productUnitConversionData.FirstOrDefault(q => q.ProductUnitConversionId == item.ProductUnitConversionId) ?? null,
                    OrderCode = item.OrderCode,
                    POCode = item.Pocode,
                    ProductionOrderCode = item.ProductionOrderCode

                });
            }
            return (packageList, total);

        }
        */

        private async Task<IList<CoupleDataInventoryDetail>> ProcessInventoryOut(InventoryEntity inventory, InventoryOutModel req)
        {
            var productIds = req.OutProducts.Select(p => p.ProductId).Distinct().ToList();

            var productInfos = await _stockDbContext.Product.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToListAsync();
            var productUnitConversions = await _stockDbContext.ProductUnitConversion.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToListAsync();
            foreach (var pu in productUnitConversions)
            {
                if (pu.IsDefault)
                {
                    pu.FactorExpression = "1";
                }
            }

            var fromPackageIds = req.OutProducts.Select(p => p.FromPackageId).ToList();
            var fromPackages = await _stockDbContext.Package.Where(p => fromPackageIds.Contains(p.PackageId)).ToListAsync();


            var inventoryDetailList = new List<CoupleDataInventoryDetail>();

            var packageRemainingPrimary = fromPackages.ToDictionary(p => p.PackageId, p => p.PrimaryQuantityRemaining);

            var packageRemainingPu = fromPackages.ToDictionary(p => p.PackageId, p => p.ProductUnitConversionRemaining);

            foreach (var detail in req.OutProducts)
            {
                if (EnumInventoryAction.OutputForSell == req.InventoryActionId && string.IsNullOrWhiteSpace(detail.OrderCode))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Xuất kho thành phẩm bán hàng bắt buộc phải có mã đơn hàng");
                else if (EnumInventoryAction.OutputForManufacture == req.InventoryActionId && string.IsNullOrWhiteSpace(detail.ProductionOrderCode))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Xuất kho vật tư cho sản xuất bắt buộc phải có mã lệnh sản xuất");

                var fromPackageInfo = fromPackages.FirstOrDefault(p => p.PackageId == detail.FromPackageId);
                if (fromPackageInfo == null) throw PackageErrorCode.PackageNotFound.BadRequest();

                if (fromPackageInfo.ProductId != detail.ProductId
                    || fromPackageInfo.ProductUnitConversionId != detail.ProductUnitConversionId
                    || fromPackageInfo.StockId != req.StockId)
                {
                    _logger.LogInformation($"InventoryService.ProcessInventoryOut error InvalidPackage. ProductId: {detail.ProductId} , FromPackageId: {detail.FromPackageId}, ProductUnitConversionId: {detail.ProductUnitConversionId}");
                    throw InventoryErrorCode.InvalidPackage.BadRequest();
                }

                var productInfo = productInfos.FirstOrDefault(p => p.ProductId == detail.ProductId);
                if (productInfo == null)
                {
                    throw ProductErrorCode.ProductNotFound.BadRequest();
                }

                var primaryQualtity = detail.PrimaryQuantity;

                var puDefault = productUnitConversions.FirstOrDefault(c => c.ProductId == detail.ProductId && c.IsDefault);

                var puInfo = productUnitConversions.FirstOrDefault(c => c.ProductUnitConversionId == detail.ProductUnitConversionId);
                if (puInfo == null)
                {
                    _logger.LogInformation($"InventoryService.ProcessInventoryOut error ProductUnitConversionNotFound. ProductId: {detail.ProductId} , FromPackageId: {detail.FromPackageId}, ProductUnitConversionId: {detail.ProductUnitConversionId}");
                    throw ProductUnitConversionErrorCode.ProductUnitConversionNotFound.BadRequest();
                }

                if (puInfo.ProductId != detail.ProductId)
                {
                    throw ProductUnitConversionErrorCode.ProductUnitConversionNotBelongToProduct.BadRequest();
                }



                if (fromPackageInfo.PrimaryQuantityRemaining == 0 || fromPackageInfo.ProductUnitConversionRemaining == 0)
                {
                    _logger.LogInformation($"InventoryService.ProcessInventoryOut error NotEnoughQuantity. ProductId: {detail.ProductId} , packageId: {fromPackageInfo.PackageId} PrimaryQuantityRemaining: {fromPackageInfo.PrimaryQuantityRemaining}, ProductUnitConversionRemaining: {fromPackageInfo.ProductUnitConversionRemaining}, req: {req.JsonSerialize()} ");


                    //return (InventoryErrorCode.NotEnoughQuantity, errorMessage);
                    throw NotEnoughBalancePackageQuantityZero.BadRequest();
                }

                //if (details.ProductUnitConversionQuantity <= 0 && primaryQualtity > 0)
                //{
                if ((puInfo.IsFreeStyle ?? false) == false)
                {
                    var calcModel = new QuantityPairInputModel()
                    {
                        PrimaryQuantity = detail.PrimaryQuantity,
                        PrimaryDecimalPlace = puDefault?.DecimalPlace ?? 12,

                        PuQuantity = detail.ProductUnitConversionQuantity,
                        PuDecimalPlace = puInfo.DecimalPlace,

                        FactorExpression = puInfo.FactorExpression,

                        FactorExpressionRate = fromPackageInfo.ProductUnitConversionRemaining / fromPackageInfo.PrimaryQuantityRemaining
                    };

                    //var (isSuccess, pucQuantity) = EvalUtils.GetProductUnitConversionQuantityFromPrimaryQuantity(detail.PrimaryQuantity, fromPackageInfo.ProductUnitConversionRemaining / fromPackageInfo.PrimaryQuantityRemaining, detail.ProductUnitConversionQuantity, puInfo.DecimalPlace);

                    var (isSuccess, primaryQuantity, pucQuantity) = EvalUtils.GetProductUnitConversionQuantityFromPrimaryQuantity(calcModel);

                    if (isSuccess)
                    {
                        detail.ProductUnitConversionQuantity = pucQuantity;
                    }
                    else
                    {
                        _logger.LogWarning($"Wrong pucQuantity input data: PrimaryQuantity={detail.PrimaryQuantity}, FactorExpression={fromPackageInfo.ProductUnitConversionRemaining / fromPackageInfo.PrimaryQuantityRemaining}, ProductUnitConversionQuantity={detail.ProductUnitConversionQuantity}, evalData={pucQuantity}");
                        //return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                        throw new BadRequestException(ProductUnitConversionErrorCode.SecondaryUnitConversionError, $"{productInfo.ProductCode} không thể tính giá trị ĐVCĐ, tính theo tỷ lệ: {pucQuantity.Format()}, nhập vào {detail.ProductUnitConversionQuantity.Format()}, kiểm tra lại độ sai số đơn vị");
                    }
                }

                if (!(detail.ProductUnitConversionQuantity > 0))
                {
                    _logger.LogInformation($"InventoryService.ProcessInventoryOut error PrimaryUnitConversionError. ProductId: {detail.ProductId} , FromPackageId: {detail.FromPackageId}, ProductUnitConversionId: {detail.ProductUnitConversionId}, FactorExpression: {puInfo.FactorExpression}, PrimaryQuantity: {detail.PrimaryQuantity}, ProductUnitConversionQuantity: {detail.ProductUnitConversionQuantity}");
                    throw ProductUnitConversionErrorCode.PrimaryUnitConversionError.BadRequest();
                }



                if (Math.Abs(detail.ProductUnitConversionQuantity - fromPackageInfo.ProductUnitConversionRemaining) <= MINIMUM_JS_NUMBER)
                {
                    detail.ProductUnitConversionQuantity = fromPackageInfo.ProductUnitConversionRemaining;
                }

                if (Math.Abs(primaryQualtity - fromPackageInfo.PrimaryQuantityRemaining) <= MINIMUM_JS_NUMBER)
                {
                    primaryQualtity = fromPackageInfo.PrimaryQuantityRemaining;

                }



                packageRemainingPrimary[fromPackageInfo.PackageId] = packageRemainingPrimary[fromPackageInfo.PackageId].SubDecimal(primaryQualtity);
                packageRemainingPu[fromPackageInfo.PackageId] = packageRemainingPu[fromPackageInfo.PackageId].SubDecimal(detail.ProductUnitConversionQuantity);

                if (packageRemainingPrimary[fromPackageInfo.PackageId] == 0)
                {
                    packageRemainingPu[fromPackageInfo.PackageId] = 0;
                }

                if (packageRemainingPu[fromPackageInfo.PackageId] == 0)
                {
                    packageRemainingPrimary[fromPackageInfo.PackageId] = 0;
                }

                if (packageRemainingPrimary[fromPackageInfo.PackageId] < 0)
                {
                    var primaryUnit = productUnitConversions.FirstOrDefault(c => c.IsDefault && c.ProductId == productInfo.ProductId);
                    var remaining = $"{fromPackageInfo.PrimaryQuantityRemaining.Format()} {primaryUnit?.ProductUnitConversionName}";

                    var samPackages = req.OutProducts.Where(d => d.FromPackageId == detail.FromPackageId);

                    var totalOut = samPackages.Sum(d => d.PrimaryQuantity);

                    var totalOutMess = $"{totalOut.Format()} {primaryUnit?.ProductUnitConversionName}";

                    _logger.LogInformation($"InventoryService.ProcessInventoryOut error NotEnoughQuantity. ProductId: {detail.ProductId} , ProductUnitConversionQuantity: {detail.ProductUnitConversionQuantity}, ProductUnitConversionRemaining: {fromPackageInfo.ProductUnitConversionRemaining}");

                    throw NotEnoughBalanceInPackage.BadRequestFormat(fromPackageInfo.PackageCode, productInfo.ProductCode, remaining, totalOutMess);
                }


                if (detail.ProductUnitConversionPrice == 0)
                {
                    detail.ProductUnitConversionPrice = detail.PrimaryQuantity * detail.UnitPrice / detail.ProductUnitConversionQuantity;
                }

                if (detail.UnitPrice == 0)
                {
                    detail.UnitPrice = detail.ProductUnitConversionQuantity * detail.ProductUnitConversionPrice / detail.PrimaryQuantity;
                }

                var eDetail = new InventoryDetail
                {
                    InventoryId = inventory.InventoryId,
                    ProductId = detail.ProductId,
                    RequestPrimaryQuantity = detail.RequestPrimaryQuantity?.RoundBy(puDefault.DecimalPlace),
                    PrimaryQuantity = primaryQualtity.RoundBy(puDefault.DecimalPlace),
                    UnitPrice = detail.UnitPrice?.RoundBy(puDefault.DecimalPlace) ?? 0,
                    ProductUnitConversionId = detail.ProductUnitConversionId,
                    RequestProductUnitConversionQuantity = detail.RequestProductUnitConversionQuantity?.RoundBy(puInfo.DecimalPlace),
                    ProductUnitConversionQuantity = detail.ProductUnitConversionQuantity.RoundBy(puInfo.DecimalPlace),
                    ProductUnitConversionPrice = detail.ProductUnitConversionPrice?.RoundBy(puInfo.DecimalPlace),
                    Money = detail.Money,
                    RefObjectTypeId = detail.RefObjectTypeId,
                    RefObjectId = detail.RefObjectId,
                    RefObjectCode = detail.RefObjectCode,
                    OrderCode = detail.OrderCode,
                    Pocode = detail.POCode,
                    ProductionOrderCode = detail.ProductionOrderCode,
                    FromPackageId = detail.FromPackageId,
                    ToPackageId = null,
                    PackageOptionId = null,
                    SortOrder = detail.SortOrder,
                    Description = detail.Description,
                    //AccountancyAccountNumberDu = detail.AccountancyAccountNumberDu,
                    //InventoryRequirementCode = detail.InventoryRequirementCode,
                    InventoryRequirementDetailId = detail.InventoryRequirementDetailId,
                    IsSubCalculation = detail.IsSubCalculation
                };

                var eSubs = detail.InProductSubs.Select(x => new InventoryDetailSubCalculation
                {
                    PrimaryQuantity = x.PrimaryQuantity,
                    ProductBomId = x.ProductBomId,
                    PrimaryUnitPrice = x.PrimaryUnitPrice,
                    UnitConversionId = x.UnitConversionId
                }).ToList();

                inventoryDetailList.Add(new CoupleDataInventoryDetail
                {
                    Detail = eDetail,
                    Subs = eSubs
                });

                fromPackageInfo.PrimaryQuantityWaiting = fromPackageInfo.PrimaryQuantityWaiting.AddDecimal(primaryQualtity)
                    .RoundBy(puDefault.DecimalPlace);

                fromPackageInfo.ProductUnitConversionWaitting = fromPackageInfo.ProductUnitConversionWaitting.AddDecimal(detail.ProductUnitConversionQuantity)
                    .RoundBy(puInfo.DecimalPlace);

                var stockProductInfo = await EnsureStockProduct(inventory.StockId, fromPackageInfo.ProductId, fromPackageInfo.ProductUnitConversionId);

                stockProductInfo.PrimaryQuantityWaiting = stockProductInfo.PrimaryQuantityWaiting.AddDecimal(primaryQualtity)
                     .RoundBy(puDefault.DecimalPlace);
                stockProductInfo.ProductUnitConversionWaitting = stockProductInfo.ProductUnitConversionWaitting.AddDecimal(detail.ProductUnitConversionQuantity)
                    .RoundBy(puInfo.DecimalPlace);
            }
            return inventoryDetailList;
        }

        private async Task<Enum> RollbackInventoryOutput(InventoryEntity inventory)
        {
            var inventoryDetails = await _stockDbContext.InventoryDetail.Where(d => d.InventoryId == inventory.InventoryId).ToListAsync();

            var fromPackageIds = inventoryDetails.Select(d => d.FromPackageId).ToList();

            var fromPackages = await _stockDbContext.Package.Where(p => fromPackageIds.Contains(p.PackageId)).ToListAsync();

            var productIds = inventoryDetails.Select(d => d.ProductId).Distinct().ToList();

            var pus = await _stockDbContext.ProductUnitConversion.Where(p => productIds.Contains(p.ProductId)).ToListAsync();

            var puConversions = pus
               .ToDictionary(p => p.ProductUnitConversionId, p => p);

            var puDefaults = pus.GroupBy(p => p.ProductId)
                .ToDictionary(p => p.Key, p => p.FirstOrDefault(d => d.IsDefault));

            foreach (var detail in inventoryDetails)
            {
                var fromPackageInfo = fromPackages.FirstOrDefault(f => f.PackageId == detail.FromPackageId);
                if (fromPackageInfo == null) return PackageErrorCode.PackageNotFound;

                var stockProductInfo = await EnsureStockProduct(inventory.StockId, detail.ProductId, detail.ProductUnitConversionId);

                puConversions.TryGetValue(detail.ProductUnitConversionId, out var puInfo);

                puDefaults.TryGetValue(detail.ProductId, out var puDefaultInfo);

                if (!inventory.IsApproved)
                {
                    fromPackageInfo.PrimaryQuantityWaiting = fromPackageInfo.PrimaryQuantityWaiting.SubDecimal(detail.PrimaryQuantity)
                        .RoundBy(puDefaultInfo?.DecimalPlace);

                    fromPackageInfo.ProductUnitConversionWaitting = fromPackageInfo.ProductUnitConversionWaitting.SubDecimal(detail.ProductUnitConversionQuantity)
                        .RoundBy(puInfo?.DecimalPlace);

                    if (fromPackageInfo.PrimaryQuantityWaiting == 0)
                    {
                        fromPackageInfo.ProductUnitConversionWaitting = 0;
                    }
                    if (fromPackageInfo.ProductUnitConversionWaitting == 0)
                    {
                        fromPackageInfo.PrimaryQuantityWaiting = 0;
                    }

                    stockProductInfo.PrimaryQuantityWaiting = stockProductInfo.PrimaryQuantityWaiting.SubDecimal(detail.PrimaryQuantity)
                         .RoundBy(puDefaultInfo?.DecimalPlace);

                    stockProductInfo.ProductUnitConversionWaitting = stockProductInfo.ProductUnitConversionWaitting.SubDecimal(detail.ProductUnitConversionQuantity)
                         .RoundBy(puInfo?.DecimalPlace);

                    if (stockProductInfo.PrimaryQuantityWaiting == 0)
                    {
                        stockProductInfo.ProductUnitConversionWaitting = 0;
                    }

                    if (stockProductInfo.ProductUnitConversionWaitting == 0)
                    {
                        stockProductInfo.PrimaryQuantityWaiting = 0;
                    }
                }
                else
                {
                    fromPackageInfo.PrimaryQuantityRemaining = fromPackageInfo.PrimaryQuantityRemaining.AddDecimal(detail.PrimaryQuantity)
                        .RoundBy(puDefaultInfo?.DecimalPlace);
                    fromPackageInfo.ProductUnitConversionRemaining = fromPackageInfo.ProductUnitConversionRemaining.AddDecimal(detail.ProductUnitConversionQuantity)
                         .RoundBy(puInfo?.DecimalPlace);

                    if (fromPackageInfo.PrimaryQuantityRemaining == 0)
                    {
                        fromPackageInfo.ProductUnitConversionRemaining = 0;
                    }
                    if (fromPackageInfo.ProductUnitConversionRemaining == 0)
                    {
                        fromPackageInfo.PrimaryQuantityRemaining = 0;
                    }

                    stockProductInfo.PrimaryQuantityRemaining = stockProductInfo.PrimaryQuantityRemaining.AddDecimal(detail.PrimaryQuantity)
                        .RoundBy(puDefaultInfo?.DecimalPlace);
                    stockProductInfo.ProductUnitConversionRemaining = stockProductInfo.ProductUnitConversionRemaining.AddDecimal(detail.ProductUnitConversionQuantity)
                         .RoundBy(puInfo?.DecimalPlace);

                    if (stockProductInfo.PrimaryQuantityRemaining == 0)
                    {
                        stockProductInfo.ProductUnitConversionRemaining = 0;
                    }
                    if (stockProductInfo.ProductUnitConversionRemaining == 0)
                    {
                        stockProductInfo.PrimaryQuantityRemaining = 0;
                    }
                }

                ValidatePackage(fromPackageInfo);
                ValidateStockProduct(stockProductInfo);

                detail.IsDeleted = true;
            }

            await _stockDbContext.SaveChangesAsync();


            return GeneralCode.Success;
        }


    }
}
