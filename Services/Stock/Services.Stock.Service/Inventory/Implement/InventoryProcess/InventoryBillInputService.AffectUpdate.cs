﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using Verp.Resources.Stock.InventoryProcess;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Services.Stock.Model.Inventory;
using InventoryEntity = VErp.Infrastructure.EF.StockDB.Inventory;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public partial class InventoryBillInputService
    {
        public async Task<IList<CensoredInventoryInputProducts>> InputUpdateGetAffectedPackages(long inventoryId, long fromDate, long toDate, InventoryInModel req)
        {
            await ValidateInventoryCode(inventoryId, req.InventoryCode);

            var data = await CensoredInventoryInputUpdateGetAffected(inventoryId, fromDate, toDate, req);
            return data.Products.ToList();

        }


        public async Task<bool> ApprovedInputDataUpdate(long inventoryId, long fromDate, long toDate, ApprovedInputDataSubmitModel req)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey());//req.Inventory.StockId

            var baseValueChains = new Dictionary<string, int>();

            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext(baseValueChains);
            var genCodeConfig = ctx.SetConfig(EnumObjectType.Package)
                                .SetConfigData(0);
          
            using var trans = await _stockDbContext.Database.BeginTransactionAsync();
            using var logBatch = _activityLogService.BeginBatchLog();
            try
            {
                var (affectedDetails, isDeleted) = await ApprovedInputDataUpdateDb(inventoryId, fromDate, toDate, req, genCodeConfig);

                trans.Commit();

                await _invInputActivityLog.LogBuilder(() => InventoryBillInputActivityLogMessage.UpdateAndApprove)
                   .MessageResourceFormatDatas(req?.Inventory?.InventoryCode)
                   .ObjectId(inventoryId)
                   .JsonData(req)
                   .CreateLog();

                if (isDeleted)
                {
                    await _invInputActivityLog.LogBuilder(() => InventoryBillInputActivityLogMessage.DeleteCauseByNoProduct)
                     .MessageResourceFormatDatas(req?.Inventory?.InventoryCode)
                     .ObjectId(inventoryId)
                     .Action(EnumActionType.Delete)
                     .JsonData(req)
                     .CreateLog();
                }

                await logBatch.CommitAsync();

                await ctx.ConfirmCode();

                await UpdateProductionOrderStatus(affectedDetails, req?.Inventory?.InventoryCode);

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "ApprovedInputDataUpdate");
                throw;
            }
        }

        public async Task<(IList<InventoryDetail> affectedDetails, bool isDeleted)> ApprovedInputDataUpdateDb(long inventoryId, long fromDate, long toDate, ApprovedInputDataSubmitModel req, IGenerateCodeAction genCodeConfig)
        {
            await ValidateInventoryCode(inventoryId, req.Inventory.InventoryCode);

            var (affectedDetails, isDeleted) = await ApprovedInputDataUpdateAction(inventoryId, fromDate, toDate, req, genCodeConfig);

            foreach (var changedInventoryId in affectedDetails.Select(d=>d.InventoryId).Distinct().ToList())
            {
                await ReCalculateRemainingAfterUpdate(changedInventoryId, inventoryId);
            }
            return (affectedDetails, isDeleted);
        }

        private async Task<InventoryInputUpdateGetAffectedModel> CensoredInventoryInputUpdateGetAffected(long inventoryId, long fromDate, long toDate, InventoryInModel req)
        {
            var inventoryInfo = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);
            if (inventoryInfo == null)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }
            if (inventoryInfo.StockId != req.StockId)
            {
                throw new BadRequestException(InventoryErrorCode.CanNotChangeStock);
            }

            if (!inventoryInfo.IsApproved)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            if (inventoryInfo.InventoryTypeId != (int)EnumInventoryType.Input)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            var details = await _stockDbContext.InventoryDetail.Where(iv => iv.InventoryId == inventoryId).ToListAsync();

            // Validate nếu thông tin nhập kho tạo từ phiếu yêu cầu => không cho phép thêm/sửa mặt hàng
            if (details.Any(id => id.InventoryRequirementDetailId.HasValue && id.InventoryRequirementDetailId > 0))
            {
                if (req.InProducts.Any(d => !details.Any(id => id.ProductId == d.ProductId && id.InventoryRequirementDetailId == d.InventoryRequirementDetailId)))
                {
                    throw new BadRequestException(InventoryErrorCode.CanNotChangeProductInventoryHasRequirement);
                }
            }

            var deletedDetails = details.Where(d => !req.InProducts.Select(u => u.InventoryDetailId).Contains(d.InventoryDetailId));

            var updateDetail = await ValidateInventoryIn(true, req, true);

            if (!updateDetail.Code.IsSuccess())
            {
                throw new BadRequestException(updateDetail.Code);
            }

            var productIds = details.Select(d => d.ProductId);
            var productInfos = _stockDbContext.Product
                .Where(p => productIds.Contains(p.ProductId))
                .AsNoTracking()
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductCode,
                    p.UnitId
                })
                .ToList()
                .ToDictionary(p => p.ProductId, p => p);

            var productUnitConversions = (await _stockDbContext.ProductUnitConversion.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToListAsync()).ToDictionary(pu => pu.ProductUnitConversionId, pu => pu);

            var products = new List<CensoredInventoryInputProducts>();

            foreach (var d in details)
            {
                decimal newProductUnitConversionQuantity = 0;
                decimal newPrimaryQuantity = 0;

                if (deletedDetails.Any(id => id.InventoryDetailId == d.InventoryDetailId))
                {
                    newProductUnitConversionQuantity = 0;
                    newPrimaryQuantity = 0;
                }

                var newDetail = updateDetail.Data.Select(x => x.Detail).ToList().FirstOrDefault(id => id.InventoryDetailId == d.InventoryDetailId);
                if (newDetail != null)
                {
                    newProductUnitConversionQuantity = newDetail.ProductUnitConversionQuantity;
                    newPrimaryQuantity = newDetail.PrimaryQuantity;
                }

                productUnitConversions.TryGetValue(d.ProductUnitConversionId, out var conversionInfo);

                if (!productInfos.TryGetValue(d.ProductId, out var productInfo))
                {
                    throw new KeyNotFoundException("Product not found " + d.ProductId);
                }

                var censoredInvInput = new CensoredInventoryInputUpdateContext(_stockDbContext, inventoryInfo, d, fromDate, toDate);

                var product = new CensoredInventoryInputProducts()
                {
                    InventoryDetailId = d.InventoryDetailId,
                    ProductId = d.ProductId,
                    ProductCode = productInfo.ProductCode,
                    PrimaryUnitId = productInfo.UnitId,


                    OldPrimaryQuantity = d.PrimaryQuantity,
                    NewPrimaryQuantity = newPrimaryQuantity,

                    ProductUnitConversionId = d.ProductUnitConversionId,
                    ProductUnitConversionName = conversionInfo?.ProductUnitConversionName,
                    FactorExpression = conversionInfo?.FactorExpression,

                    OldProductUnitConversionQuantity = d.ProductUnitConversionQuantity,
                    NewProductUnitConversionQuantity = newProductUnitConversionQuantity,
                    ToPackageId = d.ToPackageId.Value,
                    AffectObjects = censoredInvInput.GetAffectObjects(newPrimaryQuantity, newProductUnitConversionQuantity)
                };

                products.Add(product);

            }

            return new InventoryInputUpdateGetAffectedModel { Products = products, DbDetails = details, UpdateDetails = updateDetail.Data.Select(x => x.Detail).ToList() };
        }

        private async Task<(IList<InventoryDetail> affectedDetails, bool isDeleted)> ApprovedInputDataUpdateAction(long inventoryId, long fromDate, long toDate, ApprovedInputDataSubmitModel req, IGenerateCodeAction genCodeConfig)
        {

            var inventoryInfo = await _stockDbContext.Inventory.FirstOrDefaultAsync(iv => iv.InventoryId == inventoryId);

            if (!inventoryInfo.IsApproved)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotApprovedYet);
            }


            var data = await CensoredInventoryInputUpdateGetAffected(inventoryId, fromDate, toDate, req.Inventory);

            var products = data.Products;
            var dbDetails = data.DbDetails;
            var updateDetails = data.UpdateDetails;

            IList<InventoryDetail> affectedDetails = new List<InventoryDetail>();

            foreach (var inventoryDetail in dbDetails)
            {
                if (!affectedDetails.Contains(inventoryDetail))
                {
                    affectedDetails.Add(inventoryDetail);
                }
            }

            foreach (var inventoryDetail in updateDetails)
            {
                if (!affectedDetails.Contains(inventoryDetail))
                {
                    affectedDetails.Add(inventoryDetail);
                }
            }

            await ApprovedInputDataUpdateAction_Normalize(req, products);
            var issuedDate = req.Inventory.Date.UnixToDateTime().Value;

            //need to update before validate quantity order by time
            inventoryInfo.Date = issuedDate;

            var refChangesDetails = await ApprovedInputDataUpdateAction_Update(req, products, dbDetails);

            foreach (var inventoryDetail in refChangesDetails)
            {
                if (!affectedDetails.Contains(inventoryDetail))
                {
                    affectedDetails.Add(inventoryDetail);
                }
            }

            var billDate = req.Inventory.BillDate?.UnixToDateTime();

            await _stockDbContext.SaveChangesAsync();


            //add new details
            var newDetails = updateDetails.Where(d => d.InventoryDetailId <= 0).ToList();

            foreach (var d in newDetails)
            {
                d.InventoryId = inventoryId;
            }

            _stockDbContext.InventoryDetail.AddRange(newDetails);
            _stockDbContext.SaveChanges();


            //
            var isDelete = !(await _stockDbContext.InventoryDetail.AnyAsync(d => d.InventoryId == inventoryId && d.PrimaryQuantity > 0));

            if (!isDelete)
            {
                var r = await ProcessInventoryInputApprove(inventoryInfo.StockId, inventoryInfo.Date, newDetails, inventoryInfo.InventoryCode, genCodeConfig);
                if (!r.IsSuccess())
                {
                    throw new BadRequestException(r);
                }

                var totalMoney = InputCalTotalMoney(updateDetails);
                InventoryInputUpdateData(inventoryInfo, req.Inventory, totalMoney);

            }
            else
            {
                inventoryInfo.IsDeleted = true;
                inventoryInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
                inventoryInfo.UpdatedByUserId = _currentContextService.UserId;
            }


            await _stockDbContext.SaveChangesAsync();

            //if (!updateResult.Contains(inventoryId))
            //{
            //    updateResult.Add(inventoryId);               
            //}
          

            return (affectedDetails, inventoryInfo.IsDeleted);
        }

        private async Task ApprovedInputDataUpdateAction_Normalize(ApprovedInputDataSubmitModel req, IList<CensoredInventoryInputProducts> products)
        {
            var puIds = products.Select(p => p.ProductUnitConversionId).ToList();

            var productIds = products.Select(p => p.ProductId).ToList();
            var productUnits = await _stockDbContext.ProductUnitConversion.Where(pu => productIds.Contains(pu.ProductId)).ToListAsync();

            var puConversions = productUnits.ToDictionary(pu => pu.ProductUnitConversionId, pu => pu);


            var defaults = productUnits.Where(pu => pu.IsDefault).GroupBy(pu => pu.ProductId).ToDictionary(pu => pu.Key, pu => pu.FirstOrDefault());

            foreach (var p in products)
            {
                var updateProduct = req.AffectDetails.FirstOrDefault(d => d.InventoryDetailId == p.InventoryDetailId);
                if (updateProduct == null) continue;

                if (!puConversions.TryGetValue(p.ProductUnitConversionId, out var productUnitConversionInfo))
                {
                    throw new BadRequestException(ProductUnitConversionErrorCode.ProductUnitConversionNotFound);
                }

                if (p.NewPrimaryQuantity.SubDecimal(p.OldPrimaryQuantity) == 0)
                {
                    p.NewPrimaryQuantity = p.OldPrimaryQuantity;
                }

                if (p.NewProductUnitConversionQuantity.SubDecimal(p.OldProductUnitConversionQuantity) == 0 || p.NewPrimaryQuantity == p.OldPrimaryQuantity)
                {
                    p.NewProductUnitConversionQuantity = p.OldProductUnitConversionQuantity;
                }

                if (p.NewProductUnitConversionQuantity == p.OldProductUnitConversionQuantity)
                {
                    p.NewPrimaryQuantity = p.OldPrimaryQuantity;
                }

                defaults.TryGetValue(p.ProductId, out var puDefault);

                foreach (var obj in p.AffectObjects)
                {
                    if (productUnitConversionInfo.IsFreeStyle == false)
                    {

                        if (obj.NewPrimaryQuantity >= 0)
                        {

                            //var primaryQualtity = Utils.GetPrimaryQuantityFromProductUnitConversionQuantity(obj.NewProductUnitConversionQuantity, productUnitConversionInfo.FactorExpression);

                            bool isSuccess = false;
                            decimal pucQuantity = 0;

                            decimal primaryQuantity = 0;

                            //if (obj.OldPrimaryQuantity != 0)
                            //{
                            //    (isSuccess, pucQuantity) = EvalUtils.GetProductUnitConversionQuantityFromPrimaryQuantity(obj.NewPrimaryQuantity, obj.OldProductUnitConversionQuantity / obj.OldPrimaryQuantity, obj.NewProductUnitConversionQuantity);

                            //}
                            //else
                            {
                                if (obj.OldPrimaryQuantity.SubDecimal(obj.NewPrimaryQuantity) == 0)
                                {
                                    (isSuccess, pucQuantity) = (true, obj.OldProductUnitConversionQuantity);
                                }
                                else
                                {

                                    var calcModel = new QuantityPairInputModel()
                                    {
                                        PrimaryQuantity = obj.NewPrimaryQuantity,
                                        PrimaryDecimalPlace = puDefault?.DecimalPlace ?? 12,

                                        PuQuantity = obj.NewProductUnitConversionQuantity,
                                        PuDecimalPlace = productUnitConversionInfo.DecimalPlace,

                                        FactorExpression = productUnitConversionInfo.FactorExpression,

                                        FactorExpressionRate = null
                                    };

                                    //(isSuccess, pucQuantity) = EvalUtils.GetProductUnitConversionQuantityFromPrimaryQuantity(obj.NewPrimaryQuantity, productUnitConversionInfo.FactorExpression, obj.NewProductUnitConversionQuantity, productUnitConversionInfo.DecimalPlace);

                                    (isSuccess, primaryQuantity, pucQuantity) = EvalUtils.GetProductUnitConversionQuantityFromPrimaryQuantity(calcModel);
                                }
                            }

                            if (isSuccess)
                            {
                                obj.NewProductUnitConversionQuantity = pucQuantity;
                            }
                            else
                            {
                                throw new BadRequestException(ProductUnitConversionErrorCode.SecondaryUnitConversionError);
                            }

                            if (!(obj.NewProductUnitConversionQuantity > 0) && obj.NewPrimaryQuantity > 0)
                            {
                                throw new BadRequestException(ProductUnitConversionErrorCode.SecondaryUnitConversionError);
                            }

                            //if (obj.NewPrimaryQuantity == obj.OldPrimaryQuantity)
                            //{
                            //    obj.NewProductUnitConversionQuantity = obj.OldProductUnitConversionQuantity;
                            //}
                        }
                        else
                        {
                            throw new Exception($"Negative PrimaryQuantity {obj.ObjectTypeId} {obj.ObjectId} {obj.ObjectCode}");
                        }
                    }



                    if (obj.NewPrimaryQuantity.SubDecimal(obj.OldPrimaryQuantity) == 0)
                    {
                        obj.NewPrimaryQuantity = obj.OldPrimaryQuantity;
                    }

                    if (obj.NewProductUnitConversionQuantity.SubDecimal(obj.OldProductUnitConversionQuantity) == 0 || obj.NewPrimaryQuantity == obj.OldPrimaryQuantity)
                    {
                        obj.NewProductUnitConversionQuantity = obj.OldProductUnitConversionQuantity;
                    }

                    if (obj.NewPrimaryQuantity == obj.OldPrimaryQuantity)
                    {
                        obj.NewProductUnitConversionQuantity = obj.OldProductUnitConversionQuantity;
                    }


                    if ((obj.NewProductUnitConversionQuantity != 0 || obj.NewPrimaryQuantity != 0)
                        && (obj.NewProductUnitConversionQuantity <= 0 || obj.NewPrimaryQuantity <= 0)
                        )
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams);
                    }

                    if (obj.Children != null)
                    {
                        foreach (var c in obj.Children)
                        {
                            if ((productUnitConversionInfo.IsFreeStyle ?? false) == false)
                            {
                                if (c.NewTransferPrimaryQuantity >= 0)
                                {
                                    //var primaryQualtity = Utils.GetPrimaryQuantityFromProductUnitConversionQuantity(c.NewTransferProductUnitConversionQuantity, productUnitConversionInfo.FactorExpression);

                                    //c.NewTransferProductUnitConversionQuantity = c.NewTransferPrimaryQuantity * c.OldTransferProductUnitConversionQuantity / c.OldTransferPrimaryQuantity;

                                    bool isSuccess = false;
                                    decimal pucQuantity = 0;
                                    decimal primaryQuantity = 0;

                                    //if (c.OldTransferPrimaryQuantity != 0)
                                    //{
                                    //    (isSuccess, pucQuantity) = EvalUtils.GetProductUnitConversionQuantityFromPrimaryQuantity(c.NewTransferPrimaryQuantity, c.OldTransferProductUnitConversionQuantity / c.OldTransferPrimaryQuantity, c.NewTransferProductUnitConversionQuantity);

                                    //}
                                    //else
                                    {
                                        if (c.NewTransferPrimaryQuantity.SubDecimal(c.OldTransferPrimaryQuantity) == 0)
                                        {
                                            (isSuccess, pucQuantity) = (true, c.OldTransferProductUnitConversionQuantity);
                                        }
                                        else
                                        {
                                            var calcModel = new QuantityPairInputModel()
                                            {
                                                PrimaryQuantity = c.NewTransferPrimaryQuantity,
                                                PrimaryDecimalPlace = puDefault?.DecimalPlace ?? 12,

                                                PuQuantity = c.NewTransferProductUnitConversionQuantity,
                                                PuDecimalPlace = productUnitConversionInfo.DecimalPlace,

                                                FactorExpression = productUnitConversionInfo.FactorExpression,

                                                FactorExpressionRate = null
                                            };

                                            //(isSuccess, pucQuantity) = EvalUtils.GetProductUnitConversionQuantityFromPrimaryQuantity(c.NewTransferPrimaryQuantity, productUnitConversionInfo.FactorExpression, c.NewTransferProductUnitConversionQuantity, productUnitConversionInfo.DecimalPlace);

                                            (isSuccess, primaryQuantity, pucQuantity) = EvalUtils.GetProductUnitConversionQuantityFromPrimaryQuantity(calcModel);
                                        }
                                    }


                                    if (isSuccess)
                                    {
                                        c.NewTransferProductUnitConversionQuantity = pucQuantity;
                                    }
                                    else
                                    {
                                        throw new BadRequestException(ProductUnitConversionErrorCode.SecondaryUnitConversionError);
                                    }


                                    if (!(c.NewTransferProductUnitConversionQuantity > 0) && c.NewTransferPrimaryQuantity > 0)
                                    {
                                        throw new BadRequestException(ProductUnitConversionErrorCode.SecondaryUnitConversionError);
                                    }
                                }
                                else
                                {
                                    throw new Exception($"Negative TransferPrimaryQuantity from {obj.ObjectCode} to {c.ObjectTypeId} {c.ObjectId}");
                                }
                            }

                            if (c.NewTransferPrimaryQuantity.SubDecimal(c.OldTransferPrimaryQuantity) == 0)
                            {
                                c.NewTransferPrimaryQuantity = c.OldTransferPrimaryQuantity;
                            }

                            if (c.NewTransferProductUnitConversionQuantity.SubDecimal(c.OldTransferProductUnitConversionQuantity) == 0)
                            {
                                c.NewTransferProductUnitConversionQuantity = c.OldTransferProductUnitConversionQuantity;
                            }
                        }
                    }

                }


                foreach (var obj in p.AffectObjects)
                {
                    if (obj.Children != null && obj.Children.All(c => !c.IsEditable)) continue;

                    var updatedObj = updateProduct.AffectObjects.FirstOrDefault(a => a.ObjectKey == obj.ObjectKey);
                    if (updatedObj == null) continue;

                    decimal totalInPrimaryQuantity = 0;
                    decimal totalInProductUnitConversionQuantity = 0;

                    foreach (var parent in p.AffectObjects)
                    {
                        if (parent.Children != null)
                        {
                            foreach (var child in parent.Children)
                            {
                                if (child.ObjectKey == obj.ObjectKey)
                                {
                                    totalInPrimaryQuantity += child.NewTransferPrimaryQuantity;
                                    totalInProductUnitConversionQuantity += child.NewTransferProductUnitConversionQuantity;
                                }
                            }
                        }
                    }


                    obj.NewPrimaryQuantity = updatedObj.NewPrimaryQuantity;
                    obj.NewProductUnitConversionQuantity = updatedObj.NewProductUnitConversionQuantity;

                    decimal totalOutPrimaryQuantity = 0;
                    decimal totalOutProductUnitConversionQuantity = 0;

                    if (obj.Children != null && updatedObj.Children != null)
                    {

                        foreach (var child in obj.Children)
                        {
                            var updatedChild = updatedObj.Children.FirstOrDefault(c => c.ObjectKey == child.ObjectKey);
                            if (updatedChild == null) throw new Exception($"Không tìm thấy {child.ObjectKey} từ {obj.ObjectKey}");

                            child.NewTransferPrimaryQuantity = updatedChild.NewTransferPrimaryQuantity;
                            child.NewTransferProductUnitConversionQuantity = updatedChild.NewTransferProductUnitConversionQuantity;

                            totalOutPrimaryQuantity += child.NewTransferPrimaryQuantity;
                            totalOutProductUnitConversionQuantity += child.NewTransferProductUnitConversionQuantity;
                        }
                    }
                    if (totalOutPrimaryQuantity.SubDecimal(totalInPrimaryQuantity) > 0 || totalOutProductUnitConversionQuantity.SubDecimal(totalInProductUnitConversionQuantity) > 0)
                    {
                        throw new BadRequestException(InventoryErrorCode.InOuputAffectObjectsInvalid);
                    }

                    //if (totalOutPrimaryQuantity.SubDecimal(totalInPrimaryQuantity) > 0 || totalOutProductUnitConversionQuantity.SubDecimal(totalInProductUnitConversionQuantity) > 0)
                    //{
                    //    return InventoryErrorCode.InOuputAffectObjectsInvalid;
                    //}
                }
            }

        }


        private async Task<IList<InventoryDetail>> ApprovedInputDataUpdateAction_Update(ApprovedInputDataSubmitModel req, IList<CensoredInventoryInputProducts> products, IList<InventoryDetail> details)
        {
            var validateOutputDetails = new Dictionary<long, CensoredOutputInventoryDetailUpdate>();

            IList<InventoryDetail> changesDetails = new List<InventoryDetail>();


            foreach (var p in products)
            {
                var detail = details.FirstOrDefault(d => d.InventoryDetailId == p.InventoryDetailId);
                if (detail == null) continue;

                var submitDetail = req.Inventory?.InProducts?.FirstOrDefault(i => i.InventoryDetailId == p.InventoryDetailId);

                detail.PrimaryQuantity = p.NewPrimaryQuantity;
                detail.ProductUnitConversionQuantity = p.NewProductUnitConversionQuantity;

                if (submitDetail != null)
                {
                    detail.UnitPrice = submitDetail?.UnitPrice ?? 0;
                    detail.RefObjectId = submitDetail?.RefObjectId;
                    detail.RefObjectTypeId = submitDetail?.RefObjectTypeId;
                    detail.RefObjectCode = submitDetail?.RefObjectCode;

                    detail.OrderCode = submitDetail?.OrderCode;
                    detail.Pocode = submitDetail?.POCode;
                    detail.ProductionOrderCode = submitDetail?.ProductionOrderCode;
                    //detail.InventoryRequirementCode = submitDetail?.InventoryRequirementCode;
                    detail.InventoryRequirementDetailId = submitDetail?.InventoryRequirementDetailId;
                    detail.Description = submitDetail?.Description;

                    //detail.AccountancyAccountNumberDu = submitDetail?.AccountancyAccountNumberDu;
                }


                if (p.NewPrimaryQuantity == 0)
                {
                    detail.IsDeleted = true;
                }

                var firstPackage = await _stockDbContext.Package.FirstOrDefaultAsync(d => d.PackageId == detail.ToPackageId);

                if (firstPackage == null) throw new Exception("Invalid data");

                var primaryDelaRemain = p.NewPrimaryQuantity - p.OldPrimaryQuantity;
                var secondDelaRemain = p.NewProductUnitConversionQuantity - p.OldProductUnitConversionQuantity;

                firstPackage.AddRemaining(primaryDelaRemain, secondDelaRemain);

                var stockProduct = await EnsureStockProduct(req.Inventory.StockId, p.ProductId, p.ProductUnitConversionId);

                stockProduct.AddRemaining(primaryDelaRemain, secondDelaRemain);

                var updatedPackages = new List<Package>();
                var updatedInventoryDetails = new List<InventoryDetail>();
                var updatedInventories = new List<InventoryEntity>();
                var updatedPackageRefs = new List<PackageRef>();

                foreach (var obj in p.AffectObjects)
                {
                    object parent = null;
                    if (obj.ObjectId > 0)
                    {
                        switch (obj.ObjectTypeId)
                        {
                            case EnumObjectType.Package:
                                parent = await _stockDbContext.Package.FirstOrDefaultAsync(d => d.PackageId == obj.ObjectId);
                                //if (!updatedPackages.Contains((Package)parent))
                                //{
                                //    updatedPackages.Add((Package)parent);
                                //}
                                break;
                            case EnumObjectType.InventoryDetail:
                                parent = await _stockDbContext.InventoryDetail.FirstOrDefaultAsync(d => d.InventoryDetailId == obj.ObjectId);

                                //var inventory = _stockDbContext.Inventory.FirstOrDefault(iv => iv.InventoryId == ((InventoryDetail)parent).InventoryId);

                                //await ValidateInventoryConfig(inventory.Date, inventory.Date);

                                //if (!updatedInventoryDetails.Contains((InventoryDetail)parent))
                                //{
                                //    updatedInventoryDetails.Add((InventoryDetail)parent);
                                //}

                                //if (!updatedInventories.Contains(inventory))
                                //{
                                //    updatedInventories.Add(inventory);
                                //}

                                break;
                            default:
                                throw new NotSupportedException();
                        }
                    }

                    if (obj.Children != null)
                    {
                        foreach (var r in obj.Children)
                        {
                            if (r.IsEditable)
                            {
                                //substract parent
                                var deltaPrimaryQuantity = r.NewTransferPrimaryQuantity.SubDecimal(r.OldTransferPrimaryQuantity);
                                var deltaConversionQuantity = r.NewTransferProductUnitConversionQuantity.SubDecimal(r.OldTransferProductUnitConversionQuantity);

                                if (deltaPrimaryQuantity == 0 && deltaConversionQuantity == 0) continue;

                                //addition children
                                switch (r.ObjectTypeId)
                                {
                                    case EnumObjectType.Package:

                                        var refInfo = await _stockDbContext.PackageRef.FirstOrDefaultAsync(rd => rd.PackageId == r.ObjectId && rd.RefPackageId == obj.ObjectId);

                                        refInfo.AddTransfer(deltaPrimaryQuantity, deltaConversionQuantity);

                                        var childPackage = await _stockDbContext.Package.FirstOrDefaultAsync(c => c.PackageId == r.ObjectId);

                                        childPackage.AddRemaining(deltaPrimaryQuantity, deltaConversionQuantity);

                                        if (!updatedPackages.Contains(childPackage))
                                        {
                                            updatedPackages.Add(childPackage);
                                        }

                                        if (!updatedPackageRefs.Contains(refInfo))
                                        {
                                            updatedPackageRefs.Add(refInfo);
                                        }

                                        //sub parent
                                        switch (obj.ObjectTypeId)
                                        {
                                            case EnumObjectType.Package:
                                                ((Package)parent).AddRemaining(-deltaPrimaryQuantity, -deltaConversionQuantity);

                                                if (!updatedPackages.Contains((Package)parent))
                                                {
                                                    updatedPackages.Add((Package)parent);
                                                }
                                                break;

                                            case EnumObjectType.InventoryDetail:
                                                ((InventoryDetail)parent).AddQuantity(-deltaPrimaryQuantity, -deltaConversionQuantity);


                                                var parentInv = _stockDbContext.Inventory.FirstOrDefault(iv => iv.InventoryId == ((InventoryDetail)parent).InventoryId);

                                                await ValidateInventoryConfig(parentInv.Date, parentInv.Date);

                                                if (!updatedInventoryDetails.Contains((InventoryDetail)parent))
                                                {
                                                    updatedInventoryDetails.Add((InventoryDetail)parent);
                                                }

                                                if (!updatedInventories.Contains(parentInv))
                                                {
                                                    updatedInventories.Add(parentInv);
                                                }

                                                break;
                                            default:
                                                throw new NotSupportedException();
                                        }

                                        break;

                                    case EnumObjectType.InventoryDetail:

                                        var childInventoryDetail = await _stockDbContext.InventoryDetail.FirstOrDefaultAsync(c => c.InventoryDetailId == r.ObjectId);

                                        if (!updatedInventoryDetails.Contains(childInventoryDetail))
                                        {
                                            updatedInventoryDetails.Add(childInventoryDetail);
                                        }


                                        var inventory = _stockDbContext.Inventory.FirstOrDefault(iv => iv.InventoryId == childInventoryDetail.InventoryId);

                                        if (!updatedInventories.Contains(inventory))
                                        {
                                            updatedInventories.Add(inventory);
                                        }

                                        await ValidateInventoryConfig(inventory.Date, inventory.Date);

                                        //if(inventory.InventoryTypeId==(int)EnumInventoryType.Output)                                        

                                        childInventoryDetail.AddQuantity(deltaPrimaryQuantity, deltaConversionQuantity);

                                        if (childInventoryDetail.PrimaryQuantity < 0 || childInventoryDetail.ProductUnitConversionQuantity < 0)
                                        {
                                            throw new Exception("Invalid negative output inventory data!");
                                        }

                                        if (childInventoryDetail.PrimaryQuantity == 0)
                                        {
                                            childInventoryDetail.IsDeleted = true;
                                        }

                                        if (inventory.InventoryTypeId != (int)EnumInventoryType.Output) throw new Exception("Invalid inventory type!");

                                        if (inventory.IsApproved)
                                        {
                                            //sub parent
                                            switch (obj.ObjectTypeId)
                                            {
                                                case EnumObjectType.Package:

                                                    ((Package)parent).AddRemaining(-deltaPrimaryQuantity, -deltaConversionQuantity);

                                                    stockProduct.AddRemaining(-deltaPrimaryQuantity, -deltaConversionQuantity);

                                                    break;
                                                default:
                                                    throw new NotSupportedException();
                                            }


                                            var outputDetail = new CensoredOutputInventoryDetailUpdate()
                                            {
                                                InventoryId = childInventoryDetail.InventoryId,
                                                InventoryDetailId = childInventoryDetail.InventoryDetailId,
                                                Date = inventory.Date,
                                                ProductId = childInventoryDetail.ProductId,
                                                ProductUnitConversionId = childInventoryDetail.ProductUnitConversionId,
                                                OutputPrimary = childInventoryDetail.PrimaryQuantity,
                                                OutputSecondary = childInventoryDetail.ProductUnitConversionQuantity
                                            };

                                            if (!validateOutputDetails.ContainsKey(childInventoryDetail.InventoryDetailId))
                                            {
                                                validateOutputDetails.Add(childInventoryDetail.InventoryDetailId, outputDetail);
                                            }
                                            else
                                            {
                                                validateOutputDetails[childInventoryDetail.InventoryDetailId] = outputDetail;
                                            }
                                        }
                                        else
                                        {
                                            //sub parent
                                            switch (obj.ObjectTypeId)
                                            {
                                                case EnumObjectType.Package:

                                                    ((Package)parent).AddWaiting(deltaPrimaryQuantity, deltaConversionQuantity);

                                                    stockProduct.AddWaiting(deltaPrimaryQuantity, deltaConversionQuantity);

                                                    break;
                                                default:
                                                    throw new NotSupportedException();
                                            }
                                        }

                                        break;
                                    default:
                                        throw new NotSupportedException();

                                }
                            }
                        }
                    }

                }

                //Validate data before save
                foreach (var obj in p.AffectObjects)
                {
                    if (obj.NewPrimaryQuantity < 0 || obj.NewProductUnitConversionQuantity < 0)
                    {
                        throw new Exception("Invalid negative object data");
                    }

                    if (obj.Children != null)
                    {
                        foreach (var r in obj.Children)
                        {
                            if (r.NewTransferPrimaryQuantity < 0 || r.NewTransferProductUnitConversionQuantity < 0)
                            {
                                throw new Exception("Invalid negative transfer data");
                            }
                        }
                    }
                }

                foreach (var packageInfo in updatedPackages)
                {
                    if (packageInfo.PrimaryQuantityRemaining < 0 || packageInfo.ProductUnitConversionRemaining < 0)
                    {
                        throw new Exception("Invalid negative package data " + packageInfo.PackageId);
                    }

                    await _packageActivityLog.LogBuilder(() => InventoryBillInputActivityLogMessage.UpdatedCauseByRefInvInput)
                     .MessageResourceFormatDatas(packageInfo.PackageCode, req.Inventory.InventoryCode)
                     .ObjectId(packageInfo.PackageId)
                     .JsonData(req)
                     .CreateLog();
                }

                foreach (var packageRef in updatedPackageRefs)
                {
                    if (packageRef.PrimaryQuantity < 0 || packageRef.ProductUnitConversionQuantity < 0)
                    {
                        throw new Exception("Invalid negative package ref data packageId: " + packageRef.PackageId + " ref: " + packageRef.RefPackageId);
                    }
                }

                foreach (var inventoryDetail in updatedInventoryDetails)
                {
                    if (inventoryDetail.PrimaryQuantity < 0 || inventoryDetail.ProductUnitConversionQuantity < 0)
                    {
                        throw new Exception("Invalid negative inventory detail data");
                    }

                    if (!changesDetails.Contains(inventoryDetail))
                    {
                        changesDetails.Add(inventoryDetail);
                    }
                    //if (!changesInventories.Contains(inventoryDetail.InventoryId))
                    //{
                    //    changesInventories.Add(inventoryDetail.InventoryId);
                    //}
                }

                foreach (var inv in updatedInventories)
                {

                    var invDetails = updatedInventoryDetails.Where(d => d.InventoryId == inv.InventoryId).ToList();

                    ObjectActivityLogModelBuilder<string> builder;
                    if (inv.InventoryTypeId == (int)EnumInventoryType.Input)
                    {
                        builder = _invInputActivityLog.LogBuilder(() => InventoryBillInputActivityLogMessage.UpdatedCauseByRefInvInput);
                    }
                    else
                    {
                        builder = _invOutActivityLog.LogBuilder(() => InventoryBillInputActivityLogMessage.UpdatedCauseByRefInvInput);
                    }

                    await builder
                       .MessageResourceFormatDatas(inv.InventoryCode, req.Inventory.InventoryCode)
                       .ObjectId(inv.InventoryId)
                       .JsonData(req)
                       .CreateLog();
                }


                if (stockProduct.PrimaryQuantityRemaining < 0 || stockProduct.ProductUnitConversionRemaining < 0)
                {
                    throw new Exception("Invalid negative stock product data!");
                }
            }

            return changesDetails;
        }

        public class InventoryInputUpdateGetAffectedModel
        {
            public IList<CensoredInventoryInputProducts> Products { get; set; }
            public IList<InventoryDetail> DbDetails { get; set; }

            public IList<InventoryDetail> UpdateDetails { get; set; }
        }

        private class CensoredOutputInventoryDetailUpdate
        {
            public long InventoryId { get; set; }
            public long InventoryDetailId { get; set; }
            public DateTime Date { get; set; }
            public int ProductId { get; set; }
            public int ProductUnitConversionId { get; set; }
            public decimal OutputPrimary { get; set; }
            public decimal OutputSecondary { get; set; }
        }
    }

}
