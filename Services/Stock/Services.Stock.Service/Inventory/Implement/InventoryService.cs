using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VErp.Services.Master.Service.Dictionay;
using VErp.Commons.Enums.StandardEnum;
using VErp.Services.Master.Service.Activity;
using VErp.Commons.Enums.MasterEnum;
using VErp.Services.Stock.Model.Product;
using VErp.Commons.Library;
using System.Globalization;

namespace VErp.Services.Stock.Service.Invetory.Implement
{
    public class InventoryService : IInventoryService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IUnitService _unitService;
        private readonly IActivityService _activityService;

        public InventoryService(StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<InventoryService> logger
            , IUnitService unitService
            , IActivityService activityService)
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _unitService = unitService;
            _activityService = activityService;
        }

        /// <summary>
        /// Lấy danh sách phiếu nhập / xuất kho
        /// </summary>
        /// <param name="keyword">Tìm kiếm trong Mã phiếu, mã SP, tên SP, tên người gủi/nhận, tên Obj liên quan RefObjectCode</param>
        /// <param name="stockId">Id kho</param>
        /// <param name="type">Loại typeId: 1 nhập ; 2 : xuất kho theo MasterEnum.EnumInventory</param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public async Task<PageData<InventoryOutput>> GetList(string keyword, int stockId = 0, EnumInventory type = 0, DateTime? beginTime = null, DateTime? endTime = null, int page = 1, int size = 10)
        {
            var query = from i in _stockDbContext.Inventory
                        join id in _stockDbContext.InventoryDetail on i.InventoryId equals id.InventoryId
                        join p in _stockDbContext.Product on id.ProductId equals p.ProductId
                        join s in _stockDbContext.Stock on i.StockId equals s.StockId                        
                        select new { i, id, p, s };
            if (stockId > 0)
            {
                query = query.Where(q => q.i.StockId == stockId);
            }

            if (type > 0 && Enum.IsDefined(typeof(EnumInventory), type))
            {
                query = query.Where(q => q.i.InventoryTypeId == (int)type);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from q in query
                        where q.i.InventoryCode.Contains(keyword) || q.p.ProductCode.Contains(keyword) || q.p.ProductName.Contains(keyword) || q.i.Shipper.Contains(keyword) || q.id.RefObjectCode.Contains(keyword)
                        select q;
            }

            if (beginTime.HasValue && endTime.HasValue)
            {
                query = query.Where(q => q.i.DateUtc >= beginTime && q.i.DateUtc <= endTime);
            }
            else
            {
                if (beginTime.HasValue)
                {
                    query = query.Where(q => q.i.DateUtc >= beginTime);
                }
                if (endTime.HasValue)
                {
                    query = query.Where(q => q.i.DateUtc <= endTime);
                }
            }

            var total = query.Count();
            var dataList = query.AsNoTracking().Skip((page - 1) * size).Take(size).ToList();

            var inventoryList = dataList.Select(q => q.i).ToList();



            //var fileList = dataList.Select(q => q.f).ToList();
            var pagedData = new List<InventoryOutput>();
            foreach (var item in inventoryList)
            {
                var attachedFiles = new List<File>(4);
                #region Get Attached files 
                if (_stockDbContext.InventoryFile.Any(q => q.InventoryId == item.InventoryId))
                {
                    var fileIdlist = _stockDbContext.InventoryFile.Where(q => q.InventoryId == item.InventoryId).Select(q => q.FileId).ToList();
                    attachedFiles = _stockDbContext.File.Where(q => fileIdlist.Contains(q.FileId)).ToList();
                }
                #endregion

                var listInventoryDetails = dataList.Where(q => q.id.InventoryId == item.InventoryId).Select(q => q.id).ToList();
                var listInventoryDetailsOutput = new List<InventoryDetailOutput>(listInventoryDetails.Count);

                foreach (var details in listInventoryDetails)
                {
                    var productInfo = dataList.Select(q => q.p).FirstOrDefault(q => q.ProductId == details.ProductId);
                    var productOutput = new ProductListOutput
                    {
                        ProductId = productInfo.ProductId,
                        ProductCode = productInfo.ProductCode,
                        ProductName = productInfo.ProductName,
                        MainImageFileId = productInfo.MainImageFileId,
                        ProductTypeId = productInfo.ProductTypeId,
                        ProductTypeName = string.Empty,
                        ProductCateId = productInfo.ProductCateId,
                        ProductCateName = string.Empty,
                        Barcode = productInfo.Barcode,
                        Specification = string.Empty,
                        UnitId = productInfo.UnitId,
                        UnitName = string.Empty
                    };
                    listInventoryDetailsOutput.Add(new InventoryDetailOutput
                    {
                        InventoryId = details.InventoryId,
                        InventoryDetailId = details.InventoryDetailId,
                        ProductId = details.ProductId,
                        PrimaryUnitId = details.PrimaryUnitId,
                        PrimaryQuantity = details.PrimaryQuantity,
                        SecondaryUnitId = details.SecondaryUnitId,
                        SecondaryQuantity = details.SecondaryQuantity,
                        RefObjectTypeId = details.RefObjectTypeId,
                        RefObjectId = details.RefObjectId,
                        RefObjectCode = details.RefObjectCode,

                        ProductOutput = productOutput
                    });
                }

                var stockInfo = dataList.Select(q => q.s).FirstOrDefault(q => q.StockId == item.StockId);

                var inventoryOutput = new InventoryOutput()
                {
                    InventoryId = item.InventoryId,
                    StockId = item.StockId,
                    InventoryCode = item.InventoryCode,
                    InventoryTypeId = item.InventoryTypeId,
                    Shipper = item.Shipper,
                    Content = item.Content,
                    DateUtc = item.DateUtc,
                    CustomerId = item.CustomerId,
                    Department = item.Department,
                    UserId = item.UserId,
                    IsApproved = item.IsApproved,
                    CreatedByUserId = item.CreatedByUserId,
                    UpdatedByUserId = item.UpdatedByUserId,

                    StockOutput = stockInfo == null ? null : new Model.Stock.StockOutput
                    {
                        StockId = stockInfo.StockId,
                        StockName = stockInfo.StockName,
                        StockKeeperName = stockInfo.StockKeeperName,
                        StockKeeperId = stockInfo.StockKeeperId
                    },
                    InventoryDetailOutputList = listInventoryDetailsOutput,
                    FileList = attachedFiles
                };
                pagedData.Add(inventoryOutput);
            }
            return (pagedData, total);
        }

        public async Task<ServiceResult<InventoryOutput>> GetInventory(int inventoryId)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetInventory");
                return GeneralCode.InternalError;
            }
        }

        /// <summary>
        /// Thêm mới phiếu nhập  / xuất kho
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<ServiceResult<long>> AddInventory(int currentUserId, InventoryInput req)
        {
            try
            {
                if (req == null || req.InventoryDetailInputList.Count < 1)
                {
                    return GeneralCode.InvalidParams;
                }
                if (Enum.IsDefined(typeof(EnumInventory), req.InventoryTypeId) == false)
                {
                    return GeneralCode.InvalidParams;
                }
                if (string.IsNullOrEmpty(req.DateUtc))
                {
                    return GeneralCode.InvalidParams;
                }
                var issuedDate = DateTime.MinValue;

                if (!DateTime.TryParseExact(req.DateUtc, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out issuedDate))
                {
                    return GeneralCode.InvalidParams;
                }                
                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var inventoryObj = new Inventory
                        {
                            StockId = req.StockId,
                            InventoryCode = req.InventoryCode,
                            InventoryTypeId = req.InventoryTypeId,
                            Shipper = req.Shipper,
                            Content = req.Content,
                            DateUtc = issuedDate,
                            CustomerId = req.CustomerId,
                            Department = req.Department,
                            UserId = req.UserId,
                            CreatedByUserId = currentUserId,
                            UpdatedByUserId = currentUserId,
                            CreatedDatetimeUtc = DateTime.Now,
                            UpdatedDatetimeUtc = DateTime.Now,
                            IsDeleted = false,
                            IsApproved = false
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

                        //switch (req.InventoryTypeId)
                        //{
                        //    case (int)EnumInventory.Input:
                        //        break;
                        //    case (int)EnumInventory.Output:
                        //        break;
                        //}
                        if (req.InventoryDetailInputList.Count > 0)
                        {
                            var inventoryDetailList = new List<InventoryDetail>(req.InventoryDetailInputList.Count);
                            foreach (var details in req.InventoryDetailInputList)
                            {
                                switch (req.InventoryTypeId)
                                {
                                    //long currentPackageId = 0;
                                    case (int)EnumInventory.Input:
                                        //var productObj = _stockDbContext.Product.FirstOrDefault(q => q.ProductId == details.ProductId);
                                        //var newPackageCode = CreatePackageCode(inventoryObj.InventoryCode, (productObj.ProductCode ?? string.Empty), DateTime.Now);
                                        //await _stockDbContext.AddAsync(package);
                                        //await _stockDbContext.SaveChangesAsync();
                                        //currentPackageId = package.PackageId;
                                        break;
                                    case (int)EnumInventory.Output:
                                        //currentPackageId = details.PackageId;
                                        break;
                                }
                                inventoryDetailList.Add(new InventoryDetail
                                {
                                    InventoryId = inventoryObj.InventoryId,
                                    ProductId = details.ProductId,
                                    CreatedDatetimeUtc = DateTime.Now,
                                    UpdatedDatetimeUtc = DateTime.Now,
                                    IsDeleted = false,
                                    PrimaryUnitId = details.PrimaryUnitId,
                                    PrimaryQuantity = details.PrimaryQuantity,
                                    SecondaryUnitId = details.SecondaryUnitId,
                                    SecondaryQuantity = details.SecondaryUnitId,
                                    RefObjectTypeId = details.RefObjectTypeId,
                                    RefObjectId = details.RefObjectId,
                                    RefObjectCode = details.RefObjectCode,
                                    FromPackageId = details.FromPackageId ?? null
                                });
                                await _stockDbContext.AddRangeAsync(inventoryDetailList);
                                await _stockDbContext.SaveChangesAsync();
                            }
                        }
                        trans.Commit();
                        var objLog = GetInventoryInfoForLog(inventoryObj);
                        _activityService.CreateActivityAsync(EnumObjectType.Inventory, inventoryObj.InventoryId, $"Thêm mới phiếu nhập/xuất kho, mã: {inventoryObj.InventoryCode} ", null, objLog);

                        return inventoryObj.InventoryId;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        _logger.LogError(ex, "AddInventory");
                        return GeneralCode.InternalError;
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "UpdateInventory");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> UpdateInventory(int inventoryId, int currentUserId, InventoryInput model)
        {
            try
            {
                if (inventoryId <= 0)
                {
                    return InventoryErrorCode.InventoryNotFound;
                }
                if (!_stockDbContext.Inventory.Any(q => q.InventoryId == inventoryId))
                {
                    return InventoryErrorCode.InventoryNotFound;
                }
                var issuedDate = DateTime.MinValue;

                if (!DateTime.TryParseExact(model.DateUtc, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out issuedDate))
                {
                    return GeneralCode.InvalidParams;
                }

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        #region Update Inventory - Phiếu nhập xuất kho
                        var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);

                        var originalObj = GetInventoryInfoForLog(inventoryObj);

                        inventoryObj.StockId = model.StockId;
                        inventoryObj.InventoryCode = model.InventoryCode;
                        inventoryObj.InventoryTypeId = model.InventoryTypeId;
                        inventoryObj.Shipper = model.Shipper;
                        inventoryObj.Content = model.Content;
                        inventoryObj.DateUtc = issuedDate;
                        inventoryObj.CustomerId = model.CustomerId;
                        inventoryObj.Department = model.Department;
                        inventoryObj.UserId = model.UserId;

                        inventoryObj.UpdatedByUserId = currentUserId;
                        inventoryObj.UpdatedDatetimeUtc = DateTime.Now;
                        #endregion

                        #region Update Inventory Details - Chi tiết phiếu nhập xuất kho
                        var oldInventoryDetailIdList = _stockDbContext.InventoryDetail.AsNoTracking().Where(q => q.InventoryId == inventoryId).Select(q => q.InventoryDetailId).ToList();
                        if (model.InventoryDetailInputList.Count > 0)
                        {
                            var postedInventoryDetailIdList = model.InventoryDetailInputList.Select(q => q.InventoryDetailId).ToList();
                            var deletedInventoryDetailIdList = oldInventoryDetailIdList.Except(postedInventoryDetailIdList).ToList();

                            // đánh dấu xoá 
                            if (deletedInventoryDetailIdList.Count > 0)
                            {
                                var deletedInventoryDetails = _stockDbContext.InventoryDetail.Where(q => deletedInventoryDetailIdList.Contains(q.InventoryDetailId)).ToList();
                                foreach (var item in deletedInventoryDetails)
                                {
                                    item.IsDeleted = true;
                                    item.UpdatedDatetimeUtc = DateTime.Now;
                                }

                            }
                            var insertedInventoryDetailList = new List<InventoryDetail>(model.InventoryDetailInputList.Count);
                            
                            foreach (var item in model.InventoryDetailInputList.Where(q => !deletedInventoryDetailIdList.Contains(q.InventoryDetailId)))
                            {
                                if (item.InventoryDetailId < 1)
                                {
                                    insertedInventoryDetailList.Add(new InventoryDetail
                                    {
                                        InventoryId = inventoryId,
                                        ProductId = item.ProductId,
                                        PrimaryUnitId = item.PrimaryUnitId,
                                        PrimaryQuantity = item.PrimaryQuantity,
                                        SecondaryUnitId = item.SecondaryUnitId ?? null,
                                        SecondaryQuantity = item.SecondaryQuantity ?? null,
                                        FromPackageId = item.FromPackageId ?? null,
                                        RefObjectId = item.RefObjectId ?? null,
                                        RefObjectTypeId = item.RefObjectTypeId ?? null,
                                        RefObjectCode = item.RefObjectCode ?? string.Empty,
                                        CreatedDatetimeUtc = DateTime.Now,
                                        UpdatedDatetimeUtc = DateTime.Now,
                                        IsDeleted = false
                                    });
                                }
                                if (item.InventoryDetailId > 0 && item.IsUpdated)
                                {
                                    var updatedItem = new InventoryDetail
                                    {
                                        InventoryDetailId = item.InventoryDetailId,
                                    };
                                    _stockDbContext.InventoryDetail.Attach(updatedItem);

                                    //updatedItem.InventoryId = inventoryId;
                                    updatedItem.ProductId = item.ProductId;
                                    updatedItem.PrimaryUnitId = item.PrimaryUnitId;
                                    updatedItem.PrimaryQuantity = item.PrimaryQuantity;
                                    updatedItem.SecondaryUnitId = item.SecondaryUnitId ?? null;
                                    updatedItem.SecondaryQuantity = item.SecondaryQuantity ?? null;
                                    updatedItem.FromPackageId = item.FromPackageId ?? null;
                                    updatedItem.RefObjectId = item.RefObjectId ?? null;
                                    updatedItem.RefObjectTypeId = item.RefObjectTypeId ?? null;
                                    updatedItem.RefObjectCode = item.RefObjectCode ?? string.Empty;
                                    //updatedItem.CreatedDatetimeUtc = DateTime.Now;
                                    updatedItem.UpdatedDatetimeUtc = DateTime.Now;
                                    //updatedItem.IsDeleted = false;
                                }

                            }
                            await _stockDbContext.InventoryDetail.AddRangeAsync(insertedInventoryDetailList);
                            await _stockDbContext.SaveChangesAsync();
                        }
                        #endregion

                        await _stockDbContext.SaveChangesAsync();
                        trans.Commit();

                        var objLog = GetInventoryInfoForLog(inventoryObj);
                        _activityService.CreateActivityAsync(EnumObjectType.Inventory, inventoryObj.InventoryId, $"Cập nhật thông tin phiếu nhập/xuất kho, mã: {inventoryObj.InventoryCode} ", originalObj.JsonSerialize(), objLog);                        
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        _logger.LogError(ex, "UpdateInventory");
                        return GeneralCode.InternalError;
                    }
                }
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateInventory");
                return GeneralCode.InternalError;
            }
        }


        public async Task<Enum> DeleteInventory(int inventoryId, int currentUserId)
        {
            var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(p => p.InventoryId == inventoryId);
            var objLog = GetInventoryInfoForLog(inventoryObj);
            if (inventoryObj == null)
            {
                return InventoryErrorCode.InventoryNotFound;
            }
            inventoryObj.IsDeleted = true;
            inventoryObj.IsApproved = false;
            inventoryObj.UpdatedByUserId = currentUserId;
            inventoryObj.UpdatedDatetimeUtc = DateTime.UtcNow;

            var dataBefore = objLog.JsonSerialize();

            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var typeName = inventoryObj.InventoryTypeId == (int)EnumInventory.Input ? "phiếu nhập kho" : "phiếu xuất kho";
                    await _stockDbContext.SaveChangesAsync();
                    trans.Commit();
                    _activityService.CreateActivityAsync(EnumObjectType.Product, inventoryObj.StockId, string.Format("Xóa {0} | mã phiếu {1}", typeName, inventoryObj.InventoryCode), dataBefore, null);
                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "DeleteStock");
                    return GeneralCode.InternalError;
                }
            }
        }

        #region Private helper method

        private object GetInventoryInfoForLog(VErp.Infrastructure.EF.StockDB.Inventory inventoryObj)
        {
            return inventoryObj;
        }

        private string CreatePackageCode(string inventoryCode, string productCode, DateTime productManufactureDateTimeUtc)
        {
            var packageCode = string.Format("{0}-{1}-{2},", inventoryCode, productCode, productManufactureDateTimeUtc.ToString("YYYYMMdd"));
            return packageCode;
        }
        #endregion
    }
}
