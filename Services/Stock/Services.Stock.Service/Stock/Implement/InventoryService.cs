using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Config;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.FileResources;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Package;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.FileResources;
using PackageEntity = VErp.Infrastructure.EF.StockDB.Package;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public partial class InventoryService : IInventoryService
    {
        const decimal MINIMUM_JS_NUMBER = Numbers.MINIMUM_ACCEPT_DECIMAL_NUMBER;

        private readonly MasterDBContext _masterDBContext;
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IUnitService _unitService;
        private readonly IFileService _fileService;
        private readonly IObjectGenCodeService _objectGenCodeService;
        private readonly IAsyncRunnerService _asyncRunner;


        public InventoryService(MasterDBContext masterDBContext, StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<InventoryService> logger
            , IActivityLogService activityLogService
            , IUnitService unitService
            , IFileService fileService
            , IObjectGenCodeService objectGenCodeService
            , IAsyncRunnerService asyncRunner
            )
        {
            _masterDBContext = masterDBContext;
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _unitService = unitService;
            _fileService = fileService;
            _objectGenCodeService = objectGenCodeService;
            _asyncRunner = asyncRunner;
        }



        public Task<PageData<InventoryOutput>> GetList(string keyword, int stockId = 0, EnumInventoryType type = 0, long beginTime = 0, long endTime = 0, string sortBy = "date", bool asc = false, int page = 1, int size = 10)
        {
            var bTime = DateTime.MinValue;
            var eTime = DateTime.MinValue;

            if (beginTime > 0)
            {
                bTime = beginTime.UnixToDateTime();
            }
            if (endTime > 0)
            {
                eTime = endTime.UnixToDateTime();
                eTime = eTime.AddDays(1);
            }

            var query = from i in _stockDbContext.Inventory
                        select i;
            if (stockId > 0)
            {
                query = query.Where(q => q.StockId == stockId);
            }

            if (type > 0 && Enum.IsDefined(typeof(EnumInventoryType), type))
            {
                query = query.Where(q => q.InventoryTypeId == (int)type);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(q => q.InventoryCode.Contains(keyword) || q.Shipper.Contains(keyword));
            }

            if (bTime != DateTime.MinValue && eTime != DateTime.MinValue)
            {
                query = query.Where(q => q.Date >= bTime && q.Date < eTime);
            }
            else
            {
                if (bTime != DateTime.MinValue)
                {
                    query = query.Where(q => q.Date >= bTime);
                }
                if (eTime != DateTime.MinValue)
                {
                    query = query.Where(q => q.Date < eTime);
                }
            }
            query = query.SortByFieldName(sortBy, asc);

            var total = query.Count();
            var inventoryDataList = query.AsNoTracking().Skip((page - 1) * size).Take(size).ToList();

            // var inventoryIdList = inventoryDataList.Select(q => q.InventoryId).ToList();
            // var inventoryDetailsDataList = _stockDbContext.InventoryDetail.AsNoTracking().Where(q => inventoryIdList.Contains(q.InventoryId)).ToList();

            // var productIdList = inventoryDetailsDataList.Select(q => q.ProductId).Distinct().ToList();
            //var productDataList = _stockDbContext.Product.AsNoTracking().Where(q => productIdList.Contains(q.ProductId)).ToList();

            var pagedData = new List<InventoryOutput>();
            foreach (var item in inventoryDataList)
            {
                var stockInfo = _stockDbContext.Stock.AsNoTracking().FirstOrDefault(q => q.StockId == item.StockId);

                var inventoryOutput = new InventoryOutput()
                {
                    InventoryId = item.InventoryId,
                    StockId = item.StockId,
                    InventoryCode = item.InventoryCode,
                    InventoryTypeId = item.InventoryTypeId,
                    Shipper = item.Shipper,
                    Content = item.Content,
                    Date = item.Date.GetUnix(),
                    CustomerId = item.CustomerId,
                    Department = item.Department,
                    StockKeeperUserId = item.StockKeeperUserId,
                    BillCode = item.BillCode,
                    BillSerial = item.BillSerial,
                    BillDate = item.BillDate.HasValue ? item.BillDate.Value.GetUnix() : (long?)null,
                    TotalMoney = item.TotalMoney,
                    IsApproved = item.IsApproved,
                    CreatedByUserId = item.CreatedByUserId,
                    UpdatedByUserId = item.UpdatedByUserId,
                    UpdatedDatetimeUtc = item.UpdatedDatetimeUtc.GetUnix(),
                    CreatedDatetimeUtc = item.CreatedDatetimeUtc.GetUnix(),

                    StockOutput = stockInfo == null ? null : new Model.Stock.StockOutput
                    {
                        StockId = stockInfo.StockId,
                        StockName = stockInfo.StockName,
                        StockKeeperName = stockInfo.StockKeeperName,
                        StockKeeperId = stockInfo.StockKeeperId
                    },
                    InventoryDetailOutputList = null,
                    FileList = null,
                };
                pagedData.Add(inventoryOutput);
            }
            return Task.FromResult((PageData<InventoryOutput>)(pagedData, total));
        }

        public async Task<ServiceResult<InventoryOutput>> GetInventory(long inventoryId)
        {
            try
            {
                if (inventoryId < 1)
                {
                    return GeneralCode.InvalidParams;
                }
                var inventoryObj = _stockDbContext.Inventory.AsNoTracking().FirstOrDefault(q => q.InventoryId == inventoryId);
                if (inventoryObj == null)
                {
                    return GeneralCode.InvalidParams;
                }

                #region Get inventory details
                var inventoryDetails = await _stockDbContext.InventoryDetail.Where(q => q.InventoryId == inventoryObj.InventoryId).AsNoTracking().OrderBy(s => s.SortOrder).ThenBy(s => s.CreatedDatetimeUtc).ToListAsync();

                var productIds = inventoryDetails.Select(d => d.ProductId).ToList();

                var productInfos = _stockDbContext.Product.AsNoTracking()
                    .Where(p => productIds.Contains(p.ProductId))
                    .ToList()
                    .ToDictionary(p => p.ProductId, p => p);

                var productUnitConversionIds = inventoryDetails.Select(d => d.ProductUnitConversionId).ToList();

                var productUnitConversions = _stockDbContext.ProductUnitConversion.AsNoTracking()
                    .Where(pu => productUnitConversionIds.Contains(pu.ProductUnitConversionId))
                    .ToList()
                    .ToDictionary(pu => pu.ProductUnitConversionId, pu => pu);

                var listInventoryDetailsOutput = new List<InventoryDetailOutput>(inventoryDetails.Count);
                foreach (var details in inventoryDetails)
                {
                    ProductListOutput productOutput = null;
                    if (productInfos.TryGetValue(details.ProductId, out var productInfo))
                    {
                        productOutput = new ProductListOutput
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
                    }

                    productUnitConversions.TryGetValue(details.ProductUnitConversionId ?? 0, out var productUnitConversionInfo);

                    listInventoryDetailsOutput.Add(new InventoryDetailOutput
                    {
                        InventoryId = details.InventoryId,
                        InventoryDetailId = details.InventoryDetailId,
                        ProductId = details.ProductId,
                        PrimaryUnitId = productInfo?.UnitId,
                        PrimaryQuantity = details.PrimaryQuantity,
                        UnitPrice = details.UnitPrice,
                        ProductUnitConversionId = details.ProductUnitConversionId,
                        ProductUnitConversionQuantity = details.ProductUnitConversionQuantity,
                        FromPackageId = details.FromPackageId,
                        ToPackageId = details.ToPackageId,
                        PackageOptionId = details.PackageOptionId,

                        RefObjectTypeId = details.RefObjectTypeId,
                        RefObjectId = details.RefObjectId,
                        RefObjectCode = details.RefObjectCode,
                        OrderCode = details.OrderCode,
                        POCode = details.Pocode,
                        ProductionOrderCode = details.ProductionOrderCode,

                        ProductOutput = productOutput,
                        ProductUnitConversion = productUnitConversionInfo ?? null,
                        SortOrder = details.SortOrder
                    });
                }
                #endregion

                #region Get Attached files 
                var attachedFiles = new List<FileToDownloadInfo>(4);
                if (_stockDbContext.InventoryFile.Any(q => q.InventoryId == inventoryObj.InventoryId))
                {
                    var fileIdArray = _stockDbContext.InventoryFile.Where(q => q.InventoryId == inventoryObj.InventoryId).Select(q => q.FileId).ToArray();
                    attachedFiles = _fileService.GetListFileUrl(fileIdArray, EnumThumbnailSize.Large);
                }
                #endregion

                var stockInfo = _stockDbContext.Stock.AsNoTracking().FirstOrDefault(q => q.StockId == inventoryObj.StockId);

                var inventoryOutput = new InventoryOutput()
                {
                    InventoryId = inventoryObj.InventoryId,
                    StockId = inventoryObj.StockId,
                    InventoryCode = inventoryObj.InventoryCode,
                    InventoryTypeId = inventoryObj.InventoryTypeId,
                    Shipper = inventoryObj.Shipper,
                    Content = inventoryObj.Content,
                    Date = inventoryObj.Date.GetUnix(),
                    CustomerId = inventoryObj.CustomerId,
                    Department = inventoryObj.Department,
                    StockKeeperUserId = inventoryObj.StockKeeperUserId,
                    BillCode = inventoryObj.BillCode,
                    BillSerial = inventoryObj.BillSerial,
                    BillDate = inventoryObj.BillDate != null ? ((DateTime)inventoryObj.BillDate).GetUnix() : 0,
                    TotalMoney = inventoryObj.TotalMoney,
                    IsApproved = inventoryObj.IsApproved,
                    CreatedByUserId = inventoryObj.CreatedByUserId,
                    UpdatedByUserId = inventoryObj.UpdatedByUserId,

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
                return inventoryOutput;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetInventory");
                return GeneralCode.InternalError;
            }
        }

        /// <summary>
        /// Thêm mới phiếu nhập kho
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<ServiceResult<long>> AddInventoryInput(int currentUserId, InventoryInModel req)
        {
            if (req == null || req.InProducts.Count == 0)
            {
                return GeneralCode.InvalidParams;
            }

            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(req.StockId)))
            {
                if (_stockDbContext.Inventory.Any(q => q.InventoryCode == req.InventoryCode.Trim()))
                {
                    return InventoryErrorCode.InventoryCodeAlreadyExisted;
                }

                var issuedDate = req.Date.UnixToDateTime();
                var billDate = req.BillDate.UnixToDateTime();
                var validInventoryDetails = await ValidateInventoryIn(false, req);

                if (!validInventoryDetails.Code.IsSuccess())
                {
                    return validInventoryDetails.Code;
                }

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var totalMoney = InputCalTotalMoney(validInventoryDetails.Data);

                        var inventoryObj = new Inventory
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
                            BillCode = req.BillCode,
                            BillSerial = req.BillSerial,
                            BillDate = billDate == DateTime.MinValue ? null : (DateTime?)billDate,
                            TotalMoney = totalMoney,
                            CreatedByUserId = currentUserId,
                            UpdatedByUserId = currentUserId,
                            CreatedDatetimeUtc = DateTime.UtcNow,
                            UpdatedDatetimeUtc = DateTime.UtcNow,
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


                        foreach (var item in validInventoryDetails.Data)
                        {
                            item.InventoryId = inventoryObj.InventoryId;
                        }
                        inventoryObj.TotalMoney = totalMoney;
                        await _stockDbContext.InventoryDetail.AddRangeAsync(validInventoryDetails.Data);
                        await _stockDbContext.SaveChangesAsync();

                        trans.Commit();

                        await _activityLogService.CreateLog(EnumObjectType.Inventory, inventoryObj.InventoryId, $"Thêm mới phiếu nhập kho, mã: {inventoryObj.InventoryCode}", req.JsonSerialize());

                        //Move file from tmp folder
                        if (req.FileIdList != null)
                        {
                            foreach (var fileId in req.FileIdList)
                            {
                                _asyncRunner.RunAsync<IFileService>(f => f.FileAssignToObject(EnumObjectType.Inventory, inventoryObj.InventoryId, fileId));
                            }
                        }
                        return inventoryObj.InventoryId;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        _logger.LogError(ex, "AddInventoryInput");
                        return GeneralCode.InternalError;
                    }
                }
            }
        }

        private decimal InputCalTotalMoney(IList<InventoryDetail> data)
        {
            var totalMoney = (decimal)0;
            foreach (var item in data)
            {
                totalMoney += (item.UnitPrice * item.PrimaryQuantity);
            }
            return totalMoney;
        }

        /// <summary>
        /// Thêm mới phiếu xuất kho
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<ServiceResult<long>> AddInventoryOutput(int currentUserId, InventoryOutModel req)
        {
            if (req == null || req.OutProducts.Count == 0)
            {
                return GeneralCode.InvalidParams;
            }
            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(req.StockId)))
            {
                if (_stockDbContext.Inventory.Any(q => q.InventoryCode == req.InventoryCode.Trim()))
                {
                    return InventoryErrorCode.InventoryCodeAlreadyExisted;
                }
                var issuedDate = req.Date.UnixToDateTime();

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var inventoryObj = new Inventory
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
                            BillCode = string.Empty,
                            BillSerial = string.Empty,
                            BillDate = null,
                            CreatedByUserId = currentUserId,
                            UpdatedByUserId = currentUserId,
                            CreatedDatetimeUtc = DateTime.UtcNow,
                            UpdatedDatetimeUtc = DateTime.UtcNow,
                            IsDeleted = false,
                            IsApproved = false
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

                        var processInventoryOut = await ProcessInventoryOut(inventoryObj, req);

                        if (!processInventoryOut.Code.IsSuccess())
                        {
                            trans.Rollback();
                            return processInventoryOut.Code;
                        }

                        await _stockDbContext.InventoryDetail.AddRangeAsync(processInventoryOut.Data);
                        await _stockDbContext.SaveChangesAsync();

                        trans.Commit();

                        await _activityLogService.CreateLog(EnumObjectType.Inventory, inventoryObj.InventoryId, $"Thêm mới phiếu xuất kho, mã: {inventoryObj.InventoryCode} ", req.JsonSerialize());

                        //Move file from tmp folder
                        if (req.FileIdList != null)
                        {
                            foreach (var fileId in req.FileIdList)
                            {
                                _asyncRunner.RunAsync<IFileService>(f => f.FileAssignToObject(EnumObjectType.Inventory, inventoryObj.InventoryId, fileId));
                            }
                        }
                        return inventoryObj.InventoryId;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        _logger.LogError(ex, "AddInventoryOutput");
                        return GeneralCode.InternalError;
                    }
                }
            }
        }

        /// <summary>
        /// Cập nhật phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Enum> UpdateInventoryInput(long inventoryId, int currentUserId, InventoryInModel req)
        {
            if (inventoryId <= 0)
            {
                return InventoryErrorCode.InventoryNotFound;
            }

            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(req.StockId)))
            {

                var issuedDate = req.Date.UnixToDateTime();
                var billDate = req.Date.UnixToDateTime();

                var validate = await ValidateInventoryIn(false, req);

                if (!validate.Code.IsSuccess())
                {
                    return validate.Code;
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
                            return InventoryErrorCode.InventoryNotFound;
                        }

                        if (inventoryObj.StockId != req.StockId)
                        {
                            trans.Rollback();
                            return InventoryErrorCode.CanNotChangeStock;
                        }

                        if (inventoryObj.IsApproved)
                        {
                            trans.Rollback();
                            return GeneralCode.NotYetSupported;
                        }

                        //inventoryObj.StockId = req.StockId; Khong cho phep sua kho
                        inventoryObj.InventoryCode = req.InventoryCode;
                        inventoryObj.Date = issuedDate;
                        inventoryObj.Shipper = req.Shipper;
                        inventoryObj.Content = req.Content;
                        inventoryObj.CustomerId = req.CustomerId;
                        inventoryObj.Department = req.Department;
                        inventoryObj.StockKeeperUserId = req.StockKeeperUserId;
                        inventoryObj.BillCode = req.BillCode;
                        inventoryObj.BillSerial = req.BillSerial;
                        inventoryObj.BillDate = billDate == DateTime.MinValue ? null : (DateTime?)billDate;
                        inventoryObj.UpdatedByUserId = currentUserId;
                        inventoryObj.UpdatedDatetimeUtc = DateTime.UtcNow;

                        #endregion

                        var inventoryDetails = await _stockDbContext.InventoryDetail.Where(d => d.InventoryId == inventoryId).ToListAsync();
                        foreach (var d in inventoryDetails)
                        {
                            d.IsDeleted = true;
                            d.UpdatedDatetimeUtc = DateTime.UtcNow;
                        }
                        var totalMoney = (decimal)0;
                        foreach (var item in validate.Data)
                        {
                            item.InventoryId = inventoryObj.InventoryId;

                            totalMoney += (item.UnitPrice * item.PrimaryQuantity);
                        }
                        inventoryObj.TotalMoney = totalMoney;

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


                        var messageLog = string.Format("Cập nhật phiếu nhập kho, mã: {0}", inventoryObj.InventoryCode);
                        await _activityLogService.CreateLog(EnumObjectType.Inventory, inventoryObj.InventoryId, messageLog, req.JsonSerialize());
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        _logger.LogError(ex, "UpdateInventoryInput");
                        return GeneralCode.InternalError;
                    }
                }

                //Move file from tmp folder
                if (req.FileIdList != null)
                {
                    foreach (var fileId in req.FileIdList)
                    {
                        _asyncRunner.RunAsync<IFileService>(f => f.FileAssignToObject(EnumObjectType.Inventory, inventoryId, fileId));
                    }
                }
                return GeneralCode.Success;
            }
        }

        /// <summary>
        /// Cập nhật phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Enum> UpdateInventoryOutput(long inventoryId, int currentUserId, InventoryOutModel req)
        {
            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(req.StockId)))
            {
                var issuedDate = req.Date.UnixToDateTime();

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {

                        var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);
                        if (inventoryObj == null)
                        {
                            trans.Rollback();
                            return InventoryErrorCode.InventoryNotFound;
                        }

                        if (inventoryObj.StockId != req.StockId)
                        {
                            trans.Rollback();
                            return InventoryErrorCode.CanNotChangeStock;
                        }

                        var rollbackResult = await RollbackInventoryOutput(inventoryObj);
                        if (!rollbackResult.IsSuccess())
                        {
                            trans.Rollback();
                            return rollbackResult;
                        }


                        var processInventoryOut = await ProcessInventoryOut(inventoryObj, req);

                        if (!processInventoryOut.Code.IsSuccess())
                        {
                            trans.Rollback();
                            return processInventoryOut.Code;
                        }
                        await _stockDbContext.InventoryDetail.AddRangeAsync(processInventoryOut.Data);


                        //note: update IsApproved after RollbackInventoryOutput
                        inventoryObj.InventoryCode = req.InventoryCode;
                        inventoryObj.Shipper = req.Shipper;
                        inventoryObj.Content = req.Content;
                        inventoryObj.Date = issuedDate;
                        inventoryObj.CustomerId = req.CustomerId;
                        inventoryObj.Department = req.Department;
                        inventoryObj.StockKeeperUserId = req.StockKeeperUserId;
                        inventoryObj.IsApproved = false;
                        inventoryObj.UpdatedByUserId = currentUserId;
                        inventoryObj.UpdatedDatetimeUtc = DateTime.UtcNow;


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

                        var messageLog = string.Format("Cập nhật phiếu xuất kho, mã:", inventoryObj.InventoryCode);
                        await _activityLogService.CreateLog(EnumObjectType.Inventory, inventoryObj.InventoryId, messageLog, req.JsonSerialize());
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        _stockDbContext.RollbackEntities();
                        _logger.LogError(ex, "UpdateInventoryOutput");
                        return GeneralCode.InternalError;
                    }
                }

                //Move file from tmp folder
                if (req.FileIdList != null)
                {
                    foreach (var fileId in req.FileIdList)
                    {
                        _asyncRunner.RunAsync<IFileService>(f => f.FileAssignToObject(EnumObjectType.Inventory, inventoryId, fileId));
                    }
                }
                return GeneralCode.Success;
            }
        }

        /// <summary>
        /// Xoá phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        public async Task<Enum> DeleteInventoryInput(long inventoryId, int currentUserId)
        {
            var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(p => p.InventoryId == inventoryId);
            if (inventoryObj == null)
            {
                return InventoryErrorCode.InventoryNotFound;
            }


            if (inventoryObj.InventoryTypeId == (int)EnumInventoryType.Output)
            {
                return GeneralCode.InvalidParams;
            }

            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(inventoryObj.StockId)))
            {
                //reload inventory after lock
                inventoryObj = _stockDbContext.Inventory.FirstOrDefault(p => p.InventoryId == inventoryId);

                if (inventoryObj.IsApproved)
                {
                    /*Khong duoc phep xoa phieu nhap da duyet (Cần xóa theo lưu đồ, flow)*/
                    return InventoryErrorCode.NotSupportedYet;

                    //var processResult = await RollBackInventoryInput(inventoryObj);
                    //if (!Equals(processResult, GeneralCode.Success))
                    //{
                    //    trans.Rollback();
                    //    return GeneralCode.InvalidParams;
                    //}
                }

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        inventoryObj.IsDeleted = true;
                        //inventoryObj.IsApproved = false;
                        inventoryObj.UpdatedByUserId = currentUserId;
                        inventoryObj.UpdatedDatetimeUtc = DateTime.UtcNow;

                        await _activityLogService.CreateLog(EnumObjectType.Inventory, inventoryObj.InventoryId, string.Format("Xóa phiếu nhập kho, mã phiếu {0}", inventoryObj.InventoryCode), inventoryObj.JsonSerialize());
                        await _stockDbContext.SaveChangesAsync();
                        trans.Commit();

                        return GeneralCode.Success;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        _logger.LogError(ex, "DeleteInventoryInput");
                        return GeneralCode.InternalError;
                    }
                }
            }
        }

        /// <summary>
        /// Xoá phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        public async Task<Enum> DeleteInventoryOutput(long inventoryId, int currentUserId)
        {
            var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(p => p.InventoryId == inventoryId);

            if (inventoryObj == null)
            {
                return InventoryErrorCode.InventoryNotFound;
            }
            if (inventoryObj.InventoryTypeId == (int)EnumInventoryType.Input)
            {
                return GeneralCode.InvalidParams;
            }
            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(inventoryObj.StockId)))
            {
                //reload from db after lock
                inventoryObj = _stockDbContext.Inventory.FirstOrDefault(p => p.InventoryId == inventoryId);

                // Xử lý xoá thông tin phiếu xuất kho
                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        //Cần rollback cả 2 loại phiếu đã duyệt và chưa duyệt All approved or not need tobe rollback, bỏ if (inventoryObj.IsApproved)
                        var processResult = await RollbackInventoryOutput(inventoryObj);
                        if (!processResult.IsSuccess())
                        {
                            trans.Rollback();
                            return GeneralCode.InvalidParams;
                        }

                        //update status after rollback
                        inventoryObj.IsDeleted = true;
                        //inventoryObj.IsApproved = false;
                        inventoryObj.UpdatedByUserId = currentUserId;
                        inventoryObj.UpdatedDatetimeUtc = DateTime.UtcNow;

                        await _stockDbContext.SaveChangesAsync();

                        await _activityLogService.CreateLog(EnumObjectType.Inventory, inventoryObj.InventoryId, string.Format("Xóa phiếu xuất kho, mã phiếu {0}", inventoryObj.InventoryCode), new { InventoryId = inventoryId }.JsonSerialize());

                        trans.Commit();

                        return GeneralCode.Success;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        _stockDbContext.RollbackEntities();

                        _logger.LogError(ex, "DeleteInventoryOutput");
                        return GeneralCode.InternalError;
                    }
                }
            }
        }


        /// <summary>
        /// Duyệt phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Enum> ApproveInventoryInput(long inventoryId, int currentUserId)
        {
            if (inventoryId < 0)
            {
                return InventoryErrorCode.InventoryNotFound;
            }

            var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);
            if (inventoryObj == null)
            {
                return InventoryErrorCode.InventoryNotFound;
            }
            if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Input)
            {

                return InventoryErrorCode.InventoryNotFound;
            }

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
                            return InventoryErrorCode.InventoryAlreadyApproved;
                        }

                        inventoryObj.IsApproved = true;
                        inventoryObj.UpdatedByUserId = currentUserId;
                        inventoryObj.UpdatedDatetimeUtc = DateTime.UtcNow;

                        await _stockDbContext.SaveChangesAsync();

                        var inventoryDetails = _stockDbContext.InventoryDetail.Where(q => q.InventoryId == inventoryId).ToList();

                        var r = await ProcessInventoryInputApprove(inventoryObj.StockId, inventoryObj.Date, inventoryDetails);
                        if (!r.IsSuccess())
                        {
                            trans.Rollback();
                            return r;
                        }

                        await ReCalculateRemainingAfterUpdate(inventoryId);

                        trans.Commit();

                        var messageLog = $"Duyệt phiếu nhập kho, mã: {inventoryObj.InventoryCode}";
                        await _activityLogService.CreateLog(EnumObjectType.Inventory, inventoryObj.InventoryId, messageLog, new { InventoryId = inventoryId }.JsonSerialize());

                        return GeneralCode.Success;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        _logger.LogError(ex, "ApproveInventoryInput");
                        return GeneralCode.InternalError;
                    }
                }
            }
        }


        private async Task<Enum> ProcessInventoryInputApprove(int stockId, DateTime date, IList<InventoryDetail> inventoryDetails)
        {
            var inputTransfer = new List<InventoryDetailToPackage>();
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

                            var createNewPackageResult = await CreateNewPackage(stockId, date, item);
                            if (!createNewPackageResult.Code.IsSuccess())
                            {
                                return createNewPackageResult.Code;
                            }

                            item.ToPackageId = createNewPackageResult.Data.PackageId;
                            break;
                        default:
                            return GeneralCode.NotYetSupported;
                    }
                else
                {
                    var createNewPackageResult = await CreateNewPackage(stockId, date, item);
                    if (!createNewPackageResult.Code.IsSuccess())
                    {
                        return createNewPackageResult.Code;
                    }

                    item.ToPackageId = createNewPackageResult.Data.PackageId;
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


        /// <summary>
        /// Tính toán lại vết khi update phiếu nhập/xuất
        /// </summary>
        /// <param name="inventoryId"></param>
        private async Task ReCalculateRemainingAfterUpdate(long inventoryId)
        {
            var inventoryTrackingFacade = await InventoryTrackingFacadeFactory.Create(_stockDbContext, inventoryId);
            await inventoryTrackingFacade.Execute();

        }

        /// <summary>
        /// Duyệt phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ServiceResult> ApproveInventoryOutput(long inventoryId, int currentUserId)
        {
            if (inventoryId <= 0)
            {
                return InventoryErrorCode.InventoryNotFound;
            }

            var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);
            if (inventoryObj == null)
            {
                return InventoryErrorCode.InventoryNotFound;
            }
            if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Output)
            {
                return InventoryErrorCode.InventoryNotFound;
            }


            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(inventoryObj.StockId)))
            {
                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);

                        if (inventoryObj.IsApproved)
                        {
                            trans.Rollback();
                            return InventoryErrorCode.InventoryAlreadyApproved;
                        }

                        inventoryObj.IsApproved = true;
                        inventoryObj.UpdatedByUserId = currentUserId;
                        inventoryObj.UpdatedDatetimeUtc = DateTime.UtcNow;

                        var inventoryDetails = _stockDbContext.InventoryDetail.Where(d => d.InventoryId == inventoryId).ToList();

                        var fromPackageIds = inventoryDetails.Select(f => f.FromPackageId).ToList();
                        var fromPackages = _stockDbContext.Package.Where(p => fromPackageIds.Contains(p.PackageId)).ToList();

                        var original = fromPackages.ToDictionary(p => p.PackageId, p => p.PrimaryQuantityRemaining);

                        var groupByProducts = inventoryDetails
                            .GroupBy(g => new { g.ProductId, g.ProductUnitConversionId })
                            .Select(g => new
                            {
                                g.Key.ProductId,
                                g.Key.ProductUnitConversionId,
                                OutPrimary = g.Sum(d => d.PrimaryQuantity),
                                OutSecondary = g.Sum(d => d.ProductUnitConversionQuantity)
                            });
                        foreach (var product in groupByProducts)
                        {
                            var validate = await ValidateBalanceForOutput(inventoryObj.StockId, product.ProductId, inventoryObj.InventoryId, product.ProductUnitConversionId.Value, inventoryObj.Date, product.OutPrimary, product.OutSecondary);

                            if (!validate.IsSuccessCode())
                            {
                                trans.Rollback();

                                return validate;
                            }
                        }

                        foreach (var detail in inventoryDetails)
                        {
                            var fromPackageInfo = fromPackages.FirstOrDefault(p => p.PackageId == detail.FromPackageId);
                            if (fromPackageInfo == null) return PackageErrorCode.PackageNotFound;

                            fromPackageInfo.PrimaryQuantityWaiting -= detail.PrimaryQuantity;
                            fromPackageInfo.PrimaryQuantityRemaining -= detail.PrimaryQuantity;
                            fromPackageInfo.ProductUnitConversionWaitting -= detail.ProductUnitConversionQuantity;
                            fromPackageInfo.ProductUnitConversionRemaining -= detail.ProductUnitConversionQuantity;

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
                                    return ProductErrorCode.ProductNotFound;
                                }


                                var message = $"Số dư trong kho mặt hàng {productInfo.ProductCode} ({original[detail.FromPackageId.Value].Format()} {productInfo.ProductUnitConversionName}) không đủ để xuất ";
                                var samPackages = inventoryDetails.Where(d => d.FromPackageId == detail.FromPackageId);

                                var total = samPackages.Sum(d => d.PrimaryQuantity).Format();

                                message += $" < {total} {productInfo.ProductUnitConversionName} = "
                                    + string.Join(" + ", samPackages.Select(d => d.PrimaryQuantity.Format()));

                                trans.Rollback();
                                return (InventoryErrorCode.NotEnoughQuantity, message);
                            }

                            ValidatePackage(fromPackageInfo);

                            var stockProduct = await EnsureStockProduct(inventoryObj.StockId, detail.ProductId, detail.ProductUnitConversionId);

                            stockProduct.PrimaryQuantityWaiting -= detail.PrimaryQuantity;
                            stockProduct.PrimaryQuantityRemaining -= detail.PrimaryQuantity;
                            stockProduct.ProductUnitConversionWaitting -= detail.ProductUnitConversionQuantity;
                            stockProduct.ProductUnitConversionRemaining -= detail.ProductUnitConversionQuantity;

                            ValidateStockProduct(stockProduct);
                        }

                        await _stockDbContext.SaveChangesAsync();

                        await ReCalculateRemainingAfterUpdate(inventoryId);

                        trans.Commit();

                        var messageLog = $"Duyệt phiếu xuất kho, mã: {inventoryObj.InventoryCode}";
                        await _activityLogService.CreateLog(EnumObjectType.Inventory, inventoryObj.InventoryId, messageLog, new { InventoryId = inventoryId }.JsonSerialize());

                        return GeneralCode.Success;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        _logger.LogError(ex, "ApproveInventoryOutput");
                        return GeneralCode.InternalError;
                    }
                }
            }
        }

        private void ValidatePackage(Package package)
        {

            if (package.PrimaryQuantityWaiting < 0) throw new Exception("Negative PrimaryQuantityWaiting!");

            if (package.PrimaryQuantityRemaining < 0) throw new Exception("Negative PrimaryQuantityRemaining!");

            if (package.ProductUnitConversionWaitting < 0) throw new Exception("Negative ProductUnitConversionWaitting!");

            if (package.ProductUnitConversionRemaining < 0) throw new Exception("Negative ProductUnitConversionRemaining!");


        }

        private void ValidateStockProduct(StockProduct stockProduct)
        {

            if (stockProduct.PrimaryQuantityWaiting < 0) throw new Exception("Negative PrimaryQuantityWaiting!");

            if (stockProduct.PrimaryQuantityRemaining < 0) throw new Exception("Negative PrimaryQuantityRemaining!");

            if (stockProduct.ProductUnitConversionWaitting < 0) throw new Exception("Negative ProductUnitConversionWaitting!");

            if (stockProduct.ProductUnitConversionRemaining < 0) throw new Exception("Negative ProductUnitConversionRemaining!");

        }


        /// <summary>
        /// Lấy danh sách sản phẩm để xuất kho
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="stockIdList"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public async Task<PageData<ProductListOutput>> GetProductListForExport(string keyword, IList<int> stockIdList, int page = 1, int size = 20)
        {
            var products = _stockDbContext.Product.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
                products = products.Where(q => q.ProductName.Contains(keyword) || q.ProductCode.Contains(keyword));
            var productInStockQuery = (
                from s in _stockDbContext.StockProduct
                join p in products on s.ProductId equals p.ProductId
                where stockIdList.Contains(s.StockId) // && s.PrimaryQuantityRemaining > 0
                select p
                ).Distinct();

            var total = productInStockQuery.Count();
            var pagedData = productInStockQuery.AsNoTracking().OrderBy(p => p.ProductCode).Skip((page - 1) * size).Take(size).ToList();

            var productIdList = pagedData.Select(q => q.ProductId).ToList();
            var productExtraData = _stockDbContext.ProductExtraInfo.AsNoTracking()
                .Where(q => productIdList.Contains(q.ProductId))
                .ToList()
                .ToDictionary(e => e.ProductId, e => e);

            var unitIdList = pagedData.Select(q => q.UnitId).Distinct().ToList();
            var unitOutputList = (await _unitService.GetListByIds(unitIdList)).ToDictionary(u => u.UnitId, u => u);

            var stockProductData = _stockDbContext.StockProduct.AsNoTracking().Where(q => stockIdList.Contains(q.StockId)).Where(q => productIdList.Contains(q.ProductId)).ToList();

            var productList = new List<ProductListOutput>(total);
            foreach (var item in pagedData)
            {
                if (!unitOutputList.TryGetValue(item.UnitId, out var unitInfo))
                {
                    throw new Exception($"Unit {item.UnitId} not found");
                }

                productList.Add(new ProductListOutput
                {
                    ProductId = item.ProductId,
                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    MainImageFileId = item.MainImageFileId,
                    ProductTypeId = item.ProductTypeId,
                    ProductTypeName = string.Empty,
                    ProductCateId = item.ProductCateId,
                    ProductCateName = string.Empty,
                    Barcode = item.Barcode,
                    Specification = productExtraData[item.ProductId].Specification,
                    UnitId = item.UnitId,
                    UnitName = unitInfo.UnitName,
                    EstimatePrice = item.EstimatePrice ?? 0,
                    StockProductModelList = stockProductData.Where(q => q.ProductId == item.ProductId).Select(q => new StockProductOutput
                    {
                        StockId = q.StockId,
                        ProductId = q.ProductId,
                        PrimaryUnitId = item.UnitId,
                        PrimaryQuantityRemaining = q.PrimaryQuantityRemaining,
                        ProductUnitConversionId = q.ProductUnitConversionId,
                        ProductUnitConversionRemaining = q.ProductUnitConversionRemaining
                    }).ToList()
                });
            }
            return (productList, total);
        }

        /// <summary>
        /// Lấy danh sách kiện để xuất kho
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="stockIdList"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public async Task<PageData<PackageOutputModel>> GetPackageListForExport(int productId, IList<int> stockIdList, int page = 1, int size = 20)
        {

            var query = from pk in _stockDbContext.Package
                        join p in _stockDbContext.Product on pk.ProductId equals p.ProductId
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
                            p.UnitId,
                            pk.PrimaryQuantityRemaining,
                            pk.PrimaryQuantityWaiting,
                            pk.ProductUnitConversionRemaining,
                            pk.ProductUnitConversionWaitting,
                            pk.CreatedDatetimeUtc,
                            pk.UpdatedDatetimeUtc
                        };

            var total = query.Count();
            var packageData = query.AsNoTracking().Skip((page - 1) * size).Take(size).ToList();
            var locationIdList = packageData.Select(q => q.LocationId).ToList();
            var productUnitConversionIdList = packageData.Select(q => q.ProductUnitConversionId).ToList();
            var locationData = await _stockDbContext.Location.AsNoTracking().Where(q => locationIdList.Contains(q.LocationId)).ToListAsync();
            var productUnitConversionData = _stockDbContext.ProductUnitConversion.Where(q => productUnitConversionIdList.Contains(q.ProductUnitConversionId)).AsNoTracking().ToList();

            var packageList = new List<PackageOutputModel>(total);
            foreach (var item in packageData)
            {
                var locationObj = item.LocationId > 0 ? locationData.FirstOrDefault(q => q.LocationId == item.LocationId) : null;
                var locationOutputModel = locationObj != null ? new VErp.Services.Stock.Model.Location.LocationOutput
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
                    PrimaryUnitId = item.UnitId,
                    ProductUnitConversionId = item.ProductUnitConversionId,
                    PrimaryQuantityWaiting = item.PrimaryQuantityWaiting,
                    PrimaryQuantityRemaining = item.PrimaryQuantityRemaining,
                    ProductUnitConversionWaitting = item.ProductUnitConversionWaitting,
                    ProductUnitConversionRemaining = item.ProductUnitConversionRemaining,

                    CreatedDatetimeUtc = item.CreatedDatetimeUtc != null ? ((DateTime)item.CreatedDatetimeUtc).GetUnix() : 0,
                    UpdatedDatetimeUtc = item.UpdatedDatetimeUtc != null ? ((DateTime)item.UpdatedDatetimeUtc).GetUnix() : 0,
                    LocationOutputModel = locationOutputModel,
                    ProductUnitConversionModel = productUnitConversionData.FirstOrDefault(q => q.ProductUnitConversionId == item.ProductUnitConversionId) ?? null
                });
            }
            return (packageList, total);

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
            var productDatas = _stockDbContext.Product.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                productDatas = productDatas.Where(q => q.ProductName.Contains(keyword) || q.ProductCode.Contains(keyword));
            }

            var query = (
                from p in productDatas
                join pv in _stockDbContext.ProductStockValidation on p.ProductId equals pv.ProductId into pvs
                from pv in pvs.DefaultIfEmpty()
                where pv == null || stockIdList.Contains(pv.StockId)
                select new
                {
                    p.ProductId,
                    p.ProductCode,
                    p.ProductName,
                    p.MainImageFileId,
                    p.ProductTypeId,
                    p.ProductCateId,
                    p.UnitId,
                    p.Barcode,
                    p.EstimatePrice
                })
                .Distinct();

            var total = query.Count();

            var pagedData = query.OrderBy(q => q.ProductCode).Skip((page - 1) * size).Take(size).ToList();
            var productIdList = pagedData.Select(q => q.ProductId).ToList();
            var productExtraData = _stockDbContext.ProductExtraInfo.AsNoTracking().Where(q => productIdList.Contains(q.ProductId)).ToList();
            var unitIdList = pagedData.Select(q => q.UnitId).Distinct().ToList();
            var unitOutputList = await _unitService.GetListByIds(unitIdList);

            var stockProductData = _stockDbContext.StockProduct.AsNoTracking().Where(q => stockIdList.Contains(q.StockId)).Where(q => productIdList.Contains(q.ProductId)).ToList();

            var productList = new List<ProductListOutput>(total);
            foreach (var item in pagedData)
            {
                var specification = productExtraData.FirstOrDefault(q => q.ProductId == item.ProductId)?.Specification ?? string.Empty;
                var unitName = unitOutputList.FirstOrDefault(q => q.UnitId == item.UnitId)?.UnitName ?? string.Empty;

                productList.Add(new ProductListOutput
                {
                    ProductId = item.ProductId,
                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    MainImageFileId = item.MainImageFileId,
                    ProductTypeId = item.ProductTypeId,
                    ProductTypeName = string.Empty,
                    ProductCateId = item.ProductCateId,
                    ProductCateName = string.Empty,
                    Barcode = item.Barcode,
                    Specification = specification,
                    UnitId = item.UnitId,
                    UnitName = unitName,
                    EstimatePrice = item.EstimatePrice ?? 0,
                    StockProductModelList = stockProductData.Where(q => q.ProductId == item.ProductId).Select(q => new StockProductOutput
                    {
                        StockId = q.StockId,
                        ProductId = q.ProductId,
                        PrimaryUnitId = item.UnitId,
                        PrimaryQuantityRemaining = q.PrimaryQuantityRemaining,
                        ProductUnitConversionId = q.ProductUnitConversionId,
                        ProductUnitConversionRemaining = q.ProductUnitConversionRemaining
                    }).ToList()
                });
            }
            return (productList, total);

        }

        #region Private helper method

        private async Task<ServiceResult<IList<InventoryDetail>>> ValidateInventoryIn(bool isApproved, InventoryInModel req)
        {
            if (req.InProducts == null)
                req.InProducts = new List<InventoryInProductModel>();
            var productIds = req.InProducts.Select(p => p.ProductId).Distinct().ToList();

            var productInfos = await _stockDbContext.Product.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToListAsync();
            var productUnitConversions = await _stockDbContext.ProductUnitConversion.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToListAsync();

            var toPackageIds = req.InProducts.Select(p => p.ToPackageId).ToList();
            var toPackages = await _stockDbContext.Package.Where(p => toPackageIds.Contains(p.PackageId) && p.PackageTypeId == (int)EnumPackageType.Custom).ToListAsync();

            var inventoryDetailList = new List<InventoryDetail>(req.InProducts.Count);
            foreach (var details in req.InProducts)
            {
                var productInfo = productInfos.FirstOrDefault(p => p.ProductId == details.ProductId);

                if (details.ProductUnitConversionQuantity == 0)
                    details.ProductUnitConversionQuantity = details.PrimaryQuantity;

                if (productInfo == null)
                {
                    return ProductErrorCode.ProductNotFound;
                }
                if (!isApproved)
                {
                    if (details.ProductUnitConversionQuantity <= 0 || details.PrimaryQuantity <= 0)
                    {
                        return GeneralCode.InvalidParams;
                    }
                }


                if (details.ProductUnitConversionId != null && details.ProductUnitConversionId > 0)
                {
                    var productUnitConversionInfo = productUnitConversions.FirstOrDefault(c => c.ProductUnitConversionId == details.ProductUnitConversionId);
                    if (productUnitConversionInfo == null)
                    {
                        return ProductUnitConversionErrorCode.ProductUnitConversionNotFound;
                    }
                    if (productUnitConversionInfo.ProductId != details.ProductId)
                    {
                        return ProductUnitConversionErrorCode.ProductUnitConversionNotBelongToProduct;
                    }

                    if (productUnitConversionInfo.IsFreeStyle ?? false == false)
                    {
                        // primaryQty = Utils.GetPrimaryQuantityFromProductUnitConversionQuantity(details.ProductUnitConversionQuantity, productUnitConversionInfo.FactorExpression);
                        //details.ProductUnitConversionQuantity = Utils.GetProductUnitConversionQuantityFromPrimaryQuantity(details.PrimaryQuantity, productUnitConversionInfo.FactorExpression);

                        var (isSuccess, pucQuantity) = Utils.GetProductUnitConversionQuantityFromPrimaryQuantity(details.PrimaryQuantity, productUnitConversionInfo.FactorExpression, details.ProductUnitConversionQuantity);
                        if (isSuccess)
                        {
                            details.ProductUnitConversionQuantity = pucQuantity;
                        }
                        else
                        {
                            _logger.LogWarning($"Wrong pucQuantity input data: PrimaryQuantity={details.PrimaryQuantity}, FactorExpression={productUnitConversionInfo.FactorExpression}, ProductUnitConversionQuantity={details.ProductUnitConversionQuantity}, evalData={pucQuantity}");
                            return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                        }
                    }

                    if (!isApproved && details.ProductUnitConversionQuantity <= 0)
                    {
                        return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
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
                    PrimaryQuantity = details.PrimaryQuantity,
                    UnitPrice = details.UnitPrice,
                    ProductUnitConversionQuantity = details.ProductUnitConversionQuantity,
                    ProductUnitConversionId = details.ProductUnitConversionId,
                    RefObjectTypeId = details.RefObjectTypeId,
                    RefObjectId = details.RefObjectId,
                    RefObjectCode = details.RefObjectCode,
                    OrderCode = details.OrderCode,
                    Pocode = details.POCode,
                    ProductionOrderCode = details.ProductionOrderCode,
                    FromPackageId = null,
                    ToPackageId = details.ToPackageId,
                    PackageOptionId = (int)details.PackageOptionId,
                    SortOrder = details.SortOrder
                });
            }
            return inventoryDetailList;
        }

        private async Task<ServiceResult<IList<InventoryDetail>>> ProcessInventoryOut(Inventory inventory, InventoryOutModel req)
        {
            var productIds = req.OutProducts.Select(p => p.ProductId).Distinct().ToList();

            var productInfos = await _stockDbContext.Product.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToListAsync();
            var productUnitConversions = await _stockDbContext.ProductUnitConversion.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToListAsync();

            var fromPackageIds = req.OutProducts.Select(p => p.FromPackageId).ToList();
            var fromPackages = await _stockDbContext.Package.Where(p => fromPackageIds.Contains(p.PackageId)).ToListAsync();


            var inventoryDetailList = new List<InventoryDetail>();

            foreach (var details in req.OutProducts)
            {
                var fromPackageInfo = fromPackages.FirstOrDefault(p => p.PackageId == details.FromPackageId);
                if (fromPackageInfo == null) return PackageErrorCode.PackageNotFound;

                if (fromPackageInfo.ProductId != details.ProductId
                    || fromPackageInfo.ProductUnitConversionId != details.ProductUnitConversionId
                    || fromPackageInfo.StockId != req.StockId)
                {
                    _logger.LogInformation($"InventoryService.ProcessInventoryOut error InvalidPackage. ProductId: {details.ProductId} , FromPackageId: {details.FromPackageId}, ProductUnitConversionId: {details.ProductUnitConversionId}");
                    return InventoryErrorCode.InvalidPackage;
                }

                var productInfo = productInfos.FirstOrDefault(p => p.ProductId == details.ProductId);
                if (productInfo == null)
                {
                    return ProductErrorCode.ProductNotFound;
                }

                var primaryQualtity = details.PrimaryQuantity;


                var productUnitConversionInfo = productUnitConversions.FirstOrDefault(c => c.ProductUnitConversionId == details.ProductUnitConversionId);
                if (productUnitConversionInfo == null)
                {
                    _logger.LogInformation($"InventoryService.ProcessInventoryOut error ProductUnitConversionNotFound. ProductId: {details.ProductId} , FromPackageId: {details.FromPackageId}, ProductUnitConversionId: {details.ProductUnitConversionId}");
                    return ProductUnitConversionErrorCode.ProductUnitConversionNotFound;
                }

                if (productUnitConversionInfo.ProductId != details.ProductId)
                {
                    return ProductUnitConversionErrorCode.ProductUnitConversionNotBelongToProduct;
                }

                if (fromPackageInfo.PrimaryQuantityRemaining == 0 || fromPackageInfo.ProductUnitConversionRemaining == 0)
                {
                    _logger.LogInformation($"InventoryService.ProcessInventoryOut error NotEnoughQuantity. ProductId: {details.ProductId} , packageId: {fromPackageInfo.PackageId} PrimaryQuantityRemaining: {fromPackageInfo.PrimaryQuantityRemaining}, ProductUnitConversionRemaining: {fromPackageInfo.ProductUnitConversionRemaining}");
                    return InventoryErrorCode.NotEnoughQuantity;
                }

                //if (details.ProductUnitConversionQuantity <= 0 && primaryQualtity > 0)
                //{
                if (productUnitConversionInfo.IsFreeStyle ?? false == false)
                {
                    var (isSuccess, pucQuantity) = Utils.GetProductUnitConversionQuantityFromPrimaryQuantity(details.PrimaryQuantity, fromPackageInfo.ProductUnitConversionRemaining / fromPackageInfo.PrimaryQuantityRemaining, details.ProductUnitConversionQuantity);
                    if (isSuccess)
                    {
                        details.ProductUnitConversionQuantity = pucQuantity;
                    }
                    else
                    {
                        _logger.LogWarning($"Wrong pucQuantity input data: PrimaryQuantity={details.PrimaryQuantity}, FactorExpression={fromPackageInfo.ProductUnitConversionRemaining / fromPackageInfo.PrimaryQuantityRemaining}, ProductUnitConversionQuantity={details.ProductUnitConversionQuantity}, evalData={pucQuantity}");
                        return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                    }
                }
                //Utils.GetProductUnitConversionQuantityFromPrimaryQuantity(primaryQualtity, productUnitConversionInfo.FactorExpression);

                if (!(details.ProductUnitConversionQuantity > 0))
                {
                    _logger.LogInformation($"InventoryService.ProcessInventoryOut error PrimaryUnitConversionError. ProductId: {details.ProductId} , FromPackageId: {details.FromPackageId}, ProductUnitConversionId: {details.ProductUnitConversionId}, FactorExpression: {productUnitConversionInfo.FactorExpression}");
                    return ProductUnitConversionErrorCode.PrimaryUnitConversionError;
                }
                //return GeneralCode.InvalidParams;
                //}


                //if (primaryQualtity <= 0 && details.ProductUnitConversionQuantity > 0)
                //{
                //    if (productUnitConversionInfo.IsFreeStyle ?? false == false)
                //    {
                //        //   primaryQualtity = details.ProductUnitConversionQuantity * fromPackageInfo.PrimaryQuantityRemaining / fromPackageInfo.ProductUnitConversionRemaining;
                //        var (isSuccess, priQuantity) = Utils.GetPrimaryQuantityFromProductUnitConversionQuantity(details.ProductUnitConversionQuantity, fromPackageInfo.ProductUnitConversionRemaining / fromPackageInfo.PrimaryQuantityRemaining, primaryQualtity);
                //        if (isSuccess)
                //        {
                //            primaryQualtity = priQuantity;
                //        }
                //        else
                //        {
                //            _logger.LogWarning($"Wrong priQuantity input data: PrimaryQuantity={details.PrimaryQuantity}, FactorExpression={fromPackageInfo.ProductUnitConversionRemaining / fromPackageInfo.PrimaryQuantityRemaining}, ProductUnitConversionQuantity={details.ProductUnitConversionQuantity}, evalData={priQuantity}");
                //            return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                //        }

                //    }


                //    if (!(primaryQualtity > 0))
                //    {
                //        return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                //    }
                //}


                if (Math.Abs(details.ProductUnitConversionQuantity - fromPackageInfo.ProductUnitConversionRemaining) <= MINIMUM_JS_NUMBER)
                {
                    details.ProductUnitConversionQuantity = fromPackageInfo.ProductUnitConversionRemaining;
                }

                if (Math.Abs(primaryQualtity - fromPackageInfo.PrimaryQuantityRemaining) <= MINIMUM_JS_NUMBER)
                {
                    primaryQualtity = fromPackageInfo.PrimaryQuantityRemaining;

                }

                if (primaryQualtity > fromPackageInfo.PrimaryQuantityRemaining)
                {
                    _logger.LogInformation($"InventoryService.ProcessInventoryOut error NotEnoughQuantity. ProductId: {details.ProductId} , ProductUnitConversionQuantity: {details.ProductUnitConversionQuantity}, ProductUnitConversionRemaining: {fromPackageInfo.ProductUnitConversionRemaining}");
                    return InventoryErrorCode.NotEnoughQuantity;
                }

                //// Check số lượng sản phẩm theo đơn vị chính nếu <= 0 báo lỗi
                //if (primaryQualtity <= 0)
                //{
                //    _logger.LogInformation($"InventoryService.ProcessInventoryOut error PrimaryUnitConversionError. ProductId: {details.ProductId} , FromPackageId: {details.FromPackageId}, ProductUnitConversionId: {details.ProductUnitConversionId}, FactorExpression: {productUnitConversionInfo.FactorExpression}");
                //    return ProductUnitConversionErrorCode.PrimaryUnitConversionError;
                //}
                //// Tính số lượng sản phẩm theo đơn vị chuyển đổi từ đơn vị chính
                //var secondaryQualtity = (primaryQualtity * fromPackageInfo.ProductUnitConversionRemaining) / fromPackageInfo.PrimaryQuantityRemaining;
                //// Check số lượng sản phẩm theo đơn vị chuyển đổi có khớp không
                //if (secondaryQualtity != details.ProductUnitConversionQuantity)
                //{
                //    return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                //}

                //// Check số lượng sản phẩm tồn có đủ hay không
                //if (primaryQualtity > fromPackageInfo.PrimaryQuantityRemaining)
                //{
                //    _logger.LogInformation($"InventoryService.ProcessInventoryOut error NotEnoughQuantity. ProductId: {details.ProductId} , ProductUnitConversionQuantity: {details.ProductUnitConversionQuantity}, ProductUnitConversionRemaining: {fromPackageInfo.ProductUnitConversionRemaining}");
                //    return InventoryErrorCode.NotEnoughQuantity;
                //}


                inventoryDetailList.Add(new InventoryDetail
                {
                    InventoryId = inventory.InventoryId,
                    ProductId = details.ProductId,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    PrimaryQuantity = primaryQualtity,
                    UnitPrice = details.UnitPrice,
                    ProductUnitConversionQuantity = details.ProductUnitConversionQuantity,
                    ProductUnitConversionId = details.ProductUnitConversionId,
                    RefObjectTypeId = details.RefObjectTypeId,
                    RefObjectId = details.RefObjectId,
                    RefObjectCode = details.RefObjectCode,
                    OrderCode = details.OrderCode,
                    Pocode = details.POCode,
                    ProductionOrderCode = details.ProductionOrderCode,
                    FromPackageId = details.FromPackageId,
                    ToPackageId = null,
                    PackageOptionId = null,
                    SortOrder = details.SortOrder
                });

                fromPackageInfo.PrimaryQuantityWaiting += primaryQualtity;
                fromPackageInfo.ProductUnitConversionWaitting += details.ProductUnitConversionQuantity;

                var stockProductInfo = await EnsureStockProduct(inventory.StockId, fromPackageInfo.ProductId, fromPackageInfo.ProductUnitConversionId);

                stockProductInfo.PrimaryQuantityWaiting += primaryQualtity;
                stockProductInfo.ProductUnitConversionWaitting += details.ProductUnitConversionQuantity;
            }
            return inventoryDetailList;
        }

        private async Task<StockProduct> EnsureStockProduct(int stockId, int productId, int? productUnitConversionId)
        {
            var stockProductInfo = await _stockDbContext.StockProduct
                                .FirstOrDefaultAsync(s =>
                                                s.StockId == stockId
                                                && s.ProductId == productId
                                                && s.ProductUnitConversionId == productUnitConversionId
                                                );

            if (stockProductInfo == null)
            {
                stockProductInfo = new Infrastructure.EF.StockDB.StockProduct()
                {
                    StockId = stockId,
                    ProductId = productId,
                    ProductUnitConversionId = productUnitConversionId,
                    PrimaryQuantityWaiting = 0,
                    PrimaryQuantityRemaining = 0,
                    ProductUnitConversionWaitting = 0,
                    ProductUnitConversionRemaining = 0,
                    UpdatedDatetimeUtc = DateTime.UtcNow
                };
                await _stockDbContext.StockProduct.AddAsync(stockProductInfo);
                await _stockDbContext.SaveChangesAsync();
            }
            return stockProductInfo;
        }

        private async Task UpdateStockProduct(int stockId, InventoryDetail detail, EnumInventoryType type = EnumInventoryType.Input)
        {
            var stockProductInfo = await EnsureStockProduct(stockId, detail.ProductId, detail.ProductUnitConversionId);
            switch (type)
            {
                case EnumInventoryType.Input:
                    {
                        stockProductInfo.PrimaryQuantityRemaining += detail.PrimaryQuantity;
                        stockProductInfo.ProductUnitConversionRemaining += detail.ProductUnitConversionQuantity;
                        break;
                    }
                case EnumInventoryType.Output:
                    {
                        stockProductInfo.PrimaryQuantityRemaining -= detail.PrimaryQuantity;
                        stockProductInfo.ProductUnitConversionRemaining -= detail.ProductUnitConversionQuantity;
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
            packageInfo.PrimaryQuantityRemaining += detail.PrimaryQuantity;
            //packageInfo.ProductUnitConversionQuantity += detail.ProductUnitConversionQuantity;
            packageInfo.ProductUnitConversionRemaining += detail.ProductUnitConversionQuantity;
            packageInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
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
                ensureDefaultPackage = new Infrastructure.EF.StockDB.Package()
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
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _stockDbContext.Package.AddAsync(ensureDefaultPackage);
            }

            //ensureDefaultPackage.PrimaryQuantity += detail.PrimaryQuantity;
            ensureDefaultPackage.PrimaryQuantityRemaining += detail.PrimaryQuantity;
            //ensureDefaultPackage.ProductUnitConversionQuantity += detail.ProductUnitConversionQuantity;
            ensureDefaultPackage.ProductUnitConversionRemaining += detail.ProductUnitConversionQuantity;

            await _stockDbContext.SaveChangesAsync();

            return ensureDefaultPackage;
        }

        private async Task<ServiceResult<PackageEntity>> CreateNewPackage(int stockId, DateTime date, InventoryDetail detail)
        {
            var newPackageCodeResult = await _objectGenCodeService.GenerateCode(EnumObjectType.Package);
            if (!newPackageCodeResult.Code.IsSuccess())
            {
                return newPackageCodeResult.Code;
            }
            var newPackage = new Infrastructure.EF.StockDB.Package()
            {
                PackageTypeId = (int)EnumPackageType.Custom,
                PackageCode = newPackageCodeResult.Data,
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
                CreatedDatetimeUtc = DateTime.UtcNow,
                UpdatedDatetimeUtc = DateTime.UtcNow,
                IsDeleted = false
            };
            await _stockDbContext.Package.AddAsync(newPackage);
            await _stockDbContext.SaveChangesAsync();
            return newPackage;
        }

        private async Task<Enum> RollbackInventoryOutput(Inventory inventory)
        {
            var inventoryDetails = await _stockDbContext.InventoryDetail.Where(d => d.InventoryId == inventory.InventoryId).ToListAsync();

            var fromPackageIds = inventoryDetails.Select(d => d.FromPackageId).ToList();

            var fromPackages = await _stockDbContext.Package.Where(p => fromPackageIds.Contains(p.PackageId)).ToListAsync();

            foreach (var detail in inventoryDetails)
            {
                var fromPackageInfo = fromPackages.FirstOrDefault(f => f.PackageId == detail.FromPackageId);
                if (fromPackageInfo == null) return PackageErrorCode.PackageNotFound;

                var stockProductInfo = await EnsureStockProduct(inventory.StockId, detail.ProductId, detail.ProductUnitConversionId);

                if (!inventory.IsApproved)
                {
                    fromPackageInfo.PrimaryQuantityWaiting -= detail.PrimaryQuantity;
                    fromPackageInfo.ProductUnitConversionWaitting -= detail.ProductUnitConversionQuantity;

                    stockProductInfo.PrimaryQuantityWaiting -= detail.PrimaryQuantity;
                    stockProductInfo.ProductUnitConversionWaitting -= detail.ProductUnitConversionQuantity;
                }
                else
                {
                    fromPackageInfo.PrimaryQuantityRemaining += detail.PrimaryQuantity;
                    fromPackageInfo.ProductUnitConversionRemaining += detail.ProductUnitConversionQuantity;

                    stockProductInfo.PrimaryQuantityRemaining += detail.PrimaryQuantity;
                    stockProductInfo.ProductUnitConversionRemaining += detail.ProductUnitConversionQuantity;
                }

                ValidatePackage(fromPackageInfo);
                ValidateStockProduct(stockProductInfo);

                fromPackageInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
                stockProductInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                detail.IsDeleted = true;
                detail.UpdatedDatetimeUtc = DateTime.UtcNow;
            }

            await _stockDbContext.SaveChangesAsync();

            if (inventory.IsApproved)
            {
                await ReCalculateRemainingAfterUpdate(inventory.InventoryId);
            }

            return GeneralCode.Success;
        }

        private async Task<ServiceResult> ValidateBalanceForOutput(int stockId, int productId, long currentInventoryId, int productUnitConversionId, DateTime endDate, decimal outPrimary, decimal outSecondary)
        {
            var sums = await (
                from id in _stockDbContext.InventoryDetail
                join iv in _stockDbContext.Inventory on id.InventoryId equals iv.InventoryId
                where iv.StockId == stockId
                && id.ProductId == productId
                && id.ProductUnitConversionId == productUnitConversionId
                && iv.Date <= endDate
                && iv.IsApproved
                && iv.InventoryId != currentInventoryId
                select new
                {
                    iv.InventoryTypeId,
                    id.PrimaryQuantity,
                    id.ProductUnitConversionQuantity
                }).GroupBy(g => true)
                   .Select(g => new
                   {
                       TotalPrimary = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity),
                       TotalSecondary = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.ProductUnitConversionQuantity : -d.ProductUnitConversionQuantity)
                   }).FirstAsync();


            if (sums.TotalPrimary.SubDecimal(outPrimary) < 0 || sums.TotalSecondary.SubDecimal(outSecondary) < 0)
            {
                var productCode = await _stockDbContext
                                    .Product
                                    .Where(p => p.ProductId == productId)
                                    .Select(p => p.ProductCode)
                                    .FirstOrDefaultAsync();

                var total = sums.TotalSecondary;
                var output = outSecondary;

                if (sums.TotalPrimary - outPrimary < MINIMUM_JS_NUMBER)
                {
                    total = sums.TotalPrimary;
                    output = outPrimary;
                }

                var message = $"Số lượng \"{productCode}\" trong kho tại thời điểm {endDate.ToString("dd-MM-yyyy")} là " +
                   $"{total.Format()} không đủ để xuất ({output.Format()})";

                return (InventoryErrorCode.NotEnoughQuantity, message);
            }

            return GeneralCode.Success;

        }
        private string CreatePackageCode(string inventoryCode, string productCode, DateTime productManufactureDateTimeUtc)
        {
            var packageCode = string.Format("{0}-{1}-{2},", inventoryCode, productCode, productManufactureDateTimeUtc.ToString("YYYYMMdd"));
            //var package = await _objectGenCodeService.GenerateCode(EnumObjectType.Package);
            return packageCode;
        }
        #endregion
    }
}
