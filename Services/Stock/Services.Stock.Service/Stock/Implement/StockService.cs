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
using VErp.Services.Stock.Model.Stock;
using VErp.Infrastructure.EF.MasterDB;
using System.Globalization;

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
            if (_stockContext.Stock.Any(q => q.StockName.ToLower() == req.StockName.ToLower()))
                return StockErrorCode.StockNameAlreadyExisted;
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
                    //join d in _stockContext.InventoryDetail on pk.InventoryDetailId equals d.InventoryDetailId
                    //join iv in _stockContext.Inventory on d.InventoryId equals iv.InventoryId
                join p in _stockContext.Product on pk.ProductId equals p.ProductId
                join ps in _stockContext.ProductStockInfo on p.ProductId equals ps.ProductId
                where pk.ExpiryTime < DateTime.UtcNow
                select new
                {
                    pk.StockId,
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
                            sp.PrimaryUnitId,
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
                    var productWithExprires = from pk in _stockContext.Package
                                                  //from iv in _stockContext.Inventory
                                                  //join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
                                                  //join pk in _stockContext.Package on d.InventoryDetailId equals pk.InventoryDetailId
                                              where pk.StockId == stockId && pk.ExpiryTime < DateTime.UtcNow
                                              select new
                                              {
                                                  pk.ProductId,
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
                    UnitId = item.PrimaryUnitId,
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
                join c in _stockContext.ProductUnitConversion on pk.ProductUnitConversionId equals c.ProductUnitConversionId into cs
                from c in cs.DefaultIfEmpty()
                join l in _stockContext.Location on pk.LocationId equals l.LocationId into ls
                from l in ls.DefaultIfEmpty()
                where pk.StockId == stockId && pk.ProductId == productId
                select new StockProductPackageDetail()
                {
                    PackageId = pk.PackageId,
                    PackageCode = pk.PackageCode,
                    LocationId = pk.LocationId,
                    LocationName = l == null ? null : l.Name,
                    Date = pk.Date,
                    ExpriredDate = pk.ExpiryTime,
                    PrimaryUnitId = pk.PrimaryUnitId,
                    PrimaryQuantity = pk.PrimaryQuantityRemaining,
                    SecondaryUnitId = c.SecondaryUnitId,
                    ProductUnitConversionId = pk.ProductUnitConversionId,
                    ProductUnitConversionName = c == null ? null : c.ProductUnitConversionName,
                    ProductUnitConversionQualtity = pk.ProductUnitConversionRemaining,
                    RefObjectId = null,
                    RefObjectCode = ""
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

        public async Task<PageData<LocationProductPackageOuput>> LocationProductPackageDetails(int stockId, int? locationId, IList<int> productTypeIds, IList<int> productCateIds, int page, int size)
        {
            var products = _stockContext.Product.AsQueryable();
            if (productTypeIds != null && productTypeIds.Count > 0)
            {
                var typeIds = productTypeIds.Cast<int?>();
                products = from p in products
                           where typeIds.Contains(p.ProductTypeId)
                           select p;
            }

            if (productCateIds != null && productCateIds.Count > 0)
            {
                products = from p in products
                           where productCateIds.Contains(p.ProductCateId)
                           select p;
            }
            var query = (
                from pk in _stockContext.Package
                join p in products on pk.ProductId equals p.ProductId
                join ps in _stockContext.ProductStockInfo on p.ProductId equals ps.ProductId
                join c in _stockContext.ProductUnitConversion on pk.ProductUnitConversionId equals c.ProductUnitConversionId into cs
                from c in cs.DefaultIfEmpty()
                where pk.StockId == stockId && pk.LocationId == locationId
                orderby pk.Date descending
                select new LocationProductPackageOuput()
                {
                    ProductId = p.ProductId,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    PackageId = pk.PackageId,
                    PackageCode = pk.PackageCode,
                    Date = pk.Date,
                    ExpriredDate = pk.ExpiryTime,
                    PrimaryUnitId = pk.PrimaryUnitId,
                    PrimaryQuantity = pk.PrimaryQuantityRemaining,
                    SecondaryUnitId = c.SecondaryUnitId,
                    ProductUnitConversionId = pk.ProductUnitConversionId,
                    ProductUnitConversionName = c == null ? null : c.ProductUnitConversionName,
                    ProductUnitConversionQualtity = pk.ProductUnitConversionRemaining,
                    RefObjectId = null,
                    RefObjectCode = ""
                }
                );
            var total = await query.CountAsync();

            var lstData = await query.Skip((page - 1) * size).Take(size).ToListAsync();
            return (lstData, total);
        }

        public async Task<PageData<StockSumaryReportOutput>> StockSumaryReport(string keyword, IList<int> stockIds, IList<int> productTypeIds, IList<int> productCateIds, DateTime fromDate, DateTime toDate, int page, int size)
        {
            toDate = toDate.AddDays(1).Date;

            var productQuery = _stockContext.Product.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                productQuery = from p in productQuery
                               where p.ProductName.Contains(keyword)
                               || p.ProductCode.Contains(keyword)
                               select p;
            }


            if (productTypeIds != null && productTypeIds.Count > 0)
            {
                var productTypes = await _stockContext.ProductType.ToListAsync();

                var types = new List<int?>();
                foreach (var productTypeId in productTypeIds)
                {
                    var st = new Stack<int>();
                    st.Push(productTypeId);
                    while (st.Count > 0)
                    {
                        var parentId = st.Pop();
                        types.Add(parentId);
                        var children = productTypes.Where(p => p.ParentProductTypeId == parentId);
                        foreach (var t in children)
                        {
                            st.Push(t.ProductTypeId);
                        }
                    }

                }


                productQuery = from p in productQuery
                               where types.Contains(p.ProductTypeId)
                               select p;
            }


            if (productCateIds != null && productCateIds.Count > 0)
            {
                var productCates = await _stockContext.ProductCate.ToListAsync();

                var cates = new List<int>();
                foreach (var productCateId in productCateIds)
                {
                    var st = new Stack<int>();
                    st.Push(productCateId);
                    while (st.Count > 0)
                    {
                        var parentId = st.Pop();
                        cates.Add(parentId);
                        var children = productCates.Where(p => p.ParentProductCateId == parentId);
                        foreach (var t in children)
                        {
                            st.Push(t.ProductCateId);
                        }
                    }

                }

                productQuery = from p in productQuery
                               where cates.Contains(p.ProductCateId)
                               select p;
            }
            var inventories = _stockContext.Inventory.AsQueryable();

            if (stockIds != null && stockIds.Count > 0)
            {
                inventories = inventories.Where(iv => stockIds.Contains(iv.StockId));
            }

            var befores = await (
               from iv in inventories
               join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
               join p in productQuery on d.ProductId equals p.ProductId
               where iv.IsApproved && iv.DateUtc < fromDate
               group new { d.PrimaryQuantity, iv.InventoryTypeId } by new { d.ProductId, d.PrimaryUnitId } into g
               select new
               {
                   g.Key.ProductId,
                   g.Key.PrimaryUnitId,
                   Total = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity)
               }
               ).ToListAsync();


            var afters = await (
                from iv in inventories
                join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
                join p in productQuery on d.ProductId equals p.ProductId
                where iv.IsApproved && iv.DateUtc >= fromDate && iv.DateUtc < toDate
                group new { d.PrimaryQuantity, iv.InventoryTypeId } by new { d.ProductId, d.PrimaryUnitId } into g
                select new
                {
                    g.Key.ProductId,
                    g.Key.PrimaryUnitId,
                    TotalInput = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : 0),
                    TotalOutput = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Output ? d.PrimaryQuantity : 0),
                    Total = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity)
                }
                ).ToListAsync();

            var productList = new List<ProductUnitModel>();
            foreach (var b in befores)
            {
                if (!productList.Any(p => p.ProductId == b.ProductId && p.PrimaryUnitId == b.PrimaryUnitId))
                {
                    productList.Add(new ProductUnitModel()
                    {
                        ProductId = b.ProductId,
                        PrimaryUnitId = b.PrimaryUnitId
                    });
                }
            }

            foreach (var a in afters)
            {
                if (!productList.Any(p => p.ProductId == a.ProductId && p.PrimaryUnitId == a.PrimaryUnitId))
                {
                    productList.Add(new ProductUnitModel()
                    {
                        ProductId = a.ProductId,
                        PrimaryUnitId = a.PrimaryUnitId
                    });
                }
            }

            var total = productList.Count;
            var productPaged = productList.Skip((page - 1) * size).Take(size);
            var productIds = productPaged.Select(p => p.ProductId);

            var productInfos = await _stockContext.Product.Where(p => productIds.Contains(p.ProductId)).ToListAsync();

            var data = (
                from p in productPaged
                join info in productInfos on p.ProductId equals info.ProductId
                join b in befores on new { p.ProductId, p.PrimaryUnitId } equals new { b.ProductId, b.PrimaryUnitId } into bp
                from b in bp.DefaultIfEmpty()
                join a in afters on new { p.ProductId, p.PrimaryUnitId } equals new { a.ProductId, a.PrimaryUnitId } into ap
                from a in ap.DefaultIfEmpty()
                select new StockSumaryReportOutput
                {
                    ProductCode = info.ProductCode,
                    ProductName = info.ProductName,
                    UnitId = p.PrimaryUnitId,
                    PrimaryQualtityBefore = b == null ? 0 : b.Total,
                    PrimaryQualtityInput = a == null ? 0 : a.TotalInput,
                    PrimaryQualtityOutput = a == null ? 0 : a.TotalOutput,
                    PrimaryQualtityAfter = (b == null ? 0 : b.Total) + (a == null ? 0 : a.Total)
                }).ToList();

            return (data, total);
        }

        /// <summary>
        /// Báo cáo chi tiết nhập xuất sp trong kỳ
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="stockIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public async Task<ServiceResult<StockProductDetailsReportOutput>> StockProductDetailsReport(int productId, IList<int> stockIds, string fromDateString, string toDateString)
        {
            if (productId <= 0)
                return GeneralCode.InvalidParams;
            if (string.IsNullOrEmpty(fromDateString) && string.IsNullOrEmpty(toDateString))
                return GeneralCode.InvalidParams;

            var resultData = new StockProductDetailsReportOutput
            {
                OpeningStock = null,
                Details = null
            };
            DateTime fromDate = DateTime.MinValue;
            DateTime toDate = DateTime.MinValue;
            if (!string.IsNullOrEmpty(fromDateString))
            {
                if (!DateTime.TryParseExact(fromDateString, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDate))
                {
                    return GeneralCode.InvalidParams;
                }
            }
            if (!string.IsNullOrEmpty(toDateString))
            {
                if (!DateTime.TryParseExact(toDateString, new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out toDate))
                {
                    return GeneralCode.InvalidParams;
                }
            }

            try
            {
                DateTime? beginTime = fromDate != DateTime.MinValue ? fromDate : _stockContext.Inventory.OrderBy(q => q.DateUtc).Select(q => q.DateUtc).FirstOrDefault().AddDays(-1);

                #region Lấy dữ liệu tồn đầu
                var openingStockQuery = from i in _stockContext.Inventory
                                        join id in _stockContext.InventoryDetail on i.InventoryId equals id.InventoryId
                                        where i.IsApproved && id.ProductId == productId
                                        select new { i, id };
                if (stockIds.Count > 0)
                    openingStockQuery = openingStockQuery.Where(q => stockIds.Contains(q.i.StockId));

                if (beginTime.HasValue && beginTime != DateTime.MinValue)
                    openingStockQuery = openingStockQuery.Where(q => q.i.DateUtc < beginTime);

#if DEBUG
                var openingStockQueryDataInput = (from q in openingStockQuery
                                                  where q.i.InventoryTypeId == (int)EnumInventoryType.Input
                                                  group q by q.id.PrimaryUnitId into g
                                                  select new { PrimaryUnitId = g.Key, Total = g.Sum(v => v.id.PrimaryQuantity) }).ToList();

                var openingStockQueryDataOutput = (from q in openingStockQuery
                                                   where q.i.InventoryTypeId == (int)EnumInventoryType.Output
                                                   group q by q.id.PrimaryUnitId into g
                                                   select new { PrimaryUnitId = g.Key, Total = g.Sum(v => v.id.PrimaryQuantity) }).ToList();
#endif

                var openingStockQueryData = openingStockQuery.GroupBy(q => q.id.PrimaryUnitId).Select(g => new { PrimaryUnitId = g.Key, Total = g.Sum(v => v.i.InventoryTypeId == (int)EnumInventoryType.Input ? v.id.PrimaryQuantity : (v.i.InventoryTypeId == (int)EnumInventoryType.Output ? -v.id.PrimaryQuantity : 0)) }).ToList();
                #endregion

                #region Lấy dữ liệu giao dịch trong kỳ
                var inPerdiodQuery = from i in _stockContext.Inventory
                                     join id in _stockContext.InventoryDetail on i.InventoryId equals id.InventoryId
                                     join conversion in _stockContext.ProductUnitConversion on id.ProductUnitConversionId equals conversion.ProductUnitConversionId
                                     where i.IsApproved && id.ProductId == productId
                                     select new { i, id, conversion };
                if (stockIds.Count > 0)
                    inPerdiodQuery = inPerdiodQuery.Where(q => stockIds.Contains(q.i.StockId));

                if (fromDate != DateTime.MinValue && toDate != DateTime.MinValue)
                {
                    inPerdiodQuery = inPerdiodQuery.Where(q => q.i.DateUtc >= fromDate && q.i.DateUtc <= toDate);
                }
                else
                {
                    if (fromDate != DateTime.MinValue)
                    {
                        inPerdiodQuery = inPerdiodQuery.Where(q => q.i.DateUtc >= fromDate);
                    }
                    if (toDate != DateTime.MinValue)
                    {
                        inPerdiodQuery = inPerdiodQuery.Where(q => q.i.DateUtc <= toDate);
                    }
                }

                var totalRecord = inPerdiodQuery.Count();
                var inPeriodData = inPerdiodQuery.AsNoTracking().Select(q => new
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
                    SecondaryUnitId = q.conversion.SecondaryUnitId,
                    SecondaryQuantity = q.id.ProductUnitConversionQuantity,
                    ProductUnitConversionId = q.id.ProductUnitConversionId
                }).ToList();

                var productUnitConversionIdsList = inPeriodData.Where(q => q.ProductUnitConversionId > 0).Select(q => q.ProductUnitConversionId).ToList();
                var productUnitConversionData = _stockContext.ProductUnitConversion.AsNoTracking().Where(q => productUnitConversionIdsList.Contains(q.ProductUnitConversionId)).ToList();
                var unitData = await _masterDBContext.Unit.AsNoTracking().ToListAsync();

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
                    var productUnitConversionObj = productUnitConversionData.FirstOrDefault(q => q.ProductUnitConversionId == item.ProductUnitConversionId);

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
                        ProductUnitConversionId = item.ProductUnitConversionId,
                        ProductUnitConversion = productUnitConversionObj
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

        private class ProductUnitModel
        {
            public int ProductId { get; set; }
            public int PrimaryUnitId { get; set; }
        }
    }
}
