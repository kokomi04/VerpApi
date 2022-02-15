using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using Verp.Resources.Stock.InventoryProcess;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Service.Inventory.Implement.Abstract;
using VErp.Services.Stock.Service.Stock;
using VErp.Services.Stock.Service.Stock.Implement;
using InventoryEntity = VErp.Infrastructure.EF.StockDB.Inventory;
using static Verp.Resources.Stock.InventoryProcess.InventoryBillOutputMessage;
using static Verp.Resources.Stock.InventoryProcess.InventoryBillInputMessage;
using System.Linq.Expressions;

namespace VErp.Services.Stock.Service.Inventory.Implement.InventoryProcess
{
    internal class InventoryRotationService : InventoryServiceAbstract, IInventoryRotationService
    {
        private readonly IInventoryBillInputService _inventoryBillInputService;
        private readonly IInventoryBillOutputService _inventoryBillOutputService;
        private readonly ObjectActivityLogFacade invActivityLogFacade;
        private readonly ObjectActivityLogFacade outActivityLogFacade;
        private readonly IActivityLogService activityLogService;
        private readonly ICurrentContextService contextService;
        public InventoryRotationService(
            StockDBContext stockContext,
            ILogger<InventoryRotationService> logger,
            ICustomGenCodeHelperService customGenCodeHelperService,
            IProductionOrderHelperService productionOrderHelperService,
            IProductionHandoverHelperService productionHandoveHelperService,
            ICurrentContextService currentContextService,
            IInventoryBillInputService inventoryBillInputService,
            IInventoryBillOutputService inventoryBillOutputService,
            IActivityLogService activityLogService, ICurrentContextService contextService)
            : base(stockContext, logger, customGenCodeHelperService, productionOrderHelperService, productionHandoveHelperService, currentContextService)
        {
            _inventoryBillInputService = inventoryBillInputService;
            _inventoryBillOutputService = inventoryBillOutputService;
            invActivityLogFacade = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.InventoryInput);
            outActivityLogFacade = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.InventoryOutput);
            this.activityLogService = activityLogService;
            this.contextService = contextService;
        }


        public async Task<long> Create(InventoryOutRotationModel req)
        {
            await ValidateInventoryConfig(req.Date.UnixToDateTime(), req.Date.UnixToDateTime());

            req.InventoryActionId = EnumInventoryAction.Rotation;

            if (req.StockId <= 0 || req.ToStockId <= 0)
            {
                throw GeneralCode.InvalidParams.BadRequest(RotationStockIsRequired);
            }

            if (req.StockId == req.ToStockId)
            {
                throw GeneralCode.InvalidParams.BadRequest(RotationRequireDiffStock);
            }


            var genCodeContexts = new List<GenerateCodeContext>();
            var baseValueChains = new Dictionary<string, int>();

            long inventoryId;

            using (var @lock1 = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(req.StockId)))
            using (var @lock2 = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(req.ToStockId)))
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                if (req?.OutProducts == null || req?.OutProducts?.Count == 0)
                {
                    throw new BadRequestException("No products found!");
                }

                genCodeContexts.Add(await GenerateInventoryCode(EnumInventoryType.Output, req, baseValueChains));

                var outInv = await _inventoryBillOutputService.AddInventoryOutputDb(req);

                inventoryId = outInv.InventoryId;



                var inputReq = TransferInvInputModel(req);

                genCodeContexts.Add(await GenerateInventoryCode(EnumInventoryType.Input, inputReq, baseValueChains));

                var inInv = await _inventoryBillInputService.AddInventoryInputDB(inputReq);


                inInv.RefInventoryId = outInv.InventoryId;
                outInv.RefInventoryId = inInv.InventoryId;

                await _stockDbContext.SaveChangesAsync();

                await trans.CommitAsync();

                await AcivitityLog(outInv, inInv, () => InventoryBillOutputActivityMessage.RotationCreate);

            }

            foreach (var item in genCodeContexts)
            {
                await item.ConfirmCode();
            }

            return inventoryId;

        }

        public async Task<bool> Approve(long inventoryId)
        {
            var (outputObj, inputObj) = await Validate(inventoryId);


            ValidateApprove(outputObj);

            ValidateApprove(inputObj);

            var baseValueChains = new Dictionary<string, int>();

            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext(baseValueChains);
            var genCodeConfig = ctx.SetConfig(EnumObjectType.Package)
                                .SetConfigData(0);


            using (var @lock1 = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(outputObj.StockId)))
            using (var @lock2 = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(inputObj.StockId)))



            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {

                try
                {
                    outputObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);

                    await _inventoryBillOutputService.ApproveInventoryOutputDb(outputObj);

                    inputObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == outputObj.RefInventoryId);

                    await _inventoryBillInputService.ApproveInventoryInputDb(inputObj, genCodeConfig);


                    trans.Commit();


                    await AcivitityLog(outputObj, inputObj, () => InventoryBillOutputActivityMessage.RotationApprove);


                    var outDetails = await _stockDbContext.InventoryDetail.Where(d => d.InventoryId == inventoryId).ToListAsync();

                    await UpdateProductionOrderStatus(outDetails, EnumProductionStatus.Processing, outputObj.InventoryCode);

                    await UpdateIgnoreAllocation(outDetails);


                    var intDetails = await _stockDbContext.InventoryDetail.Where(d => d.InventoryId == inputObj.InventoryId).ToListAsync();

                    await UpdateProductionOrderStatus(intDetails, EnumProductionStatus.Processing, inputObj.InventoryCode);

                    await UpdateIgnoreAllocation(intDetails);

                    await ctx.ConfirmCode();

                    return true;
                }
                catch (Exception ex)
                {
                    trans.TryRollbackTransaction();
                    _logger.LogError(ex, "ApproveInventoryOutputRotation");
                    throw;
                }
            }


        }

        public async Task<bool> SentToCensor(long inventoryId)
        {

            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                var info = await _stockDbContext.Inventory.FirstOrDefaultAsync(d => d.InventoryId == inventoryId);
                if (info == null) throw new BadRequestException(InventoryErrorCode.InventoryNotFound);

                if (info.IsApproved || info.InventoryTypeId != (int)EnumInventoryType.Output || info.InventoryActionId != (int)EnumInventoryAction.Rotation)
                {
                    throw GeneralCode.InvalidParams.BadRequest();
                }

                var refInfo = await _stockDbContext.Inventory.FirstOrDefaultAsync(d => d.InventoryId == info.RefInventoryId);
                await SentToCensor(refInfo);

                await SentToCensor(info);

                trans.Commit();

                await AcivitityLog(info, refInfo, () => InventoryBillOutputActivityMessage.RotationApprove);

                return true;
            }
        }

        private async Task SentToCensor(InventoryEntity info)
        {

            if (info.InventoryStatusId != (int)EnumInventoryStatus.Draff && info.InventoryStatusId != (int)EnumInventoryStatus.Reject)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotDraffYet);
            }

            info.InventoryStatusId = (int)EnumInventoryStatus.WaitToCensor;

            await _stockDbContext.SaveChangesAsync();

        }

        public async Task<bool> Reject(long inventoryId)
        {
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                var info = await _stockDbContext.Inventory.FirstOrDefaultAsync(d => d.InventoryId == inventoryId);
                if (info == null) throw new BadRequestException(InventoryErrorCode.InventoryNotFound);


                var refInfo = await _stockDbContext.Inventory.FirstOrDefaultAsync(d => d.InventoryId == info.RefInventoryId);
                await Reject(refInfo);


                await Reject(info);

                trans.Commit();


                await AcivitityLog(info, refInfo, () => InventoryBillOutputActivityMessage.RotationReject);

                return true;
            }
        }

        private async Task Reject(InventoryEntity info)
        {

            if (info.InventoryStatusId != (int)EnumInventoryStatus.WaitToCensor)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotSentToCensorYet);
            }

            info.IsApproved = false;

            info.InventoryStatusId = (int)EnumInventoryStatus.Reject;
            info.CensorDatetimeUtc = DateTime.UtcNow;
            info.CensorByUserId = contextService.UserId;

            await _stockDbContext.SaveChangesAsync();

        }

        public async Task<bool> NotApprovedDelete(long outInvId)
        {
            var (outputObj, inputObj) = await Validate(outInvId);


            if (outputObj.IsApproved)
            {
                throw GeneralCode.InvalidParams.BadRequest();
            }

            if (inputObj.IsApproved)
            {
                throw GeneralCode.InvalidParams.BadRequest();
            }


            using var @lockOut = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(outputObj.StockId));
            using var @lockIn = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(inputObj.StockId));

            using var trans = await _stockDbContext.Database.BeginTransactionAsync();
            try
            {
                await _inventoryBillOutputService.DeleteInventoryOutputDb(outputObj);

                await _inventoryBillInputService.DeleteInventoryInputDb(inputObj);

                trans.Commit();


                await AcivitityLog(outputObj, inputObj, () => InventoryBillOutputActivityMessage.RotationDelete);

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Rotation NotApprovedDelete");
                throw;
            }

        }

        public async Task<bool> ApprovedDelete(long outInvId, long fromDate, long toDate, ApprovedInputDataSubmitModel req)
        {
            req.Inventory.InProducts = new List<InventoryInProductModel>();

            var (outputObj, inputObj) = await Validate(outInvId);


            if (!outputObj.IsApproved)
            {
                throw GeneralCode.InvalidParams.BadRequest();
            }

            if (!inputObj.IsApproved)
            {
                throw GeneralCode.InvalidParams.BadRequest();
            }

            var baseValueChains = new Dictionary<string, int>();

            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext(baseValueChains);
            var genCodeConfig = ctx.SetConfig(EnumObjectType.Package)
                                .SetConfigData(0);

            using var @lockOut = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(outputObj.StockId));
            using var @lockIn = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(inputObj.StockId));
            using var trans = await _stockDbContext.Database.BeginTransactionAsync();
            try
            {
                await _inventoryBillOutputService.DeleteInventoryOutputDb(outputObj);

                var (affectedInventoryIds, isDeleted) = await _inventoryBillInputService.ApprovedInputDataUpdateDb(outputObj.RefInventoryId.Value, fromDate, toDate, req, genCodeConfig);

                if (!isDeleted)
                {
                    throw GeneralCode.InvalidParams.BadRequest();
                }

                trans.Commit();

                await AcivitityLog(outputObj, inputObj, () => InventoryBillOutputActivityMessage.RotationDelete);
                
                await ctx.ConfirmCode();

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Rotation ApprovedDelete");
                throw;
            }


        }

        private async Task AcivitityLog<T>(InventoryEntity outputObj, InventoryEntity inputObj, Expression<Func<T>> messageResourceName)
        {
            await outActivityLogFacade.LogBuilder(messageResourceName)
                   .MessageResourceFormatDatas(outputObj.InventoryCode, inputObj.InventoryCode)
                          .ObjectId(outputObj.InventoryId)
                          .JsonData(outputObj.JsonSerialize())
                          .CreateLog();

            await invActivityLogFacade.LogBuilder(messageResourceName)
                 .MessageResourceFormatDatas(outputObj.InventoryCode, inputObj.InventoryCode)
               .ObjectId(inputObj.InventoryId)
               .JsonData(inputObj.JsonSerialize())
               .CreateLog();
        }

        private void ValidateApprove(InventoryEntity inv)
        {

            if (inv.IsApproved || (inv.InventoryStatusId != (int)EnumInventoryStatus.Reject && inv.InventoryStatusId != (int)EnumInventoryStatus.WaitToCensor))
            {
                throw GeneralCode.InvalidParams.BadRequest();
            }
        }

        private async Task<(InventoryEntity outInv, InventoryEntity inInv)> Validate(long outInvId)
        {
            if (outInvId <= 0)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }

            var outputObj = await _stockDbContext.Inventory.FirstOrDefaultAsync(iv => iv.InventoryId == outInvId);
            if (outputObj == null || outputObj.InventoryTypeId != (int)EnumInventoryType.Output)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }


            await ValidateInventoryConfig(outputObj.Date, outputObj.Date);

            var inputObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == outputObj.RefInventoryId);

            if (inputObj == null || inputObj.InventoryTypeId != (int)EnumInventoryType.Input)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }

            await ValidateInventoryConfig(inputObj.Date, inputObj.Date);

            if (inputObj.InventoryActionId != (int)EnumInventoryAction.Rotation || outputObj.InventoryActionId != (int)EnumInventoryAction.Rotation)
            {
                throw GeneralCode.InvalidParams.BadRequest();
            }

            return (outputObj, inputObj);
        }

        private InventoryInModel TransferInvInputModel(InventoryOutRotationModel req)
        {
            return new InventoryInModel()
            {
                StockId = req.ToStockId,
                InventoryCode = req.ToInventoryCode,
                Date = req.Date,
                InventoryActionId = EnumInventoryAction.Rotation,

                Shipper = req.Shipper,
                Content = req.Content,

                CustomerId = req.CustomerId,
                Department = req.Department,
                StockKeeperUserId = req.StockKeeperUserId,

                BillForm = req.BillForm,
                BillCode = req.BillCode,
                BillSerial = req.BillSerial,
                BillDate = req.BillDate,

                DepartmentId = req.DepartmentId,

                FileIdList = req.FileIdList,

                InProducts = req.OutProducts.Select(p => new InventoryInProductModel()
                {
                    InventoryDetailId = null,
                    ProductId = p.ProductId,

                    RequestPrimaryQuantity = p.RequestPrimaryQuantity,

                    PrimaryQuantity = p.PrimaryQuantity,
                    UnitPrice = p.UnitPrice,

                    ProductUnitConversionId = p.ProductUnitConversionId,
                    RequestProductUnitConversionQuantity = p.RequestProductUnitConversionQuantity,
                    ProductUnitConversionQuantity = p.ProductUnitConversionQuantity,
                    ProductUnitConversionPrice = p.ProductUnitConversionPrice,

                    RefObjectTypeId = p.RefObjectTypeId,
                    RefObjectId = p.RefObjectId,
                    RefObjectCode = p.RefObjectCode,

                    OrderCode = p.OrderCode,

                    POCode = p.POCode,

                    ProductionOrderCode = p.ProductionOrderCode,

                    ToPackageId = null,// p.ToPackageId,

                    PackageOptionId = EnumPackageOption.NoPackageManager,// p.PackageOptionId,

                    SortOrder = p.SortOrder,
                    Description = p.Description,

                    InventoryRequirementDetailId = p.InventoryRequirementDetailId,
                    InventoryRequirementCode = p.InventoryRequirementCode

                }).ToList()
            };
        }


    }
}
