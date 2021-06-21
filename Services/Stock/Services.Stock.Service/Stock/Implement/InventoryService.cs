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
        private readonly IAsyncRunnerService _asyncRunner;
        private readonly IOrganizationHelperService _organizationHelperService;
        private readonly IStockHelperService _stockHelperService;
        private readonly IProductHelperService _productHelperService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IProductService _productService;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IProductionOrderHelperService _productionOrderHelperService;
        private readonly IProductionHandoverService _productionHandoverService;

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
            , IProductionHandoverService productionHandoverService
            )
        {
            _masterDBContext = masterDBContext;
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _unitService = unitService;
            _fileService = fileService;
            _asyncRunner = asyncRunner;
            _organizationHelperService = organizationHelperService;
            _stockHelperService = stockHelperService;
            _productHelperService = productHelperService;
            _currentContextService = currentContextService;
            _productService = productService;
            _customGenCodeHelperService = customGenCodeHelperService;
            _productionOrderHelperService = productionOrderHelperService;
            _productionHandoverService = productionHandoverService;
        }



        public async Task<PageData<InventoryOutput>> GetList(string keyword, int? customerId, IList<int> productIds, string accountancyAccountNumber, int stockId = 0, bool? isApproved = null, EnumInventoryType? type = null, long? beginTime = 0, long? endTime = 0, bool? isExistedInputBill = null, IList<string> mappingFunctionKeys = null, string sortBy = "date", bool asc = false, int page = 1, int size = 10)
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



            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var inventoryDetails = _stockDbContext.InventoryDetail.AsQueryable();
                if (productIds != null && productIds.Count > 0)
                {
                    inventoryDetails = inventoryDetails.Where(d => productIds.Contains(d.ProductId));

                }
                var inventoryIdsQuery = from p in _stockDbContext.Product
                                        join d in inventoryDetails on p.ProductId equals d.ProductId
                                        where p.ProductCode.Contains(keyword)
                                        || p.ProductName.Contains(keyword)
                                        || p.ProductNameEng.Contains(keyword)
                                        || d.OrderCode.Contains(keyword)
                                        || d.ProductionOrderCode.Contains(keyword)
                                        || d.Pocode.Contains(keyword)
                                        || d.Description.Contains(keyword)
                                        || d.RefObjectCode.Contains(keyword)
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

            if (isApproved.HasValue)
            {
                inventoryQuery = inventoryQuery.Where(q => q.IsApproved == isApproved);
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
                            MappingFunctionKey = null,
                            InputTypeId = m.InputTypeId,
                            SourceId = item.InventoryId.ToString(),
                            InputBillFId = m.InputBillFId,
                            BillObjectTypeId = EnumObjectType.InputBill

                        }).ToList()
                });

            }
            return (pagedData, total);
        }

        public async Task<InventoryOutput> InventoryInfo(long inventoryId, IList<string> mappingFunctionKeys = null)
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

                foreach (var details in inventoryDetails)
                {
                    ProductListOutput productOutput = null;

                    PackageEntity packageInfo = null;

                    if (details.FromPackageId > 0)
                    {
                        packgeInfos.TryGetValue(details.FromPackageId.Value, out packageInfo);
                    }

                    if (details.ToPackageId > 0)
                    {
                        packgeInfos.TryGetValue(details.ToPackageId.Value, out packageInfo);
                    }

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
                            UnitName = string.Empty,
                        };
                    }

                    productUnitConversions.TryGetValue(details.ProductUnitConversionId, out var productUnitConversionInfo);

                    var detail = new InventoryDetailOutput
                    {
                        InventoryId = details.InventoryId,
                        InventoryDetailId = details.InventoryDetailId,
                        ProductId = details.ProductId,
                        PrimaryUnitId = productInfo?.UnitId,
                        RequestPrimaryQuantity = details.RequestPrimaryQuantity?.Round(),
                        PrimaryQuantity = details.PrimaryQuantity.Round(),
                        UnitPrice = details.UnitPrice,
                        ProductUnitConversionId = details.ProductUnitConversionId,
                        RequestProductUnitConversionQuantity = details.RequestProductUnitConversionQuantity?.Round(),
                        ProductUnitConversionQuantity = details.ProductUnitConversionQuantity.Round(),
                        ProductUnitConversionPrice = details.ProductUnitConversionPrice,
                        FromPackageId = details.FromPackageId,
                        ToPackageId = details.ToPackageId,
                        FromPackageCode = packageInfo?.PackageCode,
                        PackageOptionId = details.PackageOptionId,

                        RefObjectTypeId = details.RefObjectTypeId,
                        RefObjectId = details.RefObjectId,
                        RefObjectCode = details.RefObjectCode,
                        OrderCode = details.OrderCode,
                        POCode = details.Pocode,
                        ProductionOrderCode = details.ProductionOrderCode,

                        ProductOutput = productOutput,
                        ProductUnitConversion = productUnitConversionInfo ?? null,
                        SortOrder = details.SortOrder,
                        Description = details.Description,
                        AccountancyAccountNumberDu = details.AccountancyAccountNumberDu,
                        InventoryRequirementCode = details.InventoryRequirementCode,

                        DepartmentId = details.DepartmentId,
                    };

                    //if (!string.IsNullOrEmpty(detail.InventoryRequirementCode) && inventoryRequirementMap.ContainsKey(detail.InventoryRequirementDetailId.Value))
                    //{
                    //    detail.InventoryRequirementInfo = inventoryRequirementMap[detail.InventoryRequirementDetailId.Value];
                    //}

                    listInventoryDetailsOutput.Add(detail);
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

                IList<MappingInputBillModel> mappingObjects = null;

                if (mappingFunctionKeys != null && mappingFunctionKeys.Count > 0)
                {
                    mappingObjects = _stockDbContext.VMappingOusideImportObject
                        .Where(m => mappingFunctionKeys.Contains(m.MappingFunctionKey) && m.SourceId == inventoryId.ToString())
                        .Select(m => new MappingInputBillModel()
                        {
                            MappingFunctionKey = m.MappingFunctionKey,
                            InputTypeId = m.InputTypeId,
                            SourceId = m.SourceId,
                            InputBillFId = m.InputBillFId
                        })
                        .ToList();
                }

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
                    StockOutput = stockInfo == null ? null : new StockOutput
                    {
                        StockId = stockInfo.StockId,
                        StockName = stockInfo.StockName,
                        StockKeeperName = stockInfo.StockKeeperName,
                        StockKeeperId = stockInfo.StockKeeperId
                    },
                    InventoryDetailOutputList = listInventoryDetailsOutput,
                    FileList = attachedFiles,
                    InputBills = mappingObjects
                };
                return inventoryOutput;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetInventory");
                throw;
            }
        }

        public async Task<(Stream stream, string fileName, string contentType)> InventoryInfoExport(long inventoryId, IList<string> mappingFunctionKeys = null)
        {
            var inventoryExport = new InventoryExportFacade();
            inventoryExport.SetCurrentContext(_currentContextService);
            inventoryExport.SetInventoryService(this);
            inventoryExport.SetOrganizationHelperService(_organizationHelperService);
            inventoryExport.SetProductHelperService(_productHelperService);
            inventoryExport.SetStockHelperService(_stockHelperService);
            return await inventoryExport.InventoryInfoExport(inventoryId, mappingFunctionKeys);
        }

        public CategoryNameModel GetInventoryDetailFieldDataForMapping()
        {
            var result = new CategoryNameModel()
            {
                CategoryId = 1,
                CategoryCode = "Inventory",
                CategoryTitle = "Xuất/Nhập kho",
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
                CategoryId = 1,
                CategoryCode = inventoryTypeId == EnumInventoryType.Input ? "InventoryInput" : "InventoryOutput",
                CategoryTitle = inventoryTypeId == EnumInventoryType.Input ? "Nhập kho" : "Xuất kho",
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

                    if (model.Type == EnumInventoryType.Input)
                    {
                        var inventoryData = inventoryExport.GetInputInventoryModels();

                        foreach (var item in inventoryData)
                        {
                            genCodeContexts.Add(await GenerateInventoryCode(model.Type, item, baseValueChains));

                            inventoryId = await AddInventoryInputDB(item);
                            insertedData.Add(inventoryId, (item.InventoryCode, item));
                        }
                    }
                    else
                    {
                        var inventoryData = await inventoryExport.GetOutputInventoryModels();

                        foreach (var item in inventoryData)
                        {
                            genCodeContexts.Add(await GenerateInventoryCode(model.Type, item, baseValueChains));

                            inventoryId = await AddInventoryOutputDb(item);
                            insertedData.Add(inventoryId, (item.InventoryCode, item));
                        }

                    }

                    await trans.CommitAsync();
                }


                foreach (var item in insertedData)
                {
                    if (model.Type == EnumInventoryType.Input)
                    {
                        await _activityLogService.CreateLog(EnumObjectType.InventoryInput, item.Key, $"Nhập tồn đầu {item.Value.inventoryCode}", item.Value.data.JsonSerialize());
                    }
                    else
                    {
                        await _activityLogService.CreateLog(EnumObjectType.InventoryInput, item.Key, $"Xuất tồn đầu {item.Value.inventoryCode}", item.Value.data.JsonSerialize());
                    }
                }

                foreach (var item in genCodeContexts)
                {
                    await item.ConfirmCode();
                }

                return inventoryId;
            }
        }


        /// <summary>
        /// Thêm mới phiếu nhập kho
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<long> AddInventoryInput(InventoryInModel req)
        {
            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(req.StockId)))
            {
                var ctx = await GenerateInventoryCode(EnumInventoryType.Input, req);

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    var inventoryId = await AddInventoryInputDB(req);
                    await trans.CommitAsync();

                    await _activityLogService.CreateLog(EnumObjectType.InventoryInput, inventoryId, $"Thêm mới phiếu nhập kho, mã: {req.InventoryCode}", req.JsonSerialize());

                    await ctx.ConfirmCode();

                    return inventoryId;
                }
            }

        }

        private async Task<long> AddInventoryInputDB(InventoryInModel req)
        {
            if (req == null || req.InProducts.Count == 0)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            await ValidateInventoryConfig(req.Date.UnixToDateTime(), null);

            req.InventoryCode = req.InventoryCode.Trim();

            var stockInfo = await _stockDbContext.Stock.AsNoTracking().FirstOrDefaultAsync(s => s.StockId == req.StockId);
            if (stockInfo == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy kho");
            }

            //using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(req.StockId)))
            {

                await ValidateInventoryCode(null, req.InventoryCode);

                var issuedDate = req.Date.UnixToDateTime().Value;

                var validInventoryDetails = await ValidateInventoryIn(false, req);

                if (!validInventoryDetails.Code.IsSuccess())
                {
                    throw new BadRequestException(validInventoryDetails.Code);
                }

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
                    BillForm = req.BillForm,
                    BillCode = req.BillCode,
                    BillSerial = req.BillSerial,
                    BillDate = req.BillDate?.UnixToDateTime(),
                    TotalMoney = totalMoney,
                    AccountancyAccountNumber = req.AccountancyAccountNumber,
                    CreatedByUserId = _currentContextService.UserId,
                    UpdatedByUserId = _currentContextService.UserId,
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

                //Move file from tmp folder
                if (req.FileIdList != null)
                {
                    foreach (var fileId in req.FileIdList)
                    {
                        _asyncRunner.RunAsync<IFileService>(f => f.FileAssignToObject(EnumObjectType.InventoryInput, inventoryObj.InventoryId, fileId));
                    }
                }
                return inventoryObj.InventoryId;
            }
        }

        private async Task ValidateInventoryCode(long? inventoryId, string inventoryCode)
        {
            inventoryId = inventoryId ?? 0;
            if (await _stockDbContext.Inventory.AnyAsync(q => q.InventoryId != inventoryId && q.InventoryCode == inventoryCode))
            {
                throw new BadRequestException(InventoryErrorCode.InventoryCodeAlreadyExisted);
            }
        }


        /// <summary>
        /// Thêm mới phiếu xuất kho
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<long> AddInventoryOutput(InventoryOutModel req)
        {
            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(req.StockId)))
            {
                var ctx = await GenerateInventoryCode(EnumInventoryType.Output, req);
                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    var inventoryId = await AddInventoryOutputDb(req);
                    await trans.CommitAsync();

                    await _activityLogService.CreateLog(EnumObjectType.InventoryOutput, inventoryId, $"Thêm mới phiếu xuất kho, mã: {req.InventoryCode}", req.JsonSerialize());

                    await ctx.ConfirmCode();
                    return inventoryId;
                }
            }
        }

        private async Task<GenerateCodeContext> GenerateInventoryCode(EnumInventoryType inventoryTypeId, InventoryModelBase req, Dictionary<string, int> baseValueChains = null)
        {
            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext(baseValueChains);

            var objectTypeId = inventoryTypeId == EnumInventoryType.Input ? EnumObjectType.InventoryInput : EnumObjectType.InventoryOutput;
            var code = await ctx
                .SetConfig(objectTypeId, EnumObjectType.Stock, req.StockId)
                .SetConfigData(0, req.Date)
                .TryValidateAndGenerateCode(_stockDbContext.Inventory, req.InventoryCode, (s, code) => s.InventoryTypeId == (int)inventoryTypeId && s.InventoryCode == code);

            req.InventoryCode = code;
            return ctx;
        }

        private async Task<long> AddInventoryOutputDb(InventoryOutModel req)
        {
            if (req == null || req.OutProducts.Count == 0)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            await ValidateInventoryConfig(req.Date.UnixToDateTime(), null);

            req.InventoryCode = req.InventoryCode.Trim();

            //using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(req.StockId)))
            {
                await ValidateInventoryCode(null, req.InventoryCode);

                var issuedDate = req.Date.UnixToDateTime().Value;

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
                    BillForm = req.BillForm,
                    BillCode = req.BillCode,
                    BillSerial = req.BillSerial,
                    BillDate = req.BillDate?.UnixToDateTime(),
                    AccountancyAccountNumber = req.AccountancyAccountNumber,
                    CreatedByUserId = _currentContextService.UserId,
                    UpdatedByUserId = _currentContextService.UserId,
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
                    throw new BadRequestException(processInventoryOut.Code, processInventoryOut.Message);
                }

                var totalMoney = InputCalTotalMoney(processInventoryOut.Data);

                inventoryObj.TotalMoney = totalMoney;

                await _stockDbContext.InventoryDetail.AddRangeAsync(processInventoryOut.Data);
                await _stockDbContext.SaveChangesAsync();


                //Move file from tmp folder
                if (req.FileIdList != null)
                {
                    foreach (var fileId in req.FileIdList)
                    {
                        _asyncRunner.RunAsync<IFileService>(f => f.FileAssignToObject(EnumObjectType.InventoryOutput, inventoryObj.InventoryId, fileId));
                    }
                }
                return inventoryObj.InventoryId;

            }
        }

        /// <summary>
        /// Cập nhật phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> UpdateInventoryInput(long inventoryId, InventoryInModel req)
        {
            if (inventoryId <= 0)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }

            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(req.StockId)))
            {

                var issuedDate = req.Date.UnixToDateTime().Value;

                var validate = await ValidateInventoryIn(false, req);

                await ValidateInventoryCode(inventoryId, req.InventoryCode);

                if (!validate.Code.IsSuccess())
                {
                    throw new BadRequestException(validate.Code);
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
                            throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
                        }

                        if (inventoryObj.StockId != req.StockId)
                        {
                            trans.Rollback();
                            throw new BadRequestException(InventoryErrorCode.CanNotChangeStock);
                        }

                        if (inventoryObj.IsApproved)
                        {
                            trans.Rollback();
                            throw new BadRequestException(GeneralCode.NotYetSupported);
                        }

                        if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Input)
                        {
                            throw new BadRequestException(GeneralCode.InvalidParams);
                        }

                        await ValidateInventoryConfig(req.Date.UnixToDateTime(), inventoryObj.Date);

                        #endregion

                        var inventoryDetails = await _stockDbContext.InventoryDetail.Where(d => d.InventoryId == inventoryId).ToListAsync();
                        foreach (var d in inventoryDetails)
                        {
                            d.IsDeleted = true;
                            d.UpdatedDatetimeUtc = DateTime.UtcNow;
                        }

                        foreach (var item in validate.Data)
                        {
                            item.InventoryId = inventoryObj.InventoryId;
                        }

                        InventoryInputUpdateData(inventoryObj, req, InputCalTotalMoney(validate.Data));

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
                        await _activityLogService.CreateLog(EnumObjectType.InventoryInput, inventoryObj.InventoryId, messageLog, req.JsonSerialize());
                    }
                    catch (Exception ex)
                    {
                        trans.TryRollbackTransaction();
                        _logger.LogError(ex, "UpdateInventoryInput");
                        throw;
                    }
                }

                //Move file from tmp folder
                if (req.FileIdList != null)
                {
                    foreach (var fileId in req.FileIdList)
                    {
                        _asyncRunner.RunAsync<IFileService>(f => f.FileAssignToObject(EnumObjectType.InventoryInput, inventoryId, fileId));
                    }
                }

                return true;
            }
        }

        protected void InventoryInputUpdateData(Inventory inventoryObj, InventoryInModel req, decimal totalMoney)
        {
            var issuedDate = req.Date.UnixToDateTime().Value;

            //inventoryObj.StockId = req.StockId; Khong cho phep sua kho
            inventoryObj.InventoryCode = req.InventoryCode;
            inventoryObj.Date = issuedDate;
            inventoryObj.Shipper = req.Shipper;
            inventoryObj.Content = req.Content;
            inventoryObj.CustomerId = req.CustomerId;
            inventoryObj.Department = req.Department;
            inventoryObj.StockKeeperUserId = req.StockKeeperUserId;
            inventoryObj.BillForm = req.BillForm;
            inventoryObj.BillCode = req.BillCode;
            inventoryObj.BillSerial = req.BillSerial;
            inventoryObj.BillDate = req.BillDate?.UnixToDateTime();
            inventoryObj.AccountancyAccountNumber = req.AccountancyAccountNumber;
            inventoryObj.UpdatedByUserId = _currentContextService.UserId;
            inventoryObj.TotalMoney = totalMoney;
        }

        /// <summary>
        /// Cập nhật phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> UpdateInventoryOutput(long inventoryId, InventoryOutModel req)
        {
            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(req.StockId)))
            {
                var issuedDate = req.Date.UnixToDateTime().Value;
                await ValidateInventoryCode(inventoryId, req.InventoryCode);

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {

                        var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);
                        if (inventoryObj == null)
                        {
                            trans.Rollback();
                            throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
                        }

                        if (inventoryObj.StockId != req.StockId)
                        {
                            trans.Rollback();
                            throw new BadRequestException(InventoryErrorCode.CanNotChangeStock);
                        }

                        await ValidateInventoryConfig(req.Date.UnixToDateTime(), inventoryObj.Date);

                        var rollbackResult = await RollbackInventoryOutput(inventoryObj);
                        if (!rollbackResult.IsSuccess())
                        {
                            trans.Rollback();
                            throw new BadRequestException(rollbackResult);
                        }

                        if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Output)
                        {
                            throw new BadRequestException(GeneralCode.InvalidParams);
                        }

                        var processInventoryOut = await ProcessInventoryOut(inventoryObj, req);

                        if (!processInventoryOut.Code.IsSuccess())
                        {
                            trans.Rollback();
                            throw new BadRequestException(processInventoryOut.Code, processInventoryOut.Message);
                        }
                        await _stockDbContext.InventoryDetail.AddRangeAsync(processInventoryOut.Data);

                        var totalMoney = InputCalTotalMoney(processInventoryOut.Data);

                        inventoryObj.TotalMoney = totalMoney;

                        //note: update IsApproved after RollbackInventoryOutput
                        inventoryObj.InventoryCode = req.InventoryCode;
                        inventoryObj.Shipper = req.Shipper;
                        inventoryObj.Content = req.Content;
                        inventoryObj.Date = issuedDate;
                        inventoryObj.CustomerId = req.CustomerId;
                        inventoryObj.Department = req.Department;
                        inventoryObj.StockKeeperUserId = req.StockKeeperUserId;

                        inventoryObj.BillForm = req.BillForm;
                        inventoryObj.BillCode = req.BillCode;
                        inventoryObj.BillSerial = req.BillSerial;
                        inventoryObj.BillDate = req.BillDate?.UnixToDateTime();

                        inventoryObj.IsApproved = false;
                        inventoryObj.AccountancyAccountNumber = req.AccountancyAccountNumber;
                        inventoryObj.UpdatedByUserId = _currentContextService.UserId;


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

                        await ReCalculateRemainingAfterUpdate(inventoryObj.InventoryId);

                        trans.Commit();

                        var messageLog = string.Format("Cập nhật phiếu xuất kho, mã: {0}", inventoryObj.InventoryCode);
                        await _activityLogService.CreateLog(EnumObjectType.InventoryOutput, inventoryObj.InventoryId, messageLog, req.JsonSerialize());
                    }
                    catch (Exception ex)
                    {
                        trans.TryRollbackTransaction();
                        _stockDbContext.RollbackEntities();
                        _logger.LogError(ex, "UpdateInventoryOutput");
                        throw;
                    }
                }

                //Move file from tmp folder
                if (req.FileIdList != null)
                {
                    foreach (var fileId in req.FileIdList)
                    {
                        _asyncRunner.RunAsync<IFileService>(f => f.FileAssignToObject(EnumObjectType.InventoryOutput, inventoryId, fileId));
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Xoá phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteInventoryInput(long inventoryId)
        {
            var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(p => p.InventoryId == inventoryId);
            if (inventoryObj == null)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }

            if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Input)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(inventoryObj.StockId)))
            {
                //reload inventory after lock
                inventoryObj = _stockDbContext.Inventory.FirstOrDefault(p => p.InventoryId == inventoryId);

                if (inventoryObj.IsApproved)
                {
                    /*Khong duoc phep xoa phieu nhap da duyet (Cần xóa theo lưu đồ, flow)*/
                    throw new BadRequestException(InventoryErrorCode.NotSupportedYet);

                    //var processResult = await RollBackInventoryInput(inventoryObj);
                    //if (!Equals(processResult, GeneralCode.Success))
                    //{
                    //    trans.Rollback();
                    //    return GeneralCode.InvalidParams;
                    //}
                }

                await ValidateInventoryConfig(null, inventoryObj.Date);

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        inventoryObj.IsDeleted = true;
                        //inventoryObj.IsApproved = false;

                        var inventoryDetails = await _stockDbContext.InventoryDetail.Where(iv => iv.InventoryId == inventoryId).ToListAsync();
                        foreach (var item in inventoryDetails)
                        {
                            item.IsDeleted = true;
                        }

                        await _stockDbContext.SaveChangesAsync();
                        trans.Commit();

                        await _activityLogService.CreateLog(EnumObjectType.InventoryInput, inventoryObj.InventoryId, string.Format("Xóa phiếu nhập kho, mã phiếu {0}", inventoryObj.InventoryCode), inventoryObj.JsonSerialize());

                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.TryRollbackTransaction();
                        _logger.LogError(ex, "DeleteInventoryInput");
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Xoá phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteInventoryOutput(long inventoryId)
        {
            var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(p => p.InventoryId == inventoryId);

            if (inventoryObj == null)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }
            if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Output)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey(inventoryObj.StockId)))
            {
                //reload from db after lock
                inventoryObj = _stockDbContext.Inventory.FirstOrDefault(p => p.InventoryId == inventoryId);

                await ValidateInventoryConfig(null, inventoryObj.Date);

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
                            throw new BadRequestException(GeneralCode.InvalidParams);
                        }

                        //update status after rollback
                        inventoryObj.IsDeleted = true;
                        //inventoryObj.IsApproved = false;

                        await _stockDbContext.SaveChangesAsync();

                        if (inventoryObj.IsApproved)
                        {
                            await ReCalculateRemainingAfterUpdate(inventoryObj.InventoryId);
                        }

                        trans.Commit();

                        await _activityLogService.CreateLog(EnumObjectType.InventoryOutput, inventoryObj.InventoryId, string.Format("Xóa phiếu xuất kho, mã phiếu {0}", inventoryObj.InventoryCode), new { InventoryId = inventoryId }.JsonSerialize());

                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.TryRollbackTransaction();
                        _stockDbContext.RollbackEntities();
                        _logger.LogError(ex, "DeleteInventoryOutput");

                        throw;
                    }
                }
            }
        }


        /// <summary>
        /// Duyệt phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> ApproveInventoryInput(long inventoryId)
        {
            if (inventoryId < 0)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }

            var inventoryObj = _stockDbContext.Inventory.FirstOrDefault(q => q.InventoryId == inventoryId);
            if (inventoryObj == null)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }
            if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Input)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }

            await ValidateInventoryConfig(inventoryObj.Date, inventoryObj.Date);

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
                            throw new BadRequestException(InventoryErrorCode.InventoryAlreadyApproved);
                        }

                        inventoryObj.IsApproved = true;
                        //inventoryObj.UpdatedByUserId = currentUserId;
                        //inventoryObj.UpdatedDatetimeUtc = DateTime.UtcNow;
                        inventoryObj.CensorByUserId = _currentContextService.UserId;
                        inventoryObj.CensorDatetimeUtc = DateTime.UtcNow;

                        await _stockDbContext.SaveChangesAsync();

                        var inventoryDetails = _stockDbContext.InventoryDetail.Where(q => q.InventoryId == inventoryId).ToList();

                        var r = await ProcessInventoryInputApprove(inventoryObj.StockId, inventoryObj.Date, inventoryDetails);

                        if (!r.IsSuccess())
                        {
                            trans.Rollback();
                            throw new BadRequestException(r);
                        }

                        await ReCalculateRemainingAfterUpdate(inventoryId);

                        try
                        {
                            await UpdateProductionOrderStatus(inventoryDetails, EnumProductionStatus.Finished);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi cập nhật trạng thái lệnh sản xuất");
                            throw new Exception("Lỗi cập nhật trạng thái lệnh sản xuất: " + ex.Message, ex);
                        }


                        trans.Commit();

                        var messageLog = $"Duyệt phiếu nhập kho, mã: {inventoryObj.InventoryCode}";
                        await _activityLogService.CreateLog(EnumObjectType.InventoryInput, inventoryObj.InventoryId, messageLog, new { InventoryId = inventoryId }.JsonSerialize());

                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.TryRollbackTransaction();
                        _logger.LogError(ex, "ApproveInventoryInput");
                        throw;
                    }
                }
            }
        }



        /// <summary>
        /// Duyệt phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> ApproveInventoryOutput(long inventoryId)
        {
            if (inventoryId <= 0)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }

            var inventoryObj = await _stockDbContext.Inventory.FirstOrDefaultAsync(q => q.InventoryId == inventoryId);
            if (inventoryObj == null)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }
            if (inventoryObj.InventoryTypeId != (int)EnumInventoryType.Output)
            {
                throw new BadRequestException(InventoryErrorCode.InventoryNotFound);
            }

            await ValidateInventoryConfig(inventoryObj.Date, inventoryObj.Date);

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
                            throw new BadRequestException(InventoryErrorCode.InventoryAlreadyApproved);
                        }

                        inventoryObj.IsApproved = true;
                        //inventoryObj.UpdatedByUserId = currentUserId;
                        //inventoryObj.UpdatedDatetimeUtc = DateTime.UtcNow;
                        inventoryObj.CensorByUserId = _currentContextService.UserId;
                        inventoryObj.CensorDatetimeUtc = DateTime.UtcNow;

                        var inventoryDetails = _stockDbContext.InventoryDetail.Where(d => d.InventoryId == inventoryId).ToList();

                        var fromPackageIds = inventoryDetails.Select(f => f.FromPackageId).ToList();
                        var fromPackages = _stockDbContext.Package.Where(p => fromPackageIds.Contains(p.PackageId)).ToList();

                        var original = fromPackages.ToDictionary(p => p.PackageId, p => p.PrimaryQuantityRemaining.Round());

                        //var groupByProducts = inventoryDetails
                        //    .GroupBy(g => new { g.ProductId, g.ProductUnitConversionId })
                        //    .Select(g => new
                        //    {
                        //        g.Key.ProductId,
                        //        g.Key.ProductUnitConversionId,
                        //        OutPrimary = g.Sum(d => d.PrimaryQuantity),
                        //        OutSecondary = g.Sum(d => d.ProductUnitConversionQuantity)
                        //    });
                        //foreach (var product in groupByProducts)
                        //{

                        //    var validate = await ValidateBalanceForOutput(inventoryObj.StockId, product.ProductId, inventoryObj.InventoryId, product.ProductUnitConversionId, inventoryObj.Date, product.OutPrimary, product.OutSecondary);

                        //    if (!validate.IsSuccessCode())
                        //    {
                        //        trans.Rollback();

                        //        throw new BadRequestException(validate.Code, validate.Message);
                        //    }
                        //}

                        foreach (var detail in inventoryDetails)
                        {
                            var fromPackageInfo = fromPackages.FirstOrDefault(p => p.PackageId == detail.FromPackageId);
                            if (fromPackageInfo == null) throw new BadRequestException(PackageErrorCode.PackageNotFound);

                            fromPackageInfo.PrimaryQuantityWaiting = fromPackageInfo.PrimaryQuantityWaiting.SubDecimal(detail.PrimaryQuantity);
                            fromPackageInfo.ProductUnitConversionWaitting = fromPackageInfo.ProductUnitConversionWaitting.SubDecimal(detail.ProductUnitConversionQuantity);
                            if (fromPackageInfo.PrimaryQuantityWaiting == 0)
                            {
                                fromPackageInfo.ProductUnitConversionWaitting = 0;
                            }
                            if (fromPackageInfo.ProductUnitConversionWaitting == 0)
                            {
                                fromPackageInfo.PrimaryQuantityWaiting = 0;
                            }

                            fromPackageInfo.PrimaryQuantityRemaining = fromPackageInfo.PrimaryQuantityRemaining.SubDecimal(detail.PrimaryQuantity);
                            fromPackageInfo.ProductUnitConversionRemaining = fromPackageInfo.ProductUnitConversionRemaining.SubDecimal(detail.ProductUnitConversionQuantity);
                            if (fromPackageInfo.PrimaryQuantityRemaining == 0)
                            {
                                fromPackageInfo.ProductUnitConversionRemaining = 0;
                            }
                            if (fromPackageInfo.ProductUnitConversionRemaining == 0)
                            {
                                fromPackageInfo.PrimaryQuantityRemaining = 0;
                            }

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
                                    throw new BadRequestException(ProductErrorCode.ProductNotFound);
                                }


                                var message = $"Số dư trong kho mặt hàng {productInfo.ProductCode} ({original[detail.FromPackageId.Value].Format()} {productInfo.ProductUnitConversionName}) không đủ để xuất ";
                                var samPackages = inventoryDetails.Where(d => d.FromPackageId == detail.FromPackageId);

                                var total = samPackages.Sum(d => d.PrimaryQuantity).Format();

                                message += $" < {total} {productInfo.ProductUnitConversionName} = "
                                    + string.Join(" + ", samPackages.Select(d => d.PrimaryQuantity.Format()));

                                trans.Rollback();

                                throw new BadRequestException(InventoryErrorCode.NotEnoughQuantity, message);
                            }

                            ValidatePackage(fromPackageInfo);

                            var stockProduct = await EnsureStockProduct(inventoryObj.StockId, detail.ProductId, detail.ProductUnitConversionId);

                            stockProduct.PrimaryQuantityWaiting = stockProduct.PrimaryQuantityWaiting.SubDecimal(detail.PrimaryQuantity);
                            stockProduct.ProductUnitConversionWaitting = stockProduct.ProductUnitConversionWaitting.SubDecimal(detail.ProductUnitConversionQuantity);
                            if (stockProduct.PrimaryQuantityWaiting == 0)
                            {
                                stockProduct.ProductUnitConversionWaitting = 0;
                            }
                            if (stockProduct.ProductUnitConversionWaitting == 0)
                            {
                                stockProduct.PrimaryQuantityWaiting = 0;
                            }

                            stockProduct.PrimaryQuantityRemaining = stockProduct.PrimaryQuantityRemaining.SubDecimal(detail.PrimaryQuantity);
                            stockProduct.ProductUnitConversionRemaining = stockProduct.ProductUnitConversionRemaining.SubDecimal(detail.ProductUnitConversionQuantity);
                            if (stockProduct.PrimaryQuantityRemaining == 0)
                            {
                                stockProduct.ProductUnitConversionRemaining = 0;
                            }
                            if (stockProduct.ProductUnitConversionRemaining == 0)
                            {
                                stockProduct.PrimaryQuantityRemaining = 0;
                            }

                            ValidateStockProduct(stockProduct);
                        }

                        await _stockDbContext.SaveChangesAsync();

                        await ReCalculateRemainingAfterUpdate(inventoryId);


                        try
                        {
                            await UpdateProductionOrderStatus(inventoryDetails, EnumProductionStatus.Processing);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi cập nhật trạng thái lệnh sản xuất");
                            throw new Exception("Lỗi cập nhật trạng thái lệnh sản xuất: " + ex.Message, ex);
                        }

                        trans.Commit();

                        var messageLog = $"Duyệt phiếu xuất kho, mã: {inventoryObj.InventoryCode}";
                        await _activityLogService.CreateLog(EnumObjectType.InventoryOutput, inventoryObj.InventoryId, messageLog, new { InventoryId = inventoryId }.JsonSerialize());

                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.TryRollbackTransaction();
                        _logger.LogError(ex, "ApproveInventoryOutput");
                        throw;
                    }
                }
            }
        }

        private async Task UpdateProductionOrderStatus(IList<InventoryDetail> inventoryDetails, EnumProductionStatus status)
        {
            // update trạng thái cho lệnh sản xuất
            var requirementDetailIds = inventoryDetails.Where(d => d.InventoryRequirementDetailId.HasValue).Select(d => d.InventoryRequirementDetailId).Distinct().ToList();
            var requirementDetails = _stockDbContext.InventoryRequirementDetail
                .Include(rd => rd.InventoryRequirement)
                .Where(rd => requirementDetailIds.Contains(rd.InventoryRequirementDetailId))
                .ToList();
            var productionOrderCodes = requirementDetails
                .Where(rd => !string.IsNullOrEmpty(rd.ProductionOrderCode))
                .Select(rd => rd.ProductionOrderCode)
                .Distinct()
                .ToList();

            Dictionary<string, DataTable> inventoryMap = new Dictionary<string, DataTable>();

            foreach (var productionOrderCode in productionOrderCodes)
            {
                var parammeters = new SqlParameter[]
                {
                        new SqlParameter("@ProductionOrderCode", productionOrderCode)
                };
                var resultData = await _stockDbContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByProductionOrder_new", parammeters);
                inventoryMap.Add(productionOrderCode, resultData);
                await _productionOrderHelperService.UpdateProductionOrderStatus(productionOrderCode, resultData, status);
            }

            // update trạng thái cho phân công công việc
            var assignments = requirementDetails
                .Where(rd => !string.IsNullOrEmpty(rd.ProductionOrderCode) && rd.DepartmentId.GetValueOrDefault() > 0)
                .Select(rd => new
                {
                    ProductionOrderCode = rd.ProductionOrderCode,
                    DepartmentId = rd.DepartmentId.Value
                })
                .Distinct()
                .ToList();

            foreach (var assignment in assignments)
            {
                await _productionHandoverService.ChangeAssignedProgressStatus(assignment.ProductionOrderCode, assignment.DepartmentId, inventoryMap[assignment.ProductionOrderCode]);
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
            var productList = await _productService.GetList(keyword, new int[0], "", new int[0], new int[0], page, size, null, null, null, stockIdList);

            var pagedData = productList.List;

            var productIdList = pagedData.Select(p => p.ProductId).ToList();

            var stockProductData = await _stockDbContext.StockProduct.AsNoTracking().Where(q => stockIdList.Contains(q.StockId)).Where(q => productIdList.Contains(q.ProductId)).ToListAsync();

            foreach (var item in pagedData)
            {
                item.StockProductModelList =
                    stockProductData.Where(q => q.ProductId == item.ProductId).Select(q => new StockProductOutput
                    {
                        StockId = q.StockId,
                        ProductId = q.ProductId,
                        PrimaryUnitId = item.UnitId,
                        PrimaryQuantityRemaining = q.PrimaryQuantityRemaining.Round(),
                        ProductUnitConversionId = q.ProductUnitConversionId,
                        ProductUnitConversionRemaining = q.ProductUnitConversionRemaining.Round()
                    }).ToList();
            }

            return productList;
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
                            pk.Description,
                            p.UnitId,
                            pk.PrimaryQuantityRemaining,
                            pk.PrimaryQuantityWaiting,
                            pk.ProductUnitConversionRemaining,
                            pk.ProductUnitConversionWaitting,
                            pk.CreatedDatetimeUtc,
                            pk.UpdatedDatetimeUtc,
                            pk.OrderCode,
                            pk.Pocode,
                            pk.ProductionOrderCode
                        };

            var total = await query.CountAsync();

            var packageData = size > 0 ? await query.AsNoTracking().Skip((page - 1) * size).Take(size).ToListAsync() : await query.AsNoTracking().ToListAsync();

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
                    Description = item.Description,
                    PrimaryUnitId = item.UnitId,
                    ProductUnitConversionId = item.ProductUnitConversionId,
                    PrimaryQuantityWaiting = item.PrimaryQuantityWaiting.Round(),
                    PrimaryQuantityRemaining = item.PrimaryQuantityRemaining.Round(),
                    ProductUnitConversionWaitting = item.ProductUnitConversionWaitting.Round(),
                    ProductUnitConversionRemaining = item.ProductUnitConversionRemaining.Round(),

                    CreatedDatetimeUtc = item.CreatedDatetimeUtc.GetUnix(),
                    UpdatedDatetimeUtc = item.UpdatedDatetimeUtc.GetUnix(),
                    LocationOutputModel = locationOutputModel,
                    ProductUnitConversionModel = productUnitConversionData.FirstOrDefault(q => q.ProductUnitConversionId == item.ProductUnitConversionId) ?? null,
                    OrderCode = item.OrderCode,
                    POCode = item.Pocode,
                    ProductionOrderCode = item.ProductionOrderCode

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
            var productList = await _productService.GetList(keyword, new int[0], "", new int[0], new int[0], page, size, null, null, null);

            var pagedData = productList.List;

            var productIdList = pagedData.Select(p => p.ProductId).ToList();

            var stockProductData = await _stockDbContext.StockProduct.AsNoTracking().Where(q => stockIdList.Contains(q.StockId)).Where(q => productIdList.Contains(q.ProductId)).ToListAsync();

            foreach (var item in pagedData)
            {
                item.StockProductModelList =
                    stockProductData.Where(q => q.ProductId == item.ProductId).Select(q => new StockProductOutput
                    {
                        StockId = q.StockId,
                        ProductId = q.ProductId,
                        PrimaryUnitId = item.UnitId,
                        PrimaryQuantityRemaining = q.PrimaryQuantityRemaining.Round(),
                        ProductUnitConversionId = q.ProductUnitConversionId,
                        ProductUnitConversionRemaining = q.ProductUnitConversionRemaining.Round()
                    }).ToList();
            }

            return productList;

        }


        #region Private helper method

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

                            var newPackage = await CreateNewPackage(stockId, date, item);
                            item.ToPackageId = newPackage.PackageId;
                            break;
                        default:
                            return GeneralCode.NotYetSupported;
                    }
                else
                {
                    var newPackage = await CreateNewPackage(stockId, date, item);

                    item.ToPackageId = newPackage.PackageId;
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
            //var errorInventoryId = new SqlParameter("@ErrorInventoryId", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
            var errorIventoryDetailId = new SqlParameter("@ErrorIventoryDetailId", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

            // await _stockDbContext.Database.ExecuteSqlRawAsync("EXEC usp_InventoryDetail_UpdatePrimaryQuantityRemanings_Event @UpdatedInventoryId = @UpdatedInventoryId", new SqlParameter("@UpdatedInventoryId", inventoryId), errorInventoryId);

            await _stockDbContext.ExecuteNoneQueryProcedure("usp_InventoryDetail_UpdatePrimaryQuantityRemanings_Event", new[] { new SqlParameter("@UpdatedInventoryId", inventoryId), errorIventoryDetailId });
            //var inventoryTrackingFacade = await InventoryTrackingFacadeFactory.Create(_stockDbContext, inventoryId);
            //await inventoryTrackingFacade.Execute();

            var inventoryDetailId = (errorIventoryDetailId.Value as long?).GetValueOrDefault();
            if (inventoryDetailId > 0)
            {
                var errorInfo = await (
                    from iv in _stockDbContext.Inventory
                    join id in _stockDbContext.InventoryDetail on iv.InventoryId equals id.InventoryId
                    join p in _stockDbContext.Product on id.ProductId equals p.ProductId into ps
                    from p in ps.DefaultIfEmpty()
                    where id.InventoryDetailId == inventoryDetailId
                    select new
                    {
                        iv.Date,
                        iv.InventoryId,
                        iv.InventoryCode,
                        iv.InventoryTypeId,
                        ProductCode = p == null ? null : p.ProductCode,
                        ProductId = p == null ? (int?)null : p.ProductId,
                        ProductName = p == null ? null : p.ProductName,

                        id.InventoryDetailId,
                        id.PrimaryQuantityRemaning
                    }).FirstOrDefaultAsync();
                if (errorInfo == null)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Có phiếu bị lỗi. Không thể lấy thông tin chi tiết lỗi");
                }
                else
                {

                    var message = $"Số lượng \"{errorInfo.ProductCode}\" trong kho tại thời điểm {errorInfo.Date:dd-MM-yyyy} phiếu " +
                        $"{(errorInfo.InventoryTypeId == (int)EnumInventoryType.Input ? "Nhập" : "Xuất")} {errorInfo.InventoryCode} không đủ. Số tồn là " +
                       $"{errorInfo.PrimaryQuantityRemaning.Value.Format()} không hợp lệ";

                    throw new BadRequestException(GeneralCode.InvalidParams, message);

                }
            }

        }


        private void ValidatePackage(Package package)
        {

            if (package.PrimaryQuantityWaiting < 0) throw new Exception("Package Negative PrimaryQuantityWaiting! " + package.PackageId);

            if (package.PrimaryQuantityRemaining < 0) throw new Exception("Package Negative PrimaryQuantityRemaining! " + package.PackageId);

            if (package.ProductUnitConversionWaitting < 0) throw new Exception("Package Negative ProductUnitConversionWaitting! " + package.PackageId);

            if (package.ProductUnitConversionRemaining < 0)
            {
                throw new Exception("Package Negative ProductUnitConversionRemaining! " + package.PackageId);
            }


        }

        private void ValidateStockProduct(StockProduct stockProduct)
        {

            if (stockProduct.PrimaryQuantityWaiting < 0) throw new Exception("Stock Negative PrimaryQuantityWaiting! " + stockProduct.StockProductId);

            if (stockProduct.PrimaryQuantityRemaining < 0) throw new Exception("Stock Negative PrimaryQuantityRemaining! " + stockProduct.StockProductId);

            if (stockProduct.ProductUnitConversionWaitting < 0) throw new Exception("Stock Negative ProductUnitConversionWaitting! " + stockProduct.StockProductId);

            if (stockProduct.ProductUnitConversionRemaining < 0) throw new Exception("Stock Negative ProductUnitConversionRemaining! " + stockProduct.StockProductId);

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

        private async Task<ServiceResult<IList<InventoryDetail>>> ValidateInventoryIn(bool isApproved, InventoryInModel req)
        {
            if (req.InProducts == null)
                req.InProducts = new List<InventoryInProductModel>();

            var productIds = req.InProducts.Select(p => p.ProductId).Distinct().ToList();

            var productInfos = (await _stockDbContext.Product.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToListAsync()).ToDictionary(p => p.ProductId, p => p);

            var productUnitConversions = await _stockDbContext.ProductUnitConversion.Where(p => productIds.Contains(p.ProductId)).AsNoTracking().ToListAsync();

            var toPackageIds = req.InProducts.Select(p => p.ToPackageId).ToList();
            var toPackages = await _stockDbContext.Package.Where(p => toPackageIds.Contains(p.PackageId) && p.PackageTypeId == (int)EnumPackageType.Custom).ToListAsync();

            var inventoryDetailList = new List<InventoryDetail>(req.InProducts.Count);
            foreach (var details in req.InProducts)
            {
                productInfos.TryGetValue(details.ProductId, out var productInfo);
                if (productInfo == null)
                {
                    return ProductErrorCode.ProductNotFound;
                }
                var puDefault = productUnitConversions.FirstOrDefault(c => c.ProductId == details.ProductId && c.IsDefault);

                var puInfo = productUnitConversions.FirstOrDefault(c => c.ProductUnitConversionId == details.ProductUnitConversionId);
                if (puInfo == null)
                {
                    return ProductUnitConversionErrorCode.ProductUnitConversionNotFound;
                }
                if (puInfo.ProductId != details.ProductId)
                {
                    return ProductUnitConversionErrorCode.ProductUnitConversionNotBelongToProduct;
                }

                if ((puInfo.IsFreeStyle ?? false) == false)
                {
                    var (isSuccess, pucQuantity) = Utils.GetProductUnitConversionQuantityFromPrimaryQuantity(details.PrimaryQuantity, puInfo.FactorExpression, details.ProductUnitConversionQuantity);
                    if (isSuccess)
                    {
                        details.ProductUnitConversionQuantity = pucQuantity;
                    }
                    else
                    {
                        _logger.LogWarning($"Wrong pucQuantity input data: PrimaryQuantity={details.PrimaryQuantity}, FactorExpression={puInfo.FactorExpression}, ProductUnitConversionQuantity={details.ProductUnitConversionQuantity}, evalData={pucQuantity}");
                        //return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                        throw new BadRequestException(ProductUnitConversionErrorCode.SecondaryUnitConversionError,
                            $"Không thể tính giá trị đơn vị chuyển đổi \"{puInfo.ProductUnitConversionName}\" sản phẩm \"{productInfo.ProductCode}\"");
                    }
                }

                if (!isApproved && details.ProductUnitConversionQuantity <= 0)
                {
                    //return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                    throw new BadRequestException(ProductUnitConversionErrorCode.SecondaryUnitConversionError,
                        $"Không thể tính giá trị đơn vị chuyển đổi \"{puInfo.ProductUnitConversionName}\" sản phẩm \"{productInfo.ProductCode}\"");
                }

                // }

                if (!isApproved)
                {
                    if (details.ProductUnitConversionQuantity <= 0 || details.PrimaryQuantity <= 0)
                    {
                        return GeneralCode.InvalidParams;
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
                    RequestPrimaryQuantity = details.RequestPrimaryQuantity?.Round(puDefault.DecimalPlace),
                    PrimaryQuantity = details.PrimaryQuantity.Round(puDefault.DecimalPlace),
                    UnitPrice = details.UnitPrice.Round(puDefault.DecimalPlace),
                    ProductUnitConversionId = details.ProductUnitConversionId,
                    RequestProductUnitConversionQuantity = details.RequestProductUnitConversionQuantity?.Round(puInfo.DecimalPlace),
                    ProductUnitConversionQuantity = details.ProductUnitConversionQuantity.Round(puInfo.DecimalPlace),
                    ProductUnitConversionPrice = details.ProductUnitConversionPrice.Round(puInfo.DecimalPlace),
                    RefObjectTypeId = details.RefObjectTypeId,
                    RefObjectId = details.RefObjectId,
                    RefObjectCode = details.RefObjectCode,
                    OrderCode = details.OrderCode,
                    Pocode = details.POCode,
                    ProductionOrderCode = details.ProductionOrderCode,
                    FromPackageId = null,
                    ToPackageId = details.ToPackageId,
                    PackageOptionId = (int)details.PackageOptionId,
                    SortOrder = details.SortOrder,
                    Description = details.Description,
                    AccountancyAccountNumberDu = details.AccountancyAccountNumberDu,
                    InventoryRequirementCode = details.InventoryRequirementCode,
                    DepartmentId = details.DepartmentId,
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

            foreach (var detail in req.OutProducts)
            {
                var fromPackageInfo = fromPackages.FirstOrDefault(p => p.PackageId == detail.FromPackageId);
                if (fromPackageInfo == null) return PackageErrorCode.PackageNotFound;

                if (fromPackageInfo.ProductId != detail.ProductId
                    || fromPackageInfo.ProductUnitConversionId != detail.ProductUnitConversionId
                    || fromPackageInfo.StockId != req.StockId)
                {
                    _logger.LogInformation($"InventoryService.ProcessInventoryOut error InvalidPackage. ProductId: {detail.ProductId} , FromPackageId: {detail.FromPackageId}, ProductUnitConversionId: {detail.ProductUnitConversionId}");
                    return InventoryErrorCode.InvalidPackage;
                }

                var productInfo = productInfos.FirstOrDefault(p => p.ProductId == detail.ProductId);
                if (productInfo == null)
                {
                    return ProductErrorCode.ProductNotFound;
                }

                var primaryQualtity = detail.PrimaryQuantity;

                var puDefault = productUnitConversions.FirstOrDefault(c => c.ProductId == detail.ProductId && c.IsDefault);

                var puInfo = productUnitConversions.FirstOrDefault(c => c.ProductUnitConversionId == detail.ProductUnitConversionId);
                if (puInfo == null)
                {
                    _logger.LogInformation($"InventoryService.ProcessInventoryOut error ProductUnitConversionNotFound. ProductId: {detail.ProductId} , FromPackageId: {detail.FromPackageId}, ProductUnitConversionId: {detail.ProductUnitConversionId}");
                    return ProductUnitConversionErrorCode.ProductUnitConversionNotFound;
                }

                if (puInfo.ProductId != detail.ProductId)
                {
                    return ProductUnitConversionErrorCode.ProductUnitConversionNotBelongToProduct;
                }

                var primaryUnit = productUnitConversions.FirstOrDefault(c => c.IsDefault && c.ProductId == productInfo.ProductId);

                var errorMessage = $"Số dư trong kiện {fromPackageInfo.PackageCode} mặt hàng {productInfo.ProductCode} ({fromPackageInfo.PrimaryQuantityRemaining.Format()} {primaryUnit?.ProductUnitConversionName}) ";
                var samPackages = req.OutProducts.Where(d => d.FromPackageId == detail.FromPackageId);

                var totalOut = samPackages.Sum(d => d.PrimaryQuantity);
                var isEnough = totalOut < fromPackageInfo.PrimaryQuantityRemaining;

                errorMessage += $" {(isEnough ? ">=" : "<")} {totalOut.Format()} {primaryUnit?.ProductUnitConversionName} ";
                if (!isEnough)
                {
                    errorMessage += " không đủ để xuất";
                }

                if (fromPackageInfo.PrimaryQuantityRemaining == 0 || fromPackageInfo.ProductUnitConversionRemaining == 0)
                {
                    _logger.LogInformation($"InventoryService.ProcessInventoryOut error NotEnoughQuantity. ProductId: {detail.ProductId} , packageId: {fromPackageInfo.PackageId} PrimaryQuantityRemaining: {fromPackageInfo.PrimaryQuantityRemaining}, ProductUnitConversionRemaining: {fromPackageInfo.ProductUnitConversionRemaining}, req: {req.JsonSerialize()} ");

                    return (InventoryErrorCode.NotEnoughQuantity, errorMessage);
                }

                //if (details.ProductUnitConversionQuantity <= 0 && primaryQualtity > 0)
                //{
                if ((puInfo.IsFreeStyle ?? false) == false)
                {
                    var (isSuccess, pucQuantity) = Utils.GetProductUnitConversionQuantityFromPrimaryQuantity(detail.PrimaryQuantity, fromPackageInfo.ProductUnitConversionRemaining / fromPackageInfo.PrimaryQuantityRemaining, detail.ProductUnitConversionQuantity);
                    if (isSuccess)
                    {
                        detail.ProductUnitConversionQuantity = pucQuantity;
                    }
                    else
                    {
                        _logger.LogWarning($"Wrong pucQuantity input data: PrimaryQuantity={detail.PrimaryQuantity}, FactorExpression={fromPackageInfo.ProductUnitConversionRemaining / fromPackageInfo.PrimaryQuantityRemaining}, ProductUnitConversionQuantity={detail.ProductUnitConversionQuantity}, evalData={pucQuantity}");
                        //return ProductUnitConversionErrorCode.SecondaryUnitConversionError;
                        throw new BadRequestException(ProductUnitConversionErrorCode.SecondaryUnitConversionError, $"{productInfo.ProductCode} không thể tính giá trị ĐVCĐ, tính theo tỷ lệ: {pucQuantity.Format()}, nhập vào {detail.ProductUnitConversionQuantity.Format()}");
                    }
                }

                if (!(detail.ProductUnitConversionQuantity > 0))
                {
                    _logger.LogInformation($"InventoryService.ProcessInventoryOut error PrimaryUnitConversionError. ProductId: {detail.ProductId} , FromPackageId: {detail.FromPackageId}, ProductUnitConversionId: {detail.ProductUnitConversionId}, FactorExpression: {puInfo.FactorExpression}, PrimaryQuantity: {detail.PrimaryQuantity}, ProductUnitConversionQuantity: {detail.ProductUnitConversionQuantity}");
                    return ProductUnitConversionErrorCode.PrimaryUnitConversionError;
                }



                if (Math.Abs(detail.ProductUnitConversionQuantity - fromPackageInfo.ProductUnitConversionRemaining) <= MINIMUM_JS_NUMBER)
                {
                    detail.ProductUnitConversionQuantity = fromPackageInfo.ProductUnitConversionRemaining;
                }

                if (Math.Abs(primaryQualtity - fromPackageInfo.PrimaryQuantityRemaining) <= MINIMUM_JS_NUMBER)
                {
                    primaryQualtity = fromPackageInfo.PrimaryQuantityRemaining;

                }

                if (primaryQualtity > fromPackageInfo.PrimaryQuantityRemaining)
                {
                    _logger.LogInformation($"InventoryService.ProcessInventoryOut error NotEnoughQuantity. ProductId: {detail.ProductId} , ProductUnitConversionQuantity: {detail.ProductUnitConversionQuantity}, ProductUnitConversionRemaining: {fromPackageInfo.ProductUnitConversionRemaining}");

                    return (InventoryErrorCode.NotEnoughQuantity, errorMessage);
                }

                inventoryDetailList.Add(new InventoryDetail
                {
                    InventoryId = inventory.InventoryId,
                    ProductId = detail.ProductId,
                    RequestPrimaryQuantity = detail.RequestPrimaryQuantity?.Round(puDefault.DecimalPlace),
                    PrimaryQuantity = primaryQualtity.Round(puDefault.DecimalPlace),
                    UnitPrice = detail.UnitPrice.Round(puDefault.DecimalPlace),
                    ProductUnitConversionId = detail.ProductUnitConversionId,
                    RequestProductUnitConversionQuantity = detail.RequestProductUnitConversionQuantity?.Round(puInfo.DecimalPlace),
                    ProductUnitConversionQuantity = detail.ProductUnitConversionQuantity.Round(puInfo.DecimalPlace),
                    ProductUnitConversionPrice = detail.ProductUnitConversionPrice.Round(puInfo.DecimalPlace),
                    RefObjectTypeId = detail.RefObjectTypeId,
                    RefObjectId = detail.RefObjectId,
                    RefObjectCode = detail.RefObjectCode,
                    OrderCode = detail.OrderCode,
                    Pocode = detail.POCode,
                    ProductionOrderCode = detail.ProductionOrderCode,
                    FromPackageId = detail.FromPackageId,
                    ToPackageId = null,
                    PackageOptionId = null,
                    SortOrder = detail.SortOrder,
                    Description = detail.Description,
                    AccountancyAccountNumberDu = detail.AccountancyAccountNumberDu,
                    InventoryRequirementCode = detail.InventoryRequirementCode,
                    DepartmentId = detail.DepartmentId,
                });

                fromPackageInfo.PrimaryQuantityWaiting = fromPackageInfo.PrimaryQuantityWaiting.AddDecimal(primaryQualtity);
                fromPackageInfo.ProductUnitConversionWaitting = fromPackageInfo.ProductUnitConversionWaitting.AddDecimal(detail.ProductUnitConversionQuantity);

                var stockProductInfo = await EnsureStockProduct(inventory.StockId, fromPackageInfo.ProductId, fromPackageInfo.ProductUnitConversionId);

                stockProductInfo.PrimaryQuantityWaiting = stockProductInfo.PrimaryQuantityWaiting.AddDecimal(primaryQualtity);
                stockProductInfo.ProductUnitConversionWaitting = stockProductInfo.ProductUnitConversionWaitting.AddDecimal(detail.ProductUnitConversionQuantity);
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
                stockProductInfo = new StockProduct()
                {
                    StockId = stockId,
                    ProductId = productId,
                    ProductUnitConversionId = productUnitConversionId,
                    PrimaryQuantityWaiting = 0,
                    PrimaryQuantityRemaining = 0,
                    ProductUnitConversionWaitting = 0,
                    ProductUnitConversionRemaining = 0,
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
                        stockProductInfo.PrimaryQuantityRemaining = stockProductInfo.PrimaryQuantityRemaining.AddDecimal(detail.PrimaryQuantity);
                        stockProductInfo.ProductUnitConversionRemaining = stockProductInfo.ProductUnitConversionRemaining.AddDecimal(detail.ProductUnitConversionQuantity);
                        break;
                    }
                case EnumInventoryType.Output:
                    {
                        stockProductInfo.PrimaryQuantityRemaining = stockProductInfo.PrimaryQuantityRemaining.SubDecimal(detail.PrimaryQuantity);
                        stockProductInfo.ProductUnitConversionRemaining = stockProductInfo.ProductUnitConversionRemaining.SubDecimal(detail.ProductUnitConversionQuantity);
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
            packageInfo.PrimaryQuantityRemaining = packageInfo.PrimaryQuantityRemaining.AddDecimal(detail.PrimaryQuantity);
            //packageInfo.ProductUnitConversionQuantity += detail.ProductUnitConversionQuantity;
            packageInfo.ProductUnitConversionRemaining = packageInfo.ProductUnitConversionRemaining.AddDecimal(detail.ProductUnitConversionQuantity);
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
                ensureDefaultPackage = new Package()
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
                };

                await _stockDbContext.Package.AddAsync(ensureDefaultPackage);
            }

            //ensureDefaultPackage.PrimaryQuantity += detail.PrimaryQuantity;
            ensureDefaultPackage.PrimaryQuantityRemaining = ensureDefaultPackage.PrimaryQuantityRemaining.AddDecimal(detail.PrimaryQuantity);
            //ensureDefaultPackage.ProductUnitConversionQuantity += detail.ProductUnitConversionQuantity;
            ensureDefaultPackage.ProductUnitConversionRemaining = ensureDefaultPackage.ProductUnitConversionRemaining.AddDecimal(detail.ProductUnitConversionQuantity);

            await _stockDbContext.SaveChangesAsync();

            return ensureDefaultPackage;
        }

        private async Task<PackageEntity> CreateNewPackage(int stockId, DateTime date, InventoryDetail detail)
        {
            var config = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.Package, EnumObjectType.Package, 0, null, null, date.GetUnix());

            var newPackageCodeResult = await _customGenCodeHelperService.GenerateCode(config.CustomGenCodeId, config.CurrentLastValue.LastValue, null, null, date.GetUnix());

            var newPackage = new Package()
            {
                PackageTypeId = (int)EnumPackageType.Custom,
                PackageCode = newPackageCodeResult?.CustomCode,
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
            };
            await _stockDbContext.Package.AddAsync(newPackage);
            await _stockDbContext.SaveChangesAsync();

            await _customGenCodeHelperService.ConfirmCode(config.CurrentLastValue);

            await _activityLogService.CreateLog(EnumObjectType.Package, newPackage.PackageId, "Tạo thông tin kiện", newPackage.JsonSerialize());

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
                    fromPackageInfo.PrimaryQuantityWaiting = fromPackageInfo.PrimaryQuantityWaiting.SubDecimal(detail.PrimaryQuantity);
                    fromPackageInfo.ProductUnitConversionWaitting = fromPackageInfo.ProductUnitConversionWaitting.SubDecimal(detail.ProductUnitConversionQuantity);

                    stockProductInfo.PrimaryQuantityWaiting = stockProductInfo.PrimaryQuantityWaiting.SubDecimal(detail.PrimaryQuantity);
                    stockProductInfo.ProductUnitConversionWaitting = stockProductInfo.ProductUnitConversionWaitting.SubDecimal(detail.ProductUnitConversionQuantity);
                }
                else
                {
                    fromPackageInfo.PrimaryQuantityRemaining = fromPackageInfo.PrimaryQuantityRemaining.AddDecimal(detail.PrimaryQuantity);
                    fromPackageInfo.ProductUnitConversionRemaining = fromPackageInfo.ProductUnitConversionRemaining.AddDecimal(detail.ProductUnitConversionQuantity);

                    stockProductInfo.PrimaryQuantityRemaining = stockProductInfo.PrimaryQuantityRemaining.AddDecimal(detail.PrimaryQuantity);
                    stockProductInfo.ProductUnitConversionRemaining = stockProductInfo.ProductUnitConversionRemaining.AddDecimal(detail.ProductUnitConversionQuantity);
                }

                ValidatePackage(fromPackageInfo);
                ValidateStockProduct(stockProductInfo);

                detail.IsDeleted = true;
            }

            await _stockDbContext.SaveChangesAsync();


            return GeneralCode.Success;
        }

        //private async Task<ServiceResult> ValidateBalanceForOutput(int stockId, int productId, long currentInventoryId, int productUnitConversionId, DateTime endDate, decimal outPrimary, decimal outSecondary)
        //{
        //    var sums = await (
        //        from id in _stockDbContext.InventoryDetail
        //        join iv in _stockDbContext.Inventory on id.InventoryId equals iv.InventoryId
        //        where iv.StockId == stockId
        //        && id.ProductId == productId
        //        && id.ProductUnitConversionId == productUnitConversionId
        //        && iv.Date <= endDate
        //        && iv.IsApproved
        //        && iv.InventoryId != currentInventoryId
        //        select new
        //        {
        //            iv.InventoryTypeId,
        //            id.PrimaryQuantity,
        //            id.ProductUnitConversionQuantity
        //        }).GroupBy(g => true)
        //           .Select(g => new
        //           {
        //               TotalPrimary = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity),
        //               TotalSecondary = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.ProductUnitConversionQuantity : -d.ProductUnitConversionQuantity)
        //           }).FirstAsync();


        //    if (sums.TotalPrimary.SubDecimal(outPrimary) < 0 || sums.TotalSecondary.SubDecimal(outSecondary) < 0)
        //    {
        //        var productCode = await _stockDbContext
        //                            .Product
        //                            .Where(p => p.ProductId == productId)
        //                            .Select(p => p.ProductCode)
        //                            .FirstOrDefaultAsync();

        //        var total = sums.TotalSecondary;
        //        var output = outSecondary;

        //        if (sums.TotalPrimary - outPrimary < MINIMUM_JS_NUMBER)
        //        {
        //            total = sums.TotalPrimary;
        //            output = outPrimary;
        //        }


        //        var message = $"Số lượng \"{productCode}\" trong kho tại thời điểm {endDate:dd-MM-yyyy} là " +
        //           $"{total.Format()} không đủ để xuất ({output.Format()})";

        //        return (InventoryErrorCode.NotEnoughQuantity, message);
        //    }

        //    return GeneralCode.Success;

        //}

        protected async Task ValidateInventoryConfig(DateTime? billDate, DateTime? oldDate)
        {
            if (billDate != null || oldDate != null)
            {

                var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
                var sqlParams = new List<SqlParameter>
                {
                    result
                };

                if (oldDate.HasValue)
                {
                    sqlParams.Add(new SqlParameter("@OldDate", SqlDbType.DateTime2) { Value = oldDate });
                }

                if (billDate.HasValue)
                {
                    sqlParams.Add(new SqlParameter("@BillDate", SqlDbType.DateTime2) { Value = billDate });
                }

                await _stockDbContext.ExecuteStoreProcedure("asp_ValidateBillDate", sqlParams, true);

                if (!(result.Value as bool?).GetValueOrDefault())
                    throw new BadRequestException(GeneralCode.InvalidParams, "Ngày chứng từ không được phép trước ngày chốt sổ");
            }
        }
        #endregion
    }
}
