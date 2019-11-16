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
using VErp.Services.Stock.Model.FileResources;
using VErp.Services.Stock.Service.FileResources;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Stock.Service.Inventory.Implement
{
    public class InventoryService : IInventoryService
    {
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityService _activityService;
        private readonly IUnitService _unitService;
        private readonly IFileService _fileService;
        private readonly IAsyncRunnerService _asyncRunner;

        public InventoryService(StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<InventoryService> logger
            , IActivityService activityService
            , IUnitService unitService
            , IFileService fileService
            , IAsyncRunnerService asyncRunner
            )
        {
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityService = activityService;
            _unitService = unitService;
            _fileService = fileService;
            _asyncRunner = asyncRunner;
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

            var pagedData = new List<InventoryOutput>();
            foreach (var item in inventoryList)
            {
                #region Get Attached files 
                var attachedFiles = new List<FileToDownloadInfo>(4);
                if (_stockDbContext.InventoryFile.Any(q => q.InventoryId == item.InventoryId))
                {
                    var fileIdArray = _stockDbContext.InventoryFile.Where(q => q.InventoryId == item.InventoryId).Select(q => q.FileId).ToArray();
                    attachedFiles = _fileService.GetListFileUrl(fileIdArray, EnumThumbnailSize.Large);
                }
                #endregion

                var listInventoryDetails = dataList.Where(q => q.id.InventoryId == item.InventoryId).Select(q => q.id).ToList();
                var listInventoryDetailsOutput = new List<InventoryDetailOutput>(listInventoryDetails.Count);

                foreach (var details in listInventoryDetails)
                {
                    var productInfo = dataList.Select(q => q.p).FirstOrDefault(q => q.ProductId == details.ProductId);
                    var productUnitConversionInfo = _stockDbContext.ProductUnitConversion.AsNoTracking().FirstOrDefault(q => q.ProductUnitConversionId == details.ProductUnitConversionId);
                    ProductListOutput productOutput = null;
                    if (productInfo != null)
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
                    listInventoryDetailsOutput.Add(new InventoryDetailOutput
                    {
                        InventoryId = details.InventoryId,
                        InventoryDetailId = details.InventoryDetailId,
                        ProductId = details.ProductId,
                        PrimaryUnitId = details.PrimaryUnitId,
                        PrimaryQuantity = details.PrimaryQuantity,
                        SecondaryUnitId = details.SecondaryUnitId,
                        SecondaryQuantity = details.SecondaryQuantity,
                        ProductUnitConversionId = details.ProductUnitConversionId ?? 0,
                        RefObjectTypeId = details.RefObjectTypeId,
                        RefObjectId = details.RefObjectId,
                        RefObjectCode = details.RefObjectCode,

                        ProductOutput = productOutput,
                        ProductUnitConversion = productUnitConversionInfo
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
                var inventoryDetails = await _stockDbContext.InventoryDetail.Where(q => q.InventoryId == inventoryObj.InventoryId).AsNoTracking().ToListAsync();
                var listInventoryDetailsOutput = new List<InventoryDetailOutput>(inventoryDetails.Count);
                foreach (var details in inventoryDetails)
                {
                    var productInfo = _stockDbContext.Product.AsNoTracking().FirstOrDefault(q => q.ProductId == details.ProductId);
                    var productUnitConversionInfo = _stockDbContext.ProductUnitConversion.AsNoTracking().FirstOrDefault(q => q.ProductUnitConversionId == details.ProductUnitConversionId);
                    ProductListOutput productOutput = null;
                    if (productInfo != null)
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
                    listInventoryDetailsOutput.Add(new InventoryDetailOutput
                    {
                        InventoryId = details.InventoryId,
                        InventoryDetailId = details.InventoryDetailId,
                        ProductId = details.ProductId,
                        PrimaryUnitId = details.PrimaryUnitId,
                        PrimaryQuantity = details.PrimaryQuantity,
                        SecondaryUnitId = details.SecondaryUnitId,
                        SecondaryQuantity = details.SecondaryQuantity,
                        ProductUnitConversionId = details.ProductUnitConversionId ?? 0,
                        RefObjectTypeId = details.RefObjectTypeId,
                        RefObjectId = details.RefObjectId,
                        RefObjectCode = details.RefObjectCode,

                        ProductOutput = productOutput,
                        ProductUnitConversion = productUnitConversionInfo ?? null
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
                    DateUtc = inventoryObj.DateUtc,
                    CustomerId = inventoryObj.CustomerId,
                    Department = inventoryObj.Department,
                    UserId = inventoryObj.UserId,
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
        public async Task<ServiceResult<long>> AddInventoryInput(int currentUserId, InventoryInput req)
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
                if (_stockDbContext.Inventory.Any(q => q.InventoryCode == req.InventoryCode.Trim()))
                {
                    return GeneralCode.InvalidParams;
                }
                if (!DateTime.TryParseExact(req.DateUtc, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var issuedDate))
                {
                    return GeneralCode.InvalidParams;
                }

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var inventoryObj = new Infrastructure.EF.StockDB.Inventory
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

                        if (req.InventoryDetailInputList.Count > 0)
                        {
                            var inventoryDetailList = new List<InventoryDetail>(req.InventoryDetailInputList.Count);

                            foreach (var details in req.InventoryDetailInputList)
                            {
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
                                    ProductUnitConversionId = details.ProductUnitConversionId ?? null,
                                    RefObjectTypeId = details.RefObjectTypeId,
                                    RefObjectId = details.RefObjectId,
                                    RefObjectCode = details.RefObjectCode,
                                    FromPackageId = (req.InventoryTypeId == (int)EnumInventory.Output) ? details.FromPackageId : null
                                });
                            }
                            await _stockDbContext.InventoryDetail.AddRangeAsync(inventoryDetailList);
                            await _stockDbContext.SaveChangesAsync();
                        }
                        await _stockDbContext.SaveChangesAsync();
                        trans.Commit();
                        var objLog = GetInventoryInfoForLog(inventoryObj);
                        _activityService.CreateActivityAsync(EnumObjectType.Inventory, inventoryObj.InventoryId, $"Thêm mới phiếu nhập kho, mã: {inventoryObj.InventoryCode}", null, objLog);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddInventoryInput");
                return GeneralCode.InternalError;
            }
        }

        /// <summary>
        /// Thêm mới phiếu xuất kho
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<ServiceResult<long>> AddInventoryOutput(int currentUserId, InventoryInput req)
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
                if (_stockDbContext.Inventory.Any(q => q.InventoryCode == req.InventoryCode.Trim()))
                {
                    return GeneralCode.InvalidParams;
                }
                if (string.IsNullOrEmpty(req.DateUtc))
                {
                    return GeneralCode.InvalidParams;
                }

                if (!DateTime.TryParseExact(req.DateUtc, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var issuedDate))
                {
                    return GeneralCode.InvalidParams;
                }
                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var inventoryObj = new Infrastructure.EF.StockDB.Inventory
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

                        #region Thêm danh sách file đính kèm vào phiếu xuất
                        if (req.FileIdList != null && req.FileIdList.Count > 0)
                        {
                            var attachedFiles = new List<InventoryFile>(req.FileIdList.Count);
                            attachedFiles.AddRange(req.FileIdList.Select(fileId => new InventoryFile() { FileId = fileId, InventoryId = inventoryObj.InventoryId }));
                            await _stockDbContext.AddRangeAsync(attachedFiles);
                            await _stockDbContext.SaveChangesAsync();
                        }
                        #endregion

                        #region Thêm chi tiết và update thông tin kiện Package và StockProduct tương ứng
                        if (req.InventoryDetailInputList.Count > 0)
                        {
                            var inventoryDetailList = new List<InventoryDetail>(req.InventoryDetailInputList.Count);

                            foreach (var details in req.InventoryDetailInputList)
                            {
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
                                    ProductUnitConversionId = details.ProductUnitConversionId ?? null,
                                    RefObjectTypeId = details.RefObjectTypeId,
                                    RefObjectId = details.RefObjectId,
                                    RefObjectCode = details.RefObjectCode,
                                    FromPackageId = details.FromPackageId
                                });
                            }
                            await _stockDbContext.InventoryDetail.AddRangeAsync(inventoryDetailList);
                            await _stockDbContext.SaveChangesAsync();

                            var newInventoryDetails = _stockDbContext.InventoryDetail.Where(q => q.InventoryId == inventoryObj.InventoryId).AsNoTracking().ToList();
                            foreach (var item in newInventoryDetails)
                            {
                                if (item.FromPackageId == null || !(item.FromPackageId > 0)) continue;
                                var package = _stockDbContext.Package.FirstOrDefault(q => q.PackageId == item.FromPackageId);
                                if (package == null) continue;
                                package.PrimaryQuantityWaiting += item.PrimaryQuantity;
                                package.SecondaryQuantityWaitting += (item.SecondaryQuantity ?? 0);


                                #region Cập nhật thông tin StockProduct
                                var oldStockProduct = _stockDbContext.StockProduct.FirstOrDefault(q =>
                                    q.StockId == inventoryObj.StockId && q.ProductId == item.ProductId &&
                                    q.SecondaryUnitId == item.SecondaryUnitId && q.ProductUnitConversionId == item.ProductUnitConversionId);
                                if (oldStockProduct != null)
                                {
                                    oldStockProduct.PrimaryQuantityWaiting += item.PrimaryQuantity;
                                    oldStockProduct.SecondaryQuantityWaitting += (item.SecondaryQuantity ?? 0);
                                    oldStockProduct.UpdatedDatetimeUtc = DateTime.Now;
                                }
                                #endregion
                                await _stockDbContext.SaveChangesAsync();
                            }
                        }
                        #endregion

                        await _stockDbContext.SaveChangesAsync();
                        trans.Commit();
                        var objLog = GetInventoryInfoForLog(inventoryObj);
                        _activityService.CreateActivityAsync(EnumObjectType.Inventory, inventoryObj.InventoryId, $"Thêm mới phiếu xuất kho, mã: {inventoryObj.InventoryCode} ", null, objLog);


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
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddInventoryOutput");
                return GeneralCode.InternalError;
            }
        }

        /// <summary>
        /// Cập nhật phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Enum> UpdateInventoryInput(int inventoryId, int currentUserId, InventoryInput model)
        {
            try
            {
                if (inventoryId <= 0)
                {
                    return InventoryErrorCode.InventoryNotFound;
                }
                if (!DateTime.TryParseExact(model.DateUtc, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var issuedDate))
                {
                    return GeneralCode.InvalidParams;
                }
                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        #region Update Inventory - Phiếu nhập xuất kho
                        var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);
                        if (inventoryObj == null)
                        {
                            trans.Rollback();
                            return InventoryErrorCode.InventoryNotFound;
                        }
                        if (inventoryObj.IsApproved)
                        {
                            trans.Rollback();
                            return GeneralCode.NotYetSupported;
                        }
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
                        //inventoryObj.IsApproved = model.IsApproved;
                        inventoryObj.UpdatedByUserId = currentUserId;
                        inventoryObj.UpdatedDatetimeUtc = DateTime.Now;

                        await _stockDbContext.SaveChangesAsync();
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
                                        ProductUnitConversionId = item.ProductUnitConversionId ?? null,
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
                                    updatedItem.ProductUnitConversionId = item.ProductUnitConversionId ?? null;
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

                        #region Update Attached File
                        if (model.FileIdList != null && model.FileIdList.Count > 0)
                        {
                            var oldFileIdList = _stockDbContext.InventoryFile.Where(q => q.InventoryId == inventoryId).Select(q => q.FileId).ToList();
                            var deletedFileIdList = oldFileIdList.Except(model.FileIdList).ToList();
                            var deletedObjList = _stockDbContext.InventoryFile.Where(q => q.InventoryId == inventoryId && deletedFileIdList.Contains(q.FileId)).ToList();

                            var newAttachedFileList = new List<InventoryFile>(model.FileIdList.Count);
                            foreach (var fileId in model.FileIdList)
                            {
                                if (oldFileIdList.Contains(fileId))
                                    continue;
                                newAttachedFileList.Add(new InventoryFile { FileId = fileId, InventoryId = inventoryId });
                            }
                            _stockDbContext.RemoveRange(deletedObjList);
                            await _stockDbContext.AddRangeAsync(newAttachedFileList);
                            await _stockDbContext.SaveChangesAsync();
                        }
                        #endregion

                        await _stockDbContext.SaveChangesAsync();
                        trans.Commit();

                        var objLog = GetInventoryInfoForLog(inventoryObj);
                        var messageLog = inventoryObj.IsApproved ? string.Format("Duyệt phiếu nhập kho, mã:", inventoryObj.InventoryCode) : string.Format("Cập nhật phiếu nhập kho, mã:", inventoryObj.InventoryCode);
                        _activityService.CreateActivityAsync(EnumObjectType.Inventory, inventoryObj.InventoryId, messageLog, originalObj.JsonSerialize(), objLog);
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        _logger.LogError(ex, "UpdateInventoryInput");
                        return GeneralCode.InternalError;
                    }
                }
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateInventoryInput");
                return GeneralCode.InternalError;
            }
        }

        /// <summary>
        /// Cập nhật phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Enum> UpdateInventoryOutput(int inventoryId, int currentUserId, InventoryInput model)
        {
            try
            {
                if (inventoryId <= 0)
                {
                    return InventoryErrorCode.InventoryNotFound;
                }
                if (!DateTime.TryParseExact(model.DateUtc, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var issuedDate))
                {
                    return GeneralCode.InvalidParams;
                }
                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        #region Update Inventory - Phiếu nhập xuất kho
                        var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);
                        if (inventoryObj == null)
                        {
                            trans.Rollback();
                            return InventoryErrorCode.InventoryNotFound;
                        }
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
                        //inventoryObj.IsApproved = model.IsApproved;
                        inventoryObj.UpdatedByUserId = currentUserId;
                        inventoryObj.UpdatedDatetimeUtc = DateTime.Now;

                        await _stockDbContext.SaveChangesAsync();
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
                                        ProductUnitConversionId = item.ProductUnitConversionId ?? null,
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
                                    updatedItem.ProductUnitConversionId = item.ProductUnitConversionId ?? null;
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


                        #region Update Attached File
                        if (model.FileIdList != null && model.FileIdList.Count > 0)
                        {
                            var oldFileIdList = _stockDbContext.InventoryFile.Where(q => q.InventoryId == inventoryId).Select(q => q.FileId).ToList();
                            var deletedFileIdList = oldFileIdList.Except(model.FileIdList).ToList();
                            var deletedObjList = _stockDbContext.InventoryFile.Where(q => q.InventoryId == inventoryId && deletedFileIdList.Contains(q.FileId)).ToList();

                            var newAttachedFileList = new List<InventoryFile>(model.FileIdList.Count);
                            foreach (var fileId in model.FileIdList)
                            {
                                if (oldFileIdList.Contains(fileId))
                                    continue;
                                newAttachedFileList.Add(new InventoryFile { FileId = fileId, InventoryId = inventoryId });
                            }
                            _stockDbContext.RemoveRange(deletedObjList);
                            await _stockDbContext.AddRangeAsync(newAttachedFileList);
                            await _stockDbContext.SaveChangesAsync();
                        }
                        #endregion

                        #region Update StockProduct - Số liệu tồn kho
                        // Nếu phiếu nhập xuất được duyệt thì update số lượng
                        if (inventoryObj.IsApproved)
                        {
                            var inventoryDetails = _stockDbContext.InventoryDetail.Where(q => q.InventoryId == inventoryId).AsNoTracking().ToList();
                            foreach (var item in inventoryDetails)
                            {
                                var oldStockProduct = _stockDbContext.StockProduct.FirstOrDefault(q => q.StockId == inventoryObj.StockId && q.ProductId == item.ProductId && q.PrimaryUnitId == item.PrimaryUnitId);
                                if (oldStockProduct != null)
                                {
                                    oldStockProduct.PrimaryQuantityRemaining -= item.PrimaryQuantity;
                                    if (oldStockProduct.SecondaryUnitId == item.SecondaryUnitId)
                                        oldStockProduct.SecondaryQuantityRemaining -= (item.SecondaryQuantity ?? 0);
                                    oldStockProduct.UpdatedDatetimeUtc = DateTime.Now;
                                }
                                if (item.FromPackageId != null && item.FromPackageId > 0)
                                {
                                    var package = _stockDbContext.Package.FirstOrDefault(q => q.PackageId == item.FromPackageId);
                                    if (package != null)
                                    {
                                        package.UpdatedDatetimeUtc = DateTime.Now;
                                        package.PrimaryQuantityRemaining -= item.PrimaryQuantity;
                                        if (package.SecondaryUnitId == item.SecondaryUnitId)
                                            package.SecondaryQuantityWaitting = package.PrimaryQuantityRemaining - (item.SecondaryQuantity ?? 0);
                                    }
                                }
                                await _stockDbContext.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            var inventoryDetails = _stockDbContext.InventoryDetail.Where(q => q.InventoryId == inventoryId).AsNoTracking().ToList();
                            foreach (var item in inventoryDetails)
                            {
                                var oldStockProduct = _stockDbContext.StockProduct.FirstOrDefault(q => q.StockId == inventoryObj.StockId && q.ProductId == item.ProductId && q.SecondaryUnitId == item.SecondaryUnitId);
                                if (oldStockProduct != null)
                                {
                                    //oldStockProduct.PrimaryQuantityWaiting = oldStockProduct.PrimaryQuantity + ;
                                    //oldStockProduct.SecondaryQuantityRemaining -= (item.SecondaryQuantity ?? 0);
                                    //oldStockProduct.UpdatedDatetimeUtc = DateTime.Now;
                                }
                                if (item.FromPackageId != null && item.FromPackageId > 0)
                                {
                                    var package = _stockDbContext.Package.FirstOrDefault(q => q.PackageId == item.FromPackageId);
                                    if (package != null)
                                    {
                                        package.UpdatedDatetimeUtc = DateTime.Now;
                                        package.PrimaryQuantityRemaining -= item.PrimaryQuantity;
                                        if (package.SecondaryUnitId == item.SecondaryUnitId)
                                            package.SecondaryQuantityWaitting = package.PrimaryQuantityRemaining - (item.SecondaryQuantity ?? 0);
                                    }
                                }
                                await _stockDbContext.SaveChangesAsync();
                            }
                        }
                        #endregion

                        await _stockDbContext.SaveChangesAsync();
                        trans.Commit();

                        var objLog = GetInventoryInfoForLog(inventoryObj);
                        var messageLog = inventoryObj.IsApproved ? string.Format("Duyệt phiếu xuất kho, mã:", inventoryObj.InventoryCode) : string.Format("Cập nhật phiếu xuất kho, mã:", inventoryObj.InventoryCode);
                        _activityService.CreateActivityAsync(EnumObjectType.Inventory, inventoryObj.InventoryId, messageLog, originalObj.JsonSerialize(), objLog);
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        _logger.LogError(ex, "UpdateInventoryOutput");
                        return GeneralCode.InternalError;
                    }
                }
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateInventoryOutput");
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

        /// <summary>
        /// Duyệt phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Enum> ApproveInventoryInput(int inventoryId, int currentUserId)
        {
            try
            {
                if (inventoryId <= 0)
                {
                    return InventoryErrorCode.InventoryNotFound;
                }
                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        #region Update Inventory - Phiếu nhập xuất kho
                        var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);
                        if (inventoryObj == null)
                        {
                            trans.Rollback();
                            return InventoryErrorCode.InventoryNotFound;
                        }
                        var originalObj = GetInventoryInfoForLog(inventoryObj);

                        inventoryObj.IsDeleted = false;
                        inventoryObj.IsApproved = true;
                        inventoryObj.UpdatedByUserId = currentUserId;
                        inventoryObj.UpdatedDatetimeUtc = DateTime.Now;

                        await _stockDbContext.SaveChangesAsync();
                        #endregion

                        #region Update InventoryDetails
                        var inventoryDetailList = _stockDbContext.InventoryDetail.Where(q => q.InventoryId == inventoryId).ToList();
                        foreach (var item in inventoryDetailList)
                        {
                            if (item.ProductUnitConversionId > 0 && item.SecondaryUnitId > 0)
                                continue;
                            item.ProductUnitConversionId = null;
                            item.SecondaryUnitId = item.PrimaryUnitId;
                            item.SecondaryQuantity = item.PrimaryQuantity;
                        }
                        await _stockDbContext.SaveChangesAsync();
                        #endregion

                        var inventoryDetails = _stockDbContext.InventoryDetail.Where(q => q.InventoryId == inventoryId).AsNoTracking().ToList();

                        #region Update Package - Thông tin kiện

                        var packageList = new List<VErp.Infrastructure.EF.StockDB.Package>(inventoryDetails.Count);
                        foreach (var item in inventoryDetails)
                        {
                            var productObj = _stockDbContext.Product.AsNoTracking().FirstOrDefault(q => q.ProductId == item.ProductId);
                            var newPackageCode = CreatePackageCode(inventoryObj.InventoryCode, (productObj?.ProductCode ?? string.Empty), DateTime.Now);
                            packageList.Add(new VErp.Infrastructure.EF.StockDB.Package
                            {
                                InventoryDetailId = item.InventoryDetailId,
                                PackageCode = newPackageCode,
                                LocationId = null,
                                Date = inventoryObj.DateUtc,
                                ExpiryTime = null,
                                PrimaryUnitId = item.PrimaryUnitId,
                                PrimaryQuantity = item.PrimaryQuantity,
                                ProductUnitConversionId = item.ProductUnitConversionId ?? null,
                                SecondaryUnitId = item.SecondaryUnitId,
                                SecondaryQuantity = item.SecondaryQuantity,
                                PrimaryQuantityRemaining = item.PrimaryQuantity,
                                SecondaryQuantityRemaining = item.SecondaryQuantity ?? 0,
                                PrimaryQuantityWaiting = 0,
                                SecondaryQuantityWaitting = 0
                            });
                        }
                        await _stockDbContext.Package.AddRangeAsync(packageList);
                        await _stockDbContext.SaveChangesAsync();
                        #endregion

                        #region Update StockProduct - Số liệu tồn kho
                        //var inventoryDetails = _stockDbContext.InventoryDetail.Where(q => q.InventoryId == inventoryId).AsNoTracking().ToList();
                        foreach (var item in inventoryDetails)
                        {
                            var oldStockProduct = _stockDbContext.StockProduct.FirstOrDefault(q =>
                                             q.StockId == inventoryObj.StockId && q.ProductId == item.ProductId &&
                                             q.SecondaryUnitId == item.SecondaryUnitId && q.ProductUnitConversionId == item.ProductUnitConversionId);
                            if (oldStockProduct != null)
                            {
                                oldStockProduct.PrimaryQuantity += item.PrimaryQuantity;
                                oldStockProduct.SecondaryQuantity += item.SecondaryQuantity;

                                oldStockProduct.PrimaryQuantityRemaining += item.PrimaryQuantity;
                                oldStockProduct.SecondaryQuantityRemaining += item.SecondaryQuantity ?? 0;

                                oldStockProduct.UpdatedDatetimeUtc = DateTime.Now;
                            }
                            else
                            {
                                var newStockProduct = new Infrastructure.EF.StockDB.StockProduct
                                {
                                    StockId = inventoryObj.StockId,
                                    ProductId = item.ProductId,
                                    SecondaryUnitId = item.SecondaryUnitId ?? 0,
                                    SecondaryQuantity = item.SecondaryQuantity,
                                    ProductUnitConversionId = item.ProductUnitConversionId,
                                    PrimaryUnitId = item.PrimaryUnitId,
                                    PrimaryQuantity = item.PrimaryQuantity,
                                    PrimaryQuantityRemaining = item.PrimaryQuantity,
                                    SecondaryQuantityRemaining = item.SecondaryQuantity ?? 0,
                                    PrimaryQuantityWaiting = 0,
                                    SecondaryQuantityWaitting = 0,
                                    UpdatedDatetimeUtc = DateTime.Now
                                };
                                await _stockDbContext.StockProduct.AddAsync(newStockProduct);
                            }
                            await _stockDbContext.SaveChangesAsync();
                        }

                        #endregion

                        await _stockDbContext.SaveChangesAsync();
                        trans.Commit();

                        var objLog = GetInventoryInfoForLog(inventoryObj);
                        var messageLog = $"Duyệt phiếu nhập kho, mã: {inventoryObj.InventoryCode}";
                        _activityService.CreateActivityAsync(EnumObjectType.Inventory, inventoryObj.InventoryId, messageLog, originalObj.JsonSerialize(), objLog);
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        _logger.LogError(ex, "ApproveInventoryInput");
                        return GeneralCode.InternalError;
                    }
                }
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApproveInventoryInput");
                return GeneralCode.InternalError;
            }
        }

        /// <summary>
        /// Duyệt phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Enum> ApproveInventoryOutput(int inventoryId, int currentUserId)
        {
            try
            {
                if (inventoryId <= 0)
                {
                    return InventoryErrorCode.InventoryNotFound;
                }
                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        #region Update Inventory - Phiếu xuất kho
                        var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);
                        if (inventoryObj == null)
                        {
                            trans.Rollback();
                            return InventoryErrorCode.InventoryNotFound;
                        }
                        var originalObj = GetInventoryInfoForLog(inventoryObj);

                        inventoryObj.IsDeleted = false;
                        inventoryObj.IsApproved = true;
                        inventoryObj.UpdatedByUserId = currentUserId;
                        inventoryObj.UpdatedDatetimeUtc = DateTime.Now;

                        await _stockDbContext.SaveChangesAsync();
                        #endregion

                        #region Update Package & StockProduct - Số liệu tồn kho
                        var inventoryDetails = _stockDbContext.InventoryDetail.Where(q => q.InventoryId == inventoryId).AsNoTracking().ToList();
                        foreach (var item in inventoryDetails)
                        {
                            var oldStockProduct = _stockDbContext.StockProduct.FirstOrDefault(q => q.StockId == inventoryObj.StockId && q.ProductId == item.ProductId && q.SecondaryUnitId == item.SecondaryUnitId && q.ProductUnitConversionId == item.ProductUnitConversionId);
                            if (oldStockProduct != null)
                            {
                                oldStockProduct.PrimaryQuantityRemaining -= item.PrimaryQuantity;
                                oldStockProduct.SecondaryQuantityRemaining -= (item.SecondaryQuantity ?? 0);

                                oldStockProduct.PrimaryQuantityWaiting -= item.PrimaryQuantity;
                                oldStockProduct.SecondaryQuantityWaitting -= (item.SecondaryQuantity ?? 0);

                                oldStockProduct.UpdatedDatetimeUtc = DateTime.Now;
                            }
                            if (item.FromPackageId != null && item.FromPackageId > 0)
                            {
                                var package = _stockDbContext.Package.FirstOrDefault(q => q.PackageId == item.FromPackageId);
                                if (package != null)
                                {
                                    package.UpdatedDatetimeUtc = DateTime.Now;
                                    package.PrimaryQuantityRemaining -= item.PrimaryQuantity;
                                    package.SecondaryQuantityWaitting -= (item.SecondaryQuantity ?? 0);
                                }
                            }
                            await _stockDbContext.SaveChangesAsync();
                        }
                        #endregion

                        await _stockDbContext.SaveChangesAsync();
                        trans.Commit();

                        var objLog = GetInventoryInfoForLog(inventoryObj);
                        var messageLog = $"Duyệt phiếu xuất kho, mã: {inventoryObj.InventoryCode}";
                        _activityService.CreateActivityAsync(EnumObjectType.Inventory, inventoryObj.InventoryId, messageLog, originalObj.JsonSerialize(), objLog);
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        _logger.LogError(ex, "ApproveInventoryOutput");
                        return GeneralCode.InternalError;
                    }
                }
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApproveInventoryOutput");
                return GeneralCode.InternalError;
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
