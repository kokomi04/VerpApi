using System;
using System.Collections.Generic;
using System.Linq;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.StockDB;
using VErp.Services.Stock.Model.Inventory;
using static VErp.Services.Stock.Service.Resources.InventoryProcess.CensoredInventoryInputUpdateContextTitle;
using InventoryEntity = VErp.Infrastructure.EF.StockDB.Inventory;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public class CensoredInventoryInputUpdateContext
    {
        //private StockDBContext stockDbContext;
        private readonly IQueryable<Package> _packages;
        private readonly IQueryable<PackageRef> _packageRefs;
        private readonly IQueryable<InventoryDetailAffectModel> _inventoryAffectDetails;
        private readonly IList<CensoredInventoryInputObject> _affectObjects;
        private readonly InventoryDetail _inventoryDetail;
        private readonly InventoryEntity _inventory;
        private readonly DateTime _fromDate;
        private readonly DateTime _toDate;
        private readonly int OTHER_INVENTORY_INPUT_ID = -1;
        private readonly int OTHER_INVENTORY_OUTPUT_ID = -2;

        public CensoredInventoryInputUpdateContext(StockDBContext stockDbContext, InventoryEntity inventory, InventoryDetail inventoryDetail, long fromDate, long toDate)
        {
            this._fromDate = fromDate.UnixToDateTime().Value;
            this._toDate = toDate.UnixToDateTime().Value;

            //this.stockDbContext = stockDbContext;
            this._affectObjects = new List<CensoredInventoryInputObject>();
            this._inventory = inventory;
            this._inventoryDetail = inventoryDetail;

            _packages = stockDbContext.Package
                   .Where(p => p.StockId == inventory.StockId && p.ProductId == inventoryDetail.ProductId && p.ProductUnitConversionId == inventoryDetail.ProductUnitConversionId)
                   .AsQueryable();

            _packageRefs = (
                from r in stockDbContext.PackageRef
                join p in stockDbContext.Package on r.PackageId equals p.PackageId
                //join pf in _stockDbContext.Package on r.RefPackageId equals pf.PackageId
                where p.StockId == inventory.StockId &&
                        p.ProductId == inventoryDetail.ProductId &&
                        p.ProductUnitConversionId == inventoryDetail.ProductUnitConversionId
                select r
                ).AsQueryable();

            _inventoryAffectDetails = (
                from id in stockDbContext.InventoryDetail
                join iv in stockDbContext.Inventory on id.InventoryId equals iv.InventoryId
                where iv.StockId == inventory.StockId &&
                        id.ProductId == inventoryDetail.ProductId &&
                        id.ProductUnitConversionId == inventoryDetail.ProductUnitConversionId &&
                        iv.IsApproved
                select new InventoryDetailAffectModel
                {
                    InventoryId = iv.InventoryId,
                    InventoryCode = iv.InventoryCode,
                    InventoryDetailId = id.InventoryDetailId,
                    ToPackageId = id.ToPackageId,
                    FromPackageId = id.FromPackageId,
                    PrimaryQuantity = id.PrimaryQuantity,
                    ProductUnitConversionQuantity = id.ProductUnitConversionQuantity,
                    Date = iv.Date
                })
                .AsQueryable();
        }

        public IList<CensoredInventoryInputObject> GetAffectObjects(decimal newPrimaryQuantity, decimal newProductUnitConversionQuantity)
        {
            var topPackage = _packages.FirstOrDefault(p => p.PackageId == _inventoryDetail.ToPackageId);

            //Lấy thông tin về phiếu nhập vào Kiện
            AffectInventoryInput(newPrimaryQuantity, newProductUnitConversionQuantity);

            var queue = new Queue<long>();

            //Duyệt đồng cấp (nút cha duyệt trước)
            queue.Enqueue(topPackage.PackageId);
            while (queue.Count > 0)
            {
                var packageId = queue.Dequeue();

                //INPUT
                //Lấy các kiện cha ảnh hưởng tới kiện hiện tại
                AffectAddParentPackages(packageId);

                //Lấy các phiếu nhập ảnh hưởng đến kiện hiện tại
                AffectAddParentInputs(packageId);

                //OUTPUT
                //Lấy các kiện con và phiếu xuất mà kiện hiện tại ảnh hưởng đến
                var childPackgeIds = AffectAddChildren(packageId);

                foreach (var id in childPackgeIds)
                {
                    queue.Enqueue(id);
                }
            }

            return _affectObjects;
        }

        private void AffectInventoryInput(decimal newPrimaryQuantity, decimal newProductUnitConversionQuantity)
        {
            _affectObjects.Add(new CensoredInventoryInputObject()
            {
                ObjectId = _inventoryDetail.InventoryDetailId,
                ObjectCode = _inventory.InventoryCode,
                ObjectTypeId = EnumObjectType.InventoryDetail,
                IsRoot = true,
                IsCurrentFlow = true,

                OldPrimaryQuantity = _inventoryDetail.PrimaryQuantity,
                NewPrimaryQuantity = newPrimaryQuantity,

                OldProductUnitConversionQuantity = _inventoryDetail.ProductUnitConversionQuantity,
                NewProductUnitConversionQuantity = newProductUnitConversionQuantity,

                Children = new List<TransferToObject>()
                    {
                        new TransferToObject {
                            IsEditable = false,
                            ObjectId = _inventoryDetail.ToPackageId.Value,
                            ObjectTypeId = EnumObjectType.Package,
                            PackageOperationTypeId = EnumPackageOperationType.Join,

                            OldTransferPrimaryQuantity = _inventoryDetail.PrimaryQuantity,
                            NewTransferPrimaryQuantity = newPrimaryQuantity,

                            OldTransferProductUnitConversionQuantity = _inventoryDetail.ProductUnitConversionQuantity,
                            NewTransferProductUnitConversionQuantity = newProductUnitConversionQuantity,
                        }
                    }
            });
        }

        private void AffectAddParentPackages(long packageId)
        {
            var refParentPackages = _packageRefs.Where(r => r.PackageId == packageId).ToList();

            var refParentPackageIds = refParentPackages.Select(r => r.RefPackageId);

            var refParentPackageInfos = _packages.Where(p => refParentPackageIds.Contains(p.PackageId)).ToList();

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

                if (!_affectObjects.Any(a => a.ObjectKey == newObject.ObjectKey))
                {
                    _affectObjects.Add(newObject);
                }
            }

        }

        private CensoredInventoryInputObject _otherInventoryInput = null;
        private void AffectAddParentInputs(long packageId)
        {
            var refInventoryIns = _inventoryAffectDetails.Where(id => id.ToPackageId == packageId).ToList();

            foreach (var r in refInventoryIns)
            {
                if (r.InventoryDetailId == _inventoryDetail.InventoryDetailId)
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
                                    ObjectTypeId = EnumObjectType.Package,
                                    PackageOperationTypeId = EnumPackageOperationType.Join,

                                    OldTransferPrimaryQuantity = r.PrimaryQuantity,
                                    NewTransferPrimaryQuantity = r.PrimaryQuantity,

                                    OldTransferProductUnitConversionQuantity = r.ProductUnitConversionQuantity,
                                    NewTransferProductUnitConversionQuantity = r.ProductUnitConversionQuantity
                                }
                            }
                    };

                    if (!_affectObjects.Any(a => a.ObjectKey == newObject.ObjectKey))
                    {
                        _affectObjects.Add(newObject);
                    }
                }
                else
                {
                    if (_otherInventoryInput == null)
                    {
                        _otherInventoryInput = new CensoredInventoryInputObject()
                        {
                            ObjectId = OTHER_INVENTORY_INPUT_ID,
                            ObjectCode = OtherInventoryOutputCode,
                            ObjectTypeId = EnumObjectType.InventoryDetail,
                            IsRoot = false,

                            OldPrimaryQuantity = 0,
                            NewPrimaryQuantity = 0,

                            OldProductUnitConversionQuantity = 0,
                            NewProductUnitConversionQuantity = 0,

                            Children = new List<TransferToObject>()
                        };

                        _affectObjects.Add(_otherInventoryInput);

                    }


                    _otherInventoryInput.OldPrimaryQuantity += r.PrimaryQuantity;
                    _otherInventoryInput.NewPrimaryQuantity += r.PrimaryQuantity;

                    _otherInventoryInput.OldProductUnitConversionQuantity += r.ProductUnitConversionQuantity;
                    _otherInventoryInput.NewProductUnitConversionQuantity += r.ProductUnitConversionQuantity;

                    var child = _otherInventoryInput.Children.FirstOrDefault(c => c.ObjectTypeId == EnumObjectType.Package && c.ObjectId == packageId);
                    if (child != null)
                    {
                        child.OldTransferPrimaryQuantity += r.PrimaryQuantity;
                        child.NewTransferPrimaryQuantity += r.PrimaryQuantity;

                        child.OldTransferProductUnitConversionQuantity += r.ProductUnitConversionQuantity;
                        child.NewTransferProductUnitConversionQuantity += r.ProductUnitConversionQuantity;
                    }
                    else
                    {
                        _otherInventoryInput.Children.Add(
                                        new TransferToObject
                                        {
                                            IsEditable = false,
                                            ObjectId = packageId,
                                            ObjectTypeId = EnumObjectType.Package,
                                            PackageOperationTypeId = EnumPackageOperationType.Join,

                                            OldTransferPrimaryQuantity = r.PrimaryQuantity,
                                            NewTransferPrimaryQuantity = r.PrimaryQuantity,

                                            OldTransferProductUnitConversionQuantity = r.ProductUnitConversionQuantity,
                                            NewTransferProductUnitConversionQuantity = r.ProductUnitConversionQuantity
                                        }
                                    );
                    }
                }
            }

        }

        private IList<long> AffectAddChildren(long packageId)
        {
            var packageInfo = _packages.FirstOrDefault(p => p.PackageId == packageId);

            var childrenPackages = _packageRefs.Where(p => p.RefPackageId == packageId).ToList();

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

            AffectAddChildrenOut(currentPackageNode, packageId);

            if (!_affectObjects.Any(o => o.ObjectKey == currentPackageNode.ObjectKey))
                _affectObjects.Add(currentPackageNode);

            return childrenPackages.Select(c => c.PackageId).ToList();
        }

        private CensoredInventoryInputObject _otherInventoryOutput = null;
        private void AffectAddChildrenOut(CensoredInventoryInputObject currentPackageNode, long packageId)
        {

            var childrenInventoryOuts = _inventoryAffectDetails.Where(id => id.FromPackageId == packageId).ToList();

            foreach (var iv in childrenInventoryOuts)
            {
                if (iv.Date >= _fromDate && iv.Date <= _toDate)
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

                    if (!_affectObjects.Any(o => o.ObjectKey == outObject.ObjectKey))
                        _affectObjects.Add(outObject);
                }
                else
                {
                    if (_otherInventoryOutput == null)
                    {
                        _otherInventoryOutput = new CensoredInventoryInputObject()
                        {
                            IsRoot = false,
                            IsCurrentFlow = true,
                            ObjectId = OTHER_INVENTORY_OUTPUT_ID,
                            ObjectCode = OtherInventoryOutputCode,
                            ObjectTypeId = EnumObjectType.InventoryDetail,

                            OldPrimaryQuantity = 0,
                            NewPrimaryQuantity = 0,

                            OldProductUnitConversionQuantity = 0,
                            NewProductUnitConversionQuantity = 0,

                            Children = null
                        };

                        _affectObjects.Add(_otherInventoryOutput);
                    }

                    _otherInventoryOutput.OldPrimaryQuantity += iv.PrimaryQuantity;
                    _otherInventoryOutput.NewPrimaryQuantity += iv.PrimaryQuantity;

                    _otherInventoryOutput.OldProductUnitConversionQuantity += iv.ProductUnitConversionQuantity;
                    _otherInventoryOutput.NewProductUnitConversionQuantity += iv.ProductUnitConversionQuantity;

                    var currentOutput = currentPackageNode.Children.FirstOrDefault(c => c.ObjectTypeId == EnumObjectType.InventoryDetail && c.ObjectId == OTHER_INVENTORY_OUTPUT_ID);
                    if (currentOutput == null)
                    {
                        currentOutput = new TransferToObject()
                        {
                            IsEditable = false,
                            ObjectId = OTHER_INVENTORY_OUTPUT_ID,
                            ObjectTypeId = EnumObjectType.InventoryDetail,
                            PackageOperationTypeId = EnumPackageOperationType.Split,

                            OldTransferPrimaryQuantity = 0,
                            NewTransferPrimaryQuantity = 0,

                            OldTransferProductUnitConversionQuantity = 0,
                            NewTransferProductUnitConversionQuantity = 0
                        };

                        currentPackageNode.Children.Add(currentOutput);
                    }

                    currentOutput.OldTransferPrimaryQuantity += iv.PrimaryQuantity;
                    currentOutput.NewTransferPrimaryQuantity += iv.PrimaryQuantity;

                    currentOutput.OldTransferProductUnitConversionQuantity += iv.ProductUnitConversionQuantity;
                    currentOutput.NewTransferProductUnitConversionQuantity += iv.ProductUnitConversionQuantity;


                }

            }

        }

    }
}
