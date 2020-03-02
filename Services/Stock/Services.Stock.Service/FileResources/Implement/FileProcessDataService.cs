using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VErp.Commons.Enums.StandardEnum;
using VErp.Services.Stock.Service.Stock;
using VErp.Services.Master.Service.Customer;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Services.Master.Service.Activity;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.FileResources;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using EFCore.BulkExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory.OpeningBalance;
using Microsoft.EntityFrameworkCore;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;

namespace VErp.Services.Stock.Service.FileResources.Implement
{
    public class FileProcessDataService : IFileProcessDataService
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly StockDBContext _stockDbContext;

        private readonly IFileService _fileService;
        private readonly IInventoryService _inventoryService;

        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;

        public FileProcessDataService(MasterDBContext masterDBContext, StockDBContext stockDbContext
            , IOptions<AppSetting> appSetting
            , ILogger<FileProcessDataService> logger
            , IFileService fileService
            , IInventoryService inventoryService
            )
        {
            _masterDBContext = masterDBContext;
            _stockDbContext = stockDbContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _fileService = fileService;
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// Đọc file và cập nhật dữ liệu đối tác | khách hàng
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public async Task<Enum> ImportCustomerData(int currentUserId, long fileId)
        {
            try
            {
                if (fileId < 1)
                    return GeneralCode.InvalidParams;


                var ret = await _fileService.GetFileAndPath(fileId);
                if (ret.Data.info == null)
                    return GeneralCode.InternalError;

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
                                return GeneralCode.InvalidParams;
                        }
                        #region Process wb and sheet
                        var sheetListCount = sheetList.Count;
                        var returnResult = await ProcessCustomerExcelSheet(sheetList, currentUserId);
                        if ((GeneralCode)returnResult != GeneralCode.Success)
                        {
                            return GeneralCode.InternalError;
                        }
                        #endregion
                    }
                    else
                    {
                        return GeneralCode.InternalError;
                    }
                }
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportCustomerData");
                return GeneralCode.InternalError;
            }
        }

        /// <summary>
        /// Đọc file và cập nhật dữ liệu số dư đầu kỳ theo kho
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ServiceResult<long>> ImportInventoryInputOpeningBalance(int currentUserId, InventoryOpeningBalanceModel model)
        {
            try
            {
                var result = GeneralCode.Success;
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
                            var returnResult = await ProcessInventoryInputExcelSheet(sheetList, model, currentUserId);
                            if ((GeneralCode)returnResult != GeneralCode.Success)
                            {
                                return GeneralCode.InternalError;
                            }
                            #endregion
                        }
                        else
                        {
                            return GeneralCode.InternalError;
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportInventoryInputOpeningBalance");
                return GeneralCode.InternalError;
            }
        }

        /// <summary>
        /// Đọc file và cập nhật số liêu phiếu xuất kho
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ServiceResult<long>> ImportInventoryOutput(int currentUserId, InventoryOpeningBalanceModel model)
        {
            try
            {
                var result = GeneralCode.Success;
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
                            var returnResult = await ProcessInventoryOutputExcelSheet(sheetList, model, currentUserId);
                            if ((GeneralCode)returnResult != GeneralCode.Success)
                            {
                                return GeneralCode.InternalError;
                            }
                            #endregion
                        }
                        else
                        {
                            return GeneralCode.InternalError;
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportInventoryOutput");
                return GeneralCode.InternalError;
            }
        }

        #region Private functions

        private decimal HelperCellGetNumericValue(ICell myCell)
        {
            try
            {
                var cellValue = myCell.NumericCellValue;
                decimal ret = Convert.ToDecimal(cellValue);
                return ret;
            }
            catch
            {
                return 0;
            }
        }

        private string HelperCellGetStringValue(ICell myCell)
        {
            try
            {
                var cellValue = string.Empty;
                switch (myCell.CellType)
                {
                    case CellType.Formula:
                        cellValue = myCell.CellFormula.ToString();
                        break;
                    case CellType.Boolean:
                        cellValue = myCell.BooleanCellValue.ToString();
                        break;
                    case CellType.Numeric:
                        cellValue = myCell.NumericCellValue.ToString().Trim();
                        break;
                    default:
                        cellValue = myCell.StringCellValue.Trim();
                        break;
                }
                return cellValue;
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<Enum> ProcessCustomerExcelSheet(List<ISheet> sheetList, int currentUserId)
        {
            try
            {
                foreach (var sheet in sheetList)
                {
                    var totalRowCount = sheet.LastRowNum + 1;
                    var customerExcelModelList = new List<ExcelCustomerModel>(totalRowCount);

                    var currentCateName = string.Empty;
                    var cateName = string.Empty;
                    var code = string.Empty;
                    var name = string.Empty;
                    var address = string.Empty;
                    var taxCode = string.Empty;
                    var phoneNumber = string.Empty;
                    var webSite = string.Empty;
                    var eMail = string.Empty;
                    var description = string.Empty;
                    ExcelCustomerModel cellItem = null;

                    try
                    {
                        try
                        {
                            for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                            {
                                var row = sheet.GetRow(i);
                                if (row == null) continue;

                                cateName = HelperCellGetStringValue(row.GetCell(0));
                                if (!string.IsNullOrEmpty(cateName))
                                {
                                    currentCateName = cateName;
                                }
                                code = HelperCellGetStringValue(row.GetCell(1));
                                name = HelperCellGetStringValue(row.GetCell(2));

                                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
                                    continue;

                                #region read all cell values
                                address = HelperCellGetStringValue(row.GetCell(3));
                                taxCode = HelperCellGetStringValue(row.GetCell(4));
                                phoneNumber = HelperCellGetStringValue(row.GetCell(5));
                                webSite = HelperCellGetStringValue(row.GetCell(6));
                                eMail = HelperCellGetStringValue(row.GetCell(7));
                                description = HelperCellGetStringValue(row.GetCell(8));

                                cellItem = new ExcelCustomerModel
                                {
                                    CategoryName = currentCateName,
                                    Code = code,
                                    Name = name,
                                    Address = address,
                                    TaxCode = taxCode,
                                    PhoneNumber = phoneNumber,
                                    WebSite = webSite,
                                    Email = eMail,
                                    Description = description
                                };
                                customerExcelModelList.Add(cellItem);
                                #endregion
                            }
                        }
                        catch (Exception ex)
                        {
                            return GeneralCode.InternalError;
                            throw ex;
                        }
                        if (customerExcelModelList != null && customerExcelModelList.Count > 0)
                        {
                            var customerDataList = new List<Customer>(customerExcelModelList.Count);
                            foreach (var item in customerExcelModelList)
                            {
                                if (customerDataList.Any(q => q.CustomerCode == item.Code))
                                    continue;
                                customerDataList.Add(new Customer
                                {
                                    CustomerCode = item.Code,
                                    CustomerName = item.Name,
                                    CustomerTypeId = (item.Code.Contains("NV") || item.Code.Contains("nv")) ? 2 : 1,
                                    Address = item.Address,
                                    TaxIdNo = item.TaxCode,
                                    PhoneNumber = item.PhoneNumber,
                                    Website = item.WebSite,
                                    Email = item.Email,
                                    Description = item.Description,
                                    IsActived = true,
                                    IsDeleted = false,
                                    CreatedDatetimeUtc = DateTime.UtcNow,
                                    UpdatedDatetimeUtc = DateTime.UtcNow,
                                });
                            }
                            var readCustomerBulkConfig = new BulkConfig { UpdateByProperties = new List<string> { nameof(Customer.CustomerCode) } };
                            _masterDBContext.BulkRead<Customer>(customerDataList, readCustomerBulkConfig);
                            _masterDBContext.BulkInsertOrUpdate<Customer>(customerDataList, new BulkConfig { PreserveInsertOrder = false, SetOutputIdentity = false, PropertiesToExclude = new List<string> { nameof(Customer.CustomerStatusId) } });
                            return GeneralCode.Success;
                        }
                    }
                    catch (Exception ex)
                    {
                        return GeneralCode.InternalError;
                        throw ex;
                    }
                }
                return GeneralCode.InternalError;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessCustomerExcelSheet");
                return GeneralCode.InternalError;
            }
        }

        private async Task<Enum> ProcessInventoryInputExcelSheet(List<ISheet> sheetList, InventoryOpeningBalanceModel model, int currentUserId)
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

                    var productDataList = new List<Product>(totalRowCount);
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
                            cateName = cellCateName != null ? HelperCellGetStringValue(cellCateName) : string.Empty;
                            catePrefixCode = cellCatePreifxCode != null ? HelperCellGetStringValue(cellCatePreifxCode) : string.Empty;
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
                            var productCode = cellProductCode != null ? HelperCellGetStringValue(cellProductCode) : string.Empty;
                            if (string.IsNullOrEmpty(productCode))
                                continue;
                            #region Get All Cell value
                            var productName = row.GetCell(3) != null ? HelperCellGetStringValue(row.GetCell(3)) : string.Empty;
                            var cellUnit = row.GetCell(4);

                            var unitName = cellUnit != null ? HelperCellGetStringValue(cellUnit) : string.Empty;
                            if (string.IsNullOrEmpty(unitName))
                                continue;

                            var cellUnitAlt = row.GetCell(9);
                            var unitAltName = cellUnitAlt != null ? HelperCellGetStringValue(cellUnitAlt) : string.Empty;
                            var qTy = row.GetCell(5) != null ? HelperCellGetNumericValue(row.GetCell(5)) : 0;
                            var unitPrice = row.GetCell(6) != null ? (decimal)HelperCellGetNumericValue(row.GetCell(6)) : 0;
                            var qTy2 = row.GetCell(11) != null ? HelperCellGetNumericValue(row.GetCell(11)) : 0;
                            var factor = row.GetCell(10) != null ? HelperCellGetNumericValue(row.GetCell(10)) : 0;
                            var specification = row.GetCell(8) != null ? HelperCellGetStringValue(row.GetCell(8)) : string.Empty;
                            var heightSize = row.GetCell(13) != null ? HelperCellGetNumericValue(row.GetCell(13)) : 0;
                            var widthSize = row.GetCell(14) != null ? HelperCellGetNumericValue(row.GetCell(14)) : 0;
                            var longSize = row.GetCell(15) != null ? HelperCellGetNumericValue(row.GetCell(15)) : 0;

                            var cellItem = new OpeningBalanceModel
                            {
                                CateName = currentCateName,
                                CatePrefixCode = currentCatePrefixCode,
                                ProductCode = productCode,
                                ProductName = productName,
                                Unit1 = unitName.ToLower(),
                                Qty1 = qTy,
                                UnitPrice = unitPrice,
                                Specification = specification,
                                Unit2 = unitAltName.ToLower(),
                                Qty2 = qTy2,
                                Factor = factor,
                                Height = heightSize,
                                Width = widthSize,
                                Long = longSize
                            };
                            excelModel.Add(cellItem);
                            #endregion
                        } // end for loop
                    }
                    catch (Exception ex)
                    {
                        return GeneralCode.InternalError;
                        throw ex;
                    }

                    #region Cập nhật ProductCate && ProductType
                    var productCateNameModelList = excelModel.GroupBy(g => g.CateName).Select(q => q.First()).Where(q => !string.IsNullOrEmpty(q.CateName)).Select(q => q.CateName).ToList();
                    var productCateEntities = new List<ProductCate>(productCateNameModelList.Count);
                    foreach (var item in productCateNameModelList)
                    {
                        var exists = _stockDbContext.ProductCate.Any(q => q.ProductCateName == item);
                        if (!exists)
                        {
                            var newCate = new ProductCate
                            {
                                ProductCateName = item,
                                ParentProductCateId = null,
                                CreatedDatetimeUtc = DateTime.UtcNow,
                                UpdatedDatetimeUtc = DateTime.UtcNow,
                                IsDeleted = false
                            };
                            _stockDbContext.ProductCate.Add(newCate);
                        }
                    }
                    _stockDbContext.SaveChanges();
                    productCateEntities = _stockDbContext.ProductCate.AsNoTracking().ToList();

                    // Thêm Cate prefix ProductType
                    var productTypeModelList = excelModel.GroupBy(g => g.CatePrefixCode).Select(q => q.First()).Where(q => !string.IsNullOrEmpty(q.CatePrefixCode)).Select(q => q.CatePrefixCode).ToList();
                    var productTypeEntities = new List<ProductType>(productTypeModelList.Count);

                    foreach (var item in productTypeModelList)
                    {
                        var exists = _stockDbContext.ProductType.Any(q => q.ProductTypeName == item);
                        if (!exists)
                        {
                            var newProductType = new ProductType
                            {
                                ProductTypeName = item,
                                ParentProductTypeId = null,
                                IdentityCode = item,
                                CreatedDatetimeUtc = DateTime.UtcNow,
                                UpdatedDatetimeUtc = DateTime.UtcNow,
                                IsDeleted = false
                            };
                            _stockDbContext.ProductType.Add(newProductType);
                        }
                    }
                    _stockDbContext.SaveChanges();
                    productTypeEntities = _stockDbContext.ProductType.AsNoTracking().ToList();

                    #endregion

                    #region Cập nhật đơn vị tính chính & phụ
                    var unit1ModelList = excelModel.GroupBy(g => g.Unit1).Select(q => q.First()).Where(q => !string.IsNullOrEmpty(q.Unit1)).Select(q => q.Unit1).ToList();
                    var unit2ModelList = excelModel.GroupBy(g => g.Unit2).Select(q => q.First()).Where(q => !string.IsNullOrEmpty(q.Unit2)).Select(q => q.Unit2).ToList();
                    var unitModelList = unit1ModelList.Union(unit2ModelList).GroupBy(g => g.ToLower()).Select(q => q.First());
                    foreach (var u in unitModelList)
                    {
                        var exists = _masterDBContext.Unit.Any(q => q.UnitName == u);
                        if (!exists)
                        {
                            var newUnit = new Unit
                            {
                                UnitName = u,
                                IsDeleted = false,
                                CreatedDatetimeUtc = DateTime.UtcNow,
                                UpdatedDatetimeUtc = DateTime.UtcNow
                            };
                            _masterDBContext.Unit.Add(newUnit);
                        }
                    }
                    _masterDBContext.SaveChanges();
                    var unitDataList = _masterDBContext.Unit.AsNoTracking().ToList();
                    #endregion

                    #region Cập nhật sản phẩm & các thông tin bổ sung
                    foreach (var item in excelModel)
                    {
                        if (productDataList.Any(q => q.ProductCode == item.ProductCode))
                            continue;
                        var productCateObj = productCateEntities.FirstOrDefault(q => q.ProductCateName == item.CateName);
                        var productTypeObj = productTypeEntities.FirstOrDefault(q => q.IdentityCode == item.CatePrefixCode);
                        var unitObj = unitDataList.FirstOrDefault(q => q.UnitName == item.Unit1);
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
                            CreatedDatetimeUtc = DateTime.UtcNow,
                            UpdatedDatetimeUtc = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        productDataList.Add(productEntity);
                    }

                    var readBulkConfig = new BulkConfig { UpdateByProperties = new List<string> { nameof(Product.ProductCode) } };
                    _stockDbContext.BulkRead<Product>(productDataList, readBulkConfig);
                    _stockDbContext.BulkInsertOrUpdate<Product>(productDataList, new BulkConfig { PreserveInsertOrder = false, SetOutputIdentity = true });

                    // Cập nhật đơn vị chuyển đổi mặc định
                    var defaultProductUnitConversionList = new List<ProductUnitConversion>(productDataList.Count);

                    foreach (var p in productDataList)
                    {
                        if (p.ProductId > 0)
                        {
                            var unitObj = unitDataList.FirstOrDefault(q => q.UnitId == p.UnitId);
                            if (unitObj != null)
                            {
                                var defaultProductUnitConversionEntity = new ProductUnitConversion()
                                {
                                    ProductUnitConversionName = unitObj.UnitName,
                                    ProductId = p.ProductId,
                                    SecondaryUnitId = unitObj.UnitId,
                                    FactorExpression = "1",
                                    ConversionDescription = "Mặc định",
                                    IsFreeStyle = false,
                                    IsDefault = true
                                };
                                defaultProductUnitConversionList.Add(defaultProductUnitConversionEntity);
                            }
                        }
                    }
                    var readDefaultProductUnitConversionBulkConfig = new BulkConfig { UpdateByProperties = new List<string> { nameof(ProductUnitConversion.ProductId), nameof(ProductUnitConversion.SecondaryUnitId), nameof(ProductUnitConversion.IsDefault) } };
                    _stockDbContext.BulkRead<ProductUnitConversion>(defaultProductUnitConversionList, readDefaultProductUnitConversionBulkConfig);
                    _stockDbContext.BulkInsert<ProductUnitConversion>(defaultProductUnitConversionList.Where(q => q.ProductUnitConversionId == 0).ToList(), new BulkConfig { PreserveInsertOrder = true, SetOutputIdentity = true });

                    #region Cập nhật mô tả sản phẩm & thông tin bổ sung
                    var productExtraInfoList = new List<ProductExtraInfo>(productDataList.Count);
                    var productExtraInfoModel = excelModel.Select(q => new { q.ProductCode, q.Specification }).GroupBy(g => g.ProductCode).Select(q => q.First()).ToList();
                    foreach (var item in productExtraInfoModel)
                    {
                        var productObj = productDataList.FirstOrDefault(q => q.ProductCode == item.ProductCode);
                        if (productObj != null)
                        {
                            var productExtraInfoEntity = new ProductExtraInfo
                            {
                                ProductId = productObj.ProductId,
                                Specification = item.Specification,
                                Description = string.Empty,
                                IsDeleted = false
                            };
                            productExtraInfoList.Add(productExtraInfoEntity);
                        }
                    }
                    var readProductExtraInfoBulkConfig = new BulkConfig { UpdateByProperties = new List<string> { nameof(ProductExtraInfo.ProductId) } };
                    _stockDbContext.BulkRead<ProductExtraInfo>(productExtraInfoList, readProductExtraInfoBulkConfig);
                    _stockDbContext.BulkInsertOrUpdate<ProductExtraInfo>(productExtraInfoList, new BulkConfig { PreserveInsertOrder = false, SetOutputIdentity = false });
                    #endregion

                    #region Cập nhật đơn vị chuyển đổi - ProductUnitConversion
                    var newProductUnitConversionList = new List<ProductUnitConversion>(productDataList.Count);
                    foreach (var item in excelModel)
                    {
                        if (string.IsNullOrEmpty(item.ProductCode) || string.IsNullOrEmpty(item.Unit2) || item.Factor == 0)
                            continue;
                        var unit1 = unitDataList.FirstOrDefault(q => q.UnitName == item.Unit1);
                        var unit2 = unitDataList.FirstOrDefault(q => q.UnitName == item.Unit2);

                        var productObj = productDataList.FirstOrDefault(q => q.ProductCode == item.ProductCode);

                        if (item.Factor > 0 && productObj != null && unit1 != null && unit2 != null)
                        {
                            var newProductUnitConversion = new ProductUnitConversion
                            {
                                ProductUnitConversionName = string.Format("{0}-{1}", unit2.UnitName, item.Factor).Replace(@",", ""),
                                ProductId = productObj.ProductId,
                                SecondaryUnitId = unit2.UnitId,
                                FactorExpression = item.Factor.ToString().Replace(@",", ""),
                                ConversionDescription = string.Format("{0} {1} {2}", unit1.UnitName, unit2.UnitName, item.Factor),
                                IsDefault = false
                            };

                            if (Utils.GetPrimaryQuantityFromProductUnitConversionQuantity(0, newProductUnitConversion.FactorExpression) != 0
                                ||
                                Utils.GetPrimaryQuantityFromProductUnitConversionQuantity(1, newProductUnitConversion.FactorExpression)<=0
                                )
                            {
                                return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                            }

                            if (newProductUnitConversionList.Any(q => q.ProductUnitConversionName == newProductUnitConversion.ProductUnitConversionName && q.ProductId == newProductUnitConversion.ProductId))
                                continue;
                            else
                                newProductUnitConversionList.Add(newProductUnitConversion);
                        }
                    }
                    var readProductUnitConversionBulkConfig = new BulkConfig { UpdateByProperties = new List<string> { nameof(ProductUnitConversion.ProductUnitConversionName), nameof(ProductUnitConversion.ProductId), nameof(ProductUnitConversion.IsDefault) } };
                    _stockDbContext.BulkRead<ProductUnitConversion>(newProductUnitConversionList, readProductUnitConversionBulkConfig);
                    _stockDbContext.BulkInsertOrUpdate<ProductUnitConversion>(newProductUnitConversionList, new BulkConfig { PreserveInsertOrder = true, SetOutputIdentity = true });

                    #endregion

                    #endregion end db updating product & related data

                    #region Tạo và xửa lý phiếu nhập kho

                    newProductUnitConversionList = _stockDbContext.ProductUnitConversion.AsNoTracking().ToList();
                    foreach (var item in excelModel)
                    {
                        if (string.IsNullOrEmpty(item.ProductCode))
                            continue;

                        if (item.Qty1 == 0)
                            continue;
                        ProductUnitConversion productUnitConversionObj = null;
                        var productObj = productDataList.FirstOrDefault(q => q.ProductCode == item.ProductCode);

                        if (!string.IsNullOrEmpty(item.Unit2) && item.Factor > 0)
                        {
                            var unit2 = unitDataList.FirstOrDefault(q => q.UnitName == item.Unit2);
                            if (unit2 != null && item.Factor > 0)
                            {
                                var factorExpression = item.Factor;
                                productUnitConversionObj = newProductUnitConversionList.FirstOrDefault(q => q.ProductId == productObj.ProductId && q.SecondaryUnitId == unit2.UnitId && q.FactorExpression == factorExpression.ToString() && !q.IsDefault);
                            }
                        }
                        else
                            productUnitConversionObj = newProductUnitConversionList.FirstOrDefault(q => q.ProductId == productObj.ProductId && q.IsDefault);

                        newInventoryInputModel.Add(
                                new InventoryInProductExtendModel
                                {
                                    ProductId = productObj != null ? productObj.ProductId : 0,
                                    ProductCode = item.ProductCode,
                                    ProductUnitConversionId = productUnitConversionObj != null ? productUnitConversionObj?.ProductUnitConversionId : null,
                                    PrimaryQuantity = item.Qty1,
                                    ProductUnitConversionQuantity = item.Qty2,
                                    UnitPrice = item.UnitPrice,
                                    RefObjectTypeId = null,
                                    RefObjectId = null,
                                    RefObjectCode = item.CatePrefixCode,
                                    ToPackageId = null,
                                    PackageOptionId = EnumPackageOption.NoPackageManager
                                }
                            ); ;
                    }
                    if (newInventoryInputModel.Count > 0)
                    {
                        var groupList = newInventoryInputModel.GroupBy(g => g.RefObjectCode).ToList();
                        var index = 1;
                        foreach (var g in groupList)
                        {
                            var details = g.ToList();
                            var newInventory = new InventoryInModel
                            {
                                StockId = model.StockId,
                                InventoryCode = string.Format("PN_TonDau_{0}_{1}", index, DateTime.UtcNow.ToString("ddMMyyyyHHmmss")),
                                DateUtc = model.IssuedDate,
                                Shipper = string.Empty,
                                Content = model.Description,
                                CustomerId = null,
                                Department = string.Empty,
                                StockKeeperUserId = null,
                                BillCode = string.Empty,
                                BillSerial = string.Empty,
                                BillDate = model.IssuedDate,
                                FileIdList = null,
                                InProducts = new List<InventoryInProductModel>(details.Count)
                            };
                            foreach (var item in details)
                            {
                                var currentProductObj = productDataList.FirstOrDefault(q => q.ProductCode == item.ProductCode);
                                newInventory.InProducts.Add(new InventoryInProductModel
                                {
                                    ProductId = item.ProductId,
                                    ProductUnitConversionId = item.ProductUnitConversionId,
                                    PrimaryQuantity = item.PrimaryQuantity,
                                    ProductUnitConversionQuantity = item.ProductUnitConversionQuantity,
                                    UnitPrice = item.UnitPrice,
                                    RefObjectTypeId = item.RefObjectTypeId,
                                    RefObjectId = item.RefObjectId,
                                    RefObjectCode = string.Format("PN_TonDau_{0}_{1}_{2}", index, DateTime.UtcNow.ToString("ddMMyyyyHHmmss"), item.RefObjectCode),
                                    ToPackageId = null,
                                    PackageOptionId = EnumPackageOption.NoPackageManager
                                });
                            }
                            inventoryInputList.Add(newInventory);
                            index++;
                        }
                    }

                    if (inventoryInputList.Count > 0)
                    {
                        foreach (var item in inventoryInputList)
                        {
                            var ret = await _inventoryService.AddInventoryInput(currentUserId, item);
                            if (ret.Data > 0)
                            {
                                // Duyệt phiếu nhập kho
                                //await _inventoryService.ApproveInventoryInput(ret.Data, currentUserId); 
                                continue;
                            }
                            else
                            {
                                _logger.LogWarning(string.Format("ProcessInventoryInputExcelSheet not success, please recheck -> AddInventoryInput: {0}", item.InventoryCode));
                            }
                        }
                    }
                    #endregion
                }
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessInventoryInputExcelSheet");
                return GeneralCode.InternalError;
            }
        }

        private async Task<Enum> ProcessInventoryOutputExcelSheet(List<ISheet> sheetList, InventoryOpeningBalanceModel model, int currentUserId)
        {
            try
            {
                foreach (var sheet in sheetList)
                {                    
                    var totalRowCount = sheet.LastRowNum + 1;
                    var excelModel = new List<OpeningBalanceModel>(totalRowCount);                    
                    try
                    {
                        for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                        {
                            var row = sheet.GetRow(i);
                            if (row == null) continue;

                            var cellProductCode = row.GetCell(0);
                            if (cellProductCode == null)
                                continue;
                            var productCode = cellProductCode != null ? HelperCellGetStringValue(cellProductCode) : string.Empty;
                            if (string.IsNullOrEmpty(productCode))
                                continue;

                            #region Get All Cell value
                            var cellUnit = row.GetCell(2);
                            var unitName = cellUnit != null ? HelperCellGetStringValue(cellUnit) : string.Empty;

                            var cellUnitAlt = row.GetCell(7);
                            var unitAltName = cellUnitAlt != null ? HelperCellGetStringValue(cellUnitAlt) : string.Empty;

                            var qTy = row.GetCell(3) != null ? HelperCellGetNumericValue(row.GetCell(3)) : 0;
                            var unitPrice = row.GetCell(4) != null ? (decimal)HelperCellGetNumericValue(row.GetCell(4)) : 0;

                            var qTy2 = row.GetCell(9) != null ? HelperCellGetNumericValue(row.GetCell(9)) : 0;
                            var factor = row.GetCell(8) != null ? HelperCellGetNumericValue(row.GetCell(8)) : 0;

                            var cellItem = new OpeningBalanceModel
                            {
                                CateName = string.Empty,
                                CatePrefixCode = string.Empty,
                                ProductCode = productCode,
                                ProductName = string.Empty,
                                Unit1 = unitName.ToLower(),
                                Qty1 = qTy,
                                UnitPrice = unitPrice,
                                Specification = string.Empty,
                                Unit2 = unitAltName.ToLower(),
                                Qty2 = qTy2,
                                Factor = factor,
                                Height = 0,
                                Width = 0,
                                Long = 0
                            };
                            excelModel.Add(cellItem);
                            #endregion
                        } // end for loop
                    }
                    catch (Exception ex)
                    {
                        return GeneralCode.InternalError;
                        throw ex;
                    }

                    #region Thông tin sản phẩm
                    var productDataList = new List<Product>(excelModel.Count);
                    foreach (var item in excelModel)
                    {
                        if (productDataList.Any(q => q.ProductCode == item.ProductCode))
                            continue;
                        var productEntity = new Product
                        {
                            ProductCode = item.ProductCode,
                            ProductName = item.ProductName,
                            IsCanBuy = true,
                            IsCanSell = true,
                            MainImageFileId = null,
                            ProductTypeId = null,
                            ProductCateId = 0,
                            BarcodeStandardId = null,
                            BarcodeConfigId = null,
                            Barcode = null,
                            UnitId = 0,
                            EstimatePrice = item.UnitPrice,
                            Long = item.Long,
                            Width = item.Width,
                            Height = item.Height,
                            CreatedDatetimeUtc = DateTime.UtcNow,
                            UpdatedDatetimeUtc = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        productDataList.Add(productEntity);
                    }

                    var readBulkConfig = new BulkConfig { UpdateByProperties = new List<string> { nameof(Product.ProductCode) } };
                    _stockDbContext.BulkRead<Product>(productDataList, readBulkConfig);
                    #endregion

                    #region Thông tin đơn vị tính & chuyển đổi
                    var unitDataList = await _masterDBContext.Unit.AsNoTracking().ToListAsync();
                    var productUnitConversionDataList = new List<ProductUnitConversion>(excelModel.Count);
                    foreach (var item in excelModel)
                    {
                        var productEntity = productDataList.FirstOrDefault(q => q.ProductCode == item.ProductCode);
                        var productUnitConversionName = string.Format("{0}-{1}", item.Unit2, item.Factor);
                        if (productEntity != null)
                        {
                            var productUnitConversionEntity = new ProductUnitConversion
                            {
                                ProductId = productEntity.ProductId,
                                ProductUnitConversionName = productUnitConversionName
                            };

                            productUnitConversionDataList.Add(productUnitConversionEntity);
                        }
                    }
                    var readProductUnitConversionBulkConfig = new BulkConfig { UpdateByProperties = new List<string> { nameof(ProductUnitConversion.ProductUnitConversionName), nameof(ProductUnitConversion.ProductId) } };
                    _stockDbContext.BulkRead<ProductUnitConversion>(productUnitConversionDataList, readProductUnitConversionBulkConfig);
                    #endregion

                    #region Thông tin kiện mặc định
                    var defaultPackageDataList = (from pk in _stockDbContext.Package
                                                  join p in productDataList on pk.ProductId equals p.ProductId
                                                  where pk.StockId == model.StockId && pk.PackageTypeId == (int)EnumPackageType.Default
                                                  select pk).AsNoTracking().ToList();

                    #endregion

                    #region Tạo và xửa lý phiếu xuất kho   
                    var inventoryOutProductModelList = new List<InventoryOutProductModel>(excelModel.Count);
                    foreach (var item in excelModel)
                    {
                        if (string.IsNullOrEmpty(item.ProductCode) || (item.Qty1 == 0 && item.Qty2 == 0))
                            continue;

                        var productUnitConversionName = string.Format("{0}-{1}", item.Unit2, item.Factor);
                        var productObj = productDataList.FirstOrDefault(q => q.ProductCode == item.ProductCode);
                        var productUnitConversionObj = productUnitConversionDataList.FirstOrDefault(q => q.ProductId == productObj.ProductId && q.ProductUnitConversionName == productUnitConversionName);
                        var packageObj = defaultPackageDataList.FirstOrDefault(q => q.ProductId == productObj.ProductId);

                        var inventoryOutProductModel = new InventoryOutProductModel
                        {
                            ProductId = productObj.ProductId,
                            ProductUnitConversionId = productUnitConversionObj.ProductUnitConversionId,                            
                            PrimaryQuantity = item.Qty1,
                            ProductUnitConversionQuantity = item.Qty2,
                            UnitPrice = item.UnitPrice,
                            RefObjectTypeId = null,
                            RefObjectId = null,
                            RefObjectCode = string.Format("PX_{0}", DateTime.UtcNow.ToString("ddMMyyyyHHmmss")),
                            FromPackageId = packageObj.PackageId,
                            OrderCode = string.Empty,
                            POCode = string.Empty,
                            ProductionOrderCode = string.Empty,
                        };
                        inventoryOutProductModelList.Add(inventoryOutProductModel);
                    }
                    if (inventoryOutProductModelList.Count > 0)
                    {
                        var pageSize = 60;
                        var totalPage = (int)Math.Ceiling(inventoryOutProductModelList.Count / (decimal)pageSize);
                        var inventoryOutputList = new List<InventoryOutModel>(totalPage);

                        for (var pageIndex = 1; pageIndex < totalPage; pageIndex++)
                        {
                            var outProductsList = inventoryOutProductModelList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                            var inventoryOutputEntity = new InventoryOutModel
                            {
                                StockId = model.StockId,
                                InventoryCode = string.Format("PX_TonDau_{0}_{1}", pageIndex, DateTime.UtcNow.ToString("ddMMyyyyHHmmss")),
                                Shipper = string.Empty,
                                Content = model.Description,
                                DateUtc = model.IssuedDate,
                                CustomerId = null,
                                Department = string.Empty,
                                StockKeeperUserId = null,
                                FileIdList = null,
                                OutProducts = outProductsList
                            };
                            inventoryOutputList.Add(inventoryOutputEntity);
                        }
                        if (inventoryOutputList.Count > 0)
                        {
                            foreach (var item in inventoryOutputList)
                            {
                                var ret = await _inventoryService.AddInventoryOutput(currentUserId, item);
                                if (ret.Data > 0)
                                {
                                    // Duyệt phiếu xuất kho
                                    //await _inventoryService.ApproveInventoryInput(ret.Data, currentUserId); 
                                    continue;
                                }
                                else
                                {
                                    _logger.LogWarning(string.Format("ProcessInventoryOutputExcelSheet not success, please recheck -> AddInventoryOutput: {0}", item.InventoryCode));
                                }
                            }
                        }
                        else
                        {
                            return GeneralCode.InternalError;
                        }
                    }
                    #endregion
                }
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessInventoryOutputExcelSheet");
                return GeneralCode.InternalError;
            }
        }
        #endregion
    }

}
