using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using Verp.Resources.Stock.Inventory.Abstract;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.GlobalObject.InternalDataInterface.Manufacturing;
using VErp.Commons.GlobalObject.InternalDataInterface.System;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Hr;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Inv;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Product;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.QueueHelper;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.System;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.FileResources;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Inventory.OpeningBalance;
using VErp.Services.Stock.Model.Package;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Products;
using VErp.Services.Stock.Service.Stock.Implement.InventoryFileData;
using static Verp.Resources.Stock.InventoryProcess.InventoryBillInputMessage;
using static Verp.Resources.Stock.InventoryProcess.InventoryBillOutputMessage;
using PackageEntity = VErp.Infrastructure.EF.StockDB.Package;

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
        private readonly IProductService _productService;
        private readonly IInventoryBillOutputService _inventoryBillOutputService;
        private readonly IInventoryBillInputService _inventoryBillInputService;
        private readonly IUserHelperService _userHelperService;
        private readonly IMailFactoryService _mailFactoryService;
        private readonly ILongTaskResourceLockService _longTaskResourceLockService;

        public InventoryService(MasterDBContext masterDBContext, StockDBContext stockContext
            , ILogger<InventoryService> logger
            , IActivityLogService activityLogService
            , IFileService fileService
            , IOrganizationHelperService organizationHelperService
            , IStockHelperService stockHelperService
            , IProductHelperService productHelperService
            , ICurrentContextService currentContextService
            , IProductService productService
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IInventoryBillOutputService inventoryBillOutputService
            , IInventoryBillInputService inventoryBillInputService
            , IProductionOrderQueueHelperService productionOrderQueueHelperService
            , ILongTaskResourceLockService longTaskResourceLockService
            , IUserHelperService userHelperService = null, IMailFactoryService mailFactoryService = null) : base(stockContext, logger, customGenCodeHelperService, currentContextService, productionOrderQueueHelperService)
        {
            _masterDBContext = masterDBContext;
            _activityLogService = activityLogService;
            _fileService = fileService;
            _organizationHelperService = organizationHelperService;
            _stockHelperService = stockHelperService;
            _productHelperService = productHelperService;
            _productService = productService;
            _inventoryBillOutputService = inventoryBillOutputService;
            _inventoryBillInputService = inventoryBillInputService;
            _longTaskResourceLockService = longTaskResourceLockService;
            _userHelperService = userHelperService;
            _mailFactoryService = mailFactoryService;
        }



        public async Task<PageData<InventoryListOutput>> GetList(string keyword, int? customerId, IList<int> productIds, int stockId = 0, int? inventoryStatusId = null, EnumInventoryType? type = null, long? beginTime = 0, long? endTime = 0, bool? isInputBillCreated = null, string sortBy = "date", bool asc = false, int page = 1, int size = 10, int? inventoryActionId = null, Clause filters = null)
        {
            keyword = keyword?.Trim();

            var inventoryQuery = _stockDbContext.Inventory.Include(iv => iv.RefInventory).AsNoTracking().AsQueryable();

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

            if (inventoryActionId.HasValue)
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


            var sourceBillCodes = _stockDbContext.RefInputBillSourceBillCode.GroupBy(c => new { c.SourceBillCode }).Select(c => c.Key); ;


            var query = from q in inventoryQuery
                        join b in sourceBillCodes on q.InventoryCode equals b.SourceBillCode into bs
                        from b in bs.DefaultIfEmpty()
                        select new
                        {
                            q.InventoryId,
                            q.StockId,
                            q.InventoryCode,
                            q.InventoryTypeId,
                            q.Shipper,
                            q.Content,
                            q.Date,

                            q.CustomerId,

                            q.Department,
                            q.StockKeeperUserId,

                            q.BillForm,
                            q.BillCode,
                            q.BillSerial,

                            q.BillDate,

                            q.TotalMoney,

                            q.CreatedByUserId,
                            q.UpdatedByUserId,

                            q.CreatedDatetimeUtc,
                            q.UpdatedDatetimeUtc,
                            q.IsApproved,
                            q.DepartmentId,
                            IsInputBillCreated = b.SourceBillCode != null,
                            b.SourceBillCode,
                            q.CensorByUserId,
                            q.InventoryActionId,
                            q.InventoryStatusId,

                            q.RefInventoryId,
                            RefInventoryCode = q != null ? q.RefInventory.InventoryCode : null,
                            RefStockId = q != null ? (int?)q.RefInventory.StockId : null,
                        };

            if (isInputBillCreated != null)
            {
                if (isInputBillCreated.Value)
                {
                    query = query.Where(q => q.IsInputBillCreated);
                }
                else
                {
                    query = query.Where(q => !q.IsInputBillCreated);
                }
            }

            query = query.InternalFilter(filters);

            var total = await query.CountAsync();

            var inventoryDataList = await query.SortByFieldName(sortBy, asc).AsNoTracking().Skip((page - 1) * size).Take(size).ToListAsync();

            //enrich data
            var stockIds = inventoryDataList.Select(iv => iv.StockId).ToList();

            var stockInfos = (await _stockDbContext.Stock.AsNoTracking().Where(s => stockIds.Contains(s.StockId)).ToListAsync()).ToDictionary(s => s.StockId, s => s);

            var inventoryIds = inventoryDataList.Select(iv => iv.InventoryId.ToString()).ToList();
            var inventoryCodes = inventoryDataList.Select(iv => iv.InventoryCode).ToList();


            var inputObjects = await _stockDbContext.RefInputBillSourceBillCode.Where(m => inventoryCodes.Contains(m.SourceBillCode)).ToListAsync();


            var pagedData = new List<InventoryListOutput>();
            foreach (var item in inventoryDataList)
            {
                stockInfos.TryGetValue(item.StockId, out var stockInfo);

                pagedData.Add(new InventoryListOutput()
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
                    IsInputBillCreated = item.IsInputBillCreated,
                    //AccountancyAccountNumber = item.AccountancyAccountNumber,
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
                    //InventoryDetailOutputList = null,
                    //FileList = null,
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
                        .Where(m => m.SourceBillCode.ToLower() == item.InventoryCode.ToLower())
                        .Select(m => new MappingInputBillModel()
                        {
                            SourceBillCode = m.SourceBillCode,
                            SoCt = m.SoCt,
                            //MappingFunctionKey = null,
                            InputTypeId = m.InputTypeId,
                            //SourceId = item.InventoryId.ToString(),
                            InputBillFId = m.InputBillFId,
                            BillObjectTypeId = EnumObjectType.InputBill,
                            InputType_Title = m.InputTypeTitle

                        }).ToList(),
                    InventoryActionId = (EnumInventoryAction)item.InventoryActionId,
                    InventoryStatusId = item.InventoryStatusId
                });

            }
            return (pagedData, total);
        }

        public async Task<PageData<InventoryListProductOutput>> GetListDetails(string keyword, int? customerId, IList<int> productIds, int stockId = 0, int? inventoryStatusId = null, EnumInventoryType? type = null, long? beginTime = 0, long? endTime = 0, bool? isInputBillCreated = null, string sortBy = "date", bool asc = false, int page = 1, int size = 10, int? inventoryActionId = null, Clause filters = null)
        {
            keyword = keyword?.Trim();

            var inventoryQuery = _stockDbContext.Inventory.Include(iv => iv.RefInventory).AsNoTracking().AsQueryable();

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

            if (inventoryActionId.HasValue)
            {
                inventoryQuery = inventoryQuery.Where(q => q.InventoryActionId == inventoryActionId);
            }

            var inventoryDetails = _stockDbContext.InventoryDetail.AsQueryable();
            if (productIds != null && productIds.Count > 0)
            {
                inventoryDetails = inventoryDetails.Where(d => productIds.Contains(d.ProductId));

            }


            if (inventoryStatusId.HasValue)
            {
                inventoryQuery = inventoryQuery.Where(q => q.InventoryStatusId == inventoryStatusId.Value);
            }

            if (customerId.HasValue)
            {
                inventoryQuery = inventoryQuery.Where(q => q.CustomerId == customerId);
            }

            var sourceBillCodes = _stockDbContext.RefInputBillSourceBillCode.GroupBy(c => new { c.SourceBillCode }).Select(c => c.Key); ;

            var query = from q in inventoryQuery
                        join d in inventoryDetails on q.InventoryId equals d.InventoryId
                        join p in _stockDbContext.Product on d.ProductId equals p.ProductId
                        join c in _stockDbContext.RefCustomerBasic on q.CustomerId equals c.CustomerId into cs
                        from c in cs.DefaultIfEmpty()
                        join pu in _stockDbContext.ProductUnitConversion on d.ProductUnitConversionId equals pu.ProductUnitConversionId into pus
                        from pu in pus.DefaultIfEmpty()
                        join puDefault in _stockDbContext.ProductUnitConversion.Where(u => u.IsDefault) on d.ProductId equals puDefault.ProductId into puDefaults
                        from puDefault in puDefaults.DefaultIfEmpty()
                        join accountantRefBillCode in sourceBillCodes on q.InventoryCode equals accountantRefBillCode.SourceBillCode into accountantRefBillCodes
                        from accountantRefBillCode in accountantRefBillCodes.DefaultIfEmpty()
                        select new
                        {

                            q.InventoryId,
                            q.InventoryCode,
                            q.Date,
                            q.InventoryTypeId,
                            q.InventoryStatusId,
                            q.InventoryActionId,
                            q.Content,
                            q.BillCode,
                            q.BillDate,
                            q.BillForm,
                            q.BillSerial,
                            q.StockId,
                            q.CustomerId,
                            c.CustomerCode,
                            c.CustomerName,
                            q.DepartmentId,
                            q.Shipper,
                            q.StockKeeperUserId,
                            q.Department,

                            q.RefInventoryId,
                            RefInventoryCode = q != null ? q.RefInventory.InventoryCode : null,
                            RefStockId = q != null ? (int?)q.RefInventory.StockId : null,
                            q.TotalMoney,
                            q.IsApproved,
                            q.CreatedByUserId,
                            q.UpdatedByUserId,
                            q.CensorByUserId,
                            q.UpdatedDatetimeUtc,
                            q.CreatedDatetimeUtc,
                            q.CensorDatetimeUtc,

                            d.InventoryDetailId,
                            d.ProductId,
                            p.ProductCode,
                            p.ProductName,
                            p.ProductNameEng,
                            p.UnitId,
                            UnitName = puDefault != null ? puDefault.ProductUnitConversionName : null,
                            d.OrderCode,
                            d.ProductionOrderCode,
                            d.Pocode,
                            d.Description,
                            d.PrimaryQuantity,
                            d.ProductUnitConversionId,
                            d.ProductUnitConversionQuantity,
                            pu.ProductUnitConversionName,


                            IsInputBillCreated = accountantRefBillCode.SourceBillCode != null
                        };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from q in query
                        where q.InventoryCode.Contains(keyword)
                           || q.Shipper.Contains(keyword)
                           || q.Content.Contains(keyword)
                           //|| q.Department.Contains(keyword)
                           || q.BillForm.Contains(keyword)
                           || q.BillCode.Contains(keyword)
                           || q.BillSerial.Contains(keyword)
                           || q.CustomerCode.Contains(keyword)
                           || q.CustomerName.Contains(keyword)


                           || q.ProductCode.Contains(keyword)
                              || q.ProductName.Contains(keyword)
                              || q.ProductNameEng.Contains(keyword)
                              || q.OrderCode.Contains(keyword)
                              || q.ProductionOrderCode.Contains(keyword)
                              || q.Pocode.Contains(keyword)
                              || q.Description.Contains(keyword)
                        // || q.RefObjectCode.Contains(keyword)

                        select q;
            }





            if (isInputBillCreated != null)
            {
                if (isInputBillCreated.Value)
                {
                    query = query.Where(q => q.IsInputBillCreated);
                }
                else
                {
                    query = query.Where(q => !q.IsInputBillCreated);
                }
            }

            query = query.InternalFilter(filters);

            var total = await query.CountAsync();

            var inventoryDataList = await query.SortByFieldName(sortBy, asc).AsNoTracking().Skip((page - 1) * size).Take(size).ToListAsync();

            //enrich data
            var stockIds = inventoryDataList.Select(iv => iv.StockId).ToList();

            var stockInfos = (await _stockDbContext.Stock.AsNoTracking().Where(s => stockIds.Contains(s.StockId)).ToListAsync()).ToDictionary(s => s.StockId, s => s);

            var inventoryIds = inventoryDataList.Select(iv => iv.InventoryId.ToString()).ToList();
            var inventoryCodes = inventoryDataList.Select(iv => iv.InventoryCode).ToList();


            var inputObjects = await _stockDbContext.RefInputBillSourceBillCode.Where(m => inventoryCodes.Contains(m.SourceBillCode)).ToListAsync();

            var pagedData = new List<InventoryListProductOutput>();
            foreach (var item in inventoryDataList)
            {
                stockInfos.TryGetValue(item.StockId, out var stockInfo);

                pagedData.Add(new InventoryListProductOutput()
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
                    IsInputBillCreated = item.IsInputBillCreated,
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
                    InputBills = inputObjects
                        .Where(m => m.SourceBillCode.ToLower() == item.InventoryCode.ToLower())
                        .Select(m => new MappingInputBillModel()
                        {
                            SourceBillCode = m.SourceBillCode,
                            SoCt = m.SoCt,
                            //MappingFunctionKey = null,
                            InputTypeId = m.InputTypeId,
                            //SourceId = item.InventoryId.ToString(),
                            InputBillFId = m.InputBillFId,
                            BillObjectTypeId = EnumObjectType.InputBill,
                            InputType_Title = m.InputTypeTitle

                        }).ToList(),
                    InventoryActionId = (EnumInventoryAction)item.InventoryActionId,
                    InventoryStatusId = item.InventoryStatusId,


                    InventoryDetailId = item.InventoryDetailId,
                    ProductId = item.ProductId,
                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    UnitId = item.UnitId,
                    UnitName = item.UnitName,
                    ProductUnitConversionId = item.ProductUnitConversionId,
                    ProductUnitConversionName = item.ProductUnitConversionName,
                    PrimaryQuantity = item.PrimaryQuantity,
                    ProductUnitConversionQuantity = item.ProductUnitConversionQuantity,
                    PoCode = item.Pocode,
                    OrderCode = item.OrderCode,
                    ProductionOrderCode = item.ProductionOrderCode,
                });

            }
            return (pagedData, total);
        }

        public async Task<InventoryOutput> InventoryInfo(long inventoryId)
        {
            var invs = await GetInfosByIds(new[] { inventoryId }, null);
            var inventoryObj = invs.FirstOrDefault(q => q.InventoryId == inventoryId);
            if (inventoryObj == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            return inventoryObj;
        }


        public async Task<IList<InventoryOutput>> GetInfosByIds(IList<long> inventoryIds, EnumInventoryType? inventoryTypeId)
        {

            var inventoryObjs = await _stockDbContext.Inventory.Include(q => q.RefInventory).AsNoTracking()
                .Where(q => inventoryIds.Contains(q.InventoryId)).ToListAsync();
            if (inventoryTypeId.HasValue)
            {
                inventoryObjs = inventoryObjs.Where(q => q.InventoryTypeId == (int)inventoryTypeId.Value).ToList();
            }


            #region Get inventory details
            var inventoryDetails = await _stockDbContext.InventoryDetail
                .Where(q => inventoryIds.Contains(q.InventoryId))
                .AsNoTracking()
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.CreatedDatetimeUtc)
                .ToListAsync();

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
                    Money = detail.Money,
                    FromPackageId = detail.FromPackageId,
                    ToPackageId = detail.ToPackageId,
                    ToPackageCode = packageInfo?.PackageCode,
                    ToPackageInfo = detail.ToPackageInfo?.JsonDeserialize<PackageInputModel>(),
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
                    InventoryRequirementDetailId = detail.InventoryRequirementDetailId,
                    IsSubCalculation = detail.IsSubCalculation
                };

                //if (!string.IsNullOrEmpty(detail.InventoryRequirementCode) && inventoryRequirementMap.ContainsKey(detail.InventoryRequirementDetailId.Value))
                //{
                //    detail.InventoryRequirementInfo = inventoryRequirementMap[detail.InventoryRequirementDetailId.Value];
                //}

                var subs = await _stockDbContext.InventoryDetailSubCalculation.Where(x => x.InventoryDetailId == detailModel.InventoryDetailId).
                Select(x => new InventoryDetailSubCalculationModel
                {
                    InventoryDetailId = x.InventoryDetailId,
                    InventoryDetailSubCalculationId = x.InventoryDetailSubCalculationId,
                    ProductBomId = x.ProductBomId,
                    UnitConversionId = x.UnitConversionId,
                    PrimaryUnitPrice = x.PrimaryUnitPrice,
                    PrimaryQuantity = x.PrimaryQuantity
                }).ToListAsync();

                detailModel.InventoryDetailSubCalculations = subs;

                listInventoryDetailsOutput.Add(detailModel);
            }
            #endregion

            #region Get Attached files 

            var files = await _stockDbContext.InventoryFile.Where(q => inventoryIds.Contains(q.InventoryId)).ToListAsync();
            var fileIds = files.Select(q => q.FileId).ToList();

            var attachedFiles = await _fileService.GetListFileUrl(fileIds, EnumThumbnailSize.Large);
            if (attachedFiles == null)
            {
                attachedFiles = new List<FileToDownloadInfo>();
            }
            #endregion

            var stockInfos = await _stockDbContext.Stock.AsNoTracking().Where(q => inventoryObjs.Select(v => v.StockId).Contains(q.StockId)).ToListAsync();

            var invCodes = inventoryObjs.Select(v => v.InventoryCode).ToList();

            var mappingObjects = _stockDbContext.RefInputBillSourceBillCode.Where(b => invCodes.Contains(b.SourceBillCode))
                 .Select(m => new MappingInputBillModel()
                 {
                     SourceBillCode = m.SourceBillCode,
                     SoCt = m.SoCt,
                     //MappingFunctionKey = m.MappingFunctionKey,
                     InputTypeId = m.InputTypeId,
                     //SourceId = inventoryObj.InventoryId.ToString(),
                     InputBillFId = m.InputBillFId,
                     InputType_Title = m.InputTypeTitle
                 }).ToList();


            var lst = new List<InventoryOutput>();

            foreach (var inventoryObj in inventoryObjs)
            {
                var invFileIds = files.Where(f => f.InventoryId == inventoryObj.InventoryId).Select(q => q.FileId).ToList();
                var stockInfo = stockInfos.FirstOrDefault(s => s.StockId == inventoryObj.StockId);

                var invMappingObjs = mappingObjects.Where(m => m.SourceBillCode?.ToLower() == inventoryObj.InventoryCode?.ToLower()).ToList();

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
                    //AccountancyAccountNumber = inventoryObj.AccountancyAccountNumber,
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
                    InventoryDetailOutputList = listInventoryDetailsOutput.Where(d => d.InventoryId == inventoryObj.InventoryId).ToList(),
                    FileList = attachedFiles.Where(f => invFileIds.Contains(f.FileId ?? 0)).ToList(),
                    IsInputBillCreated = invMappingObjs.Count() > 0,
                    InputBills = invMappingObjs,
                    InventoryStatusId = inventoryObj.InventoryStatusId,
                    InventoryActionId = (EnumInventoryAction)inventoryObj.InventoryActionId,
                    InputTypeSelectedState = inventoryObj.InputTypeSelectedState.HasValue ? (EnumInputType)inventoryObj.InputTypeSelectedState : EnumInputType.Default,
                    InputUnitTypeSelectedState = inventoryObj.InputUnitTypeSelectedState.HasValue ? (EnumInputUnitType)inventoryObj.InputUnitTypeSelectedState : null,
                    UpdatedDatetimeUtc = inventoryObj.UpdatedDatetimeUtc.GetUnix(),
                };

                if (inventoryObj.RefInventory != null)
                {
                    inventoryOutput.RefInventoryId = inventoryObj.RefInventory?.InventoryId;
                    inventoryOutput.RefInventoryCode = inventoryObj.RefInventory?.InventoryCode;
                    inventoryOutput.RefStockId = inventoryObj.RefInventory?.StockId;
                }

                lst.Add(inventoryOutput);
            }

            return lst;

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



        public CategoryNameModel OutFieldsForParse()
        {
            var result = new CategoryNameModel()
            {
                //CategoryId = 1,
                CategoryCode = "Output",
                CategoryTitle = InventoryAbstractMessage.InventoryOuput,
                IsTreeView = false,
                Fields = ExcelUtils.GetFieldNameModels<InventoryOutExcelParseModel>()
            };
            return result;
        }



        public async Task<CategoryNameModel> InputFieldsForParse()
        {
            var result = new CategoryNameModel()
            {
                //CategoryId = 1,
                CategoryCode = "Inventory",
                CategoryTitle = InventoryAbstractMessage.InventoryInput,
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };
            var fields = ExcelUtils.GetFieldNameModels<InventoryInputExcelParseModel>();


            var packageField = fields.First(f => f.FieldName.StartsWith(nameof(InventoryInputExcelParseModel.ToPackgeInfo)));

            var customProps = await _stockDbContext.PackageCustomProperty.ToListAsync();

            foreach (var p in customProps)
            {
                var f = new CategoryFieldNameModel()
                {
                    GroupName = packageField.GroupName,
                    //CategoryFieldId = prop.Name.GetHashCode(),
                    FieldName = nameof(ImportInvInputModel.ToPackgeInfo) + nameof(PackageInputModel.CustomPropertyValue) + p.PackageCustomPropertyId,
                    FieldTitle = "(Kiện) - " + p.Title,
                    IsRequired = false,
                    Type = null,
                    RefCategory = null
                };

                fields.Add(f);
            }

            result.Fields = fields;
            return result;
        }



        public IAsyncEnumerable<InvInputDetailRowValue> InputParseExcel(ImportExcelMapping mapping, Stream stream, int stockId)
        {
            var parse = new InvInputDetailParseFacade();

            parse.SetProductService(_productService)
                .SetStockDbContext(_stockDbContext);

            return parse.ParseExcel(mapping, stream, stockId);
        }

        public IAsyncEnumerable<InvOutDetailRowValue> OutParseExcel(ImportExcelMapping mapping, Stream stream, int stockId)
        {
            var parse = new InvOutDetailParseFacade();

            parse.SetProductService(_productService)
                .SetStockDbContext(_stockDbContext);

            return parse.ParseExcel(mapping, stream, stockId);
        }


        public async Task<CategoryNameModel> InputFieldsForMapping()
        {
            var result = new CategoryNameModel()
            {
                //CategoryId = 1,
                CategoryCode = "Inventory",
                CategoryTitle = "Inventory",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };
            var fields = ExcelUtils.GetFieldNameModels<ImportInvInputModel>();


            var packageField = fields.First(f => f.FieldName.StartsWith(nameof(ImportInvInputModel.ToPackgeInfo)));

            var customProps = await _stockDbContext.PackageCustomProperty.ToListAsync();

            var sortOrder = fields.Max(f => f.SortOrder);
            foreach (var p in customProps)
            {
                var f = new CategoryFieldNameModel()
                {
                    GroupName = packageField.GroupName,
                    //CategoryFieldId = prop.Name.GetHashCode(),
                    FieldName = nameof(ImportInvInputModel.ToPackgeInfo) + nameof(PackageInputModel.CustomPropertyValue) + p.PackageCustomPropertyId,
                    FieldTitle = "(Kiện) - " + p.Title,
                    IsRequired = false,
                    Type = null,
                    RefCategory = null,
                    SortOrder = sortOrder++
                };

                fields.Add(f);
            }

            var actionField = fields.First(f => f.FieldName.StartsWith(nameof(ImportInvInputModel.InventoryActionId)));

            var actions = ImportInvInputModel.InventoryActionIds;

            actionField.FieldTitle = $"Loại ({string.Join(",", actions.Select(a => $"{(int)a}: {a.GetEnumDescription()}"))})";

            result.Fields = fields;
            return result;
        }



        public CategoryNameModel OutputFieldsForMapping()
        {
            var result = new CategoryNameModel()
            {
                //CategoryId = 1,
                CategoryCode = "InventoryOutput",
                CategoryTitle = "InventoryOutput",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };
            var fields = ExcelUtils.GetFieldNameModels<ImportInvOutputModel>();

            var actionField = fields.First(f => f.FieldName.StartsWith(nameof(ImportInvOutputModel.InventoryActionId)));

            var actions = ImportInvOutputModel.InventoryActionIds;

            actionField.FieldTitle = $"Loại ({string.Join(",", actions.Select(a => $"{(int)a}: {a.GetEnumDescription()}"))})";

            result.Fields = fields;
            return result;
        }

        public async Task<bool> InventoryInputImport(ImportExcelMapping mapping, Stream stream)
        {
            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey()))
            {
                using (var longTask = await _longTaskResourceLockService.Accquire($"Nhập dữ liệu vật tư tiêu hao từ excel"))
                {
                    using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                    {
                        var baseValueChains = new Dictionary<string, int>();

                        using (var batchLog = _activityLogService.BeginBatchLog())
                        {

                            var inventoryInImport = new InventoryInputImportFacade();
                            inventoryInImport.SetProductService(_productService);
                            inventoryInImport.SetMasterDBContext(_masterDBContext);
                            inventoryInImport.SetStockDBContext(_stockDbContext, _currentContextService);
                            inventoryInImport.SetOrganizationHelper(_organizationHelperService);
                            await inventoryInImport.ProcessExcelFile(longTask, mapping, stream);

                            var inventoryDatas = await inventoryInImport.GetInputInventoryModel();

                            longTask.SetCurrentStep("Thêm dữ liệu thẻ kho vào cơ sở dữ liệu", inventoryDatas.Count);

                            foreach (var inventoryData in inventoryDatas)
                            {
                                if (inventoryData?.InProducts == null || inventoryData?.InProducts?.Count == 0)
                                {
                                    throw new BadRequestException("No products found!");
                                }

                                if (inventoryData.InventoryActionId == EnumInventoryAction.Rotation)
                                {
                                    throw GeneralCode.InvalidParams.BadRequestFormat(CannotUpdateInvInputRotation);
                                }

                                var ctx = await GenerateInventoryCode(EnumInventoryType.Input, inventoryData, baseValueChains);

                                var entity = await _inventoryBillInputService.AddInventoryInputDB(inventoryData, true);


                                await ctx.ConfirmCode();

                                await _inventoryBillInputService.ImportedLogBuilder()
                                   .MessageResourceFormatDatas(entity.InventoryCode)
                                   .ObjectId(entity.InventoryId)
                                   .JsonData(inventoryData)
                                   .CreateLog();

                                longTask.IncProcessedRows();
                            }

                            await trans.CommitAsync();

                            await batchLog.CommitAsync();
                        }
                        return true;
                    }
                }
            }
        }


        public async Task<bool> InventoryOutImport(ImportExcelMapping mapping, Stream stream)
        {
            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockStockResourceKey()))
            {

                var baseValueChains = new Dictionary<string, int>();

                using (var trans = await _stockDbContext.Database.BeginTransactionAsync())
                {
                    using (var batchLog = _activityLogService.BeginBatchLog())
                    {
                        var inventoryExport = new InventoryOutImportFacade();
                        inventoryExport.SetStockDBContext(_stockDbContext, _currentContextService);
                        inventoryExport.SetOrganizationHelper(_organizationHelperService);
                        await inventoryExport.ProcessExcelFile(mapping, stream);


                        var inventoryDatas = await inventoryExport.GetOutputInventoryModel();
                        foreach (var inventoryData in inventoryDatas)
                        {
                            if(inventoryData.CustomerId == null && inventoryData.DepartmentId == null)
                            {
                                throw GeneralCode.InvalidParams.BadRequestFormat(RequireCustomerIdOrDepartmentId);
                            }
                            if (inventoryData.CustomerId != null && inventoryData.DepartmentId != null)
                            {
                                throw GeneralCode.InvalidParams.BadRequestFormat(RequireOnlyCustomerIdOrDepartmentId);
                            }
                            if (inventoryData?.OutProducts == null || inventoryData?.OutProducts?.Count == 0)
                            {
                                throw new BadRequestException("No products found!");
                            }
                            
                            if (inventoryData.InventoryActionId == EnumInventoryAction.Rotation)
                            {
                                throw GeneralCode.InvalidParams.BadRequestFormat(CannotUpdateInvOutputRotation);
                            }

                            var ctx = await GenerateInventoryCode(EnumInventoryType.Output, inventoryData, baseValueChains);

                            var entity = await _inventoryBillOutputService.AddInventoryOutputDb(inventoryData);


                            await ctx.ConfirmCode();

                            await _inventoryBillOutputService.ImportedLogBuilder()
                                .MessageResourceFormatDatas(inventoryData.InventoryCode)
                                .ObjectId(entity.InventoryId)
                                .JsonData(inventoryData)
                                .CreateLog();

                        }

                        await trans.CommitAsync();

                        await batchLog.CommitAsync();
                        return true;
                    }
                }
            }
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


    }
}
