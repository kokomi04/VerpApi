﻿using Microsoft.EntityFrameworkCore;
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
using VErp.Services.Stock.Model.Inventory.OpeningBalance;
using VErp.Services.Stock.Model.Stock;

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
                var billDate = DateTime.MinValue;
                if (!string.IsNullOrEmpty(req.BillDate))
                {
                    DateTime.TryParseExact(req.DateUtc, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out billDate);
                }

                var validInventoryDetails = await ValidateInventoryIn(false, req);

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
                        inventoryObj.UpdatedDatetimeUtc = DateTime.Now;

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
                        inventoryObj.UpdatedDatetimeUtc = DateTime.Now;

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
                    var ret = await _fileService.GetFileAndPath(fileId);
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
                    var sheetList = new List<ISheet>(4);

                    using (var fs = new FileStream(ret.Data.physicalPath, FileMode.Open, FileAccess.Read))
                    {
                        if (fs != null)
                        {
                            switch (fileExtension)
                            {
                                case "xls":
                                    {

                                        wb = new HSSFWorkbook(fs);
                                        var numberOfSheet = wb.NumberOfSheets;
                                        for (var i = 0; i < numberOfSheet; i++)
                                        {
                                            var sheetName = wb.GetSheetAt(i).SheetName;
                                            var sheet = (HSSFSheet)wb.GetSheet(sheetName);
                                            sheetList.Add(sheet);
                                        }
                                        break;
                                    }
                                case "xlsx":
                                    {
                                        wb = new XSSFWorkbook(fs);
                                        var numberOfSheet = wb.NumberOfSheets;
                                        for (var i = 0; i < numberOfSheet; i++)
                                        {
                                            var sheetName = wb.GetSheetAt(i).SheetName;
                                            var sheet = (XSSFSheet)wb.GetSheet(sheetName);
                                            sheetList.Add(sheet);
                                        }
                                        break;
                                    }
                                default:
                                    continue;
                            }
                            #region Process wb and sheet
                            var sheetListCount = sheetList.Count;
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

        private async Task<ServiceResult<IList<InventoryDetail>>> ValidateInventoryIn(bool isApproved, InventoryInModel req)
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

                if (details.ProductUnitConversionQuantity < 0)
                {
                    return GeneralCode.InvalidParams;
                }
                if (details.ProductUnitConversionId != null && details.ProductUnitConversionId > 0)
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

                var primaryQualtity = details.PrimaryQuantity;

                if (details.ProductUnitConversionId != null && details.ProductUnitConversionId > 0)
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

                    if (details.ProductUnitConversionQuantity > fromPackageInfo.ProductUnitConversionQuantity)
                    {
                        return InventoryErrorCode.NotEnoughQuantity;
                    }

                    var expression = $"({details.ProductUnitConversionQuantity})*({productUnitConversionInfo.FactorExpression})";

                    primaryQualtity = Utils.Eval(expression);
                    if (!(primaryQualtity > 0))
                    {
                        return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
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

                    var totalRowCount = sheet.LastRowNum + 1;
                    var excelModel = new List<OpeningBalanceModel>(totalRowCount);

                    var newProductList = new List<Product>(totalRowCount);
                    var oldProductList = new List<Product>(totalRowCount);
                    var newProductExtraInfoList = new List<ProductExtraInfo>(totalRowCount);
                    var oldProductExtraInfoList = new List<ProductExtraInfo>(totalRowCount);

                    var newUnit1List = new List<UnitModel>(totalRowCount);
                    var newUnit2List = new List<UnitModel>(totalRowCount);
                    var newInventoryInputModel = new List<InventoryInProductExtendModel>(totalRowCount);

                    var currentCateName = string.Empty;
                    var currentCatePrefixCode = string.Empty;
                    var cateName = string.Empty;
                    var catePrefixCode = string.Empty;
                    try
                    {
                        for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                        {
                            var row = sheet.GetRow(i);
                            if (row == null) continue;

                            var cellCateName = row.GetCell(0);
                            var cellCatePreifxCode = row.GetCell(1);
                            cateName = cellCateName != null ? cellCateName.StringCellValue.Trim() : string.Empty;
                            catePrefixCode = cellCatePreifxCode != null ? cellCatePreifxCode.StringCellValue.Trim() : string.Empty;
                            if (!string.IsNullOrEmpty(cateName))
                            {
                                currentCateName = cateName;
                            }
                            if (!string.IsNullOrEmpty(catePrefixCode))
                            {
                                currentCatePrefixCode = catePrefixCode;
                            }
                            var cellProductCode = row.GetCell(2);
                            if (cellProductCode == null)
                                continue;
                            var productCode = cellProductCode != null ? cellProductCode.StringCellValue.Trim() : string.Empty;
                            if (string.IsNullOrEmpty(productCode))
                                continue;
                            #region Get All Cell value
                            var productName = row.GetCell(3) != null ? row.GetCell(3).StringCellValue.Trim() : string.Empty;
                            var cellUnit = row.GetCell(4);
                            var cellUnitAlt = row.GetCell(9);
                            var unitName = cellUnit != null ? cellUnit.StringCellValue.Trim() : string.Empty;
                            var unitAltName = cellUnitAlt != null ? cellUnitAlt.StringCellValue.Trim() : string.Empty;

                            var qTy = row.GetCell(5) != null ? row.GetCell(5).NumericCellValue : 0;
                            var unitPrice = row.GetCell(6) != null ? (decimal)row.GetCell(6).NumericCellValue : 0;
                            var qTy2 = row.GetCell(11) != null ? row.GetCell(11).NumericCellValue : 0;
                            var factor = row.GetCell(10) != null ? row.GetCell(10).NumericCellValue : 0;
                            var specification = row.GetCell(8) != null ? row.GetCell(8).StringCellValue : string.Empty;
                            var heightSize = row.GetCell(13) != null ? row.GetCell(13).NumericCellValue : 0;
                            var widthSize = row.GetCell(14) != null ? row.GetCell(14).NumericCellValue : 0;
                            var longSize = row.GetCell(15) != null ? row.GetCell(15).NumericCellValue : 0;

                            var cellItem = new OpeningBalanceModel();
                            cellItem.CateName = currentCateName;
                            cellItem.CatePrefixCode = currentCatePrefixCode;
                            cellItem.ProductCode = productCode;
                            cellItem.ProductName = productName;
                            cellItem.Unit1 = unitName;
                            cellItem.Qty1 = (decimal)qTy;
                            cellItem.UnitPrice = unitPrice;
                            cellItem.Specification = specification;
                            cellItem.Unit2 = unitAltName;
                            cellItem.Qty2 = (decimal)qTy2;
                            cellItem.Factor = (decimal)factor;
                            cellItem.Height = (decimal)heightSize;
                            cellItem.Width = (decimal)widthSize;
                            cellItem.Long = (decimal)longSize;

                            excelModel.Add(cellItem);
                            #endregion
                        } // end for loop
                    }
                    catch (Exception ex)
                    {
                        return GeneralCode.InternalError;
                        throw ex;
                    }
                    #region Insert & update data to DB

                    #region Cập nhật ProductCate && ProductType
                    var productCateNameData = excelModel.GroupBy(g => g.CateName).Select(q => q.First()).Where(q => !string.IsNullOrEmpty(q.CateName)).Select(q => q.CateName).ToList();
                    foreach (var item in productCateNameData)
                    {
                        var checkExistCate = _stockDbContext.ProductCate.Any(q => q.ProductCateName == item);
                        if (!checkExistCate)
                        {
                            var newCate = new ProductCate
                            {
                                ProductCateName = item,
                                ParentProductCateId = null,
                                CreatedDatetimeUtc = DateTime.Now,
                                UpdatedDatetimeUtc = DateTime.Now,
                                IsDeleted = false
                            };
                            await _stockDbContext.ProductCate.AddAsync(newCate);
                        }
                    }
                    await _stockDbContext.SaveChangesAsync();

                    // Thêm Cate prefix ProductType
                    var productCatePrefixCodeData = excelModel.GroupBy(g => g.CatePrefixCode).Select(q => q.First()).Where(q => !string.IsNullOrEmpty(q.CatePrefixCode)).Select(q => q.CatePrefixCode).ToList();
                    foreach (var item in productCatePrefixCodeData)
                    {
                        var checkExistProductType = _stockDbContext.ProductType.Any(q => q.IdentityCode == item);
                        if (!checkExistProductType)
                        {
                            var newProductType = new ProductType
                            {
                                ProductTypeName = item,
                                ParentProductTypeId = null,
                                IdentityCode = catePrefixCode,
                                CreatedDatetimeUtc = DateTime.Now,
                                UpdatedDatetimeUtc = DateTime.Now,
                                IsDeleted = false
                            };
                            await _stockDbContext.ProductType.AddAsync(newProductType);
                            await _stockDbContext.SaveChangesAsync();
                        }
                    }
                    await _stockDbContext.SaveChangesAsync();
                    #endregion

                    #region Cập nhật đơn vị tính chính & phụ
                    var unit1Data = excelModel.GroupBy(g => g.Unit1).Select(q => q.First()).Where(q => !string.IsNullOrEmpty(q.Unit1)).Select(q => q.Unit1).ToList();
                    var unit1DataList = new List<Unit>(unit1Data.Count);
                    foreach (var item in unit1Data)
                    {
                        var checkExistUnitCode = _masterDBContext.Unit.Any(q => q.UnitName == item);
                        if (!checkExistUnitCode)
                        {
                            var unitEntity = new Unit { UnitName = item, IsDeleted = false, CreatedDatetimeUtc = DateTime.Now, UpdatedDatetimeUtc = DateTime.Now };
                            await _masterDBContext.Unit.AddAsync(unitEntity);
                            unit1DataList.Add(unitEntity);
                        }
                    }
                    if (unit1DataList.Count > 0)
                        await _masterDBContext.SaveChangesAsync();

                    var unit2Data = excelModel.GroupBy(g => g.Unit2).Select(q => q.First()).Where(q => !string.IsNullOrEmpty(q.Unit2)).Select(q => q.Unit2).ToList();
                    var unit2DataList = new List<Unit>(unit2Data.Count);
                    foreach (var item in unit2Data)
                    {
                        var checkExistUnitCode = _masterDBContext.Unit.Any(q => q.UnitName == item);
                        if (!checkExistUnitCode)
                        {
                            var unitEntity = new Unit { UnitName = item, IsDeleted = false, CreatedDatetimeUtc = DateTime.Now, UpdatedDatetimeUtc = DateTime.Now };
                            await _masterDBContext.Unit.AddAsync(unitEntity);
                            unit2DataList.Add(unitEntity);
                        }
                    }
                    if (unit2DataList.Count > 0)
                        await _masterDBContext.SaveChangesAsync();

                    var unitDataList = unit1DataList.Union(unit2DataList).ToList();
                    #endregion

                    #region Cập nhật sản phẩm
                    foreach (var item in excelModel)
                    {
                        if (!string.IsNullOrEmpty(item.ProductCode))
                        {
                            var checkExistProductCode = _stockDbContext.Product.Any(q => q.ProductCode == item.ProductCode);
                            var productCateObj = _stockDbContext.ProductCate.AsNoTracking().FirstOrDefault(q => q.ProductCateName == item.CateName);
                            var productTypeObj = _stockDbContext.ProductType.AsNoTracking().FirstOrDefault(q => q.IdentityCode == item.CatePrefixCode);
                            var unitObj = _masterDBContext.Unit.AsNoTracking().FirstOrDefault(q => q.UnitName == item.Unit1);
                            if (!checkExistProductCode)
                            {
                                var productEntity = new Product
                                {
                                    ProductCode = item.ProductCode,
                                    ProductName = item.ProductName,
                                    IsCanBuy = true,
                                    IsCanSell = true,
                                    MainImageFileId = null,
                                    ProductTypeId = productTypeObj != null ? (int?)productTypeObj.ProductTypeId : null,
                                    ProductCateId = productCateObj != null ? productCateObj.ProductCateId : 0,
                                    BarcodeStandardId = null,
                                    BarcodeConfigId = null,
                                    Barcode = null,
                                    UnitId = unitObj != null ? unitObj.UnitId : 0,
                                    EstimatePrice = item.UnitPrice,
                                    Long = item.Long,
                                    Width = item.Width,
                                    Height = item.Height,
                                    CreatedDatetimeUtc = DateTime.Now,
                                    UpdatedDatetimeUtc = DateTime.Now,
                                    IsDeleted = false
                                };
                                newProductList.Add(productEntity);
                            }
                            else
                            {
                                var productObj = _stockDbContext.Product.FirstOrDefault(q => q.ProductCode == item.ProductCode);
                                productObj.UnitId = unitObj.UnitId;
                                productObj.EstimatePrice = item.UnitPrice;
                                productObj.Long = item.Long;
                                productObj.Width = item.Width;
                                productObj.Height = item.Height;

                                oldProductList.Add(productObj);
                            }
                        }
                    }
                    if (newProductList.Count > 0)
                    {
                        await _stockDbContext.Product.AddRangeAsync(newProductList);
                        await _stockDbContext.SaveChangesAsync();
                    }
                    if (oldProductList.Count > 0)
                    {
                        _stockDbContext.Product.UpdateRange(oldProductList);
                        await _stockDbContext.SaveChangesAsync();
                    }
                    var productData = newProductList.Union(oldProductList).ToList();
                    #endregion

                    #region Cập nhật mô tả sản phẩm
                    foreach (var item in excelModel)
                    {
                        if (!string.IsNullOrEmpty(item.Specification) && !string.IsNullOrEmpty(item.ProductCode))
                        {
                            var productObj = productData.FirstOrDefault(q => q.ProductCode == item.ProductCode);
                            if (productObj != null)
                            {
                                var checkExist = _stockDbContext.ProductExtraInfo.Any(q => q.ProductId == productObj.ProductId);
                                if (checkExist)
                                {
                                    var productExtObj = _stockDbContext.ProductExtraInfo.FirstOrDefault(q => q.ProductId == productObj.ProductId);
                                    productExtObj.Specification = item.Specification;
                                    oldProductExtraInfoList.Add(productExtObj);
                                }
                                else
                                {
                                    var productExtraInfoEntity = new ProductExtraInfo
                                    {
                                        ProductId = productObj.ProductId,
                                        Specification = item.Specification,
                                        Description = string.Empty,
                                        IsDeleted = false
                                    };
                                    newProductExtraInfoList.Add(productExtraInfoEntity);
                                }
                            }
                        }
                    }
                    if(newProductExtraInfoList.Count > 0)
                    {
                        await _stockDbContext.ProductExtraInfo.AddRangeAsync(newProductExtraInfoList);
                        await _stockDbContext.SaveChangesAsync();
                    }
                    if (oldProductExtraInfoList.Count > 0)
                    {
                        _stockDbContext.ProductExtraInfo.UpdateRange(oldProductExtraInfoList);
                        await _stockDbContext.SaveChangesAsync();
                    }                    
                    #endregion

                    #region Cập nhật đơn vị chuyển đổi - ProductUnitConversion - Tạm thời chưa thực hiện
                    //foreach (var item in excelModel)
                    //{
                    //    if (!string.IsNullOrEmpty(item.ProductCode))
                    //    {
                    //        var productObj = productData.FirstOrDefault(q => q.ProductCode == item.ProductCode);
                    //        if (!string.IsNullOrEmpty(item.Unit1) && !string.IsNullOrEmpty(item.Unit2) && item.Factor > 0 && productObj != null)
                    //        {
                    //            var unit1 = unitDataList.FirstOrDefault(q => q.UnitName == item.Unit1);
                    //            var unit2 = unitDataList.FirstOrDefault(q => q.UnitName == item.Unit2);
                    //            if (unit1 != null && unit2 != null)
                    //            {
                    //                var checkExists = _stockDbContext.ProductUnitConversion.Any(q => q.ProductId == productObj.ProductId && q.SecondaryUnitId == unit2.UnitId);
                    //                if (checkExists)
                    //                {
                    //                    var productUnitConversionEntity = _stockDbContext.ProductUnitConversion.FirstOrDefault(q => q.ProductId == productObj.ProductId && q.SecondaryUnitId == unit2.UnitId);
                    //                }
                    //                else
                    //                {

                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                    #endregion

                    #endregion end db updating

                    #region Tạo và xửa lý phiếu

                    #region Thông tin phiếu newInventoryInputModel
                    foreach (var item in excelModel)
                    {
                        var productObj = productData.FirstOrDefault(q => q.ProductCode == item.ProductCode);
                        newInventoryInputModel.Add(
                                new InventoryInProductExtendModel
                                {
                                    ProductId = productObj != null ? productObj.ProductId : 0,
                                    ProductCode = item.ProductCode,
                                    ProductUnitConversionId = null,
                                    PrimaryQuantity = item.Qty1,
                                    ProductUnitConversionQuantity = item.Qty2,
                                    UnitPrice = item.UnitPrice,
                                    RefObjectTypeId = null,
                                    RefObjectId = null,
                                    RefObjectCode = item.CateName,
                                    ToPackageId = null,
                                    PackageOptionId = EnumPackageOption.NoPackageManager
                                }
                            );
                    }
                    #endregion

                    if (newInventoryInputModel.Count > 0)
                    {
                        var groupList = newInventoryInputModel.GroupBy(g => g.RefObjectCode).ToList();

                        foreach (var g in groupList)
                        {
                            var details = g.ToList();
                            var newInventory = new InventoryInModel();
                            newInventory.StockId = model.StockId;
                            newInventory.InventoryCode = string.Format("PN_TonDau_{0}", DateTime.Now.ToString("ddMMyyyyHHmmss"));
                            newInventory.DateUtc = model.IssuedDate;
                            newInventory.Shipper = string.Empty;
                            newInventory.Content = model.Description;
                            newInventory.CustomerId = null;
                            newInventory.Department = string.Empty;
                            newInventory.StockKeeperUserId = null;
                            newInventory.BillCode = string.Empty;
                            newInventory.BillSerial = string.Empty;
                            newInventory.BillDate = null;
                            newInventory.FileIdList = null;
                            newInventory.InProducts = new List<InventoryInProductModel>(details.Count);
                            foreach (var item in details)
                            {
                                var currentProductObj = newProductList.FirstOrDefault(q => q.ProductCode == item.ProductCode);
                                newInventory.InProducts.Add(new InventoryInProductModel
                                {
                                    ProductId = item.ProductId,
                                    ProductUnitConversionId = null,
                                    PrimaryQuantity = item.PrimaryQuantity,
                                    ProductUnitConversionQuantity = item.ProductUnitConversionQuantity,
                                    UnitPrice = item.UnitPrice,
                                    RefObjectTypeId = item.RefObjectTypeId,
                                    RefObjectId = item.RefObjectId,
                                    RefObjectCode = item.RefObjectCode,
                                    ToPackageId = null,
                                    PackageOptionId = EnumPackageOption.NoPackageManager
                                });
                            }
                            inventoryInputList.Add(newInventory);
                        }
                    }
                    if (inventoryInputList.Count > 0)
                    {
                        foreach (var item in inventoryInputList)
                        {
                            var ret = await AddInventoryInput(currentUserId, item);
                            if (ret.Data > 0)
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
