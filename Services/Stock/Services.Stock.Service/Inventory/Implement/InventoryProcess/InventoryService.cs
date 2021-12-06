﻿using Microsoft.EntityFrameworkCore;
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
using VErp.Commons.GlobalObject;
using Microsoft.Data.SqlClient;
using NPOI.SS.UserModel;
using System.IO;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Commons.GlobalObject.InternalDataInterface;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using VErp.Services.Stock.Service.Stock.Implement.InventoryFileData;
using VErp.Services.Stock.Service.Products;
using VErp.Commons.Library.Model;
using VErp.Services.Stock.Model.Inventory.OpeningBalance;
using System.Data;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.ServiceCore.Facade;
using Verp.Resources.Stock.Inventory.Abstract;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public partial class InventoryService : InventoryServiceAbstract, IInventoryService
    {
        //const decimal MINIMUM_JS_NUMBER = Numbers.MINIMUM_ACCEPT_DECIMAL_NUMBER;

        private readonly MasterDBContext _masterDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly IFileService _fileService;
        private readonly IOrganizationHelperService _organizationHelperService;
        private readonly IStockHelperService _stockHelperService;
        private readonly IProductHelperService _productHelperService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IProductService _productService;
        private readonly IInventoryBillOutputService _inventoryBillOutputService;
        private readonly IInventoryBillInputService _inventoryBillInputService;
        private readonly IUserHelperService _userHelperService;
        private readonly IMailFactoryService _mailFactoryService;

        public InventoryService(MasterDBContext masterDBContext, StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<InventoryService> logger
            , IActivityLogService activityLogService
            , IUnitService unitService
            , IFileService fileService
            , IAsyncRunnerService asyncRunner
            , IOrganizationHelperService organizationHelperService
            , IStockHelperService stockHelperService
            , IProductHelperService productHelperService
            , ICurrentContextService currentContextService
            , IProductService productService
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IProductionOrderHelperService productionOrderHelperService
            , IProductionHandoverHelperService productionHandoverHelperService
            , IInventoryBillOutputService inventoryBillOutputService
            , IInventoryBillInputService inventoryBillInputService
            , IUserHelperService userHelperService = null, IMailFactoryService mailFactoryService = null) : base(stockContext, logger, customGenCodeHelperService, productionOrderHelperService, productionHandoverHelperService, currentContextService)
        {
            _masterDBContext = masterDBContext;
            _activityLogService = activityLogService;
            _fileService = fileService;
            _organizationHelperService = organizationHelperService;
            _stockHelperService = stockHelperService;
            _productHelperService = productHelperService;
            _currentContextService = currentContextService;
            _productService = productService;
            _inventoryBillOutputService = inventoryBillOutputService;
            _inventoryBillInputService = inventoryBillInputService;
            _userHelperService = userHelperService;
            _mailFactoryService = mailFactoryService;
        }



        public async Task<PageData<InventoryOutput>> GetList(string keyword, int? customerId, IList<int> productIds, string accountancyAccountNumber, int stockId = 0, int? inventoryStatusId = null, EnumInventoryType? type = null, long? beginTime = 0, long? endTime = 0, bool? isExistedInputBill = null, string sortBy = "date", bool asc = false, int page = 1, int size = 10, int? inventoryActionId = null)
        {
            keyword = keyword?.Trim();
            accountancyAccountNumber = accountancyAccountNumber?.Trim();

            var inventoryQuery = _stockDbContext.Inventory.AsNoTracking().AsQueryable();

            if (stockId > 0)
            {
                inventoryQuery = inventoryQuery.Where(q => q.StockId == stockId);
            }

            if (type > 0)
            {
                inventoryQuery = inventoryQuery.Where(q => q.InventoryTypeId == (int)type);
            }

            if (beginTime > 0)
            {
                var startDate = beginTime?.UnixToDateTime();
                inventoryQuery = inventoryQuery.Where(q => q.Date >= startDate);
            }

            if (endTime > 0)
            {
                var endDate = endTime?.UnixToDateTime();
                inventoryQuery = inventoryQuery.Where(q => q.Date <= endDate);
            }

            if(inventoryActionId.HasValue)
            {
                inventoryQuery = inventoryQuery.Where(q => q.InventoryActionId == inventoryActionId);
            }



            if (!string.IsNullOrWhiteSpace(keyword) || productIds?.Count > 0)
            {
                var inventoryDetails = _stockDbContext.InventoryDetail.AsQueryable();
                if (productIds != null && productIds.Count > 0)
                {
                    inventoryDetails = inventoryDetails.Where(d => productIds.Contains(d.ProductId));

                }

                if (!string.IsNullOrWhiteSpace(keyword))
                {

                    inventoryDetails = from p in _stockDbContext.Product
                                       join d in inventoryDetails on p.ProductId equals d.ProductId
                                       where p.ProductCode.Contains(keyword)
                                       || p.ProductName.Contains(keyword)
                                       || p.ProductNameEng.Contains(keyword)
                                       || d.OrderCode.Contains(keyword)
                                       || d.ProductionOrderCode.Contains(keyword)
                                       || d.Pocode.Contains(keyword)
                                       || d.Description.Contains(keyword)
                                       || d.RefObjectCode.Contains(keyword)
                                       select d;
                }

                var inventoryIdsQuery = from d in inventoryDetails
                                        select d.InventoryId;

                inventoryQuery = from q in inventoryQuery
                                 join c in _stockDbContext.RefCustomerBasic on q.CustomerId equals c.CustomerId into cs
                                 from c in cs.DefaultIfEmpty()
                                 where q.InventoryCode.Contains(keyword)
                                    || q.Shipper.Contains(keyword)
                                    || q.Content.Contains(keyword)
                                    || q.Department.Contains(keyword)
                                    || q.BillForm.Contains(keyword)
                                    || q.BillCode.Contains(keyword)
                                    || q.BillSerial.Contains(keyword)
                                    || c.CustomerCode.Contains(keyword)
                                    || c.CustomerName.Contains(keyword)
                                    || inventoryIdsQuery.Contains(q.InventoryId)
                                 select q;
            }

            if (inventoryStatusId.HasValue)
            {
                inventoryQuery = inventoryQuery.Where(q => q.InventoryStatusId == inventoryStatusId.Value);
            }

            if (customerId.HasValue)
            {
                inventoryQuery = inventoryQuery.Where(q => q.CustomerId == customerId);
            }

            if (!string.IsNullOrWhiteSpace(accountancyAccountNumber))
            {
                inventoryQuery = inventoryQuery.Where(q => q.AccountancyAccountNumber.StartsWith(accountancyAccountNumber));
            }

            //IQueryable<VMappingOusideImportObject> mappingObjectQuery = null;

            //if (mappingFunctionKeys != null && mappingFunctionKeys.Count > 0)
            //{
            //    mappingObjectQuery = _stockDbContext.VMappingOusideImportObject
            //         .Where(m => mappingFunctionKeys.Contains(m.MappingFunctionKey));

            //    if (isExistedInputBill != null)
            //    {
            //        if (isExistedInputBill.Value)
            //        {
            //            inventoryQuery = from q in inventoryQuery
            //                             where mappingObjectQuery.Select(m => m.SourceId).Contains(q.InventoryId.ToString())
            //                             select q;
            //        }
            //        else
            //        {
            //            inventoryQuery = from q in inventoryQuery
            //                             where !mappingObjectQuery.Select(m => m.SourceId).Contains(q.InventoryId.ToString())
            //                             select q;
            //        }
            //    }
            //}


            IQueryable<RefInputBillBasic> billQuery = _stockDbContext.RefInputBillBasic.AsQueryable();

            if (isExistedInputBill != null)
            {
                if (isExistedInputBill.Value)
                {
                    inventoryQuery = from q in inventoryQuery
                                     join b in billQuery on q.InventoryCode equals b.SoCt into bs
                                     from b in bs.DefaultIfEmpty()
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
                                     where b.InputBillFId != null
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
                                     select q;
                }
                else
                {
                    inventoryQuery = from q in inventoryQuery
                                     join b in billQuery on q.InventoryCode equals b.SoCt into bs
                                     from b in bs.DefaultIfEmpty()
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
                                     where b.InputBillFId == null
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
                                     select q;
                }
            }

            var total = await inventoryQuery.CountAsync();

            var inventoryDataList = await inventoryQuery.SortByFieldName(sortBy, asc).AsNoTracking().Skip((page - 1) * size).Take(size).ToListAsync();

            //enrich data
            var stockIds = inventoryDataList.Select(iv => iv.StockId).ToList();

            var stockInfos = (await _stockDbContext.Stock.AsNoTracking().Where(s => stockIds.Contains(s.StockId)).ToListAsync()).ToDictionary(s => s.StockId, s => s);

            var inventoryIds = inventoryDataList.Select(iv => iv.InventoryId.ToString()).ToList();
            var inventoryCodes = inventoryDataList.Select(iv => iv.InventoryCode).ToList();
            //var mappingObjects = new List<VMappingOusideImportObject>();
            //if (mappingObjectQuery != null)
            //{
            //    mappingObjects = await mappingObjectQuery.Where(m => inventoryIds.Contains(m.SourceId)).ToListAsync();
            //}

            var inputObjects = new List<RefInputBillBasic>();
            if (billQuery != null)
            {
                inputObjects = await billQuery.Where(m => inventoryCodes.Contains(m.SoCt)).ToListAsync();
            }

            var pagedData = new List<InventoryOutput>();
            foreach (var item in inventoryDataList)
            {
                stockInfos.TryGetValue(item.StockId, out var stockInfo);

                pagedData.Add(new InventoryOutput()
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
                    BillForm = item.BillForm,
                    BillCode = item.BillCode,
                    BillSerial = item.BillSerial,
                    BillDate = item.BillDate.HasValue ? item.BillDate.Value.GetUnix() : (long?)null,
                    TotalMoney = item.TotalMoney,
                    IsApproved = item.IsApproved,
                    AccountancyAccountNumber = item.AccountancyAccountNumber,
                    CreatedByUserId = item.CreatedByUserId,
                    UpdatedByUserId = item.UpdatedByUserId,
                    UpdatedDatetimeUtc = item.UpdatedDatetimeUtc.GetUnix(),
                    CreatedDatetimeUtc = item.CreatedDatetimeUtc.GetUnix(),
                    DepartmentId = item.DepartmentId,
                    StockOutput = stockInfo == null ? null : new StockOutput
                    {
                        StockId = stockInfo.StockId,
                        StockName = stockInfo.StockName,
                        StockKeeperName = stockInfo.StockKeeperName,
                        StockKeeperId = stockInfo.StockKeeperId
                    },
                    InventoryDetailOutputList = null,
                    FileList = null,
                    //InputBills = mappingObjects
                    //    .Where(m => m.SourceId == item.InventoryId.ToString())
                    //    .Select(m => new MappingInputBillModel()
                    //    {
                    //        MappingFunctionKey = m.MappingFunctionKey,
                    //        InputTypeId = m.InputTypeId,
                    //        SourceId = m.SourceId,
                    //        InputBillFId = m.InputBillFId,
                    //        BillObjectTypeId = (EnumObjectType)m.BillObjectTypeId

                    //    }).ToList()
                    InputBills = inputObjects
                        .Where(m => m.SoCt.ToLower() == item.InventoryCode.ToLower())
                        .Select(m => new MappingInputBillModel()
                        {
                            //MappingFunctionKey = null,
                            InputTypeId = m.InputTypeId,
                            //SourceId = item.InventoryId.ToString(),
                            InputBillFId = m.InputBillFId,
                            BillObjectTypeId = EnumObjectType.InputBill,
                            InputType_Title = m.InputTypeTitle

                        }).ToList(),
                    InventoryActionId = item.InventoryActionId,
                    InventoryStatusId = item.InventoryStatusId
                });

            }
            return (pagedData, total);
        }
        public async Task<InventoryOutput> InventoryInfo(long inventoryId)

        {
            try
            {
                var inventoryObj = _stockDbContext.Inventory.AsNoTracking().FirstOrDefault(q => q.InventoryId == inventoryId);
                if (inventoryObj == null)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams);
                }

                #region Get inventory details
                var inventoryDetails = await _stockDbContext.InventoryDetail.Where(q => q.InventoryId == inventoryObj.InventoryId).AsNoTracking().OrderBy(s => s.SortOrder).ThenBy(s => s.CreatedDatetimeUtc).ToListAsync();

                var productIds = inventoryDetails.Select(d => d.ProductId).ToList();

                var productInfos = (await _stockDbContext.Product.AsNoTracking()
                    .Where(p => productIds.Contains(p.ProductId))
                    .ToListAsync())
                    .ToDictionary(p => p.ProductId, p => p);

                var productUnitConversionIds = inventoryDetails.Select(d => d.ProductUnitConversionId).ToList();

                var productUnitConversions = (await _stockDbContext.ProductUnitConversion.AsNoTracking()
                    .Where(pu => productUnitConversionIds.Contains(pu.ProductUnitConversionId))
                    .ToListAsync())
                    .ToDictionary(pu => pu.ProductUnitConversionId, pu => pu);

                var packetIds = inventoryDetails.SelectMany(d => new[] { d.FromPackageId, d.ToPackageId }).Where(d => d.HasValue).Select(d => d.Value).Distinct().ToList();
                var packgeInfos = (await _stockDbContext.Package.AsNoTracking()
                    .Where(p => packetIds.Contains(p.PackageId))
                    .ToListAsync())
                    .ToDictionary(p => p.PackageId, p => p);

                var listInventoryDetailsOutput = new List<InventoryDetailOutput>(inventoryDetails.Count);

                //var inventoryRequirementCodes = inventoryDetails
                //    .Where(d => !string.IsNullOrEmpty(d.InventoryRequirementCode))
                //    .Select(d => d.InventoryRequirementCode)
                //    .ToList();

                //var inventoryRequirementMap = _stockDbContext.InventoryRequirementDetail
                //    .Include(id => id.InventoryRequirement)
                //    .Where(id => inventoryRequirementDetailIds.Contains(id.InventoryRequirementDetailId))
                //    .Select(id => new
                //    {
                //        id.InventoryRequirementDetailId,
                //        id.InventoryRequirement.InventoryRequirementCode,
                //        id.InventoryRequirement.InventoryRequirementId
                //    })
                //    .ToList()
                //    .GroupBy(id => id.InventoryRequirementDetailId)
                //    .ToDictionary(g => g.Key, g => g.Select(id => new InventoryRequirementSimpleInfo
                //    {
                //        InventoryRequirementId = id.InventoryRequirementId,
                //        InventoryRequirementCode = id.InventoryRequirementCode
                //    }).ToList());

                var requirementInventoryDetailIds = inventoryDetails.Select(id => id.InventoryRequirementDetailId).ToList();
                var requirementInventoryCodeMap = (from ird in _stockDbContext.InventoryRequirementDetail
                                                   join ir in _stockDbContext.InventoryRequirement on ird.InventoryRequirementId equals ir.InventoryRequirementId
                                                   where requirementInventoryDetailIds.Contains(ird.InventoryRequirementDetailId)
                                                   select new
                                                   {
                                                       ird.InventoryRequirementDetailId,
                                                       ir.InventoryRequirementCode
                                                   })
                                                   .ToDictionary(ird => ird.InventoryRequirementDetailId, ird => ird.InventoryRequirementCode);

                foreach (var detail in inventoryDetails)
                {
                    ProductListOutput productOutput = null;

                    PackageEntity packageInfo = null;

                    if (detail.FromPackageId > 0)
                    {
                        packgeInfos.TryGetValue(detail.FromPackageId.Value, out packageInfo);
                    }

                    if (detail.ToPackageId > 0)
                    {
                        packgeInfos.TryGetValue(detail.ToPackageId.Value, out packageInfo);
                    }

                    if (productInfos.TryGetValue(detail.ProductId, out var productInfo))
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
                            UnitName = string.Empty,
                        };
                    }

                    productUnitConversions.TryGetValue(detail.ProductUnitConversionId, out var productUnitConversionInfo);
                    var inventoryRequirementCode = detail.InventoryRequirementDetailId.HasValue && requirementInventoryCodeMap.ContainsKey(detail.InventoryRequirementDetailId.Value) ? requirementInventoryCodeMap[detail.InventoryRequirementDetailId.Value] : null;
                    var detailModel = new InventoryDetailOutput
                    {
                        InventoryId = detail.InventoryId,
                        InventoryDetailId = detail.InventoryDetailId,
                        ProductId = detail.ProductId,
                        PrimaryUnitId = productInfo?.UnitId,
                        RequestPrimaryQuantity = detail.RequestPrimaryQuantity?.RoundBy(),
                        PrimaryQuantity = detail.PrimaryQuantity.RoundBy(),
                        UnitPrice = detail.UnitPrice,
                        ProductUnitConversionId = detail.ProductUnitConversionId,
                        RequestProductUnitConversionQuantity = detail.RequestProductUnitConversionQuantity?.RoundBy(),
                        ProductUnitConversionQuantity = detail.ProductUnitConversionQuantity.RoundBy(),
                        ProductUnitConversionPrice = detail.ProductUnitConversionPrice,
                        FromPackageId = detail.FromPackageId,
                        ToPackageId = detail.ToPackageId,
                        ToPackageCode = packageInfo?.PackageCode,
                        FromPackageCode = packageInfo?.PackageCode,
                        PackageOptionId = detail.PackageOptionId,

                        RefObjectTypeId = detail.RefObjectTypeId,
                        RefObjectId = detail.RefObjectId,
                        RefObjectCode = detail.RefObjectCode,
                        OrderCode = detail.OrderCode,
                        POCode = detail.Pocode,
                        ProductionOrderCode = detail.ProductionOrderCode,

                        ProductOutput = productOutput,
                        ProductUnitConversion = productUnitConversionInfo ?? null,
                        SortOrder = detail.SortOrder,
                        Description = detail.Description,
                        //AccountancyAccountNumberDu = details.AccountancyAccountNumberDu,
                        InventoryRequirementCode = inventoryRequirementCode,
                        InventoryRequirementDetailId = detail.InventoryRequirementDetailId
                    };

                    //if (!string.IsNullOrEmpty(detail.InventoryRequirementCode) && inventoryRequirementMap.ContainsKey(detail.InventoryRequirementDetailId.Value))
                    //{
                    //    detail.InventoryRequirementInfo = inventoryRequirementMap[detail.InventoryRequirementDetailId.Value];
                    //}

                    listInventoryDetailsOutput.Add(detailModel);
                }
                #endregion

                #region Get Attached files 

                var fileIds = await _stockDbContext.InventoryFile.Where(q => q.InventoryId == inventoryObj.InventoryId).Select(q => q.FileId).ToListAsync();
                var attachedFiles = await _fileService.GetListFileUrl(fileIds, EnumThumbnailSize.Large);
                if (attachedFiles == null)
                {
                    attachedFiles = new List<FileToDownloadInfo>();
                }
                #endregion

                var stockInfo = _stockDbContext.Stock.AsNoTracking().FirstOrDefault(q => q.StockId == inventoryObj.StockId);

                var mappingObjects = _stockDbContext.RefInputBillBasic.Where(b => b.SoCt == inventoryObj.InventoryCode)
                     .Select(m => new MappingInputBillModel()
                     {
                         //MappingFunctionKey = m.MappingFunctionKey,
                         InputTypeId = m.InputTypeId,
                         //SourceId = inventoryObj.InventoryId.ToString(),
                         InputBillFId = m.InputBillFId,
                         InputType_Title = m.InputTypeTitle
                     }).ToList();

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
                    BillForm = inventoryObj.BillForm,
                    BillCode = inventoryObj.BillCode,
                    BillSerial = inventoryObj.BillSerial,
                    BillDate = inventoryObj.BillDate.GetUnix(),
                    TotalMoney = inventoryObj.TotalMoney,
                    IsApproved = inventoryObj.IsApproved,
                    AccountancyAccountNumber = inventoryObj.AccountancyAccountNumber,
                    CreatedByUserId = inventoryObj.CreatedByUserId,
                    UpdatedByUserId = inventoryObj.UpdatedByUserId,
                    DepartmentId = inventoryObj.DepartmentId,
                    CensorByUserId = inventoryObj.CensorByUserId,
                    StockOutput = stockInfo == null ? null : new StockOutput
                    {
                        StockId = stockInfo.StockId,
                        StockName = stockInfo.StockName,
                        StockKeeperName = stockInfo.StockKeeperName,
                        StockKeeperId = stockInfo.StockKeeperId
                    },
                    InventoryDetailOutputList = listInventoryDetailsOutput,
                    FileList = attachedFiles,
                    InputBills = mappingObjects,
                    InventoryStatusId = inventoryObj.InventoryStatusId,
                    InventoryActionId = inventoryObj.InventoryActionId
                };
                return inventoryOutput;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetInventory");
                throw;
            }
        }

        public async Task<(Stream stream, string fileName, string contentType)> InventoryInfoExport(long inventoryId)
        {
            var inventoryExport = new InventoryExportFacade();
            inventoryExport.SetCurrentContext(_currentContextService);
            inventoryExport.SetInventoryService(this);
            inventoryExport.SetOrganizationHelperService(_organizationHelperService);
            inventoryExport.SetProductHelperService(_productHelperService);
            inventoryExport.SetStockHelperService(_stockHelperService);
            return await inventoryExport.InventoryInfoExport(inventoryId);
        }

        public CategoryNameModel GetInventoryDetailFieldDataForMapping()
        {
            var result = new CategoryNameModel()
            {
                //CategoryId = 1,
                CategoryCode = "Inventory",
                CategoryTitle = "Inventory",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };
            var fields = Utils.GetFieldNameModels<OpeningBalanceModel>();
            result.Fields = fields;
            return result;
        }


        public CategoryNameModel FieldsForParse(EnumInventoryType inventoryTypeId)
        {
            var result = new CategoryNameModel()
            {
                //CategoryId = 1,
                CategoryCode = inventoryTypeId == EnumInventoryType.Input ? "Input" : "Output",
                CategoryTitle = inventoryTypeId == EnumInventoryType.Input ? InventoryAbstractMessage.InventoryInput : InventoryAbstractMessage.InventoryOuput,
                IsTreeView = false,
                Fields = Utils.GetFieldNameModels<InventoryExcelParseModel>((int)inventoryTypeId)
            };
            return result;
        }

        public IAsyncEnumerable<InventoryDetailRowValue> ParseExcel(ImportExcelMapping mapping, Stream stream, EnumInventoryType inventoryTypeId)
        {
            var parse = new InventoryDetailParseFacade();

            parse.SetProductService(_productService)
                .SetStockDbContext(_stockDbContext);

            return parse.ParseExcel(mapping, stream, inventoryTypeId);
        }

        public async Task<long> InventoryImport(ImportExcelMapping mapping, Stream stream, InventoryOpeningBalanceModel model)
        {
            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(model.StockId)))
            {
                var insertedData = new Dictionary<long, (string inventoryCode, object data)>();

                var genCodeContexts = new List<GenerateCodeContext>();
                var baseValueChains = new Dictionary<string, int>();
                long inventoryId = 0;

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    var inventoryExport = new InventoryImportFacade();
                    inventoryExport.SetProductService(_productService);
                    inventoryExport.SetMasterDBContext(_masterDBContext);
                    inventoryExport.SetStockDBContext(_stockDbContext);
                    await inventoryExport.ProcessExcelFile(mapping, stream, model);

                    if (model.InventoryTypeId == EnumInventoryType.Input)
                    {
                        var inventoryData = inventoryExport.GetInputInventoryModel();
                        if (inventoryData?.InProducts == null || inventoryData?.InProducts?.Count == 0)
                        {
                            throw new BadRequestException("No products found!");
                        }
                        inventoryData.InventoryCode = model.InventoryCode;
                        //foreach (var item in inventoryData)
                        {
                            genCodeContexts.Add(await GenerateInventoryCode(model.InventoryTypeId, inventoryData, baseValueChains));

                            inventoryId = await _inventoryBillInputService.AddInventoryInputDB(inventoryData);
                            insertedData.Add(inventoryId, (inventoryData.InventoryCode, inventoryData));
                        }
                    }
                    else
                    {
                        var inventoryData = await inventoryExport.GetOutputInventoryModel();
                        if (inventoryData?.OutProducts == null || inventoryData?.OutProducts?.Count == 0)
                        {
                            throw new BadRequestException("No products found!");
                        }
                        inventoryData.InventoryCode = model.InventoryCode;
                        //foreach (var item in inventoryData)
                        {
                            genCodeContexts.Add(await GenerateInventoryCode(model.InventoryTypeId, inventoryData, baseValueChains));

                            inventoryId = await _inventoryBillOutputService.AddInventoryOutputDb(inventoryData);
                            insertedData.Add(inventoryId, (inventoryData.InventoryCode, inventoryData));
                        }

                    }

                    await trans.CommitAsync();
                }


                foreach (var item in insertedData)
                {
                    await ImportedLogBuilder(model.InventoryTypeId)
                        .MessageResourceFormatDatas(item.Value.inventoryCode)
                        .ObjectId(item.Key)
                        .JsonData(item.Value.data.JsonSerialize())
                        .CreateLog();
                }

                foreach (var item in genCodeContexts)
                {
                    await item.ConfirmCode();
                }

                return inventoryId;
            }
        }

        private ObjectActivityLogModelBuilder<string> ImportedLogBuilder(EnumInventoryType inventoryType)
        {
            return inventoryType == EnumInventoryType.Input
                ? _inventoryBillInputService.ImportedLogBuilder()
                : _inventoryBillOutputService.ImportedLogBuilder();
        }

        public async Task<bool> SendMailNotifyCensor(long inventoryId, string mailTemplateCode, string[] mailTo)
        {
            var inventoryInfo = await InventoryInfo(inventoryId);
            var userIds = new[] { inventoryInfo.CreatedByUserId, inventoryInfo.UpdatedByUserId, inventoryInfo.CensorByUserId.GetValueOrDefault() };
            var users = await _userHelperService.GetByIds(userIds);

            var createdUser = users.FirstOrDefault(x => x.UserId == inventoryInfo.CreatedByUserId)?.FullName;
            var updatedUser = users.FirstOrDefault(x => x.UserId == inventoryInfo.UpdatedByUserId)?.FullName;
            var censortUser = users.FirstOrDefault(x => x.UserId == inventoryInfo.CensorByUserId)?.FullName;

            var businessInfo = await _organizationHelperService.BusinessInfo();

            var sendSuccess = await _mailFactoryService.Dispatch(mailTo, mailTemplateCode, new ObjectDataTemplateMail()
            {
                CensoredByUser = censortUser,
                CreatedByUser = createdUser,
                UpdatedByUser = updatedUser,
                CompanyName = businessInfo.CompanyName,
                F_Id = inventoryId,
                Code = inventoryInfo.InventoryCode,
                TotalMoney = inventoryInfo.TotalMoney.ToString("#,##0.##"),
                Domain = _currentContextService.Domain
            });

            return sendSuccess;
        }

        public async Task<bool> SentToCensor(long inventoryId)
        {
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                var info = await _stockDbContext.Inventory.FirstOrDefaultAsync(d => d.InventoryId == inventoryId);
                if (info == null) throw new BadRequestException(InventoryErrorCode.InventoryNotFound);

                if (info.InventoryStatusId != (int)EnumInventoryStatus.Draff)
                {
                    throw new BadRequestException(InventoryErrorCode.InventoryNotDraffYet);
                }

                info.InventoryStatusId = (int)EnumInventoryStatus.WaitToCensor;

                await _stockDbContext.SaveChangesAsync();

                trans.Commit();

                return true;
            }
        }

        public async Task<bool> Reject(long inventoryId)
        {
            using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
            {
                var info = await _stockDbContext.Inventory.FirstOrDefaultAsync(d => d.InventoryId == inventoryId);
                if (info == null) throw new BadRequestException(InventoryErrorCode.InventoryNotFound);

                if (info.InventoryStatusId != (int)EnumInventoryStatus.WaitToCensor)
                {
                    throw new BadRequestException(InventoryErrorCode.InventoryNotSentToCensorYet);
                }

                info.IsApproved = false;

                info.InventoryStatusId = (int)EnumInventoryStatus.Reject;
                info.CensorDatetimeUtc = DateTime.UtcNow;
                info.CensorByUserId = _currentContextService.UserId;

                await _stockDbContext.SaveChangesAsync();

                trans.Commit();
              
                return true;
            }
        }
    }
}
