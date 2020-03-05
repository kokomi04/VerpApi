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
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Stock;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public class StockService : IStockService
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly StockDBContext _stockContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IUnitService _unitService;
        private readonly IActivityLogService _activityLogService;

        public StockService(
            MasterDBContext masterDBContext,
            StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<StockService> logger
            , IUnitService unitService
            , IActivityLogService activityLogService
            )
        {
            _masterDBContext = masterDBContext;
            _stockContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _unitService = unitService;
            _activityLogService = activityLogService;
        }


        #region CRUD Stocks
        public async Task<ServiceResult<int>> AddStock(StockModel req)
        {
            if (_stockContext.Stock.IgnoreQueryFilters().Where(q => !q.IsDeleted).Any(q => q.StockName.ToLower() == req.StockName.ToLower()))
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
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        UpdatedDatetimeUtc = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _stockContext.AddAsync(stockInfo);

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();


                    await _activityLogService.CreateLog(EnumObjectType.Stock, stockInfo.StockId, $"Thêm mới kho {stockInfo.StockName}", req.JsonSerialize());

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
            var stockInfo = await _stockContext.Stock.IgnoreQueryFilters().Where(q => !q.IsDeleted).FirstOrDefaultAsync(p => p.StockId == stockId);
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

            var checkExistsName = await _stockContext.Stock.IgnoreQueryFilters().Where(q => !q.IsDeleted).AnyAsync(p => p.StockName == req.StockName && p.StockId != stockId);
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

                    //Update

                    //stockInfo.StockId = req.StockId;
                    stockInfo.StockName = req.StockName;
                    stockInfo.Description = req.Description;
                    stockInfo.StockKeeperId = req.StockKeeperId;
                    stockInfo.StockKeeperName = req.StockKeeperName;
                    stockInfo.Type = req.Type;
                    stockInfo.Status = req.Status;
                    stockInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();

                    await _activityLogService.CreateLog(EnumObjectType.Stock, stockInfo.StockId, $"Cập nhật thông tin kho hàng {stockInfo.StockName}", req.JsonSerialize());

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
            var stockInfo = await _stockContext.Stock.IgnoreQueryFilters().Where(q => !q.IsDeleted).FirstOrDefaultAsync(p => p.StockId == stockId);

            if (stockInfo == null)
            {
                return StockErrorCode.StockNotFound;
            }

            stockInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
            using (var trans = await _stockContext.Database.BeginTransactionAsync())
            {
                try
                {
                    stockInfo.IsDeleted = true;
                    stockInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                    await _stockContext.SaveChangesAsync();
                    trans.Commit();

                    await _activityLogService.CreateLog(EnumObjectType.Stock, stockInfo.StockId, $"Xóa kho {stockInfo.StockName}", stockInfo.JsonSerialize());

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
        public async Task<PageData<StockOutput>> GetAll(string keyword, int page, int size)
        {
            var query = from p in _stockContext.Stock.IgnoreQueryFilters().Where(q => !q.IsDeleted)
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
        #endregion


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
                     p.UnitId,
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

            var productRemaining = from sp in _stockContext.StockProduct
                                   join p in productQuery on sp.ProductId equals p.ProductId
                                   where sp.StockId == stockId
                                   group sp by new { sp.ProductId, sp.StockId } into g
                                   //from p in g
                                   select new
                                   {
                                       g.Key.ProductId,
                                       PrimaryQuantityRemaining = g.Sum(q => q.PrimaryQuantityRemaining)
                                   };


            var query = from sp in productRemaining
                        join p in productQuery on sp.ProductId equals p.ProductId
                        join ps in _stockContext.ProductStockInfo on p.ProductId equals ps.ProductId
                        where sp.PrimaryQuantityRemaining > 0
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
                join p in _stockContext.Product on pk.ProductId equals p.ProductId
                join c in _stockContext.ProductUnitConversion on pk.ProductUnitConversionId equals c.ProductUnitConversionId into cs
                from c in cs.DefaultIfEmpty()
                join l in _stockContext.Location on pk.LocationId equals l.LocationId into ls
                from l in ls.DefaultIfEmpty()
                where pk.StockId == stockId && pk.ProductId == productId && pk.PrimaryQuantityRemaining > 0
                select new StockProductPackageDetail()
                {
                    PackageId = pk.PackageId,
                    PackageCode = pk.PackageCode,
                    LocationId = pk.LocationId,
                    LocationName = l == null ? null : l.Name,
                    Date = pk.Date != null ? ((DateTime)pk.Date).GetUnix() : 0,
                    ExpriredDate = pk.ExpiryTime != null ? ((DateTime)pk.ExpiryTime).GetUnix() : 0,
                    PrimaryUnitId = p.UnitId,
                    PrimaryQuantity = pk.PrimaryQuantityRemaining,
                    SecondaryUnitId = c == null ? (int?)null : c.SecondaryUnitId,
                    ProductUnitConversionId = pk.ProductUnitConversionId,
                    ProductUnitConversionName = c == null ? null : c.ProductUnitConversionName,
                    ProductUnitConversionQualtity = pk.ProductUnitConversionRemaining,
                    PackageTypeId = (EnumPackageType)pk.PackageTypeId,
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
                where pk.StockId == stockId && pk.LocationId == locationId && pk.PrimaryQuantityRemaining > 0
                orderby pk.Date descending
                select new LocationProductPackageOuput()
                {
                    ProductId = p.ProductId,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    PackageId = pk.PackageId,
                    PackageCode = pk.PackageCode,
                    Date = pk.Date != null ? ((DateTime)pk.Date).GetUnix() : 0,
                    ExpriredDate = pk.ExpiryTime != null ? ((DateTime)pk.ExpiryTime).GetUnix() : 0,
                    PrimaryUnitId = p.UnitId,
                    PrimaryQuantity = pk.PrimaryQuantityRemaining,
                    SecondaryUnitId = c == null ? (int?)null : c.SecondaryUnitId,
                    ProductUnitConversionId = pk.ProductUnitConversionId,
                    ProductUnitConversionName = c == null ? null : c.ProductUnitConversionName,
                    ProductUnitConversionQualtity = pk.ProductUnitConversionRemaining,
                    RefObjectId = null,
                    PackageTypeId = (EnumPackageType)pk.PackageTypeId,
                    RefObjectCode = ""
                }
                );
            var total = await query.CountAsync();

            var lstData = await query.Skip((page - 1) * size).Take(size).ToListAsync();
            return (lstData, total);
        }

        public async Task<PageData<StockSumaryReportOutput>> StockSumaryReport(string keyword, IList<int> stockIds, IList<int> productTypeIds, IList<int> productCateIds, long beginTime, long endTime, int page, int size)
        {
            DateTime fromDate = DateTime.MinValue;
            DateTime toDate = DateTime.UtcNow;

            if (beginTime > 0)
                fromDate = beginTime.UnixToDateTime();

            if (endTime > 0)
                toDate = endTime.UnixToDateTime();

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
               where iv.IsApproved && iv.Date < fromDate
               group new { d.PrimaryQuantity, iv.InventoryTypeId } by new { d.ProductId } into g
               select new
               {
                   g.Key.ProductId,
                   Total = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity)
               }
               ).ToListAsync();

            var afters = await (
                from iv in inventories
                join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
                join p in productQuery on d.ProductId equals p.ProductId
                where iv.IsApproved && iv.Date >= fromDate && iv.Date <= toDate
                group new { d.PrimaryQuantity, iv.InventoryTypeId } by new { d.ProductId } into g
                select new
                {
                    g.Key.ProductId,
                    TotalInput = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : 0),
                    TotalOutput = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Output ? d.PrimaryQuantity : 0),
                    Total = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity)
                }
                ).ToListAsync();

            var productIds = befores.Select(b => b.ProductId).Distinct().ToHashSet();

            foreach (var a in afters)
            {
                productIds.Add(a.ProductId);
            }

            var total = productIds.Count;
            var productPaged = productIds.Skip((page - 1) * size).Take(size);

            var productInfos = await _stockContext.Product.Where(p => productPaged.Contains(p.ProductId)).ToListAsync();

            var unitIds = productInfos.Select(p => p.UnitId).ToList();

            var unitInfos = await _masterDBContext.Unit.Where(p => unitIds.Contains(p.UnitId)).Select(q => new { q.UnitId, q.UnitName }).ToListAsync();

            var data = (
                from p in productInfos
                join u in unitInfos on p.UnitId equals u.UnitId
                join b in befores on p.ProductId equals b.ProductId into bp
                from b in bp.DefaultIfEmpty()
                join a in afters on p.ProductId equals a.ProductId into ap
                from a in ap.DefaultIfEmpty()
                select new StockSumaryReportOutput
                {
                    ProductId = p.ProductId,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    UnitId = u.UnitId,
                    UnitName = u.UnitName,
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
        public async Task<ServiceResult<StockProductDetailsReportOutput>> StockProductDetailsReportBk(int productId, IList<int> stockIds, long bTime, long eTime)
        {
            if (productId <= 0)
                return GeneralCode.InvalidParams;

            var productInfo = _stockContext.Product.AsNoTracking().FirstOrDefault(p => p.ProductId == productId);
            var resultData = new StockProductDetailsReportOutput
            {
                OpeningStock = null,
                Details = null
            };
            DateTime fromDate = DateTime.MinValue;
            DateTime toDate = DateTime.MinValue;

            if (bTime > 0)
                fromDate = bTime.UnixToDateTime();

            if (eTime > 0)
                toDate = eTime.UnixToDateTime();

            try
            {
                DateTime? beginTime = fromDate != DateTime.MinValue ? fromDate : _stockContext.Inventory.OrderBy(q => q.Date).Select(q => q.Date).FirstOrDefault().AddDays(-1);

                #region Lấy dữ liệu tồn đầu
                var openingStockQuery = from i in _stockContext.Inventory
                                        join id in _stockContext.InventoryDetail on i.InventoryId equals id.InventoryId
                                        where i.IsApproved && id.ProductId == productId
                                        select new { i, id };
                if (stockIds.Count > 0)
                    openingStockQuery = openingStockQuery.Where(q => stockIds.Contains(q.i.StockId));

                if (beginTime.HasValue && beginTime != DateTime.MinValue)
                    openingStockQuery = openingStockQuery.Where(q => q.i.Date < beginTime);
#if DEBUG
                var openingStockQueryDataInput = (from q in openingStockQuery
                                                  where q.i.InventoryTypeId == (int)EnumInventoryType.Input
                                                  group q by q.id.ProductUnitConversionId into g
                                                  select new { ProductUnitConversionId = g.Key, Total = g.Sum(v => v.id.PrimaryQuantity) }).ToList();

                var openingStockQueryDataOutput = (from q in openingStockQuery
                                                   where q.i.InventoryTypeId == (int)EnumInventoryType.Output
                                                   group q by q.id.ProductUnitConversionId into g
                                                   select new { ProductUnitConversionId = g.Key, Total = g.Sum(v => v.id.PrimaryQuantity) }).ToList();
#endif

                var openingStockQueryData = openingStockQuery.Sum(v => v.i.InventoryTypeId == (int)EnumInventoryType.Input ? v.id.PrimaryQuantity : (v.i.InventoryTypeId == (int)EnumInventoryType.Output ? -v.id.PrimaryQuantity : 0));
                #endregion
                #region Lấy dữ liệu giao dịch trong kỳ
                var inPerdiodQuery = from i in _stockContext.Inventory
                                     join id in _stockContext.InventoryDetail on i.InventoryId equals id.InventoryId
                                     //join conversion in _stockContext.ProductUnitConversion on id.ProductUnitConversionId equals conversion.ProductUnitConversionId
                                     where i.IsApproved && id.ProductId == productId
                                     select new { i, id };
                if (stockIds.Count > 0)
                    inPerdiodQuery = inPerdiodQuery.Where(q => stockIds.Contains(q.i.StockId));

                toDate = toDate.AddDays(1);

                if (fromDate != DateTime.MinValue && toDate != DateTime.MinValue)
                {
                    inPerdiodQuery = inPerdiodQuery.Where(q => q.i.Date >= fromDate && q.i.Date < toDate);
                }
                else
                {
                    if (fromDate != DateTime.MinValue)
                    {
                        inPerdiodQuery = inPerdiodQuery.Where(q => q.i.Date >= fromDate);
                    }
                    if (toDate != DateTime.MinValue)
                    {
                        inPerdiodQuery = inPerdiodQuery.Where(q => q.i.Date < toDate);
                    }
                }

                var totalRecord = inPerdiodQuery.Count();
                var inPeriodData = inPerdiodQuery.AsNoTracking().Select(q => new
                {
                    InventoryId = q.i.InventoryId,
                    q.i.StockId,
                    IssuedDate = q.i.Date,
                    q.i.CreatedDatetimeUtc,
                    InventoryCode = q.i.InventoryCode,
                    InventoryTypeId = q.i.InventoryTypeId,
                    Description = q.i.Content,
                    InventoryDetailId = q.id.InventoryDetailId,
                    RefObjectCode = q.id.RefObjectCode,
                    PrimaryQuantity = q.id.PrimaryQuantity,
                    //SecondaryUnitId = q.conversion.SecondaryUnitId,
                    SecondaryQuantity = q.id.ProductUnitConversionQuantity,
                    ProductUnitConversionId = q.id.ProductUnitConversionId
                }).ToList();

                var productUnitConversionIdsList = inPeriodData.Where(q => q.ProductUnitConversionId > 0).Select(q => q.ProductUnitConversionId).ToList();
                var productUnitConversionData = _stockContext.ProductUnitConversion.AsNoTracking().Where(q => productUnitConversionIdsList.Contains(q.ProductUnitConversionId)).ToList();
                var unitData = await _masterDBContext.Unit.AsNoTracking().ToListAsync();

                resultData.OpeningStock = new List<OpeningStockProductModel>
                {
                    new OpeningStockProductModel
                    {
                        PrimaryUnitId = productInfo.UnitId,
                        Total = openingStockQueryData
                    }
                };

                resultData.Details = new List<StockProductDetailsModel>(totalRecord);

                var stocks = await _stockContext.Stock.AsNoTracking().ToListAsync();
                foreach (var item in inPeriodData.OrderBy(q => q.IssuedDate).ThenBy(q => q.CreatedDatetimeUtc).ToList())
                {
                    var productUnitConversionObj = productUnitConversionData.FirstOrDefault(q => q.ProductUnitConversionId == item.ProductUnitConversionId);
                    var secondaryUnitObj = unitData.FirstOrDefault(q => q.UnitId == item.ProductUnitConversionId);
                    var secondaryUnitName = secondaryUnitObj != null ? secondaryUnitObj.UnitName : string.Empty;
                    var secondaryUnitId = secondaryUnitObj != null ? (int?)secondaryUnitObj.UnitId : null;
                    resultData.Details.Add(new StockProductDetailsModel
                    {
                        InventoryId = item.InventoryId,
                        StockId = item.StockId,
                        StockName = stocks.FirstOrDefault(s => s.StockId == item.StockId)?.StockName,
                        IssuedDate = item.IssuedDate.GetUnix(),
                        InventoryCode = item.InventoryCode,
                        InventoryTypeId = item.InventoryTypeId,
                        Description = item.Description,
                        SecondaryUnitName = secondaryUnitName,
                        InventoryDetailId = item.InventoryDetailId,
                        RefObjectCode = item.RefObjectCode,
                        PrimaryUnitId = productInfo.UnitId,
                        PrimaryQuantity = item.PrimaryQuantity,
                        SecondaryUnitId = secondaryUnitId,
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


        public async Task<ServiceResult<StockProductDetailsReportOutput>> StockProductDetailsReport(int productId, IList<int> stockIds, long bTime, long eTime)
        {
            if (productId <= 0)
                return GeneralCode.InvalidParams;

            var productInfo = _stockContext.Product.AsNoTracking().FirstOrDefault(p => p.ProductId == productId);
            var resultData = new StockProductDetailsReportOutput
            {
                OpeningStock = null,
                Details = null
            };
            DateTime fromDate = DateTime.MinValue;
            DateTime toDate = DateTime.MinValue;

            if (bTime > 0)
                fromDate = bTime.UnixToDateTime();

            if (eTime > 0)
                toDate = eTime.UnixToDateTime();

            try
            {
                DateTime? beginTime = fromDate != DateTime.MinValue ? fromDate : _stockContext.Inventory.OrderBy(q => q.Date).Select(q => q.Date).FirstOrDefault().AddDays(-1);

                #region Lấy dữ liệu tồn đầu

                var inventories = _stockContext.Inventory.AsQueryable();

                if (stockIds.Count > 0)
                    inventories = inventories.Where(q => stockIds.Contains(q.StockId));

                if (beginTime.HasValue && beginTime != DateTime.MinValue)
                    inventories = inventories.Where(q => q.Date < beginTime);

                var inventoryDetails = (
                    from i in inventories
                    join id in _stockContext.InventoryDetail on i.InventoryId equals id.InventoryId
                    where i.IsApproved && id.ProductId == productId
                    select new
                    {
                        i.InventoryTypeId,
                        id.ProductUnitConversionId,
                        id.ProductUnitConversionQuantity,
                        id.PrimaryQuantity
                    });

                var openingStockQuery = (
                    from id in inventoryDetails
                    group id by id.ProductUnitConversionId into pu
                    select new
                    {
                        ProductUnitConversionId = pu.Key,
                        TotalPrimaryUnit = pu.Sum(v => v.InventoryTypeId == (int)EnumInventoryType.Input ? v.PrimaryQuantity : -v.PrimaryQuantity),
                        TotalProductUnitConversion = pu.Sum(v => v.InventoryTypeId == (int)EnumInventoryType.Input ? v.ProductUnitConversionQuantity : -v.ProductUnitConversionQuantity),
                    }).ToList();

                #endregion
                #region Lấy dữ liệu giao dịch trong kỳ

                var inPerdiodInventories = _stockContext.Inventory.AsQueryable();

                if (stockIds.Count > 0)
                    inPerdiodInventories = inPerdiodInventories.Where(q => stockIds.Contains(q.StockId));

                toDate = toDate.AddDays(1);

                if (fromDate != DateTime.MinValue && toDate != DateTime.MinValue)
                {
                    inPerdiodInventories = inPerdiodInventories.Where(q => q.Date >= fromDate && q.Date < toDate);
                }
                else
                {
                    if (fromDate != DateTime.MinValue)
                    {
                        inPerdiodInventories = inPerdiodInventories.Where(q => q.Date >= fromDate);
                    }
                    if (toDate != DateTime.MinValue)
                    {
                        inPerdiodInventories = inPerdiodInventories.Where(q => q.Date < toDate);
                    }
                }

                var inPeriodData = (
                  from i in inPerdiodInventories
                  join id in _stockContext.InventoryDetail on i.InventoryId equals id.InventoryId
                  where i.IsApproved && id.ProductId == productId
                  select new
                  {
                      i.InventoryId,
                      i.StockId,
                      i.Date,
                      i.CreatedDatetimeUtc,
                      i.InventoryCode,
                      i.InventoryTypeId,
                      i.Content,
                      id.InventoryDetailId,
                      id.RefObjectCode,
                      id.PrimaryQuantity,
                      id.ProductUnitConversionQuantity,
                      id.ProductUnitConversionId
                  }).ToList();

                var productUnitConversionIdsList = inPeriodData.Where(q => q.ProductUnitConversionId > 0).Select(q => q.ProductUnitConversionId).ToList();
                var productUnitConversionData = _stockContext.ProductUnitConversion.AsNoTracking().Where(q => productUnitConversionIdsList.Contains(q.ProductUnitConversionId)).ToList();
                var unitData = await _masterDBContext.Unit.AsNoTracking().ToListAsync();

                resultData.OpeningStock = new List<OpeningStockProductModel>
                {
                    new OpeningStockProductModel
                    {
                        PrimaryUnitId = productInfo.UnitId,
                        Total = openingStockQuery.Sum(o=>o.TotalPrimaryUnit)
                    }
                };

                resultData.Details = new List<StockProductDetailsModel>();

                var stocks = await _stockContext.Stock.AsNoTracking().ToListAsync();
                var totalByTimes = openingStockQuery.ToDictionary(o => o.ProductUnitConversionId.Value, o => o.TotalProductUnitConversion);
                var totalPrimary = openingStockQuery.Sum(o => o.TotalPrimaryUnit);

                foreach (var item in inPeriodData.OrderBy(q => q.Date).ThenBy(q => q.CreatedDatetimeUtc).ToList())
                {
                    var productUnitConversionObj = productUnitConversionData.FirstOrDefault(q => q.ProductUnitConversionId == item.ProductUnitConversionId);
                    var secondaryUnitObj = unitData.FirstOrDefault(q => q.UnitId == item.ProductUnitConversionId);
                    var secondaryUnitName = secondaryUnitObj != null ? secondaryUnitObj.UnitName : string.Empty;
                    var secondaryUnitId = secondaryUnitObj != null ? (int?)secondaryUnitObj.UnitId : null;

                    if (!totalByTimes.ContainsKey(item.ProductUnitConversionId.Value))
                    {
                        totalByTimes.Add(item.ProductUnitConversionId.Value, 0);
                    }

                    if (item.InventoryTypeId == (int)EnumInventoryType.Input)
                    {
                        totalByTimes[item.ProductUnitConversionId.Value] += item.ProductUnitConversionQuantity;
                        totalPrimary += item.PrimaryQuantity;
                    }
                    else
                    {
                        totalByTimes[item.ProductUnitConversionId.Value] -= item.ProductUnitConversionQuantity;
                        totalPrimary -= item.PrimaryQuantity;
                    }

                    resultData.Details.Add(new StockProductDetailsModel
                    {
                        InventoryId = item.InventoryId,
                        StockId = item.StockId,
                        StockName = stocks.FirstOrDefault(s => s.StockId == item.StockId)?.StockName,
                        IssuedDate = item.Date.GetUnix(),
                        InventoryCode = item.InventoryCode,
                        InventoryTypeId = item.InventoryTypeId,
                        Description = item.Content,
                        SecondaryUnitName = secondaryUnitName,
                        InventoryDetailId = item.InventoryDetailId,
                        RefObjectCode = item.RefObjectCode,
                        PrimaryUnitId = productInfo.UnitId,
                        PrimaryQuantity = item.PrimaryQuantity,
                        SecondaryUnitId = secondaryUnitId,
                        SecondaryQuantity = item.ProductUnitConversionQuantity,
                        ProductUnitConversionId = item.ProductUnitConversionId,
                        ProductUnitConversion = productUnitConversionObj,
                        EndOfPerdiodPrimaryQuantity = totalPrimary,
                        EndOfPerdiodProductUnitConversionQuantity = totalByTimes[item.ProductUnitConversionId.Value],
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

        /// <summary>
        /// Báo cáo tổng hợp NXT 2 DVT 2 DVT (SỐ LƯỢNG) - Mẫu báo cáo kho 03
        /// </summary>
        /// <param name="keyword">Từ khóa tìm kiếm: mã sp, tên sp</param>
        /// <param name="stockIds">Danh sách id kho cần báo cáo</param>
        /// <param name="bTime"></param>
        /// <param name="eTime"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public async Task<ServiceResult<PageData<StockSumaryReportForm03Output>>> StockSumaryReportForm03(string keyword, IList<int> stockIds, IList<int> productTypeIds, IList<int> productCateIds, long bTime, long eTime, int page = 1, int size = int.MaxValue)
        {
            try
            {
                DateTime fromDate = DateTime.Now.AddMonths(-3);
                DateTime toDate = DateTime.Now;

                if (bTime > 0)
                    fromDate = bTime.UnixToDateTime();

                if (eTime > 0)
                    toDate = eTime.UnixToDateTime();

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

                var inventoryQuery = _stockContext.Inventory.AsQueryable();

                if (stockIds != null && stockIds.Count > 0)
                {
                    inventoryQuery = inventoryQuery.Where(iv => stockIds.Contains(iv.StockId));
                }

                var befores = await (
               from iv in inventoryQuery
               join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
               join p in productQuery on d.ProductId equals p.ProductId
               where iv.IsApproved && iv.Date < fromDate

               group new { d.PrimaryQuantity, iv.InventoryTypeId } by d.ProductId into g
               select new
               {
                   ProductId = g.Key,
                   Total = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity)
               }
               ).ToListAsync();

                var afters = await (
                    from iv in inventoryQuery
                    join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
                    join p in productQuery on d.ProductId equals p.ProductId
                    where iv.IsApproved && iv.Date >= fromDate && iv.Date <= toDate

                    group new { d.PrimaryQuantity, iv.InventoryTypeId } by d.ProductId into g
                    select new
                    {
                        ProductId = g.Key,
                        TotalInput = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : 0),
                        TotalOutput = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Output ? d.PrimaryQuantity : 0),
                        Total = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.PrimaryQuantity : -d.PrimaryQuantity)
                    }
                    ).ToListAsync();


                var beforesByAltUnit = await (
             from iv in inventoryQuery
             join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
             join p in productQuery on d.ProductId equals p.ProductId
             where iv.IsApproved && iv.Date < fromDate
             group new { d.ProductUnitConversionQuantity, iv.InventoryTypeId } by new { d.ProductId, d.ProductUnitConversionId } into g
             select new
             {
                 g.Key.ProductId,
                 g.Key.ProductUnitConversionId,
                 Total = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.ProductUnitConversionQuantity : -d.ProductUnitConversionQuantity)
             }
             ).ToListAsync();

                var aftersByAltUnit = await (
                   from iv in inventoryQuery
                   join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
                   join p in productQuery on d.ProductId equals p.ProductId
                   where iv.IsApproved && iv.Date >= fromDate && iv.Date <= toDate
                   group new { d.ProductUnitConversionQuantity, iv.InventoryTypeId } by new { d.ProductId, d.ProductUnitConversionId } into g
                   select new
                   {
                       g.Key.ProductId,
                       g.Key.ProductUnitConversionId,
                       TotalInput = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.ProductUnitConversionQuantity : 0),
                       TotalOutput = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Output ? d.ProductUnitConversionQuantity : 0),
                       Total = g.Sum(d => d.InventoryTypeId == (int)EnumInventoryType.Input ? d.ProductUnitConversionQuantity : -d.ProductUnitConversionQuantity)
                   }
                   ).ToListAsync();

                #region product  - ProductUnitModel  
                var productAltUnitModelList = new List<ProductAltUnitModel>();
                var productIds = befores.Select(b => b.ProductId).Distinct().ToHashSet();

                foreach (var a in afters)
                {
                    productIds.Add(a.ProductId);
                }

                foreach (var b in beforesByAltUnit)
                {
                    if (!productAltUnitModelList.Any(p => p.ProductId == b.ProductId && p.ProductUnitConversionId == b.ProductUnitConversionId) && b.ProductUnitConversionId > 0)
                    {
                        productAltUnitModelList.Add(new ProductAltUnitModel()
                        {
                            ProductId = b.ProductId,
                            ProductUnitConversionId = (int)b.ProductUnitConversionId
                        });
                    }
                }

                foreach (var a in aftersByAltUnit)
                {
                    if (!productAltUnitModelList.Any(p => p.ProductId == a.ProductId && p.ProductUnitConversionId == a.ProductUnitConversionId) && a.ProductUnitConversionId > 0)
                    {
                        productAltUnitModelList.Add(new ProductAltUnitModel()
                        {
                            ProductId = a.ProductId,
                            ProductUnitConversionId = (int)a.ProductUnitConversionId
                        });
                    }
                }

                #endregion

                var total = productIds.Count;
                var productPaged = productIds.Skip((page - 1) * size).Take(size);

                var productInfos = await _stockContext.Product.Where(p => productPaged.Contains(p.ProductId)).AsNoTracking().ToListAsync();
                var productUnitConversionInfos = await _stockContext.ProductUnitConversion.Where(p => productPaged.Contains(p.ProductId)).AsNoTracking().ToListAsync();

                var unitIds = productInfos.Select(p => p.UnitId).ToList();
                var unitInfos = await _masterDBContext.Unit.Where(p => unitIds.Contains(p.UnitId)).Select(q => new { q.UnitId, q.UnitName }).ToListAsync();

                var productAltUnitPaged = productAltUnitModelList.Where(q => productPaged.Contains(q.ProductId)).ToList();

                var packageQuery = _stockContext.Package.AsQueryable();
                if (stockIds != null && stockIds.Count > 0)
                {
                    packageQuery = packageQuery.Where(q => stockIds.Contains(q.StockId));
                }
                var packageInfoData = (from pkg in packageQuery
                                       join p in productInfos on pkg.ProductId equals p.ProductId
                                       //join id in _stockContext.InventoryDetail on p.ProductId equals id.InventoryId
                                       //join i in inventoryQuery.Where(q => (q.Date > fromDate && q.Date <= toDate) && q.IsApproved) on id.InventoryId equals i.InventoryId
                                       select new
                                       {
                                           //i.InventoryId,
                                           //i.InventoryCode,
                                           //i.InventoryTypeId,
                                           //i.Date,
                                           p.ProductId,
                                           pkg.PackageId,
                                           pkg.PackageCode,
                                           pkg.Date,
                                           p.UnitId,
                                           pkg.PrimaryQuantityRemaining,
                                           pkg.ProductUnitConversionId,
                                           pkg.ProductUnitConversionRemaining
                                       }).ToList();

                var productAltSummaryData = (
                 from p in productAltUnitPaged
                 join c in productUnitConversionInfos on p.ProductUnitConversionId equals c.ProductUnitConversionId
                 //join info in productInfos on p.ProductId equals info.ProductId
                 join b in beforesByAltUnit on new { p.ProductId, p.ProductUnitConversionId } equals new { b.ProductId, b.ProductUnitConversionId } into bp
                 from b in bp.DefaultIfEmpty()
                 join a in aftersByAltUnit on new { p.ProductId, p.ProductUnitConversionId } equals new { a.ProductId, a.ProductUnitConversionId } into ap
                 from a in ap.DefaultIfEmpty()
                 select new ProductAltSummary
                 {
                     ProductId = p.ProductId,
                     ProductUnitConversionId = p.ProductUnitConversionId,
                     UnitId = c.SecondaryUnitId,
                     ProductUnitCoversionName = c.ProductUnitConversionName,
                     ConversionDescription = c.ConversionDescription,
                     ProductUnitConversionQuantityBefore = b == null ? 0 : b.Total,
                     ProductUnitConversionQuantityInput = a == null ? 0 : a.TotalInput,
                     ProductUnitConversionQuantityOutput = a == null ? 0 : a.TotalOutput,
                     ProductUnitConversionQuantityAfter = (b == null ? 0 : b.Total) + (a == null ? 0 : a.Total)
                 }).ToList();

                var resultData = (
                from p in productInfos
                join u in unitInfos on p.UnitId equals u.UnitId
                join b in befores on p.ProductId equals b.ProductId into bp
                from b in bp.DefaultIfEmpty()
                join a in afters on p.ProductId equals a.ProductId into ap
                from a in ap.DefaultIfEmpty()
                select new StockSumaryReportForm03Output
                {
                    ProductId = p.ProductId,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    UnitId = p.UnitId,
                    UnitName = u.UnitName,
                    PrimaryQualtityBefore = b == null ? 0 : b.Total,
                    PrimaryQualtityInput = a == null ? 0 : a.TotalInput,
                    PrimaryQualtityOutput = a == null ? 0 : a.TotalOutput,
                    PrimaryQualtityAfter = (b == null ? 0 : b.Total) + (a == null ? 0 : a.Total),
                    ProductAltSummaryList = productAltSummaryData.Where(q => q.ProductId == p.ProductId).ToList()
                }).ToList();

                if (packageInfoData != null && packageInfoData.Count > 0)
                {
                    var pakageExportIdsList = (from id in _stockContext.InventoryDetail
                                               join i in _stockContext.Inventory.Where(q => q.IsApproved && q.InventoryTypeId == (int)EnumInventoryType.Output && (q.Date > fromDate && q.Date <= toDate)) on id.InventoryId equals i.InventoryId
                                               where id.FromPackageId > 0 && productIds.Contains(id.ProductId)
                                               select new { id.FromPackageId }
                                              ).ToList();
                    var pakageExportIds = pakageExportIdsList.Select(q => Convert.ToInt64(q.FromPackageId)).ToList();
                    foreach (var item in resultData)
                    {

                        var pakageNotExport = packageInfoData.Where(q => q.ProductId == item.ProductId && !pakageExportIds.Contains(q.PackageId)).OrderBy(q => q.Date).FirstOrDefault();

                        item.PakageIdNotExport = pakageNotExport != null ? pakageNotExport.PackageId : 0;
                        item.PakageCodeNotExport = pakageNotExport != null ? pakageNotExport.PackageCode : string.Empty;
                        item.PakageDateNotExport = pakageNotExport != null ? (pakageNotExport.Date.HasValue ? ((DateTime)pakageNotExport.Date).GetUnix() : 0) : 0;
                    }
                }
                return new PageData<StockSumaryReportForm03Output>
                {
                    List = resultData,
                    Total = total
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StockSumaryReportForm03");
                return GeneralCode.InternalError;
            }
        }

        /// <summary>
        /// Nhật ký nhập xuất kho - Mẫu báo cáo kho 04
        /// </summary>
        /// <param name="stockIds">Danh sách id kho cần báo cáo</param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public async Task<ServiceResult<PageData<StockSumaryReportForm04Output>>> StockSumaryReportForm04(IList<int> stockIds, long beginTime, long endTime, int page = 1, int size = int.MaxValue)
        {
            try
            {
                var bTime = DateTime.MinValue;
                var eTime = DateTime.MinValue;

                if (beginTime > 0)
                {
                    bTime = beginTime.UnixToDateTime();
                }
                if (endTime > 0)
                {
                    eTime = endTime.UnixToDateTime();
                }
                var inventoryQuery = _stockContext.Inventory.AsNoTracking().Where(q => q.IsApproved);
                if (stockIds != null && stockIds.Count > 0)
                {
                    inventoryQuery = inventoryQuery.Where(iv => stockIds.Contains(iv.StockId));
                }

                eTime = eTime.AddDays(1);
                if (bTime != DateTime.MinValue && eTime != DateTime.MinValue)
                {
                    inventoryQuery = inventoryQuery.Where(q => q.Date >= bTime && q.Date < eTime);
                }
                else
                {
                    if (bTime != DateTime.MinValue)
                    {
                        inventoryQuery = inventoryQuery.Where(q => q.Date >= bTime);
                    }
                    if (eTime != DateTime.MinValue)
                    {
                        inventoryQuery = inventoryQuery.Where(q => q.Date < eTime);
                    }
                }
                inventoryQuery = inventoryQuery.OrderByDescending(q => q.Date);
                var total = inventoryQuery.Count();
                var inventoryDataList = inventoryQuery.Skip((page - 1) * size).Take(size).ToList();
                var inventoryIds = inventoryDataList.Select(q => q.InventoryId).ToList();
                var customerIds = inventoryDataList.Select(q => q.CustomerId).ToList();

                var customerDataList = await _masterDBContext.Customer.Where(q => customerIds.Contains(q.CustomerId)).Select(q => new { q.CustomerId, q.CustomerCode, q.CustomerName }).ToListAsync();
                var inventoryDetailsData = _stockContext.InventoryDetail.AsNoTracking().Where(q => inventoryIds.Contains(q.InventoryId)).ToList();
                var productIds = inventoryDetailsData.Select(q => q.ProductId).ToList();

                var productDataList = _stockContext.Product.Where(q => productIds.Contains(q.ProductId)).Select(q => new { q.ProductId, q.ProductCode, q.ProductName, q.UnitId }).ToList();
                var unitIds = productDataList.Select(q => q.UnitId).ToList();
                var unitDataList = await _masterDBContext.Unit.Where(q => unitIds.Contains(q.UnitId)).Select(q => new { q.UnitId, q.UnitName }).ToListAsync();

                var createdUserIdsList = inventoryDataList.Select(q => q.CreatedByUserId).ToList();
                var updatedUserIdsList = inventoryDataList.Select(q => q.UpdatedByUserId).ToList();
                var userIdsList = createdUserIdsList.Concat(updatedUserIdsList).ToList();
                var userDataList = await _masterDBContext.User.Where(q => userIdsList.Contains(q.UserId)).ToListAsync();

                var reportInventoryDetailsOutputModelList = new List<ReportForm04InventoryDetailsOutputModel>(inventoryDetailsData.Count) { };
                foreach (var inventoryDetail in inventoryDetailsData)
                {
                    var productInfo = productDataList.FirstOrDefault(q => q.ProductId == inventoryDetail.ProductId);

                    var item = new ReportForm04InventoryDetailsOutputModel
                    {
                        InventoryDetailId = inventoryDetail.InventoryDetailId,
                        InventoryId = inventoryDetail.InventoryId,
                        ProductId = inventoryDetail.ProductId,
                        ProductCode = productInfo?.ProductCode ?? string.Empty,
                        ProductName = productInfo?.ProductName ?? string.Empty,
                        PrimaryUnitId = productInfo?.UnitId ?? 0,
                        UnitName = unitDataList.FirstOrDefault(q => q.UnitId == productInfo?.UnitId)?.UnitName ?? string.Empty,
                        ProductUnitConversionId = inventoryDetail.ProductUnitConversionId,
                        PrimaryQuantity = inventoryDetail.PrimaryQuantity,
                        UnitPrice = inventoryDetail.UnitPrice,
                        POCode = inventoryDetail.Pocode ?? string.Empty,
                        ProductionOrderCode = inventoryDetail.ProductionOrderCode ?? string.Empty,
                        OrderCode = inventoryDetail.OrderCode ?? string.Empty
                    };

                    reportInventoryDetailsOutputModelList.Add(item);
                }

                var reportInventoryOutputModelList = new List<StockSumaryReportForm04Output>(inventoryDataList.Count) { };
                foreach (var inventory in inventoryDataList)
                {
                    var item = new StockSumaryReportForm04Output()
                    {
                        InventoryDetailsOutputModel = new List<ReportForm04InventoryDetailsOutputModel>()
                    };
                    item.InventoryId = inventory.InventoryId;
                    item.InventoryCode = inventory.InventoryCode;
                    item.BillCode = inventory.BillCode;
                    item.DateUtc = inventory.Date.GetUnix();
                    item.InventoryTypeId = inventory.InventoryTypeId;
                    item.Content = inventory.Content;
                    item.CustomerId = inventory.CustomerId;
                    item.CustomerCode = customerDataList.FirstOrDefault(q => q.CustomerId == inventory.CustomerId)?.CustomerCode ?? string.Empty;
                    item.CustomerName = customerDataList.FirstOrDefault(q => q.CustomerId == inventory.CustomerId)?.CustomerName ?? string.Empty;
                    item.CreatedByUserId = inventory.CreatedByUserId;
                    item.CreatedByUserName = userDataList.FirstOrDefault(q => q.UserId == inventory.CreatedByUserId)?.UserName ?? string.Empty;
                    item.CreatedDatetimeUtc = inventory.CreatedDatetimeUtc.GetUnix();
                    item.UpdatedByUserId = inventory.UpdatedByUserId;
                    item.UpdatedByUserName = userDataList.FirstOrDefault(q => q.UserId == inventory.UpdatedByUserId)?.UserName ?? string.Empty;
                    item.UpdatedDatetimeUtc = inventory.UpdatedDatetimeUtc.GetUnix();
                    item.Censor = userDataList.FirstOrDefault(q => q.UserId == inventory.UpdatedByUserId)?.UserName ?? string.Empty;
                    item.CensorDate = inventory.UpdatedDatetimeUtc.GetUnix();

                    item.InventoryDetailsOutputModel = reportInventoryDetailsOutputModelList.Where(q => q.InventoryId == inventory.InventoryId).ToList();
                    reportInventoryOutputModelList.Add(item);
                }

                return new PageData<StockSumaryReportForm04Output> { List = reportInventoryOutputModelList, Total = total };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StockSumaryReportForm04");
                return GeneralCode.InternalError;
            }
        }

        #region Private Methods



        #endregion

        private class ProductAltUnitModel
        {
            public int ProductId { get; set; }

            public int? ProductUnitConversionId { get; set; }
        }
    }
}
