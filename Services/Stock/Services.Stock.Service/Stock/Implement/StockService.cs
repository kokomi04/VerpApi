using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Stock;
using VErp.Infrastructure.EF.MasterDB;


namespace VErp.Services.Stock.Service.Stock.Implement
{
    public class StockService : IStockService
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly StockDBContext _stockContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IUnitService _unitService;
        private readonly IActivityService _activityService;

        public StockService(
            MasterDBContext masterDBContext,
            StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<StockService> logger
            , IUnitService unitService
            , IActivityService activityService
            )
        {
            _masterDBContext = masterDBContext;
            _stockContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _unitService = unitService;
            _activityService = activityService;
        }

        public async Task<ServiceResult<int>> AddStock(StockModel req)
        {
            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var stockInfo = new VErp.Infrastructure.EF.StockDB.Stock()
                    {
                        //StockId = req.StockId,
                        StockName = req.StockName,
                        Description = req.Description,
                        StockKeeperId = req.StockKeeperId,
                        StockKeeperName = req.StockKeeperName,
                        Type = req.Type,
                        Status = req.Status,
                        CreatedDatetimeUtc = DateTime.Now,
                        UpdatedDatetimeUtc = DateTime.Now,
                        IsDeleted = false
                    };

                    await _stockContext.AddAsync(stockInfo);

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();

                    var objLog = GetStockForLog(stockInfo);

                    _activityService.CreateActivityAsync(EnumObjectType.Stock, stockInfo.StockId, $"Thêm mới kho {stockInfo.StockName}", null, objLog);

                    return stockInfo.StockId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "AddStock");
                    return GeneralCode.InternalError;
                }
            }
        }


        public async Task<ServiceResult<StockOutput>> StockInfo(int stockId)
        {
            var stockInfo = await _stockContext.Stock.FirstOrDefaultAsync(p => p.StockId == stockId);
            if (stockInfo == null)
            {
                return StockErrorCode.StockNotFound;
            }
            return new StockOutput()
            {
                StockId = stockInfo.StockId,
                StockName = stockInfo.StockName,
                Description = stockInfo.Description,
                StockKeeperId = stockInfo.StockKeeperId,
                StockKeeperName = stockInfo.StockKeeperName,
                Type = stockInfo.Type,
                Status = stockInfo.Status
            };
        }


        public async Task<Enum> UpdateStock(int stockId, StockModel req)
        {
            req.StockName = (req.StockName ?? "").Trim();

            var checkExistsName = await _stockContext.Stock.AnyAsync(p => p.StockName == req.StockName && p.StockId != stockId);
            if (checkExistsName)
            {
                return StockErrorCode.StockCodeAlreadyExisted;
            }

            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //Getdata
                    var stockInfo = await _stockContext.Stock.FirstOrDefaultAsync(p => p.StockId == stockId);
                    if (stockInfo == null)
                    {
                        return StockErrorCode.StockNotFound;
                    }
                    var originalObj = GetStockForLog(stockInfo);

                    //Update

                    //stockInfo.StockId = req.StockId;
                    stockInfo.StockName = req.StockName;
                    stockInfo.Description = req.Description;
                    stockInfo.StockKeeperId = req.StockKeeperId;
                    stockInfo.StockKeeperName = req.StockKeeperName;
                    stockInfo.Type = req.Type;
                    stockInfo.Status = req.Status;
                    stockInfo.UpdatedDatetimeUtc = DateTime.Now;

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();

                    var objLog = GetStockForLog(stockInfo);

                    _activityService.CreateActivityAsync(EnumObjectType.Stock, stockInfo.StockId, $"Cập nhật thông tin kho hàng {stockInfo.StockName}", originalObj.JsonSerialize(), objLog);

                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "UpdateStock");
                    return GeneralCode.InternalError;
                }
            }
        }


        public async Task<Enum> DeleteStock(int stockId)
        {
            var stockInfo = await _stockContext.Stock.FirstOrDefaultAsync(p => p.StockId == stockId);

            if (stockInfo == null)
            {
                return StockErrorCode.StockNotFound;
            }
            var objLog = GetStockForLog(stockInfo);
            var dataBefore = objLog.JsonSerialize();

            stockInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    stockInfo.IsDeleted = true;
                    stockInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();

                    _activityService.CreateActivityAsync(EnumObjectType.Product, stockInfo.StockId, $"Xóa kho {stockInfo.StockName}", dataBefore, null);

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

        public async Task<PageData<StockOutput>> GetList(string keyword, int page, int size)
        {
            var query = from p in _stockContext.Stock
                        select p;


            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from q in query
                        where q.StockName.Contains(keyword)
                        select q;
            }

            var total = await query.CountAsync();
            var lstData = await query.Skip((page - 1) * size).Take(size).ToListAsync();

            var pagedData = new List<StockOutput>();
            foreach (var item in lstData)
            {
                var stockInfo = new StockOutput()
                {
                    StockId = item.StockId,
                    StockName = item.StockName,
                    Description = item.Description,
                    StockKeeperId = item.StockKeeperId,
                    StockKeeperName = item.StockKeeperName,
                    Type = item.Type,
                    Status = item.Status

                };
                pagedData.Add(stockInfo);
            }
            return (pagedData, total);
        }

        public async Task<PageData<StockOutput>> GetListByUserId(int userId, string keyword, int page, int size)
        {
            var query = from p in _stockContext.Stock
                        where p.StockKeeperId == userId
                        select p;


            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from q in query
                        where q.StockName.Contains(keyword)
                        select q;
            }

            var total = await query.CountAsync();
            var lstData = await query.Skip((page - 1) * size).Take(size).ToListAsync();

            var pagedData = new List<StockOutput>();
            foreach (var item in lstData)
            {
                var stockInfo = new StockOutput()
                {
                    StockId = item.StockId,
                    StockName = item.StockName,
                    Description = item.Description,
                    StockKeeperId = item.StockKeeperId,
                    StockKeeperName = item.StockKeeperName,
                    Type = item.Type,
                    Status = item.Status

                };
                pagedData.Add(stockInfo);
            }
            return (pagedData, total);
        }

        public async Task<IList<SimpleStockInfo>> GetSimpleList()
        {
            return await _stockContext.Stock.Select(s => new SimpleStockInfo()
            {
                StockId = s.StockId,
                StockName = s.StockName
            }).ToListAsync();
        }

        public async Task<IList<StockWarning>> StockWarnings()
        {
            var lstMinMaxWarnings = await (
                 from sp in _stockContext.StockProduct
                 join p in _stockContext.Product on sp.ProductId equals p.ProductId
                 join ps in _stockContext.ProductStockInfo on p.ProductId equals ps.ProductId
                 where sp.PrimaryQuantityRemaining <= ps.AmountWarningMin
                 || sp.PrimaryQuantityRemaining >= ps.AmountWarningMax
                 select new
                 {
                     sp.StockId,
                     p.ProductId,
                     p.ProductCode,
                     p.ProductName,
                     StockWarningTypeId = sp.PrimaryQuantityRemaining <= ps.AmountWarningMin ? EnumWarningType.Min : EnumWarningType.Max,
                 })
                 .ToListAsync();

            var lstExpiredWarnings = await (
                from pk in _stockContext.Package
                join d in _stockContext.InventoryDetail on pk.InventoryDetailId equals d.InventoryDetailId
                join iv in _stockContext.Inventory on d.InventoryId equals iv.InventoryId
                join p in _stockContext.Product on d.ProductId equals p.ProductId
                join ps in _stockContext.ProductStockInfo on p.ProductId equals ps.ProductId
                where pk.ExpiryTime < DateTime.UtcNow
                select new
                {
                    iv.StockId,
                    p.ProductId,
                    p.ProductCode,
                    p.ProductName,
                    pk.PackageCode,
                    StockWarningTypeId = EnumWarningType.Expired
                })
                .ToListAsync();

            var result = new List<StockWarning>();
            var stocks = await _stockContext.Stock.ToListAsync();
            foreach (var s in stocks)
            {
                var warnings = lstMinMaxWarnings
                    .Where(w => w.StockId == s.StockId)
                    .Select(w => new StockWarningDetail
                    {
                        ProductId = w.ProductId,
                        ProductCode = w.ProductCode,
                        ProductName = w.ProductName,
                        StockWarningTypeId = w.StockWarningTypeId,
                        PackageCode = null
                    });

                warnings.Union(
                    lstExpiredWarnings.Where(w => w.StockId == s.StockId)
                    .Select(w => new StockWarningDetail
                    {
                        ProductId = w.ProductId,
                        ProductCode = w.ProductCode,
                        ProductName = w.ProductName,
                        StockWarningTypeId = w.StockWarningTypeId,
                        PackageCode = null
                    })
                    );

                result.Add(new StockWarning()
                {
                    StockId = s.StockId,
                    StockName = s.StockName,
                    Warnings = warnings.OrderBy(p => p.ProductCode).ToList()
                });
            }
            return result;
        }


        public async Task<PageData<StockProductListOutput>> StockProducts(int stockId, string keyword, IList<int> productTypeIds, IList<int> productCateIds, IList<EnumWarningType> stockWarningTypeIds, int page, int size)
        {
            var productQuery = (
                 from p in _stockContext.Product
                 select new
                 {
                     p.ProductId,
                     p.ProductCode,
                     p.ProductName,
                     p.UnitId,
                     p.ProductTypeId,
                     p.ProductCateId
                 }
                );
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                productQuery = from p in productQuery
                               where p.ProductName.Contains(keyword)
                               || p.ProductCode.Contains(keyword)
                               select p;
            }

            if (productTypeIds != null && productTypeIds.Count > 0)
            {
                var types = productTypeIds.Select(t => (int?)t);
                productQuery = from p in productQuery
                               where types.Contains(p.ProductTypeId)
                               select p;
            }

            if (productCateIds != null && productCateIds.Count > 0)
            {
                productQuery = from p in productQuery
                               where productCateIds.Contains(p.ProductCateId)
                               select p;
            }



            var query = from sp in _stockContext.StockProduct
                        join p in productQuery on sp.ProductId equals p.ProductId
                        join ps in _stockContext.ProductStockInfo on p.ProductId equals ps.ProductId
                        where sp.StockId == stockId
                        select new
                        {
                            p.ProductId,
                            p.ProductCode,
                            p.ProductName,
                            p.UnitId,
                            p.ProductTypeId,
                            p.ProductCateId,
                            sp.PrimaryQuantityRemaining,
                            ps.AmountWarningMin,
                            ps.AmountWarningMax,
                        };

            if (stockWarningTypeIds != null && stockWarningTypeIds.Count > 0)
            {
                if (stockWarningTypeIds.Contains(EnumWarningType.Min))
                {
                    query = from p in query
                            where p.PrimaryQuantityRemaining <= p.AmountWarningMin
                            select p;
                }

                if (stockWarningTypeIds.Contains(EnumWarningType.Max))
                {
                    query = from p in query
                            where p.AmountWarningMax >= p.AmountWarningMin
                            select p;
                }


                if (stockWarningTypeIds.Contains(EnumWarningType.Expired))
                {
                    var productWithExprires = from iv in _stockContext.Inventory
                                              join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
                                              join pk in _stockContext.Package on d.InventoryDetailId equals pk.InventoryDetailId
                                              where iv.StockId == stockId && pk.ExpiryTime < DateTime.UtcNow
                                              select new
                                              {
                                                  d.ProductId,
                                              };

                    query = from p in query
                            join e in productWithExprires on p.ProductId equals e.ProductId
                            select p;
                }
            }

            var total = await query.CountAsync();
            var lstData = await query.Skip((page - 1) * size).Take(size).ToListAsync();
            var productIds = lstData.Select(p => p.ProductId).ToList();

            if (productIds.Count == 0)
            {
                return (new List<StockProductListOutput>(), total);
            }
            var extraInfos = await (
                from p in _stockContext.ProductExtraInfo
                where productIds.Contains(p.ProductId)
                select new
                {
                    p.ProductId,
                    p.Specification
                }
                )
                .ToListAsync();

            var pagedData = new List<StockProductListOutput>();

            foreach (var item in lstData)
            {
                var extra = extraInfos.FirstOrDefault(p => p.ProductId == item.ProductId);

                var stockInfo = new StockProductListOutput()
                {
                    ProductId = item.ProductId,
                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    ProductTypeId = item.ProductTypeId,
                    ProductCateId = item.ProductCateId,
                    Specification = extra?.Specification,
                    UnitId = item.UnitId,
                    PrimaryQuantityRemaining = item.PrimaryQuantityRemaining,
                };
                pagedData.Add(stockInfo);
            }
            return (pagedData, total);
        }


        public async Task<PageData<StockProductPackageDetail>> StockProductPackageDetails(int stockId, int productId, int page, int size)
        {
            var productStockInfo = await _stockContext.ProductStockInfo.FirstOrDefaultAsync(p => p.ProductId == productId);
            var query = (
                from pk in _stockContext.Package
                join iv in _stockContext.InventoryDetail on pk.InventoryDetailId equals iv.InventoryDetailId
                join i in _stockContext.Inventory on iv.InventoryId equals i.InventoryId
                select new StockProductPackageDetail()
                {
                    PackageId = pk.PackageId,
                    PackageCode = pk.PackageCode,
                    LocationId = pk.LocationId,
                    Date = i.DateUtc,
                    ExpriredDate = pk.ExpiryTime,
                    PrimaryUnitId = pk.PrimaryUnitId,
                    PrimaryQuantity = pk.PrimaryQuantity,
                    SecondaryUnitId = pk.SecondaryUnitId,
                    SecondaryQuantity = pk.SecondaryQuantity,
                    RefObjectId = iv.RefObjectId,
                    RefObjectCode = iv.RefObjectCode
                }
                );
            var total = await query.CountAsync();
            switch ((EnumStockOutputRule)productStockInfo.StockOutputRuleId)
            {
                case EnumStockOutputRule.None:
                case EnumStockOutputRule.Fifo:
                    query = from pk in query
                            orderby pk.Date
                            select pk;
                    break;
                case EnumStockOutputRule.Lifo:
                    query = from pk in query
                            orderby pk.Date descending
                            select pk;
                    break;
            }

            var lstData = await query.Skip((page - 1) * size).Take(size).ToListAsync();

            return (lstData, total);
        }

        public async Task<PageData<StockSumaryReportOutput>> StockSumaryReport(string keyword, IList<int> stockIds, IList<int> productTypeIds, IList<int> productCateIds, DateTime fromDate, DateTime toDate, int page, int size)
        {
            var productQuery = (
                from p in _stockContext.Product
                select new
                {
                    p.ProductId,
                    p.ProductCode,
                    p.ProductName,
                    p.UnitId,
                    p.ProductTypeId,
                    p.ProductCateId
                }
               );
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                productQuery = from p in productQuery
                               where p.ProductName.Contains(keyword)
                               || p.ProductCode.Contains(keyword)
                               select p;
            }

            if (productTypeIds != null && productTypeIds.Count > 0)
            {
                var types = productTypeIds.Select(t => (int?)t);
                productQuery = from p in productQuery
                               where types.Contains(p.ProductTypeId)
                               select p;
            }

            if (productCateIds != null && productCateIds.Count > 0)
            {
                productQuery = from p in productQuery
                               where productCateIds.Contains(p.ProductCateId)
                               select p;
            }

            productQuery = from p in productQuery
                           join d in _stockContext.InventoryDetail on p.ProductId equals d.ProductId
                           join iv in _stockContext.Inventory on d.InventoryId equals iv.InventoryId
                           where iv.IsApproved
                           group 0 by new { p.ProductId, p.ProductCode, p.ProductName, p.ProductTypeId, p.ProductCateId, d.PrimaryUnitId } into g
                           select new
                           {
                               g.Key.ProductId,
                               g.Key.ProductCode,
                               g.Key.ProductName,
                               UnitId = g.Key.PrimaryUnitId,
                               g.Key.ProductTypeId,
                               g.Key.ProductCateId
                           };

            var total = await productQuery.CountAsync();

            var queryBefore = (
                from iv in _stockContext.Inventory
                join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
                join p in productQuery on d.ProductId equals p.ProductId
                where iv.IsApproved && iv.DateUtc < fromDate
                group new { d.PrimaryQuantity, iv.InventoryTypeId } by new { d.ProductId, d.PrimaryUnitId } into g
                select new
                {
                    g.Key.ProductId,
                    UnitId = g.Key.PrimaryUnitId,
                    Total = g.Sum(d => d.InventoryTypeId == (int)EnumInventory.Input ? d.PrimaryQuantity : -d.PrimaryQuantity)
                }
                );

            var queryAfter = (
                from iv in _stockContext.Inventory
                join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
                join p in productQuery on d.ProductId equals p.ProductId
                where iv.IsApproved && iv.DateUtc >= fromDate && iv.DateUtc <= toDate
                group new { d.PrimaryQuantity, iv.InventoryTypeId } by new { d.ProductId, d.PrimaryUnitId } into g
                select new
                {
                    g.Key.ProductId,
                    UnitId = g.Key.PrimaryUnitId,
                    TotalInput = g.Sum(d => d.InventoryTypeId == (int)EnumInventory.Input ? d.PrimaryQuantity : 0),
                    TotalOutput = g.Sum(d => d.InventoryTypeId == (int)EnumInventory.Output ? d.PrimaryQuantity : 0),
                    Total = g.Sum(d => d.InventoryTypeId == (int)EnumInventory.Input ? d.PrimaryQuantity : -d.PrimaryQuantity)
                }
                );

            //TODO: dotnetcore exception when join on group by
            var products = await productQuery.ToListAsync();
            var befores = await queryBefore.ToListAsync();
            var afters = await queryAfter.ToListAsync();
            var data = (
                from p in products
                join b in befores on new { p.ProductId, p.UnitId } equals new { b.ProductId, b.UnitId } into bp
                from b in bp.DefaultIfEmpty()
                join a in afters on new { p.ProductId, p.UnitId } equals new { a.ProductId, a.UnitId } into ap
                from a in ap.DefaultIfEmpty()
                select new StockSumaryReportOutput
                {
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    UnitId = p.UnitId,
                    PrimaryQualtityBefore = b == null ? 0 : b.Total,
                    PrimaryQualtityInput = a == null ? 0 : a.TotalInput,
                    PrimaryQualtityOutput = a == null ? 0 : a.TotalOutput,
                    PrimaryQualtityAfter = (b == null ? 0 : b.Total) + (a == null ? 0 : a.Total)
                });
            var pageData = data.Skip((page - 1) * size).Take(size).ToList();
            return (pageData, total);
        }

        /// <summary>
        /// Báo cáo chi tiết nhập xuất sp trong kỳ
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="stockIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public async Task<ServiceResult<StockProductDetailsReportOutput>> StockProductDetailsReport(int productId, IList<int> stockIds, DateTime? fromDate, DateTime? toDate)
        {
            var resultData = new StockProductDetailsReportOutput
            {
                OpeningStock = null,
                Details = null
            };
            try
            {
                if (productId <= 0)
                    return GeneralCode.InvalidParams;
                if (!fromDate.HasValue && !toDate.HasValue)
                    return GeneralCode.InvalidParams;

                var beginTime = fromDate.HasValue ? fromDate : _stockContext.Inventory.OrderBy(q => q.DateUtc).Select(q => q.DateUtc).FirstOrDefault().AddDays(-1);

                #region Lấy dữ liệu tồn đầu
                var openingStockQuery = from i in _stockContext.Inventory
                                        join id in _stockContext.InventoryDetail on i.InventoryId equals id.InventoryId
                                        where i.IsApproved && id.ProductId == productId
                                        select new { i, id };
                if (stockIds.Count > 0)
                    openingStockQuery = openingStockQuery.Where(q => stockIds.Contains(q.i.StockId));

                if (beginTime.HasValue)
                    openingStockQuery = openingStockQuery.Where(q => q.i.DateUtc < beginTime);

                var openingStockQueryData = openingStockQuery.GroupBy(q => q.id.PrimaryUnitId).Select(g => new { PrimaryUnitId = g.Key, Total = g.Sum(v => v.i.InventoryTypeId == (int)EnumInventory.Input ? v.id.PrimaryQuantity : (v.i.InventoryTypeId == (int)EnumInventory.Output ? -v.id.PrimaryQuantity : 0)) }).ToList();

                #endregion

                #region Lấy dữ liệu giao dịch trong kỳ
                var inPerdiodQuery = from i in _stockContext.Inventory
                                     join id in _stockContext.InventoryDetail on i.InventoryId equals id.InventoryId
                                     where i.IsApproved && id.ProductId == productId
                                     select new { i, id };
                if (stockIds.Count > 0)
                    inPerdiodQuery = inPerdiodQuery.Where(q => stockIds.Contains(q.i.StockId));

                if (fromDate.HasValue && toDate.HasValue)
                {
                    inPerdiodQuery = inPerdiodQuery.Where(q => q.i.DateUtc >= fromDate && q.i.DateUtc <= toDate);
                }
                else
                {
                    if (fromDate.HasValue)
                    {
                        inPerdiodQuery = inPerdiodQuery.Where(q => q.i.DateUtc >= fromDate);
                    }
                    if (toDate.HasValue)
                    {
                        inPerdiodQuery = inPerdiodQuery.Where(q => q.i.DateUtc <= toDate);
                    }
                }

                var totalRecord = inPerdiodQuery.Count();
                var inPeriodData = inPerdiodQuery.Select(q => new
                {
                    InventoryId = q.i.InventoryId,
                    IssuedDate = q.i.DateUtc,
                    InventoryCode = q.i.InventoryCode,
                    InventoryTypeId = q.i.InventoryTypeId,
                    Description = q.i.Content,
                    InventoryDetailId = q.id.InventoryDetailId,
                    RefObjectCode = q.id.RefObjectCode,
                    PrimaryUnitId = q.id.PrimaryUnitId,
                    PrimaryQuantity = q.id.PrimaryQuantity,
                    SecondaryUnitId = q.id.SecondaryUnitId,
                    SecondaryQuantity = q.id.SecondaryQuantity,
                    ProductUnitConversionId = q.id.ProductUnitConversionId
                }).ToList();

                var productUnitConversionIdsList = inPeriodData.Where(q => q.ProductUnitConversionId > 0).Select(q => q.ProductUnitConversionId).ToList();
                var productUnitConversionData = _stockContext.ProductUnitConversion.Where(q => productUnitConversionIdsList.Contains(q.ProductUnitConversionId)).ToList();
                var unitData = await _masterDBContext.Unit.ToListAsync();
                
                resultData.OpeningStock = new List<OpeningStockProductModel>(openingStockQueryData.Count);
                foreach (var item in openingStockQueryData)
                {
                    resultData.OpeningStock.Add(new OpeningStockProductModel
                    {
                        PrimaryUnitId = item.PrimaryUnitId,
                        Total = item.Total
                    });
                }
                resultData.Details = new List<StockProductDetailsModel>(totalRecord);
                foreach (var item in inPeriodData)
                {
                    resultData.Details.Add(new StockProductDetailsModel
                    {
                        InventoryId = item.InventoryId,
                        IssuedDate = item.IssuedDate,
                        InventoryCode = item.InventoryCode,
                        InventoryTypeId = item.InventoryTypeId,
                        Description = item.Description,
                        SecondaryUnitName = unitData.FirstOrDefault(q => q.UnitId == item.SecondaryUnitId).UnitName ?? string.Empty,
                        InventoryDetailId = item.InventoryDetailId,
                        RefObjectCode = item.RefObjectCode,
                        PrimaryUnitId = item.PrimaryUnitId,
                        PrimaryQuantity = item.PrimaryQuantity,
                        SecondaryUnitId = item.SecondaryUnitId,
                        SecondaryQuantity = item.SecondaryQuantity,
                        ProductUnitConversionId = item.ProductUnitConversionId ?? null,
                        ProductUnitConversion = (item.ProductUnitConversionId > 0 ? productUnitConversionData.FirstOrDefault(q => q.ProductUnitConversionId == item.ProductUnitConversionId) : null)
                    });
                }
                #endregion
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StockProductDetailsReport");
                return GeneralCode.InternalError;
            }
            return resultData;
        }

        #region Private Methods

        private object GetStockForLog(VErp.Infrastructure.EF.StockDB.Stock stockInfo)
        {
            return stockInfo;
        }

        #endregion
    }
}
