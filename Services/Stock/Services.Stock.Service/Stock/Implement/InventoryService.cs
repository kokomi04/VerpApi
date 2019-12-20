using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
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
using System.IO;
using System.Text.RegularExpressions;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public class InventoryService : IInventoryService
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityService _activityService;
        private readonly IUnitService _unitService;
        private readonly IFileService _fileService;
        private readonly IObjectGenCodeService _objectGenCodeService;
        private readonly IAsyncRunnerService _asyncRunner;


        public InventoryService(MasterDBContext masterDBContext,StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<InventoryService> logger
            , IActivityService activityService
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
            _activityService = activityService;
            _unitService = unitService;
            _fileService = fileService;
            _objectGenCodeService = objectGenCodeService;
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
        public async Task<PageData<InventoryOutput>> GetList(string keyword, int stockId = 0, EnumInventoryType type = 0, DateTime? beginTime = null, DateTime? endTime = null, int page = 1, int size = 10)
        {
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

            if (beginTime.HasValue && endTime.HasValue)
            {
                query = query.Where(q => q.DateUtc >= beginTime && q.DateUtc <= endTime);
            }
            else
            {
                if (beginTime.HasValue)
                {
                    query = query.Where(q => q.DateUtc >= beginTime);
                }
                if (endTime.HasValue)
                {
                    query = query.Where(q => q.DateUtc <= endTime);
                }
            }

            var total = query.Count();
            var inventoryDataList = query.AsNoTracking().Skip((page - 1) * size).Take(size).ToList();

            var inventoryIdList = inventoryDataList.Select(q => q.InventoryId).ToList();
            var inventoryDetailsDataList = _stockDbContext.InventoryDetail.AsNoTracking().Where(q => inventoryIdList.Contains(q.InventoryId)).ToList();

            var productIdList = inventoryDetailsDataList.Select(q => q.ProductId).Distinct().ToList();
            var productDataList = _stockDbContext.Product.AsNoTracking().Where(q => productIdList.Contains(q.ProductId)).ToList();

            var pagedData = new List<InventoryOutput>();
            foreach (var item in inventoryDataList)
            {
                #region Get Attached files 
                var attachedFiles = new List<FileToDownloadInfo>(4);
                if (_stockDbContext.InventoryFile.Any(q => q.InventoryId == item.InventoryId))
                {
                    var fileIdArray = _stockDbContext.InventoryFile.Where(q => q.InventoryId == item.InventoryId).Select(q => q.FileId).ToArray();
                    attachedFiles = _fileService.GetListFileUrl(fileIdArray, EnumThumbnailSize.Large);
                }
                #endregion

                var listInventoryDetails = inventoryDetailsDataList.Where(q => q.InventoryId == item.InventoryId).ToList();
                var listInventoryDetailsOutput = new List<InventoryDetailOutput>(listInventoryDetails.Count);

                foreach (var details in listInventoryDetails)
                {
                    var productInfo = productDataList.FirstOrDefault(q => q.ProductId == details.ProductId);
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
                        UnitPrice = details.UnitPrice,
                        ProductUnitConversionId = details.ProductUnitConversionId,
                        ProductUnitConversionQuantity = details.ProductUnitConversionQuantity,
                        FromPackageId = details.FromPackageId,
                        ToPackageId = details.ToPackageId,
                        PackageOptionId = details.PackageOptionId,

                        RefObjectTypeId = details.RefObjectTypeId,
                        RefObjectId = details.RefObjectId,
                        RefObjectCode = details.RefObjectCode,

                        ProductOutput = productOutput,
                        ProductUnitConversion = productUnitConversionInfo
                    });
                }

                var stockInfo = _stockDbContext.Stock.AsNoTracking().FirstOrDefault(q => q.StockId == item.StockId);

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
                    StockKeeperUserId = item.StockKeeperUserId,
                    DeliveryCode = item.DeliveryCode,
                    TotalMoney = item.TotalMoney,

                    IsApproved = item.IsApproved,
                    CreatedByUserId = item.CreatedByUserId,
                    UpdatedByUserId = item.UpdatedByUserId,
                    UpdatedDatetimeUtc = item.UpdatedDatetimeUtc,
                    CreatedDatetimeUtc = item.CreatedDatetimeUtc,

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
                        UnitPrice = details.UnitPrice,
                        ProductUnitConversionId = details.ProductUnitConversionId,
                        ProductUnitConversionQuantity = details.ProductUnitConversionQuantity,
                        FromPackageId = details.FromPackageId,
                        ToPackageId = details.ToPackageId,
                        PackageOptionId = details.PackageOptionId,

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
                    StockKeeperUserId = inventoryObj.StockKeeperUserId,
                    DeliveryCode = inventoryObj.DeliveryCode,
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
            try
            {
                if (req == null || req.InProducts.Count == 0)
                {
                    return GeneralCode.InvalidParams;
                }

                if (_stockDbContext.Inventory.Any(q => q.InventoryCode == req.InventoryCode.Trim()))
                {
                    return InventoryErrorCode.InventoryCodeAlreadyExisted;
                }
                if (!DateTime.TryParseExact(req.DateUtc, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var issuedDate))
                {
                    return GeneralCode.InvalidParams;
                }

                var validInventoryDetails = await ValidateInventoryIn(req);

                if (!validInventoryDetails.Code.IsSuccess())
                {
                    return validInventoryDetails.Code;
                }

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var totalMoney = (decimal)0;
                        foreach (var item in validInventoryDetails.Data)
                        {
                            totalMoney += (item.UnitPrice * item.PrimaryQuantity);
                        }

                        var inventoryObj = new Infrastructure.EF.StockDB.Inventory
                        {
                            StockId = req.StockId,
                            InventoryCode = req.InventoryCode,
                            InventoryTypeId = (int)EnumInventoryType.Input,
                            Shipper = req.Shipper,
                            Content = req.Content,
                            DateUtc = issuedDate,
                            CustomerId = req.CustomerId,
                            Department = req.Department,
                            StockKeeperUserId = req.StockKeeperUserId,
                            DeliveryCode = req.DeliveryCode,
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
        public async Task<ServiceResult<long>> AddInventoryOutput(int currentUserId, InventoryOutModel req)
        {
            try
            {
                if (req == null || req.OutProducts.Count == 0)
                {
                    return GeneralCode.InvalidParams;
                }
                if (_stockDbContext.Inventory.Any(q => q.InventoryCode == req.InventoryCode.Trim()))
                {
                    return InventoryErrorCode.InventoryCodeAlreadyExisted;
                }
                if (!DateTime.TryParseExact(req.DateUtc, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var issuedDate))
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
                            InventoryTypeId = (int)EnumInventoryType.Output,
                            Shipper = req.Shipper,
                            Content = req.Content,
                            DateUtc = issuedDate,
                            CustomerId = req.CustomerId,
                            Department = req.Department,
                            StockKeeperUserId = req.StockKeeperUserId,
                            DeliveryCode = string.Empty,
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
        public async Task<Enum> UpdateInventoryInput(long inventoryId, int currentUserId, InventoryInModel req)
        {
            if (!DateTime.TryParseExact(req.DateUtc, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var issuedDate))
            {
                return GeneralCode.InvalidParams;
            }

            try
            {
                if (inventoryId <= 0)
                {
                    return InventoryErrorCode.InventoryNotFound;
                }

                var validate = await ValidateInventoryIn(req);

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
                        if (inventoryObj.IsApproved)
                        {
                            trans.Rollback();
                            return GeneralCode.NotYetSupported;
                        }
                        var originalObj = GetInventoryInfoForLog(inventoryObj);

                        inventoryObj.StockId = req.StockId;
                        inventoryObj.InventoryCode = req.InventoryCode;
                        inventoryObj.DateUtc = issuedDate;
                        inventoryObj.Shipper = req.Shipper;
                        inventoryObj.Content = req.Content;
                        inventoryObj.CustomerId = req.CustomerId;
                        inventoryObj.Department = req.Department;
                        inventoryObj.StockKeeperUserId = req.StockKeeperUserId;
                        inventoryObj.DeliveryCode = req.DeliveryCode;
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

                        var objLog = GetInventoryInfoForLog(inventoryObj);
                        var messageLog = inventoryObj.IsApproved ? string.Format("Duyệt phiếu nhập kho, mã: {0}", inventoryObj.InventoryCode) : string.Format("Cập nhật phiếu nhập kho, mã: {0}", inventoryObj.InventoryCode);
                        _activityService.CreateActivityAsync(EnumObjectType.Inventory, inventoryObj.InventoryId, messageLog, originalObj.JsonSerialize(), objLog);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateInventoryInput");
                return GeneralCode.InternalError;
            }
        }

        public async Task<ServiceResult<IList<CensoredInventoryInputProducts>>> InputUpdateGetAffectedPackages(int inventoryId, InventoryInModel req)
        {
            var data = await CensoredInventoryInputUpdateGetAffected(inventoryId, req);
            if (!data.Code.IsSuccess())
            {
                return data.Code;
            }

            return data.Data.products.ToList();

        }
        public async Task<ServiceResult<(IList<CensoredInventoryInputProducts> products, IList<InventoryDetail> details)>> CensoredInventoryInputUpdateGetAffected(int inventoryId, InventoryInModel req)
        {
            var details = await _stockDbContext.InventoryDetail.Where(iv => iv.InventoryId == inventoryId).ToListAsync();

            var deletedDetails = details.Where(d => !req.InProducts.Select(u => u.InventoryDetailId).Contains(d.InventoryDetailId));

            var data = await ValidateInventoryIn(req);
            if (!data.Code.IsSuccess())
            {
                return data.Code;
            }

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

                var newDetail = data.Data.FirstOrDefault(id => id.InventoryDetailId == d.InventoryDetailId);
                if (newDetail != null)
                {
                    newProductUnitConversionQuantity = newDetail.ProductUnitConversionQuantity;
                    newPrimaryQuantity = newDetail.PrimaryQuantity;
                }


                var product = new CensoredInventoryInputProducts()
                {
                    InventoryDetailId = d.InventoryDetailId,
                    ProductId = d.ProductId,
                    PrimaryUnitId = d.PrimaryUnitId,


                    OldPrimaryQuantity = d.PrimaryQuantity,
                    NewPrimaryQuantity = newPrimaryQuantity,

                    ProductUnitConversionId = d.ProductUnitConversionId.Value,
                    OldProductUnitConversionQuantity = d.ProductUnitConversionQuantity,
                    NewProductUnitConversionQuantity = newProductUnitConversionQuantity,
                    ToPackageId = d.ToPackageId.Value
                };


                var topPackage = await _stockDbContext.Package.FirstOrDefaultAsync(p => p.PackageId == product.ToPackageId);

                var affectObjects = new List<CensoredInventoryInputObject>();
                affectObjects.Add(new CensoredInventoryInputObject()
                {
                    ObjectId = d.InventoryDetailId,
                    ObjectCode = req.InventoryCode,
                    ObjectTypeId = EnumObjectType.InventoryDetail,
                    IsRoot = true,
                    IsCurrentFlow = true,

                    OldPrimaryQuantity = d.PrimaryQuantity,
                    NewPrimaryQuantity = newPrimaryQuantity,

                    OldProductUnitConversionQuantity = d.ProductUnitConversionQuantity,
                    NewProductUnitConversionQuantity = newProductUnitConversionQuantity,

                    Children = new List<TransferToObject>()
                    {
                        new TransferToObject {
                            IsEditable = false,
                            ObjectId = topPackage.PackageId,
                            ObjectTypeId = EnumObjectType.Package,
                            PackageOperationTypeId = EnumPackageOperationType.Join,

                            OldTransferPrimaryQuantity = d.PrimaryQuantity,
                            NewTransferPrimaryQuantity = newPrimaryQuantity,

                            OldTransferProductUnitConversionQuantity = d.ProductUnitConversionQuantity,
                            NewTransferProductUnitConversionQuantity = newProductUnitConversionQuantity,
                        }
                    }
                });

                var stack = new Stack<long>();
                stack.Push(topPackage.PackageId);
                while (stack.Count > 0)
                {
                    var packageId = stack.Pop();

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

                            OldPrimaryQuantity = r.PrimaryQuantity,
                            NewPrimaryQuantity = r.PrimaryQuantity,

                            OldProductUnitConversionQuantity = r.ProductUnitConversionQuantity,
                            NewProductUnitConversionQuantity = r.ProductUnitConversionQuantity,

                            Children = new List<TransferToObject>()
                            {
                                new TransferToObject{
                                    IsEditable = false,
                                    ObjectId = topPackage.PackageId,
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
                                    ObjectId = topPackage.PackageId,
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


                    var packageInfo = await _stockDbContext.Package.FirstOrDefaultAsync(p => p.PackageId == product.ToPackageId);

                    var childrenPackages = await _stockDbContext.PackageRef.Where(p => p.RefPackageId == packageId).ToListAsync();

                    var currentPackageNode = new CensoredInventoryInputObject()
                    {
                        ObjectId = packageInfo.PackageId,
                        ObjectCode = packageInfo.PackageCode,
                        ObjectTypeId = EnumObjectType.Package,
                        IsRoot = false,

                        OldPrimaryQuantity = packageInfo.PrimaryQuantity,
                        NewPrimaryQuantity = packageInfo.PrimaryQuantity,

                        OldProductUnitConversionQuantity = packageInfo.ProductUnitConversionQuantity,
                        NewProductUnitConversionQuantity = packageInfo.ProductUnitConversionQuantity,

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
                    }

                    affectObjects.Add(currentPackageNode);

                    foreach (var c in childrenPackages)
                    {
                        stack.Push(c.PackageId);
                    }

                }
                product.AffectObjects = affectObjects;

                products.Add(product);

            }

            return (products, details);
        }

        public async Task<ServiceResult> CensoredInventoryInputUpdate(int inventoryId, InventoryInModel req, IList<CensoredInventoryInputProducts> updateProducts)
        {
            var data = await CensoredInventoryInputUpdateGetAffected(inventoryId, req);

            if (!data.Code.IsSuccess())
            {
                return data.Code;
            }

            var products = data.Data.products;
            var details = data.Data.details;

            foreach (var p in products)
            {
                var updateProduct = updateProducts.FirstOrDefault(d => d.InventoryDetailId == p.InventoryDetailId);
                if (updateProduct != null)
                {

                    var productUnitConversionInfo = await _stockDbContext.ProductUnitConversion.FirstOrDefaultAsync(c => c.ProductUnitConversionId == p.ProductUnitConversionId);
                    if (productUnitConversionInfo == null)
                    {
                        return ProductUnitConversionErrorCode.ProductUnitConversionNotFound;
                    }

                    foreach (var obj in p.AffectObjects)
                    {                       
                        var expression = $"({obj.NewProductUnitConversionQuantity})*({productUnitConversionInfo.FactorExpression})";

                        if (obj.NewProductUnitConversionQuantity > 0)
                        {
                            var primaryQualtity = Utils.Eval(expression);
                            if (!(primaryQualtity > 0))
                            {
                                return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                            }

                            obj.NewPrimaryQuantity = primaryQualtity;
                        }

                        foreach(var c in obj.Children)
                        {
                            expression = $"({c.NewTransferProductUnitConversionQuantity})*({productUnitConversionInfo.FactorExpression})";

                            if (c.NewTransferProductUnitConversionQuantity > 0)
                            {
                                var primaryQualtity = Utils.Eval(expression);
                                if (!(primaryQualtity > 0))
                                {
                                    return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                                }

                                obj.NewPrimaryQuantity = primaryQualtity;
                            }
                        }
                        
                    }

                    foreach (var obj in p.AffectObjects)
                    {
                        var updatedObj = updateProduct.AffectObjects.FirstOrDefault(a => a.ObjectKey == obj.ObjectKey);
                        if (updatedObj == null) continue;                       

                        decimal totalInPrimaryQuantity = 0;
                        decimal totalInProductUnitConversionQuantity = 0;

                        foreach (var parent in p.AffectObjects)
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


                        obj.NewPrimaryQuantity = updatedObj.NewPrimaryQuantity;
                        obj.NewProductUnitConversionQuantity = updatedObj.NewProductUnitConversionQuantity;

                        decimal totalOutPrimaryQuantity = 0;
                        decimal totalOutProductUnitConversionQuantity = 0;

                        foreach (var child in obj.Children)
                        {
                            var updatedChild = updatedObj.Children.FirstOrDefault(c => c.ObjectKey == child.ObjectKey);

                            child.NewTransferPrimaryQuantity = updatedChild.NewTransferPrimaryQuantity;
                            child.NewTransferProductUnitConversionQuantity = updatedChild.NewTransferProductUnitConversionQuantity;

                            totalOutPrimaryQuantity += child.NewTransferPrimaryQuantity;
                            totalOutProductUnitConversionQuantity += child.NewTransferProductUnitConversionQuantity;
                        }

                        if (totalOutPrimaryQuantity > totalInPrimaryQuantity || totalOutProductUnitConversionQuantity > totalInProductUnitConversionQuantity)
                        {
                            return GeneralCode.InvalidParams;
                        }
                    }
                }
            }

            foreach (var p in products)
            {
                var detail = details.FirstOrDefault(d => d.InventoryDetailId == p.InventoryDetailId);
                if (detail != null)
                {
                    if (p.NewProductUnitConversionQuantity == 0)
                    {
                        detail.IsDeleted = true;
                    }


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

                        foreach (var r in obj.Children)
                        {
                            if (r.IsEditable)
                            {
                                //substract parent
                                var deltaPrimaryQuantity = r.NewTransferPrimaryQuantity - r.OldTransferPrimaryQuantity;
                                var deltaConversionQuantity = r.NewTransferProductUnitConversionQuantity - r.OldTransferProductUnitConversionQuantity;

                                switch (obj.ObjectTypeId)
                                {
                                    case EnumObjectType.Package:
                                        ((PackageEntity)parent).PrimaryQuantity -= deltaPrimaryQuantity;
                                        ((PackageEntity)parent).ProductUnitConversionQuantity -= deltaConversionQuantity;
                                        break;
                                    case EnumObjectType.InventoryDetail:
                                        ((InventoryDetail)parent).PrimaryQuantity -= deltaPrimaryQuantity;
                                        ((InventoryDetail)parent).ProductUnitConversionQuantity -= deltaConversionQuantity;
                                        break;
                                    default:
                                        throw new NotSupportedException();
                                }

                                //addition children
                                switch (r.ObjectTypeId)
                                {
                                    case EnumObjectType.Package:

                                        var refInfo = await _stockDbContext.PackageRef.FirstOrDefaultAsync(rd => rd.PackageId == r.ObjectId && rd.RefPackageId == obj.ObjectId);
                                        refInfo.PrimaryQuantity += deltaPrimaryQuantity;
                                        refInfo.ProductUnitConversionQuantity += deltaConversionQuantity;

                                        var childPackage = await _stockDbContext.Package.FirstOrDefaultAsync(c => c.PackageId == r.ObjectId);

                                        childPackage.PrimaryQuantity += deltaPrimaryQuantity;
                                        childPackage.ProductUnitConversionQuantity += deltaConversionQuantity;

                                        break;

                                    case EnumObjectType.InventoryDetail:

                                        var childInventoryDetail = await _stockDbContext.InventoryDetail.FirstOrDefaultAsync(c => c.InventoryDetailId == r.ObjectId);

                                        childInventoryDetail.PrimaryQuantity += deltaPrimaryQuantity;
                                        childInventoryDetail.ProductUnitConversionQuantity += deltaConversionQuantity;

                                        break;
                                    default:
                                        throw new NotSupportedException();

                                }
                            }
                        }

                    }
                }

            }


            await _stockDbContext.SaveChangesAsync();

            return GeneralCode.Success;
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
            try
            {
                if (!DateTime.TryParseExact(req.DateUtc, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var issuedDate))
                {
                    return GeneralCode.InvalidParams;
                }

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
                        var originalObj = GetInventoryInfoForLog(inventoryObj);

                        inventoryObj.InventoryCode = req.InventoryCode;
                        inventoryObj.Shipper = req.Shipper;
                        inventoryObj.Content = req.Content;
                        inventoryObj.DateUtc = issuedDate;
                        inventoryObj.CustomerId = req.CustomerId;
                        inventoryObj.Department = req.Department;
                        inventoryObj.StockKeeperUserId = req.StockKeeperUserId;
                        inventoryObj.IsApproved = false;
                        inventoryObj.UpdatedByUserId = currentUserId;
                        inventoryObj.UpdatedDatetimeUtc = DateTime.UtcNow;

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateInventoryOutput");
                return GeneralCode.InternalError;
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
            var objLog = GetInventoryInfoForLog(inventoryObj);
            if (inventoryObj == null)
            {
                return InventoryErrorCode.InventoryNotFound;
            }
            if (inventoryObj.InventoryTypeId == (int)EnumInventoryType.Output)
            {
                return GeneralCode.InvalidParams;
            }
            inventoryObj.IsDeleted = true;
            //inventoryObj.IsApproved = false;
            inventoryObj.UpdatedByUserId = currentUserId;
            inventoryObj.UpdatedDatetimeUtc = DateTime.UtcNow;

            var dataBefore = objLog.JsonSerialize();

            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (inventoryObj.IsApproved)
                    {
                        var processResult = await RollBackInventoryInput(inventoryObj);
                        if (!Equals(processResult, GeneralCode.Success))
                        {
                            trans.Rollback();
                            return GeneralCode.InvalidParams;
                        }
                    }
                    _activityService.CreateActivityAsync(EnumObjectType.Product, inventoryObj.StockId, string.Format("Xóa phiếu nhập kho, mã phiếu {0}", inventoryObj.InventoryCode), dataBefore, null);
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

        /// <summary>
        /// Xoá phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        public async Task<Enum> DeleteInventoryOutput(long inventoryId, int currentUserId)
        {
            var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(p => p.InventoryId == inventoryId);
            var objLog = GetInventoryInfoForLog(inventoryObj);
            if (inventoryObj == null)
            {
                return InventoryErrorCode.InventoryNotFound;
            }
            if (inventoryObj.InventoryTypeId == (int)EnumInventoryType.Input)
            {
                return GeneralCode.InvalidParams;
            }
            inventoryObj.IsDeleted = true;
            //inventoryObj.IsApproved = false;
            inventoryObj.UpdatedByUserId = currentUserId;
            inventoryObj.UpdatedDatetimeUtc = DateTime.UtcNow;

            var dataBefore = objLog.JsonSerialize();

            // Xử lý xoá thông tin phiếu xuất kho
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (inventoryObj.IsApproved)
                    {
                        var processResult = await RollbackInventoryOutput(inventoryObj);
                        if (!Equals(processResult, GeneralCode.Success))
                        {
                            trans.Rollback();
                            return GeneralCode.InvalidParams;
                        }
                    }
                    _activityService.CreateActivityAsync(EnumObjectType.Product, inventoryObj.StockId, string.Format("Xóa phiếu xuất kho, mã phiếu {0}", inventoryObj.InventoryCode), dataBefore, null);
                    await _stockDbContext.SaveChangesAsync();
                    trans.Commit();

                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "DeleteInventoryOutput");
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
        public async Task<Enum> ApproveInventoryInput(long inventoryId, int currentUserId)
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
                        var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);
                        if (inventoryObj == null)
                        {
                            trans.Rollback();
                            return InventoryErrorCode.InventoryNotFound;
                        }
                        if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Input)
                        {
                            trans.Rollback();
                            return InventoryErrorCode.InventoryNotFound;
                        }

                        if (inventoryObj.IsApproved)
                        {
                            return InventoryErrorCode.InventoryAlreadyApproved;
                        }

                        var originalObj = GetInventoryInfoForLog(inventoryObj);

                        inventoryObj.IsApproved = true;
                        inventoryObj.UpdatedByUserId = currentUserId;
                        inventoryObj.UpdatedDatetimeUtc = DateTime.UtcNow;

                        await _stockDbContext.SaveChangesAsync();

                        var inventoryDetails = _stockDbContext.InventoryDetail.Where(q => q.InventoryId == inventoryId).ToList();

                        var inputTransfer = new List<InventoryDetailToPackage>();
                        foreach (var item in inventoryDetails)
                        {
                            await UpdateStockProduct(inventoryObj, item);

                            if (item.PackageOptionId != null)
                                switch ((EnumPackageOption)item.PackageOptionId)
                                {
                                    case EnumPackageOption.Append:
                                        var appendResult = await AppendToCustomPackage(inventoryObj, item);
                                        if (!appendResult.IsSuccess())
                                        {
                                            trans.Rollback();
                                            return appendResult;
                                        }

                                        break;

                                    case EnumPackageOption.NoPackageManager:
                                        var defaultPackge = await AppendToDefaultPackage(inventoryObj, item);
                                        item.ToPackageId = defaultPackge.PackageId;

                                        break;

                                    case EnumPackageOption.Create:

                                        var createNewPackageResult = await CreateNewPackage(inventoryObj, item);
                                        if (!createNewPackageResult.Code.IsSuccess())
                                        {
                                            trans.Rollback();
                                            return createNewPackageResult.Code;
                                        }

                                        item.ToPackageId = createNewPackageResult.Data.PackageId;
                                        break;
                                    default:
                                        return GeneralCode.NotYetSupported;
                                }
                            else
                            {
                                var createNewPackageResult = await CreateNewPackage(inventoryObj, item);
                                if (!createNewPackageResult.Code.IsSuccess())
                                {
                                    trans.Rollback();
                                    return createNewPackageResult.Code;
                                }

                                item.ToPackageId = createNewPackageResult.Data.PackageId;
                            }

                            inputTransfer.Add(new InventoryDetailToPackage()
                            {
                                InventoryDetailId = item.InventoryDetailId,
                                ToPackageId = item.ToPackageId.Value,
                                CreatedDatetimeUtc = DateTime.UtcNow,
                                IsDeleted = false
                            });
                        }

                        await _stockDbContext.InventoryDetailToPackage.AddRangeAsync(inputTransfer);
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
        public async Task<Enum> ApproveInventoryOutput(long inventoryId, int currentUserId)
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
                        var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);
                        if (inventoryObj == null)
                        {
                            trans.Rollback();
                            return InventoryErrorCode.InventoryNotFound;
                        }
                        if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Output)
                        {
                            trans.Rollback();
                            return InventoryErrorCode.InventoryNotFound;
                        }

                        var originalObj = GetInventoryInfoForLog(inventoryObj);

                        inventoryObj.IsApproved = true;
                        inventoryObj.UpdatedByUserId = currentUserId;
                        inventoryObj.UpdatedDatetimeUtc = DateTime.UtcNow;

                        var inventoryDetails = _stockDbContext.InventoryDetail.Where(d => d.InventoryId == inventoryId).ToList();

                        var fromPackageIds = inventoryDetails.Select(f => f.FromPackageId).ToList();
                        var fromPackages = _stockDbContext.Package.Where(p => fromPackageIds.Contains(p.PackageId)).ToList();

                        foreach (var detail in inventoryDetails)
                        {
                            var fromPackageInfo = fromPackages.FirstOrDefault(p => p.PackageId == detail.FromPackageId);
                            if (fromPackageInfo == null) return PackageErrorCode.PackageNotFound;

                            fromPackageInfo.PrimaryQuantityWaiting -= detail.PrimaryQuantity;
                            fromPackageInfo.PrimaryQuantityRemaining -= detail.PrimaryQuantity;
                            fromPackageInfo.ProductUnitConversionWaitting -= detail.ProductUnitConversionQuantity;
                            fromPackageInfo.ProductUnitConversionRemaining -= detail.ProductUnitConversionQuantity;

                            var stockProduct = await EnsureStockProduct(inventoryObj.StockId, detail.ProductId, detail.PrimaryUnitId, detail.ProductUnitConversionId);

                            stockProduct.PrimaryQuantityWaiting -= detail.PrimaryQuantity;
                            stockProduct.PrimaryQuantityRemaining -= detail.PrimaryQuantity;
                            stockProduct.ProductUnitConversionWaitting -= detail.ProductUnitConversionQuantity;
                            stockProduct.ProductUnitConversionRemaining -= detail.ProductUnitConversionQuantity;
                        }
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
            try
            {
                var productInStockQuery = from i in _stockDbContext.Inventory
                                          join id in _stockDbContext.InventoryDetail on i.InventoryId equals id.InventoryId
                                          join p in _stockDbContext.Product on id.ProductId equals p.ProductId
                                          where i.IsApproved && stockIdList.Contains(i.StockId) && i.InventoryTypeId == (int)EnumInventoryType.Input
                                          select p;
                if (!string.IsNullOrEmpty(keyword))
                    productInStockQuery = productInStockQuery.Where(q => q.ProductName.Contains(keyword) || q.ProductCode.Contains(keyword));

                productInStockQuery = productInStockQuery.GroupBy(q => q.ProductId).Select(g => g.First());

                var total = productInStockQuery.Count();
                var pagedData = productInStockQuery.AsNoTracking().Skip((page - 1) * size).Take(size).ToList();

                var productIdList = pagedData.Select(q => q.ProductId).ToList();
                var productExtraData = _stockDbContext.ProductExtraInfo.AsNoTracking().Where(q => productIdList.Contains(q.ProductId)).ToList();
                var unitIdList = pagedData.Select(q => q.UnitId).Distinct().ToList();
                var unitOutputList = await _unitService.GetListByIds(unitIdList);

                var productList = new List<ProductListOutput>(total);
                foreach (var item in pagedData)
                {
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
                        Specification = productExtraData.FirstOrDefault(q => q.ProductId == item.ProductId).Specification,
                        UnitId = item.UnitId,
                        UnitName = unitOutputList.FirstOrDefault(q => q.UnitId == item.UnitId).UnitName ?? string.Empty,
                        EstimatePrice = item.EstimatePrice ?? 0
                    });
                }
                return (productList, total);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetProductListForExport");
                return (null, 0);
            }
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
            try
            {
                var query = from p in _stockDbContext.Package
                            where stockIdList.Contains(p.StockId) && p.ProductId == productId && p.PrimaryQuantityRemaining > 0
                            select p;

                var total = query.Count();
                var packageData = query.AsNoTracking().Skip((page - 1) * size).Take(size).ToList();
                var locationIdList = packageData.Select(q => q.LocationId).ToList();
                var productUnitConversionIdList = packageData.Select(q => q.ProductUnitConversionId).ToList();
                var locationData = await _stockDbContext.Location.AsNoTracking().Where(q => locationIdList.Contains(q.LocationId)).ToListAsync();
                var productUnitConversionData = _stockDbContext.ProductUnitConversion.AsNoTracking().Where(q => productUnitConversionIdList.Contains(q.ProductUnitConversionId)).ToList();

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
                        Date = item.Date,
                        ExpiryTime = item.ExpiryTime,
                        PrimaryUnitId = item.PrimaryUnitId,
                        PrimaryQuantity = item.PrimaryQuantity,
                        ProductUnitConversionId = item.ProductUnitConversionId,
                        ProductUnitConversionQuantity = item.ProductUnitConversionQuantity,
                        PrimaryQuantityWaiting = item.PrimaryQuantityWaiting,
                        PrimaryQuantityRemaining = item.PrimaryQuantityRemaining,
                        ProductUnitConversionWaitting = item.ProductUnitConversionWaitting,
                        ProductUnitConversionRemaining = item.ProductUnitConversionRemaining,

                        CreatedDatetimeUtc = item.CreatedDatetimeUtc,
                        UpdatedDatetimeUtc = item.UpdatedDatetimeUtc,
                        LocationOutputModel = locationOutputModel,
                        ProductUnitConversionModel = productUnitConversionData.FirstOrDefault(q => q.ProductUnitConversionId == item.ProductUnitConversionId) ?? null
                    });
                }

                return (packageList, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPackageListForExport");
                return (null, 0);
            }
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
            try
            {
                var productWithStockValidationIdList = _stockDbContext.ProductStockValidation.Select(q => q.ProductId).ToList();

                var productWithStockValidationQuery = from p in _stockDbContext.Product
                                                      join c in _stockDbContext.ProductCate on p.ProductCateId equals c.ProductCateId
                                                      join pv in _stockDbContext.ProductStockValidation on p.ProductId equals pv.ProductId
                                                      where stockIdList.Contains(pv.StockId)
                                                      select p;
                if (!string.IsNullOrEmpty(keyword))
                {
                    productWithStockValidationQuery = productWithStockValidationQuery.Where(q => q.ProductName.Contains(keyword) || q.ProductCode.Contains(keyword));
                }

                productWithStockValidationQuery = productWithStockValidationQuery.GroupBy(q => q.ProductId).Select(v => v.First());

                var productWithoutStockValidationQuery = from p in _stockDbContext.Product
                                                         join c in _stockDbContext.ProductCate on p.ProductCateId equals c.ProductCateId
                                                         where !productWithStockValidationIdList.Contains(p.ProductId)
                                                         select p;

                var productQuery = productWithStockValidationQuery.Union(productWithoutStockValidationQuery);

                var total = productQuery.Count();
                productQuery = productQuery.AsNoTracking().OrderBy(q => q.ProductId).Skip((page - 1) * size).Take(size);

                var pagedData = productQuery.ToList();
                var productIdList = pagedData.Select(q => q.ProductId).ToList();
                var productExtraData = _stockDbContext.ProductExtraInfo.AsNoTracking().Where(q => productIdList.Contains(q.ProductId)).ToList();
                var unitIdList = pagedData.Select(q => q.UnitId).Distinct().ToList();
                var unitOutputList = await _unitService.GetListByIds(unitIdList);

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
                        EstimatePrice = item.EstimatePrice ?? 0
                    });
                }
                return (productList, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetProductListForImport");
                return (null, 0);
            }
        }

        /// <summary>
        /// Đọc file và xử lý nhập liệu số dư đầu kỳ theo kho
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ServiceResult<long>> ProcessOpeningBalance(int currentUserId, InventoryOpeningBalanceInputModel model)
        {
            try
            {
                if (model.FileIdList.Count < 1)
                    return GeneralCode.InvalidParams;
                               
                foreach (var fileId in model.FileIdList)
                {
                    var ret = await _fileService.GetFileStream(fileId);
                    if (ret.Data.info == null)
                        continue;
                    var fileExtension = string.Empty;
                    var checkExt = Regex.IsMatch(ret.Data.info.FileName, @"\bxls\b");
                    if (checkExt)
                        fileExtension = "xls";
                    else
                    {
                        checkExt = Regex.IsMatch(ret.Data.info.FileName, @"\bxlsx\b");
                        if (checkExt)
                            fileExtension = "xlsx";
                    }
                    IWorkbook wb = null;
                    List<ISheet> sheetList = null;
                    using (var fs = ret.Data.file as FileStream)
                    {
                        if (fs != null)
                        {
                            switch (fileExtension)
                            {
                                case "xls":
                                    {
                                        wb = new XSSFWorkbook(fs);
                                        var numberOfSheet = wb.NumberOfSheets;
                                        for (var i = 0; i < numberOfSheet; i++)
                                            sheetList.Add(wb.GetSheetAt(i));
                                        break;
                                    }
                                case "xlsx":
                                    {
                                        wb = new HSSFWorkbook(fs);
                                        var numberOfSheet = wb.NumberOfSheets;
                                        for (var i = 0; i < numberOfSheet; i++)
                                            sheetList.Add(wb.GetSheetAt(i));
                                        break;
                                    }
                                default:
                                    continue;
                            }
                            #region Process wb and sheet
                            await ProcessExcelSheet(sheetList, model, currentUserId);
                            #endregion
                        }
                        else
                        {
                            return GeneralCode.InternalError;
                        }
                    }
                }


                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessOpeningBalance");
                return GeneralCode.InternalError;
            }
        }

        #region Private helper method

        private async Task<ServiceResult<IList<InventoryDetail>>> ValidateInventoryIn(InventoryInModel req)
        {
            var productIds = req.InProducts.Select(p => p.ProductId).Distinct().ToList();

            var productInfos = await _stockDbContext.Product.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToListAsync();
            var productUnitConversions = await _stockDbContext.ProductUnitConversion.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToListAsync();

            var toPackageIds = req.InProducts.Select(p => p.ToPackageId).ToList();
            var toPackages = await _stockDbContext.Package.Where(p => toPackageIds.Contains(p.PackageId) && p.PackageTypeId == (int)EnumPackageType.Custom).ToListAsync();

            var inventoryDetailList = new List<InventoryDetail>(req.InProducts.Count);
            foreach (var details in req.InProducts)
            {
                var productInfo = productInfos.FirstOrDefault(p => p.ProductId == details.ProductId);

                var primaryQty = details.PrimaryQuantity;

                if (productInfo == null)
                {
                    return ProductErrorCode.ProductNotFound;
                }

                if (details.ProductUnitConversionQuantity <= 0)
                {
                    return GeneralCode.InvalidParams;
                }
                if(details.ProductUnitConversionId != null && details.ProductUnitConversionId > 0)
                {
                    var productUnitConversionInfo = productUnitConversions.FirstOrDefault(c => c.ProductUnitConversionId == details.ProductUnitConversionId);
                    if (productUnitConversionInfo == null)
                    {
                        return ProductUnitConversionErrorCode.ProductUnitConversionNotFound;
                    }
                    var expression = $"({details.ProductUnitConversionQuantity})*({productUnitConversionInfo.FactorExpression})";
                    primaryQty = Utils.Eval(expression);
                    if (!(primaryQty > 0))
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

                        if (details.ToPackageId.HasValue)
                        {
                            return GeneralCode.InvalidParams;
                        }
                        break;
                }

                inventoryDetailList.Add(new InventoryDetail
                {
                    ProductId = details.ProductId,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    PrimaryUnitId = productInfo.UnitId,
                    PrimaryQuantity = primaryQty,
                    ProductUnitConversionQuantity = details.ProductUnitConversionQuantity,
                    ProductUnitConversionId = details.ProductUnitConversionId,
                    RefObjectTypeId = details.RefObjectTypeId,
                    RefObjectId = details.RefObjectId,
                    RefObjectCode = details.RefObjectCode,
                    FromPackageId = null,
                    ToPackageId = details.ToPackageId,
                    PackageOptionId = (int)details.PackageOptionId
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
                    return InventoryErrorCode.InvalidPackage;
                }

                var productInfo = productInfos.FirstOrDefault(p => p.ProductId == details.ProductId);
                if (productInfo == null)
                {
                    return ProductErrorCode.ProductNotFound;
                }

                var productUnitConversionInfo = productUnitConversions.FirstOrDefault(c => c.ProductUnitConversionId == details.ProductUnitConversionId);
                if (productUnitConversionInfo == null)
                {
                    return ProductUnitConversionErrorCode.ProductUnitConversionNotFound;
                }

                if (details.ProductUnitConversionQuantity <= 0)
                {
                    return GeneralCode.InvalidParams;
                }

                if (details.ProductUnitConversionQuantity > fromPackageInfo.ProductUnitConversionQuantity)
                {
                    return InventoryErrorCode.NotEnoughQuantity;
                }

                var expression = $"({details.ProductUnitConversionQuantity})*({productUnitConversionInfo.FactorExpression})";

                var primaryQualtity = Utils.Eval(expression);
                if (!(primaryQualtity > 0))
                {
                    return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                }

                inventoryDetailList.Add(new InventoryDetail
                {
                    InventoryId = inventory.InventoryId,
                    ProductId = details.ProductId,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    PrimaryUnitId = fromPackageInfo.PrimaryUnitId,
                    PrimaryQuantity = primaryQualtity,
                    ProductUnitConversionQuantity = details.ProductUnitConversionQuantity,
                    ProductUnitConversionId = details.ProductUnitConversionId,
                    RefObjectTypeId = details.RefObjectTypeId,
                    RefObjectId = details.RefObjectId,
                    RefObjectCode = details.RefObjectCode,
                    FromPackageId = details.FromPackageId,
                    ToPackageId = null,
                    PackageOptionId = null
                });

                fromPackageInfo.PrimaryQuantityWaiting += primaryQualtity;
                fromPackageInfo.ProductUnitConversionWaitting += details.ProductUnitConversionQuantity;

                var stockProductInfo = await EnsureStockProduct(inventory.StockId, fromPackageInfo.ProductId, fromPackageInfo.PrimaryUnitId, fromPackageInfo.ProductUnitConversionId);

                stockProductInfo.PrimaryQuantityWaiting += primaryQualtity;
                stockProductInfo.ProductUnitConversionWaitting += details.ProductUnitConversionQuantity;
            }
            return inventoryDetailList;
        }

        private async Task<StockProduct> EnsureStockProduct(int stockId, int productId, int primaryUnitId, int? productUnitConversionId)
        {
            var stockProductInfo = await _stockDbContext.StockProduct
                                .FirstOrDefaultAsync(s =>
                                                s.StockId == stockId
                                                && s.ProductId == productId
                                                && s.PrimaryUnitId == primaryUnitId
                                                && s.ProductUnitConversionId == productUnitConversionId
                                                );

            if (stockProductInfo == null)
            {
                stockProductInfo = new Infrastructure.EF.StockDB.StockProduct()
                {
                    StockId = stockId,
                    ProductId = productId,
                    ProductUnitConversionId = productUnitConversionId,
                    PrimaryUnitId = primaryUnitId,
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

        private async Task UpdateStockProduct(InventoryEntity inventory, InventoryDetail detail, EnumInventoryType type = EnumInventoryType.Input)
        {
            var stockProductInfo = await EnsureStockProduct(inventory.StockId, detail.ProductId, detail.PrimaryUnitId, detail.ProductUnitConversionId);
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

        private async Task<Enum> AppendToCustomPackage(InventoryEntity inventory, InventoryDetail detail)
        {
            var packageInfo = await _stockDbContext.Package.FirstOrDefaultAsync(p => p.PackageId == detail.ToPackageId && p.PackageTypeId == (int)EnumPackageType.Custom);
            if (packageInfo == null) return PackageErrorCode.PackageNotFound;

            packageInfo.PrimaryQuantity += detail.PrimaryQuantity;
            packageInfo.PrimaryQuantityRemaining += detail.PrimaryQuantity;
            packageInfo.ProductUnitConversionQuantity += detail.ProductUnitConversionQuantity;
            packageInfo.ProductUnitConversionRemaining += detail.ProductUnitConversionQuantity;
            return GeneralCode.Success;
        }

        private async Task<PackageEntity> AppendToDefaultPackage(InventoryEntity inventory, InventoryDetail detail)
        {
            var ensureDefaultPackage = await _stockDbContext.Package
                                          .FirstOrDefaultAsync(p =>
                                              p.StockId == inventory.StockId
                                              && p.ProductId == detail.ProductId
                                              && p.PrimaryUnitId == detail.PrimaryUnitId
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
                    StockId = inventory.StockId,
                    ProductId = detail.ProductId,
                    PrimaryUnitId = detail.PrimaryUnitId,
                    PrimaryQuantity = 0,
                    ProductUnitConversionId = detail.ProductUnitConversionId,
                    ProductUnitConversionQuantity = 0,
                    PrimaryQuantityWaiting = 0,
                    PrimaryQuantityRemaining = 0,
                    ProductUnitConversionWaitting = 0,
                    ProductUnitConversionRemaining = 0,
                    Date = null,
                    ExpiryTime = null,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _stockDbContext.Package.AddAsync(ensureDefaultPackage);
            }

            ensureDefaultPackage.PrimaryQuantity += detail.PrimaryQuantity;
            ensureDefaultPackage.PrimaryQuantityRemaining += detail.PrimaryQuantity;
            ensureDefaultPackage.ProductUnitConversionQuantity += detail.ProductUnitConversionQuantity;
            ensureDefaultPackage.ProductUnitConversionRemaining += detail.ProductUnitConversionQuantity;

            await _stockDbContext.SaveChangesAsync();

            return ensureDefaultPackage;

        }

        private async Task<ServiceResult<PackageEntity>> CreateNewPackage(InventoryEntity inventory, InventoryDetail detail)
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
                StockId = inventory.StockId,
                ProductId = detail.ProductId,
                PrimaryUnitId = detail.PrimaryUnitId,
                PrimaryQuantity = detail.PrimaryQuantity,
                ProductUnitConversionId = detail.ProductUnitConversionId,
                ProductUnitConversionQuantity = detail.ProductUnitConversionQuantity,
                PrimaryQuantityWaiting = 0,
                PrimaryQuantityRemaining = detail.PrimaryQuantity,
                ProductUnitConversionWaitting = 0,
                ProductUnitConversionRemaining = detail.ProductUnitConversionQuantity,
                Date = inventory.DateUtc,
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
            var inventoryDetails = _stockDbContext.InventoryDetail.Where(d => d.InventoryId == inventory.InventoryId).ToList();

            var fromPackageIds = inventoryDetails.Select(d => d.FromPackageId).ToList();

            var fromPackages = _stockDbContext.Package.Where(p => fromPackageIds.Contains(p.PackageId)).ToList();

            foreach (var detail in inventoryDetails)
            {
                var fromPackageInfo = fromPackages.FirstOrDefault(f => f.PackageId == detail.FromPackageId);
                if (fromPackageInfo == null) return PackageErrorCode.PackageNotFound;

                var stockProductInfo = await EnsureStockProduct(inventory.StockId, detail.ProductId, detail.PrimaryUnitId, detail.ProductUnitConversionId);

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
                fromPackageInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
                stockProductInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                detail.IsDeleted = true;
                detail.UpdatedDatetimeUtc = DateTime.UtcNow;
            }

            return GeneralCode.Success;
        }

        private async Task<Enum> RollBackInventoryInput(Inventory inventory)
        {
            var inventoryDetails = _stockDbContext.InventoryDetail.Where(d => d.InventoryId == inventory.InventoryId).ToList();

            var toPackageIdList = inventoryDetails.Select(d => d.ToPackageId).ToList();

            var toPackagesList = _stockDbContext.Package.Where(p => toPackageIdList.Contains(p.PackageId)).ToList();

            foreach (var detail in inventoryDetails)
            {
                var packageInfo = toPackagesList.FirstOrDefault(f => f.PackageId == detail.ToPackageId);
                if (packageInfo == null) return PackageErrorCode.PackageNotFound;

                var stockProductInfo = await EnsureStockProduct(inventory.StockId, detail.ProductId, detail.PrimaryUnitId, detail.ProductUnitConversionId);

                if (!inventory.IsApproved)
                {

                }
                else
                {
                    packageInfo.PrimaryQuantityRemaining -= detail.PrimaryQuantity;
                    packageInfo.ProductUnitConversionRemaining -= detail.ProductUnitConversionQuantity;

                    stockProductInfo.PrimaryQuantityRemaining -= detail.PrimaryQuantity;
                    stockProductInfo.ProductUnitConversionRemaining -= detail.ProductUnitConversionQuantity;
                }
                packageInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
                stockProductInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                detail.IsDeleted = true;
                detail.UpdatedDatetimeUtc = DateTime.UtcNow;
            }
            return GeneralCode.Success;
        }

        private object GetInventoryInfoForLog(VErp.Infrastructure.EF.StockDB.Inventory inventoryObj)
        {
            return inventoryObj;
        }

        private string CreatePackageCode(string inventoryCode, string productCode, DateTime productManufactureDateTimeUtc)
        {
            var packageCode = string.Format("{0}-{1}-{2},", inventoryCode, productCode, productManufactureDateTimeUtc.ToString("YYYYMMdd"));
            //var package = await _objectGenCodeService.GenerateCode(EnumObjectType.Package);
            return packageCode;
        }

        private async Task<Enum> ProcessExcelSheet(List<ISheet> sheetList, InventoryOpeningBalanceInputModel model, int currentUserId)
        {
            try
            {   
                foreach (var sheet in sheetList)
                {
                    var inventoryInputList = new List<InventoryInModel>();
                    InventoryInModel inventoryInputModel = new InventoryInModel
                    {
                        InProducts = new List<InventoryInProductModel>(32)
                    };
                    for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);
                        if (row == null) continue;
                        
                        var currentCateName = string.Empty;
                        var isNewCate = false;

                        ProductCate currentCateObj = null;
                        for (int j = 0; j < 14; j++)
                        {
                            if (j == 0)
                            {
                                var cell = row.GetCell(j);
                                var cateName = cell.StringCellValue;
                                if (!string.IsNullOrEmpty(cateName))
                                {
                                    currentCateName = cateName;                                    

                                    var checkExistCate = _stockDbContext.ProductCate.Any(q => q.ProductCateName.ToLower().Trim() == currentCateName.ToLower().Trim());
                                    if (!checkExistCate)
                                    {
                                        var newCate = new ProductCate
                                        {
                                            ProductCateName = currentCateName,
                                            ParentProductCateId = null,
                                            CreatedDatetimeUtc = DateTime.UtcNow,
                                            UpdatedDatetimeUtc = DateTime.UtcNow
                                            ,
                                            IsDeleted = false
                                        };
                                        await _stockDbContext.ProductCate.AddAsync(newCate);
                                        await _stockDbContext.SaveChangesAsync();

                                        currentCateObj = newCate;
                                    }
                                    else
                                        currentCateObj = _stockDbContext.ProductCate.AsNoTracking().FirstOrDefault(q => q.ProductCateName.ToLower().Trim() == currentCateName.ToLower().Trim());

                                    isNewCate = true;
                                }
                                else
                                {
                                    isNewCate = false;
                                }
                            }
                            else if (j == 1)
                            {
                                var cell = row.GetCell(j);
                                var productCode = cell.StringCellValue;
                                Product product = null;

                                #region Thông tin đơn vị tính
                                var cellUnit = row.GetCell(3);
                                var cellUnitAlt = row.GetCell(8);
                                var unitName = cellUnit.StringCellValue;
                                var unitAltName = cellUnitAlt.StringCellValue ?? string.Empty;
                                Unit unitObj = null;

                                var checkExistUnitCode = _masterDBContext.Unit.Any(q => q.UnitName.Trim() == unitName.Trim());
                                if (!checkExistUnitCode)
                                {
                                    unitObj = new Unit { UnitName = unitName, IsDeleted = false, CreatedDatetimeUtc = DateTime.UtcNow, UpdatedDatetimeUtc = DateTime.UtcNow };
                                    await _masterDBContext.Unit.AddAsync(unitObj);
                                    await _masterDBContext.SaveChangesAsync();

                                }
                                else
                                {
                                    unitObj = _masterDBContext.Unit.AsNoTracking().FirstOrDefault(q => q.UnitName.Trim() == unitName.Trim());
                                }
                                #endregion

                                if (!string.IsNullOrEmpty(productCode))
                                {
                                    var checkExistProductCode = _stockDbContext.Product.Any(q => q.ProductCode.Trim() == productCode.Trim());
                                    if (!checkExistProductCode)
                                    {
                                        var productName = row.GetCell(2).StringCellValue;
                                        var unitPrice = (decimal?)(row.GetCell(6).NumericCellValue);

                                        product = new Product
                                        {
                                            ProductCode = productCode,
                                            ProductName = productName ?? string.Empty,
                                            IsCanBuy = true,
                                            IsCanSell = true,
                                            ProductTypeId = null,
                                            ProductCateId = currentCateObj != null ? currentCateObj.ProductCateId : 0,
                                            BarcodeStandardId = null,
                                            BarcodeConfigId = null,
                                            Barcode = null,
                                            UnitId = unitObj.UnitId,
                                            EstimatePrice = unitPrice,
                                            MainImageFileId = null,
                                            CreatedDatetimeUtc = DateTime.UtcNow,
                                            UpdatedDatetimeUtc = DateTime.UtcNow
                                            ,
                                            IsDeleted = false
                                        };
                                        await _stockDbContext.Product.AddAsync(product);
                                        await _stockDbContext.SaveChangesAsync();

                                        #region Thông tin sản phẩm bổ sung
                                        var specification = row.GetCell(5).StringCellValue;
                                        var newProductExtObj = new ProductExtraInfo
                                        {
                                            ProductId = product.ProductId,
                                            Specification = specification,
                                            Description = string.Empty,
                                            IsDeleted = false,
                                        };
                                        await _stockDbContext.ProductExtraInfo.AddAsync(newProductExtObj);
                                        await _stockDbContext.SaveChangesAsync();

                                        #endregion
                                    }
                                    else
                                    {
                                        var unitPrice = (decimal?)(row.GetCell(6).NumericCellValue);
                                        var specification = row.GetCell(5).StringCellValue;

                                        product = _stockDbContext.Product.FirstOrDefault(q => q.ProductCode.Trim() == productCode.Trim());
                                        product.UnitId = unitObj.UnitId;
                                        product.EstimatePrice = unitPrice;
                                        var productExtObj = _stockDbContext.ProductExtraInfo.FirstOrDefault(q => q.ProductId == product.ProductId);
                                        productExtObj.Specification = specification;

                                        await _stockDbContext.SaveChangesAsync();
                                    }

                                    var qTy = row.GetCell(4).NumericCellValue;

                                    inventoryInputModel.InProducts.Add(
                                        new InventoryInProductModel
                                        {
                                            ProductId = product.ProductId,
                                            ProductUnitConversionId = null,
                                            PrimaryQuantity = (decimal)qTy,
                                            UnitPrice = product.EstimatePrice ?? 0,
                                            RefObjectTypeId = null,
                                            RefObjectId = null,
                                            RefObjectCode = currentCateName,
                                            ToPackageId = null,
                                            PackageOptionId = EnumPackageOption.NoPackageManager
                                        }
                                    );
                                }                                
                            }
                            else
                                continue; // Tạm thời không đọc các cột phụ
                        }                       
                    }

                    #region Tạo và xửa lý phiếu
                    if(inventoryInputModel.InProducts.Count > 0)
                    {
                        
                        var groupList = inventoryInputModel.InProducts.GroupBy(g => g.RefObjectCode).ToList();

                        foreach (var g in groupList)
                        {
                            var details = g.ToList();
                            var newInventory = new InventoryInModel() { InProducts = details };
                            newInventory.StockId = model.StockId;
                            newInventory.InventoryCode = string.Format("PN_TonDau_{0}", DateTime.UtcNow.ToString("ddMMyyyyHHmmss"));
                            newInventory.DateUtc = model.IssuedDate;
                            newInventory.Shipper = string.Empty;
                            newInventory.Content = model.Description;
                            newInventory.CustomerId = null;
                            newInventory.Department = string.Empty;
                            newInventory.StockKeeperUserId = null;
                            newInventory.DeliveryCode = string.Empty;
                            newInventory.FileIdList = null;

                            inventoryInputList.Add(newInventory);
                        }
                    }

                    if(inventoryInputList.Count > 0)
                    {
                        foreach (var item in inventoryInputList)
                        {
                            var ret  = await AddInventoryInput(currentUserId,item);
                            await ApproveInventoryInput(ret.Data, currentUserId);
                        }
                    }
                    #endregion
                }


                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessExcelSheet");
                return GeneralCode.InternalError;
            }

        }

        #endregion
    }
}
