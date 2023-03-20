using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using Verp.Resources.Stock.InventoryProcess;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Package;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Products;
using static Verp.Resources.Stock.InventoryProcess.InventoryBillInputMessage;
using InventoryEntity = VErp.Infrastructure.EF.StockDB.Inventory;
using PackageEntity = VErp.Infrastructure.EF.StockDB.Package;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public partial class InventoryBillInputService : InventoryServiceAbstract, IInventoryBillInputService
    {
        //const decimal MINIMUM_JS_NUMBER = Numbers.MINIMUM_ACCEPT_DECIMAL_NUMBER;


        private readonly IAsyncRunnerService _asyncRunner;
        private readonly IProductService _productService;
        private readonly ObjectActivityLogFacade _invInputActivityLog;
        private readonly ObjectActivityLogFacade _invOutActivityLog;
        private readonly ObjectActivityLogFacade _packageActivityLog;
        private readonly IActivityLogService _activityLogService;
        private readonly INotificationFactoryService _notificationFactoryService;

        public InventoryBillInputService(
            StockDBContext stockContext
            , ILogger<InventoryService> logger
            , IActivityLogService activityLogService
            , IAsyncRunnerService asyncRunner
            , ICurrentContextService currentContextService
            , IProductService productService
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IProductionOrderHelperService productionOrderHelperService
            , IProductionHandoverHelperService productionHandoverHelperService
            , IQueueProcessHelperService queueProcessHelperService
            , INotificationFactoryService notificationFactoryService) : base(stockContext, logger, customGenCodeHelperService, productionOrderHelperService, productionHandoverHelperService, currentContextService, queueProcessHelperService)
        {

            _asyncRunner = asyncRunner;
            _productService = productService;
            _invInputActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.InventoryInput);
            _invOutActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.InventoryOutput);
            _packageActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Package);
            _activityLogService = activityLogService;
            _notificationFactoryService = notificationFactoryService;
        }


        public ObjectActivityLogModelBuilder<string> ImportedLogBuilder()
        {
            return _invInputActivityLog.LogBuilder(() => InventoryBillInputActivityLogMessage.Import);
        }


        public async Task<long> AddInventoryInput(InventoryInModel req)
        {
            if (req.InventoryActionId == EnumInventoryAction.Rotation)
            {
                throw GeneralCode.InvalidParams.BadRequestFormat(CannotUpdateInvInputRotation);
            }

            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey()))//req.StockId
            {
                var ctx = await GenerateInventoryCode(EnumInventoryType.Input, req);

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    var invInput = await AddInventoryInputDB(req, true);

                    await trans.CommitAsync();

                    await ctx.ConfirmCode();

                    await _invInputActivityLog.LogBuilder(() => InventoryBillInputActivityLogMessage.Create)
                       .MessageResourceFormatDatas(req.InventoryCode)
                       .ObjectId(invInput.InventoryId)
                       .JsonData(req.JsonSerialize())
                       .CreateLog();


                    return invInput.InventoryId;
                }
            }

        }

        public async Task<InventoryEntity> AddInventoryInputDB(InventoryInModel req, bool validatePackageInfo)
        {
            //if (req.InventoryActionId == EnumInventoryAction.Rotation)
            //{
            //    throw GeneralCode.InvalidParams.BadRequestFormat(CannotUpdateInvInputRotation);
            //}

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

                var validInventoryDetails = await ValidateInventoryIn(false, req, validatePackageInfo);

                if (!validInventoryDetails.Code.IsSuccess())
                {
                    throw new BadRequestException(validInventoryDetails.Code);
                }

                var totalMoney = InputCalTotalMoney(validInventoryDetails.Data.Select(x => x.Detail).ToList());

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

                inventoryObj.TotalMoney = totalMoney;

                await _stockDbContext.SaveChangesAsync();

                //Move file from tmp folder
                if (req.FileIdList != null)
                {
                    foreach (var fileId in req.FileIdList)
                    {
                        _asyncRunner.RunAsync<IFileService>(f => f.FileAssignToObject(EnumObjectType.InventoryInput, inventoryObj.InventoryId, fileId));
                    }
                }
                return inventoryObj;
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

            if (req.InventoryActionId == EnumInventoryAction.Rotation)
            {
                throw GeneralCode.InvalidParams.BadRequestFormat(CannotUpdateInvInputRotation);
            }

            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey()))//req.StockId
            {

                var issuedDate = req.Date.UnixToDateTime().Value;

                var validate = await ValidateInventoryIn(false, req, true);

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


                        if (req.UpdatedDatetimeUtc != inventoryObj.UpdatedDatetimeUtc.GetUnix())
                        {
                            throw GeneralCode.DataIsOld.BadRequest();
                        }

                        if (inventoryObj.InventoryActionId == (int)EnumInventoryAction.Rotation)
                        {
                            throw GeneralCode.InvalidParams.BadRequestFormat(CannotUpdateInvInputRotation);
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

                        // Validate nếu thông tin nhập kho tạo từ phiếu yêu cầu => không cho phép thêm/sửa mặt hàng
                        if (inventoryDetails.Any(id => id.InventoryRequirementDetailId.HasValue && id.InventoryRequirementDetailId > 0))
                        {
                            if (validate.Data.Select(x => x.Detail).Any(d => !inventoryDetails.Any(id => id.ProductId == d.ProductId)))
                            {
                                throw new BadRequestException(InventoryErrorCode.CanNotChangeProductInventoryHasRequirement);
                            }
                        }

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

                        foreach (var item in validate.Data)
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

                        InventoryInputUpdateData(inventoryObj, req, InputCalTotalMoney(validate.Data.Select(x => x.Detail).ToList()));

                        // await _stockDbContext.InventoryDetail.AddRangeAsync(validate.Data);

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
            //inventoryObj.AccountancyAccountNumber = req.AccountancyAccountNumber;
            inventoryObj.UpdatedByUserId = _currentContextService.UserId;
            inventoryObj.TotalMoney = totalMoney;
            inventoryObj.DepartmentId = req.DepartmentId;
            inventoryObj.InventoryActionId = (int)req.InventoryActionId;

            if (inventoryObj.InventoryStatusId != (int)EnumInventoryStatus.Censored)
                inventoryObj.InventoryStatusId = (int)EnumInventoryStatus.Draff;

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


            if (inventoryObj.InventoryActionId == (int)EnumInventoryAction.Rotation)
            {
                throw GeneralCode.InvalidParams.BadRequestFormat(CannotUpdateInvInputRotation);
            }


            if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Input)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey()))//inventoryObj.StockId
            {

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        //reload inventory after lock
                        inventoryObj = _stockDbContext.Inventory.FirstOrDefault(p => p.InventoryId == inventoryId);

                        await DeleteInventoryInputDb(inventoryObj);

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

        public async Task DeleteInventoryInputDb(InventoryEntity inventoryObj)
        {
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


            inventoryObj.IsDeleted = true;
            //inventoryObj.IsApproved = false;

            var inventoryDetails = await _stockDbContext.InventoryDetail.Where(iv => iv.InventoryId == inventoryObj.InventoryId).ToListAsync();
            var arrInventoryDetailId = inventoryDetails.Select(x => x.InventoryDetailId);
            var inventoryDetailSubCalculations = await _stockDbContext.InventoryDetailSubCalculation.Where(d => arrInventoryDetailId.Contains(d.InventoryDetailId)).ToListAsync();

            foreach (var s in inventoryDetailSubCalculations)
            {
                s.IsDeleted = true;
                s.UpdatedDatetimeUtc = DateTime.UtcNow;
            }

            foreach (var item in inventoryDetails)
            {
                item.IsDeleted = true;
            }

            await _stockDbContext.SaveChangesAsync();
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


            if (inventoryObj.InventoryActionId == (int)EnumInventoryAction.Rotation)
            {
                throw GeneralCode.InvalidParams.BadRequestFormat(CannotUpdateInvInputRotation);
            }


            if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Input)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }

            await ValidateInventoryConfig(inventoryObj.Date, inventoryObj.Date);

            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey()))//inventoryObj.StockId
            {
                var baseValueChains = new Dictionary<string, int>();

                var ctx = _customGenCodeHelperService.CreateGenerateCodeContext(baseValueChains);
                var genCodeConfig = ctx.SetConfig(EnumObjectType.Package)
                                    .SetConfigData(0);


                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        //reload after lock
                        inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);

                        await ApproveInventoryInputDb(inventoryObj, genCodeConfig);


                        var inventoryDetails = _stockDbContext.InventoryDetail.Where(q => q.InventoryId == inventoryId).ToList();


                        trans.Commit();


                        await _invInputActivityLog.LogBuilder(() => InventoryBillInputActivityLogMessage.Approve)
                        .MessageResourceFormatDatas(inventoryObj.InventoryCode)
                        .ObjectId(inventoryId)
                        .JsonData(inventoryObj.JsonSerialize())
                        .CreateLog();


                        await UpdateProductionOrderStatus(inventoryDetails, EnumProductionStatus.Finished, inventoryObj.InventoryCode);

                        await ctx.ConfirmCode();

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

        public async Task ApproveInventoryInputDb(InventoryEntity inventoryObj, IGenerateCodeAction genCodeConfig)
        {
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

            await _stockDbContext.SaveChangesAsync();

            var inventoryDetails = _stockDbContext.InventoryDetail.Where(q => q.InventoryId == inventoryObj.InventoryId).ToList();

            var r = await ProcessInventoryInputApprove(inventoryObj.StockId, inventoryObj.Date, inventoryDetails, inventoryObj.InventoryCode, genCodeConfig);

            if (!r.IsSuccess())
            {

                throw new BadRequestException(r);
            }

            await ReCalculateRemainingAfterUpdate(inventoryObj.InventoryId);
        }

        public async Task<bool> SentToCensor(long inventoryId)
        {
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                var info = await _stockDbContext.Inventory.FirstOrDefaultAsync(d => d.InventoryId == inventoryId);
                if (info == null) throw new BadRequestException(InventoryErrorCode.InventoryNotFound);


                if (info.InventoryActionId == (int)EnumInventoryAction.Rotation)
                {
                    throw GeneralCode.InvalidParams.BadRequestFormat(CannotUpdateInvInputRotation);
                }


                if (info.InventoryStatusId != (int)EnumInventoryStatus.Draff && info.InventoryStatusId != (int)EnumInventoryStatus.Reject)
                {
                    throw new BadRequestException(InventoryErrorCode.InventoryNotDraffYet);
                }

                var details = await _stockDbContext.InventoryDetail.Where(d => d.InventoryId == inventoryId).ToListAsync();

                ValidatePackageInfos(details);

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

                await _invInputActivityLog.LogBuilder(() => InventoryBillInputActivityLogMessage.WaitToCensor)
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


                if (info.InventoryActionId == (int)EnumInventoryAction.Rotation)
                {
                    throw GeneralCode.InvalidParams.BadRequestFormat(CannotUpdateInvInputRotation);
                }


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

                await _invInputActivityLog.LogBuilder(() => InventoryBillInputActivityLogMessage.Reject)
                        .MessageResourceFormatDatas(info.InventoryCode)
                        .ObjectId(inventoryId)
                        .JsonData(info.JsonSerialize())
                        .CreateLog();


                return true;
            }
        }


        #region Private helper method

        private async Task<Enum> ProcessInventoryInputApprove(int stockId, DateTime date, IList<InventoryDetail> inventoryDetails, string inventoryCode, IGenerateCodeAction genCodeConfig)
        {
            var inputTransfer = new List<InventoryDetailToPackage>();
            var billPackages = new List<PackageEntity>();


            var newPackageCodes = inventoryDetails.Where(d => d.PackageOptionId == (int)EnumPackageOption.Create || d.PackageOptionId == (int)EnumPackageOption.CreateMerge)
                .Select(d => d.ToPackageInfo?.JsonDeserialize<PackageInputModel>()?.PackageCode)
                .Distinct()
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .ToList();
            if (newPackageCodes.Count > 0)
            {
                var existedPackages = await _stockDbContext.Package.Where(p => newPackageCodes.Contains(p.PackageCode)).ToListAsync();
                if (existedPackages.Count > 0)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể tạo kiện với mã kiện đã tồn tại: {string.Join(',', newPackageCodes)}");
                }
            }


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

                            var newPackage = await CreateNewPackage(stockId, date, item, inventoryCode, genCodeConfig);
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
                                var createPackage = await CreateNewPackage(stockId, date, item, inventoryCode, genCodeConfig);
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
                    var newPackage = await CreateNewPackage(stockId, date, item, inventoryCode, genCodeConfig);

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


        private async Task<ServiceResult<IList<CoupleDataInventoryDetail>>> ValidateInventoryIn(bool isApproved, InventoryInModel req, bool validatePackageInfo)
        {
            if (req.InProducts == null)
                req.InProducts = new List<InventoryInProductModel>();

            var productIds = req.InProducts.Select(p => p.ProductId).Distinct().ToList();

            var productInfos = (await _stockDbContext.Product.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToListAsync()).ToDictionary(p => p.ProductId, p => p);

            var productUnitConversions = await _stockDbContext.ProductUnitConversion.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToListAsync();

            foreach (var pu in productUnitConversions)
            {
                if (pu.IsDefault)
                {
                    pu.FactorExpression = "1";
                }
            }

            var toPackageIds = req.InProducts.Select(p => p.ToPackageId).ToList();
            var toPackages = await _stockDbContext.Package.Where(p => toPackageIds.Contains(p.PackageId) && p.PackageTypeId == (int)EnumPackageType.Custom).ToListAsync();

            var inventoryDetailList = new List<CoupleDataInventoryDetail>(req.InProducts.Count);
            foreach (var detail in req.InProducts)
            {
                if (EnumInventoryAction.InputOfMaterial == req.InventoryActionId && string.IsNullOrWhiteSpace(detail.POCode))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Nhập kho vật tư bắt buộc phải có mã đơn mua hàng");
                else if (EnumInventoryAction.InputOfProduct == req.InventoryActionId && string.IsNullOrWhiteSpace(detail.ProductionOrderCode))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Nhập kho thành phẩm bắt buộc phải có mã lệnh sản xuất");

                productInfos.TryGetValue(detail.ProductId, out var productInfo);
                if (productInfo == null)
                {
                    return ProductErrorCode.ProductNotFound;
                }
                var puDefault = productUnitConversions.FirstOrDefault(c => c.ProductId == detail.ProductId && c.IsDefault);

                var puInfo = productUnitConversions.FirstOrDefault(c => c.ProductUnitConversionId == detail.ProductUnitConversionId);
                if (puInfo == null)
                {
                    return ProductUnitConversionErrorCode.ProductUnitConversionNotFound;
                }
                if (puInfo.ProductId != detail.ProductId)
                {
                    return ProductUnitConversionErrorCode.ProductUnitConversionNotBelongToProduct;
                }

                if ((puInfo.IsFreeStyle ?? false) == false)
                {


                    var calcModel = new QuantityPairInputModel()
                    {
                        PrimaryQuantity = detail.PrimaryQuantity,
                        PrimaryDecimalPlace = puDefault?.DecimalPlace ?? 12,

                        PuQuantity = detail.ProductUnitConversionQuantity,
                        PuDecimalPlace = puInfo.DecimalPlace,

                        FactorExpression = puInfo.FactorExpression,

                        FactorExpressionRate = null
                    };


                    //  var (isSuccess, pucQuantity) = EvalUtils.GetProductUnitConversionQuantityFromPrimaryQuantity(details.PrimaryQuantity, puInfo.FactorExpression, details.ProductUnitConversionQuantity, puInfo.DecimalPlace);

                    var (isSuccess, primaryQuantity1, pucQuantity) = EvalUtils.GetProductUnitConversionQuantityFromPrimaryQuantity(calcModel);

                    if (isSuccess)
                    {
                        detail.ProductUnitConversionQuantity = pucQuantity;
                    }
                    else
                    {
                        _logger.LogWarning($"Wrong pucQuantity input data: PrimaryQuantity={detail.PrimaryQuantity}, FactorExpression={puInfo.FactorExpression}, ProductUnitConversionQuantity={detail.ProductUnitConversionQuantity}, evalData={pucQuantity}");
                        //return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                        throw PuConversionError.BadRequestFormat(puInfo.ProductUnitConversionName, productInfo.ProductCode);
                    }
                }

                if (!isApproved && detail.ProductUnitConversionQuantity <= 0)
                {
                    //return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                    throw PuConversionError.BadRequestFormat(puInfo.ProductUnitConversionName, productInfo.ProductCode);
                }

                // }

                if (!isApproved)
                {
                    if (detail.ProductUnitConversionQuantity <= 0 || detail.PrimaryQuantity <= 0)
                    {
                        return GeneralCode.InvalidParams;
                    }
                }

                switch (detail.PackageOptionId)
                {
                    case EnumPackageOption.Append:

                        var toPackageInfo = toPackages.FirstOrDefault(p => p.PackageId == detail.ToPackageId);
                        if (toPackageInfo == null) return PackageErrorCode.PackageNotFound;

                        if (toPackageInfo.ProductId != detail.ProductId
                            || toPackageInfo.ProductUnitConversionId != detail.ProductUnitConversionId
                            || toPackageInfo.StockId != req.StockId)
                        {
                            return InventoryErrorCode.InvalidPackage;
                        }
                        break;
                    case EnumPackageOption.Create:
                    case EnumPackageOption.CreateMerge:
                    case EnumPackageOption.NoPackageManager:

                        if (!isApproved && detail.ToPackageId.HasValue)
                        {
                            return GeneralCode.InvalidParams;
                        }
                        break;
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
                    InventoryDetailId = isApproved ? detail.InventoryDetailId ?? 0 : 0,
                    ProductId = detail.ProductId,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    RequestPrimaryQuantity = detail.RequestPrimaryQuantity?.RoundBy(puDefault.DecimalPlace),
                    PrimaryQuantity = detail.PrimaryQuantity.RoundBy(puDefault.DecimalPlace),
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
                    FromPackageId = null,
                    ToPackageId = detail.ToPackageId,
                    ToPackageInfo = detail.ToPackageInfo?.JsonSerialize(),
                    PackageOptionId = (int)detail.PackageOptionId,
                    SortOrder = detail.SortOrder,
                    Description = detail.Description,
                    //AccountancyAccountNumberDu = details.AccountancyAccountNumberDu,
                    InventoryRequirementDetailId = detail.InventoryRequirementDetailId,
                    //InventoryRequirementCode = detail.InventoryRequirementCode
                    IsSubCalculation = detail.IsSubCalculation
                };


                if (eDetail.PrimaryQuantity == 0 || eDetail.ProductUnitConversionQuantity == 0)
                {
                    throw GeneralCode.InvalidParams.BadRequest("Invalid data");
                }

                if (detail.InProductSubs == null)
                {
                    detail.InProductSubs = new List<InventoryDetailSubCalculationModel>();
                }

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


            }

            if (validatePackageInfo)
            {
                ValidatePackageInfos(inventoryDetailList.Select(x => x.Detail).ToList());
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
                ensureDefaultPackage = new PackageEntity()
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

        private async Task<PackageEntity> CreateNewPackage(int stockId, DateTime date, InventoryDetail detail, string inventoryCode, IGenerateCodeAction genCodeConfig)
        {
            PackageInputModel packageInfo = null;

            int? locationId = null;

            if (!string.IsNullOrWhiteSpace(detail.ToPackageInfo))
            {
                packageInfo = detail.ToPackageInfo.JsonDeserialize<PackageInputModel>();

                if (packageInfo?.LocationId > 0)
                {
                    if (await _stockDbContext.Location.AnyAsync(l => l.LocationId == packageInfo.LocationId))
                    {
                        locationId = packageInfo.LocationId;
                    }
                }
            }

            var packageCode = packageInfo?.PackageCode;

            if (string.IsNullOrWhiteSpace(packageCode))
            {
                packageCode = await genCodeConfig.TryValidateAndGenerateCode(_stockDbContext.Inventory, packageCode, null);
            }


            var newPackage = new PackageEntity()
            {
                PackageTypeId = (int)EnumPackageType.Custom,
                PackageCode = packageCode,
                LocationId = locationId,
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
                ExpiryTime = packageInfo?.ExpiryTime.UnixToDateTime(),
                Description = packageInfo.Description ?? inventoryCode,
                OrderCode = packageInfo.OrderCode ?? detail.OrderCode,
                Pocode = packageInfo.POCode ?? detail.Pocode,
                ProductionOrderCode = packageInfo.ProductionOrderCode ?? detail.ProductionOrderCode,
                CustomPropertyValue = packageInfo?.CustomPropertyValue?.JsonSerialize()
            };
            await _stockDbContext.Package.AddAsync(newPackage);
            await _stockDbContext.SaveChangesAsync();

            // await _customGenCodeHelperService.ConfirmCode(config.CurrentLastValue);

            await _packageActivityLog.LogBuilder(() => InventoryBillInputActivityLogMessage.CreatePackage)
               .MessageResourceFormatDatas(newPackage.PackageCode, inventoryCode)
               .ObjectId(newPackage.PackageId)
               .JsonData(newPackage.JsonSerialize())
               .CreateLog();


            return newPackage;
        }


        private void ValidatePackageInfos(IList<InventoryDetail> details)
        {
            foreach (var d in details)
            {
                if (!string.IsNullOrWhiteSpace(d.ToPackageInfo))
                {
                    var packageInfo = d.ToPackageInfo.JsonDeserialize<PackageInputModel>();
                    var desc = "Thông tin kiện";
                    Utils.ValidateCodeSpecialCharactors(packageInfo.PackageCode, desc);
                    Utils.ValidateCodeSpecialCharactors(packageInfo.POCode, desc);
                    Utils.ValidateCodeSpecialCharactors(packageInfo.ProductionOrderCode, desc);
                    Utils.ValidateCodeSpecialCharactors(packageInfo.OrderCode, desc);
                }

            }
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
