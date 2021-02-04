using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public partial class InventoryService
    {
        public async Task<IList<CensoredInventoryInputProducts>> InputUpdateGetAffectedPackages(long inventoryId, long fromDate, long toDate, InventoryInModel req)
        {
            await ValidateInventoryCode(inventoryId, req.InventoryCode);

            var data = await CensoredInventoryInputUpdateGetAffected(inventoryId, fromDate, toDate, req);
            return data.Products.ToList();

        }


        public async Task<bool> ApprovedInputDataUpdate(long inventoryId, long fromDate, long toDate, ApprovedInputDataSubmitModel req)
        {
            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(req.Inventory.StockId)))
            {
                await ValidateInventoryCode(inventoryId, req.Inventory.InventoryCode);

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var data = await ApprovedInputDataUpdateAction(inventoryId, fromDate, toDate, req);

                        foreach (var changedInventoryId in data)
                        {
                            await ReCalculateRemainingAfterUpdate(changedInventoryId);
                        }

                        trans.Commit();

                        var messageLog = string.Format("Cập nhật & duyệt phiếu nhập kho đã duyệt, mã: {0}", req?.Inventory?.InventoryCode);
                        await _activityLogService.CreateLog(EnumObjectType.InventoryInput, inventoryId, messageLog, req.JsonSerialize());

                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.TryRollbackTransaction();
                        _logger.LogError(ex, "ApprovedInputDataUpdate");
                        throw;
                    }
                }
            }
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

            var deletedDetails = details.Where(d => !req.InProducts.Select(u => u.InventoryDetailId).Contains(d.InventoryDetailId));

            var updateDetail = await ValidateInventoryIn(true, req);

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

                var newDetail = updateDetail.Data.FirstOrDefault(id => id.InventoryDetailId == d.InventoryDetailId);
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

            return new InventoryInputUpdateGetAffectedModel { Products = products, DbDetails = details, UpdateDetails = updateDetail.Data };
        }

        private async Task<HashSet<long>> ApprovedInputDataUpdateAction(long inventoryId, long fromDate, long toDate, ApprovedInputDataSubmitModel req)
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

            await ApprovedInputDataUpdateAction_Normalize(req, products);

            var updateResult = await ApprovedInputDataUpdateAction_Update(req, products, dbDetails);

            var issuedDate = req.Inventory.Date.UnixToDateTime().Value;
            var billDate = req.Inventory.BillDate?.UnixToDateTime();

            await _stockDbContext.SaveChangesAsync();



            var isDelete = !(await _stockDbContext.InventoryDetail.AnyAsync(d => d.InventoryId == inventoryId && d.PrimaryQuantity > 0));

            if (!isDelete)
            {
                var newDetails = updateDetails.Where(d => d.InventoryDetailId <= 0).ToList();

                foreach (var d in newDetails)
                {
                    d.InventoryId = inventoryId;
                }

                _stockDbContext.InventoryDetail.AddRange(newDetails);
                _stockDbContext.SaveChanges();

                var r = await ProcessInventoryInputApprove(inventoryInfo.StockId, inventoryInfo.Date, newDetails);
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

                await _activityLogService.CreateLog(EnumObjectType.InventoryInput, inventoryId, $"Xóa phiếu {inventoryInfo.InventoryCode} do không tồn tại mặt hàng nào", req.JsonSerialize(), EnumActionType.Delete);
            }


            await _stockDbContext.SaveChangesAsync();

            if (!updateResult.Contains(inventoryId))
            {
                updateResult.Add(inventoryId);
            }

            return updateResult;
        }

        private async Task ApprovedInputDataUpdateAction_Normalize(ApprovedInputDataSubmitModel req, IList<CensoredInventoryInputProducts> products)
        {
            var puIds = products.Select(p => p.ProductUnitConversionId).ToList();

            var puConversions = (await _stockDbContext.ProductUnitConversion.Where(pu => puIds.Contains(pu.ProductUnitConversionId)).ToListAsync()).ToDictionary(pu => pu.ProductUnitConversionId, pu => pu);

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

                foreach (var obj in p.AffectObjects)
                {
                    if (productUnitConversionInfo.IsFreeStyle == false)
                    {

                        if (obj.NewPrimaryQuantity >= 0)
                        {

                            //var primaryQualtity = Utils.GetPrimaryQuantityFromProductUnitConversionQuantity(obj.NewProductUnitConversionQuantity, productUnitConversionInfo.FactorExpression);

                            bool isSuccess = false;
                            decimal pucQuantity = 0;

                            //if (obj.OldPrimaryQuantity != 0)
                            //{
                            //    (isSuccess, pucQuantity) = Utils.GetProductUnitConversionQuantityFromPrimaryQuantity(obj.NewPrimaryQuantity, obj.OldProductUnitConversionQuantity / obj.OldPrimaryQuantity, obj.NewProductUnitConversionQuantity);

                            //}
                            //else
                            {
                                if (obj.OldPrimaryQuantity.SubDecimal(obj.NewPrimaryQuantity) == 0)
                                {
                                    (isSuccess, pucQuantity) = (true, obj.OldProductUnitConversionQuantity);
                                }
                                else
                                {
                                    (isSuccess, pucQuantity) = Utils.GetProductUnitConversionQuantityFromPrimaryQuantity(obj.NewPrimaryQuantity, productUnitConversionInfo.FactorExpression, obj.NewProductUnitConversionQuantity);
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


                                    //if (c.OldTransferPrimaryQuantity != 0)
                                    //{
                                    //    (isSuccess, pucQuantity) = Utils.GetProductUnitConversionQuantityFromPrimaryQuantity(c.NewTransferPrimaryQuantity, c.OldTransferProductUnitConversionQuantity / c.OldTransferPrimaryQuantity, c.NewTransferProductUnitConversionQuantity);

                                    //}
                                    //else
                                    {
                                        if (c.NewTransferPrimaryQuantity.SubDecimal(c.OldTransferPrimaryQuantity) == 0)
                                        {
                                            (isSuccess, pucQuantity) = (true, c.OldTransferProductUnitConversionQuantity);
                                        }
                                        else
                                        {
                                            (isSuccess, pucQuantity) = Utils.GetProductUnitConversionQuantityFromPrimaryQuantity(c.NewTransferPrimaryQuantity, productUnitConversionInfo.FactorExpression, c.NewTransferProductUnitConversionQuantity);
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
                    if (totalOutPrimaryQuantity > totalInPrimaryQuantity || totalOutProductUnitConversionQuantity > totalInProductUnitConversionQuantity)
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


        private async Task<HashSet<long>> ApprovedInputDataUpdateAction_Update(ApprovedInputDataSubmitModel req, IList<CensoredInventoryInputProducts> products, IList<InventoryDetail> details)
        {
            var validateOutputDetails = new Dictionary<long, CensoredOutputInventoryDetailUpdate>();

            HashSet<long> changesInventories = new HashSet<long>();


            foreach (var p in products)
            {
                var detail = details.FirstOrDefault(d => d.InventoryDetailId == p.InventoryDetailId);
                if (detail == null) continue;

                var submitDetail = req.Inventory?.InProducts?.FirstOrDefault(i => i.InventoryDetailId == p.InventoryDetailId);

                detail.PrimaryQuantity = p.NewPrimaryQuantity;
                detail.ProductUnitConversionQuantity = p.NewProductUnitConversionQuantity;
                detail.UnitPrice = submitDetail?.UnitPrice ?? 0;
                detail.RefObjectId = submitDetail?.RefObjectId;
                detail.RefObjectTypeId = submitDetail?.RefObjectTypeId;
                detail.RefObjectCode = submitDetail?.RefObjectCode;

                detail.OrderCode = submitDetail?.OrderCode;
                detail.Pocode = submitDetail?.POCode;
                detail.ProductionOrderCode = submitDetail?.ProductionOrderCode;

                detail.Description = submitDetail?.Description;

                detail.AccountancyAccountNumberDu = submitDetail?.AccountancyAccountNumberDu;

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
                                if (!updatedPackages.Contains((Package)parent))
                                {
                                    updatedPackages.Add((Package)parent);
                                }
                                break;
                            case EnumObjectType.InventoryDetail:
                                parent = await _stockDbContext.InventoryDetail.FirstOrDefaultAsync(d => d.InventoryDetailId == obj.ObjectId);

                                var inventory = _stockDbContext.Inventory.FirstOrDefault(iv => iv.InventoryId == ((InventoryDetail)parent).InventoryId);

                                await ValidateInventoryConfig(inventory.Date, inventory.Date);

                                if (!updatedInventoryDetails.Contains((InventoryDetail)parent))
                                {
                                    updatedInventoryDetails.Add((InventoryDetail)parent);
                                }
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
                                                break;
                                            case EnumObjectType.InventoryDetail:
                                                ((InventoryDetail)parent).AddQuantity(-deltaPrimaryQuantity, -deltaConversionQuantity);
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

                    if (!changesInventories.Contains(inventoryDetail.InventoryId))
                    {
                        changesInventories.Add(inventoryDetail.InventoryId);
                    }
                }

                if (stockProduct.PrimaryQuantityRemaining < 0 || stockProduct.ProductUnitConversionRemaining < 0)
                {
                    throw new Exception("Invalid negative stock product data!");
                }
            }


            foreach (var output in validateOutputDetails)
            {
                var validate = await ValidateBalanceForOutput(req.Inventory.StockId, output.Value.ProductId, output.Value.InventoryId, output.Value.ProductUnitConversionId, output.Value.Date, output.Value.OutputPrimary, output.Value.OutputSecondary);

                if (!validate.IsSuccessCode())
                {
                    throw new BadRequestException(validate.Code);
                }
            }

            return changesInventories;
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
