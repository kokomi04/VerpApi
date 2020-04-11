﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public partial class InventoryService
    {
        public async Task<ServiceResult<IList<CensoredInventoryInputProducts>>> InputUpdateGetAffectedPackages(long inventoryId, long fromDate, long toDate, InventoryInModel req)
        {
            var data = await CensoredInventoryInputUpdateGetAffected(inventoryId, fromDate, toDate, req);
            if (!data.Code.IsSuccess())
            {
                return data.Code;
            }

            return data.Data.Products.ToList();

        }

        private async Task<ServiceResult<InventoryInputUpdateGetAffectedModel>> CensoredInventoryInputUpdateGetAffected(long inventoryId, long fromDate, long toDate, InventoryInModel req)
        {
            var inventoryInfo = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);
            if (inventoryInfo == null)
            {
                return InventoryErrorCode.InventoryNotFound;
            }
            if (inventoryInfo.StockId != req.StockId)
            {
                return InventoryErrorCode.CanNotChangeStock;
            }

            if (!inventoryInfo.IsApproved)
            {
                return GeneralCode.InvalidParams;
            }

            var details = await _stockDbContext.InventoryDetail.Where(iv => iv.InventoryId == inventoryId).ToListAsync();

            var deletedDetails = details.Where(d => !req.InProducts.Select(u => u.InventoryDetailId).Contains(d.InventoryDetailId));

            var updateDetail = await ValidateInventoryIn(true, req);

            if (!updateDetail.Code.IsSuccess())
            {
                return updateDetail.Code;
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

            var productUnitConversions = _stockDbContext.ProductUnitConversion.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToList();

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


                var conversionInfo = productUnitConversions.FirstOrDefault(c => c.ProductUnitConversionId == d.ProductUnitConversionId.Value);

                if (!productInfos.TryGetValue(d.ProductId, out var productInfo))
                {
                    throw new KeyNotFoundException("Product not found " + d.ProductId);
                }

                var product = new CensoredInventoryInputProducts()
                {
                    InventoryDetailId = d.InventoryDetailId,
                    ProductId = d.ProductId,
                    ProductCode = productInfo.ProductCode,
                    PrimaryUnitId = productInfo.UnitId,


                    OldPrimaryQuantity = d.PrimaryQuantity,
                    NewPrimaryQuantity = newPrimaryQuantity,

                    ProductUnitConversionId = d.ProductUnitConversionId.Value,
                    ProductUnitConversionName = conversionInfo?.ProductUnitConversionName,
                    FactorExpression = conversionInfo?.FactorExpression,

                    OldProductUnitConversionQuantity = d.ProductUnitConversionQuantity,
                    NewProductUnitConversionQuantity = newProductUnitConversionQuantity,
                    ToPackageId = d.ToPackageId.Value,
                    AffectObjects = new CensoredInventoryInputUpdateContext(_stockDbContext, inventoryInfo, d, fromDate, toDate)
                    .GetAffectObjects(newPrimaryQuantity, newProductUnitConversionQuantity)
                };

                products.Add(product);

            }

            return new InventoryInputUpdateGetAffectedModel { Products = products, DbDetails = details, UpdateDetails = updateDetail.Data };
        }

        public async Task<ServiceResult> ApprovedInputDataUpdate(int currentUserId, long inventoryId, long fromDate, long toDate, ApprovedInputDataSubmitModel req)
        {
            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(req.Inventory.StockId)))
            {
                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var r = await ApprovedInputDataUpdateAction(currentUserId, inventoryId, fromDate, toDate, req);
                        if (!r.Code.IsSuccess())
                        {
                            trans.Rollback();
                            return r;
                        }


                        trans.Commit();

                        var messageLog = string.Format("Cập nhật & duyệt phiếu nhập kho đã duyệt, mã: {0}", req?.Inventory?.InventoryCode);
                        await _activityLogService.CreateLog(EnumObjectType.Inventory, inventoryId, messageLog, req.JsonSerialize());

                        return r;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        _logger.LogError(ex, "ApprovedInputDataUpdate");
                        return GeneralCode.InternalError;
                    }
                }
            }
        }

        private async Task<ServiceResult> ApprovedInputDataUpdateAction(int currentUserId, long inventoryId, long fromDate, long toDate, ApprovedInputDataSubmitModel req)
        {
            var inventoryInfo = await _stockDbContext.Inventory.FirstOrDefaultAsync(iv => iv.InventoryId == inventoryId);

            if (!inventoryInfo.IsApproved)
            {
                return InventoryErrorCode.InventoryNotApprovedYet;
            }


            var data = await CensoredInventoryInputUpdateGetAffected(inventoryId, fromDate, toDate, req.Inventory);

            if (!data.Code.IsSuccess())
            {
                return data.Code;
            }

            var products = data.Data.Products;
            var dbDetails = data.Data.DbDetails;
            var updateDetails = data.Data.UpdateDetails;

            var normalizeStatus = await ApprovedInputDataUpdateAction_Normalize(req, products);
            if (!normalizeStatus.IsSuccess()) return normalizeStatus;

            var updateStatus = await ApprovedInputDataUpdateAction_Update(req, products, dbDetails);
            if (!updateStatus.IsSuccessCode()) return updateStatus;

            var issuedDate = req.Inventory.DateUtc.UnixToDateTime();
            var billDate = req.Inventory.BillDate.UnixToDateTime();

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
                    return r;
                }

                var totalMoney = InputCalTotalMoney(updateDetails);

                inventoryInfo.TotalMoney = totalMoney;
                inventoryInfo.InventoryCode = req.Inventory.InventoryCode;
                inventoryInfo.Shipper = req.Inventory.Shipper;
                inventoryInfo.Content = req.Inventory.Content;
                inventoryInfo.Date = issuedDate;
                inventoryInfo.CustomerId = req.Inventory.CustomerId;
                inventoryInfo.Department = req.Inventory.Department;
                inventoryInfo.StockKeeperUserId = req.Inventory.StockKeeperUserId;
                inventoryInfo.BillCode = req.Inventory.BillCode;
                inventoryInfo.BillSerial = req.Inventory.BillSerial;
                inventoryInfo.BillDate = billDate == DateTime.MinValue ? null : (DateTime?)billDate;
                inventoryInfo.TotalMoney = totalMoney;
                inventoryInfo.UpdatedByUserId = currentUserId;
                inventoryInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
            }
            else
            {
                inventoryInfo.IsDeleted = true;
                inventoryInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
                inventoryInfo.UpdatedByUserId = currentUserId;
            }


            await _stockDbContext.SaveChangesAsync();

            return GeneralCode.Success;
        }

        private async Task<Enum> ApprovedInputDataUpdateAction_Normalize(ApprovedInputDataSubmitModel req, IList<CensoredInventoryInputProducts> products)
        {
            foreach (var p in products)
            {
                var updateProduct = req.AffectDetails.FirstOrDefault(d => d.InventoryDetailId == p.InventoryDetailId);
                if (updateProduct == null) continue;

                var productUnitConversionInfo = await _stockDbContext.ProductUnitConversion.FirstOrDefaultAsync(c => c.ProductUnitConversionId == p.ProductUnitConversionId);
                if (productUnitConversionInfo == null)
                {
                    return ProductUnitConversionErrorCode.ProductUnitConversionNotFound;
                }

                if (p.NewPrimaryQuantity.SubDecimal(p.OldPrimaryQuantity) == 0)
                {
                    p.NewPrimaryQuantity = p.OldPrimaryQuantity;
                }

                if (p.NewProductUnitConversionQuantity.SubDecimal(p.OldProductUnitConversionQuantity) == 0)
                {
                    p.NewProductUnitConversionQuantity = p.OldProductUnitConversionQuantity;
                }

                foreach (var obj in p.AffectObjects)
                {
                    if (productUnitConversionInfo.IsFreeStyle == false)
                    {

                        if (obj.NewPrimaryQuantity >= 0)
                        {
                            //var primaryQualtity = Utils.GetPrimaryQuantityFromProductUnitConversionQuantity(obj.NewProductUnitConversionQuantity, productUnitConversionInfo.FactorExpression);

                            if (obj.OldPrimaryQuantity != 0)
                            {
                                obj.NewProductUnitConversionQuantity = obj.NewPrimaryQuantity * obj.OldProductUnitConversionQuantity / obj.OldPrimaryQuantity;
                            }
                            else
                            {
                                obj.NewProductUnitConversionQuantity = Utils.GetProductUnitConversionQuantityFromPrimaryQuantity(obj.NewPrimaryQuantity, productUnitConversionInfo.FactorExpression);
                            }

                            if (!(obj.NewProductUnitConversionQuantity > 0) && obj.NewPrimaryQuantity > 0)
                            {
                                return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
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

                    if (obj.NewProductUnitConversionQuantity.SubDecimal(obj.OldProductUnitConversionQuantity) == 0)
                    {
                        obj.NewProductUnitConversionQuantity = obj.OldProductUnitConversionQuantity;
                    }



                    if ((obj.NewProductUnitConversionQuantity != 0 || obj.NewPrimaryQuantity != 0)
                        && (obj.NewProductUnitConversionQuantity <= 0 || obj.NewPrimaryQuantity <= 0)
                        )
                    {
                        return GeneralCode.InvalidParams;
                    }

                    if (obj.Children != null)
                    {
                        foreach (var c in obj.Children)
                        {
                            if (productUnitConversionInfo.IsFreeStyle ?? false == false)
                            {
                                if (c.NewTransferPrimaryQuantity >= 0)
                                {
                                    //var primaryQualtity = Utils.GetPrimaryQuantityFromProductUnitConversionQuantity(c.NewTransferProductUnitConversionQuantity, productUnitConversionInfo.FactorExpression);

                                    c.NewTransferProductUnitConversionQuantity = c.NewTransferPrimaryQuantity * c.OldTransferProductUnitConversionQuantity / c.OldTransferPrimaryQuantity;

                                    if (!(c.NewTransferProductUnitConversionQuantity > 0) && c.NewTransferPrimaryQuantity > 0)
                                    {
                                        return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
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
                        return InventoryErrorCode.InOuputAffectObjectsInvalid;
                    }

                    //if (totalOutPrimaryQuantity.SubDecimal(totalInPrimaryQuantity) > 0 || totalOutProductUnitConversionQuantity.SubDecimal(totalInProductUnitConversionQuantity) > 0)
                    //{
                    //    return InventoryErrorCode.InOuputAffectObjectsInvalid;
                    //}
                }
            }
            return GeneralCode.Success;

        }

        private async Task<ServiceResult> ApprovedInputDataUpdateAction_Update(ApprovedInputDataSubmitModel req, IList<CensoredInventoryInputProducts> products, IList<InventoryDetail> details)
        {

            var validateOutputDetails = new Dictionary<long, CensoredOutputInventoryDetailUpdate>();

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
                                                ProductUnitConversionId = childInventoryDetail.ProductUnitConversionId.Value,
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
                        throw new Exception("Invalid negative package data");
                    }
                }

                foreach (var packageRef in updatedPackageRefs)
                {
                    if (packageRef.PrimaryQuantity < 0 || packageRef.ProductUnitConversionQuantity < 0)
                    {
                        throw new Exception("Invalid negative package ref data");
                    }
                }

                foreach (var inventoryDetail in updatedInventoryDetails)
                {
                    if (inventoryDetail.PrimaryQuantity < 0 || inventoryDetail.ProductUnitConversionQuantity < 0)
                    {
                        throw new Exception("Invalid negative inventory detail data");
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
                    return validate;
                }
            }
            return GeneralCode.Success;
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
