using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Master.Service.Config;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.FileResources;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Package;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.FileResources;
using InventoryEntity = VErp.Infrastructure.EF.StockDB.Inventory;
using PackageEntity = VErp.Infrastructure.EF.StockDB.Package;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public partial class InventoryService
    {

        public async Task<ServiceResult<IList<CensoredInventoryInputProducts>>> InputUpdateGetAffectedPackages(long inventoryId, InventoryInModel req)
        {
            var data = await CensoredInventoryInputUpdateGetAffected(inventoryId, req);
            if (!data.Code.IsSuccess())
            {
                return data.Code;
            }

            return data.Data.Products.ToList();

        }
        public async Task<ServiceResult<InventoryInputUpdateGetAffectedModel>> CensoredInventoryInputUpdateGetAffected(long inventoryId, InventoryInModel req)
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
            var productInfos = _stockDbContext.Product.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToList();

            //var productUnitConversions = _stockDbContext.ProductUnitConversion.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToList();

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


                //var conversionInfo = productUnitConversions.FirstOrDefault(c => c.ProductUnitConversionId == d.ProductUnitConversionId.Value);

                var product = new CensoredInventoryInputProducts()
                {
                    InventoryDetailId = d.InventoryDetailId,
                    ProductId = d.ProductId,
                    ProductCode = productInfos.FirstOrDefault(p => p.ProductId == d.ProductId)?.ProductCode,
                    PrimaryUnitId = d.PrimaryUnitId,


                    OldPrimaryQuantity = d.PrimaryQuantity,
                    NewPrimaryQuantity = newPrimaryQuantity,

                    ProductUnitConversionId = d.ProductUnitConversionId.Value,
                    //ProductUnitConversionName = conversionInfo?.ProductUnitConversionName,
                    //FactorExpression = conversionInfo?.FactorExpression,

                    OldProductUnitConversionQuantity = d.ProductUnitConversionQuantity,
                    NewProductUnitConversionQuantity = newProductUnitConversionQuantity,
                    ToPackageId = d.ToPackageId.Value
                };


                var topPackage = await _stockDbContext.Package.FirstOrDefaultAsync(p => p.PackageId == product.ToPackageId);

                var affectObjects = new List<CensoredInventoryInputObject>();

                //Lấy thông tin về phiếu nhập -> Kiện
                _affectInventoryInput(affectObjects, d.InventoryDetailId, req.InventoryCode, topPackage.PackageId,
                    d.PrimaryQuantity, newPrimaryQuantity, d.ProductUnitConversionQuantity, newProductUnitConversionQuantity
                    );

                var queue = new Queue<long>();

                //Duyệt đồng cấp (nút cha duyệt trước)
                queue.Enqueue(topPackage.PackageId);
                while (queue.Count > 0)
                {
                    var packageId = queue.Dequeue();

                    //Lấy các kiện cha ảnh hưởng tới kiện hiện tại
                    await _affectAddParentPackages(affectObjects, packageId);

                    //Lấy các phiếu nhập ảnh hưởng đến kiện hiện tại
                    await _affectAddParentInputs(affectObjects, packageId);


                    //Lấy các kiện con và phiếu xuất mà kiện hiện tại ảnh hưởng đến
                    var childPackgeIds = await _affectAddChildren(affectObjects, packageId);

                    foreach (var id in childPackgeIds)
                    {
                        queue.Enqueue(id);
                    }

                }
                product.AffectObjects = affectObjects;

                products.Add(product);

            }

            return new InventoryInputUpdateGetAffectedModel { Products = products, DbDetails = details, UpdateDetails = updateDetail.Data };
        }


        private void _affectInventoryInput(
            IList<CensoredInventoryInputObject> affectObjects, long inInventoryDetailId, string inInventoryCode
            , long toPackageId
            , decimal oldPrimaryQuantity, decimal newPrimaryQuantity
            , decimal oldProductUnitConversionQuantity, decimal newProductUnitConversionQuantity
            )
        {
            affectObjects.Add(new CensoredInventoryInputObject()
            {
                ObjectId = inInventoryDetailId,
                ObjectCode = inInventoryCode,
                ObjectTypeId = EnumObjectType.InventoryDetail,
                IsRoot = true,
                IsCurrentFlow = true,

                OldPrimaryQuantity = oldPrimaryQuantity,
                NewPrimaryQuantity = newPrimaryQuantity,

                OldProductUnitConversionQuantity = oldProductUnitConversionQuantity,
                NewProductUnitConversionQuantity = newProductUnitConversionQuantity,

                Children = new List<TransferToObject>()
                    {
                        new TransferToObject {
                            IsEditable = false,
                            ObjectId = toPackageId,
                            ObjectTypeId = EnumObjectType.Package,
                            PackageOperationTypeId = EnumPackageOperationType.Join,

                            OldTransferPrimaryQuantity = oldPrimaryQuantity,
                            NewTransferPrimaryQuantity = newPrimaryQuantity,

                            OldTransferProductUnitConversionQuantity = oldProductUnitConversionQuantity,
                            NewTransferProductUnitConversionQuantity = newProductUnitConversionQuantity,
                        }
                    }
            });
        }

        private async Task _affectAddParentPackages(IList<CensoredInventoryInputObject> affectObjects, long packageId)
        {
            var refParentPackages = await _stockDbContext.PackageRef.Where(r => r.PackageId == packageId).ToListAsync();

            var refParentPackageIds = refParentPackages.Select(r => r.RefPackageId);

            var refParentPackageInfos = await _stockDbContext.Package.Where(p => refParentPackageIds.Contains(p.PackageId)).ToListAsync();

            foreach (var r in refParentPackageInfos)
            {
                var refQuantity = refParentPackages.FirstOrDefault(q => q.RefPackageId == r.PackageId);

                var newObject = new CensoredInventoryInputObject()
                {
                    ObjectId = r.PackageId,
                    ObjectCode = r.PackageCode,
                    ObjectTypeId = EnumObjectType.Package,
                    IsRoot = false,
                    IsCurrentFlow = false,

                    OldPrimaryQuantity = r.PrimaryQuantityRemaining,
                    NewPrimaryQuantity = r.PrimaryQuantityRemaining,

                    OldProductUnitConversionQuantity = r.ProductUnitConversionRemaining,
                    NewProductUnitConversionQuantity = r.ProductUnitConversionRemaining,

                    Children = new List<TransferToObject>()
                            {
                                new TransferToObject{
                                    IsEditable = false,
                                    ObjectId = packageId,
                                    ObjectTypeId = EnumObjectType.Package,
                                    PackageOperationTypeId = (EnumPackageOperationType)refQuantity.PackageOperationTypeId,

                                    OldTransferPrimaryQuantity = refQuantity.PrimaryQuantity.Value,
                                    NewTransferPrimaryQuantity = refQuantity.PrimaryQuantity.Value,

                                    OldTransferProductUnitConversionQuantity = refQuantity.ProductUnitConversionQuantity.Value,
                                    NewTransferProductUnitConversionQuantity = refQuantity.ProductUnitConversionQuantity.Value
                                }
                            }
                };

                if (!affectObjects.Any(a => a.ObjectKey == newObject.ObjectKey))
                {
                    affectObjects.Add(newObject);
                }
            }

        }

        private async Task _affectAddParentInputs(IList<CensoredInventoryInputObject> affectObjects, long packageId)
        {
            var refInventoryIns = await (
                       from id in _stockDbContext.InventoryDetail
                       join iv in _stockDbContext.Inventory on id.InventoryId equals iv.InventoryId
                       where id.ToPackageId == packageId
                       select new
                       {
                           iv.InventoryId,
                           iv.InventoryCode,
                           id.InventoryDetailId,
                           id.PrimaryQuantity,
                           id.ProductUnitConversionQuantity
                       }
                       ).ToListAsync();

            foreach (var r in refInventoryIns)
            {
                var newObject = new CensoredInventoryInputObject()
                {
                    ObjectId = r.InventoryDetailId,
                    ObjectCode = r.InventoryCode,
                    ObjectTypeId = EnumObjectType.InventoryDetail,
                    IsRoot = false,

                    OldPrimaryQuantity = r.PrimaryQuantity,
                    NewPrimaryQuantity = r.PrimaryQuantity,

                    OldProductUnitConversionQuantity = r.ProductUnitConversionQuantity,
                    NewProductUnitConversionQuantity = r.ProductUnitConversionQuantity,

                    Children = new List<TransferToObject>()
                            {
                                new TransferToObject{
                                    IsEditable = false,
                                    ObjectId = packageId,
                                    ObjectTypeId =EnumObjectType.Package,
                                    PackageOperationTypeId = EnumPackageOperationType.Join,

                                    OldTransferPrimaryQuantity = r.PrimaryQuantity,
                                    NewTransferPrimaryQuantity = r.PrimaryQuantity,

                                    OldTransferProductUnitConversionQuantity = r.ProductUnitConversionQuantity,
                                    NewTransferProductUnitConversionQuantity = r.ProductUnitConversionQuantity
                                }
                            }
                };

                if (!affectObjects.Any(a => a.ObjectKey == newObject.ObjectKey))
                {
                    affectObjects.Add(newObject);
                }
            }

        }
        private async Task<IList<long>> _affectAddChildren(IList<CensoredInventoryInputObject> affectObjects, long packageId)
        {
            var packageInfo = await _stockDbContext.Package.FirstOrDefaultAsync(p => p.PackageId == packageId);

            var childrenPackages = await _stockDbContext.PackageRef.Where(p => p.RefPackageId == packageId).ToListAsync();

            var currentPackageNode = new CensoredInventoryInputObject()
            {
                ObjectId = packageInfo.PackageId,
                ObjectCode = packageInfo.PackageCode,
                ObjectTypeId = EnumObjectType.Package,
                IsRoot = false,

                OldPrimaryQuantity = packageInfo.PrimaryQuantityRemaining,
                NewPrimaryQuantity = packageInfo.PrimaryQuantityRemaining,

                OldProductUnitConversionQuantity = packageInfo.ProductUnitConversionRemaining,
                NewProductUnitConversionQuantity = packageInfo.ProductUnitConversionRemaining,

                Children = childrenPackages.Select(r => new TransferToObject()
                {
                    IsEditable = true,
                    ObjectId = r.PackageId,
                    ObjectTypeId = EnumObjectType.Package,
                    PackageOperationTypeId = (EnumPackageOperationType)r.PackageOperationTypeId,

                    OldTransferPrimaryQuantity = r.PrimaryQuantity.Value,
                    NewTransferPrimaryQuantity = r.PrimaryQuantity.Value,

                    OldTransferProductUnitConversionQuantity = r.ProductUnitConversionQuantity.Value,
                    NewTransferProductUnitConversionQuantity = r.ProductUnitConversionQuantity.Value
                }).ToList()
            };

            await _affectAddChildrenOut(affectObjects, currentPackageNode, packageId);

            if (!affectObjects.Any(o => o.ObjectKey == currentPackageNode.ObjectKey))
                affectObjects.Add(currentPackageNode);

            return childrenPackages.Select(c => c.PackageId).ToList();
        }

        private async Task _affectAddChildrenOut(IList<CensoredInventoryInputObject> affectObjects, CensoredInventoryInputObject currentPackageNode, long packageId)
        {

            var childrenInventoryOuts = await (
                        from id in _stockDbContext.InventoryDetail
                        join iv in _stockDbContext.Inventory on id.InventoryId equals iv.InventoryId
                        where id.FromPackageId == packageId
                        select new
                        {
                            iv.InventoryId,
                            iv.InventoryCode,
                            id.InventoryDetailId,
                            id.PrimaryQuantity,
                            id.ProductUnitConversionQuantity
                        }
                        ).ToListAsync();

            foreach (var iv in childrenInventoryOuts)
            {
                currentPackageNode.Children.Add(new TransferToObject()
                {
                    IsEditable = true,
                    ObjectId = iv.InventoryDetailId,
                    ObjectTypeId = EnumObjectType.InventoryDetail,
                    PackageOperationTypeId = EnumPackageOperationType.Split,

                    OldTransferPrimaryQuantity = iv.PrimaryQuantity,
                    NewTransferPrimaryQuantity = iv.PrimaryQuantity,

                    OldTransferProductUnitConversionQuantity = iv.ProductUnitConversionQuantity,
                    NewTransferProductUnitConversionQuantity = iv.ProductUnitConversionQuantity
                });

                var outObject = new CensoredInventoryInputObject()
                {
                    IsRoot = false,
                    IsCurrentFlow = true,
                    ObjectId = iv.InventoryDetailId,
                    ObjectCode = iv.InventoryCode,
                    ObjectTypeId = EnumObjectType.InventoryDetail,

                    OldPrimaryQuantity = iv.PrimaryQuantity,
                    NewPrimaryQuantity = iv.PrimaryQuantity,

                    OldProductUnitConversionQuantity = iv.ProductUnitConversionQuantity,
                    NewProductUnitConversionQuantity = iv.ProductUnitConversionQuantity,

                    Children = null
                };

                if (!affectObjects.Any(o => o.ObjectKey == outObject.ObjectKey))
                    affectObjects.Add(outObject);
            }


        }


        public async Task<ServiceResult> ApprovedInputDataUpdate(int currentUserId, long inventoryId, ApprovedInputDataSubmitModel req)
        {
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var r = await ApprovedInputDataUpdateAction(currentUserId, inventoryId, req);
                    if (!r.Code.IsSuccess())
                    {
                        trans.Rollback();
                        return r;
                    }


                    trans.Commit();

                    var messageLog = string.Format("Cập nhật & duyệt phiếu nhập kho đã duyệt, mã: {0}", req?.Inventory?.InventoryCode);
                    _activityService.CreateActivityAsync(EnumObjectType.Inventory, inventoryId, messageLog, "", req);

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
        private async Task<ServiceResult> ApprovedInputDataUpdateAction(int currentUserId, long inventoryId, ApprovedInputDataSubmitModel req)
        {
            var data = await CensoredInventoryInputUpdateGetAffected(inventoryId, req.Inventory);

            if (!data.Code.IsSuccess())
            {
                return data.Code;
            }

            var products = data.Data.Products;
            var dbDetails = data.Data.DbDetails;
            var updateDetails = data.Data.UpdateDetails;

            var normalizeStatus = await ApprovedInputDataUpdateAction_Normalize(req, products, dbDetails);
            if (!normalizeStatus.IsSuccess()) return normalizeStatus;

            var updateStatus = await ApprovedInputDataUpdateAction_Update(req, products, dbDetails);
            if (!updateStatus.IsSuccess()) return updateStatus;


            if (!DateTime.TryParseExact(req.Inventory.DateUtc, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var issuedDate))
            {
                return GeneralCode.InvalidParams;
            }

            var billDate = DateTime.MinValue;
            if (!string.IsNullOrEmpty(req.Inventory.BillDate))
            {
                DateTime.TryParseExact(req.Inventory.DateUtc, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out billDate);
            }


            await _stockDbContext.SaveChangesAsync();



            var inventoryInfo = await _stockDbContext.Inventory.FirstOrDefaultAsync(iv => iv.InventoryId == inventoryId);

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

                var r = await ProcessInventoryInputApprove(inventoryInfo.StockId, inventoryInfo.DateUtc, newDetails);
                if (!r.IsSuccess())
                {
                    return r;
                }

                var totalMoney = InputCalTotalMoney(updateDetails);

                inventoryInfo.TotalMoney = totalMoney;
                inventoryInfo.InventoryCode = req.Inventory.InventoryCode;
                inventoryInfo.Shipper = req.Inventory.Shipper;
                inventoryInfo.Content = req.Inventory.Content;
                inventoryInfo.DateUtc = issuedDate;
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

        private async Task<Enum> ApprovedInputDataUpdateAction_Normalize(ApprovedInputDataSubmitModel req, IList<CensoredInventoryInputProducts> products, IList<InventoryDetail> details)
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


                foreach (var obj in p.AffectObjects)
                {
                    if (productUnitConversionInfo.IsFreeStyle == false)
                    {

                        if (obj.NewProductUnitConversionQuantity > 0)
                        {
                            var primaryQualtity = Utils.GetPrimaryQuantityFromProductUnitConversionQuantity(obj.NewProductUnitConversionQuantity, productUnitConversionInfo.FactorExpression);
                            if (!(primaryQualtity > 0))
                            {
                                return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                            }

                            obj.NewPrimaryQuantity = primaryQualtity;
                        }
                        else
                        {
                            obj.NewPrimaryQuantity = 0;
                        }
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
                            if (productUnitConversionInfo.IsFreeStyle == false)
                            {
                                if (obj.NewProductUnitConversionQuantity > 0)
                                {
                                    var primaryQualtity = Utils.GetPrimaryQuantityFromProductUnitConversionQuantity(c.NewTransferProductUnitConversionQuantity, productUnitConversionInfo.FactorExpression);
                                    if (!(primaryQualtity > 0))
                                    {
                                        return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                                    }

                                    c.NewTransferPrimaryQuantity = primaryQualtity;
                                }
                                else
                                {
                                    c.NewTransferPrimaryQuantity = 0;
                                }
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
                }
            }
            return GeneralCode.Success;

        }

        private async Task<Enum> ApprovedInputDataUpdateAction_Update(ApprovedInputDataSubmitModel req, IList<CensoredInventoryInputProducts> products, IList<InventoryDetail> details)
        {
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


                var firstPackage = await _stockDbContext.Package.FirstOrDefaultAsync(d => d.PackageId == detail.ToPackageId);

                if (firstPackage == null) throw new Exception("Invalid data");

                firstPackage.PrimaryQuantityRemaining += p.NewPrimaryQuantity - p.OldPrimaryQuantity;
                firstPackage.ProductUnitConversionRemaining += p.NewProductUnitConversionQuantity - p.OldProductUnitConversionQuantity;

                if (p.NewPrimaryQuantity == 0)
                {
                    detail.IsDeleted = true;
                }
                var stockProduct = await EnsureStockProduct(req.Inventory.StockId, p.ProductId, p.PrimaryUnitId, p.ProductUnitConversionId);

                stockProduct.PrimaryQuantityRemaining += p.NewPrimaryQuantity - p.OldPrimaryQuantity;
                stockProduct.ProductUnitConversionRemaining += p.NewProductUnitConversionQuantity - p.OldProductUnitConversionQuantity;

                foreach (var obj in p.AffectObjects)
                {
                    object parent = null;
                    switch (obj.ObjectTypeId)
                    {
                        case EnumObjectType.Package:
                            parent = await _stockDbContext.Package.FirstOrDefaultAsync(d => d.PackageId == obj.ObjectId);
                            break;
                        case EnumObjectType.InventoryDetail:
                            parent = await _stockDbContext.InventoryDetail.FirstOrDefaultAsync(d => d.InventoryDetailId == obj.ObjectId);
                            break;
                        default:
                            throw new NotSupportedException();
                    }

                    if (obj.Children != null)
                    {
                        foreach (var r in obj.Children)
                        {
                            if (r.IsEditable)
                            {
                                //substract parent
                                var deltaPrimaryQuantity = r.NewTransferPrimaryQuantity - r.OldTransferPrimaryQuantity;
                                var deltaConversionQuantity = r.NewTransferProductUnitConversionQuantity - r.OldTransferProductUnitConversionQuantity;

                                //addition children
                                switch (r.ObjectTypeId)
                                {
                                    case EnumObjectType.Package:

                                        var refInfo = await _stockDbContext.PackageRef.FirstOrDefaultAsync(rd => rd.PackageId == r.ObjectId && rd.RefPackageId == obj.ObjectId);
                                        refInfo.PrimaryQuantity += deltaPrimaryQuantity;
                                        refInfo.ProductUnitConversionQuantity += deltaConversionQuantity;

                                        var childPackage = await _stockDbContext.Package.FirstOrDefaultAsync(c => c.PackageId == r.ObjectId);

                                        childPackage.PrimaryQuantityRemaining += deltaPrimaryQuantity;
                                        childPackage.ProductUnitConversionRemaining += deltaConversionQuantity;


                                        //sub parent
                                        switch (obj.ObjectTypeId)
                                        {
                                            case EnumObjectType.Package:
                                                ((PackageEntity)parent).PrimaryQuantityRemaining -= deltaPrimaryQuantity;
                                                ((PackageEntity)parent).ProductUnitConversionRemaining -= deltaConversionQuantity;
                                                break;
                                            case EnumObjectType.InventoryDetail:
                                                ((InventoryDetail)parent).PrimaryQuantity -= deltaPrimaryQuantity;
                                                ((InventoryDetail)parent).ProductUnitConversionQuantity -= deltaConversionQuantity;
                                                break;
                                            default:
                                                throw new NotSupportedException();
                                        }

                                        break;

                                    case EnumObjectType.InventoryDetail:

                                        var childInventoryDetail = await _stockDbContext.InventoryDetail.FirstOrDefaultAsync(c => c.InventoryDetailId == r.ObjectId);

                                        var inventory = _stockDbContext.Inventory.FirstOrDefault(iv => iv.InventoryId == childInventoryDetail.InventoryId);

                                        //if(inventory.InventoryTypeId==(int)EnumInventoryType.Output)                                        
                                        childInventoryDetail.PrimaryQuantity += deltaPrimaryQuantity;
                                        childInventoryDetail.ProductUnitConversionQuantity += deltaConversionQuantity;

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
                                                    ((PackageEntity)parent).PrimaryQuantityRemaining -= deltaPrimaryQuantity;
                                                    ((PackageEntity)parent).ProductUnitConversionRemaining -= deltaConversionQuantity;
                                                    break;
                                                default:
                                                    throw new NotSupportedException();
                                            }
                                        }
                                        else
                                        {
                                            //sub parent
                                            switch (obj.ObjectTypeId)
                                            {
                                                case EnumObjectType.Package:
                                                    ((PackageEntity)parent).PrimaryQuantityWaiting += deltaPrimaryQuantity;
                                                    ((PackageEntity)parent).ProductUnitConversionWaitting += deltaConversionQuantity;

                                                    stockProduct.PrimaryQuantityWaiting += deltaPrimaryQuantity;
                                                    stockProduct.ProductUnitConversionWaitting += deltaConversionQuantity;
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

            }

            return GeneralCode.Success;
        }

        public class InventoryInputUpdateGetAffectedModel
        {
            public IList<CensoredInventoryInputProducts> Products { get; set; }
            public IList<InventoryDetail> DbDetails { get; set; }

            public IList<InventoryDetail> UpdateDetails { get; set; }
        }
    }

}
