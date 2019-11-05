using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Stock;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VErp.Services.Master.Service.Dictionay;
using VErp.Commons.Enums.StandardEnum;
using VErp.Services.Master.Service.Activity;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using static VErp.Services.Stock.Model.Stock.StockModel;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public class StockService : IStockService
    {
        private readonly StockDBContext _stockContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IUnitService _unitService;
        private readonly IActivityService _activityService;

        public StockService(
            StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<StockService> logger
            , IUnitService unitService
            , IActivityService activityService
            )
        {
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
                 join ps in _stockContext.ProductStockInfo on p.ProductId equals ps.ProductId
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

            var query = (
                 from sp in _stockContext.StockProduct
                 join p in productQuery on sp.ProductId equals p.ProductId
                 join ps in _stockContext.ProductStockInfo on p.ProductId equals ps.ProductId
                 join iv in _stockContext.Inventory on sp.StockId equals iv.StockId
                 join d in _stockContext.InventoryDetail on iv.InventoryId equals d.InventoryId
                 join pk in _stockContext.Package on d.InventoryDetailId equals pk.InventoryDetailId
                 where sp.StockId == stockId
                 group new { IsExpired = pk.ExpiryTime < DateTime.UtcNow } by new
                 {
                     p.ProductId,
                     p.ProductCode,
                     p.ProductName,
                     p.UnitId,
                     p.ProductTypeId,
                     p.ProductCateId,
                     sp.PrimaryQuantityRemaining,
                     IsMinWarning = sp.PrimaryQuantityRemaining <= ps.AmountWarningMin,
                     IsMaxWarning = sp.PrimaryQuantityRemaining >= ps.AmountWarningMax
                 } into g
                 select new
                 {
                     g.Key.ProductId,
                     g.Key.ProductCode,
                     g.Key.ProductName,
                     g.Key.UnitId,
                     g.Key.ProductTypeId,
                     g.Key.ProductCateId,
                     g.Key.PrimaryQuantityRemaining,
                     g.Key.IsMinWarning,
                     g.Key.IsMaxWarning,
                     IsExpired = g.Max(v => v.IsExpired)
                 });

            if (stockWarningTypeIds != null && stockWarningTypeIds.Count > 0)
            {
                if (stockWarningTypeIds.Contains(EnumWarningType.Min))
                {
                    query = from p in query
                            where p.IsMinWarning
                            select p;
                }

                if (stockWarningTypeIds.Contains(EnumWarningType.Max))
                {
                    query = from p in query
                            where p.IsMaxWarning
                            select p;
                }


                if (stockWarningTypeIds.Contains(EnumWarningType.Expired))
                {
                    query = from p in query
                            where p.IsExpired
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

        private object GetStockForLog(VErp.Infrastructure.EF.StockDB.Stock stockInfo)
        {
            return stockInfo;
        }


    }
}
