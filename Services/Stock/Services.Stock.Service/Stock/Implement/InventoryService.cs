﻿using Microsoft.EntityFrameworkCore;
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
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.FileResources;
using PackageEntity = VErp.Infrastructure.EF.StockDB.Package;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public partial class InventoryService : IInventoryService
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


        public InventoryService(MasterDBContext masterDBContext, StockDBContext stockContext
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
        public async Task<PageData<InventoryOutput>> GetList(string keyword, int stockId = 0, EnumInventoryType type = 0, string beginTime = null, string endTime = null, int page = 1, int size = 10)
        {
            var bTime = DateTime.MinValue;
            var eTime = DateTime.MinValue;

            if (!string.IsNullOrEmpty(beginTime))
            {
                if (!DateTime.TryParseExact(beginTime, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out bTime))
                {
                    return null;
                }
            }
            if (!string.IsNullOrEmpty(endTime))
            {
                if (!DateTime.TryParseExact(endTime, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out eTime))
                {
                    return null;
                }
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
                query = query.Where(q => q.DateUtc >= bTime && q.DateUtc <= eTime);
            }
            else
            {
                if (bTime != DateTime.MinValue)
                {
                    query = query.Where(q => q.DateUtc >= bTime);
                }
                if (eTime != DateTime.MinValue)
                {
                    query = query.Where(q => q.DateUtc <= eTime);
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
                    BillCode = item.BillCode,
                    BillSerial = item.BillSerial,
                    BillDate = item.BillDate,
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
                    InventoryDetailOutputList = null,
                    FileList = null,
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
                        OrderCode = details.OrderCode,
                        POCode = details.Pocode,
                        ProductionOrderCode = details.ProductionOrderCode,

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
                    BillCode = inventoryObj.BillCode,
                    BillSerial = inventoryObj.BillSerial,
                    BillDate = inventoryObj.BillDate,
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
        public async Task<ServiceResult<long>> AddInventoryInput(int currentUserId, InventoryInModel req, bool IsFreeStyle = false)
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
                var billDate = DateTime.MinValue;
                if (!string.IsNullOrEmpty(req.BillDate))
                {
                    DateTime.TryParseExact(req.DateUtc, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out billDate);
                }

                var validInventoryDetails = await ValidateInventoryIn(false, req, IsFreeStyle);

                if (!validInventoryDetails.Code.IsSuccess())
                {
                    return validInventoryDetails.Code;
                }

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var totalMoney = InputCalTotalMoney(validInventoryDetails.Data);

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
                            BillCode = req.BillCode,
                            BillSerial = req.BillSerial,
                            BillDate = billDate == DateTime.MinValue ? null : (DateTime?)billDate,
                            TotalMoney = totalMoney,
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
        public async Task<ServiceResult<long>> AddInventoryOutput(int currentUserId, InventoryOutModel req, bool IsFreeStyle = false)
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
                            BillCode = string.Empty,
                            BillSerial = string.Empty,
                            BillDate = null,
                            CreatedByUserId = currentUserId,
                            UpdatedByUserId = currentUserId,
                            CreatedDatetimeUtc = DateTime.Now,
                            UpdatedDatetimeUtc = DateTime.Now,
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
            var billDate = DateTime.MinValue;
            if (!string.IsNullOrEmpty(req.BillDate))
            {
                DateTime.TryParseExact(req.DateUtc, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out billDate);
            }

            try
            {
                if (inventoryId <= 0)
                {
                    return InventoryErrorCode.InventoryNotFound;
                }

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
                        var originalObj = GetInventoryInfoForLog(inventoryObj);

                        //inventoryObj.StockId = req.StockId; Khong cho phep sua kho
                        inventoryObj.InventoryCode = req.InventoryCode;
                        inventoryObj.DateUtc = issuedDate;
                        inventoryObj.Shipper = req.Shipper;
                        inventoryObj.Content = req.Content;
                        inventoryObj.CustomerId = req.CustomerId;
                        inventoryObj.Department = req.Department;
                        inventoryObj.StockKeeperUserId = req.StockKeeperUserId;
                        inventoryObj.BillCode = req.BillCode;
                        inventoryObj.BillSerial = req.BillSerial;
                        inventoryObj.BillDate = billDate == DateTime.MinValue ? null : (DateTime?)billDate;
                        inventoryObj.UpdatedByUserId = currentUserId;
                        inventoryObj.UpdatedDatetimeUtc = DateTime.Now;

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
                        var messageLog = string.Format("Cập nhật phiếu nhập kho, mã: {0}", inventoryObj.InventoryCode);
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

                        if (inventoryObj.StockId != req.StockId)
                        {
                            trans.Rollback();
                            return InventoryErrorCode.CanNotChangeStock;
                        }

                        var originalObj = GetInventoryInfoForLog(inventoryObj);

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
                        inventoryObj.DateUtc = issuedDate;
                        inventoryObj.CustomerId = req.CustomerId;
                        inventoryObj.Department = req.Department;
                        inventoryObj.StockKeeperUserId = req.StockKeeperUserId;
                        inventoryObj.IsApproved = false;
                        inventoryObj.UpdatedByUserId = currentUserId;
                        inventoryObj.UpdatedDatetimeUtc = DateTime.Now;


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
                        var messageLog = string.Format("Cập nhật phiếu xuất kho, mã:", inventoryObj.InventoryCode);
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


            var dataBefore = objLog.JsonSerialize();

            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    inventoryObj.IsDeleted = true;
                    //inventoryObj.IsApproved = false;
                    inventoryObj.UpdatedByUserId = currentUserId;
                    inventoryObj.UpdatedDatetimeUtc = DateTime.UtcNow;

                    _activityService.CreateActivityAsync(EnumObjectType.Inventory, inventoryObj.InventoryId, string.Format("Xóa phiếu nhập kho, mã phiếu {0}", inventoryObj.InventoryCode), dataBefore, null);
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


            var dataBefore = objLog.JsonSerialize();

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

                    _activityService.CreateActivityAsync(EnumObjectType.Inventory, inventoryObj.InventoryId, string.Format("Xóa phiếu xuất kho, mã phiếu {0}", inventoryObj.InventoryCode), dataBefore, null);
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
                if (inventoryId < 0)
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

                        var r = await ProcessInventoryInputApprove(inventoryObj.StockId, inventoryObj.DateUtc, inventoryDetails);
                        if (!r.IsSuccess())
                        {
                            trans.Rollback();
                            return r;
                        }

                        trans.Commit();

                        var objLog = GetInventoryInfoForLog(inventoryObj);
                        var messageLog = $"Duyệt phiếu nhập kho, mã: {inventoryObj.InventoryCode}";
                        _activityService.CreateActivityAsync(EnumObjectType.Inventory, inventoryObj.InventoryId, messageLog, originalObj.JsonSerialize(), objLog);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApproveInventoryInput");
                return GeneralCode.InternalError;
            }
        }


        private async Task<Enum> ProcessInventoryInputApprove(int stockId, DateTime date, IList<InventoryDetail> inventoryDetails)
        {
            var inputTransfer = new List<InventoryDetailToPackage>();
            foreach (var item in inventoryDetails)
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
                            var defaultPackge = await AppendToDefaultPackage(stockId, item);
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
                        inventoryObj.UpdatedDatetimeUtc = DateTime.Now;

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
                var productInStockQuery = (
                    from s in _stockDbContext.StockProduct
                    join p in _stockDbContext.Product on s.ProductId equals p.ProductId
                    where stockIdList.Contains(s.StockId)// && s.PrimaryQuantityRemaining > 0
                    group 0 by p into p

                    select p.Key
                    );
                    //.Distinct();

                if (!string.IsNullOrEmpty(keyword))
                    productInStockQuery = productInStockQuery.Where(q => q.ProductName.Contains(keyword) || q.ProductCode.Contains(keyword));

                var total = productInStockQuery.Count();
                var pagedData = productInStockQuery.AsNoTracking().Skip((page - 1) * size).Take(size).ToList();

                var productIdList = pagedData.Select(q => q.ProductId).ToList();
                var productExtraData = _stockDbContext.ProductExtraInfo.AsNoTracking().Where(q => productIdList.Contains(q.ProductId)).ToList();
                var unitIdList = pagedData.Select(q => q.UnitId).Distinct().ToList();
                var unitOutputList = await _unitService.GetListByIds(unitIdList);

                var stockProductData = _stockDbContext.StockProduct.AsNoTracking().Where(q => stockIdList.Contains(q.StockId)).Where(q => productIdList.Contains(q.ProductId)).ToList();

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
                        EstimatePrice = item.EstimatePrice ?? 0,
                        StockProductModelList = stockProductData.Where(q => q.ProductId == item.ProductId).Select(q => new StockProductOutput
                        {
                            StockId = q.StockId,
                            ProductId = q.ProductId,
                            PrimaryUnitId = q.PrimaryUnitId,
                            PrimaryQuantityRemaining = q.PrimaryQuantityRemaining,
                            ProductUnitConversionId = q.ProductUnitConversionId,
                            ProductUnitConversionRemaining = q.ProductUnitConversionRemaining
                        }).ToList()
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
                        Date = item.Date,
                        ExpiryTime = item.ExpiryTime,
                        PrimaryUnitId = item.PrimaryUnitId,
                        ProductUnitConversionId = item.ProductUnitConversionId,
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
                if (!string.IsNullOrEmpty(keyword))
                {
                    productWithoutStockValidationQuery = productWithoutStockValidationQuery.Where(q => q.ProductName.Contains(keyword) || q.ProductCode.Contains(keyword));
                }

                var productQuery = productWithStockValidationQuery.Union(productWithoutStockValidationQuery);

                var total = productQuery.Count();
                productQuery = productQuery.AsNoTracking().OrderBy(q => q.ProductId).Skip((page - 1) * size).Take(size);

                var pagedData = productQuery.ToList();
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
                            PrimaryUnitId = q.PrimaryUnitId,
                            PrimaryQuantityRemaining = q.PrimaryQuantityRemaining,
                            ProductUnitConversionId = q.ProductUnitConversionId,
                            ProductUnitConversionRemaining = q.ProductUnitConversionRemaining
                        }).ToList()
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


        #region Private helper method

        private async Task<ServiceResult<IList<InventoryDetail>>> ValidateInventoryIn(bool isApproved, InventoryInModel req, bool IsFreeStyle = false)
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

                var primaryQty = details.PrimaryQuantity;

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
                if (IsFreeStyle == false)
                {
                    var productUnitConversionInfo = productUnitConversions.FirstOrDefault(c => c.ProductUnitConversionId == details.ProductUnitConversionId);
                    if (productUnitConversionInfo == null)
                    {
                        return ProductUnitConversionErrorCode.ProductUnitConversionNotFound;
                    }
                    primaryQty = Utils.GetPrimaryQuantityFromProductUnitConversionQuantity(details.ProductUnitConversionQuantity, productUnitConversionInfo.FactorExpression);
                    if (!isApproved && primaryQty <= 0)
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
                    CreatedDatetimeUtc = DateTime.Now,
                    UpdatedDatetimeUtc = DateTime.Now,
                    IsDeleted = false,
                    PrimaryUnitId = productInfo.UnitId,
                    PrimaryQuantity = primaryQty,
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
                    PackageOptionId = (int)details.PackageOptionId
                });
            }
            return inventoryDetailList;
        }

        private async Task<ServiceResult<IList<InventoryDetail>>> ProcessInventoryOut(Inventory inventory, InventoryOutModel req, bool IsFreeStyle = false)
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

                var primaryQualtity = details.PrimaryQuantity;

                if (details.ProductUnitConversionId != null && details.ProductUnitConversionId > 0)
                {
                    if (IsFreeStyle == false)
                    {
                        var productUnitConversionInfo = productUnitConversions.FirstOrDefault(c => c.ProductUnitConversionId == details.ProductUnitConversionId);
                        if (productUnitConversionInfo == null)
                        {
                            return ProductUnitConversionErrorCode.ProductUnitConversionNotFound;
                        }

                        if (details.ProductUnitConversionQuantity <= 0)
                        {
                            return GeneralCode.InvalidParams;
                        }

                        if (details.ProductUnitConversionQuantity > fromPackageInfo.ProductUnitConversionRemaining)
                        {
                            return InventoryErrorCode.NotEnoughQuantity;
                        }
                        if (primaryQualtity == 0)
                        {
                            primaryQualtity = Utils.GetPrimaryQuantityFromProductUnitConversionQuantity(details.ProductUnitConversionQuantity, productUnitConversionInfo.FactorExpression);
                            if (!(primaryQualtity > 0))
                            {
                                return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                            }
                        }
                    }
                }
                inventoryDetailList.Add(new InventoryDetail
                {
                    InventoryId = inventory.InventoryId,
                    ProductId = details.ProductId,
                    CreatedDatetimeUtc = DateTime.Now,
                    UpdatedDatetimeUtc = DateTime.Now,
                    IsDeleted = false,
                    PrimaryUnitId = fromPackageInfo.PrimaryUnitId,
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

        private async Task UpdateStockProduct(int stockId, InventoryDetail detail, EnumInventoryType type = EnumInventoryType.Input)
        {
            var stockProductInfo = await EnsureStockProduct(stockId, detail.ProductId, detail.PrimaryUnitId, detail.ProductUnitConversionId);
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
            return GeneralCode.Success;
        }

        private async Task<PackageEntity> AppendToDefaultPackage(int stockId, InventoryDetail detail)
        {
            var ensureDefaultPackage = await _stockDbContext.Package
                                          .FirstOrDefaultAsync(p =>
                                              p.StockId == stockId
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
                    StockId = stockId,
                    ProductId = detail.ProductId,
                    PrimaryUnitId = detail.PrimaryUnitId,
                    //PrimaryQuantity = 0,
                    ProductUnitConversionId = detail.ProductUnitConversionId,
                    //ProductUnitConversionQuantity = 0,
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
                PrimaryUnitId = detail.PrimaryUnitId,
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

        #endregion
    }
}